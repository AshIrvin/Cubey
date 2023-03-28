using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteChangerManager : MonoBehaviour
{
    [SerializeField] private float timeMin;
    [SerializeField] private float timeMax;
    public float time;
    [SerializeField] private float smileTime;
    [SerializeField] private float timeSpriteDisabled = 0.2f;

    [SerializeField] private bool blinkEyes;
    [SerializeField] private bool smiles;
    [SerializeField] private GameObject[] sprites;
    
    private SpriteRenderer spr;
    private Color colour;
    private bool timerEnabled;

    private void Start()
    {
        smileTime = SetRandomTime();
        SetSprite(1, 0);
        BlinkingEyes();
        time = 1f;
    }

    private void Update()
    {
        if (blinkEyes)
            BlinkingEyes();

        if (smiles)
            SetSmiles();
    }

    /*private void SmoothTransition()
    {

        time -= Time.deltaTime;
        if (time < 0)
        {
            SetSprite(1, 0);
            time = 1f;
        }
    }*/

    private void SetSmiles()
    {
        smileTime -= Time.deltaTime;

        if (smileTime < 0)
        {
            SetSprite(-1, 0);

            smileTime = SetRandomTime();
        }
    }

    private void BlinkingEyes() 
    {
        if (timerEnabled)
        {
            time -= Time.deltaTime;
            if (time < 0)
            {
                SetSprite(0, 2);

                time = SetRandomTime();
                timerEnabled = false;
            }
        }
        else
        {
            time -= Time.deltaTime;
            if (time < 0)
            {
                SetSprite(1, 0);

                timerEnabled = true;
                time = timeSpriteDisabled;
            }
        }
    }

    private float SetRandomTime()
    {
        time = Random.Range(timeMin, timeMax);
        return time;
    }

    private void SetAlpha(int c)
    {
        colour.a = c;
        spr.color = colour;
    }

    // -1 = random sprite
    // otherwise, set manually
    public void SetSprite(int n, int m)
    {
        foreach (GameObject go in sprites)
        {
            spr = go.GetComponent<SpriteRenderer>();

            go.SetActive(false);

        }
        switch (n)
        {
            case -1:
                sprites[Random.Range(0, sprites.Length-1)]?.SetActive(true);
                break;
            case 0:
                sprites[n]?.SetActive(true);
                break;
            case 1:
                if (sprites.Length > 1)
                {
                    sprites[n]?.SetActive(true);
                }
                break;
        }

        switch (m)
        {
            case 0:
                break;
            case 1:
                break;
            case 2:
                if (sprites.Length > 2)
                    sprites[m]?.SetActive(true);
                break;
        }
        
        // if (n >= 0 && n < sprites.Length)
        // {
        //     sprites[n].SetActive(true);
        // }
        
        // if (n == 0 && sprites.Length >= m)
        // {
        //     if (m == 2)
        //     {
        //         sprites[m].SetActive(true);
        //     }
        // }

        // if (n == -1 && sprites.Length > 0)
        // {
        //     sprites[Random.Range(0, sprites.Length)].SetActive(true);
        // }

    }

}
