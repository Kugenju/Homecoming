using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum ButterType
{
    Normal,
    Special
}

public class CookieMiniGameController : MonoBehaviour
{
    [Header("配置")]
    public List<CookieRecipe> allRecipes; // 6个饼干配方

    [Header("UI 引用")]
    public GameObject butterSelectionPanel;
    public GameObject recipeSelectionPanel;
    public GameObject detailPopupPanel;
    //public TMPro.TMP_Text detailTitleText;
    public TMPro.TMP_Text detailDescText;

    private ButterType? pendingButterSelection = null;
    private CookieRecipe pendingRecipeSelection = null;

    public void OnPlayerWin() => MiniGameEvents.OnMiniGameFinished?.Invoke(true);
    public void OnPlayerLose() => MiniGameEvents.OnMiniGameFinished?.Invoke(false);
    // 学生与原料关联表
    private static readonly Dictionary<string, Ingredient> StudentToIngredient = new()
    {
        { "WangChun", Ingredient.Sugar },
        { "LiuLe", Ingredient.Raisin },
        { "ZhaoYan", Ingredient.Cream },
        { "SunRui", Ingredient.Cream },
        { "LiHaotian", Ingredient.Sugar }
    };

    void Start()
    {
        ShowButterSelection();
    }

    void ShowButterSelection()
    {
        butterSelectionPanel.SetActive(true);
        recipeSelectionPanel.SetActive(false);
        if (detailPopupPanel) detailPopupPanel.SetActive(false);
    }

    void ShowRecipeSelection()
    {
        butterSelectionPanel.SetActive(false);
        recipeSelectionPanel.SetActive(true);
        if (detailPopupPanel) detailPopupPanel.SetActive(false);
    }

    // ———————— 黄油选择：触发弹窗 ————————

    public void OnClickNormalButter()
    {
        ShowButterDetail(ButterType.Normal);
    }

    public void OnClickSpecialButter()
    {
        ShowButterDetail(ButterType.Special);
    }

    void ShowButterDetail(ButterType type)
    {
        pendingButterSelection = type;
        //detailTitleText.text = type == ButterType.Normal ? "普通黄油" : "特制黄油";
        detailDescText.text = GetButterDescription(type);
        detailPopupPanel.SetActive(true);
    }

    // ———————— 饼干选择：触发弹窗 ————————

    public void OnClickRecipe(CookieRecipe recipe)
    {
        pendingRecipeSelection = recipe;
        //detailTitleText.text = recipe.recipeName;
        detailDescText.text = recipe.description;
        detailPopupPanel.SetActive(true);
    }

    // ———————— 弹窗确认 / 取消 ————————

    public void OnConfirmSelection()
    {
        if (pendingButterSelection.HasValue)
        {
            // 确认的是黄油
            ApplyButterSelection(pendingButterSelection.Value);
            pendingButterSelection = null;
            detailPopupPanel.SetActive(false);
            ShowRecipeSelection();
        }
        else if (pendingRecipeSelection != null)
        {
            // 确认的是饼干
            ApplyRecipeSelection(pendingRecipeSelection);
            pendingRecipeSelection = null;
            detailPopupPanel.SetActive(false);
            FinishGame(); // 本轮结束
        }
    }

    public void OnCancelSelection()
    {
        pendingButterSelection = null;
        pendingRecipeSelection = null;
        detailPopupPanel.SetActive(false);
    }

    // ———————— 应用选择 ————————

    void ApplyButterSelection(ButterType type)
    {
        bool isSpecial = (type == ButterType.Special);

        var students = GameStateTracker.Instance.students;
        foreach (var student in students)
        {
            if (isSpecial)
                student.dangerLevel = Mathf.Max(0, student.dangerLevel - 1);
            else
                student.dangerLevel += 1;
        }

        Debug.Log($"[Cookie Game] Applied butter: {(isSpecial ? "Special" : "Normal")}");
    }

    void ApplyRecipeSelection(CookieRecipe recipe)
    {
        foreach (var ingredient in recipe.ingredients)
        {
            foreach (var kvp in StudentToIngredient)
            {
                if (kvp.Value == ingredient)
                {
                    GameStateTracker.Instance.IncreaseDanger(kvp.Key, 1);
                }
            }
        }

        Debug.Log($"[Cookie Game] Applied recipe: {recipe.recipeName}");
    }

    // ———————— 工具方法 ————————

    string GetButterDescription(ButterType type)
    {
        return type switch
        {
            ButterType.Normal => "普通黄油，入口即化，甜而不腻。",
            ButterType.Special => "青山小学特制黄油，听说有独特的醇香，校长指定黄油，但你真的要加吗？",
            _ => "未知黄油"
        };
    }

    // ———————— 结束本轮 ————————

    void FinishGame()
    {
        OnPlayerWin();
    }
}