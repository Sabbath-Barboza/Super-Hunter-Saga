using OctoberStudio.Audio;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OctoberStudio.UI
{
    public class CharacterItemBehavior : MonoBehaviour
    {
        [Header("Info")]
        [SerializeField] Image iconImage;
        [SerializeField] TMP_Text titleLabel;

        [Header("Button")]
        [SerializeField] Button upgradeButton;
        [SerializeField] Sprite enabledButtonSprite;
        [SerializeField] Sprite disabledButtonSprite;
        [SerializeField] Sprite selectedButtonSprite;

        [Header("Stats")]
        [SerializeField] TMP_Text hpText;
        [SerializeField] TMP_Text damageText;

        [Space]
        [SerializeField] ScalingLabelBehavior costLabel;
        [SerializeField] TMP_Text buttonText;

        public CurrencySave GoldCurrency { get; private set; }
        private CharactersSave charactersSave;

        public CharacterData Data { get; private set; }
        public int CharacterId { get; private set; }

        private void Start()
        {
            upgradeButton.onClick.AddListener(SelectButtonClick);
        }

        public void Init(int id, CharacterData characterData)
        {
            if(charactersSave == null)
            {
                charactersSave = GameController.SaveManager.GetSave<CharactersSave>("Characters");
                charactersSave.onSelectedCharacterChanged += RedrawVisuals;
            }

            if (GoldCurrency == null)
            {
                GoldCurrency = GameController.SaveManager.GetSave<CurrencySave>("gold");
                GoldCurrency.OnGoldAmountChanged += OnGoldAmountChanged;
            }

            Data = characterData;
            CharacterId = id;

            RedrawVisuals();
        }

        private void RedrawVisuals()
        {
            titleLabel.text = Data.Name;
            iconImage.sprite = Data.Icon;

            hpText.text = Data.BaseHP.ToString();
            damageText.text = Data.BaseDamage.ToString();

            RedrawButton();
        }

        private void RedrawButton()
        {
            if (charactersSave.HasCharacterBeenBought(CharacterId))
            {
                costLabel.gameObject.SetActive(false);
                buttonText.gameObject.SetActive(true);

                if(charactersSave.SelectedCharacterId == CharacterId)
                {
                    upgradeButton.enabled = false;
                    upgradeButton.image.sprite = selectedButtonSprite;

                    buttonText.text = "SELECTED";

                } else
                {
                    upgradeButton.enabled = true;
                    upgradeButton.image.sprite = enabledButtonSprite;

                    buttonText.text = "SELECT";
                }
            }
            else
            {
                costLabel.gameObject.SetActive(true);
                buttonText.gameObject.SetActive(false);

                costLabel.SetAmount(Data.Cost);

                if (GoldCurrency.CanAfford(Data.Cost))
                {
                    upgradeButton.enabled = true;
                    upgradeButton.image.sprite = enabledButtonSprite;
                }
                else
                {
                    upgradeButton.enabled = false;
                    upgradeButton.image.sprite = disabledButtonSprite;
                }
            }
        }

        private void SelectButtonClick()
        {
            if (!charactersSave.HasCharacterBeenBought(CharacterId))
            {
                GoldCurrency.Withdraw(Data.Cost);
                charactersSave.AddBoughtCharacter(CharacterId);
            }

            charactersSave.SetSelectedCharacterId(CharacterId);

            GameController.AudioManager.PlaySound(AudioManager.BUTTON_CLICK_HASH);
        }

        private void OnGoldAmountChanged(int amount)
        {
            RedrawButton();
        }

        private void OnDestroy()
        {
            if (GoldCurrency != null)
            {
                GoldCurrency.OnGoldAmountChanged -= OnGoldAmountChanged;
            }

            if(charactersSave != null)
            {
                charactersSave.onSelectedCharacterChanged -= RedrawVisuals;
            }
        }
    }
}