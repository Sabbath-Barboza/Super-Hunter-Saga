using OctoberStudio.Easing;
using UnityEngine;
using UnityEngine.Playables;

namespace OctoberStudio.Timeline
{
    public class CameraControlBehavior : PlayableBehaviour
    {
        public float TargetCameraSize { get; set; }
        public EasingType Easing { get; set; }

        private float startCameraSize;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            startCameraSize = Camera.main.orthographicSize;
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {       
            float time = (float)playable.GetTime();
            float duration = (float)playable.GetDuration();

            float t = EasingFunctions.ApplyEasing(time / duration, Easing);

            StageController.CameraController.SetSize(Mathf.Lerp(startCameraSize, TargetCameraSize, t));
        }
    }
}