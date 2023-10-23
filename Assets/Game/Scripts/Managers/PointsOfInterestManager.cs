using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointsOfInterestManager : MonoBehaviour
{
    // for the characters looking at each other
    /*private MapManager mapManager;
    public List<GameObject> pointsOfInterestList;
    private GameObject levelObject;
    [SerializeField] private bool enableInterestSearch = true; 
    private void Awake()
    {
        mapManager = FindObjectOfType<MapManager>();
    }

    private void OnEnable()
    {
        if (pointsOfInterestList.Count == 0 && enableInterestSearch)
            FindPointsOfInterest(gameObject);
    }

    public void LevelInit()
    {
        FindPointsOfInterest(levelObject);
    }

    private void FindPointsOfInterest(GameObject go)
    {
        pointsOfInterestList.Clear();
        if (mapManager.LevelGameObject != null)
            levelObject = mapManager.LevelGameObject;

        if (go.transform.childCount == 0)
            return;
        var transforms = go.GetComponentsInChildren<Transform>();
        foreach (var t in transforms)
        {
            if (t.transform.CompareTag("PointOfInterest"))
            {
                pointsOfInterestList.Add(t.gameObject);
            }
        }
        Logger.Instance.ShowDebugLog("Points of interests found: " + pointsOfInterestList.Count + " in " + go.name);
    }*/
}
