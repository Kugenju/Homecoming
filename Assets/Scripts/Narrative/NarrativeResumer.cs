using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NarrativeResumer : MonoBehaviour
{
    void Start()
    {
        // 检查是否需要从小游戏恢复
        string graphId = GameStateTracker.Instance.GetTempFlag("resume_narrative_graph_id");
        if (!string.IsNullOrEmpty(graphId))
        {
            string nodeId = GameStateTracker.Instance.GetTempFlag("resume_narrative_node_id");

            // 清除临时 flag
            GameStateTracker.Instance.ClearTempFlags();

            // 加载对应的 NarrativeGraph（现在在主场景，Resources 可用）
            var graph = Resources.Load<NarrativeGraph>($"NarrativeGraphs/Chapter_{graphId}");
            // 或根据你的实际路径调整，比如：
            // var graph = Resources.Load<NarrativeGraph>($"NarrativeGraphs/{graphId}");

            if (graph != null)
            {
                DialogueManager.Instance.LoadAndPlayGraph(graph);
                DialogueManager.Instance.PlayFromNode(nodeId);
            }
            else
            {
                Debug.LogError($"Failed to load narrative graph: {graphId}");
                GameFlowController.Instance.EnterMainMenu();
            }
        }
    }
}