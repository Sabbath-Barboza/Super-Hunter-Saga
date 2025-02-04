using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Upgrades
{
    [CreateAssetMenu(fileName = "Upgrades", menuName = "October/Upgrades/Upgrade")]
    public class UpgradeData : ScriptableObject
    {
        [SerializeField] UpgradeType upgradeType;
        [SerializeField] Sprite icon;
        [SerializeField] string title;
        [SerializeField] int devStartLevel = 0;

        public UpgradeType UpgradeType => upgradeType;
        public Sprite Icon => icon;
        public string Title => title;

        public int DevStartLevel => devStartLevel;

        [SerializeField] List<UpgradeLevel> levels;

        public int LevelsCount => levels.Count;

        public UpgradeLevel GetLevel(int id)
        {
            return levels[id];
        }
    }

    [System.Serializable] 
    public class UpgradeLevel
    {
        [SerializeField] int cost;
        [SerializeField] float value;

        public int Cost => cost;
        public float Value => value;
    }
}