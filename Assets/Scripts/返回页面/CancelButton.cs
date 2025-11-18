using UnityEngine;
using UnityEngine.UI; // 引入UI命名空间

public class CancelButton : MonoBehaviour
{
    [Header("暂停相关设置")]
    public GameObject blurLayer; // 关联模糊层对象
    public BackButton backButton; // 返回按钮

    private Button cancelButton; // 按钮组件引用

    void Awake()
    {
        // 获取或添加Button组件
        cancelButton = this.GetComponent<Button>();
        if (cancelButton == null)
        {
            cancelButton = gameObject.AddComponent<Button>();
            Debug.LogWarning("自动添加了Button组件", gameObject);
        }

        // 绑定点击事件
        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(OnCancelClicked);
    }

    // 取消按钮点击事件处理
    private void OnCancelClicked()
    {
        blurLayer?.SetActive(false);
        if (backButton != null)
        {
            backButton.isPaused = false;
            // 如果需要恢复时间流逝，可以在这里补充
            Time.timeScale = 1;
        }
        else
        {
            Debug.LogWarning("未关联返回按钮（backButton）", this);
        }
    }
}