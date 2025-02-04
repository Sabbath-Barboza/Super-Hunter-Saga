using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio
{
    [CreateAssetMenu(fileName = "Characters Database", menuName = "October/Characters Database")]
    public class CharactersDatabase : ScriptableObject
    {
        [SerializeField] List<CharacterData> characters = new List<CharacterData>();
        public int CharactersCount => characters.Count;
        
        public CharacterData GetCharacterData(int index)
        {
            return characters[index];
        }
    }
}