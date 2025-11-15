// NarrativeGraph.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "New Narrative Graph", menuName = "Narrative/Narrative Graph")]
public class NarrativeGraph : ScriptableObject
{
    [Tooltip("段落唯一标识，如 'segment_visit_school'")]
    public string graphId;
    public int chapterNumber;

    [Tooltip("进入此段落时播放的第一个节点")]
    public string startNodeId;

    [Tooltip("包含的所有剧情节点")]
    public NarrativeNode[] nodes;

    // 运行时加速查找（编辑器中不序列化）
    private Dictionary<string, NarrativeNode> _nodeMap;

    public NarrativeNode GetNode(string nodeId)
    {
        if (_nodeMap == null)
        {
            _nodeMap = nodes.ToDictionary(n => n.nodeId, n => n);
        }
        return _nodeMap.TryGetValue(nodeId, out var node) ? node : null;
    }

    // 获取重试检查点（默认为段落起点）
    public string GetRestartCheckpoint()
    {
        return startNodeId;
    }
}