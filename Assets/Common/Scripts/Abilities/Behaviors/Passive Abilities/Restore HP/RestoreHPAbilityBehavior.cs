using OctoberStudio.Abilities;
using System.Collections;
using UnityEngine;

namespace OctoberStudio
{
    public class RestoreHPAbilityBehavior : AbilityBehavior<RestoreHPAbilityData, RestoreHPAbilityLevel>
    {
        public override void Init(AbilityData data, int stageId)
        {
            base.Init(data, stageId);

            StartCoroutine(AbilityCoroutine());
        }

        protected override void SetAbilityLevel(int stageId)
        {
            base.SetAbilityLevel(stageId);
        }

        private IEnumerator AbilityCoroutine()
        {
            while (true)
            {
                PlayerBehavior.Player.RestoreHP(AbilityLevel.RestoredHPPercent);

                yield return new WaitForSeconds(AbilityLevel.Cooldown * PlayerBehavior.Player.CooldownMultiplier);
            }
        }
    }
}