using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Food", menuName = "Cooking/Food Item")]
public class FoodItemSO : ScriptableObject
{
    public string foodName;                 // ����
    public Sprite icon;                     // ͼ��
    public int basePrice;                   // �����ۼ�
    public float cookTime;                  // ����ʱ�䣨�룩
    public List<IngredientSO> requiredIngredients; // �������
    public bool isComboAllowed = false;     // �Ƿ����Ϊ��ϲ�
}
