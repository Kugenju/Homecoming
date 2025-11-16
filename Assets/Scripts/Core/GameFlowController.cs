// GameFlowController.cs
using UnityEngine;

public class GameFlowController : Singleton<GameFlowController>
{
    [Header("场景配置")]
    public SceneConfig[] allSceneConfigs;
    public DialogueManager dialogueManager;

    [Header("当前状态")]
    public GameMode currentMode = GameMode.MainMenu;
    public int currentStoryChapter = 0; // 0 表示未开始剧情

    // ------------------------------
    // 外部调用接口
    // ------------------------------

    public void EnterSubMenu()
    {
        var config = GetSceneConfig(GameMode.SubMenu, -1);
        if (config != null)
        {
            currentMode = GameMode.SubMenu;
            SceneLoader.Instance.LoadScene(config.sceneName, GameMode.SubMenu);
        }
        else
        {
            Debug.LogError("未找到子菜单场景配置！");
        }
    }

    public void EnterMainMenu()
    {
        var config = GetSceneConfig(GameMode.MainMenu, -1);
        if (config != null)
        {
            currentMode = GameMode.MainMenu;
            SceneLoader.Instance.LoadScene(config.sceneName, GameMode.MainMenu);
        }
        else
        {
            Debug.LogError("未找到主菜单场景配置！");
        }
    }

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
        if(currentMode != GameMode.StoryLine)
        {
            var config = GetSceneConfig(GameMode.StoryLine, 1);
            if (config != null)
            {
                currentMode = GameMode.StoryLine;
                currentStoryChapter = chapter;
                SceneLoader.Instance.LoadScene(config.sceneName, GameMode.StoryLine);
            }
            else
            {
                Debug.LogError($"未找到剧情场景配置！");
            }
        }
    }

    public void EnterMiniGame(string GameID)
    {
        try
        {
            SceneLoader.Instance.LoadScene(GameID, GameMode.MiniGame);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"加载小游戏 {GameID} 失败: {e.Message}");
        }

    }

    /// <summary>
    /// 剧情推进到下一章
    /// </summary>
    public void AdvanceToNextChapter()
    {
        currentStoryChapter++;
        LoadAndPlayNarrativeForChapter(currentStoryChapter);
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

    public void RestartCurrentMode()
    {
        switch (currentMode)
        {
            case GameMode.MainMenu:
                ReturnToMainMenu();
                break;
            case GameMode.DailyLife:
                EnterDailyLife();
                break;
            case GameMode.StoryLine:
                EnterStoryChapter(currentStoryChapter);
                break;
            case GameMode.SubMenu:
                EnterSubMenu();
                break;
            default:
                Debug.LogError("未知游戏模式，无法重启！");
                break;
        }
    }

    public void ExitGame()
    {
        Debug.Log("[GameFlowController] 退出游戏");
        #if UNITY_EDITOR
                            UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }

    public void ReturnToSubMenu()
    {
        currentMode = GameMode.SubMenu;
        EnterSubMenu();
    }
    // ------------------------------
    // 内部逻辑
    // ------------------------------

    /// <summary>
    /// 场景加载完成后由 SceneLoader 调用
    /// </summary>
    public void OnSceneLoaded(GameMode mode)
    {
        Debug.Log($"[GameFlowController] 当前模式: {mode}");
        if (mode == GameMode.StoryLine)
        {
            // 判断是否是主线剧情（而非小游戏）
            if (currentStoryChapter < 100) // 假设 100+ 是小游戏
            {
                LoadAndPlayNarrativeForChapter(currentStoryChapter);
            }
        }
    }

    private void LoadAndPlayNarrativeForChapter(int chapter)
    {
        string graphPath = $"NarrativeGraphs/Chapter_{chapter}";
        var graph = Resources.Load<NarrativeGraph>(graphPath);

        if (graph != null)
        {
            DialogueManager.Instance.LoadAndPlayGraph(graph);
        }
        else
        {
            Debug.LogError($"未找到剧情段落: {graphPath}");
            // 可选：回退到主菜单
            EnterMainMenu();
        }
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