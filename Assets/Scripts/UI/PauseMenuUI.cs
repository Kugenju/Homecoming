using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ��Ϸ����ͣ�˵� UI
/// ���� SettingsPanel.prefab
/// </summary>
public class PauseMenuUI : MonoBehaviour
{

    [Header("UI Elements")]
    public GameObject pausePanel;          // ��ͣ�����
    public GameObject pauseMenu;        // ��ͣ�˵�������󣨰��������Ͱ�ť��
    public Button settingsButton;
    public Button resumeButton;
    public Button mainMenuButton;
    public Button quitButton;

    [Header("Settings Prefab")]
    public GameObject settingsPrefab;     // SettingsPanel.prefab
    public Transform settingsContainer;   // ��̬����λ��

    private GameObject currentSettingsPanel; // ��ǰʵ��

    private void Awake()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager δ�ҵ�����ȷ���������� GameManager ����");
            return;
        }

        // ��GameManagerע���Լ�
        GameManager.Instance.RegisterPauseMenuUI(this);

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
        GameManager.Instance.currentState = GameManager.GameState.Paused;
        //Cursor.visible = true;
    }

    public void Hide()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        GameManager.Instance.currentState = GameManager.GameState.Playing;
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
    /// �����������Ƿ񼤻�
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
        // ���������
        pauseMenu.SetActive(false);

        // ��̬�����������
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
        pauseMenu.SetActive(true); // ������ͣ�˵�
    }

    private void ReturnToMainMenu()
    {
        GameManager.Instance.LoadMainMenu();
    }

    private void QuitGame()
    {
        GameManager.Instance.QuitGame();
    }

}