using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapsButtons : MonoBehaviour
{
    // public class List<GameObject> chapterMaps;
    // public List<GameObject> mapLevelButtons;

    public chapterMaps[] maps = new chapterMaps[5];
    public chapterButtons[] buttons;

}

public struct chapterMaps
{
    // public ChapterList chapterList;
    // public SaveMetaData saveMetaData;

    public GameObject maps;


}

public struct chapterButtons
{
    public GameObject buttons;
}
