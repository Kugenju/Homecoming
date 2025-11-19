using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SubMenu : MonoBehaviour
{
    [Header("二级菜单面板")]
    public GameObject subMenuPanel;          // 主菜单面板

    private GameObject currentSettingsPanel; // 当前实例化的设置面板
    private Button DailyButton;
    private Button StoryButton;

    void Awake()
    {
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
        ShowSubMenu();
    }

    private void FindUIElements()
    {
        // 主菜单按钮
        Transform subMenu = subMenuPanel?.transform;
        if (subMenu != null)
        {
            DailyButton = subMenu.Find("DailyButton")?.GetComponent<Button>();
            StoryButton = subMenu.Find("StoryButton")?.GetComponent<Button>();
        }
        else
        {
            Debug.LogError("主菜单面板未分配或为空！");
        }
    }

    private void SetupButtonListeners()
    {
        if (DailyButton != null)
        {
            DailyButton.onClick.AddListener(OnDailyButtonClicked);
        }
        else
        {
            Debug.LogError("DailyButton 未找到！");
        }
        if (StoryButton != null)
        {
            StoryButton.onClick.AddListener(OnStoryButtonClicked);
        }
        else
        {
            Debug.LogError("StoryButton 未找到！");
        }
    }

    private void OnDailyButtonClicked()
    {
        Debug.Log("点击了日常模式按钮");
        GameFlowController.Instance.EnterDailyLife();
    }

    private void OnStoryButtonClicked()
    {
        Debug.Log("点击了剧情模式按钮");
        GameFlowController.Instance.EnterStoryChapter(GameFlowController.Instance.currentStoryChapter); // 进入第一章剧情
    }

    private void ShowSubMenu()
    {
        subMenuPanel.SetActive(true);
    }
}
