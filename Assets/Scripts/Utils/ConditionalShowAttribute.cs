using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// �Զ������ԣ����ݲ����ֶο���Inspector���ֶε���ʾ
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
