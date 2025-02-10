using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio
{
    [CreateAssetMenu(fileName = "Stage Field Data", menuName = "October/Stage Field Data")]
    public class StageFieldData : ScriptableObject
    {
        [SerializeField] GameObject backgroundPrefab;
        public GameObject BackgroundPrefab => backgroundPrefab;

        [Header("Sides")]
        [SerializeField] GameObject topPrefab;
        [SerializeField] GameObject bottomPrefab;
        [SerializeField] GameObject leftPrefab;
        [SerializeField] GameObject rightPrefab;

        public GameObject TopPrefab => topPrefab;
        public GameObject BottomPrefab => bottomPrefab;
        public GameObject LeftPrefab => leftPrefab;
        public GameObject RightPrefab => rightPrefab;

        [Header("Corners")]
        [SerializeField] GameObject topRightPrefab;
        [SerializeField] GameObject topLeftPrefab;
        [SerializeField] GameObject bottomRightPrefab;
        [SerializeField] GameObject bottomLeftPrefab;

        public GameObject TopRightPrefab => topRightPrefab;
        public GameObject TopLeftPrefab => topLeftPrefab;
        public GameObject BottomRightPrefab => bottomRightPrefab;
        public GameObject BottomLeftPrefab => bottomLeftPrefab;

        [Header("Prop")]
        [SerializeField] List<StagePropData> propChances;
        public List<StagePropData> PropChances => propChances;
    }

    [System.Serializable]
    public class StagePropData
    {
        [SerializeField] GameObject prefab;
        [SerializeField, Min(1)] int maxAmount;
        [SerializeField, Range(0, 100)] float chance;
        
        public GameObject Prefab => prefab;
        public int MaxAmount => maxAmount;
        public float Chance => chance;
    }
}