namespace OctoberStudio.Abilities
{
    public class SportShoesAbilityBehavior : AbilityBehavior<MoveSpeedAbilityData, MoveSpeedAbilityLevel>
    {
        protected override void SetAbilityLevel(int stageId)
        {
            base.SetAbilityLevel(stageId);

            PlayerBehavior.Player.RecalculateMoveSpeed(AbilityLevel.SpeedMultiplier);
        }
    }
}