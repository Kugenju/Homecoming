// DialogueUI.cs
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    [Header("UI 引用")]
    public GameObject dialoguePanel;           // 对话面板（含文本框）
    public TMP_Text dialogueText;                  // 台词文本
    public TMP_Text nameText;                     // 说话人姓名文本
    public GameObject choicesPanel;            // 选项容器
    public Button choiceButtonPrefab;          // 选项按钮预制体（需在 Inspector 挂接）

    [Header("视觉表现")]
    public Image background;                   // 背景图
    public Image[] characterSlots;             // 角色立绘槽位（Left, Center, Right）

    private Queue<string> _linesQueue = new();
    private Action _onDialogueComplete;
    private List<Button> _spawnedButtons = new();

    void Start()
    {
        HideAll();
    }

    public void ShowDialogue(string[] lines, Action onComplete)
    {
        HideChoices();
        dialoguePanel.SetActive(true);

        _linesQueue = new Queue<string>(lines);
        _onDialogueComplete = onComplete;
        DisplayNextLine();
    }

    private void DisplayNextLine()
    {
        if (_linesQueue.Count == 0)
        {
            dialoguePanel.SetActive(false);
            _onDialogueComplete?.Invoke();
            return;
        }

        string line = _linesQueue.Dequeue();
        dialogueText.text = line;

        // 等待点击继续（简化版：自动延迟或监听输入）
        // 此处假设点击任意位置继续（实际可绑定按钮或 Input）
        StartCoroutine(WaitForClickOrAutoAdvance());
    }

    System.Collections.IEnumerator WaitForClickOrAutoAdvance()
    {
        // 方案1：等待玩家点击（推荐）
        // yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

        // 方案2：自动播放（调试用）
        yield return new WaitForSeconds(1.5f);

        DisplayNextLine();
    }

    public void ShowChoices(ChoiceOption[] options, Action<ChoiceOption> onSelect)
    {
        dialoguePanel.SetActive(false);
        choicesPanel.SetActive(true);

        // 清除旧按钮
        foreach (var btn in _spawnedButtons) Destroy(btn.gameObject);
        _spawnedButtons.Clear();

        // 创建新选项按钮
        foreach (var option in options)
        {
            Button btn = Instantiate(choiceButtonPrefab, choicesPanel.transform);
            btn.GetComponentInChildren<Text>().text = option.text;
            var localOption = option; // 避免闭包陷阱
            btn.onClick.AddListener(() =>
            {
                onSelect?.Invoke(localOption);
                HideChoices();
            });
            _spawnedButtons.Add(btn);
        }
    }

    private void HideChoices()
    {
        choicesPanel.SetActive(false);
        foreach (var btn in _spawnedButtons) Destroy(btn.gameObject);
        _spawnedButtons.Clear();
    }

    public void SetBackground(string backgroundAssetName)
    {
        if (string.IsNullOrEmpty(backgroundAssetName))
        {
            background.sprite = null;
            return;
        }

        // 从 Resources 或 Addressables 加载（此处用 Resources 简化）
        Sprite bgSprite = Resources.Load<Sprite>($"Backgrounds/{backgroundAssetName}");
        background.sprite = bgSprite;
    }

    public void SetCharacters(CharacterShow[] characters)
    {
        // 先清空所有槽位
        foreach (var slot in characterSlots) slot.sprite = null;

        if (characters == null) return;

        foreach (var charShow in characters)
        {
            int slotIndex = GetSlotIndex(charShow.position);
            if (slotIndex < characterSlots.Length)
            {
                // 加载角色立绘（命名规范：Character_WangChun_normal）
                string spriteName = $"Characters/{charShow.characterName}_{charShow.expression}";
                Sprite charSprite = Resources.Load<Sprite>(spriteName);
                characterSlots[slotIndex].sprite = charSprite;
            }
        }
    }

    private int GetSlotIndex(CharacterPosition pos)
    {
        return pos switch
        {
            CharacterPosition.Left => 0,
            CharacterPosition.Center => 1,
            CharacterPosition.Right => 2,
            _ => 1
        };
    }

    private void HideAll()
    {
        dialoguePanel?.SetActive(false);
        choicesPanel?.SetActive(false);
    }
}