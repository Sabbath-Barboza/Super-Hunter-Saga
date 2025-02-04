using OctoberStudio.Extensions;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace OctoberStudio.Upgrades.UI
{
    public class UpgradesWindowBehavior : MonoBehaviour
    {
        private Canvas canvas;

        [SerializeField] UpgradesDatabase database;

        [Space]
        [SerializeField] GameObject itemPrefab;
        [SerializeField] RectTransform itemsParent;

        [Space]
        [SerializeField] Button backButton;

        private List<UpgradeItemBehavior> items = new List<UpgradeItemBehavior>();

        private void Awake()
        {
            canvas = GetComponent<Canvas>();
        }

        public void Init(UnityAction onBackButtonClicked)
        {
            backButton.onClick.AddListener(onBackButtonClicked);

            for(int i = 0; i < database.UpgradesCount; i++)
            {
                var upgrade = database.GetUpgrade(i);

                var item = Instantiate(itemPrefab, itemsParent).GetComponent<UpgradeItemBehavior>();
                item.transform.ResetLocal();

                var level = GameController.UpgradesManager.GetUpgradeLevel(upgrade.UpgradeType);

                item.Init(upgrade, level + 1);

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