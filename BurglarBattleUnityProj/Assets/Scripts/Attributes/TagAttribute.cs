// Author: William Whitehouse (WSWhitehouse)
 
using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

/// <summary>
/// Put this attribute on a string variable exposed in the inspector to turn it into
/// a dropdown list of all available tags. This replaces manually typing tags into
/// fields which can lead to typos and/or invalid tags altogether.
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class TagAttribute : PropertyAttribute { }

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(TagAttribute))]
public class TagAttributePropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        switch (property.propertyType)
        {
            case SerializedPropertyType.String:
            {
                // NOTE(WSWhitehouse): Set tag to untagged if its empty
                if (string.IsNullOrEmpty(property.stringValue))
                {
                    property.stringValue = "Untagged";
                }

                EditorGUI.BeginProperty(position, label, property);
                property.stringValue = EditorGUI.TagField(position, label, property.stringValue);
                EditorGUI.EndProperty();
                break;
            }
            
            default:
            {
                EditorGUI.PropertyField(position, property, label);
                break;
            }
        }
    }
}
#endif // UNITY_EDITOR
