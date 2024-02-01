// Author: William Whitehouse (WSWhitehouse)

using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif // UNITY_EDITOR

/// <summary>
/// Put this attribute on a string or integer field exposed to the inspector to turn it
/// into a dropdown list of all available scenes. If the field is a string the name of
/// the scene is placed in the value, if it's an integer the build index of the string
/// is placed into the value. Warning: If the build settings are updated the scene is
/// NOT automatically updated to the correct value - instead it will use whatever value
/// was originally set. This just makes it easier to spot!
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class SceneAttribute : PropertyAttribute { }

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(SceneAttribute))]
public class SceneAttributePropertyDrawer : PropertyDrawer
{
    private const string NoScene = "<< No Scene >>";

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;

        List<string> sceneList = new List<string>(scenes.Length + 1);
        List<string> sceneDisplayName = new List<string>(scenes.Length + 1);

        sceneList.Add(NoScene);
        sceneDisplayName.Add(NoScene);

        for (int i = 0; i < scenes.Length; i++)
        {
            string sceneName = Path.GetFileNameWithoutExtension(scenes[i].path);
            sceneList.Add(sceneName);
            sceneDisplayName.Add($"{i}: {sceneName}");
        }

        switch (property.propertyType)
        {
            case SerializedPropertyType.String:
            {
                string propertyString = property.stringValue;
                int index = 0;
                for (int i = 1; i < sceneList.Count; i++)
                {
                    if (sceneList[i] != propertyString) continue;

                    index = i;
                    break;
                }

                index = EditorGUI.Popup(position, label.text, index, sceneDisplayName.ToArray());

                EditorGUI.BeginProperty(position, label, property);
                property.stringValue = index >= 1 ? sceneList[index] : string.Empty;
                EditorGUI.EndProperty();
                break;
            }

            case SerializedPropertyType.Integer:
            {
                int index = property.intValue + 1;
                index = EditorGUI.Popup(position, label.text, index, sceneDisplayName.ToArray());

                EditorGUI.BeginProperty(position, label, property);
                property.intValue = index >= 1 ? index - 1 : -1;
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