using OctoberStudio.Extensions;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace OctoberStudio.UI
{
    public class CharactersWindowBehavior : MonoBehaviour
    {
        private Canvas canvas;

        [SerializeField] CharactersDatabase database;

        [Space]
        [SerializeField] GameObject itemPrefab;
        [SerializeField] RectTransform itemsParent;

        [Space]
        [SerializeField] Button backButton;

        private CharactersSave charactersSave;

        private List<CharacterItemBehavior> items = new List<CharacterItemBehavior>();

        private void Awake()
        {
            canvas = GetComponent<Canvas>();
        }

        public void Init(UnityAction onBackButtonClicked)
        {
            charactersSave = GameController.SaveManager.GetSave<CharactersSave>("Characters");

            backButton.onClick.AddListener(onBackButtonClicked);

            for (int i = 0; i < database.CharactersCount; i++)
            {
                var item = Instantiate(itemPrefab, itemsParent).GetComponent<CharacterItemBehavior>();
                item.transform.ResetLocal();

                item.Init(i, database.GetCharacterData(i));

                items.Add(item);
            }
        }

        public void Open()
        {
            canvas.enabled = true;
        }

        public void Close()
        {
            canvas.enabled = false;
        }
    }
}