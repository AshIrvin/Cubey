using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Game.Scripts
{
    public class PlayerConfig : MonoBehaviour
    {

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.CompareTag("Platform"))
            {
                VisualEffects.Instance.LandingDust();
            }
        }

    }
}