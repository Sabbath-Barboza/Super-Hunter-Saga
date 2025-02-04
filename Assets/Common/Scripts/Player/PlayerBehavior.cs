using OctoberStudio.Easing;
using OctoberStudio.Extensions;
using OctoberStudio.UI;
using OctoberStudio.Upgrades;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;

namespace OctoberStudio
{
    public class PlayerBehavior : MonoBehaviour
    {
        private static readonly int DEATH_HASH = "Death".GetHashCode();
        private static readonly int REVIVE_HASH = "Revive".GetHashCode();
        private static readonly int RECEIVING_DAMAGE_HASH = "Receiving Damage".GetHashCode();

        private static PlayerBehavior instance;
        public static PlayerBehavior Player => instance;

        [SerializeField] CharactersDatabase charactersDatabase;

        [Header("Stats")]
        [SerializeField, Min(0.01f)] float speed = 2;
        [SerializeField, Min(0.1f)] float defaultMagnetRadius = 0.75f;
        [SerializeField, Min(1f)] float xpMultiplier = 1;
        [SerializeField, Range(0.1f, 1f)] float cooldownMultiplier = 1;
        [SerializeField, Range(0, 100)] int initialDamageReductionPercent = 0;
        [SerializeField, Min(1f)] float initialProjectileSpeedMultiplier = 1;
        [SerializeField, Min(1f)] float initialSizeMultiplier = 1f;
        [SerializeField, Min(1f)] float initialDurationMultiplier = 1f;
        [SerializeField, Min(1f)] float initialGoldMultiplier = 1;

        [Header("References")]
        [SerializeField] JoystickBehavior joystick;
        [SerializeField] HealthbarBehavior healthbar;
        [SerializeField] Transform centerPoint;

        public static Transform CenterTransform => instance.centerPoint;
        public static Vector2 CenterPosition => instance.centerPoint.position;

        [Header("Death and Revive")]
        [SerializeField] ParticleSystem reviveParticle;

        [Space]
        [SerializeField] SpriteRenderer reviveBackgroundSpriteRenderer;
        [SerializeField, Range(0, 1)] float reviveBackgroundAlpha;
        [SerializeField, Range(0, 1)] float reviveBackgroundSpawnDelay;
        [SerializeField, Range(0, 1)] float reviveBackgroundHideDelay;

        [Space]
        [SerializeField] SpriteRenderer reviveBottomSpriteRenderer;
        [SerializeField, Range(0, 1)] float reviveBottomAlpha;
        [SerializeField, Range(0, 1)] float reviveBottomSpawnDelay;
        [SerializeField, Range(0, 1)] float reviveBottomHideDelay;

        [Header("Other")]
        [SerializeField] Vector2 fenceOffset;
        [SerializeField] Color hitColor;
        [SerializeField] float enemyInsideDamageInterval = 2f;

        public event UnityAction onPlayerDied;

        public float Damage { get; private set; }
        public float MagnetRadiusSqr { get; private set; }
        public float Speed { get; private set; }

        public float XPMultiplier { get; private set; }
        public float CooldownMultiplier { get; private set; }
        public float DamageReductionMultiplier { get; private set; }
        public float ProjectileSpeedMultiplier { get; private set; }
        public float SizeMultiplier { get; private set; }
        public float DurationMultiplier { get; private set; }
        public float GoldMultiplier { get; private set; }

        public Vector2 LookDirection { get; private set; }
        public bool IsMovingAlowed { get; set; }

        private bool invincible = false;

        private List<EnemyBehavior> enemiesInside = new List<EnemyBehavior>();

        private CharactersSave charactersSave;
        private CharacterData Data { get; set; }
        private CharacterBehavior Character { get; set; }

