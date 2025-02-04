#if UNITY_IOS
using System.Runtime.InteropServices;
using UnityEngine;
using System;
#endif

namespace OctoberStudio.Vibration
{
    public class IOSVibrationHandler : SimpleVibrationHandler
    {
#if UNITY_IOS
        [DllImport("__Internal")]
        private static extern void _Initialize();

        [DllImport("__Internal")]
        private static extern void _Play(float duration, float intensity);

        [DllImport("__Internal")]
        private static extern void _PlayPattern(string patternID);

        [DllImport("__Internal")]
        private static extern void _RegisterPattern(string jsonPatternData);

#endif

        public IOSVibrationHandler()
        {
#if UNITY_IOS
            try
            {
                _Initialize();
            }
            catch(Exception ex)
            {
                Debug.LogError(ex.Message);
            }
#endif
        }

        public override bool Vibrate(float duration, float intensity)
        {
#if UNITY_IOS
            try 
            {
                _Play(duration, intensity);
                return true;
            }
            catch
            {
                return false;
            }
#else
            return false;
#endif
        }
    }
}