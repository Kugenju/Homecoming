using UnityEngine;
using System;

/// <summary>
/// 游戏内时间系统
/// 控制营业时间、时间流逝、时间事件等
/// 与 GameManager 状态同步（暂停/播放）
/// </summary>
public class TimeSystem : MonoBehaviour
{
    // 单例模式
    public static TimeSystem Instance { get; private set; }

    [Header("时间设置")]
    public float startTime = 8.0f;        // 营业开始时间（8:00）
    public float endTime = 12.0f;         // 营业结束时间（12:00）
    public float timeScale = 60.0f;       // 游戏时间流速：1秒 = 60秒（即1分钟）

    [Header("UI 显示")]
    public TMPro.TextMeshProUGUI timeText; // 可选：显示当前时间的UI

    // 当前游戏时间（以小时为单位，如 8.5 表示 8:30）
    private float currentTime;

    // 是否正在运行
    private bool isRunning = false;

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
        InitializeTime();
    }

    private void Update()
    {
        if (!isRunning) return;

        // 只在 Playing 状态下推进时间
        if (GameManager.Instance.IsGamePlaying())
        {
            AdvanceTime(Time.unscaledDeltaTime * timeScale);
        }
    }

    // ------------------------------
    // 时间控制方法
    // ------------------------------

    /// <summary>
    /// 初始化时间（通常在进入游戏场景时调用）
    /// </summary>
    public void InitializeTime()
    {
        currentTime = startTime;
        isRunning = true;
        UpdateUITime();
        Debug.Log($"🕒 营业开始：{FormatTime(currentTime)}");
    }

    /// <summary>
    /// 推进时间（按增量）
    /// </summary>
    private void AdvanceTime(float hours)
    {
        currentTime += hours;

        // 检查是否到点打烊
        if (currentTime >= endTime)
        {
            currentTime = endTime;
            OnTimeEnded();
        }
        else
        {
            // 检查是否整点变化（可触发事件）
            int prevHour = Mathf.FloorToInt(currentTime);
            int currHour = Mathf.FloorToInt(currentTime + hours);
            if (currHour > prevHour)
            {
                OnHourChanged(currHour);
            }
        }

        UpdateUITime();
    }

    /// <summary>
    /// 手动设置时间（调试或剧情需要）
    /// </summary>
    public void SetTime(float hour)
    {
        currentTime = Mathf.Clamp(hour, startTime, endTime);
        UpdateUITime();
    }

    /// <summary>
    /// 暂停时间（由 GameManager 控制）
    /// </summary>
    public void PauseTime()
    {
        isRunning = false;
    }

    /// <summary>
    /// 恢复时间
    /// </summary>
    public void ResumeTime()
    {
        if (GameManager.Instance.IsGamePlaying() && currentTime < endTime)
        {
            isRunning = true;
        }
    }

    // ------------------------------
    // 事件响应
    // ------------------------------

    /// <summary>
    /// 时间到达结束点
    /// </summary>
    private void OnTimeEnded()
    {
        isRunning = false;
        Debug.Log($"🔚 营业结束！今日收入结算中...");

        // 触发事件（可对接结算系统）
        // EventManager.TriggerEvent("OnTimeEnded");

        // 通知 GameManager 结束游戏
        GameManager.Instance.EndGame();
    }

    /// <summary>
    /// 整点变化
    /// </summary>
    private void OnHourChanged(int hour)
    {
        Debug.Log($"🔔 时间来到 {hour}:00");
        // 可触发客流变化、顾客类型切换等
    }

    // ------------------------------
    // UI 与工具方法
    // ------------------------------

    /// <summary>
    /// 更新 UI 显示时间
    /// </summary>
    private void UpdateUITime()
    {
        if (timeText != null)
        {
            timeText.text = FormatTime(currentTime);
        }
    }

    /// <summary>
    /// 格式化时间为 HH:MM
    /// </summary>
    public string FormatTime(float hour)
    {
        int h = Mathf.FloorToInt(hour);
        int m = Mathf.FloorToInt((hour - h) * 60);
        return $"{h:D2}:{m:D2}";
    }

    /// <summary>
    /// 获取当前时间（外部查询用）
    /// </summary>
    public float GetCurrentTime() => currentTime;

    /// <summary>
    /// 获取进度百分比（用于进度条）
    /// </summary>
    public float GetProgress()
    {
        float duration = endTime - startTime;
        float elapsed = currentTime - startTime;
        return Mathf.Clamp01(elapsed / duration);
    }

    // ------------------------------
    // 外部接口（供 GameManager 调用）
    // ------------------------------

    /// <summary>
    /// 由 GameManager 在游戏开始时调用
    /// </summary>
    public void StartGameplay()
    {
        InitializeTime();
    }

    /// <summary>
    /// 由 GameManager 在暂停时调用
    /// </summary>
    public void OnGamePaused()
    {
        PauseTime();
    }

    /// <summary>
    /// 由 GameManager 在恢复时调用
    /// </summary>
    public void OnGameResumed()
    {
        ResumeTime();
    }
}