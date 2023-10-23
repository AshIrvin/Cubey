using System;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

namespace Samples.Purchasing.GooglePlay.RestoringTransactions
{
    [RequireComponent(typeof(UserWarningGooglePlayStore))]
    public class RestoringTransactions : MonoBehaviour, IStoreListener
    {
        IStoreController m_StoreController;
        IGooglePlayStoreExtensions m_GooglePlayStoreExtensions;

        public string noAdsProductId = "com.ashirvin.cubey.unlockgame";
        public Text hasNoAdsText;
        public Text restoreStatusText;

        public GameObject thanksGo;
        [SerializeField] private ShopManager shopManager;

        private void Awake()
        {
            if (hasNoAdsText == null)
                hasNoAdsText = transform.Find("GamePurchasedText").GetComponent<Text>();
            if (restoreStatusText == null)
                restoreStatusText = transform.Find("GamePurchasedRestored").GetComponent<Text>();
            if (thanksGo == null)
                thanksGo = transform.Find("ThanksForBuying").gameObject;
            if (shopManager == null)
                shopManager = transform.Find("Shop").GetComponent<ShopManager>();
        }

        void Start()
        {
            InitializePurchasing();
            UpdateWarningMessage();
        }

        void InitializePurchasing()
        {
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            builder.AddProduct(noAdsProductId, ProductType.NonConsumable);

            UnityPurchasing.Initialize(this, builder);
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            Logger.Instance.ShowDebugLog("In-App Purchasing successfully initialized");

            m_StoreController = controller;
            m_GooglePlayStoreExtensions = extensions.GetExtension<IGooglePlayStoreExtensions>();

            UpdateUI();
        }

        public void Restore()
        {
            m_GooglePlayStoreExtensions.RestoreTransactions(OnRestore);
        }

        void OnRestore(bool success)
        {
            Logger.Instance.ShowDebugLog("Trying to restore purchase...");
            var restoreMessage = "";
            if (success)
            {
                // This does not mean anything was restored,
                // merely that the restoration process succeeded.
                restoreMessage = "Restore Successful";
                ShopManager.RestoreTransaction();
                shopManager.gameObject.SetActive(false);
                thanksGo.SetActive(HasNoAds());
            }
            else
            {
                // Restoration failed.
                restoreMessage = "Restore Failed";
            }

            Logger.Instance.ShowDebugLog(restoreMessage);
            restoreStatusText.text = restoreMessage;
        }

        public void BuyNoAds()
        {
            m_StoreController.InitiatePurchase(noAdsProductId);
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            var product = args.purchasedProduct;

            Logger.Instance.ShowDebugLog($"Processing Purchase: {product.definition.id}");
            if (args.purchasedProduct.hasReceipt)
            {
                Logger.Instance.ShowDebugLog("Purchase product has receipt: " + args.purchasedProduct.receipt);
                shopManager.PurchaseGameButton();
                shopManager.gameObject.SetActive(false);
            }
            
            UpdateUI();
            
            return PurchaseProcessingResult.Complete;
        }

        void UpdateUI()
        {
            // hasNoAdsText.text = HasNoAds() ? "No ads will be shown" : "Ads will be shown";
            thanksGo.SetActive(HasNoAds());
        }

        bool HasNoAds()
        {
            var noAdsProduct = m_StoreController.products.WithID(noAdsProductId);
            return noAdsProduct != null && noAdsProduct.hasReceipt;
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Logger.Instance.ShowDebugLog($"In-App Purchasing initialize failed: {error}");
        }

        public void OnInitializeFailed(InitializationFailureReason error, string text)
        {
            Logger.Instance.ShowDebugLog($"In-App Purchasing initialize failed: {error}. Info: {text}");
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            Logger.Instance.ShowDebugLog($"Purchase failed - Product: '{product.definition.id}', PurchaseFailureReason: {failureReason}");
        }

        void UpdateWarningMessage()
        {
            GetComponent<UserWarningGooglePlayStore>().UpdateWarningText();
        }
    }
}
