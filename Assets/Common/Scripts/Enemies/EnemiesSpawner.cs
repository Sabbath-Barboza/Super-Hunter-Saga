using OctoberStudio.Bossfight;
using OctoberStudio.Extensions;
using OctoberStudio.Pool;
using OctoberStudio.Timeline;
using OctoberStudio.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

namespace OctoberStudio
{
    public class EnemiesSpawner : MonoBehaviour
    {
        private List<EnemyBehavior> enemies = new List<EnemyBehavior>();

        [SerializeField] EnemiesDatabase database;
        [SerializeField] BossfightDatabase bossfightDatabase;

        [Space]
        [SerializeField] ScalingLabelBehavior enemiesDiedLabel;

        [Space]
        [SerializeField] int enemiesCap = 500;

        private int enemiesDiedCounter;

        private Dictionary<EnemyType, PoolComponent<EnemyBehavior>> enemyPools;

        StageSave stageSave;

        // Were creating pools only for the enemies that are present in the Stage Timeline
        public void Init(PlayableDirector director)
        {
            stageSave = GameController.SaveManager.GetSave<StageSave>("Stage");

            Dictionary<EnemyType, int> enemiesOnLevel = new Dictionary<EnemyType, int>();

            var waves = director.GetAssets<WaveTrack, WaveAsset>();

            for(int i = 0; i < waves.Count; i++)
            {
                var wave = waves[i];
                var enemyType = wave.EnemyType;
                var enemiesCount = wave.EnemiesCount;

                if (enemiesOnLevel.ContainsKey(enemyType))
                {
                    if (enemiesOnLevel[enemyType] < enemiesCount)
                    {
                        enemiesOnLevel[enemyType] = enemiesCount;
                    }
                }
                else
                {
                    enemiesOnLevel.Add(enemyType, enemiesCount);
                }
            }

            var trackEnemies = new List<EnemyType>();
            foreach (var output in director.playableAsset.outputs)
            {
                if(output.sourceObject is WaveTrack waveTrack)
                {
                    if (!trackEnemies.Contains(waveTrack.EnemyType))
                    {
                        trackEnemies.Add(waveTrack.EnemyType);
                    }
                }
            }

            enemyPools = new Dictionary<EnemyType, PoolComponent<EnemyBehavior>>();

            foreach(var enemyType in enemiesOnLevel.Keys)
            {
                var data = database.GetEnemyData(enemyType);

                var amount = enemiesOnLevel[enemyType];
                if (amount > 100) amount = 100;
                if (amount < 0) amount = 1;

                var pool = new PoolComponent<EnemyBehavior>($"Enemy {enemyType}", data.Prefab, amount);

                enemyPools.Add(data.Type, pool);
            }

            foreach(var enemyType in trackEnemies)
            {
                if (!enemyPools.ContainsKey(enemyType))
                {
                    var data = database.GetEnemyData(enemyType);
                    var pool = new PoolComponent<EnemyBehavior>($"Enemy {enemyType}", data.Prefab, 1);

                    enemyPools.Add(data.Type, pool);
                }
            }

            enemiesDiedCounter = 0;
            if (!stageSave.ResetStageData)
            {
                enemiesDiedCounter = stageSave.EnemiesKilled;
            }
            
            enemiesDiedLabel.SetAmount(enemiesDiedCounter);
        }

        public EnemyBehavior GetClosestEnemy(Vector2 point)
        {
            EnemyBehavior closestEnemy = null;
            float closestDistance = float.MaxValue;

            for(int i = 0; i < enemies.Count; i++)
            {
                if (enemies[i] == null)
                {
                    enemies.RemoveAt(i);
                    i--;
                    continue;
                }

                float distance = (point - enemies[i].transform.position.XY()).sqrMagnitude;

                if(distance < closestDistance)
                {
                    closestEnemy = enemies[i];
                    closestDistance = distance;
                }
            }

            return closestEnemy;
        }

