using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteChangerManager : MonoBehaviour
{
    SpriteRenderer spr;

    public float timeMin, timeMax;
    public float time, smileTime, timeSpriteDisabled = 0.2f;

    Color colour;

    bool timerEnabled;
    public bool blinkEyes, smiles;//, smoothTransition;

    public GameObject[] sprites;

    //Color t = new Color(1, 1, 1, 0);
    //Color o = new Color(1, 1, 1, 1);

    // Start is called before the first frame update
    void Start()
    {

        //time = SetRandomTime();
        smileTime = SetRandomTime();
        SetSprite(1, 0);
        BlinkingEyes();
        time = 1f;
    }

    void FixedUpdate()
    {

        if (blinkEyes)
            BlinkingEyes();

        if (smiles)
            SetSmiles();

        //if (smoothTransition)
        //    SmoothTransition();

    }

    void SmoothTransition()
    {

        time -= Time.deltaTime;
        if (time < 0)
        {
            SetSprite(1, 0);
            time = 1f;
        }

        
    }

    void SetSmiles()
    {
        smileTime -= Time.deltaTime;

        if (smileTime < 0)
        {
            SetSprite(-1, 0);

            smileTime = SetRandomTime();
        }
    }

    void BlinkingEyes() 
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

    float SetRandomTime()
    {
        time = Random.Range(timeMin, timeMax);
        return time;

    }

    void SetAlpha(int c)
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

            }
        }

        if (n == -1 && sprites.Length > 0)
        {
            sprites[Random.Range(0, sprites.Length)].SetActive(true);
        }

    }

}
