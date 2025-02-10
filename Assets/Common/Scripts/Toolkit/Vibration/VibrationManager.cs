using OctoberStudio.Easing;
using OctoberStudio.Input;
using UnityEngine;
using UnityEngine.InputSystem;
#if UNITY_WEBGL
using UnityEngine.InputSystem.WebGL;
#endif

namespace OctoberStudio.Vibration
{
    public class VibrationManager : MonoBehaviour
    {
        private static VibrationManager instance;

        private VibrationSave save;

        private SimpleVibrationHandler vibrationHandler;

        public bool IsVibrationEnabled { get => save.IsVibrationEnabled; set => save.IsVibrationEnabled = value; }

        private IEasingCoroutine gamepadVibrationCoroutine;

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

            if(GameController.InputManager.ActiveInput != InputType.Gamepad)
            {
                vibrationHandler.Vibrate(duration, intensity);
            }
            else
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                if (WebGLGamepad.current != null) WebGLGamepad.current.SetMotorSpeeds(intensity, intensity);
#else
                if (Gamepad.current != null) Gamepad.current.SetMotorSpeeds(intensity, intensity);
#endif
                gamepadVibrationCoroutine.StopIfExists();

                gamepadVibrationCoroutine = EasingManager.DoAfter(duration, () => 
                {
#if UNITY_WEBGL && !UNITY_EDITOR
                if (WebGLGamepad.current != null) WebGLGamepad.current.SetMotorSpeeds(0, 0);
#else
                    if (Gamepad.current != null) Gamepad.current.SetMotorSpeeds(0, 0);
#endif
                });
            }
        }

        public void LightVibration() => Vibrate(0.08f, 0.4f);
        public void MediumVibration() => Vibrate(0.1f, 0.6f);
        public void StrongVibration() => Vibrate(0.15f, 1f);
    }
}