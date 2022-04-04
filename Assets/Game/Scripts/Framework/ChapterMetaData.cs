using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Cubey/Chapter")]
public class ChapterMetaData : ScriptableObject
{
    [Tooltip("Prefab for menu screen")]
    [SerializeField] private GameObject menuEnvironment;
    [SerializeField] private int chapterNumber;
    [SerializeField] private string chapterName;
    [SerializeField] private GameObject pickupIcon;
    // [SerializeField] private int lastLevelPlayed;
    // [SerializeField] private int lastLevelUnlocked;
    // [SerializeField] private bool chapterUnlocked;
    [SerializeField] private GameObject chapterMap;
    [SerializeField] private GameObject cubeyPlayer;
    [SerializeField] private LevelList levelList;
    [SerializeField] private float menuZoomLevel;
    [SerializeField] private List<GameObject> chapterMapButtonList;
    [SerializeField] private List<GameObject> inGameMapButtonList;
    // [SerializeField] private int awardsBronze;
    // [SerializeField] private int awardsSilver;
    // [SerializeField] private int awardsGold;
    [SerializeField] private string fullPathName = "Assets/Src_Images/MainMenu/MapLevelImages/Chapter";
    
    private readonly string levelButton = "Leveln_button";
    public GameObject MenuEnvironment => menuEnvironment;
    public int ChapterNumber => chapterNumber;
    public string ChapterName => chapterName;
    public GameObject PickupIcon => pickupIcon;
    public GameObject ChapterMap => chapterMap;
    public LevelList LevelList => levelList;
    public List<GameObject> ChapterMapButtonList => chapterMapButtonList;

    public List<GameObject> InGameMapButtonList
    {
        get => inGameMapButtonList;
        set => inGameMapButtonList = value;
    }

    public float MenuZoomLevel
    {
        get => menuZoomLevel;
        set => menuZoomLevel = value;
    }
    
    /// <summary>
    /// Takes the buttons from the prefab map and assigns them above
    /// </summary>
    public void UnityEditorAssignData()
    {
#if UNITY_EDITOR
        Debug.Log("Assigning buttons from game to list");
        
        var buttonParent = chapterMap.transform.Find("Canvas_Map").Find("Map_buttons").gameObject;
        chapterMapButtonList.Clear();
        
        for (int i = 0; i < buttonParent.transform.childCount; i++)
        {
            if (buttonParent.transform.GetChild(i).name.Contains(levelButton))
            {
                chapterMapButtonList.Add(buttonParent.transform.GetChild(i).gameObject);
            }
        }
#endif
    }
    
    /// <summary>
    /// Assigns sprite from location to each prefab level button
    /// </summary>
    public void UnityEditorAutoAssignSprite()
    {
#if UNITY_EDITOR
        if (fullPathName == "") return;

        for (int i = 0; i < chapterMapButtonList.Count; i++)
        {
            var imageName = "Level" + (i+1).ToString("00") + ".JPG";

            var sprite = (Sprite) AssetDatabase.LoadAssetAtPath(fullPathName + imageName, typeof(Sprite));
            if (sprite == null)
            {
                imageName = "Level " + (i+1).ToString("00") + ".JPG";
                sprite = (Sprite) AssetDatabase.LoadAssetAtPath(fullPathName + imageName, typeof(Sprite));
            }
            var image = chapterMapButtonList[i].transform.Find("Mask/Screenshot").GetComponent<Image>();

            if (sprite == null)
            {
                Debug.LogError("can't find sprite");
                return;
            }
            
            if (image == null)
            {
                Debug.LogError("can't find image");
                return;
            }

            image.sprite = sprite;
            Debug.Log($"Assigning {sprite.name} sprite to {image.transform.parent.parent.name}. image.sprite null: {image.sprite}");
            Debug.LogError("Assign in game instead. User mapmanager/line 114");
        }
#endif
    }

    /// <summary>
    /// Assigns numbers to each level button of this chapter
    /// </summary>
    public void UnityEditorAssignLevelNumbers()
    {
#if UNITY_EDITOR
        for (int i = 0; i < chapterMapButtonList.Count; i++)
        {
            ChapterMapButtonList[i].GetComponentInChildren<Text>()
                .text = (i + 1).ToString();
        }
        
        EditorUtil.ApplyChanges(this);
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    public void UnityEditorAssignInfoToLevelMetaData()
    {
#if UNITY_EDITOR
        for (int i = 0; i < levelList.Count; i++)
        {
            levelList[i].UnityEditorAssignData();
        }
        // EditorUtil.ApplyChanges(this);
#endif
    }
    
    public void UnityEditorAssignPathToLevelMetaData()
    {
#if UNITY_EDITOR
        for (int i = 0; i < levelList.Count; i++)
        {
            levelList[i].AssignPath(chapterNumber);
            
        }
#endif
    }

    /*public void DeleteAwardsForChapter()
    {
        // TODO - delete all level awards in each chapter 
/*#if UNITY_EDITOR
        for (int i = 0; i < levelList.Count; i++)
        {
            // levelList[i].AwardsReceived = 0;
            
            SaveLoadManager.SetAward(i, 0);
        }
#endif#1#
    }*/
}
