using OctoberStudio.Drop;
using OctoberStudio.Easing;
using OctoberStudio.Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio
{
    public class DropManager : MonoBehaviour
    {
        [SerializeField] DropDatabase database;

        public Dictionary<DropType, PoolComponent<DropBehavior>> dropPools = new Dictionary<DropType, PoolComponent<DropBehavior>>();
        public Dictionary<DropType, float> lastTimeDropped = new Dictionary<DropType, float>();
        
        public List<DropBehavior> dropList = new List<DropBehavior>();

        private int startIndex;

        public void Init()
        {
            for (int i = 0; i < database.GemsCount; i++)
            {
                var data = database.GetGemData(i);

                var pool = new PoolComponent<DropBehavior>($"Drop_{data.DropType}", data.Prefab, 100);

                dropPools.Add(data.DropType, pool);
                lastTimeDropped.Add(data.DropType, 0);
            }
        }

        private void Update()
        {
            // Evaluating only a third drops every frame. Optimization techick.
            startIndex++;
            startIndex %= 3;
            for (int i = startIndex; i < dropList.Count; i += 3)
            {
                if(PlayerBehavior.Player.IsInsideMagnetRadius(dropList[i].transform))
                {
                    var drop = dropList[i];
                    drop.transform.DoPosition(PlayerBehavior.CenterTransform, 0.25f).SetEasing(EasingType.BackIn).SetOnFinish(() =>
                    {
                        drop.OnPickedUp();
                    });

                    dropList.RemoveAt(i);

                    i--;
                }
            }
        }

        public void PickUpAllDrop()
        {
            StartCoroutine(PickUpAllDropCoroutine());
        }

        private IEnumerator PickUpAllDropCoroutine()
        {
            var mgnetizedDropList = new List<DropBehavior>();

            for(int i = 0; i < dropList.Count; i++)
            {
                if (dropList[i].DropData.AffectedByMagnet)
                {
                    mgnetizedDropList.Add(dropList[i]);
                    dropList.RemoveAt(i);
                    i--;
                }
            }

            for (int i = 0; i < mgnetizedDropList.Count; i ++)
            {
                // Preventing lags by picking up 10 drops each frame
                if (i % 10 == 0) yield return null;

                var drop = mgnetizedDropList[i];
                drop.transform.DoPosition(PlayerBehavior.CenterTransform, 0.4f).SetEasing(EasingType.BackIn).SetOnFinish(() =>
                {
                    drop.OnPickedUp();
                });
            }

            mgnetizedDropList.Clear();
        }

        public bool CheckDropCooldown(DropType dropType)
        {
            return Time.time - lastTimeDropped[dropType] >= database.GetGemData(dropType).DropCooldown;
        }

        public void Drop(DropType dropType, Vector3 position)
        {
            var drop = dropPools[dropType].GetEntity();

            drop.Init(database.GetGemData(dropType));
            drop.transform.position = position;

            dropList.Add(drop);

            lastTimeDropped[dropType] = Time.time;
        }
    }
}