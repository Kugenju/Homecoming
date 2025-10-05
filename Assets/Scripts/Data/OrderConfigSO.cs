using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "New Order Config", menuName = "Cooking/Order Configuration")]
public class OrderConfigSO : ScriptableObject
{
    [Header("������Ϣ")]
    public string displayName = "Ĭ�ϲ�Ʒ";
    public Sprite icon; // ��Ʒͼ�꣨������UI��ʾ��

    [Header("ͨ������")]
    public bool isComboAllowed = false; // �Ƿ�������ֲ������
    public int baseQuantity = 1;        // Ĭ������

    [Space]
    [Tooltip("��С�����������������������������")]
    public Vector2Int quantityRange = new Vector2Int(1, 2); // �𻨸��Ҫ1~2��
    [ConditionalShow("isComboAllowed")]
    [Header("�������ר�á���Ϲ���")]
    public List<ComboRule> comboRules;

    /// <summary>
    /// ��Ϲ��򣺶���һ����ϼ����Ӧ�ļ۸�ͳ���Ȩ��
    /// </summary>
    [Serializable]
    public class ComboRule
    {
        [Tooltip("����ϵ����ƣ������ڱ༭���鿴��")]
        public string ruleName = "δ�������";

        [Tooltip("��Ҫ��ӵ�ʳ�ģ���ѼѪ��Ѽ�⣩")]
        public List<IngredientSO> ingredients;

        [Tooltip("����ϵ��ۼۣ�Ԫ��")]
        public int price = 2;

        [Tooltip("��������������ʱ��Ȩ�أ���ֵԽ��Խ������")]
        public int weight = 1;

        /// <summary>
        /// �ж���������Ƿ������ͬ��ʳ�ģ�����˳��
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
            string names = string.Join(" + ", ingredients.ConvertAll(i => i?.ingredientName ?? "��"));
            return $"{names} ({price}Ԫ)";
        }
    }

    // ================================
    // ������������ OrderManager ����
    // ================================

    /// <summary>
    /// ��ȡ������ɵĶ����������� quantityRange ��Χ�ڣ�
    /// </summary>
    public int GetRandomQuantity()
    {
        return UnityEngine.Random.Range(quantityRange.x, quantityRange.y + 1);
    }

    /// <summary>
    /// ��������Ʒ������Ȩ�����ѡ��һ����Ϲ���
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

        return comboRules[0]; // ��ֹ�������
    }

    /// <summary>
    /// ������ѡ���ϲ���ƥ�����Ϲ������ڽ��㣩
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
    /// ��ȡ�����������߼۸�����UI��ʾ��ɾͣ�
    /// </summary>
    public int GetMaxPrice()
    {
        int max = baseQuantity * 2; // �����۶���
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
