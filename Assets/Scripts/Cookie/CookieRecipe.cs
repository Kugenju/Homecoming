using UnityEngine;
using System.Collections.Generic;

public enum Ingredient
{
    Butter,      // 黄油（仅第一阶段使用）
    Sugar,       // 糖
    Raisin,      // 葡萄干
    Cream,       // 奶油
    Chocolate,   // 巧克力
    Matcha       // 抹茶
}

[CreateAssetMenu(menuName = "MiniGame/Cookie Recipe", fileName = "New Cookie Recipe")]
public class CookieRecipe : ScriptableObject
{
    [Header("基本信息")]
    public string recipeName = "新饼干";

    [Header("原料列表")]
    public List<Ingredient> ingredients = new List<Ingredient>();

    [TextArea(3, 6)]
    public string description = "点击查看详情...";
}