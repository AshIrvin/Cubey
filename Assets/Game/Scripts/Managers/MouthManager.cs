using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouthManager : MonoBehaviour
{

    Animator anim;
    Animation animation;
    float frame = 2.3f;
    float totalFrames;
    float rand;



    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();

        totalFrames = anim.GetCurrentAnimatorClipInfo(0)[0].clip.length;

        ChangeMouth();

        //print("frame set to: " + frame + ", out of: " + totalFrames);

        SetRandomTimer();
    }

    void FixedUpdate()
    {

        rand -= Time.deltaTime;

        if (rand < 0)
        {
            ChangeMouth();
            SetRandomTimer();
        }
    }

    void SetRandomTimer() 
    {
        rand = Random.Range(4, 6);
        //print("timer set to: " + rand);
    }

    void ChangeMouth()
    {
        frame = Random.Range(1, totalFrames);
        anim.Play("Cubey_mouth", 0, (1f / totalFrames) * frame);
        anim.speed = 0f;
    }


}
