#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

/// <summary>
/// Class <c>ToggleFieldPropertyDraw</c> A Property drawer used to disable or hide variables in editor
/// </summary>
[CustomPropertyDrawer(typeof(ToggleFieldAttribute))]
public class ToggleFieldPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ToggleFieldAttribute toggleAttribute = (ToggleFieldAttribute)attribute;
        bool showField = ToggleAttributeResult(toggleAttribute, property);
        bool guiEnabled = GUI.enabled;
        GUI.enabled = showField;
        
        if (!toggleAttribute.hideField || showField)
        {
            EditorGUI.PropertyField(position, property, label, true);
        }
        GUI.enabled = guiEnabled;
    }
    
    /// <summary>
    /// Function that checks the attributes bool value and inverts it if invert is true
    /// </summary>
    /// <param name="toggleAttribute"></param>
    /// <param name="property"></param>
    /// <returns>If the field should be shown or not</returns>
    public bool ToggleAttributeResult(ToggleFieldAttribute toggleAttribute, SerializedProperty property)
    {
        bool showProperty = true;
        string path = property.propertyPath;
        
        // toggleAttribute.fieldname is the name of the boolean to be checked. property.name is the property to be toggled.
        string pathToToggle = path.Replace(property.name, toggleAttribute.fieldName);
        
        // toggleProperty is the property to be shown or hidden
        SerializedProperty toggleProperty = property.serializedObject.FindProperty(pathToToggle);
       // //Debug.Log(toggleProperty.name);
        if (toggleProperty != null)
        {
            if (toggleAttribute.inverted)
            {
                showProperty = invert(toggleProperty.boolValue);
            }
            else
            {
                showProperty = toggleProperty.boolValue;
            }
        }
        else
        {
            Debug.LogWarning("Using ToggleFieldAttribute with no boolean control, missing" + toggleAttribute.fieldName);
        }
        return showProperty;
    }
    
    /// <summary>
    /// Function <c>GetPropertyHeight</c> is used to correctly place the property in editor.
    /// </summary>
    /// <param name="property"></param>
    /// <param name="label"></param>
    /// <returns></returns>
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        ToggleFieldAttribute toggleAttribute = (ToggleFieldAttribute)attribute;
        bool showField = ToggleAttributeResult(toggleAttribute, property);
        if (showField || !toggleAttribute.hideField)
        {
            return EditorGUI.GetPropertyHeight(property, label);
        }
        else
        {
            return -EditorGUIUtility.standardVerticalSpacing;
        }

    }

    private bool invert(bool propertyBool)
    {
        return !propertyBool;
    }
}

#endif // UNITY_EDITOR
