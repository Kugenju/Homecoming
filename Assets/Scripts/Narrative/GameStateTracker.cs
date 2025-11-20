// GameStateTracker.cs
using UnityEngine;
using System.Collections.Generic;

public class GameStateTracker : MonoBehaviour
{
    public static GameStateTracker Instance { get; private set; }

    [System.Serializable]
    public class StudentState
    {
        public string name;
        public int dangerLevel = 0; // 0 ~ 4+
    }

    [Header("持久状态")]
    public List<StudentState> students = new();
    public HashSet<string> unlockedEndings = new();

    [Header("临时标记（用于跨场景传递）")]
    private Dictionary<string, string> _tempFlags = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeStudents();
    }

    private void InitializeStudents()
    {
        // 根据你的剧情，预设学生名单
        string[] studentNames = { "WangChun", "LiuLe", "LiHaotian", "ZhaoYan", "SunRui" };
        foreach (var name in studentNames)
        {
            if (!students.Exists(s => s.name == name))
                students.Add(new StudentState { name = name });
        }
    }

    // ———————— 危险值管理 ————————
    public void IncreaseDanger(string studentName, int amount = 1)
    {
        var student = students.Find(s => s.name == studentName);
        if (student != null)
        {
            student.dangerLevel += amount;
            Debug.Log($"[Danger] {studentName} → {student.dangerLevel}");
        }
    }

    public int GetDanger(string studentName)
    {
        return students.Find(s => s.name == studentName)?.dangerLevel ?? 0;
    }

    public int CountStudentsWithDangerAtLeast(int threshold)
    {
        return students.FindAll(s => s.dangerLevel >= threshold).Count;
    }

    // ———————— 结局管理 ————————
    public void UnlockEnding(string endingId)
    {
        unlockedEndings.Add(endingId);
        Debug.Log($"[Ending Unlocked] {endingId}");
    }

    public bool HasUnlockedEnding(string endingId)
    {
        return unlockedEndings.Contains(endingId);
    }

    // ———————— 临时标记（用于小游戏结果、段落回溯等）——————
    public void SetTempFlag(string key, string value)
    {
        _tempFlags[key] = value;
    }

    public string GetTempFlag(string key)
    {
        _tempFlags.TryGetValue(key, out string value);
        return value;
    }

    public void ClearTempFlags()
    {
        _tempFlags.Clear();
    }

    // ———————— （可选）存档/读档接口 ————————
    // public void Save() { ... }
    // public void Load() { ... }
}