#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

// https://forum.unity.com/threads/how-can-i-play-an-animation-backwards.498287/
public static class ReverseAnimationContext
{
    [MenuItem("Assets/Create Reversed Clip", false, 14)]
    private static void ReverseClip()
    {
        string directoryPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(Selection.activeObject));
        string fileName = Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject));
        string fileExtension = Path.GetExtension(AssetDatabase.GetAssetPath(Selection.activeObject));
        fileName = fileName.Split('.')[0];
        string copiedFilePath = directoryPath + Path.DirectorySeparatorChar + fileName + "_Reversed" + fileExtension;

        AnimationClip clip = GetSelectedClip();
        AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(Selection.activeObject), copiedFilePath);
        clip = (AnimationClip)AssetDatabase.LoadAssetAtPath(copiedFilePath, typeof(AnimationClip));
        if (clip == null) return;

        float clipLength = clip.length;
        AnimationClipCurveData[] curves = AnimationUtility.GetAllCurves(clip, true);
        clip.ClearCurves();
        
        foreach (AnimationClipCurveData curve in curves)
        {
            Keyframe[] keys = curve.curve.keys;
            int keyCount = keys.Length;
            WrapMode postWrapmode = curve.curve.postWrapMode;
            curve.curve.postWrapMode = curve.curve.preWrapMode;
            curve.curve.preWrapMode = postWrapmode;
            for (int i = 0; i < keyCount; i++)
            {
                Keyframe K = keys[i];
                K.time = clipLength - K.time;
                float tmp = -K.inTangent;
                K.inTangent = -K.outTangent;
                K.outTangent = tmp;
                keys[i] = K;
            }
            curve.curve.keys = keys;
            clip.SetCurve(curve.path, curve.type, curve.propertyName, curve.curve);
        }
        
        AnimationEvent[] events = AnimationUtility.GetAnimationEvents(clip);
        if (events.Length > 0)
        {
            for (int i = 0; i < events.Length; i++)
            {
                events[i].time = clipLength - events[i].time;
            }
            AnimationUtility.SetAnimationEvents(clip, events);
        }

        Debug.Log("Animation reversed!");
    }
 
    [MenuItem("Assets/Create Reversed Clip", true)]
    private static bool ReverseClipValidation()
    {
        return Selection.activeObject is AnimationClip;
    }
 
    public static AnimationClip GetSelectedClip()
    {
        AnimationClip[] clips = Selection.GetFiltered(typeof(AnimationClip), SelectionMode.Assets) as AnimationClip[];
        if (clips != null && clips.Length > 0)
        {
            return clips[0];
        }
        
        return null;
    }
 
}
#endif