        public EnemyBehavior Spawn(EnemyType enemyType, Vector2 position, UnityAction<EnemyBehavior> onEnemyDiedCallback = null)
        {
            if (enemies.Count >= enemiesCap) return null;

            if(!enemyPools.ContainsKey(enemyType))
            {
                var data = database.GetEnemyData(enemyType);

                var pool = new PoolComponent<EnemyBehavior>($"Enemy {enemyType}", data.Prefab, 10);

                enemyPools.Add(data.Type, pool);
            }

            var enemy = enemyPools[enemyType].GetEntity();

            enemy.SetData(database.GetEnemyData(enemyType));

            enemy.transform.position = position;

            enemy.onEnemyDied += OnEnemyDied;
            if (onEnemyDiedCallback != null) enemy.onEnemyDied += onEnemyDiedCallback;

            enemy.Play();

            enemies.Add(enemy);

            return enemy;
        }

        public void Spawn(EnemyType type, WaveOverride waveOverride, bool circularSpawn = false, int amount = 1, UnityAction<EnemyBehavior> onEnemyDiedCallback = null)
        {
            for(int i = 0; i < amount; i++)
            {
                if (enemies.Count >= enemiesCap) return;

                var enemy = enemyPools[type].GetEntity();

                enemy.SetData(database.GetEnemyData(type));
                enemy.SetWaveOverride(waveOverride);

                var triesCount = 0;
                var maxTriesCount = 10;

                Vector3 position = Vector3.zero;
                bool foundPosition = false;

                while(triesCount < maxTriesCount)
                {
                    triesCount++;

                    if (circularSpawn)
                    {
                        float height = Camera.main.orthographicSize;
                        float width = height * Camera.main.aspect;
                        float diagonal = Mathf.Sqrt(width * width + height * height);
                        position = PlayerBehavior.Player.transform.position + Random.onUnitSphere.SetZ(0).normalized * diagonal * 1.05f;
                    } else
                    {
                        position = CameraManager.GetRandomPointOutsideCamera(0.5f);
                    }
                    
                    if(StageController.FieldManager.ValidatePosition(position, Vector2.zero))
                    {
                        foundPosition = true;
                        break;
                    }
                }

                if (!foundPosition)
                {
                    for (int j = 1; j < 10; j++)
                    {
                        var middlePosition = Vector3.Lerp(position, PlayerBehavior.Player.transform.position, 1 - j / 10f);

                        if (StageController.FieldManager.ValidatePosition(middlePosition, Vector2.zero))
                        {
                            foundPosition = true;
                            position = middlePosition;
                            break;
                        }
                    }
                }

                if (!foundPosition)
                {
                    position = StageController.FieldManager.GetRandomPositionOnBorder();
                }

                enemy.transform.position = position;

                enemy.onEnemyDied += OnEnemyDied;
                if (onEnemyDiedCallback != null) enemy.onEnemyDied += onEnemyDiedCallback;

                enemy.Play();

                enemies.Add(enemy);
            }
        }

        public EnemyBehavior GetRandomVisibleEnemy()
        {
            if (enemies.Count == 0) return null;

            // Trying to find random visible enemy 10 times
            for(int i = 0; i < 10; i++)
            {
                var randomIndex = Random.Range(0, enemies.Count);

                var enemy = enemies[randomIndex];

                if (enemy.IsVisible) return enemy;
            }

            for(int i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];

                if (enemy.IsVisible) return enemy;
            }

            return null;
        }

        public List<EnemyBehavior> GetEnemiesInRadius(Vector2 position, float radius)
        {
            var result = new List<EnemyBehavior>();

            float radiusSqr = radius * radius;

            for(int i = 0; i < enemies.Count; i++)
            {
                if ((enemies[i].transform.position.XY() - position).sqrMagnitude <= radiusSqr)
                {
                    result.Add(enemies[i]);
                }
            }

            return result;
        }

