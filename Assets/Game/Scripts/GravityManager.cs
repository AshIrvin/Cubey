using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Game.Scripts
{
    public class GravityManager : MonoBehaviour
    {

        [Tooltip("1-up. 2-normal/down. 3-left. 4-right")]
        public int gravityDir;
        
        private float grav = -9.81f;

        private void ChangeGravity()
        {
            print("gravity: " + grav);

            switch (gravityDir)
            {
                case 1:
                    Physics.gravity = new Vector3(0, -grav, 0);
                    break;
                case 2:
                    Physics.gravity = new Vector3(0, grav, 0);
                    break;
                case 3:
                    Physics.gravity = new Vector3(-grav, 0, 0);
                    break;
                case 4:
                    Physics.gravity = new Vector3(grav, 0, 0);
                    break;
                default:
                    Physics.gravity = new Vector3(0, grav, 0);
                    break;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                ChangeGravity();
            }
        }

    }

}