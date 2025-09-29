using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class MainMenuUI : MonoBehaviour
{
    [Header("���˵����")]
    public GameObject mainMenuPanel;          // ���˵����

    [Header("�������")]
    public GameObject settingsPanel;         // �������
    public Slider volumeSlider;              // �������������ɶԽ� AudioManager��

    // ���а�ť�� Awake ���Զ����¼�������ࣩ
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

        // ������尴ť
        Transform settings = settingsPanel?.transform;
        if (settings != null)
        {
            volumeSlider = settings.Find("VolumeSlider")?.GetComponent<Slider>();
            backButton = settings.Find("BackButton")?.GetComponent<Button>();
        }
        else
        {
            Debug.LogWarning("�������δ���䣬���޷������á�");
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
        if (volumeSlider != null)
        {
            volumeSlider.value = 0.7f; // Ĭ������
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }
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
        settingsPanel.SetActive(true);
    }

    /// <summary>
    /// �ر�������壬�������˵�
    /// </summary>
    private void OnCloseSettings()
    {
        settingsPanel.SetActive(false);
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
        settingsPanel?.SetActive(false);
    }

    /// <summary>
    /// ��ʾ�������
    /// </summary>
    public void ShowSettings()
    {
        mainMenuPanel?.SetActive(false);
        settingsPanel?.SetActive(true);
    }
}
