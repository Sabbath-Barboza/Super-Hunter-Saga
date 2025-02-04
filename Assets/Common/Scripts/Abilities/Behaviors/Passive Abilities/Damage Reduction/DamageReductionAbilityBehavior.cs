namespace OctoberStudio.Abilities
{
    public class DamageReductionAbilityBehavior : AbilityBehavior<DamageReductionAbilityData, DamageReductionAbilityLevel>
    {
        protected override void SetAbilityLevel(int stageId)
        {
            base.SetAbilityLevel(stageId);

            PlayerBehavior.Player.RecalculateDamageReduction(AbilityLevel.DamageReductionPercent);
        }
    }
}