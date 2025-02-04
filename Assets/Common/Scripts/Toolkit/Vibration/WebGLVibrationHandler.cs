#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
using UnityEngine;
using System;
#endif

namespace OctoberStudio.Vibration
{
    public class WebGLVibrationHandler : SimpleVibrationHandler
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void _Initialize();

        [DllImport("__Internal")]
        private static extern void _Play(int duration);
#endif

        public WebGLVibrationHandler()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
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
#if UNITY_WEBGL && !UNITY_EDITOR
            if (!Application.isMobilePlatform) return false;

            try 
            {
                _Play((int)(duration * 1000));
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