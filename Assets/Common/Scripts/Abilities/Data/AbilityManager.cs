using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Abilities
{
    using OctoberStudio.Easing;
    using OctoberStudio.Extensions;
    using OctoberStudio.UI;
    using UI;

    public class AbilityManager : MonoBehaviour
    {
        [SerializeField] AbilitiesDatabase abilitiesDatabase;

        [Space]
        [SerializeField, Range(0, 1)] float chestChanceTier5;
        [SerializeField, Range(0, 1)] float chestChanceTier3;

        private List<IAbilityBehavior> aquiredAbilities = new List<IAbilityBehavior>();
        private List<AbilityType> removedAbilities = new List<AbilityType>();
        private AbilitiesSave save;
        private StageSave stageSave;

        public int ActiveAbilitiesCapacity => abilitiesDatabase.ActiveAbilitiesCapacity;
        public int PassiveAbilitiesCapacity => abilitiesDatabase.PassiveAbilitiesCapacity;

        private void Awake()
        {
            save = GameController.SaveManager.GetSave<AbilitiesSave>("Abilities Save");
            save.Init();

            stageSave = GameController.SaveManager.GetSave<StageSave>("Stage");
            if(stageSave.ResetStageData) save.Clear();
        }

        public void Init(PresetData testingPreset, CharacterData characterData)
        {
            StageController.ExperienceManager.onXpLevelChanged += OnXpLevelChanged;

            if(testingPreset != null)
            {
                for (int i = 0; testingPreset.Abilities.Count > i; i++)
                {
                    AbilityType type = testingPreset.Abilities[i].abilityType;

                    AbilityData data = abilitiesDatabase.GetAbility(type);
                    AddAbility(data, testingPreset.Abilities[i].level);
                }
            } else if (!stageSave.ResetStageData)
            {
                var savedAbilities = save.GetSavedAbilities();
                for(int i = 0; i < savedAbilities.Count; i++)
                {
                    AbilityType type = savedAbilities[i];

                    AbilityData data = abilitiesDatabase.GetAbility(type);
                    AddAbility(data, save.GetAbilityLevel(type));
                }

                if(savedAbilities.Count == 0)
                {
                    if (characterData.HasStartingAbility)
                    {
                        AbilityData data = abilitiesDatabase.GetAbility(characterData.StartingAbility);
                        AddAbility(data, 0);
                    } else
                    {
                        EasingManager.DoAfter(0.3f, ShowWeaponSelectScreen);
                    }
                    
                }
            } else if (characterData.HasStartingAbility)
            {
                AbilityData data = abilitiesDatabase.GetAbility(characterData.StartingAbility);
                AddAbility(data, 0);
            }
            else
            {
                EasingManager.DoAfter(0.3f, ShowWeaponSelectScreen);
            }
        }

        public void AddAbility(AbilityData abilityData, int level = 0)
        {
            IAbilityBehavior ability = Instantiate(abilityData.Prefab).GetComponent<IAbilityBehavior>();
            ability.Init(abilityData, level);

            if (abilityData.IsEvolution)
            {
                for (int i = 0; i < abilityData.EvolutionRequirements.Count; i++)
                {
                    var requirement = abilityData.EvolutionRequirements[i];

                    if (requirement.ShouldRemoveAfterEvolution)
                    {
                        var requiredAbility = GetAquiredAbility(requirement.AbilityType);

                        if (requiredAbility != null)
                        {
                            requiredAbility.Clear();

                            aquiredAbilities.Remove(requiredAbility);
                            save.RemoveAbility(requiredAbility.AbilityType);

                            removedAbilities.Add(requiredAbility.AbilityType);
                        }
                    }
                }
            }

            save.SetAbilityLevel(abilityData.AbilityType, level);
            aquiredAbilities.Add(ability);
        }

        public int GetActiveAbilitiesCount()
        {
            int counter = 0;
            foreach(var ability in aquiredAbilities)
            {
                if (ability.AbilityData.IsActiveAbility) counter++;
            }
            return counter;
        }

        public int GetPassiveAbilitiesCount()
        {
            int counter = 0;
            foreach (var ability in aquiredAbilities)
            {
                if (!ability.AbilityData.IsActiveAbility) counter++;
            }
            return counter;
        }

        private void ShowWeaponSelectScreen()
        {
            var weaponAbilities = new List<AbilityData>();

            for (int i = 0; i < abilitiesDatabase.AbilitiesCount; i++)
            {
                var abilityData = abilitiesDatabase.GetAbility(i);

                if (abilityData.IsWeaponAbility && !abilityData.IsEvolution)
                {
                    weaponAbilities.Add(abilityData);
                }
            }

            var selectedAbilities = new List<AbilityData>();

            while (weaponAbilities.Count > 0 && selectedAbilities.Count < 3)
            {
                var abilityData = weaponAbilities.PopRandom();
                selectedAbilities.Add(abilityData);
            }

            if (selectedAbilities.Count > 0)
            {
                StageController.GameScreen.ShowAbilitiesPanel(selectedAbilities, false);
            }
        }

        private void OnXpLevelChanged(int level)
        {
            var abilities = GetAvailableAbilities();
            var selectedAbilities = new List<AbilityData>();

            var weightedAbilities = new List<WeightedAbility>();

            bool firstLevels = level < 10;

            var activeCount = GetActiveAbilitiesCount();
            var passiveCount = GetPassiveAbilitiesCount();

            bool moreActive = activeCount > passiveCount;
            bool morePassive = passiveCount > activeCount;

            foreach (var ability in abilities)
            {
                var weight = 1f;

                if (IsAbilityAquired(ability.AbilityType)) weight *= abilitiesDatabase.AquiredAbilityWeightMultiplier;

                if (ability.IsActiveAbility)
                {
                    if (firstLevels) weight *= abilitiesDatabase.FirstLevelsActiveAbilityWeightMultiplier;
                    if (morePassive) weight *= abilitiesDatabase.LessAbilitiesOfTypeWeightMultiplier;
                    if (ability.IsEvolution) weight *= abilitiesDatabase.EvolutionAbilityWeightMultiplier;
                } else
                {
                    if (IsRequiredForAquiredEvolution(ability.AbilityType)) weight *= abilitiesDatabase.RequiredForEvolutionWeightMultiplier;
                    if (moreActive) weight *= abilitiesDatabase.LessAbilitiesOfTypeWeightMultiplier;
                }

                weightedAbilities.Add(new WeightedAbility() { abilityData = ability, weight = weight });
            }

            while (abilities.Count > 0 && selectedAbilities.Count < 3)
            {
                float weightSum = 0f;
                foreach(var container in weightedAbilities) weightSum += container.weight;

                foreach (var container in weightedAbilities) container.weight /= weightSum;

                float random = Random.value;
                float progress = 0;

                AbilityData selectedAbility = null;

                foreach (var container in weightedAbilities)
                {
                    progress += container.weight;

                    if(random <= progress)
                    {
                        selectedAbility = container.abilityData;
                        break;
                    }
                }

                if(selectedAbility != null)
                {
                    abilities.Remove(selectedAbility);
                } else
                {
                    selectedAbility = abilities.PopRandom();
                }

                foreach (var container in weightedAbilities)
                {
                    if(container.abilityData ==  selectedAbility)
                    {
                        weightedAbilities.Remove(container);
                        break;
                    }
                }

                selectedAbilities.Add(selectedAbility);
            }

            if(selectedAbilities.Count > 0)
            {
                StageController.GameScreen.ShowAbilitiesPanel(selectedAbilities, true);
            }
        }

        private List<AbilityData> GetAvailableAbilities()
        {
            var result = new List<AbilityData>();

            int activeAbilitiesCount = 0;
            int passiveAbilitiesCount = 0;

            for(int i = 0; i < aquiredAbilities.Count; i++)
            {
                var abilityBehavior = aquiredAbilities[i];
                var abilityData = abilitiesDatabase.GetAbility(abilityBehavior.AbilityType);

                if (abilityData.IsActiveAbility)
                {
                    activeAbilitiesCount++;
                } else
                {
                    passiveAbilitiesCount++;
                }
            }

            for (int i = 0; i < abilitiesDatabase.AbilitiesCount; i++)
            {
                var abilityData = abilitiesDatabase.GetAbility(i);

                if (abilityData.IsEndgameAbility) continue;

                // The ability is at it's last level. There are no way to upgrade it further
                if (save.GetAbilityLevel(abilityData.AbilityType) >= abilityData.LevelsCount - 1) continue;

                // The ability was evolved
                if (removedAbilities.Contains(abilityData.AbilityType)) continue;

                if (abilityData.IsEvolution)
                {
                    bool fulfilled = true;
                    for (int j = 0; j < abilityData.EvolutionRequirements.Count; j++)
                    {
                        var evolutionRequirements = abilityData.EvolutionRequirements[j];

                        var isRequiredAbilityAquired = IsAbilityAquired(evolutionRequirements.AbilityType);
                        var requiredAbilityReachedLevel = save.GetAbilityLevel(evolutionRequirements.AbilityType) >= evolutionRequirements.RequiredAbilityLevel;

                        if (!isRequiredAbilityAquired || !requiredAbilityReachedLevel)
                        {
                            fulfilled = false;
                            break;
                        }
                    }

                    if (!fulfilled) continue;
                } else
                {
                    var isAbilityAquired = IsAbilityAquired(abilityData.AbilityType);

                    // The player can only have one weapon ability at a time
                    if (abilityData.IsWeaponAbility && !isAbilityAquired) continue;

                    // There are no available active abilities slots left
                    if (abilityData.IsActiveAbility && activeAbilitiesCount >= abilitiesDatabase.ActiveAbilitiesCapacity && !isAbilityAquired) continue;

                    // There are no available passive abilities slots left
                    if (!abilityData.IsActiveAbility && passiveAbilitiesCount >= abilitiesDatabase.PassiveAbilitiesCapacity && !isAbilityAquired) continue;
                }

                result.Add(abilityData);
            }

            if (result.Count == 0)
            {
                for (int i = 0; i < abilitiesDatabase.AbilitiesCount; i++)
                {
                    var abilityData = abilitiesDatabase.GetAbility(i);

                    if (abilityData.IsEndgameAbility)
                    {
                        result.Add(abilityData);
                    }
                }
            }

            return result;
        }

        public int GetAbilityLevel(AbilityType abilityType)
        {
            return save.GetAbilityLevel(abilityType);
        }

        public IAbilityBehavior GetAquiredAbility(AbilityType abilityType)
        {
            for (int i = 0; i < aquiredAbilities.Count; i++)
            {
                if (aquiredAbilities[i].AbilityType == abilityType) return aquiredAbilities[i];
            }

            return null;
        }

        public bool IsAbilityAquired(AbilityType ability)
        {
            for (int i = 0; i < aquiredAbilities.Count; i++)
            {
                if (aquiredAbilities[i].AbilityType == ability) return true;
            }

            return false;
        }

        public bool IsRequiredForAquiredEvolution(AbilityType abilityType)
        {
            foreach(var ability in aquiredAbilities)
            {
                if (!ability.AbilityData.IsActiveAbility || ability.AbilityData.IsEvolution) continue;

                foreach(var requirement in ability.AbilityData.EvolutionRequirements)
                {
                    if (requirement.AbilityType == abilityType) return true;
                }
            }

            return false;
        }

        public bool HasEvolution(AbilityType abilityType, out AbilityType otherRequiredAbilityType)
        {
            otherRequiredAbilityType = abilityType;

            for (int i = 0; i < abilitiesDatabase.AbilitiesCount; i++)
            {
                var ability = abilitiesDatabase.GetAbility(i);

                if (!ability.IsEvolution) continue;

                for(int j = 0; j < ability.EvolutionRequirements.Count; j++)
                {
                    var requirement = ability.EvolutionRequirements[j];

                    if(requirement.AbilityType == abilityType)
                    {
                        for(int k = 0; k < ability.EvolutionRequirements.Count; k++)
                        {
                            if (k == j) continue;

                            otherRequiredAbilityType = ability.EvolutionRequirements[k].AbilityType;
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public bool HasAvailableAbilities()
        {
            var abilities = GetAvailableAbilities();

            return abilities.Count > 0;
        }

        public AbilityData GetAbilityData(AbilityType abilityType)
        {
            return abilitiesDatabase.GetAbility(abilityType);
        }

        public List<AbilityType> GetAquiredAbilityTypes()
        {
            var result = new List<AbilityType>();

            for(int i = 0; i < aquiredAbilities.Count; i++)
            {
                result.Add(aquiredAbilities[i].AbilityType);
            }

            return result;
        }

        public void ShowChest()
        {
            Time.timeScale = 0;

            var availableAbilities = GetAvailableAbilities();
            var dictionary = new Dictionary<AbilityData, int>();

            // Populating Dictionary <Ability, LevelsLeft>
            var counter = 0;
            foreach (var ability in availableAbilities)
            {
                int levelsLeft = ability.LevelsCount - save.GetAbilityLevel(ability.AbilityType) - 1;
                dictionary.Add(ability, levelsLeft);

                counter += levelsLeft;
            }

            // The chest could give 1, 3, or 5 abilities
            var selectedAbilitiesCount = 1;
            var tierId = 0;
            if(counter >= 5 && Random.value < chestChanceTier5)
            {
                selectedAbilitiesCount = 5;
                tierId = 2;
            } else if(counter >= 3 && Random.value < chestChanceTier3)
            {
                selectedAbilitiesCount = 3;
                tierId = 1;
            }

            int activeAbilitiesCount = GetActiveAbilitiesCount();
            int passiveAbilitiesCount = GetPassiveAbilitiesCount();

            // Randomly selecting abilities
            var selectedAbilities = new List<AbilityData>();
            for (int i = 0; i < selectedAbilitiesCount; i++)
            {
                // Getting random ability from dictionary
                var abilityPair = dictionary.Random();
                var ability = abilityPair.Key;

                dictionary[ability] -= 1;
                if (dictionary[ability] <= 0) dictionary.Remove(ability);

                // There is a possibility that we are reached the available capacity with this one ability that we have selected.
                // If the ability has been selected already or an evolution than it's ok 
                if (!selectedAbilities.Contains(ability) && !ability.IsEvolution)
                {
                    selectedAbilities.Add(ability);

                    // Only checking the new abilities
                    if (!IsAbilityAquired(ability.AbilityType))
                    {
                        var abilitiesToRemove = new List<AbilityData>();

                        if (ability.IsActiveAbility)
                        {
                            // There is a new active ability
                            activeAbilitiesCount++;

                            // We've reached the capacity for the active abilities
                            if(activeAbilitiesCount == ActiveAbilitiesCapacity)
                            {
                                foreach(var savedAbility in dictionary.Keys)
                                {
                                    // This one ability is no longer available for us to win from the chest
                                    if(savedAbility.IsActiveAbility && !IsAbilityAquired(savedAbility.AbilityType) && !selectedAbilities.Contains(savedAbility))
                                    {
                                        abilitiesToRemove.Add(savedAbility);
                                    }
                                }
                            }
                        } else
                        {
                            // There is a new passive ability
                            passiveAbilitiesCount++;

                            // We've reached the capacity for the active abilities
                            if (passiveAbilitiesCount == PassiveAbilitiesCapacity)
                            {
                                foreach (var savedAbility in dictionary.Keys)
                                {
                                    // This one ability is no longer available for us to win from the chest
                                    if (!savedAbility.IsActiveAbility && !IsAbilityAquired(savedAbility.AbilityType) && !selectedAbilities.Contains(savedAbility))
                                    {
                                        abilitiesToRemove.Add(savedAbility);
                                    }
                                }
                            }
                        }

                        foreach(var abilityToRemove in abilitiesToRemove)
                        {
                            dictionary.Remove(abilityToRemove);
                        }
                    }
                } else
                {
                    selectedAbilities.Add(ability);
                }

                if (dictionary.Count == 0) break;
            }

            // We might have removed some abilities in the previous step and there might not be enough abilities for selected chest tier
            while(selectedAbilities.Count < selectedAbilitiesCount)
            {
                tierId--;
                selectedAbilitiesCount -= 2;

                for(int i = selectedAbilitiesCount; i < selectedAbilities.Count; i++)
                {
                    selectedAbilities.RemoveAt(i);
                    i--;
                }
            }
            StageController.GameScreen.ShowChestWindow(tierId, availableAbilities, selectedAbilities);

            // Applying abilities
            foreach (var ability in selectedAbilities)
            {
                if (IsAbilityAquired(ability.AbilityType))
                {
                    var level = save.GetAbilityLevel(ability.AbilityType);

                    if (!ability.IsEndgameAbility) level++;

                    if (level < 0) level = 0;

                    save.SetAbilityLevel(ability.AbilityType, level);

                    ability.Upgrade(level);
                }
                else
                {
                    AddAbility(ability);
                }
            }
        }
    }

    [System.Serializable]
    public class AbilityDev
    {
        public AbilityType abilityType;
        public int level;
    }

    public class WeightedAbility
    {
        public AbilityData abilityData;
        public float weight;
    }
}