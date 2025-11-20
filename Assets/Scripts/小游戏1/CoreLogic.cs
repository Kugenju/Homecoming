using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using static MiniGameEvents;

public class CoreLogic : MonoBehaviour
{
    [Header("核心目标对象与Sprite列表")]
    [Tooltip("第一个等价对象")]
    public GameObject targetObject1;
    public List<Sprite> spriteList1;  // 索引0=默认，1=突变1，2=突变2

    [Tooltip("第二个等价对象")]
    public GameObject targetObject2;
    public List<Sprite> spriteList2;  // 索引0=默认，1=突变1，2=突变2

    [Header("触发显示对象列表")]
    [Tooltip("当index从1→0时显示[0]，从2→0时显示[1]")]
    public List<GameObject> triggerObjects;  // 需至少包含2个对象

    [Header("时间参数")]
    [Range(4f, 8f)] public float minInterval = 4f;    // 切换间隔最小值
    [Range(4f, 8f)] public float maxInterval = 8f;    // 切换间隔最大值
    public float activeDuration = 0.5f;               // index=1/2的持续时间
    public float triggerShowDuration = 1f;            // 触发对象显示时长

    [Header("引用设置")]
    [Tooltip("关联的按钮逻辑组件")]
    public ButtonLogic buttonLogic;  // 需要在Inspector中拖入ButtonLogic组件
    [Tooltip("关联背景渲染器组件")]
    public HorrorBackgroundRenderer horrorRenderer; // 关联背景渲染器

    private int index = 0;                            // 核心索引（0/1/2）
    private int totalCounter = 0;                     // 总计数器
    private SpriteRenderer renderer1;                 // 目标1渲染器
    private SpriteRenderer renderer2;                 // 目标2渲染器
    private List<SpriteRenderer> triggerRenderers;    // 触发对象的渲染器列表

    [Header("胜利条件设置")]
    public int winCondition = 6;  // 默认值为6

    [Header("坏结局背景")]
    public SpriteRenderer badEndingRenderer;  // 坏结局背景渲染器
    [Header("坏结局动画参数")]
    public float badEndingDelay = 1f; // 延迟显示时间
    public float fadeInDuration = 3f; // 淡入持续时间

    [Tooltip("恐怖音效")]
    public AudioClip horrorSound;


    void Start()
    {
        // 初始化核心对象渲染器
        renderer1 = targetObject1?.GetComponent<SpriteRenderer>();
        renderer2 = targetObject2?.GetComponent<SpriteRenderer>();

        // 初始化触发对象渲染器
        InitializeTriggerRenderers();

        // 检查配置合法性
        CheckSetupValidity();

        // 检查ButtonLogic引用
        if (buttonLogic == null)
        {
            Debug.LogError("未设置ButtonLogic引用！请在Inspector中关联ButtonLogic组件", this);
        }

        // 初始显示index=0的Sprite
        UpdateSpritesByIndex();

        // 启动索引切换循环
        StartCoroutine(IndexUpdateCycle());
    }

    /// <summary>
    /// 初始化触发对象的SpriteRenderer列表
    /// </summary>
    private void InitializeTriggerRenderers()
    {
        triggerRenderers = new List<SpriteRenderer>();
        if (triggerObjects == null) return;

        foreach (var obj in triggerObjects)
        {
            var renderer = obj?.GetComponent<SpriteRenderer>();
            triggerRenderers.Add(renderer);
            // 初始隐藏所有触发对象
            if (renderer != null)
                renderer.enabled = false;
        }
    }

