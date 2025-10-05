using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 经济系统：管理金币、收入、支出、结算
/// 与 GameManager 和 UI 联动
/// </summary>
public class EconomySystem : MonoBehaviour
{
    // 单例
    public static EconomySystem Instance { get; private set; }

    [Header("初始设置")]
    public int startingMoney = 100; // 初始资金

    [Header("UI 显示")]
    public TMPro.TextMeshProUGUI moneyText; // 金币显示文本

    // 当前金币
    private int currentMoney;

    // 当日收入与支出（用于结算）
    private int dailyIncome;
    private int dailyExpenses;

    // ------------------------------
    // Unity 生命周期函数
    // ------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        InitializeEconomy();
    }

    // ------------------------------
    // 初始化与重置
    // ------------------------------

    /// <summary>
    /// 初始化经济系统（新游戏开始）
    /// </summary>
    public void InitializeEconomy()
    {
        currentMoney = startingMoney;
        dailyIncome = 0;
        dailyExpenses = 0;
        UpdateMoneyUI();
        Debug.Log($"💰 初始资金：{currentMoney} 元");
    }

    /// <summary>
    /// 重置每日数据（每天开始时调用）
    /// </summary>
    public void ResetDailyEconomy()
    {
        dailyIncome = 0;
        dailyExpenses = 0;
        Debug.Log("📅 新的一天，收入与支出已重置。");
    }

    // ------------------------------
    // 金币操作方法
    // ------------------------------

    /// <summary>
    /// 增加金币（顾客付款）
    /// </summary>
    public void AddMoney(int amount)
    {
        if (amount <= 0) return;

        currentMoney += amount;
        dailyIncome += amount;
        UpdateMoneyUI();
        Debug.Log($"✅ 收入：+{amount} 元，当前余额：{currentMoney} 元");

        // 可触发事件：如 "OnCustomerPaid"
        // EventManager.TriggerEvent("OnIncome", amount);
    }

    /// <summary>
    /// 扣除金币（购买原料、升级等）
    /// </summary>
    public bool SpendMoney(int amount)
    {
        if (amount <= 0) return true; // 零消费视为成功

        if (currentMoney >= amount)
        {
            currentMoney -= amount;
            dailyExpenses += amount;
            UpdateMoneyUI();
            Debug.Log($"💸 支出：-{amount} 元，剩余：{currentMoney} 元");
            return true;
        }
        else
        {
            Debug.LogWarning($"❌ 余额不足！需要 {amount} 元，当前只有 {currentMoney} 元。");
            // 可触发事件：如 "OnPurchaseFailed"
            return false;
        }
    }

    // ------------------------------
    // 结算与查询
    // ------------------------------

    /// <summary>
    /// 获取当前余额
    /// </summary>
    public int GetMoney() => currentMoney;

    /// <summary>
    /// 获取当日净收入
    /// </summary>
    public int GetDailyProfit() => dailyIncome - dailyExpenses;

    /// <summary>
    /// 游戏结束时的最终结算（对接 GameManager）
    /// </summary>
    public void FinalizeDailyReport()
    {
        int profit = GetDailyProfit();
        Debug.Log($"📊 今日营业结束：");
        Debug.Log($"  收入：{dailyIncome} 元");
        Debug.Log($"  支出：{dailyExpenses} 元");
        Debug.Log($"  净收益：{profit} 元");
        Debug.Log($"  最终余额：{currentMoney} 元");

        // 可保存到存档系统
        // SaveSystem.SaveDailyReport(dailyIncome, dailyExpenses, profit);
    }

    // ------------------------------
    // UI 与工具方法
    // ------------------------------

    /// <summary>
    /// 更新金币 UI 显示
    /// </summary>
    private void UpdateMoneyUI()
    {
        if (moneyText != null)
        {
            moneyText.text = $"¥ {currentMoney}";
        }
    }

    /// <summary>
    /// 强制设置金币（调试用，谨慎使用）
    /// </summary>
    public void SetMoney(int amount)
    {
        currentMoney = Mathf.Max(0, amount);
        UpdateMoneyUI();
        Debug.Log($"🔧 金币已设置为：{currentMoney} 元");
    }
}
