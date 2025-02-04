using OctoberStudio.Timeline;
using System;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;

namespace OctoberStudio.Timeline.Editor
{
    [CustomTimelineEditor(typeof(WaveAsset))]
    public class WaveEditor : ClipEditor
    {
        public override ClipDrawOptions GetClipOptions(TimelineClip clip)
        {
            var clipOptions = base.GetClipOptions(clip);

            if(clip.asset is BurstWave)
            {
                clipOptions.highlightColor = Color.blue;
                clip.displayName = "Burst";
            } else if (clip.asset is ContinuousWave)
            {
                clipOptions.highlightColor = Color.yellow;
                clip.displayName = "Continuous";
            } else if (clip.asset is MaintainWave)
            {
                clipOptions.highlightColor = Color.red;
                clip.displayName = "Maintain";
            }

            var fromTo = $"{Math.Round(clip.start, 2)}s - {Math.Round(clip.end, 2)}s";
            clipOptions.tooltip = fromTo;

            return clipOptions;
        }
    }
}