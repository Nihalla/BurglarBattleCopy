using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

public class InputDevicesTest : MonoBehaviour
{
    // This is an empty script that's just used to create a custom editor!
}

#if UNITY_EDITOR
[CustomEditor(typeof(InputDevicesTest))]
public class InputDevicesTestEditor : Editor
{
    // TODO(WSWhitehouse): Should probably convert this into a custom window...
    
    public override void OnInspectorGUI()
    {
        serializedObject.ApplyModifiedProperties();
        serializedObject.Update();
        
        DrawDefaultInspector();
        
        if (!Application.isPlaying)
        {
             EditorGUILayout.LabelField("Enter play mode to test!");
             return;
        }

        string buttonText = InputDevices.s_SearchingForDevices ? "Stop Searching for Devices" : "Start Searching for Devices";
        
        if (GUILayout.Button(buttonText))
        {
            if (InputDevices.s_SearchingForDevices)
            {
                InputDevices.StopSearchForDevices();
            }
            else
            {
                InputDevices.StartSearchForDevices();
            }
        }
        
        EditorGUILayout.BeginVertical("box");
        for (int i = 0; i < InputDevices.MAX_DEVICE_COUNT; i++)
        {
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField($"Device {i}", new GUIStyle("boldLabel"));

                EditorGUI.indentLevel++;

                if (InputDevices.Devices[i] == null)
                {
                    EditorGUILayout.LabelField("device not paired!");
                    EditorGUI.indentLevel--;
                    continue;
                }

                ref DeviceData data = ref InputDevices.Devices[i];
                EditorGUILayout.LabelField(data.Device.name);
                
                if (GUILayout.Button("Remove Device"))
                {
                    InputDevices.RemoveDevice(i);
                }

                EditorGUI.indentLevel--;
            }
        }

        EditorGUILayout.EndVertical();
    }
}
#endif // UNITY_EDITOR