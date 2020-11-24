using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public GameObject[] tutorialScreens;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void NextTutorialScreen()
    {
        for (int i = 0; i < tutorialScreens.Length; i++)
        {
            if (tutorialScreens[i].activeInHierarchy)
            {
                tutorialScreens[i].SetActive(false);
                tutorialScreens[i + 1].SetActive(true);
            }
        }
    }
}
