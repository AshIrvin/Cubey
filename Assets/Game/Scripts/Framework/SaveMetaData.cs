using UnityEngine;

[CreateAssetMenu(menuName = "Cubey/Save")]
public class SaveMetaData : ScriptableObject
{
    [SerializeField] private string saveName;
    [SerializeField] private string playerName;
    [SerializeField] private int lastChapterPlayed;
    [SerializeField] private int lastLevelPlayed;
    // [SerializeField] private bool returnToMap;
    [SerializeField] private string levelLoaded;

    public string SaveName
    {
        get => saveName;
        set => saveName = value;
    }

    public string PlayerName
    {
        get => playerName;
        set => playerName = value;
    }

    public int LastChapterPlayed
    {
        get => lastChapterPlayed;
        set => lastChapterPlayed = value;
    }

    public int LastLevelPlayed
    {
        get => lastLevelPlayed;
        set => lastLevelPlayed = value;
    }

    /*public bool ReturnToMap
    {
        get => returnToMap;
        set => returnToMap = value;
    }*/

    public string LevelLoaded
    {
        get => levelLoaded;
        set => levelLoaded = value;
    }
}
