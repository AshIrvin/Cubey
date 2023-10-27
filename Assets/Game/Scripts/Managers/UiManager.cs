using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UiManager : MonoBehaviour
{
    public static UiManager Instance;

    [SerializeField] private List<string> finishedInfoText;
    [SerializeField] private List<string> nearlyFinishedInfoText;
    [SerializeField] private List<string> failedFinishedInfoText;

    private TextMeshProUGUI endScreenText;


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public void UpdateEndScreenInfoText(GameManager.FinishedInfo info)
    {
        
        switch (info)
        {
            case GameManager.FinishedInfo.Failed:
                endScreenText.text = failedFinishedInfoText[UnityEngine.Random.Range(0, failedFinishedInfoText.Count)];
                break;
            case GameManager.FinishedInfo.Nearly:
                endScreenText.text = nearlyFinishedInfoText[UnityEngine.Random.Range(0, nearlyFinishedInfoText.Count)];
                break;
            case GameManager.FinishedInfo.Completed:
                endScreenText.text = finishedInfoText[UnityEngine.Random.Range(0, finishedInfoText.Count)];
                break;
        }
    }
}
