using OctoberStudio.Save;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace OctoberStudio
{
    public class CharactersSave : ISave
    {
        [SerializeField] int[] boughtCharacterIds;
        [SerializeField] int selectedCharacterId;

        public UnityAction onSelectedCharacterChanged;

        public int SelectedCharacterId => selectedCharacterId;

        private List<int> BoughtCharacterIds { get; set; }

        public void Init()
        {
            if (boughtCharacterIds == null)
            {
                boughtCharacterIds = new int[] { 0 };

                selectedCharacterId = 0;
            }
            BoughtCharacterIds = new List<int>(boughtCharacterIds);
        }

        public bool HasCharacterBeenBought(int id)
        {
            if (BoughtCharacterIds == null) Init();

            return BoughtCharacterIds.Contains(id);
        }

        public void AddBoughtCharacter(int id)
        {
            if (BoughtCharacterIds == null) Init();

            BoughtCharacterIds.Add(id);
        }

        public void SetSelectedCharacterId(int id)
        {
            if (BoughtCharacterIds == null) Init();

            selectedCharacterId = id;

            onSelectedCharacterChanged?.Invoke();
        }

        public void Flush()
        {
            if (BoughtCharacterIds == null) Init();

            boughtCharacterIds = BoughtCharacterIds.ToArray();
        }
    }
}