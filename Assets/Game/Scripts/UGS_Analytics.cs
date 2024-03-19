using UnityEngine;
using Unity.Services.Analytics;
using Unity.Services.Core;
using System.Collections.Generic;
using System;

public class UGS_Analytics : MonoBehaviour
{
    public static Action<bool> AnalyticsConsent;

    private void Awake()
    {
        UiManager.AnalyticsConsent += ToggleConsent;
        LevelManager.OnLevelCompleted += LevelCompletedCustomEvent;
        LevelManager.OnLevelFailed += LevelFailedCustomEvent;
    }

    private async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();
        }
        catch (ConsentCheckException e)
        {
            Debug.Log(e.ToString());
        }

        //HasGivenConsent(PlayerPrefs.GetInt("AnalyticsConsent") == 1);
        CheckConsent();
    }

    private void CheckConsent()
    {
        if (PlayerPrefs.GetInt("AnalyticsConsent") == 1)
        {
            GiveConsent();
            AnalyticsConsent?.Invoke(true);
        }
    }

    private void ToggleConsent(bool state)
    {
        if (!state)
        {
            OptOutAnalytics();
            return;
        }

        GiveConsent();
        AnalyticsConsent?.Invoke(state);
    }

    private void GiveConsent()
    {
        PlayerPrefs.SetInt("AnalyticsConsent", 1);

        AnalyticsService.Instance.StartDataCollection();
    }

    private void OptOutAnalytics()
    {
        PlayerPrefs.SetInt("AnalyticsConsent", 0);

        AnalyticsService.Instance.StopDataCollection();
    }

    private void LevelCompletedCustomEvent(int chapterLevel)
    {
        Dictionary<string, object> parameters = new()
        {
            { "chapterLevelNo", "level" + chapterLevel.ToString("0000")}
        };

        AnalyticsService.Instance.CustomData("levelCompleted", parameters);
        //AnalyticsService.Instance.RecordEvent(parameters);

        AnalyticsService.Instance.Flush();
    }

    private void LevelFailedCustomEvent()
    {

    }
}