using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
/// <summary>
/// 游戏核心管理器：负责场景切换、游戏状态、时间管理等全局控制
/// </summary>

public class GameManager : Singleton<GameManager>
{
    //  游戏状态枚举
    public enum GameState
    {
        MainMenu,   // 主菜单
        Playing,    // 游戏中
        Paused,     // 暂停
        GameOver    // 游戏结束
    }

    [Header("场景名称")]
    public string mainMenuScene = "MainMenu"; // 主菜单场景名称
    public string gameScene_1 = "GameScene_1";         // 游戏场景名称

    [Header("当前状态")]
    public GameState currentState = GameState.MainMenu;

    private bool isChangingScene = false; // 是否正在切换场景

    // 游戏内时间（TimeSystem调用）
    private float gameStartTime = 8.0f; // 游戏开始时间
    private float gameEndTime = 18.0f; // 游戏结束时间


    protected override void Awake()
    {
        base.Awake();
        Debug.Log("GameManager 初始化");
        // 初始化代码
        DontDestroyOnLoad(this.gameObject); // 保持 GameManager 不被销毁
        Debug.Log(GameManager.Instance);
    }

    // Start is called before the first frame update
    void Start()
    {
        currentState = GameState.MainMenu;
        Cursor.visible = true; // 确保鼠标可见
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
    // 场景切换方法
    // ------------------------------

    /// <summary>
    /// 加载主菜单场景
    /// </summary>
    public void LoadMainMenu()
    {
        if(isChangingScene) return;
        StartCoroutine(LoadSceneAsync(mainMenuScene, GameState.MainMenu));
    }

    /// <summary>
    /// 开始游戏（进入游戏场景）
    /// </summary>
    public void StartGame()
    {
        if(isChangingScene) return;
        StartCoroutine(LoadSceneAsync(gameScene_1, GameState.Playing));
    }

    /// <summary>
    /// 异步加载场景协程
    /// </summary>
    private IEnumerator LoadSceneAsync(string sceneName, GameState newState)
    {
        isChangingScene = true;
        Debug.Log($"正在加载场景: {sceneName}");
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncOperation.isDone)
        {
            float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f);
            Debug.Log($"加载进度: {progress * 100:F1}%");
            yield return null;
        }
        currentState = newState;
        isChangingScene = false;
        Debug.Log($"场景 {sceneName} 加载完成，当前状态: {currentState}");
        // 场景加载完成后初始化状态
        if (newState == GameState.Playing)
        {
            OnGameSceneLoaded();
        }
    }

    // ------------------------------
    // 游戏逻辑控制
    // ------------------------------

    /// <summary>
    /// 游戏场景加载完成后的初始化
    /// </summary>
    private void OnGameSceneLoaded()
    {
        Debug.Log("游戏场景已加载，开始营业！");
        Time.timeScale = 1f; // 确保游戏正常运行
        Cursor.visible = true;

        // 可触发事件：游戏开始
        // EventManager.TriggerEvent("OnGameStart");
    }


    /// <summary>
    /// 暂停/恢复游戏
    /// </summary>
    public void TogglePause()
    {
        if (currentState == GameState.Playing)
        {
            currentState = GameState.Paused;
            Time.timeScale = 0f;
            Cursor.visible = true;
            Debug.Log("游戏已暂停");
            // 可激活“暂停UI”
        }
        else if (currentState == GameState.Paused)
        {
            currentState = GameState.Playing;
            Time.timeScale = 1f;
            Cursor.visible = false;
            Debug.Log("游戏恢复");
            // 可关闭“暂停UI”
        }
    }

    /// <summary>
    /// 结束当前游戏（返回主菜单）
    /// </summary>
    public void EndGame()
    {
        currentState = GameState.GameOver;
        Debug.Log("今日营业结束！");
        // 可播放结算界面
        Invoke("LoadMainMenu", 2f); // 2秒后返回主菜单
    }

    /// <summary>
    /// 退出游戏（仅在构建版本中有效）
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("退出游戏");
        #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }

    // ------------------------------
    // 外部可调用的获取方法
    // ------------------------------

    public bool IsGamePlaying() => currentState == GameState.Playing;
    public bool IsPaused() => currentState == GameState.Paused;
    public bool IsInMainMenu() => currentState == GameState.MainMenu;
}
