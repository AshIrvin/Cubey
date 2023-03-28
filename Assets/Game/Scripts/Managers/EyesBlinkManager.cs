using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EyesBlinkManager : MonoBehaviour
{
    [SerializeField] private GameObject[] pupils;
    [SerializeField] private int minBlink;
    [SerializeField] private int maxBlink;
    [SerializeField] private float blinkTime = 0.3f;
    
    public float time;

    private void Start()
    {
        time = SetRandomBlink();
    }

    void Update()
    {
        time -= Time.deltaTime;

        if (time < 0)
        {
            BlinkPupils(0.2f); // scale
            StartCoroutine(ResetBlink());
        }
    }

    private void BlinkPupils(float scaleValue)
    {
        foreach (var pupil in pupils)
        {
            var scale = pupil.transform.localScale;
            scale.y = scaleValue;
            pupil.transform.localScale = scale;
        }
    }

    IEnumerator ResetBlink()
    {
        yield return new WaitForSeconds(blinkTime);
        time = SetRandomBlink();
        BlinkPupils(1);
    }
    
    private int SetRandomBlink()
    {
        return UnityEngine.Random.Range(minBlink, maxBlink);
    }
}
