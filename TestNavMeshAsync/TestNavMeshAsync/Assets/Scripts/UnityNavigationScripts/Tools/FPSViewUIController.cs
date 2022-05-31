#pragma warning disable 649
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI.Ingame
{
    public class FPSViewUIController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI fpsText;

        private bool showFPS = true;

        private string fpsFormatted;

        [SerializeField]
        private float updateInterval = 0.5F;

        private float accum = 0; // FPS accumulated over the interval
        private int frames = 0; // Frames drawn over the interval
        private float timeleft; // Left time for current interval

        private void Start()
        {
            timeleft = updateInterval;
        }

        void Update()
        {
            timeleft -= Time.deltaTime;
            accum += Time.timeScale / Time.deltaTime;
            ++frames;

            // Interval ended - update GUI text and start new interval
            if (timeleft <= 0.0)
            {
                // display two fractional digits (f2 format)
                float fps = accum / frames;
                fpsFormatted = string.Format("{0:F2}", fps);

                fpsText.text = (showFPS) ? $"FPS : {fpsFormatted}" : "";

                //	DebugConsole.Log(format,level);
                timeleft = updateInterval;
                accum = 0.0F;
                frames = 0;
            }
        }
    }
}