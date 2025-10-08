using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ����״̬����Ӧһ��Ԥ�����һ���������
/// </summary>
[CreateAssetMenu(fileName = "New Ingredient", menuName = "Cooking/SoupState")]
public class SoupState : ScriptableObject
{
    public string displayName = "δ�������";
    public List<IngredientSO> requiredIngredients; // ������ȫƥ��Ĳ���
    public GameObject visualPrefab; // ��Ӧ��Ԥ����
    public int price = 2; // ������ۼ�
}