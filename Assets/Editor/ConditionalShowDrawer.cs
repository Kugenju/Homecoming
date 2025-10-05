using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ConditionalShowAttribute))]
public class ConditionalShowDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ConditionalShowAttribute condHAtt = (ConditionalShowAttribute)attribute;
        bool enabled = GetConditionalVisibility(property, condHAtt);

        if (!condHAtt.hideInInspector || enabled)
        {
            EditorGUI.BeginDisabledGroup(!enabled);
            EditorGUI.PropertyField(position, property, label, true);
            EditorGUI.EndDisabledGroup();
        }
    }

    private bool GetConditionalVisibility(SerializedProperty property, ConditionalShowAttribute condHAtt)
    {
        string path = property.propertyPath.Contains(".") ?
            property.propertyPath.Substring(0, property.propertyPath.LastIndexOf(".")) : "";
        SerializedProperty sourcePropertyValue = property.serializedObject.FindProperty(path + "." + condHAtt.conditionalSourceField);
        return sourcePropertyValue != null && sourcePropertyValue.boolValue;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        ConditionalShowAttribute condHAtt = (ConditionalShowAttribute)attribute;
        bool enabled = GetConditionalVisibility(property, condHAtt);
        if (!condHAtt.hideInInspector || enabled)
        {
            return EditorGUI.GetPropertyHeight(property, label);
        }
        else
        {
            return -EditorGUIUtility.standardVerticalSpacing;
        }
    }
}