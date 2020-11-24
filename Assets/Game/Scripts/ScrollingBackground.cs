using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Game.Scripts
{
    public class ScrollingBackground : MonoBehaviour
    {
        public float speed = 0.5f;
        public float pos = 18;
        public float resetPoint;

        public float backBushSpeed = 0.4f;
        public float midBushSpeed = 0.2f;
        public float frontBushSpeed = 0.1f;

        private int bonusLevel = 5;

        public bool dontLoop;


        private void FixedUpdate()
        {
            if (GameManager.Instance.allowFlight)
            {
                // move different layers
                if (gameObject.name.Contains("bush_back"))
                {
                    transform.Translate(Vector3.left * Time.deltaTime * (speed - backBushSpeed), Space.World);
                }
                else if (gameObject.name.Contains("bush_mid"))
                {
                    transform.Translate(Vector3.left * Time.deltaTime * (speed - midBushSpeed), Space.World);
                }
                else if (gameObject.name.Contains("bush_front"))
                {
                    transform.Translate(Vector3.left * Time.deltaTime * (speed - frontBushSpeed), Space.World);
                }
                else
                {
                    transform.Translate(Vector3.left * Time.deltaTime * speed, Space.World);
                }

                // kill
                if (transform.position.x < -resetPoint)
                {
                    var t = transform.position;
                    if (!dontLoop)
                        t.x = pos;
                    else
                        Destroy(this);
                    transform.position = t;
                }
            }
            //if (GameManager.Instance.allowFlight)
            //{
            //    transform.Translate(Vector3.left * Time.deltaTime * speed, Space.World);


            //}
        }
    }
}