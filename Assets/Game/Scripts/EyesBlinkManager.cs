using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EyesBlinkManager : MonoBehaviour
{
    // Set default position
    // Get target direction of nearest object
        // Get a list of all points of interest, find closest
    // move to target direction
    // stop within certain distance
    // 


    public GameObject blinkingGO;
    public int minBlink = 2, maxBlink = 5;

    float randomBlink;
    bool blinkAllowed = true;

    Animator blinkingAnim;

    // Start is called before the first frame update
    void Start()
    {
        blinkingAnim = blinkingGO.GetComponent<Animator>();

        SetRandomBlink();

        if (blinkingGO == null)
        {

        }
    }


    void FixedUpdate()
    {
        if (blinkAllowed){
            randomBlink -= Time.deltaTime;
            if (randomBlink < 0) {
                blinkingAnim.SetBool("Play", true);
                blinkAllowed = false;
                SetRandomBlink();
                //print("Eyes blink");
            }
        } else {
            randomBlink -= Time.deltaTime;
            if (randomBlink < 0){
                blinkingAnim.SetBool("Play", false);
                blinkAllowed = true;
                SetRandomBlink();
            }
        }

    }

    void SetRandomBlink(){
        randomBlink = UnityEngine.Random.Range(minBlink, maxBlink);
    }


    void EyesBlinking(){
        //blinkingAnim.Play("Eyes_blinking");
        blinkingAnim.SetBool("Play", true);
    }


}
