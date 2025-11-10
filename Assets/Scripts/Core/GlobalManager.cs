// GlobalManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalManager : Singleton<GlobalManager>
{
    [Header("≥°æ∞≈‰÷√")]
    public string mainMenuScene = "MainMenu";

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
        Debug.Log("[GlobalManager] ≥ı ºªØ");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void LoadMainMenu()
    {
        SceneLoader.Instance.LoadScene(mainMenuScene, GameMode.MainMenu);
    }
}