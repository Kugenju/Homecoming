using UnityEngine;
using System;

[CreateAssetMenu(fileName = "New Narrative Node", menuName = "Narrative/Narrative Node")]
public class NarrativeNode : ScriptableObject
{
    public enum NodeType
    {
        Dialogue,       // 普通对话
        Choice,         // 选项分支
        MiniGame,       // 触发小游戏
        Ending,          // 结局结算点
        VisualBeat
    }

    [Header("基础信息")]
    public string nodeId;               // 唯一ID，如 "school_01", "choice_after_liule"
    public NodeType nodeType = NodeType.Dialogue;

    [Header("对话内容")]
    [TextArea(3, 10)]
    public string[] lines;              // 支持多行台词

    [Header("视觉表现")]
    public Sprite backgroundSprite;      
    public CharacterShow[] characters;  // 显示的角色（见下方结构体）

    [Header("选项节点专用")]
    public ChoiceOption[] options;      // 若为 Choice 类型，此处有效

    [Header("小游戏节点专用")]
    public string miniGameName;           

    [Header("默认下一节点（线性推进用）")]
    public string nextNodeId;           // 自动跳转的下一个节点（选项节点可为空）

    [Header("Auto Visual Effects (Optional)")]
    public bool autoAdvance = false;                 // 是否自动跳转（不等点击）
    public float autoAdvanceDelay = 1f;              // 自动跳转前等待时间（秒）

    public bool zoomBackgroundOnEnter = false;       // 进入时是否放大背景
    public float zoomDuration = 2f;                  // 放大持续时间
    public float zoomScale = 1.2f;                   // 最终放大比例
}

// 角色显示配置（用于立绘/位置）
[Serializable]
public class CharacterShow
{
    public string characterName;        // 如 "WangChun"
    public CharacterPosition position;
    public Sprite characterSprite;            // 可选：如 "angry", "sad"
}

public enum CharacterPosition
{
    Left,
    Center,
    Right
}

// 选项数据
[Serializable]
public class ChoiceOption
{
    [TextArea] public string text;      // 选项文本
    public string targetNodeId;         // 选择后跳转的节点ID
    public string consequence;          // 可选：描述后果
}