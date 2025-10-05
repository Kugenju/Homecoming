using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Food", menuName = "Cooking/Food Item")]
public class FoodItemSO : ScriptableObject
{
    public string foodName;                 // 菜名
    public Sprite icon;                     // 图标
    public int basePrice;                   // 基础售价
    public float cookTime;                  // 制作时间（秒）
    public List<IngredientSO> requiredIngredients; // 所需材料
    public bool isComboAllowed = false;     // 是否可作为组合菜
}
