// DialogueManager.cs
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    private NarrativeGraph _currentGraph;
    private NarrativeNode _currentNode;
    private Action<bool> _miniGameCallback; // 用于接收小游戏结果

    // UI 引用（需在 Inspector 挂接或通过 FindObjectOfType 获取）
    public DialogueUI dialogueUI; // 假设你有一个负责显示文本/选项的 UI 类
    private bool _isProcessingNode = false;
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
        Debug.Log($"Loaded narrative graph '{graph.graphId}' starting from node '{graph.startNodeId}'");
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
        if (_isProcessingNode)
        {
            Debug.LogError("Already processing a node! Skipping duplicate call.");
            return;
        }
        _isProcessingNode = true;
        // 更新背景、角色等（通知 UI 系统）
        UpdateVisuals(node);

        // 2. 执行背景缩放（如果启用）
        if (node.zoomBackgroundOnEnter)
        {
            StartCoroutine(ZoomBackgroundThen(() => ShowNodeContent(node)));
        }
        else
        {
            ShowNodeContent(node);
        }

        //switch (node.nodeType)
        //{
        //    case NarrativeNode.NodeType.Dialogue:
        //        ShowDialogue(node.lines, () =>
        //        {
        //            _isProcessingNode = false;
        //            if (!string.IsNullOrEmpty(node.nextNodeId))
        //                PlayFromNode(node.nextNodeId);
        //            else
        //                OnNarrativeCompleted(); // 剧情自然结束
        //        });
        //        break;

        //    case NarrativeNode.NodeType.Choice:
        //        ShowChoices(node.options, selectedOption =>
        //        {
        //            _isProcessingNode = false;
        //            PlayFromNode(selectedOption.targetNodeId);
        //        });
        //        break;

        //    case NarrativeNode.NodeType.MiniGame:
        //        StartMiniGame(node.miniGameName);
        //        break;

        //    case NarrativeNode.NodeType.Ending:
        //        TriggerEnding(node.nodeId);
        //        break;
        //    case NarrativeNode.NodeType.VisualBeat:
        //        ShowVisualBeat(() =>
        //        {
        //            _isProcessingNode = false;
        //            if (!string.IsNullOrEmpty(node.nextNodeId))
        //                PlayFromNode(node.nextNodeId);
        //            else
        //                OnNarrativeCompleted();
        //        });
        //        break;
        //}
    }

    private IEnumerator ZoomBackgroundThen(Action onComplete)
    {
        yield return StartCoroutine(dialogueUI.ZoomInBackground(
            _currentNode.zoomDuration,
            _currentNode.zoomScale
        ));
        onComplete?.Invoke();
    }

    private void ShowNodeContent(NarrativeNode node)
    {
        switch (node.nodeType)
        {
            case NarrativeNode.NodeType.Dialogue:
                ShowDialogue(node.lines, () =>
                {
                    if (node.autoAdvance)
                        StartCoroutine(DelayedAutoAdvance(node.autoAdvanceDelay));
                    else
                        OnNodeFinished(node);
                });
                break;

            case NarrativeNode.NodeType.Choice:
                ShowChoices(node.options, selectedOption =>
                {
                    _isProcessingNode = false;
                    PlayFromNode(selectedOption.targetNodeId);
                });
                break;

            case NarrativeNode.NodeType.MiniGame:
                StartMiniGame(node.miniGameName);
                break;

            case NarrativeNode.NodeType.Ending:
                TriggerEnding(node.nodeId);
                break;
            case NarrativeNode.NodeType.VisualBeat:
                ShowVisualBeat(() =>
                {
                    if (node.autoAdvance)
                        StartCoroutine(DelayedAutoAdvance(node.autoAdvanceDelay));
                    else
                        dialogueUI.WaitForClickToContinue(() =>
                        {
                            OnNodeFinished(node);
                        });
                });
                break;
        }
    }

    private void OnNodeFinished(NarrativeNode node)
    {
        _isProcessingNode = false;
        if (!string.IsNullOrEmpty(node.nextNodeId))
            PlayFromNode(node.nextNodeId);
        else
            OnNarrativeCompleted();
    }


    private IEnumerator DelayedAutoAdvance(float delay)
    {
        yield return new WaitForSeconds(delay);
        _isProcessingNode = false;
        if (!string.IsNullOrEmpty(_currentNode.nextNodeId))
            PlayFromNode(_currentNode.nextNodeId);
        else
            OnNarrativeCompleted();
    }

    private void UpdateVisuals(NarrativeNode node)
    {
        Debug.Log($"Updating visuals for node '{node.nodeId}'");
        dialogueUI?.SetActive();
        Debug.Log($"dialogueUI is {(dialogueUI != null ? "set" : "null")}");
        // 通知 UI 系统更新背景和角色
        dialogueUI?.SetBackground(node.backgroundSprite);
        dialogueUI?.SetCharacters(node.characters);
        if(node.nodeType == NarrativeNode.NodeType.VisualBeat)
        {
            dialogueUI?.HideDialogue();
            dialogueUI?.HideChoices();
        }
    }

    private void ShowDialogue(string[] lines, Action onComplete)
    {
        dialogueUI?.ShowDialogue(lines, onComplete);
    }

    private void ShowChoices(ChoiceOption[] options, Action<ChoiceOption> onSelect)
    {
        dialogueUI?.ShowChoices(options, onSelect);
    }

    public void BindDialogueUI(DialogueUI ui)
    {
        dialogueUI = ui;
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
        dialogueUI?.HideAll();
        OutcomeManager.Instance.TriggerEnding(endingId);
    }

    private void ShowVisualBeat(Action onComplete)
    {
        Debug.Log("Showing visual beat, waiting for player to continue...");
        onComplete?.Invoke();
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