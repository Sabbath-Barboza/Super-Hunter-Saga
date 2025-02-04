using OctoberStudio.Audio;
using OctoberStudio.Easing;
using OctoberStudio.Save;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace OctoberStudio.UI
{
    public class LobbyWindowBehavior : MonoBehaviour
    {
        [SerializeField] StagesDatabase stagesDatabase;

        [Space]
        [SerializeField] Image stageIcon;
        [SerializeField] Image lockImage;
        [SerializeField] TMP_Text stageLabel;
        [SerializeField] TMP_Text stageNumberLabel;

        [Space]
        [SerializeField] Button playButton;
        [SerializeField] Button upgradesButton;
        [SerializeField] Button settingsButton;
        [SerializeField] Button charactersButton;
        [SerializeField] Button leftButton;
        [SerializeField] Button rightButton;

        [Space]
        [SerializeField] Sprite playButtonEnabledSprite;
        [SerializeField] Sprite playButtonDisabledSprite;

        [Space]
        [SerializeField] Image continueBackgroundImage;
        [SerializeField] RectTransform contituePopupRect;
        [SerializeField] Button confirmButton;
        [SerializeField] Button cancelButton;

        private Canvas canvas;

        private StageSave save;

        private int previousSavedId;

        private void Awake()
        {
            canvas = GetComponent<Canvas>();

            playButton.onClick.AddListener(OnPlayButtonClicked);
            leftButton.onClick.AddListener(DecrementSelectedStageId);
            rightButton.onClick.AddListener(IncremenSelectedStageId);

            confirmButton.onClick.AddListener(ConfirmButtonClicked);
            cancelButton.onClick.AddListener(CancelButtonClicked);
        }

        private void Start()
        {
            save = GameController.SaveManager.GetSave<StageSave>("Stage");

            save.onSelectedStageChanged += InitStage;

            if (save.IsPlaying && GameController.FirstTimeLoaded)
            {
                previousSavedId = save.SelectedStageId;
                continueBackgroundImage.gameObject.SetActive(true);

                contituePopupRect.gameObject.SetActive(true);
            }

            save.SetSelectedStageId(save.MaxReachedStageId);
        }

        public void Init(UnityAction onUpgradesButtonClicked, UnityAction onSettingsButtonClicked, UnityAction onCharactersButtonClicked)
        {
            upgradesButton.onClick.AddListener(onUpgradesButtonClicked);
            settingsButton.onClick.AddListener(onSettingsButtonClicked);
            charactersButton.onClick.AddListener(onCharactersButtonClicked);
        }

        public void InitStage(int stageId)
        {
            var stage = stagesDatabase.GetStage(stageId);

            stageLabel.text = stage.DisplayName;
            stageNumberLabel.text = $"Stage {stageId + 1}";
            stageIcon.sprite = stage.Icon;

            if(save.SelectedStageId > save.MaxReachedStageId)
            {
                lockImage.gameObject.SetActive(true);
                playButton.enabled = false;
                playButton.image.sprite = playButtonDisabledSprite;
            } else
            {
                lockImage.gameObject.SetActive(false);
                playButton.enabled = true;
                playButton.image.sprite = playButtonEnabledSprite;
            }

            leftButton.gameObject.SetActive(!save.IsFirstStageSelected);
            rightButton.gameObject.SetActive(save.SelectedStageId != stagesDatabase.StagesCount - 1);
        }

        public void Open()
        {
            canvas.enabled = true;
        }

        public void Close()
        {
            canvas.enabled = false;
        }

        public void OnPlayButtonClicked()
        {
            save.IsPlaying = true;
            save.ResetStageData = true;
            save.Time = 0f;

            GameController.AudioManager.PlaySound(AudioManager.BUTTON_CLICK_HASH);
            GameController.LoadStage();
        }

        private void IncremenSelectedStageId()
        {
            GameController.AudioManager.PlaySound(AudioManager.BUTTON_CLICK_HASH);
            save.SetSelectedStageId(save.SelectedStageId + 1);
        }

        private void DecrementSelectedStageId()
        {
            GameController.AudioManager.PlaySound(AudioManager.BUTTON_CLICK_HASH);
            save.SetSelectedStageId(save.SelectedStageId - 1);
        }

        private void OnDestroy()
        {
            save.onSelectedStageChanged -= InitStage;
        }

        private void ConfirmButtonClicked()
        {
            save.ResetStageData = false;

            GameController.AudioManager.PlaySound(AudioManager.BUTTON_CLICK_HASH);
            GameController.LoadStage();
        }

        private void CancelButtonClicked()
        {
            save.IsPlaying = false;

            continueBackgroundImage.DoAlpha(0, 0.3f).SetOnFinish(() => continueBackgroundImage.gameObject.SetActive(false));
            contituePopupRect.DoAnchorPosition(Vector2.down * 2500, 0.3f).SetEasing(EasingType.SineIn).SetOnFinish(() => contituePopupRect.gameObject.SetActive(false));
        }
    }
}