    /// <summary>
    /// 检查配置是否合法，避免运行时错误
    /// </summary>
    private void CheckSetupValidity()
    {
        // 检查核心对象
        if (targetObject1 == null) Debug.LogError("未设置targetObject1！", this);
        if (targetObject2 == null) Debug.LogError("未设置targetObject2！", this);
        if (renderer1 == null) Debug.LogError("targetObject1缺少SpriteRenderer！", targetObject1);
        if (renderer2 == null) Debug.LogError("targetObject2缺少SpriteRenderer！", targetObject2);

        // 检查核心Sprite列表
        if (spriteList1.Count < 5) Debug.LogWarning("spriteList1需至少包含5个元素（索引0/1/2/3/4）", this);
        if (spriteList2.Count < 6) Debug.LogWarning("spriteList2需至少包含6个元素（索引0/1/2/3/4/5）", this);

        // 检查触发对象列表
        if (triggerObjects == null || triggerObjects.Count < 3)
        {
            Debug.LogError("triggerObjects需至少包含3个对象（索引0对应index=1，1对应index=2，2对应index=3）", this);
        }
        else
        {
            for (int i = 0; i < triggerObjects.Count; i++)
            {
                if (triggerObjects[i] == null)
                    Debug.LogError($"triggerObjects[{i}]未赋值！", this);
                else if (triggerRenderers[i] == null)
                    Debug.LogError($"triggerObjects[{i}]缺少SpriteRenderer！", triggerObjects[i]);
            }
        }

        // 检查按钮逻辑引用
        if (buttonLogic == null)
            Debug.LogError("未设置ButtonLogic引用！请在Inspector中关联ButtonLogic组件", this);
    }

    /// <summary>
    /// 索引切换主循环：随机间隔切换→显示突变→恢复默认→触发额外对象
    /// </summary>
    public IEnumerator IndexUpdateCycle()
    {
        while (true)
        {
            // 随机等待4-8秒（下次突变前的间隔）
            float waitTime = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(waitTime);
            // 检查音效和管理器是否有效
            if (horrorSound != null && MusicManager.Instance != null)
            {
                MusicManager.Instance.PlaySound(horrorSound);
            }
            else if (horrorSound == null)
            {
                Debug.LogWarning("未分配点击音效，请在Inspector中设置horrorSound");
            }

            // 当计数达到目标值时设置newIndex为3，否则随机1-2
            int newIndex;
            if (totalCounter >= winCondition)
            {
                newIndex = 4 - totalCounter + winCondition;  // 达到条件时强制设置
            }
            else
            {
                newIndex = Random.Range(1, 3);  // 普通情况随机1-2
            }
            index = newIndex;
            UpdateSpritesByIndex();

            // 保持突变状态0.5秒
            yield return new WaitForSeconds(activeDuration);

            // 【新增逻辑】即将恢复为0时，显示对应触发对象
            int triggerIndex = index - 1;  // index=1→0，index=2→1
            ShowTriggerObject(triggerIndex);
            
            // 【新增逻辑】检查按钮选中索引与当前index是否匹配
            if (CheckButtonAndIndexMatch()) {
                // 恢复为默认状态（index=0）
                index = 0;
                UpdateSpritesByIndex();
            }

            // 等待触发对象显示完毕（不阻塞主循环）
            yield return new WaitForSeconds(triggerShowDuration);
        }
    }

    /// <summary>
    /// 检查ButtonLogic选中索引与当前index是否匹配
    /// </summary>
    private bool CheckButtonAndIndexMatch()
    {
        if (buttonLogic == null)
        {
            Debug.LogError("ButtonLogic引用为空，无法进行索引匹配检查", this);
            return false;
        }

        // 检查选中的按钮索引是否与当前核心index-1相等
        if (buttonLogic.selectedButtonIndex == index-1)
        {
            if (totalCounter == winCondition)
            {
                Debug.Log("Win");
                OnPlayerWin();
            }
            totalCounter++;
            Debug.Log($"索引匹配成功！当前总计数: {totalCounter}");
            return true;

            // 或许这里有一个跳转页面的东西
        }
        else
        {
            Debug.LogWarning($"索引不匹配！ButtonLogic选中索引: {buttonLogic.selectedButtonIndex}, CoreLogic当前index: {index}", this);

            if (totalCounter == winCondition) {
                totalCounter++;
                return true;
            }
            // 索引不匹配时的处理逻辑
            // 停止CoreLogic中所有协程
            StopAllCoroutines();

            // 设置renderer2显示第五个sprite
            if (renderer2 != null && spriteList2.Count > 5)
            {
                renderer2.sprite = spriteList2[5];
            }
            
            // 设置HorrorBackgroundRenderer一直显示
            if (horrorRenderer != null)
            {
                horrorRenderer.SetAlwaysShow(true);
            }

            // 停止按钮事件
            if (buttonLogic != null)
            {
                buttonLogic.isEnd = true;
            }
            
            // 延迟后开始坏结局渲染器淡入
            StartCoroutine(StartBadEndingFade());
        }
        return false;
    }

