/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPointGUI : MonoBehaviour
{

}

[UnityEditor.CustomEditor(typeof(FourPlayerManager))]
public class SpawnPointGUIEditor : UnityEditor.Editor
{
    public void ShowArrayProperty(UnityEditor.SerializedProperty list)
    {
        UnityEditor.EditorGUI.indentLevel += 1;
        for (int i = 0; i < list.arraySize; i++)
        {
            if (i < 2)
            {
                UnityEditor.EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i), new UnityEngine.GUIContent("Team 1 Spawnpoint"));
            }
            else
            {
                UnityEditor.EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i), new UnityEngine.GUIContent("Team 2 Spawnpoint"));
            }
        }
        UnityEditor.EditorGUI.indentLevel -= 1;
    }

    public override void OnInspectorGUI()
    {
        ShowArrayProperty(serializedObject.FindProperty("spawnpoints"));
    }
}*/
