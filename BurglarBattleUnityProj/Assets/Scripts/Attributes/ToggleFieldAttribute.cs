using System;
using UnityEngine;

/// <summary>
/// Class <c>ToggleFieldAttribute</c> allows for the toggling of displayed variables in the inspector using boolean values
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct)]
public class ToggleFieldAttribute : PropertyAttribute
{
    ///<value>The name of the boolean to be used as a flag.</value>
    public string fieldName = "";
    ///<value>If true: Hides the field. If false: disables the field.</value>
    public bool hideField; 
    /// <value>If true: the flag boolean is inverted.</value>
    public bool inverted; 
    
    /// <summary>
    /// This method registers the name of the field used to toggle the property the attribute is attached to
    /// <example>
    /// Disabling when true and enabling when false
    /// </example>
    /// <code>
    /// [SerializeField] bool myBool;
    /// [SerializeField, ToggleField("foo")] float myFloat;
    /// </code>
    /// </summary>
    /// <param name="toggleFieldName"> The name of the boolean used to toggle the field</param>
    public ToggleFieldAttribute(string toggleFieldName)
    {
        fieldName = toggleFieldName;
        hideField = false;
        inverted = false;
    }
    
    /// <summary>
    /// This method registers the name of the field used to toggle the variable.
    /// The second parameter will completely hide the variable if true
    /// </summary>
    /// <param name="toggleFieldName">The name of the boolean used to toggle the field</param>
    /// <param name="hide">If false: Disables in editor. If true: Hides in editor.</param>
    public ToggleFieldAttribute(string toggleFieldName, bool hide)
    {
        fieldName = toggleFieldName;
        hideField = hide;
        inverted = false;
    }

    /// <summary>
    /// This method registers the name of the field used to toggle the variable.
    /// The second parameter will completely hide the variable if true.
    /// The third parameter allows for inversion of the first parameter
    /// </summary>
    /// <param name="toggleFieldName">The name of the boolean used to toggle the field.</param>
    /// <param name="hide">If false: Disables in editor. If true: Hides in editor.</param>
    /// <param name="invert">If true then the parameter will hide/disable if the input toggle field is false.</param>
    public ToggleFieldAttribute(string toggleFieldName, bool hide, bool invert)
    {
        fieldName = toggleFieldName;
        hideField = hide;
        inverted = invert;
    }
}
