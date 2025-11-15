// DialogueManager.cs
using UnityEngine;
using System;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    private NarrativeGraph _currentGraph;
    private NarrativeNode _currentNode;
    private Action<bool> _miniGameCallback; // 用于接收小游戏结果

    // UI 引用（需在 Inspector 挂接或通过 FindObjectOfType 获取）
    public DialogueUI dialogueUI; // 假设你有一个负责显示文本/选项的 UI 类

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
    /// 加载一个剧情段落并从起点开始播放
    /// </summary>
    public void LoadAndPlayGraph(NarrativeGraph graph)
    {
        _currentGraph = graph;
        PlayFromNode(graph.startNodeId);
    }

    /// <summary>
    /// 从指定节点开始播放
    /// </summary>
    public void PlayFromNode(string nodeId)
    {
        _currentNode = _currentGraph?.GetNode(nodeId);
        if (_currentNode == null)
        {
            Debug.LogError($"Node '{nodeId}' not found in graph '{_currentGraph?.graphId}'");
            return;
        }

        ProcessNode(_currentNode);
    }

    private void ProcessNode(NarrativeNode node)
    {
        // 更新背景、角色等（通知 UI 系统）
        UpdateVisuals(node);

        switch (node.nodeType)
        {
            case NarrativeNode.NodeType.Dialogue:
                ShowDialogue(node.lines, () =>
                {
                    if (!string.IsNullOrEmpty(node.nextNodeId))
                        PlayFromNode(node.nextNodeId);
                    else
                        OnNarrativeCompleted(); // 剧情自然结束
                });
                break;

            case NarrativeNode.NodeType.Choice:
                ShowChoices(node.options, selectedOption =>
                {
                    PlayFromNode(selectedOption.targetNodeId);
                });
                break;

            case NarrativeNode.NodeType.MiniGame:
                StartMiniGame(node.miniGameId);
                break;

            case NarrativeNode.NodeType.Ending:
                TriggerEnding(node.nodeId);
                break;
        }
    }

    private void UpdateVisuals(NarrativeNode node)
    {
        // 通知 UI 系统更新背景和角色
        dialogueUI?.SetBackground(node.backgroundAsset);
        dialogueUI?.SetCharacters(node.characters);
    }

    private void ShowDialogue(string[] lines, Action onComplete)
    {
        dialogueUI?.ShowDialogue(lines, onComplete);
    }

    private void ShowChoices(ChoiceOption[] options, Action<ChoiceOption> onSelect)
    {
        dialogueUI?.ShowChoices(options, onSelect);
    }

    private void StartMiniGame(string gameId)
    {
        // 记录当前段落检查点（用于失败后返回）
        GameStateTracker.Instance.SetTempFlag("return_graph_id", _currentGraph.graphId);
        GameStateTracker.Instance.SetTempFlag("return_node_id", _currentGraph.GetRestartCheckpoint());

        // 跳转到对应小游戏场景（通过 GameFlowController）
        GameFlowController.Instance.EnterMiniGame(gameId);
    }

    //private int GetMiniGameChapter(string gameId)
    //{
    //    // 根据你的 SceneConfig 配置映射
    //    return gameId switch
    //    {
    //        "wangchun" => 101,
    //        "liule" => 102,
    //        _ => -1
    //    };
    //}

    private void TriggerEnding(string endingId)
    {
        OutcomeManager.Instance.TriggerEnding(endingId);
    }

    private void OnNarrativeCompleted()
    {
        Debug.Log("Narrative segment completed.");

        GameFlowController.Instance.AdvanceToNextChapter();
    }

    // ———————— 外部调用接口 ————————

    /// <summary>
    /// 供小游戏场景结束后调用，传回结果
    /// </summary>
    public void OnMiniGameFinished(bool success)
    {
        string graphId = GameStateTracker.Instance.GetTempFlag("return_graph_id");
        string nodeId = success
            ? _currentNode.nextNodeId  // 成功走主线
            : GameStateTracker.Instance.GetTempFlag("return_node_id"); // 失败返回检查点

        // 重新加载原段落并跳转
        var graph = Resources.Load<NarrativeGraph>($"NarrativeGraphs/{graphId}");
        if (graph != null)
        {
            LoadAndPlayGraph(graph);
            if (!success)
            {
                PlayFromNode(nodeId); // 重试起点
            }
        }
    }
}