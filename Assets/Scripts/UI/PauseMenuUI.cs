using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 游戏内暂停菜单 UI
/// 复用 SettingsPanel.prefab
/// </summary>
public class PauseMenuUI : MonoBehaviour
{

    [Header("UI Elements")]
    public GameObject pausePanel;          // 暂停主面板
    public GameObject pauseMenu;        // 暂停菜单整体对象（包含背景和按钮）
    public Button settingsButton;
    public Button resumeButton;
    public Button mainMenuButton;
    public Button quitButton;

    [Header("Settings Prefab")]
    public GameObject settingsPrefab;     // SettingsPanel.prefab
    public Transform settingsContainer;   // 动态加载位置

    private GameObject currentSettingsPanel; // 当前实例

    private void Awake()
    {
        if (GameFlowController.Instance == null)
        {
            Debug.LogError("GameFlowController 未初始化！");
            return;
        }

        //// 向GameManager注册自己
        //GameManager.Instance.RegisterPauseMenuUI(this);

        SetupButtons();
        Hide();
    }

    private void SetupButtons()
    {
        resumeButton.onClick.AddListener(ResumeGame);
        settingsButton.onClick.AddListener(ShowSettings);
        mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        quitButton.onClick.AddListener(QuitGame);
    }

    public void Show()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        //GameManager.Instance.currentState = GameManager.GameState.Paused;
        //Cursor.visible = true;
    }

    public void Hide()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        //GameManager.Instance.currentState = GameManager.GameState.Playing;
        //Cursor.visible = false;
    }

    public void Toggle()
    {
        if (pausePanel.activeSelf)
            Hide();
        else
            Show();
    }

    /// <summary>
    /// 检查设置面板是否激活
    /// </summary>
    public bool IsSettingsPanelActive()
    {
        return currentSettingsPanel != null && currentSettingsPanel.activeSelf;
    }

    private void ResumeGame()
    {
        Hide();
    }

    private void ShowSettings()
    {
        // 隐藏主面板
        pauseMenu.SetActive(false);

        // 动态加载设置面板
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

    public void CloseSettings()
    {
        if (currentSettingsPanel != null)
        {
            currentSettingsPanel.SetActive(false);
        }
        pauseMenu.SetActive(true); // 返回暂停菜单
    }

    private void ReturnToMainMenu()
    {
        GameFlowController.Instance.ReturnToMainMenu();
    }

    private void QuitGame()
    {
        GameFlowController.Instance.ExitGame();
    }

}