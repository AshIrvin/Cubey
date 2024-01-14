using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

namespace Samples.Purchasing.GooglePlay.RestoringTransactions
{
    [RequireComponent(typeof(UserWarningGooglePlayStore))]
    public class RestoringTransactions : MonoBehaviour, IStoreListener
    {
        private IStoreController m_StoreController;
        private IGooglePlayStoreExtensions m_GooglePlayStoreExtensions;

        private readonly string noAdsProductId = "com.ashirvin.cubey.unlockgame";
        [SerializeField] private Text hasNoAdsText;
        [SerializeField] private Text restoreStatusText;

        [SerializeField] private GameObject thanksGo;
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

        private void Start()
        {
            InitializePurchasing();
            UpdateWarningMessage();
        }

        private void InitializePurchasing()
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

        private void OnRestore(bool success)
        {
            Logger.Instance.ShowDebugLog("Trying to restore purchase...");
            var restoreMessage = "";
            if (success)
            {
                // This does not mean anything was restored,
                // merely that the restoration process succeeded.
                restoreMessage = "Restore Successful";
                // TODO - does this need fixed?
                //ShopManager.RestoreTransaction();
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

        // TODO - this needing to be public?
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
                Logger.Instance.ShowDebugLog("RestoringTransactions - Purchase product has receipt: " + args.purchasedProduct.receipt);
                //shopManager.PurchaseGameButton();
                shopManager.gameObject.SetActive(false);
            }
            
            UpdateUI();
            
            return PurchaseProcessingResult.Complete;
        }

        private void UpdateUI()
        {
            // hasNoAdsText.text = HasNoAds() ? "No ads will be shown" : "Ads will be shown";
            thanksGo.SetActive(HasNoAds());
        }

        private bool HasNoAds()
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
