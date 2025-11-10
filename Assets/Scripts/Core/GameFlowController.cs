// GameFlowController.cs
using UnityEngine;

public class GameFlowController : Singleton<GameFlowController>
{
    [Header("场景配置")]
    public SceneConfig[] allSceneConfigs;

    [Header("当前状态")]
    public GameMode currentMode = GameMode.MainMenu;
    public int currentStoryChapter = 0; // 0 表示未开始剧情

    // ------------------------------
    // 外部调用接口
    // ------------------------------

    /// <summary>
    /// 进入日常模式
    /// </summary>
    public void EnterDailyLife()
    {
        var config = GetSceneConfig(GameMode.DailyLife, -1);
        if (config != null)
        {
            currentMode = GameMode.DailyLife;
            SceneLoader.Instance.LoadScene(config.sceneName, GameMode.DailyLife);
        }
        else
        {
            Debug.LogError("未找到日常场景配置！");
        }
    }

    /// <summary>
    /// 进入剧情模式（指定章节）
    /// </summary>
    public void EnterStoryChapter(int chapter)
    {
        var config = GetSceneConfig(GameMode.StoryLine, chapter);
        if (config != null)
        {
            currentMode = GameMode.StoryLine;
            currentStoryChapter = chapter;
            SceneLoader.Instance.LoadScene(config.sceneName, GameMode.StoryLine);
        }
        else
        {
            Debug.LogError($"未找到剧情第 {chapter} 章配置！");
        }
    }

    /// <summary>
    /// 剧情推进到下一章
    /// </summary>
    public void AdvanceToNextChapter()
    {
        EnterStoryChapter(currentStoryChapter + 1);
    }

    /// <summary>
    /// 从剧情返回日常
    /// </summary>
    public void ReturnToDailyLife()
    {
        EnterDailyLife();
    }

    /// <summary>
    /// 返回主菜单
    /// </summary>
    public void ReturnToMainMenu()
    {
        currentMode = GameMode.MainMenu;
        currentStoryChapter = 0;
        GlobalManager.Instance.LoadMainMenu();
    }

    // ------------------------------
    // 内部逻辑
    // ------------------------------

    /// <summary>
    /// 场景加载完成后由 SceneLoader 调用
    /// </summary>
    public void OnSceneLoaded(GameMode mode)
    {
        // 可在此触发事件，如 EventManager.Trigger("OnGameModeChanged", mode);
        Debug.Log($"[GameFlowController] 当前模式: {mode}");
    }

    /// <summary>
    /// 根据模式和章节查找配置
    /// </summary>
    private SceneConfig GetSceneConfig(GameMode mode, int chapter)
    {
        foreach (var config in allSceneConfigs)
        {
            if (config.mode == mode)
            {
                if (mode == GameMode.StoryLine)
                {
                    if (config.chapterIndex == chapter)
                        return config;
                }
                else
                {
                    return config; // Daily 或 MainMenu 不看章节
                }
            }
        }
        return null;
    }
}