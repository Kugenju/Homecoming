using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NarrativeNode))]
public class NarrativeNodeEditor : Editor
{
    private SerializedProperty nodeId;
    private SerializedProperty nodeType;

    // Visual & Auto
    private SerializedProperty backgroundSprite;
    private SerializedProperty characters;
    private SerializedProperty nextNodeId;
    private SerializedProperty autoAdvance;
    private SerializedProperty autoAdvanceDelay;
    private SerializedProperty zoomBackgroundOnEnter;
    private SerializedProperty zoomDuration;
    private SerializedProperty zoomScale;

    // Dialogue
    private SerializedProperty lines;

    // Choice
    private SerializedProperty options;

    // MiniGame
    private SerializedProperty miniGameName;

    // Ending (just uses nodeId)

    private void OnEnable()
    {
        nodeId = serializedObject.FindProperty("nodeId");
        nodeType = serializedObject.FindProperty("nodeType");

        backgroundSprite = serializedObject.FindProperty("backgroundSprite");
        characters = serializedObject.FindProperty("characters");
        nextNodeId = serializedObject.FindProperty("nextNodeId");
        autoAdvance = serializedObject.FindProperty("autoAdvance");
        autoAdvanceDelay = serializedObject.FindProperty("autoAdvanceDelay");
        zoomBackgroundOnEnter = serializedObject.FindProperty("zoomBackgroundOnEnter");
        zoomDuration = serializedObject.FindProperty("zoomDuration");
        zoomScale = serializedObject.FindProperty("zoomScale");

        lines = serializedObject.FindProperty("lines");
        options = serializedObject.FindProperty("options");
        miniGameName = serializedObject.FindProperty("miniGameName");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(nodeId);
        EditorGUILayout.PropertyField(nodeType);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("视觉与背景", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(backgroundSprite);
        EditorGUILayout.PropertyField(characters);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("自动行为（可选）", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(zoomBackgroundOnEnter);
        if (zoomBackgroundOnEnter.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(zoomDuration);
            EditorGUILayout.PropertyField(zoomScale);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.PropertyField(autoAdvance);
        if (autoAdvance.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(autoAdvanceDelay);
            EditorGUI.indentLevel--;
        }

        // 根据节点类型显示特定内容
        NarrativeNode.NodeType type = (NarrativeNode.NodeType)nodeType.enumValueIndex;

        EditorGUILayout.Space(10);
        switch (type)
        {
            case NarrativeNode.NodeType.Dialogue:
                EditorGUILayout.LabelField("对话内容", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(lines);
                break;

            case NarrativeNode.NodeType.Choice:
                EditorGUILayout.LabelField("选项列表", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(options, true); // true = 显示子属性
                break;

            case NarrativeNode.NodeType.MiniGame:
                EditorGUILayout.LabelField("小游戏设置", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(miniGameName);
                break;

            case NarrativeNode.NodeType.Ending:
                EditorGUILayout.LabelField("结局节点", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("结局节点使用 nodeId 作为结局标识。确保 OutcomeManager 中有对应配置。", MessageType.Info);
                break;

            case NarrativeNode.NodeType.VisualBeat:
                EditorGUILayout.LabelField("视觉节拍（Visual Beat）", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("无对话、无选项，仅展示画面。配合自动行为实现过场。", MessageType.Info);
                break;
        }

        // 默认下一节点（Choice 节点通常不需要）
        if (type != NarrativeNode.NodeType.Choice)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("默认跳转", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(nextNodeId);
        }

        serializedObject.ApplyModifiedProperties();
    }
}