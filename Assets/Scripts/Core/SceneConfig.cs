// SceneConfig.cs
using UnityEngine;

[CreateAssetMenu(fileName = "New Scene Config", menuName = "Game/Scene Config")]
public class SceneConfig : ScriptableObject
{
    public string sceneName;
    public GameMode mode;
    public int chapterIndex = -1; // 仅剧情线使用，-1 表示不适用

    [TextArea] public string description;
}