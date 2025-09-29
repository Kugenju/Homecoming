using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
/// <summary>
/// ��Ϸ���Ĺ����������𳡾��л�����Ϸ״̬��ʱ������ȫ�ֿ���
/// </summary>

public class GameManager : Singleton<GameManager>
{
    //  ��Ϸ״̬ö��
    public enum GameState
    {
        MainMenu,   // ���˵�
        Playing,    // ��Ϸ��
        Paused,     // ��ͣ
        GameOver    // ��Ϸ����
    }

    [Header("��������")]
    public string mainMenuScene = "MainMenu"; // ���˵���������
    public string gameScene_1 = "GameScene_1";         // ��Ϸ��������

    [Header("��ǰ״̬")]
    public GameState currentState = GameState.MainMenu;

    private bool isChangingScene = false; // �Ƿ������л�����

    // ��Ϸ��ʱ�䣨TimeSystem���ã�
    private float gameStartTime = 8.0f; // ��Ϸ��ʼʱ��
    private float gameEndTime = 18.0f; // ��Ϸ����ʱ��


    protected override void Awake()
    {
        base.Awake();
        Debug.Log("GameManager ��ʼ��");
        // ��ʼ������
        DontDestroyOnLoad(this.gameObject); // ���� GameManager ��������
        Debug.Log(GameManager.Instance);
    }

    // Start is called before the first frame update
    void Start()
    {
        currentState = GameState.MainMenu;
        Cursor.visible = true; // ȷ�����ɼ�
    }

    // Update is called once per frame
    void Update()
    {
        if (currentState == GameState.Playing && Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    // ------------------------------
    // �����л�����
    // ------------------------------

    /// <summary>
    /// �������˵�����
    /// </summary>
    public void LoadMainMenu()
    {
        if(isChangingScene) return;
        StartCoroutine(LoadSceneAsync(mainMenuScene, GameState.MainMenu));
    }

    /// <summary>
    /// ��ʼ��Ϸ��������Ϸ������
    /// </summary>
    public void StartGame()
    {
        if(isChangingScene) return;
        StartCoroutine(LoadSceneAsync(gameScene_1, GameState.Playing));
    }

    /// <summary>
    /// �첽���س���Э��
    /// </summary>
    private IEnumerator LoadSceneAsync(string sceneName, GameState newState)
    {
        isChangingScene = true;
        Debug.Log($"���ڼ��س���: {sceneName}");
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncOperation.isDone)
        {
            float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f);
            Debug.Log($"���ؽ���: {progress * 100:F1}%");
            yield return null;
        }
        currentState = newState;
        isChangingScene = false;
        Debug.Log($"���� {sceneName} ������ɣ���ǰ״̬: {currentState}");
        // ����������ɺ��ʼ��״̬
        if (newState == GameState.Playing)
        {
            OnGameSceneLoaded();
        }
    }

    // ------------------------------
    // ��Ϸ�߼�����
    // ------------------------------

    /// <summary>
    /// ��Ϸ����������ɺ�ĳ�ʼ��
    /// </summary>
    private void OnGameSceneLoaded()
    {
        Debug.Log("��Ϸ�����Ѽ��أ���ʼӪҵ��");
        Time.timeScale = 1f; // ȷ����Ϸ��������
        Cursor.visible = true;

        // �ɴ����¼�����Ϸ��ʼ
        // EventManager.TriggerEvent("OnGameStart");
    }


    /// <summary>
    /// ��ͣ/�ָ���Ϸ
    /// </summary>
    public void TogglePause()
    {
        if (currentState == GameState.Playing)
        {
            currentState = GameState.Paused;
            Time.timeScale = 0f;
            Cursor.visible = true;
            Debug.Log("��Ϸ����ͣ");
            // �ɼ����ͣUI��
        }
        else if (currentState == GameState.Paused)
        {
            currentState = GameState.Playing;
            Time.timeScale = 1f;
            Cursor.visible = false;
            Debug.Log("��Ϸ�ָ�");
            // �ɹرա���ͣUI��
        }
    }

    /// <summary>
    /// ������ǰ��Ϸ���������˵���
    /// </summary>
    public void EndGame()
    {
        currentState = GameState.GameOver;
        Debug.Log("����Ӫҵ������");
        // �ɲ��Ž������
        Invoke("LoadMainMenu", 2f); // 2��󷵻����˵�
    }

    /// <summary>
    /// �˳���Ϸ�����ڹ����汾����Ч��
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("�˳���Ϸ");
        #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }

    // ------------------------------
    // �ⲿ�ɵ��õĻ�ȡ����
    // ------------------------------

    public bool IsGamePlaying() => currentState == GameState.Playing;
    public bool IsPaused() => currentState == GameState.Paused;
    public bool IsInMainMenu() => currentState == GameState.MainMenu;
}
