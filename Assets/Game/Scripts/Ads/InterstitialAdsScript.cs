using UnityEngine;
using UnityEngine.Advertisements;

public class InterstitialAdsScript : MonoBehaviour
{

    string gameId = "3488382";

    bool testMode = true;

    void Start()
    {
        // Initialize the Ads service:
        Advertisement.Initialize(gameId, testMode);
        // Show an ad:
        Advertisement.Show();
    }
}