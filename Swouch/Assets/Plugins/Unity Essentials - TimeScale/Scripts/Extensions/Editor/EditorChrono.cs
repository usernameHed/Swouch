﻿using UnityEditor;
using UnityEngine;

namespace unityEssentials.timeScale.extensions.editor
{
    /// <summary>
    /// Editor chrono class. Use:
    /// private static EditorChrono _timer = new EditorChrono();
    /// 
    /// start chrono:
    /// _timer.StartChrono(time);
    /// 
    /// test chrono:
    /// _timer.IsNotRunning() //has never been started, OR is finished
    /// _timer.IsFinished() //has started and finished
    /// _timer.GetTimer() //get current time
    /// _timer.Reset() //reset timer
    /// </summary>
    [System.Serializable]
    public class EditorChrono
    {
        private bool _isStarted = false;
        private float _finalTime;

        /// <summary>
        /// Start the CoolDown with a given time. It will go from time to 0
        /// </summary>
        public void StartChrono(float time)
        {
            time = (time < 0) ? 0 : time;

            _finalTime = (float)EditorApplication.timeSinceStartup + time;
            _isStarted = true;
        }

        /// <summary>
        /// return the current timer, in seconds
        /// eg: return 125.95 if the timer is at 2 minutes, 5 seconds and 95 miliseconds
        /// </summary>
        /// <returns>return the time in seconds eg: return 125.95 if the timer is at 2 minutes, 5 seconds and 95 miliseconds</returns>
        public float GetTimer()
        {
            if (!IsRunning())
                return (0);

            float currentTime = _finalTime - (float)EditorApplication.timeSinceStartup;
            return (currentTime);
        }

        /// <summary>
        /// Is the CoolDown started, but not finished yet ?
        /// </summary>
        public bool IsRunning()
        {
            if (_isStarted && !IsReady(false))
            {
                return (true);
            }
            return (false);
        }

        public bool IsNotRunning()
        {
            return (!IsRunning());
        }

        /// <summary>
        /// return true if the timer is finished
        /// if the parametter resetIfFinished si set to true, the coolDown will
        /// reset if the coolDown is finished. That's mean if we test IsFinished() again, it will return false
        /// 
        /// if the timer never started, return false;
        /// </summary>
        /// <param name="resetIfFinished">if true, the coolDown is reset. That's mean if we test IsFinished() again, it will return false</param>
        /// <returns>true if the coolDown is finished</returns>
        public bool IsFinished(bool resetIfFinished = true)
        {
            if (_isStarted && IsReady(resetIfFinished))
            {
                return (true);
            }
            return (false);
        }

        /// <summary>
        /// reset the coolDown & all the options
        /// </summary>
        public void Reset()
        {
            _isStarted = false;    //le cooldown est fini, on reset
            _finalTime = 0;
        }

        private bool IsReady(bool canDoReset)
        {
            if (!_isStarted)   //cooldown never started, do'nt do anything
                return (false);

            if ((float)EditorApplication.timeSinceStartup >= _finalTime) //le coolDown a commencé, et est terminé !
            {
                if (canDoReset)
                    Reset();
                return (true);
            }

            return (false); //cooldown a commencé, et est en cours
        }
    }
}