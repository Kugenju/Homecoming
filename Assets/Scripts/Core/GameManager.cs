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
    // private float gameStartTime = 8.0f; // 游戏开始时间
    // private float gameEndTime = 18.0f; // 游戏结束时间

    // 添加对PauseMenuUI的引用
    private PauseMenuUI pauseMenuUI;

    protected override void Awake()
    {
        base.Awake();
        Debug.Log("GameManager 初始化");
        // 初始化代码
        DontDestroyOnLoad(this.gameObject); // 保持 GameManager 不被销毁

        // 获取当前场景名称并设置对应状态
        string currentSceneName = SceneManager.GetActiveScene().name;
        if (currentSceneName == mainMenuScene)
        {
            currentState = GameState.MainMenu;
            Debug.Log("当前状态设置为：主菜单");
        }
        else if (currentSceneName == gameScene_1)
        {
            currentState = GameState.Playing;
            Debug.Log("当前状态设置为：游戏中");
        }
        else
        {
            Debug.LogWarning($"未知场景：{currentSceneName}，保持默认状态");
        }

        Debug.Log(GameManager.Instance);

    }

    //private void onsceneloaded(scene scene, loadscenemode mode)
    //{
    //    // 在游戏场景中查找pausemenuui
    //    if (currentstate == gamestate.playing)
    //    {
    //        pausemenuui = findobjectoftype<pausemenuui>();
    //        if (pausemenuui != null)
    //        {
    //            debug.log("找到pausemenuui");
    //        }
    //    }
    //    else
    //    {
    //        pausemenuui = null;
    //    }
    //}

    // Start is called before the first frame update
    void Start()
    {
        currentState = GameState.MainMenu;
        Cursor.visible = true; // 确保鼠标可见
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //Debug.Log("检测到 ESC 键按下");
            HandleEscapeKey();
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

    /// <summary>
    /// 统一处理ESC键按下事件
    /// </summary>
    private void HandleEscapeKey()
    {
        Debug.Log("处理 ESC 键事件，当前状态: " + currentState);
        switch (currentState)
        {
            case GameState.Playing:
                // 游戏中按下ESC，尝试获取或创建暂停菜单
                if (pauseMenuUI == null)
                {
                    pauseMenuUI = FindObjectOfType<PauseMenuUI>();
                    Debug.Log("尝试查找PauseMenuUI" + pauseMenuUI);
                }

                if (pauseMenuUI != null)
                {
                    pauseMenuUI.Show();
                    currentState = GameState.Paused;
                }
                else
                {
                    // 如果没有找到暂停菜单，使用GameManager的默认暂停逻辑
                    TogglePause();
                }
                break;

            case GameState.Paused:
                // 暂停状态下按下ESC，如果有设置面板先关闭设置面板
                if (pauseMenuUI != null)
                {
                    // 检查是否有活动的设置面板
                    if (pauseMenuUI.IsSettingsPanelActive())
                    {
                        pauseMenuUI.CloseSettings();
                    }
                    else
                    {
                        pauseMenuUI.Hide();
                        currentState = GameState.Playing;
                    }
                }
                else
                {
                    // 如果没有暂停菜单，使用GameManager的默认恢复逻辑
                    TogglePause();
                }
                break;
        }
    }


    // ------------------------------
    // 外部可调用的获取方法
    // ------------------------------

    public bool IsGamePlaying() => currentState == GameState.Playing;
    public bool IsPaused() => currentState == GameState.Paused;
    public bool IsInMainMenu() => currentState == GameState.MainMenu;

    /// <summary>
    /// 设置PauseMenuUI引用（供PauseMenuUI自身调用）
    /// </summary>
    public void RegisterPauseMenuUI(PauseMenuUI ui)
    {
        pauseMenuUI = ui;
        //Debug.Log("注册PauseMenuUI引用" + pauseMenuUI);
    }
}
