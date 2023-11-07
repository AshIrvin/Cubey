using UnityEngine;

public class AdSettings : MonoBehaviour
{
    public static AdSettings Instance;

    [SerializeField] private GameObject bgAdBlocker;
    private readonly int manyLevelsBeforeAds = 3;

    public int LevelsBeforeAd => manyLevelsBeforeAds;


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public void EnableAdBackgroundBlocker(bool state)
    {
        bgAdBlocker.SetActive(state);
    }
}
