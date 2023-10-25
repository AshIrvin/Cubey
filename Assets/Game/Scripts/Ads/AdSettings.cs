using System;
using UnityEngine;

public class AdSettings : MonoBehaviour
{
    public static AdSettings Instance;

    [SerializeField] private GameObject bgAdBlocker;
    private int manyLevelsBeforeAds = 3;

    public Action LoadAd;
    public int LevelsBeforeAd => manyLevelsBeforeAds;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void EnableAdBackgroundBlocker(bool state)
    {
        bgAdBlocker.SetActive(state);
    }
}
