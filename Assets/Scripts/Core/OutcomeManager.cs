using UnityEngine;

public class OutcomeManager : MonoBehaviour
{
    public static OutcomeManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 根据学生危险等级评估并触发对应结局叙事图
    /// </summary>
    public void EvaluateAndTriggerFinalEnding()
    {
        int highDangerCount = GameStateTracker.Instance.CountStudentsWithDangerAtLeast(4);
        string graphId = "Ending_Happiness"; // 默认结局

        if (highDangerCount >= 5)
            graphId = "Ending_Immortality";
        else if (highDangerCount > 3)
            graphId = "Ending_Pain";

        TriggerEndingByNarrativeGraph(graphId);
    }

    /// <summary>
    /// 加载指定ID的结局叙事图并播放（从 startNodeId 开始）
    /// </summary>
    public void TriggerEndingByNarrativeGraph(string graphId)
    {
        // 1. 解锁结局（用于成就/记录）
        GameStateTracker.Instance.UnlockEnding(graphId);

        // 2. 加载叙事图
        var graph = Resources.Load<NarrativeGraph>($"NarrativeGraphs/{graphId}");
        if (graph == null)
        {
            Debug.LogError($"[OutcomeManager] 未找到结局叙事图: NarrativeGraphs/{graphId}");
            // 回退到默认结局
            graph = Resources.Load<NarrativeGraph>("NarrativeGraphs/Ending_Happiness");
            if (graph == null)
            {
                Debug.LogError("默认结局也缺失，返回主菜单");
                GameFlowController.Instance.EnterMainMenu();
                return;
            }
        }

        // 3. 交由 DialogueManager 播放（复用现有流程）
        DialogueManager.Instance.LoadAndPlayGraph(graph);

        // 4. 监听叙事结束事件，结束后返回主菜单
        DialogueManager.Instance.OnEndingNarrativeFinished();
    }
}