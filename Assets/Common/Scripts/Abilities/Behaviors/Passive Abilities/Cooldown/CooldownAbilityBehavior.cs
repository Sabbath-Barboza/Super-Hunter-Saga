namespace OctoberStudio.Abilities
{
    public class CooldownAbilityBehavior : AbilityBehavior<CooldownAbilityData, CooldownAbilityLevel>
    {
        protected override void SetAbilityLevel(int stageId)
        {
            base.SetAbilityLevel(stageId);

            PlayerBehavior.Player.RecalculateCooldownMuliplier(AbilityLevel.CooldownMultiplier);
        }
    }
}