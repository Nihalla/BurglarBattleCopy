#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Cyan
{
  [CustomPropertyDrawer(typeof(Blit.BlitSettings))]
  public class BlitEditor : PropertyDrawer
  {
    private bool setupCompleted = false;
    private GUIStyle boldLabel;
    
    private MaterialEditor _matEditor;
    Material _material;
    SerializedProperty _materialProperty;

    private void Setup()
    {
      setupCompleted = true;
      boldLabel = GUI.skin.label;
      boldLabel.fontStyle = FontStyle.Bold;
    }
    
    private void CreateMaterialEditor()
    {
      if (_matEditor != null)
      {
        UnityEngine.Object.DestroyImmediate(_matEditor);
      }

      if (_material == null) return;
      
      _matEditor = UnityEditor.Editor.CreateEditor(_material) as MaterialEditor;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
      //base.OnGUI(position, property, label);
      if (!setupCompleted) Setup();
      
      _materialProperty = property.FindPropertyRelative("blitMaterial");
      _material         = _materialProperty.objectReferenceValue as Material;
      CreateMaterialEditor();

      // Blit Settings
      EditorGUI.BeginProperty(position, label, property);
      EditorGUI.LabelField(position, "Blit Settings", boldLabel);
      SerializedProperty _event = property.FindPropertyRelative("Event");
      EditorGUILayout.PropertyField(_event);

      // "After Rendering Post Processing" Warning
#if !UNITY_2021_2_OR_NEWER
		// Think the AfterRenderingPostProcessing event now works correctly in 2021
        if (_event.intValue == (int)UnityEngine.Rendering.Universal.RenderPassEvent.AfterRenderingPostProcessing) {
            EditorGUILayout.HelpBox("The \"After Rendering Post Processing\" event does not work with Camera Color targets. " +
                "Unsure how to actually obtain the target after post processing has been applied. " +
                "Frame debugger seems to suggest a <no name> target?\n\n" +
                "Use the \"After Rendering\" event instead!", MessageType.Warning, true);
        }
#endif

      EditorGUILayout.PropertyField(_materialProperty);
      EditorGUILayout.PropertyField(property.FindPropertyRelative("blitMaterialPassIndex"));
      EditorGUILayout.PropertyField(property.FindPropertyRelative("setInverseViewMatrix"));
#if UNITY_2020_1_OR_NEWER
      EditorGUILayout.PropertyField(property.FindPropertyRelative("requireDepthNormals"));
#endif
      EditorGUILayout.PropertyField(property.FindPropertyRelative("cameraType"));

      // Source
      EditorGUILayout.Separator();
      EditorGUILayout.LabelField("Source", boldLabel);
      SerializedProperty srcType = property.FindPropertyRelative("srcType");
      EditorGUILayout.PropertyField(srcType);
      int enumValue = srcType.intValue;
      if (enumValue == (int)Blit.Target.TextureID)
      {
        EditorGUILayout.PropertyField(property.FindPropertyRelative("srcTextureId"));
      }
      else if (enumValue == (int)Blit.Target.RenderTextureObject)
      {
        EditorGUILayout.PropertyField(property.FindPropertyRelative("srcTextureObject"));
      }

      // Destination
      EditorGUILayout.Separator();
      EditorGUILayout.LabelField("Destination", boldLabel);
      SerializedProperty dstType = property.FindPropertyRelative("dstType");
      EditorGUILayout.PropertyField(dstType);
      enumValue = dstType.intValue;
      if (enumValue == (int)Blit.Target.TextureID)
      {
        EditorGUILayout.PropertyField(property.FindPropertyRelative("dstTextureId"));

        SerializedProperty overrideGraphicsFormat = property.FindPropertyRelative("overrideGraphicsFormat");
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(overrideGraphicsFormat);
        if (overrideGraphicsFormat.boolValue)
        {
          EditorGUILayout.PropertyField(property.FindPropertyRelative("graphicsFormat"), GUIContent.none);
        }

        EditorGUILayout.EndHorizontal();
      }
      else if (enumValue == (int)Blit.Target.RenderTextureObject)
      {
        EditorGUILayout.PropertyField(property.FindPropertyRelative("dstTextureObject"));
      }
      
      if (_matEditor != null)
      {
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Material Editor", boldLabel);
        
        bool isDefaultMaterial = !AssetDatabase.GetAssetPath(_material).StartsWith("Assets");
        
        _matEditor.DrawHeader();
        using (new EditorGUI.DisabledGroupScope(isDefaultMaterial))
        {
          _matEditor.OnInspectorGUI();
        }
        
        UnityEngine.Object.DestroyImmediate(_matEditor);
      }

      EditorGUI.indentLevel = 1;
      EditorGUI.EndProperty();
      property.serializedObject.ApplyModifiedProperties();
    }
  }
}
#endif