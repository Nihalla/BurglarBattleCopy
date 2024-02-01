// Author: William Whitehouse (WSWhitehouse)

using System;
using UnityEngine;

#if UNITY_EDITOR
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
#endif

/// <summary>
/// Place this attribute on a float field to change it to a minute and second input field in the inspector.
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class TimeFieldAttribute : PropertyAttribute
{ }

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(TimeFieldAttribute))]
public class TimeFieldPropertyDrawer : PropertyDrawer
{
    private static GUIStyle s_labelStyleRightAligned = new GUIStyle("label") { alignment = TextAnchor.MiddleRight  };
    
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUIContent minLabel = new GUIContent("Min ", "Minutes");
        GUIContent secLabel = new GUIContent("Sec ", "Seconds");
        GUIContent msLabel = new GUIContent("Ms", "Milliseconds");
        GUIContent rawLabel = new GUIContent("Raw ", "Raw time value.");
        

        float minLabelWidth = s_labelStyleRightAligned.CalcSize(minLabel).x;
        float secLabelWidth = s_labelStyleRightAligned.CalcSize(secLabel).x;
        float msLabelWidth = s_labelStyleRightAligned.CalcSize(msLabel).x;
        float rawLabelWidth = s_labelStyleRightAligned.CalcSize(rawLabel).x;

        // Calculate current minutes and seconds is dependant on the property type.
        int min, sec, ms;

        // NOTE(WSWhitehouse): To add more property types add more branches here to calculate the current time.
        // Add another branch to the if statement at the bottom of this function to draw the raw value and update
        // the serialized property.
        if (property.propertyType == SerializedPropertyType.Float)
        {
            float currentTime = property.floatValue;
            min = (int)math.floor(currentTime / 60f);
            sec = (int)currentTime - (60 * min);
            float fracTime = currentTime % 1;
            ms = (int)(fracTime * 100);
        }
        else
        {
            Debug.LogError($"Property Type ({property.propertyType.ToString()}) is not supported on TimeFieldAttribute!");
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        EditorGUI.BeginProperty(position, label, property);

        // Calculate label and field rects
        const float fieldGap = 10f;
        float fieldWidth = (position.width - EditorGUIUtility.labelWidth - minLabelWidth - secLabelWidth - msLabelWidth -rawLabelWidth - (fieldGap * 3)) * 0.25f;

        Rect labelRect = position;
        labelRect.width = EditorGUIUtility.labelWidth;

        Rect minLabelRect = position;
        minLabelRect.width = minLabelWidth;
        minLabelRect.x = labelRect.x + EditorGUIUtility.labelWidth;

        Rect minRect = position;
        minRect.width = fieldWidth;
        minRect.x = minLabelRect.x + minLabelWidth;

        Rect secLabelRect = position;
        secLabelRect.width = secLabelWidth;
        secLabelRect.x = minRect.x + fieldWidth + fieldGap;
        
        Rect secRect = position;
        secRect.width = fieldWidth;
        secRect.x = secLabelRect.x + secLabelWidth;

        Rect msLabelRect = position;
        msLabelRect.width = msLabelWidth;
        msLabelRect.x = secRect.x + fieldWidth + fieldGap;
        
        Rect msRect = position;
        msRect.width = fieldWidth;
        msRect.x = msLabelRect.x + msLabelWidth;

        Rect rawLabelRect = position;
        rawLabelRect.width = rawLabelWidth;
        rawLabelRect.x = msRect.x + fieldWidth + (fieldGap*2f);

        Rect rawRect = position;
        rawRect.width = fieldWidth;
        rawRect.x = rawLabelRect.x + rawLabelWidth;
        
        EditorGUI.LabelField(labelRect, label);

        EditorGUI.LabelField(minLabelRect, minLabel, s_labelStyleRightAligned);
        int newMin = EditorGUI.IntField(minRect, min);
        
        EditorGUI.LabelField(secLabelRect, secLabel, s_labelStyleRightAligned);
        int newSec = EditorGUI.IntField(secRect, sec);
        
        EditorGUI.LabelField(msLabelRect, msLabel, s_labelStyleRightAligned);
        int newMs = EditorGUI.IntField(msRect, ms);
        
        float invms = 1 / 100;
        // Calculate the time from the int values
        float intTime = (newMin * 60f) + newSec + (newMs*invms);

    

        if (property.propertyType == SerializedPropertyType.Float)
        {
            float currentTime = (min * 60f) + sec + (ms*invms);

            EditorGUI.LabelField(rawLabelRect, rawLabel, s_labelStyleRightAligned);
            float newTime = EditorGUI.FloatField(rawRect, currentTime);

            // NOTE(WSWhitehouse): Check if new time or int time has changed and update the property if so
            if (math.abs(newTime - currentTime) >= 0.001) property.floatValue = newTime;
            if (math.abs(intTime - currentTime) >= 0.001) property.floatValue = intTime;
        }

        EditorGUI.EndProperty();
    }
}
#endif