using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts
{
    public class SeesawManager : MonoBehaviour
    {

        private void OnCollisionEnter(Collision collision)
        {
            /*if (collision.collider.CompareTag("Player"))    
                GameManager.Instance.onMovingPlatform = true;*/

        }

        private void OnCollisionExit(Collision collision)
        {
            /*if (collision.collider.CompareTag("Player"))    
                GameManager.Instance.onMovingPlatform = false;*/
        }
    }
}
