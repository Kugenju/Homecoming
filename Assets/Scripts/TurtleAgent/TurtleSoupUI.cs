using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

public class TurtleSoupUI : MonoBehaviour
{
    // --- 左侧面板 ---
    public TMP_Text soupSurfaceText;
    public TMP_Text rulesText;
    public Button startButton;

    // --- 右侧聊天面板 ---
    public TMP_InputField inputField;
    public Button sendButton;

    // 气泡预制体
    public GameObject aiMessagePrefab;
    public GameObject playerMessagePrefab;
    public Transform contentContainer;

    private TurtleSoupGameManager gameManager;

    private void Awake()
    {
        gameManager = FindObjectOfType<TurtleSoupGameManager>();
    }

    private void OnEnable()
    {
        TurtleSoupGameManager.OnNewMessage += AppendAIMessage;
        startButton.onClick.AddListener(OnStartButtonClick);
        sendButton.onClick.AddListener(() => OnSubmit(inputField.text)); // 注意这里！
    }

    private void OnDisable()
    {
        TurtleSoupGameManager.OnNewMessage -= AppendAIMessage;
        if (startButton != null)
            startButton.onClick.RemoveListener(OnStartButtonClick);
        sendButton.onClick.RemoveAllListeners(); // 或保存 Action 引用以精准移除
    }

    private void Start()
    {
        // 正确注册：OnSubmit 必须接受 string
        inputField.onSubmit.AddListener(OnSubmit);

        // 初始化规则
        rulesText.text = "现在请你通过提问，还原出故事的全貌。注意，问题的答案只能为'是’或‘否’，否则，我会拒绝回答。在你觉得已经还原全貌后，请对我说“我知道了。”我会问你两个问题，如果你全部答对，则通关；只要答错一个，你就输了。如果你问了十五个问题还没能对我说“我知道了。”那你也输啦！";
    }

    private void OnStartButtonClick()
    {
        // 调用 OnSubmit 并传入指令文本
        OnSubmit("开始游戏");
    }

    // ✅ 唯一的 OnSubmit 方法：接受 string
    private void OnSubmit(string text)
    {
        text = text.Trim();
        if (string.IsNullOrEmpty(text))
        {
            inputField.ActivateInputField(); // 保持聚焦
            return;
        }

        // 显示玩家消息（除非是内部指令如“开始游戏”）
        if (text != "开始游戏")
        {
            AppendPlayerMessage(text);
        }

        // 清空输入框
        inputField.text = "";
        inputField.ActivateInputField();

        // 发送给游戏逻辑
        gameManager?.HandleUserInput(text);
    }

    private void AppendPlayerMessage(string message)
    {
        GameObject msgObj = Instantiate(playerMessagePrefab, contentContainer);
        TMP_Text textComponent = msgObj.GetComponentInChildren<TMP_Text>();
        textComponent.text = message;

        // 手动恢复锚点（避免被父容器覆盖）
        RectTransform rectTransform = msgObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(1, 1);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.pivot = new Vector2(1, 1);

        StartCoroutine(ScrollToBottom());
    }

    private void AppendAIMessage(string message)
    {
        GameObject msgObj = Instantiate(aiMessagePrefab, contentContainer);
        TMP_Text textComponent = msgObj.GetComponentInChildren<TMP_Text>();
        textComponent.text = message;

        RectTransform rectTransform = msgObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
        rectTransform.pivot = new Vector2(0, 1);

        StartCoroutine(ScrollToBottom());
    }

    private IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();
        var scrollRect = contentContainer.parent.parent.GetComponent<ScrollRect>();
        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 0f;
    }

    public void UpdateSoupSurface(string soup)
    {
        soupSurfaceText.text = "汤面：\n" + soup;
    }
}