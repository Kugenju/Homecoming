// DialogueUI.cs
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro;
using System.Collections;


public class DialogueUI : MonoBehaviour
{
    [Header("UI 引用")]
    public GameObject dialoguePanel;           // 对话面板（含文本框）
    public TMP_Text dialogueText;                  // 台词文本
    public TMP_Text nameText;                     // 说话人姓名文本
    public GameObject choicesPanel;            // 选项容器
    public Button choiceButtonPrefab;          // 选项按钮预制体（需在 Inspector 挂接）
    public Button nextButton;              // 下一步按钮

    [Header("视觉表现")]
    public Image background;                   // 背景图
    public Image[] characterSlots;             // 角色立绘槽位（Left, Center, Right）

    private bool _isWaitingForEndConfirmation = false;
    private bool _isPlayingDialogue = false;
    private Queue<string> _linesQueue = new();
    private Action _onDialogueComplete;
    private List<Button> _spawnedButtons = new();

    void Awake()
    {
        DialogueManager.Instance.dialogueUI = this;
        nextButton.onClick.AddListener(DisplayNextLine);
        //HideAll();
    }
    public void SetActive()
    {
        dialoguePanel.SetActive(true);
        choicesPanel.SetActive(false);
    }

    public void ShowDialogue(string[] lines, Action onComplete)
    {
        if (_isPlayingDialogue)
        {
            Debug.LogWarning("Dialogue is already playing! Ignoring new request.");
            return;
        }
        HideChoices();
        dialoguePanel.SetActive(true);
        _isPlayingDialogue = true;
        _isWaitingForEndConfirmation = false;
        _linesQueue = new Queue<string>(lines);
        _onDialogueComplete = onComplete;
        DisplayNextLine();
    }

    private void DisplayNextLine()
    {
        if (_isWaitingForEndConfirmation)
        {
            // 用户确认结束对话
            _isWaitingForEndConfirmation = false;
            _isPlayingDialogue = false;
            dialoguePanel.SetActive(false);
            _onDialogueComplete?.Invoke();
            return;
        }

        if (_linesQueue.Count == 0)
        {
            // 安全兜底
            dialoguePanel.SetActive(false);
            _onDialogueComplete?.Invoke();
            return;
        }

        string line = _linesQueue.Dequeue();
        dialogueText.text = line;

        if (_linesQueue.Count == 0)
        {
            // 已无更多台词，进入“等待确认结束”状态
            _isWaitingForEndConfirmation = true;
        }
        // 否则：继续等待下一次点击
    }

    System.Collections.IEnumerator WaitForClickOrAutoAdvance()
    {
        // 方案1：等待玩家点击（推荐）
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

        // 方案2：自动播放（调试用）
        //yield return new WaitForSeconds(1.5f);

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
            TMP_Text textComponent = btn.GetComponentInChildren<TMP_Text>();
            if (textComponent != null)
            {
                textComponent.text = option.text;
            }
            else
            {
                Debug.LogError("Choice button prefab is missing a TMP_Text component as child!");
            }

            var localOption = option;
            btn.onClick.AddListener(() =>
            {
                onSelect?.Invoke(localOption);
                HideChoices();
            });
            _spawnedButtons.Add(btn);
        }
    }

    public void HideChoices()
    {
        choicesPanel.SetActive(false);
        foreach (var btn in _spawnedButtons) Destroy(btn.gameObject);
        _spawnedButtons.Clear();
    }
    public void HideDialogue()
    {
        dialoguePanel.SetActive(false);
    }

    public void SetBackground(Sprite bgSprite)
    {
        background.sprite = bgSprite; // 直接赋值
    }


    public void SetCharacters(CharacterShow[] characters)
    {
        // Step 1: 先隐藏所有角色槽
        foreach (var slot in characterSlots)
        {
            if (slot != null)
            {
                slot.gameObject.SetActive(false); //
            }
        }

        // Step 2: 如果没有角色数据，直接返回
        if (characters == null)
            return;

        // Step 3: 遍历传入的角色，激活并设置对应槽位
        foreach (var charShow in characters)
        {
            if (charShow == null || charShow.characterSprite == null)
                continue;

            int slotIndex = GetSlotIndex(charShow.position);
            if (slotIndex >= 0 && slotIndex < characterSlots.Length)
            {
                Image slot = characterSlots[slotIndex];
                if (slot != null)
                {
                    slot.sprite = charShow.characterSprite;
                    slot.gameObject.SetActive(true); 

                    // 如果是 Center 槽位，更新说话人名字
                    if (slotIndex == 1)
                    {
                        nameText.text = !string.IsNullOrEmpty(charShow.characterName)
                            ? charShow.characterName
                            : "";
                        nameText.gameObject.SetActive(true);
                    }
                }
            }
        }

        // 可选：如果 Center 没有角色，也隐藏名字
        bool hasCenterChar = characters != null &&
            System.Array.Exists(characters, c => c != null && c.position == CharacterPosition.Center);
        if (!hasCenterChar)
        {
            nameText.gameObject.SetActive(false);
        }
    }

    public IEnumerator ZoomInBackground(float duration, float targetScale = 1.2f)
    {
        if (background == null) yield break;

        RectTransform bgRect = background.rectTransform;
        Vector3 startScale = Vector3.one;
        Vector3 endScale = new Vector3(targetScale, targetScale, 1f);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            bgRect.localScale = Vector3.Lerp(startScale, endScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        bgRect.localScale = endScale;
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

    public void WaitForClickToContinue(Action onComplete)
    {
        StartCoroutine(WaitForClickCoroutine(onComplete));
    }

    private System.Collections.IEnumerator WaitForClickCoroutine(Action onComplete)
    {
        // 确保 UI 已显示（背景已设置）
        while (!Input.GetMouseButtonDown(0) && !Input.GetKeyDown(KeyCode.Space))
        {
            yield return null;
        }
        onComplete?.Invoke();
    }

    public void HideAll()
    {
        dialoguePanel?.SetActive(false);
        choicesPanel?.SetActive(false);
    }
}