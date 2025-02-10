using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio
{
    [System.Serializable]
    public class CharacterData
    {
        [SerializeField] string name;
        public string Name => name;

        [SerializeField] int cost;
        public int Cost => cost;

        [SerializeField] Sprite icon;
        public Sprite Icon => icon;

        [SerializeField] GameObject prefab;
        public GameObject Prefab => prefab;

        [Space]
        [SerializeField] bool hasStartingAbility = false;
        public bool HasStartingAbility => hasStartingAbility;

        [SerializeField] AbilityType startingAbility;
        public AbilityType StartingAbility => startingAbility;

        [Space]
        [SerializeField, Min(1)] float baseHP;
        public float BaseHP => baseHP;

        [SerializeField, Min(1f)] float baseDamage;
        public float BaseDamage => baseDamage;
    }
}