namespace OctoberStudio.Abilities
{
    public class MaxHPAbilityBehavior : AbilityBehavior<MaxHPAbilityData, MaxHPAbilityLevel>
    {
        protected override void SetAbilityLevel(int stageId)
        {
            base.SetAbilityLevel(stageId);

            PlayerBehavior.Player.RecalculateMaxHP(AbilityLevel.MaxHPMultiplier);
        }
    }
}