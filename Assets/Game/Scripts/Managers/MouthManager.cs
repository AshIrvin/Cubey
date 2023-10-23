using UnityEngine;

public class MouthManager : MonoBehaviour
{
    private Animator anim;

    private float frame = 2.3f;
    private float totalFrames;
    private float rand;
    private float startTimerRange = 4;
    private float endTimerRange = 6;

    private void Start()
    {
        anim = GetComponent<Animator>();

        totalFrames = anim.GetCurrentAnimatorClipInfo(0)[0].clip.length;

        ChangeMouthImage();

        SetRandomTimer();
    }

    private void LateUpdate()
    {
        MouthTimer();
    }

    private void MouthTimer()
    {
        rand -= Time.deltaTime;

        if (rand < 0)
        {
            ChangeMouthImage();
            SetRandomTimer();
        }
    }

    private void SetRandomTimer() 
    {
        rand = Random.Range(startTimerRange, endTimerRange);
    }

    private void ChangeMouthImage()
    {
        frame = Random.Range(1, totalFrames);
        anim.Play("Cubey_mouth", 0, (1f / totalFrames) * frame);
        anim.speed = 0f;
    }
}
