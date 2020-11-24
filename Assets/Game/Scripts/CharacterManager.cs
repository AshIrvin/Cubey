using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Game.Scripts
{
    public class CharacterManager : MonoBehaviour
    {
        private bool moveRight;

        // get clip range for rolling to the 1st stop
        // then join it together with the next movement
        // set a random wait time
        // move the start position over randomly too

        private float normStartRightOnScreen = 0.65f;
        private float normStartLeftOnScreen = 0.15f;
        private float normStartBeginning;

        private Animator anim;

        // Start is called before the first frame update
        private void Start()
        {
            //var anim = GetComponent<Animation>();
            //anim.wrapMode = WrapMode.PingPong;

            anim = GetComponent<Animator>();
            //if (anim != null)
            //{
            //anim1.StopPlayback();
            //anim1.playbackTime = 30;
            //anim1.SetFloat("Xagon_rolling2", 30f);
            //    anim.Play("Xagon_rolling2", 0, normStartRightOnScreen);
            //    print("anim passed thru");
            //}

            RandomChooseZagonStartPos();
        }

        private void RandomChooseZagonStartPos()
        {
            int r = Random.Range(0, 2);
            print("random: " + r);

            if (r == 0)
                anim.Play("Xagon_rolling2", 0, normStartLeftOnScreen);
            else if (r == 1)
                anim.Play("Xagon_rolling2", 0, normStartRightOnScreen);
            else
                anim.Play("Xagon_rolling2", 0, normStartBeginning);
        }

        // Update is called once per frame
        private void Update()
        {
            //if (gameObject.transform.name == "Xagon" &&
            //    !GetComponent<Animation>().IsPlaying("Xagon_rolling2") &&
            //    MainMenuManager.Instance != null
            //    )
            //{
            //    ChangeDirection();
            //}
        }

        private void ChangeDirection()
        {
            if (GetComponent<Animator>() != null)
            {
                var anim = GetComponent<Animation>();
                var scale = transform.localScale;
                var animator = GetComponent<Animator>();
                //if (anim.wrapMode)
                anim.wrapMode = WrapMode.PingPong;
                animator.playbackTime = 1;


                if (!anim.IsPlaying("Xagon_rolling2") &&
                    animator.playbackTime > 35)
                {
                    print("scale changed");
                    scale = new Vector3(0.5f, 0.5f, 0.5f);
                } else if (!anim.IsPlaying("Xagon_rolling2") &&
                    animator.playbackTime < 5)
                {
                    scale = new Vector3(1, 1, 1);
                }
            }
        }

        private void ChangeDirectionOffScreen()
        {
            if (gameObject.transform.position.x > Screen.width && moveRight)
            {
                moveRight = false;
                var anim = GetComponent<Animation>();
                anim.wrapMode = WrapMode.PingPong;
            }
        }

    }
}