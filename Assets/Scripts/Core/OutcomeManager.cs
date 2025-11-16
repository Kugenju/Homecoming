// OutcomeManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class OutcomeManager : MonoBehaviour
{
    public static OutcomeManager Instance { get; private set; }

    [Header("结局UI")]
    public GameObject endingCanvas;          // 结局画布（含文本、按钮）
    public UnityEngine.UI.Text endingTitle;  // 结局标题
    public UnityEngine.UI.Text endingText;   // 结局描述

    [Header("结局配置")]
    public EndingConfig[] endings;           // 所有结局定义（ScriptableObject 或内联）

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (endingCanvas != null)
            endingCanvas.SetActive(false);
    }

    /// <summary>
    /// 触发指定ID的结局（如 "axe_victim", "happiness"）
    /// </summary>
    public void TriggerEnding(string endingId)
    {
        var config = GetEndingConfig(endingId);
        if (config == null)
        {
            Debug.LogError($"Ending '{endingId}' not found!");
            return;
        }

        // 1. 解锁结局（记录到全局状态）
        GameStateTracker.Instance.UnlockEnding(endingId);

        // 2. 显示结局UI
        ShowEndingUI(config.title, config.description);

        // 3. （可选）播放音效、动画等
        // AudioManager.Play(config.audioClip);
    }

    /// <summary>
    /// 根据全局危险值自动判定最终结局
    /// </summary>
    public void EvaluateAndTriggerFinalEnding()
    {
        int highDangerCount = GameStateTracker.Instance.CountStudentsWithDangerAtLeast(4);

        string endingId = "happiness"; // 默认好结局

        if (highDangerCount >= 7)
            endingId = "immortality";
        else if (highDangerCount > 3)
            endingId = "pain";

        TriggerEnding(endingId);
    }

    private EndingConfig GetEndingConfig(string id)
    {
        foreach (var ending in endings)
        {
            if (ending.id == id)
                return ending;
        }
        return null;
    }

    private void ShowEndingUI(string title, string description)
    {
        if (endingCanvas == null)
        {
            Debug.LogWarning("EndingCanvas not assigned!");
            return;
        }

        endingTitle.text = title;
        endingText.text = description;
        endingCanvas.SetActive(true);

        // 暂停游戏时间（可选）
        Time.timeScale = 0f;
    }

    // ———————— UI 回调方法（供按钮绑定） ————————

    /// <summary>
    /// 重试：返回当前段落起点（用于失败结局如“斧下亡魂”）
    /// </summary>
    public void OnRetryButtonClicked()
    {
        Time.timeScale = 1f;
        endingCanvas.SetActive(false);

        // 从临时标记中读取返回点
        string graphId = GameStateTracker.Instance.GetTempFlag("return_graph_id");
        string nodeId = GameStateTracker.Instance.GetTempFlag("return_node_id");

        if (!string.IsNullOrEmpty(graphId) && !string.IsNullOrEmpty(nodeId))
        {
            // 重新加载原剧情段落
            var graph = Resources.Load<NarrativeGraph>($"NarrativeGraphs/{graphId}");
            if (graph != null)
            {
                DialogueManager.Instance.LoadAndPlayGraph(graph);
                DialogueManager.Instance.PlayFromNode(nodeId);
                return;
            }
        }

        // 若无有效返回点，回退到主线章节1
        GameFlowController.Instance.EnterStoryChapter(1);
    }

    /// <summary>
    /// 返回主菜单
    /// </summary>
    public void OnBackToMenuButtonClicked()
    {
        Time.timeScale = 1f;
        endingCanvas.SetActive(false);
        GameFlowController.Instance.EnterMainMenu();
    }

    /// <summary>
    /// 继续（用于好结局后进入下一章或 credits）
    /// </summary>
    public void OnContinueButtonClicked()
    {
        Time.timeScale = 1f;
        endingCanvas.SetActive(false);

        // 示例：跳转到片尾 or 主菜单
        GameFlowController.Instance.EnterMainMenu();
    }
}

// 结局配置数据（可替换为 ScriptableObject）
[System.Serializable]
public class EndingConfig
{
    public string id;               // 如 "axe_victim"
    public string title;            // 显示标题
    public string description;      // 结局文本
    public AudioClip audioClip;  // 可选：配音或BGM
}