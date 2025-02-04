using OctoberStudio.Audio;
using OctoberStudio.Easing;
using OctoberStudio.Upgrades;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace OctoberStudio.UI
{
    public class StageFailedScreen : MonoBehaviour
    {
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] Button reviveButton;
        [SerializeField] Button exitButton;

        private Canvas canvas;

        private bool revivedAlready = false;

        private void Awake()
        {
            canvas = GetComponent<Canvas>();

            reviveButton.onClick.AddListener(ReviveButtonClick);
            exitButton.onClick.AddListener(ExitButtonClick);

            revivedAlready = false;
        }

        public void Show()
        {
            canvas.enabled = true;
            canvasGroup.alpha = 0;
            canvasGroup.DoAlpha(1, 0.3f).SetUnscaledTime(true);

            if (GameController.UpgradesManager.IsUpgradeAquired(UpgradeType.Revive) && !revivedAlready)
            {
                reviveButton.gameObject.SetActive(true);
            } else
            {
                reviveButton.gameObject.SetActive(false);
            }
        }

        public void Hide(UnityAction onFinish)
        {
            canvasGroup.DoAlpha(0, 0.3f).SetUnscaledTime(true).SetOnFinish(() => {
                canvas.enabled = false;
                onFinish?.Invoke();
            });
        }

        private void ReviveButtonClick()
        {
            GameController.AudioManager.PlaySound(AudioManager.BUTTON_CLICK_HASH);
            Hide(StageController.ResurrectPlayer);
            revivedAlready = true;
            AdsManager.Adinstance.ShowIntertitialAD();
        }

        private void ExitButtonClick()
        {
            GameController.AudioManager.PlaySound(AudioManager.BUTTON_CLICK_HASH);
            Time.timeScale = 1;
            StageController.ReturnToMainMenu();
            AdsManager.Adinstance.ShowIntertitialAD();
        }
    }
}