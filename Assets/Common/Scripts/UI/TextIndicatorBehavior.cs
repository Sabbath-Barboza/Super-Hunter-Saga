using TMPro;
using UnityEngine;

namespace OctoberStudio.UI
{
    public class TextIndicatorBehavior : MonoBehaviour
    {
        [SerializeField] RectTransform rectTransform;
        [SerializeField] TMP_Text textComponent;

        public void SetText(string text)
        {
            textComponent.text = text;
        }

        public void SetAnchors(Vector2 viewportPosition)
        {
            rectTransform.anchorMin = viewportPosition;
            rectTransform.anchorMax = viewportPosition;
        }

        public void SetPosition(Vector2 position)
        {
            rectTransform.anchoredPosition = position;
        }

        public void SetScale(Vector3 scale)
        {
            rectTransform.localScale = scale;
        }
    }
}