using OctoberStudio.Easing;
using OctoberStudio.Extensions;
using OctoberStudio.Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio
{
    public class EndlessFieldBehavior : IFieldBehavior
    {
        private PoolComponent<StageChunkBehavior> pool;

        private List<List<StageChunkBehavior>> chunks = new List<List<StageChunkBehavior>>();

        bool wait = false;

        public void Init(StageFieldData stageFieldData)
        {
            pool = new PoolComponent<StageChunkBehavior>("Background", stageFieldData.BackgroundPrefab, 9);

            var row = new List<StageChunkBehavior>();
            var chunk = pool.GetEntity();

            chunk.transform.position = Vector3.zero;
            chunk.transform.rotation = Quaternion.identity;
            chunk.transform.localScale = Vector3.one;

            row.Add(chunk);
            chunks.Add(row);

            wait = false;
            EasingManager.DoNextFrame().SetOnFinish(() => wait = true);

        }

        public void Update()
        {
            if (!wait) return;

            RemoveInvisibleChunks();
            CheckForNewChunks();
        }

        #region Add New Chunks

        private void CheckForNewChunks()
        {
            TryAddTopRow();
            TryAddBottomRow();
            TryAddLeftColumn();
            TryAddRightColumn();
        }

        private void TryAddTopRow()
        {
            if (chunks[0][0].HasEmptyTop)
            {
                var newTopRow = new List<StageChunkBehavior>();

                var columnCounts = chunks[0].Count;

                for (int i = 0; i < columnCounts; i++)
                {
                    var chunk = pool.GetEntity();
                    var chunkBellow = chunks[0][i];

                    chunk.transform.position = chunkBellow.transform.position + Vector3.up * chunk.Size.y;
                    chunk.transform.rotation = Quaternion.identity;
                    chunk.transform.localScale = Vector3.one;

                    newTopRow.Add(chunk);
                }

                chunks.Insert(0, newTopRow);
            }
        }

        private void TryAddBottomRow()
        {
            if (chunks[^1][^1].HasEmptyBottom)
            {
                var newBottomRow = new List<StageChunkBehavior>();

                var columnCounts = chunks[0].Count;

                for (int i = 0; i < columnCounts; i++)
                {
                    var chunk = pool.GetEntity();
                    var chunkOnTop = chunks[^1][i];

                    chunk.transform.position = chunkOnTop.transform.position + Vector3.down * chunk.Size.y;
                    chunk.transform.rotation = Quaternion.identity;
                    chunk.transform.localScale = Vector3.one;

                    newBottomRow.Add(chunk);
                }

                chunks.Add(newBottomRow);
            }
        }

        private void TryAddLeftColumn()
        {
            if (chunks[0][0].HasEmptyLeft)
            {
                for(int i = 0; i < chunks.Count; i++)
                {
                    var row = chunks[i];

                    var chunk = pool.GetEntity();
                    var chunkOnRight = chunks[i][0];

                    chunk.transform.position = chunkOnRight.transform.position + Vector3.left * chunk.Size.x;
                    chunk.transform.rotation = Quaternion.identity;
                    chunk.transform.localScale = Vector3.one;

                    row.Insert(0, chunk);
                }
            }
        }

        private void TryAddRightColumn()
        {
            if (chunks[^1][^1].HasEmptyRight)
            {
                for (int i = 0; i < chunks.Count; i++)
                {
                    var row = chunks[i];

                    var chunk = pool.GetEntity();
                    var chunkOnLeft = chunks[i][^1];

                    chunk.transform.position = chunkOnLeft.transform.position + Vector3.right * chunk.Size.x;
                    chunk.transform.rotation = Quaternion.identity;
                    chunk.transform.localScale = Vector3.one;

                    row.Add(chunk);
                }
            }
        }

        #endregion

        #region Remove Invisible Chunks

        private void RemoveInvisibleChunks()
        {
            if (chunks.Count == 0) return;

            CheckTopRow();
            CheckLeftColumn();
            CheckRightColumn();
            CheckBottomRow();
        }

        private void CheckTopRow()
        {
            if (!chunks[0][0].IsVisible)
            {
                if (!chunks[0][^1].IsVisible)
                {
                    RemoveTopRow();
                }
            }
        }

        private void CheckLeftColumn()
        {
            if (!chunks[0][0].IsVisible)
            {
                if (!chunks[^1][0].IsVisible)
                {
                    RemoveLeftColumn();
                }
            }
        }

        private void CheckRightColumn()
        {
            if (!chunks[^1][^1].IsVisible)
            {
                if (!chunks[0][^1].IsVisible)
                {
                    RemoveRightColumn();
                }
            }
        }

        private void CheckBottomRow()
        {
            if (!chunks[^1][^1].IsVisible)
            {
                if (!chunks[^1][0].IsVisible)
                {
                    RemoveBottomRow();
                }
            }
        }

        private void RemoveTopRow()
        {
            var topRow = chunks[0];

            for (int i = 0; i < topRow.Count; i++)
            {
                var chunk = topRow[i];

                chunk.gameObject.SetActive(false);
            }

            topRow.Clear();
            chunks.RemoveAt(0);
        }

        private void RemoveLeftColumn()
        {
            for (int i = 0; i < chunks.Count; i++)
            {
                var row = chunks[i];

                row[0].gameObject.SetActive(false);
                row.RemoveAt(0);
            }
        }

        private void RemoveRightColumn()
        {
            for (int i = 0; i < chunks.Count; i++)
            {
                var row = chunks[i];

                row[^1].gameObject.SetActive(false);
                row.RemoveAt(row.Count - 1);
            }
        }

        private void RemoveBottomRow()
        {
            var bottomRow = chunks[^1];

            for (int i = 0; i < bottomRow.Count; i++)
            {
                var chunk = bottomRow[i];

                chunk.gameObject.SetActive(false);
            }

            bottomRow.Clear();
            chunks.RemoveAt(chunks.Count - 1);
        }

        #endregion

        public bool ValidatePosition(Vector2 position) => true;

        public Vector2 GetRandomPositionOnBorder() => Vector2.zero;

        public Vector2 GetBossSpawnPosition(BossFenceBehavior fence, Vector2 offset)
        {
            var playerPosition = PlayerBehavior.Player.transform.position.XY();
            return playerPosition + offset;
        }

        public bool IsPointOutsideRight(Vector2 point, out float distance)
        {
            distance = 0;
            return false;
        }

        public bool IsPointOutsideLeft(Vector2 point, out float distance)
        {
            distance = 0;
            return false;
        }

        public bool IsPointOutsideTop(Vector2 point, out float distance)
        {
            distance = 0;
            return false;
        }

        public bool IsPointOutsideBottom(Vector2 point, out float distance)
        {
            distance = 0;
            return false;
        }

        public void Clear()
        {
            pool.Destroy();
        }
    }
}