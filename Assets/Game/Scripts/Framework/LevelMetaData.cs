using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Cubey/Level")]
public class LevelMetaData : ScriptableObject
{
    [SerializeField] private int chapterNumber;
    [SerializeField] private string levelName = "Level ";
    [SerializeField] private int levelNumber;
    [SerializeField] private float timer = 99;
    [SerializeField] private int jumpsForGold = 3;
    [SerializeField] private int jumpsForSilver = 5;
    [SerializeField] private int jumpsForBronze = 8;

    [SerializeField] private Sprite levelSprite;
    [SerializeField] private GameObject levelPrefab;
    [SerializeField] private GameObject startPosition;
    [SerializeField] private GameObject exitPosition;
    [SerializeField] private int awardsReceived;
    [SerializeField] private List<GameObject> portalEnter;
    [SerializeField] private List<GameObject> portalExit;

    public string pathName = "Assets/Game/Prefabs/LevelPrefabs/Chapter0";

    public int AwardsReceived
    {
        get => awardsReceived;
        set => awardsReceived = value;
    }

    public string LevelName
    {
        get => levelName;
        set => levelName = value;
    }
    
    private int LevelNumber 
    {
        get => levelNumber;
        set => levelNumber = value;
    }
    
    public float Timer => timer;
    public int JumpsForBronze => jumpsForBronze;
    public int JumpsForSilver => jumpsForSilver;
    public int JumpsForGold => jumpsForGold;
    
    public Sprite LevelSprite => levelSprite;
    public GameObject LevelPrefab => levelPrefab;
    public GameObject StartPosition => startPosition;
    public GameObject ExitPosition => exitPosition;
    
    public List<GameObject> PortalEnter => portalEnter;
    public List<GameObject> PortalExit => portalExit;


    private void OnValidate()
    {

    }

    public void UnityEditorAssignData()
    {
        // AssignPath();
        GetLevelInfo();
        EditorUtil.ApplyChanges(this);
    }
    
    public void AssignPath(int chapter)
    {
        // if (pathName.Contains("Chapters") || pathName.EndsWith("Chapter0"))
        // {
            pathName = "Assets/Game/Prefabs/LevelPrefabs/Chapter0" + chapter;
            Debug.Log("path updated to: " + pathName);
            EditorUtil.ApplyChanges(this);
        // }
    }

    private void GetLevelInfo()
    {
        var pathList = EditorUtil.GetAssetsAtPath<GameObject>(pathName);

        foreach (var item in pathList)
        {
            var itemName = item.name.Replace("Level0", "Level ");
            
            if (item.name == name || itemName == name)
            {
                levelPrefab = item;
                int.TryParse(name[name.Length-2].ToString() + name[name.Length-1].ToString(), out int n);
                Debug.Log("name: " + name + ", n: " + n);
                levelNumber = n;
                levelName = "Level " + levelNumber;
            }
        }

        if (levelPrefab == null)
        {
            Debug.Log("Issue with prefab: " + name);
            return;
        }
        
        // get start and end positions of level
        startPosition = levelPrefab.transform.GetChild(1).name.Contains("Start") ? 
            levelPrefab.transform.GetChild(1).gameObject :
            levelPrefab.transform.Find("StartPosition").gameObject;
        
        // need to find this in children too
        exitPosition = levelPrefab.transform.GetChild(0).name.Contains("Exit") ? 
            levelPrefab.transform.GetChild(0).gameObject :
            levelPrefab.transform.Find("Spindle") ?
                levelPrefab.transform.Find("Spindle").GetChild(0).gameObject :
                levelPrefab.transform.Find("Exit").gameObject;

        var portalCount = levelPrefab.transform.GetComponentsInChildren<PortalManager>();
        portalEnter.Clear();
        portalExit.Clear();
        for (int i = 0; i < portalCount.Length; i++)
        {
            if (portalCount[i].CompareTag("PortalEnter"))
            {
                portalEnter.Add(portalCount[i].gameObject);
            }
            else if (portalCount[i].CompareTag("PortalExit"))
            {
                portalExit.Add(portalCount[i].gameObject);
            }
        }
    }
    
}
