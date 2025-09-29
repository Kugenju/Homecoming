using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class MainMenuUI : MonoBehaviour
{
    [Header("���˵����")]
    public GameObject mainMenuPanel;          // ���˵����

    [Header("Settings Prefab")]
    public GameObject settingsPrefab;     // SettingsPanel.prefab
    public Transform settingsContainer;             // �������������ɶԽ� AudioManager��

    // ���а�ť�� Awake ���Զ����¼�������ࣩ
    private GameObject currentSettingsPanel; // ��ǰʵ�������������
    private Button startButton;
    private Button settingsButton;
    private Button backButton;
    private Button quitButton;

    private void Awake()
    {
        // ��ȡ GameManager��ȷ�����ڣ�
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager δ�ҵ�����ȷ���������� GameManager ����");
            return;
        }

        // �������� UI Ԫ��
        FindUIElements();

        // ��ʼ���¼�����
        SetupButtonListeners();

        // Ĭ����ʾ���˵��������������
        ShowMainMenu();
    }

    private void FindUIElements()
    {
        // ���˵���ť
        Transform mainMenu = mainMenuPanel?.transform;
        if (mainMenu != null)
        {
            startButton = mainMenu.Find("StartButton")?.GetComponent<Button>();
            settingsButton = mainMenu.Find("SettingsButton")?.GetComponent<Button>();
            quitButton = mainMenu.Find("QuitButton")?.GetComponent<Button>();
        }
        else
        {
            Debug.LogError("���˵����δ�����Ϊ�գ�");
        }

        if (currentSettingsPanel == null)
        {
            currentSettingsPanel = Instantiate(settingsPrefab, settingsContainer);
            currentSettingsPanel.name = "SettingsPanel_Runtime";
            backButton = currentSettingsPanel.transform.Find("BackButton")?.GetComponent<Button>();
        }
        else
        {
            currentSettingsPanel.SetActive(true);
        }
    }

    // ------------------------------
    // ��ť�¼���
    // ------------------------------

    private void SetupButtonListeners()
    {
        if (startButton != null)
            startButton.onClick.AddListener(OnStartGame);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnOpenSettings);

        if (backButton != null)
            backButton.onClick.AddListener(OnCloseSettings);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitGame);

        // ������������ʾ����
    }
    // ------------------------------
    // ��ť��Ӧ����
    // ------------------------------

    /// <summary>
    /// ��ʼ��Ϸ���л�����Ϸ����
    /// </summary>
    private void OnStartGame()
    {
        GameManager.Instance.StartGame();
    }

    /// <summary>
    /// ���������
    /// </summary>
    private void OnOpenSettings()
    {
        mainMenuPanel.SetActive(false);

        // ����ͬһ�߼�
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
    /// �ر�������壬�������˵�
    /// </summary>
    private void OnCloseSettings()
    {
        currentSettingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    /// <summary>
    /// �����仯�ص�
    /// </summary>
    private void OnVolumeChanged(float value)
    {
        // �ɶԽ� AudioManager
        // AudioManager.Instance?.SetVolume(value);
        Debug.Log($"��������Ϊ: {value:F2}");
    }

    /// <summary>
    /// �˳���Ϸ
    /// </summary>
    private void OnQuitGame()
    {
        GameManager.Instance.QuitGame();
    }

    // ------------------------------
    // ��������
    // ------------------------------

    /// <summary>
    /// ��ʾ���˵�
    /// </summary>
    public void ShowMainMenu()
    {
        mainMenuPanel?.SetActive(true);
        currentSettingsPanel?.SetActive(false);
    }

    /// <summary>
    /// ��ʾ�������
    /// </summary>
    public void ShowSettings()
    {
        mainMenuPanel?.SetActive(false);
        currentSettingsPanel?.SetActive(true);
    }
}
