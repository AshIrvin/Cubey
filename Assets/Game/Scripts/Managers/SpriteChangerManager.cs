using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteChangerManager : MonoBehaviour
{
    public float timeMin;
    public float timeMax;
    private float time;
    private float smileTime;
    public float timeSpriteDisabled = 0.2f;

    public bool blinkEyes;
    public bool smiles;
    public GameObject[] sprites;
    
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
                //SetSprite(2);

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

        if (n >= 0 && n < sprites.Length)
        {
            sprites[n].SetActive(true);
            try
            {
                if (sprites.Length > 1)
                {
                    if (m == 2)
                        sprites[m].SetActive(true);
                }
            }
            catch (System.Exception ex)
            {
                Debug.Log("sprite changer error: " + ex.Message);
            }
        }

        if (n == -1 && sprites.Length > 0)
        {
            sprites[Random.Range(0, sprites.Length)].SetActive(true);
        }

    }

}
