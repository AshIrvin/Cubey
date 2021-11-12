
using UnityEngine;

[System.Serializable]
public class ClayxelsPrefs {
    public int[] boundsColor = new int[]{100, 100, 100, 50};
    public string pickingKey = "p";
    public string mirrorDuplicateKey = "m";
    public int maxPointCount = 0;
    public int maxSolidsCount = 0;
    public int maxSolidsPerVoxel = 0;
    public int frameSkip = 0;
    public int maxBounds = 3;
    public bool vramLimitEnabled = true;
    public string defaultAssetsPath = "clayxelsFrozen";
    public bool directPickEnabled = true;
    public Vector2Int renderSize = new Vector2Int(1024, 1024);
    public float globalBlend = 1.0f;
}
