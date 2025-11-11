using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TurtleSoupUI : MonoBehaviour
{
    public TMP_InputField inputField;
    public TMP_Text chatLog;
    public ScrollRect scrollRect;

    private void OnEnable()
    {
        TurtleSoupGameManager.OnNewMessage += AppendMessage;
    }

    private void OnDisable()
    {
        TurtleSoupGameManager.OnNewMessage -= AppendMessage;
    }

    private void Start()
    {
        inputField.onSubmit.AddListener(OnSubmit);
    }

    private void OnSubmit(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        AppendMessage("<color=yellow>Íæ¼Ò£º</color>" + text);
        inputField.text = "";
        inputField.ActivateInputField();

        FindObjectOfType<TurtleSoupGameManager>()?.HandleUserInput(text);
    }

    private void AppendMessage(string msg)
    {
        chatLog.text += "\n" + msg;
        StartCoroutine(ScrollToBottom());
    }

    private System.Collections.IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();
        scrollRect.verticalNormalizedPosition = 0f;
    }
}