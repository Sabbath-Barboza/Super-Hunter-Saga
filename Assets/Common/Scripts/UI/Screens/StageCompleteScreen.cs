using OctoberStudio.Audio;
using OctoberStudio.Easing;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace OctoberStudio.UI
{
    public class StageCompleteScreen : MonoBehaviour
    {
        private Canvas canvas;

        private static readonly int STAGE_COMPLETE_HASH = "Stage Complete".GetHashCode();

        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] Button button;

        private void Awake()
        {
            canvas = GetComponent<Canvas>();

            button.onClick.AddListener(OnButtonClicked);
        }

        public void Show(UnityAction onFinish = null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.DoAlpha(1f, 0.3f).SetUnscaledTime(true).SetOnFinish(onFinish);

            canvas.enabled = true;

            GameController.AudioManager.PlaySound(STAGE_COMPLETE_HASH);
        }

        public void Hide(UnityAction onFinish = null)
        {
            canvasGroup.DoAlpha(0f, 0.3f).SetUnscaledTime(true).SetOnFinish(() => {
                canvas.enabled = false;
                onFinish?.Invoke();
            });
        }

        private void OnButtonClicked()
        {
            GameController.AudioManager.PlaySound(AudioManager.BUTTON_CLICK_HASH);
            Time.timeScale = 1;
            AdsManager.Adinstance.ShowIntertitialAD();
            GameController.LoadMainMenu();
        }
    }
}