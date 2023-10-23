using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Cubey/Level")]
public class LevelMetaData : ScriptableObject
{
    /// <summary>
    /// This Scriptable Object is for auto assigning objects to the level
    /// It gets the correct level image, prefab and searches through the prefab
    /// to get the different data to be assigned in here, so when the game is running
    /// it can quickly get the information for each level
    /// </summary>
    
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
    [SerializeField] private List<GameObject> portalEnter;
    [SerializeField] private List<GameObject> portalExit;

    public string pathName = "Assets/Game/Prefabs/LevelPrefabs/Chapter0";
    public string mapSpritePathName = "Assets/Src_Images/MainMenu/MapLevelImages/Chapter0";

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

    public Sprite LevelSprite
    {
        get => levelSprite;
        set => levelSprite = value;
    }

    public GameObject LevelPrefab => levelPrefab;
    public GameObject StartPosition => startPosition;
    public GameObject ExitPosition => exitPosition;
    
    public List<GameObject> PortalEnter => portalEnter;
    public List<GameObject> PortalExit => portalExit;


    public void UnityEditorAssignData()
    {
#if UNITY_EDITOR
        GetLevelInfo();

        EditorUtil.ApplyChanges(this);
#endif
    }

    public void AssignPath(int chapter)
    {
#if UNITY_EDITOR
        pathName = "Assets/Game/Prefabs/LevelPrefabs/Chapter0" + chapter;
        mapSpritePathName = "Assets/Src_Images/MainMenu/MapLevelImages/Chapter0" + chapter;
        Logger.Instance.ShowDebugLog("path updated to: " + pathName);

        EditorUtil.ApplyChanges(this);
#endif
    }

    private void GetLevelInfo()
    {
#if UNITY_EDITOR
        var pathList = EditorUtil.GetAssetsAtPath<GameObject>(pathName);
        var mapButtonImages = EditorUtil.GetAssetsAtPath<Sprite>(mapSpritePathName);

        #region folder sprites to level data
        foreach (var item in mapButtonImages)
        {
            var itemName = item.name.Replace("Level0", "Level ");
            var itemName2 = item.name.Replace("Level ", "Level");
            
            Logger.Instance.ShowDebugLog("item.name: " + item.name + ", name: " + name + ", itemName2: " + itemName2); // level 11, level11 <- S.O
            
            if (item.name == name || itemName == name || itemName2 == name) // checks against name of Scriptable Object
            {
                LevelSprite = item;
            }
            else
            {
                Logger.Instance.ShowDebugLog("Wrong sprite name: " + item.name);
            }
        }
        #endregion

        #region Assign level data
        foreach (var item in pathList)
        {
            var itemName = item.name.Replace("Level0", "Level ");
            
            if (item.name == name || itemName == name)
            {
                levelPrefab = item;
                int.TryParse(name[name.Length-2].ToString() + name[name.Length-1].ToString(), out int n);
                Logger.Instance.ShowDebugLog("name: " + name + ", n: " + n);
                levelNumber = n;
                levelName = "Level " + levelNumber;
            }
        }

        if (levelPrefab == null)
        {
            Logger.Instance.ShowDebugLog("Issue with prefab: " + name);
            return;
        }
        #endregion

        #region Start/End
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
        #endregion
        
        #region Portals
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

        #endregion

#endif
    }
    
}
