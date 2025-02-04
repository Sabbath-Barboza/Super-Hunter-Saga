using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio
{
    public class StageChunkBehavior : MonoBehaviour
    {
        [SerializeField] SpriteRenderer sprite;
        public Vector2 Size => sprite.size;
        public bool IsVisible => sprite.isVisible;

        public float LeftBound => transform.position.x - Size.x / 2;
        public float RightBound => transform.position.x + Size.x / 2;
        public float TopBound => transform.position.y + Size.y / 2;
        public float BottomBound => transform.position.y - Size.y / 2;

        public bool HasEmptyLeft => LeftBound > CameraManager.LeftBound;
        public bool HasEmptyRight => RightBound < CameraManager.RightBound;
        public bool HasEmptyTop => TopBound < CameraManager.TopBound;
        public bool HasEmptyBottom => BottomBound > CameraManager.BottomBound;

        private List<Transform> borders = new List<Transform>();
            
        public void AddBorder(Transform border)
        {
            borders.Add(border);
        }

        public void Clear()
        {
            for(int i = 0; i < borders.Count; i++)
            {
                borders[i].gameObject.SetActive(false);
            }

            borders.Clear();

            gameObject.SetActive(false);
        }
    }
}