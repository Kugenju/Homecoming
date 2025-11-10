using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class MainMenuUI : MonoBehaviour
{
    [Header("主菜单面板")]
    public GameObject mainMenuPanel;          // 主菜单面板

    [Header("Settings Prefab")]
    public GameObject settingsPrefab;     // SettingsPanel.prefab
    public Transform settingsContainer;             // 音量滑动条

    // 所有按钮在 Awake 中自动绑定事件
    private GameObject currentSettingsPanel; // 当前实例化的设置面板
    private Button startButton;
    private Button settingsButton;
    //private Button backButton;
    private Button quitButton;

    private void Awake()
    {
        // 获取 GameManager（确保存在）
        if (GameFlowController.Instance == null)
        {
            Debug.LogError("GameFlowController 未初始化！");
            return;
        }

        // 查找所有 UI 元素
        FindUIElements();

        // 初始化事件监听
        SetupButtonListeners();

        // 默认显示主菜单，隐藏设置面板
        ShowMainMenu();
    }

    private void FindUIElements()
    {
        // 主菜单按钮
        Transform mainMenu = mainMenuPanel?.transform;
        if (mainMenu != null)
        {
            startButton = mainMenu.Find("StartButton")?.GetComponent<Button>();
            settingsButton = mainMenu.Find("SettingsButton")?.GetComponent<Button>();
            quitButton = mainMenu.Find("QuitButton")?.GetComponent<Button>();
        }
        else
        {
            Debug.LogError("主菜单面板未分配或为空！");
        }

        if (currentSettingsPanel == null)
        {
            currentSettingsPanel = Instantiate(settingsPrefab, settingsContainer);
            currentSettingsPanel.name = "SettingsPanel_Runtime";
            //backButton = currentSettingsPanel.transform.Find("BackButton")?.GetComponent<Button>();
        }
        else
        {
            currentSettingsPanel.SetActive(true);
        }
    }

    // ------------------------------
    // 按钮事件绑定
    // ------------------------------

    private void SetupButtonListeners()
    {
        if (startButton != null)
            startButton.onClick.AddListener(OnStartGame);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnOpenSettings);

        //if (backButton != null)
        //    backButton.onClick.AddListener(OnCloseSettings);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitGame);

        // 音量滑动条（示例）
    }
    // ------------------------------
    // 按钮响应方法
    // ------------------------------

    /// <summary>
    /// 开始游戏：切换到游戏场景
    /// </summary>
    private void OnStartGame()
    {
        GameFlowController.Instance.EnterSubMenu();
    }

    /// <summary>
    /// 打开设置面板
    /// </summary>
    private void OnOpenSettings()
    {
        mainMenuPanel.SetActive(false);

        // 复用同一逻辑
        if (currentSettingsPanel == null)
        {
            currentSettingsPanel = Instantiate(settingsPrefab, settingsContainer);
            currentSettingsPanel.name = "SettingsPanel_Runtime";
            currentSettingsPanel.SetActive(true);
        }
        else
        {
            currentSettingsPanel.SetActive(true);
        }
    }

    /// <summary>
    /// 关闭设置面板，返回主菜单
    /// </summary>
    private void OnCloseSettings()
    {
        currentSettingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    /// <summary>
    /// 音量变化回调
    /// </summary>
    private void OnVolumeChanged(float value)
    {
        // 可对接 AudioManager
        // AudioManager.Instance?.SetVolume(value);
        Debug.Log($"音量调整为: {value:F2}");
    }

    /// <summary>
    /// 退出游戏
    /// </summary>
    private void OnQuitGame()
    {
        GameFlowController.Instance.ExitGame();
    }

    // ------------------------------
    // 辅助方法
    // ------------------------------

    /// <summary>
    /// 显示主菜单
    /// </summary>
    public void ShowMainMenu()
    {
        mainMenuPanel?.SetActive(true);
        currentSettingsPanel?.SetActive(false);
    }

    /// <summary>
    /// 显示设置面板
    /// </summary>
    public void ShowSettings()
    {
        mainMenuPanel?.SetActive(false);
        currentSettingsPanel?.SetActive(true);
    }

    public void MainSetActive()
    {
        mainMenuPanel.SetActive(true);
    }
}
