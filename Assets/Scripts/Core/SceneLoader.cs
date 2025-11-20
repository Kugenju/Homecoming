// SceneLoader.cs
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : Singleton<SceneLoader>
{
    public delegate void SceneLoadCallback();
    public static event SceneLoadCallback OnSceneLoaded;

    public int visitCount = -1;

    private AsyncOperation currentOperation;

    public void LoadScene(string sceneName, GameMode mode)
    {
        if (currentOperation != null && !currentOperation.isDone)
        {
            Debug.LogWarning("场景正在加载中，忽略新请求");
            return;
        }
        if (sceneName == "Cookie") visitCount++;

        StopAllCoroutines();
        StartCoroutine(LoadSceneAsync(sceneName, mode));
    }

    private IEnumerator LoadSceneAsync(string sceneName, GameMode mode)
    {
        Debug.Log($"[SceneLoader] 开始加载场景: {sceneName}");
        currentOperation = SceneManager.LoadSceneAsync(sceneName);

        // 允许在加载完成前返回控制权（用于显示加载UI）
        while (!currentOperation.isDone)
        {
            yield return null;
        }

        // 通知流程控制器：场景已切换
        GameFlowController.Instance.OnSceneLoaded(mode);
        OnSceneLoaded?.Invoke();

        Debug.Log($"[SceneLoader] 场景 {sceneName} 加载完成");
    }
}