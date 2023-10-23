using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PortalManager : MonoBehaviour
{
    [SerializeField] private SaveMetaData saveMetaData;
    [SerializeField] private ChapterList chapterList;
    [SerializeField] private BoolGlobalVariable enablePortal;
    
    [SerializeField] private float portalDistance = 0.3f;
    [SerializeField] private Vector3[] portalEnter;
    [SerializeField] private Vector3[] portalExit;
    [SerializeField] private float delay = 0.5f;
    
    private int portalCount;
    private GameObject cubeyPlayer;

    /*private void Awake()
    {
        enablePortal.OnValueChanged += TriggerPortal;
        
        
        portalCount = chapterList[saveMetaData.LastChapterPlayed].LevelList[saveMetaData.LastLevelPlayed].PortalEnter.Count;
        portalEnter = new Vector3[portalCount];
        portalExit = new Vector3[portalCount];
        
        for (int i = 0; i < portalCount; i++)
        {
            portalEnter[i] = chapterList[saveMetaData.LastChapterPlayed].LevelList[saveMetaData.LastLevelPlayed].PortalEnter[i].transform.position;
            portalExit[i] = chapterList[saveMetaData.LastChapterPlayed].LevelList[saveMetaData.LastLevelPlayed].PortalExit[i].transform.position;
        }
    }

    private void OnDestroy()
    {
        enablePortal.OnValueChanged -= TriggerPortal;
    }

    private void TriggerPortal(bool on)
    {
        

        if (Vector3.Distance(portalExit[n], cubeyPlayer.transform.position) < portalDistance)
        {
            // particle effect needed
            // VisualEffects.Instance.PlayEffect(VisualEffects.Instance.Portal, other.transform.position);
            cubeyPlayer.transform.position = portalEnter[n];
        }
        else
        {
            cubeyPlayer.transform.position = portalExit[n];
        }
        // StartCoroutine(DelayPortalActivation());

    }*/
}