    /// <summary>
    /// 显示指定索引的触发对象，并在指定时间后隐藏
    /// </summary>
    private void ShowTriggerObject(int triggerIndex)
    {
        // 检查索引合法性
        if (triggerRenderers == null || triggerIndex < 0 || triggerIndex >= triggerRenderers.Count)
            return;

        var targetRenderer = triggerRenderers[triggerIndex];
        if (targetRenderer == null) return;

        // 先隐藏所有触发对象（避免重叠显示）
        HideAllTriggerObjects();

        // 显示当前触发对象
        targetRenderer.enabled = true;

        // 启动协程，1秒后隐藏
        StartCoroutine(HideTriggerAfterDelay(targetRenderer, triggerShowDuration));
    }

    /// <summary>
    /// 延迟隐藏指定的触发对象
    /// </summary>
    private IEnumerator HideTriggerAfterDelay(SpriteRenderer renderer, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (renderer != null)
            renderer.enabled = false;
    }

    /// <summary>
    /// 隐藏所有触发对象
    /// </summary>
    private void HideAllTriggerObjects()
    {
        if (triggerRenderers == null) return;
        foreach (var renderer in triggerRenderers)
        {
            if (renderer != null)
                renderer.enabled = false;
        }
    }

    /// <summary>
    /// 根据当前index更新核心对象的Sprite
    /// </summary>
    private void UpdateSpritesByIndex()
    {
        // 更新第一个对象
        if (renderer1 != null && spriteList1.Count > index)
            renderer1.sprite = spriteList1[index];

        // 更新第二个对象
        if (renderer2 != null && spriteList2.Count > index)
            renderer2.sprite = spriteList2[index];
    }

    private IEnumerator StartBadEndingFade()
    {
        yield return new WaitForSeconds(badEndingDelay);
        StartCoroutine(FadeInBadEnding());
    }

    // 新增淡入协程
    private IEnumerator FadeInBadEnding()
    {
        if (badEndingRenderer == null) yield break;

        badEndingRenderer.enabled = true;

        // 确保初始状态完全透明且可见
        Color color = badEndingRenderer.color;
        color.a = 0f;
        badEndingRenderer.color = color;
        badEndingRenderer.enabled = true;

        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
            color.a = alpha;
            badEndingRenderer.color = color;
            yield return null;
        }

        // 确保最终完全不透明
        color.a = 1f;
        badEndingRenderer.color = color;

        // 或许这里有跳转逻辑
        OnPlayerLose();
    }

    // 编辑器调试信息
    private void OnDrawGizmos()
    {
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.cyan;
        style.fontSize = 12;
        style.wordWrap = true;

        string info = $"当前index: {index}\n";
        info += $"总计数器: {totalCounter}\n";
        info += $"核心对象显示列表索引{index}的Sprite\n";
        info += $"触发对象状态: 共{triggerObjects?.Count ?? 0}个";

        //UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, info, style);
    }

    public void OnPlayerWin() => MiniGameEvents.OnMiniGameFinished?.Invoke(true);
    public void OnPlayerLose() => MiniGameEvents.OnMiniGameFinished?.Invoke(false);
}