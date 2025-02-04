using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio
{
    [CreateAssetMenu(fileName = "Stage Database", menuName = "October/Stages Database")]
    public class StagesDatabase : ScriptableObject
    {
        [SerializeField] List<StageData> stages;

        public int StagesCount => stages.Count;

        public StageData GetStage(int stageId)
        {
            return stages[stageId];
        }
    }
}