using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 自定义属性：根据布尔字段控制Inspector中字段的显示
/// </summary>
public class ConditionalShowAttribute : PropertyAttribute
{
    public string conditionalSourceField;
    public bool hideInInspector;

    public ConditionalShowAttribute(string conditionalSourceField, bool hideInInspector = false)
    {
        this.conditionalSourceField = conditionalSourceField;
        this.hideInInspector = hideInInspector;
    }
}
