using UnityEngine;
using System.Text;
using System.Collections;
using UnityEngine.Networking;

public class QwenAPI : MonoBehaviour
{
    public static QwenAPI Instance;
    // DashScope API 地址（文本生成）
    private const string API_URL = "https://dashscope.aliyuncs.com/api/v1/services/aigc/text-generation/generation";

    [Header("请在 Inspector 中填写你的 DashScope API Key")]
    public string apiKey = "YOUR_API_KEY_HERE"; // ← 必须替换！

    // 可选：是否在控制台打印请求/响应（调试用）
    public bool debugLog = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 可选：跨场景保留
        }
        else
        {
            Destroy(gameObject); // 防止重复
        }
    }

    // ----------------------------
    // 公共调用接口
    // ----------------------------

    /// <summary>
    /// 判断用户问题的是非（是/否/无关/拒绝）
    /// </summary>
    public IEnumerator JudgeYesNo(string userQuestion, string soupBottom, System.Action<string> callback)
    {
        if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_API_KEY_HERE")
        {
            Debug.LogError("[QwenAPI] 请先在 Inspector 中设置有效的 DashScope API Key！");
            callback?.Invoke("拒绝回答。");
            yield break;
        }

        string prompt = $@"你是一个严格的海龟汤裁判。请根据以下事实，仅用“是”或“否”回答用户的问题。
如果问题与事实无关，请回答“没有关系哦。”
如果问题不是是非疑问句，或无法用“是/否”回答，请回答“拒绝回答。”

【事实】
{soupBottom}

【用户问题】
{userQuestion}

【你的回答】
（只能输出以下之一：是 / 否 / 没有关系哦。 / 拒绝回答。）";

        var requestObj = new QwenRequest
        {
            model = "qwen-max",
            input = new Input
            {
                messages = new[]
                {
                    new Message { role = "user", content = prompt }
                }
            },
            parameters = new Parameters { max_tokens = 50, temperature = 0.1f }
        };

        string jsonPayload = JsonUtility.ToJson(requestObj);
        if (debugLog) Debug.Log($"[QwenAPI] 发送请求:\n{jsonPayload}");

        using (UnityWebRequest req = new UnityWebRequest(API_URL, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Authorization", $"Bearer {apiKey}");
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Accept", "application/json");

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                string responseText = req.downloadHandler.text;
                if (debugLog) Debug.Log($"[QwenAPI] 响应:\n{responseText}");

                string answer = ExtractAnswer(responseText);
                callback?.Invoke(answer);
            }
            else
            {
                string errorMsg = $"[QwenAPI] 请求失败: {req.error}";
                string responseText = req.downloadHandler?.text ?? "无响应内容";
                Debug.LogError(errorMsg);
                Debug.LogError($"[QwenAPI] 服务器返回:\n{responseText}");

                // 尝试解析 DashScope 的错误信息
                try
                {
                    var errorObj = JsonUtility.FromJson<ErrorResponse>(responseText);
                    if (!string.IsNullOrEmpty(errorObj?.message))
                    {
                        Debug.LogError($"[QwenAPI] 错误详情: {errorObj.message}");
                    }
                }
                catch { /* 忽略解析失败 */ }

                callback?.Invoke("拒绝回答。");
            }
        }
    }

    // ----------------------------
    // 工具方法：从响应中提取回答
    // ----------------------------

    private string ExtractAnswer(string jsonResponse)
    {
        try
        {
            var response = JsonUtility.FromJson<QwenResponse>(jsonResponse);
            if (!string.IsNullOrEmpty(response?.output?.text))
            {
                string content = response.output.text.Trim();

                // 严格模式：只接受完全匹配
                if (content == "是" || content == "否" ||
                    content == "没有关系哦。" || content == "拒绝回答。")
                {
                    return content;
                }

                // 宽松模式：尝试从句子中提取（可选）
                content = content.Replace(" ", "").Replace("。", "");
                if (content == "是") return "是";
                if (content == "否") return "否";
                if (content.Contains("没有关系")) return "没有关系哦。";
                if (content.Contains("拒绝") || content.Contains("不能") || content.Contains("无法"))
                    return "拒绝回答。";
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[QwenAPI] 解析失败: {ex.Message}\n{jsonResponse}");
        }
        return "拒绝回答。";
    }

    // ----------------------------
    // 数据结构定义（必须 [Serializable]）
    // ----------------------------

    [System.Serializable]
    private class QwenRequest
    {
        public string model;
        public Input input;
        public Parameters parameters;
    }

    [System.Serializable]
    private class Input
    {
        public Message[] messages;
    }

    [System.Serializable]
    private class Message
    {
        public string role;
        public string content;
    }

    [System.Serializable]
    private class Parameters
    {
        public int max_tokens;
        public float temperature; // 降低随机性
    }

    // 响应结构
    [System.Serializable]
    private class QwenResponse
    {
        public Output output;
    }

    [System.Serializable]
    private class Output
    {
        public string text;
        public string finish_reason;
    }

    [System.Serializable]
    private class Choice
    {
        public Message message;
    }

    // 错误响应结构（用于调试）
    [System.Serializable]
    private class ErrorResponse
    {
        public string code;
        public string message;
    }
}