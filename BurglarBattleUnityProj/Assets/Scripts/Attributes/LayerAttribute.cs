// Author: William Whitehouse (WSWhitehouse)

using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

/// <summary>
/// Place this attribute on a string or integer field to turn it into a dropdown of
/// all available layers. If the field is a string the name of the layer is stored,
/// if it is an integer the layer index is stored instead. Warning: The values stored
/// will NOT be automatically updated with the layers in project settings if they change.
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class LayerAttribute : PropertyAttribute { }

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(LayerAttribute))]
public class LayerAttributePropertyDrawer : PropertyDrawer
{
  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
  {
      switch (property.propertyType)
      {
          case SerializedPropertyType.String:
          {
              int layerInt = LayerMask.NameToLayer(property.stringValue);
      
              // NOTE(WSWhitehouse): Set layer to default if it doesnt exist!
              if (layerInt < 0) layerInt = 0;
   
              EditorGUI.BeginProperty(position, label, property);
              property.stringValue = LayerMask.LayerToName(EditorGUI.LayerField(position, label, layerInt));
              EditorGUI.EndProperty();
              break;
          }
          
          case SerializedPropertyType.Integer:
          {
              EditorGUI.BeginProperty(position, label, property);
              property.intValue = EditorGUI.LayerField(position, label, property.intValue);
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