        private void Awake()
        {
            charactersSave = GameController.SaveManager.GetSave<CharactersSave>("Characters");
            Data = charactersDatabase.GetCharacterData(charactersSave.SelectedCharacterId);

            Character = Instantiate(Data.Prefab).GetComponent<CharacterBehavior>();
            Character.transform.SetParent(transform);
            Character.transform.ResetLocal();

            instance = this;
            healthbar.Init(Data.BaseHP);
            healthbar.SetAutoHideWhenMax(true);
            healthbar.SetAutoShowOnChanged(true);

            RecalculateMagnetRadius(1);
            RecalculateMoveSpeed(1);
            RecalculateDamage(1);
            RecalculateMaxHP(1);
            RecalculateXPMuliplier(1);
            RecalculateCooldownMuliplier(1);
            RecalculateDamageReduction(0);
            RecalculateProjectileSpeedMultiplier(1f);
            RecalculateSizeMultiplier(1f);
            RecalculateDurationMultiplier(1);
            RecalculateGoldMultiplier(1);

            LookDirection = Vector2.right;

            IsMovingAlowed = true;
        }

        private void Update()
        {
            if (healthbar.IsZero) return;

            foreach(var enemy in enemiesInside)
            {
                if (Time.time - enemy.LastTimeDamagedPlayer > enemyInsideDamageInterval)
                {
                    TakeDamage(enemy.GetDamage());
                    enemy.LastTimeDamagedPlayer = Time.time;
                }
            }

            if (!IsMovingAlowed) return;

            float joysticPower = joystick.Value.magnitude;
            Character.SetSpeed(joysticPower);

            if (!Mathf.Approximately(joysticPower, 0))
            {
                var frameMovement = joystick.Value * Time.deltaTime * Speed;

                if (StageController.FieldManager.ValidatePosition(transform.position + Vector3.right * frameMovement.x, fenceOffset))
                {
                    transform.position += Vector3.right * frameMovement.x;
                }

                if (StageController.FieldManager.ValidatePosition(transform.position + Vector3.up * frameMovement.y, fenceOffset))
                {
                    transform.position += Vector3.up * frameMovement.y;
                }
                Character.SetLocalScale(new Vector3(joystick.Value.x > 0 ? 1 : -1, 1, 1));

                LookDirection = joystick.Value.normalized;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsInsideMagnetRadius(Transform target)
        {
            return (transform.position - target.position).sqrMagnitude <= MagnetRadiusSqr;
        }

        public void RecalculateMagnetRadius(float magnetRadiusMultiplier)
        {
            MagnetRadiusSqr = Mathf.Pow(defaultMagnetRadius * magnetRadiusMultiplier, 2);
        }

        public void RecalculateMoveSpeed(float moveSpeedMultiplier)
        {
            Speed = speed * moveSpeedMultiplier;
        }

        public void RecalculateDamage(float damageMultiplier)
        {
            Damage = Data.BaseDamage * damageMultiplier;
            if (GameController.UpgradesManager.IsUpgradeAquired(UpgradeType.Damage))
            {
                Damage *= GameController.UpgradesManager.GetUpgadeValue(UpgradeType.Damage);
            }
        }

        public void RecalculateMaxHP(float maxHPMultiplier)
        {
            var upgradeValue = GameController.UpgradesManager.GetUpgadeValue(UpgradeType.Health);
            healthbar.ChangeMaxHP((Data.BaseHP + upgradeValue) * maxHPMultiplier);
        }

        public void RecalculateXPMuliplier(float xpMultiplier)
        {
            XPMultiplier = this.xpMultiplier * xpMultiplier;
        }

        public void RecalculateCooldownMuliplier(float cooldownMultiplier)
        {
            CooldownMultiplier = this.cooldownMultiplier * cooldownMultiplier;
        }

        public void RecalculateDamageReduction(float damageReductionPercent)
        {
            DamageReductionMultiplier = (100f - initialDamageReductionPercent - damageReductionPercent) / 100f;

            if (GameController.UpgradesManager.IsUpgradeAquired(UpgradeType.Armor))
            {
                DamageReductionMultiplier *= GameController.UpgradesManager.GetUpgadeValue(UpgradeType.Armor);
            } 
        }

        public void RecalculateProjectileSpeedMultiplier(float projectileSpeedMultiplier)
        {
            ProjectileSpeedMultiplier = initialProjectileSpeedMultiplier * projectileSpeedMultiplier;
        }

        public void RecalculateSizeMultiplier(float sizeMultiplier)
        {
            SizeMultiplier = initialSizeMultiplier * sizeMultiplier;
        }

        public void RecalculateDurationMultiplier(float durationMultiplier)
        {
            DurationMultiplier = initialDurationMultiplier * durationMultiplier;
        }

        public void RecalculateGoldMultiplier(float goldMultiplier)
        {
            GoldMultiplier = initialGoldMultiplier * goldMultiplier;
        }

        public void RestoreHP(float hpPercent)
        {
            healthbar.AddPercentage(hpPercent);
        }

        public void Heal(float hp)
        {
            healthbar.AddHP(hp + GameController.UpgradesManager.GetUpgadeValue(UpgradeType.Healing));
        }

        public void Revive()
        {
            Character.PlayReviveAnimation();
            reviveParticle.Play();

            invincible = true;
            IsMovingAlowed = false;
            healthbar.ResetHP(1f);

            Character.SetSortingOrder(102);

            reviveBackgroundSpriteRenderer.DoAlpha(0f, 0.3f, reviveBottomHideDelay).SetUnscaledTime(true).SetOnFinish(() => reviveBackgroundSpriteRenderer.gameObject.SetActive(false));
            reviveBottomSpriteRenderer.DoAlpha(0f, 0.3f, reviveBottomHideDelay).SetUnscaledTime(true).SetOnFinish(() => reviveBottomSpriteRenderer.gameObject.SetActive(false));

            GameController.AudioManager.PlaySound(REVIVE_HASH);
            EasingManager.DoAfter(1f, () =>
            {
                IsMovingAlowed = true;
                Character.SetSortingOrder(0);
            });

            EasingManager.DoAfter(3, () => invincible = false);
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.layer == 7)
            {
                if (invincible) return;

                var enemy = collision.GetComponent<EnemyBehavior>();

                if (enemy != null)
                {
                    enemiesInside.Add(enemy);
                    enemy.LastTimeDamagedPlayer = Time.time;

                    enemy.onEnemyDied += OnEnemyDied;
                    TakeDamage(enemy.GetDamage());
                }
            }
            else
            {
                if (invincible) return;

                var projectile = collision.GetComponent<SimpleEnemyProjectileBehavior>();
                if (projectile != null)
                {
                    TakeDamage(projectile.Damage);
                }
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.gameObject.layer == 7)
            {
                if (invincible) return;

                var enemy = collision.GetComponent<EnemyBehavior>();

                if (enemy != null)
                {
                    enemiesInside.Remove(enemy);
                    enemy.onEnemyDied -= OnEnemyDied;
                }
            }
        }

        private void OnEnemyDied(EnemyBehavior enemy)
        {
            enemy.onEnemyDied -= OnEnemyDied;
            enemiesInside.Remove(enemy);
        }

        private float lastTimeVibrated = 0f;

        public void TakeDamage(float damage)
        {
            if (invincible || healthbar.IsZero) return;

            healthbar.Subtract(damage * DamageReductionMultiplier);

            Character.FlashHit();

            if (healthbar.IsZero)
            {
                Character.PlayDefeatAnimation();
                Character.SetSortingOrder(102);

                reviveBackgroundSpriteRenderer.gameObject.SetActive(true);
                reviveBackgroundSpriteRenderer.DoAlpha(reviveBackgroundAlpha, 0.3f, reviveBackgroundSpawnDelay).SetUnscaledTime(true);
                reviveBackgroundSpriteRenderer.transform.position = transform.position.SetZ(reviveBackgroundSpriteRenderer.transform.position.z);

                reviveBottomSpriteRenderer.gameObject.SetActive(true);
                reviveBottomSpriteRenderer.DoAlpha(reviveBottomAlpha, 0.3f, reviveBottomSpawnDelay).SetUnscaledTime(true);

                GameController.AudioManager.PlaySound(DEATH_HASH);

                EasingManager.DoAfter(0.5f, () =>
                {
                    onPlayerDied?.Invoke();
                }).SetUnscaledTime(true);

                GameController.VibrationManager.StrongVibration();
            } else
            {
                if(Time.time - lastTimeVibrated > 0.05f)
                {
                    GameController.VibrationManager.LightVibration();
                    lastTimeVibrated = Time.time;
                }
                
                GameController.AudioManager.PlaySound(RECEIVING_DAMAGE_HASH);
            }
        }
    }
}