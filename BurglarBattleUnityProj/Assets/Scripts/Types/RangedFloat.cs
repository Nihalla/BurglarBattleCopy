// Author: William Whitehouse (WSWhitehouse)

using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

/// <summary>
/// A struct that has a minimum and maximum float value. With some useful utility functions.
/// Adding the <see cref="RangedTypeAttribute"/> attribute to an inspector exposed field will
/// create a min/max slider, and be able to set the min and max value.
/// </summary>
[Serializable]
public struct RangedFloat
{
    public RangedFloat(float minValue = 0f, float maxValue = 1f)
    {
        this.minValue = minValue;
        this.maxValue = maxValue;
    }

    public float minValue;
    public float maxValue;

    /// <summary>
    /// Is the value included in the min/max range (inclusive).
    /// </summary>
    /// <param name="val">Value to check.</param>
    /// <returns>True when in range; false otherwise.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool InRange(float val) => val >= minValue && val <= maxValue;

    /// <summary>
    /// Randomly generate a value within the min/max range (inclusive).
    /// </summary>
    /// <returns>Randomly generated float within min/max range</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Random() => UnityEngine.Random.Range(minValue, maxValue);

    /// <summary>
    /// Clamp the value within the min/max range.
    /// </summary>
    /// <param name="val">Value to clamp.</param>
    /// <returns>Clamped value between the min and max.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Clamp(float val) => math.clamp(val, minValue, maxValue);

    /// <summary>
    /// Lerp between the min and max by <paramref name="t"/>.
    /// </summary>
    /// <param name="t">Time value to lerp.</param>
    /// <returns>Interpolated value between min/max by <paramref name="t"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Lerp(float t) => math.lerp(minValue, maxValue, t);
    
    /// <summary>
    /// Formatted string representation of the RangedFloat. Useful for
    /// outputting to the debug console.
    /// </summary>
    /// <returns>String value of RangedFloat.</returns>
    public override string ToString()
    {
       return $"Min Value: {minValue.ToString(CultureInfo.CurrentCulture)}, Max Value: {maxValue.ToString(CultureInfo.CurrentCulture)}";
    }
}

/// <summary>
/// An attribute that can be added to the <see cref="RangedFloat"/> type to specify the min
/// and max values. As well as provide a min/max slider.
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class RangedTypeAttribute : PropertyAttribute
{
    public RangedTypeAttribute(float min, float max)
    {
        this.min = min;
        this.max = max;
    }

    public float min;
    public float max;
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(RangedFloat), true)]
public class RangedFloatDrawer : PropertyDrawer
{
    private RangedTypeAttribute _rangeAttribute;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        RangedTypeAttribute[] attributes = fieldInfo.GetCustomAttributes(typeof(RangedTypeAttribute), true) as RangedTypeAttribute[];
        _rangeAttribute = attributes?.Length > 0 ? attributes[0] : null;

        label    = EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, label);

        SerializedProperty minProperty = property.FindPropertyRelative("minValue");
        SerializedProperty maxProperty = property.FindPropertyRelative("maxValue");

        if (_rangeAttribute == null)
        {
            // When no RangedTypeAttribute has been added to the field.
            DefaultRangedFloatDrawer(position, property);
        }
        else
        {
            float minValue = minProperty.floatValue;
            float maxValue = maxProperty.floatValue;
            
            float floatFieldWidth   = position.width / 8.0f;
            float minMaxSliderWidth = (floatFieldWidth * 6.0f) - 1.0f;

            // Min Value Field
            {
                EditorGUI.BeginChangeCheck();

                position.width = floatFieldWidth;
                minValue = EditorGUI.FloatField(position, minValue);

                if (EditorGUI.EndChangeCheck())
                {
                    minProperty.floatValue = math.clamp(minValue, _rangeAttribute.min, math.min(_rangeAttribute.max, maxValue));
                }
            }

            position.x += floatFieldWidth;

            // Min Max Slider
            {
                EditorGUI.BeginChangeCheck();
                
                position.width = minMaxSliderWidth;
                EditorGUI.MinMaxSlider(position, ref minValue, ref maxValue, _rangeAttribute.min, _rangeAttribute.max);
                
                if (EditorGUI.EndChangeCheck())
                {
                    minProperty.floatValue = math.clamp(minValue, _rangeAttribute.min, math.min(_rangeAttribute.max, maxValue));
                    maxProperty.floatValue = math.clamp(maxValue, math.max(_rangeAttribute.min, minValue), _rangeAttribute.max);
                }
            }
            
            position.x += minMaxSliderWidth + 1f;

            // Max Value Field
            {
                EditorGUI.BeginChangeCheck();
                
                position.width = floatFieldWidth;
                maxValue = EditorGUI.FloatField(position, maxValue);
                        
                if (EditorGUI.EndChangeCheck())
                {
                    maxProperty.floatValue = math.clamp(maxValue, math.max(_rangeAttribute.min, minValue), _rangeAttribute.max);
                }
            }
        }

        EditorGUI.EndProperty();
    }

    private static void DefaultRangedFloatDrawer(Rect position, SerializedProperty property)
    {
        position.width /= 4f;
        EditorGUIUtility.labelWidth /= 4f;
        position.width *= 4f;
        position.width *= 0.375f;

        EditorGUI.PropertyField(position, property.FindPropertyRelative("minValue"), new GUIContent("Min"));

        position.x += position.width;

        EditorGUI.PropertyField(position, property.FindPropertyRelative("maxValue"), new GUIContent("Max"));
    }
}
#endif // UNITY_EDITOR