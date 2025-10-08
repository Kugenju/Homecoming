using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 汤的状态：对应一种预制体和一组所需材料
/// </summary>
[CreateAssetMenu(fileName = "New Ingredient", menuName = "Cooking/SoupState")]
public class SoupState : ScriptableObject
{
    public string displayName = "未命名组合";
    public List<IngredientSO> requiredIngredients; // 必须完全匹配的材料
    public GameObject visualPrefab; // 对应的预制体
    public int price = 2; // 此组合售价
}