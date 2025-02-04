using UnityEngine;

namespace OctoberStudio.Vibration
{
    public class VibrationManager : MonoBehaviour
    {
        private static VibrationManager instance;

        private VibrationSave save;

        private SimpleVibrationHandler vibrationHandler;

        public bool IsVibrationEnabled { get => save.IsVibrationEnabled; set => save.IsVibrationEnabled = value; }

        private void Awake()
        {
            if(instance != null)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Start()
        {
            save = GameController.SaveManager.GetSave<VibrationSave>("Vibration");
            IsVibrationEnabled = true;
#if UNITY_EDITOR
            vibrationHandler = new SimpleVibrationHandler();
#elif UNITY_IOS
            vibrationHandler = new IOSVibrationHandler();
#elif UNITY_ANDROID
            vibrationHandler = new AndroidVibrationHandler();
#elif UNITY_WEBGL
            vibrationHandler = new WebGLVibrationHandler();
#else
            vibrationHandler = new SimpleVibrationHandler();
#endif
        }

        public void Vibrate(float duration, float intensity = 1.0f)
        {
            if (!IsVibrationEnabled) return;

            if (duration <= 0) return;

            vibrationHandler.Vibrate(duration, intensity);
        }

        public void LightVibration() => Vibrate(0.08f, 0.4f);
        public void MediumVibration() => Vibrate(0.1f, 0.6f);
        public void StrongVibration() => Vibrate(0.15f, 1f);
    }
}