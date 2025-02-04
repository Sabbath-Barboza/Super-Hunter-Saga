namespace OctoberStudio.Abilities
{
    public class MagnetAbilityBehavior : AbilityBehavior<MagnetAbilityData, MagnetAbilityLevel>
    {
        protected override void SetAbilityLevel(int stageId)
        {
            base.SetAbilityLevel(stageId);

            PlayerBehavior.Player.RecalculateMagnetRadius(AbilityLevel.RadiusMultiplier);
        }
    }
}