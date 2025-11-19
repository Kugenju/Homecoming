using UnityEngine;
using UnityEngine.UI; // 引入UI命名空间

public class ReturnButton : MonoBehaviour
{
    private Button returnButton; // 按钮组件引用

    void Awake()
    {
        if (GameFlowController.Instance == null)
        {
            Debug.LogError("GameFlowController 未初始化！");
            return;
        }

        // 获取或添加Button组件
        returnButton = this.GetComponent<Button>();
        if (returnButton == null)
        {
            returnButton = gameObject.AddComponent<Button>();
            Debug.LogWarning("自动添加了Button组件", gameObject);
        }

        // 绑定点击事件
        returnButton.onClick.RemoveAllListeners();
        returnButton.onClick.AddListener(OnReturnClicked);
    }

    // 取消按钮点击事件处理
    private void OnReturnClicked()
    {
        Debug.Log("返回主页面");
        GameFlowController.Instance.EnterMainMenu();
        DialogueManager.Instance.SafetyReset();
    }
}