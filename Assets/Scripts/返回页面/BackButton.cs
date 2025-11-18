using UnityEngine;
using UnityEngine.UI; // 引入UI命名空间

public class BackButton : MonoBehaviour
{
    [Header("暂停相关设置")]
    public GameObject blurLayer; // 关联模糊层对象
    public bool isPaused = false; // 暂停状态

    private Button backButton; // 按钮组件引用


    void Awake()
    {
        // 获取或添加Button组件
        backButton = this.GetComponent<Button>();
        if (backButton == null)
        {
            backButton = gameObject.AddComponent<Button>();
            Debug.LogWarning("自动添加了Button组件", gameObject);
        }

        // 绑定点击事件
        backButton.onClick.RemoveAllListeners();
        backButton.onClick.AddListener(OnBackClicked);

        // 初始化模糊层状态
        blurLayer?.SetActive(isPaused);
    }

    // 点击处理逻辑
    private void OnBackClicked()
    {
        isPaused = !isPaused;

        if (blurLayer != null)
        {
            blurLayer.SetActive(isPaused);
        }
        else
        {
            Debug.LogWarning("未设置模糊层对象（blurLayer）！");
        }

        Time.timeScale = isPaused ? 0 : 1;
    }
}