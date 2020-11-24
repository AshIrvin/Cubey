using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Game.Scripts
{
    public class PlayerConfig : MonoBehaviour
    {
        // Todo - move player related setup methods here
        // or change name of script

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.CompareTag("Platform"))
            {
                VisualEffects.Instance.LandingDust();
            }
        }

    }
}