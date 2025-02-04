using OctoberStudio.Abilities.UI;
using OctoberStudio.Audio;
using OctoberStudio.Easing;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace OctoberStudio.UI
{
    public class PauseWindowBehavior : MonoBehaviour
    {
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] Button continueButton;
        [SerializeField] Button exitButton;
        [SerializeField] List<AbilitiesIndicatorsListBehavior> pauseAbilitiesLists;

        [Header("Settings")]
        [SerializeField] ToggleBehavior soundToggle;
        [SerializeField] ToggleBehavior musicToggle;
        [SerializeField] ToggleBehavior vibrationToggle;

        public event UnityAction OnStartedClosing;
        public event UnityAction OnClosed;

        private StageSave stageSave;

        private void Awake()
        {
            continueButton.onClick.AddListener(ContinueButtonClick);
            exitButton.onClick.AddListener(ExitButtonClick);
        }

        private void Start()
        {
            soundToggle.SetToggle(GameController.AudioManager.SoundVolume != 0);
            musicToggle.SetToggle(GameController.AudioManager.MusicVolume != 0);
            vibrationToggle.SetToggle(GameController.VibrationManager.IsVibrationEnabled);

            soundToggle.onChanged += (soundEnabled) => GameController.AudioManager.SoundVolume = soundEnabled ? 1 : 0;
            musicToggle.onChanged += (musicEnabled) => GameController.AudioManager.MusicVolume = musicEnabled ? 1 : 0;
            vibrationToggle.onChanged += (vibrationEnabled) => GameController.VibrationManager.IsVibrationEnabled = vibrationEnabled;

            stageSave = GameController.SaveManager.GetSave<StageSave>("Stage");
        }

        public void Open()
        {
            gameObject.SetActive(true);

            canvasGroup.alpha = 0f;
            canvasGroup.DoAlpha(1, 0.3f).SetUnscaledTime(true);

            Time.timeScale = 0f;

            for (int i = 0; i < pauseAbilitiesLists.Count; i++)
            {
                var abilityList = pauseAbilitiesLists[i];

                abilityList.Show();
                abilityList.Refresh();
            }
        }

        public void Close()
        {
            canvasGroup.DoAlpha(0, 0.3f).SetUnscaledTime(true).SetOnFinish(() => {
                gameObject.SetActive(false);
                Time.timeScale = 1f;

                OnClosed?.Invoke();
            });

            OnStartedClosing?.Invoke();
        }

        private void ContinueButtonClick()
        {
            GameController.AudioManager.PlaySound(AudioManager.BUTTON_CLICK_HASH);
            Close();
        }

        private void ExitButtonClick()
        {
            GameController.AudioManager.PlaySound(AudioManager.BUTTON_CLICK_HASH);
            AdsManager.Adinstance.ShowIntertitialAD();
            Time.timeScale = 1f;

            stageSave.IsPlaying = false;

            StageController.ReturnToMainMenu();
        }
    }
}