using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Game.Scripts
{
    public class Timer : MonoBehaviour
    {
        public static Timer Instance { get; set; }

        public Text timerText;

        public bool enableTimer;
        public bool enableStopWatch;
        public bool objectAction;

        public float countdown;
        public float stopwatch;

        private void Start()
        {
            objectAction = false;

            countdown = 0;
        }

        private void Update()
        {
            if (enableTimer && !GameManager.Instance.allowFlight && GameManager.Instance.useTimer)
            {
                countdown -= Time.deltaTime;
                DisplayTimer(countdown);

                if (countdown < 1)
                {
                    GameManager.Instance.LoadEndScreen(false);
                    objectAction = true;
                    timerText.text = "--";
                    enableTimer = false;
                }
            }

            if (enableStopWatch) // (GameManager.Instance.allowFlight)
            {
                stopwatch += Time.deltaTime;
                DisplayTimer(stopwatch);
            }
        }

        public void ResetClocks()
        {
            //if (GameManager.Instance.allowFlight)
            //{
            //    stopwatch = 0;
            //    DisplayTimer(stopwatch);
            //} else
            //{
            //    countdown = 60;
            //    DisplayTimer(countdown);
            //}
        }


        public void StartTimer(float time)
        {
            timerText.gameObject.SetActive(true);
            countdown = time;
            enableTimer = true;
            objectAction = false;
        }

        public void DisplayTimer(float n)
        {
            timerText.text = n.ToString("00");
        }

        public void StopTimer()
        {
            enableTimer = false;
            timerText.gameObject.SetActive(false);
        }

    }
}