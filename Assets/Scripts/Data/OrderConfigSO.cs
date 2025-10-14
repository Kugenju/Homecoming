using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "New Order Config", menuName = "Cooking/Order Configuration")]
public class OrderConfigSO : ScriptableObject
{
    [Header("基础信息")]
    public string displayName = "默认菜品";
    public Sprite icon; // 菜品图标（可用于UI显示）

    [Header("通用设置")]
    public bool isComboAllowed = false; // 是否允许多种材料组合
    public int baseQuantity = 1;        // 默认数量

    [Space]
    [Tooltip("最小和最大可售数量，用于随机订单生成")]
    public Vector2Int quantityRange = new Vector2Int(1, 2); // 桂花糕可要1~2块
    [ConditionalShow("isComboAllowed")]
    [Header("【组合类专用】组合规则")]
    public List<ComboRule> comboRules;

    /// <summary>
    /// 组合规则：定义一组材料及其对应的价格和出现权重
    /// </summary>
    [Serializable]
    public class ComboRule
    {
        [Tooltip("此组合的名称（仅用于编辑器查看）")]
        public string ruleName = "未命名组合";

        [Tooltip("需要添加的食材（如鸭血、鸭肉）")]
        public List<IngredientSO> ingredients;

        [Tooltip("该组合的售价（元）")]
        public int price = 2;

        [Tooltip("该组合在随机生成时的权重（数值越高越常见）")]
        public int weight = 1;

        /// <summary>
        /// 判断两个组合是否包含相同的食材（无视顺序）
        /// </summary>
        
        public bool Matches(List<IngredientSO> otherIngredients)
        {
            if (ingredients.Count != otherIngredients.Count) return false;
            foreach (var ing in ingredients)
            {
                if (!otherIngredients.Contains(ing)) return false;
            }
            return true;
        }

        public override string ToString()
        {
            string names = string.Join(" + ", ingredients.ConvertAll(i => i?.ingredientName ?? "空"));
            return $"{names} ({price}元)";
        }
    }

    // ================================
    // 公共方法：供 OrderManager 调用
    // ================================

    /// <summary>
    /// 获取随机生成的订单数量（在 quantityRange 范围内）
    /// </summary>
    public int GetRandomQuantity()
    {
        return UnityEngine.Random.Range(quantityRange.x, quantityRange.y + 1);
    }

    /// <summary>
    /// （组合类菜品）根据权重随机选择一个组合规则
    /// </summary>
    public ComboRule GetRandomComboRule()
    {
        if (comboRules == null || comboRules.Count == 0) return null;

        int totalWeight = 0;
        foreach (var rule in comboRules)
        {
            totalWeight += rule.weight;
        }

        int randomValue = UnityEngine.Random.Range(0, totalWeight);
        int cumulativeWeight = 0;

        foreach (var rule in comboRules)
        {
            cumulativeWeight += rule.weight;
            if (randomValue < cumulativeWeight)
            {
                return rule;
            }
        }

        return comboRules[0]; // 防止极端情况
    }

    /// <summary>
    /// 根据已选材料查找匹配的组合规则（用于结算）
    /// </summary>
    public ComboRule FindMatchingRule(List<IngredientSO> selectedIngredients)
    {
        foreach (var rule in comboRules)
        {
            if (rule.Matches(selectedIngredients))
            {
                return rule;
            }
        }
        return null;
    }

    /// <summary>
    /// 获取所有组合中最高价格（用于UI提示或成就）
    /// </summary>
    public int GetMaxPrice()
    {
        int max = baseQuantity * 2; // 基础价兜底
        if (isComboAllowed)
        {
            foreach (var rule in comboRules)
            {
                max = Mathf.Max(max, rule.price);
            }
        }
        return max;
    }
}
