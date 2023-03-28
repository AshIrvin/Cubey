using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts
{
    public class CharacterManager : MonoBehaviour
    {
        private bool moveRight;

        private float normStartRightOnScreen = 0.65f;
        private float normStartLeftOnScreen = 0.15f;
        private float normStartBeginning;

        private Animator anim;

        private void Start()
        {
            anim = transform.Find("Xagon").GetComponent<Animator>();

            var n = Random.Range(0, 2);
            if (n == 1)
            {
                gameObject.SetActive(false);
            }
            else
            {
                RandomChooseXagonStartPos();    
            }
        }

        private void RandomChooseXagonStartPos()
        {
            int r = Random.Range(0, 3);

            if (r == 0)
                anim.Play("Xagon_rolling2", 0, normStartLeftOnScreen);
            else if (r == 1)
                anim.Play("Xagon_rolling2", 0, normStartRightOnScreen);
            else
                anim.Play("Xagon_rolling2", 0, normStartBeginning);
        }

        /*private void ChangeDirection()
        {
            if (GetComponent<Animator>() != null)
            {
                var anim = GetComponent<Animation>();
                var scale = transform.localScale;
                var animator = GetComponent<Animator>();
                //if (anim.wrapMode)
                anim.wrapMode = WrapMode.PingPong;
                animator.playbackTime = 1;

                if (!anim.IsPlaying("Xagon_rolling2") && animator.playbackTime > 35)
                {
                    print("scale changed");
                    scale = new Vector3(0.5f, 0.5f, 0.5f);
                } 
                else if (!anim.IsPlaying("Xagon_rolling2") && animator.playbackTime < 5)
                {
                    scale = new Vector3(1, 1, 1);
                }
            }
        }*/

        /*private void ChangeDirectionOffScreen()
        {
            if (gameObject.transform.position.x > Screen.width && moveRight)
            {
                moveRight = false;
                var anim = GetComponent<Animation>();
                anim.wrapMode = WrapMode.PingPong;
            }
        }*/

    }
}