        public void KillEveryEnemy()
        {
            foreach(var enemy in enemies)
            {
                enemy.onEnemyDied -= OnEnemyDied;
                enemy.Kill();
            }

            enemiesDiedCounter += enemies.Count;
            stageSave.EnemiesKilled = enemiesDiedCounter;

            enemiesDiedLabel.SetAmount(enemiesDiedCounter);

            enemies.Clear();
        }

        public void DealDamageToAllEnemies(float damage)
        {
            var aliveEnemies = new List<EnemyBehavior>();

            foreach(var enemy in enemies)
            {
                if(enemy.HP <= damage)
                {
                    // if enemy is not a boss
                    if(enemy.Data != null)
                    {
                        enemy.onEnemyDied -= OnEnemyDied;
                        enemy.Kill();

                        foreach (var dropData in enemy.GetDropData())
                        {
                            if (dropData.Chance == 0) continue;

                            if (Random.value * 100 <= dropData.Chance && StageController.DropManager.CheckDropCooldown(dropData.DropType))
                            {
                                StageController.DropManager.Drop(dropData.DropType, enemy.transform.position.XY() + Random.insideUnitCircle * 0.2f);
                            }
                        }
                    } else
                    {
                        aliveEnemies.Add(enemy);
                    }
                }
                else
                {
                    // if enemy is not a boss
                    if (enemy.Data != null)
                    {
                        enemy.TakeDamage(damage);
                    }
                    aliveEnemies.Add(enemy);
                }
            }

            enemiesDiedCounter += enemies.Count - aliveEnemies.Count;

            stageSave.EnemiesKilled = enemiesDiedCounter;
            enemiesDiedLabel.SetAmount(enemiesDiedCounter);

            enemies.Clear();
            enemies.AddRange(aliveEnemies);
        }

        private void OnEnemyDied(EnemyBehavior enemy)
        {
            enemies.Remove(enemy);
            enemy.onEnemyDied -= OnEnemyDied;

            foreach(var dropData in enemy.GetDropData())
            {
                if(dropData.Chance == 0) continue;
                if(Random.value * 100 <= dropData.Chance && StageController.DropManager.CheckDropCooldown(dropData.DropType))
                {
                    StageController.DropManager.Drop(dropData.DropType, enemy.transform.position.XY() + Random.insideUnitCircle * 0.2f);
                }
            }

            enemiesDiedCounter++;
            stageSave.EnemiesKilled = enemiesDiedCounter;
            enemiesDiedLabel.SetAmount(enemiesDiedCounter);
        }

        private void OnBossDied(EnemyBehavior boss)
        {
            enemies.Remove(boss);
            boss.onEnemyDied -= OnBossDied;

            if (boss.ShouldSpawnChestOnDeath && StageController.AbilityManager.HasAvailableAbilities()) StageController.DropManager.Drop(DropType.Chest, boss.transform.position.XY() + Random.insideUnitCircle);
            StageController.DropManager.Drop(DropType.Magnet, boss.transform.position.XY() + Random.insideUnitCircle);
            StageController.DropManager.Drop(DropType.Food, boss.transform.position.XY() + Random.insideUnitCircle);

            enemiesDiedCounter++;
            stageSave.EnemiesKilled = enemiesDiedCounter;
            enemiesDiedLabel.SetAmount(enemiesDiedCounter);
        }

        public EnemyBehavior SpawnBoss(BossType bossType, Vector2 spawnPosition, UnityAction<EnemyBehavior> onBossDied = null)
        {
            var bossData = bossfightDatabase.GetBossfight(bossType);

            var boss = Instantiate(bossData.BossPrefab).GetComponent<EnemyBehavior>();
            boss.transform.position = spawnPosition;

            boss.Play();

            boss.onEnemyDied += OnBossDied;
            boss.onEnemyDied += onBossDied;

            enemies.Add(boss);

            return boss;
        }

        public BossfightData GetBossData(BossType bossType)
        {
            return bossfightDatabase.GetBossfight(bossType);
        }
    }
}