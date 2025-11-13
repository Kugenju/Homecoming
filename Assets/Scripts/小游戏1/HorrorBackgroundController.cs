using UnityEngine;
using System.Collections;

public class HorrorBackgroundRenderer : MonoBehaviour
{
    [Header("核心设置")]
    [Tooltip("需要控制的恐怖背景的SpriteRenderer组件")]
    public SpriteRenderer horrorRenderer; // 直接引用SpriteRenderer组件
    [Tooltip("是否在游戏开始后立即启用效果")]
    public bool startOnGameBegin = true;
    [Tooltip("是否一直显示恐怖背景")] // 新增参数
    public bool alwaysShow = false;

    [Header("随机间隔设置（秒）")]
    [Range(0.1f, 5f)] public float minInterval = 3f;
    [Range(5f, 60f)] public float maxInterval = 30f;

    [Header("闪现时长设置（秒）")]
    [Range(0.05f, 0.3f)] public float minShowTime = 0.1f;
    [Range(0.3f, 10f)] public float maxShowTime = 0.5f;

    [Header("特殊效果概率")]
    [Range(0f, 0.3f)] public float consecutiveChance = 0.15f; // 连续闪现概率
    [Range(2, 5)] public int maxConsecutiveCount = 3; // 最多连续闪现次数

    private Coroutine flashCoroutine;
    private bool isFlashing = false; // 防止重复激活的状态标记

    void Start()
    {
        // 初始化渲染状态（确保初始隐藏）
        if (horrorRenderer != null)
            horrorRenderer.enabled = false;

        if (startOnGameBegin)
            StartFlashing();
    }

    /// <summary>
    /// 开始闪现效果
    /// </summary>
    public void StartFlashing()
    {
        if (flashCoroutine == null && horrorRenderer != null)
            flashCoroutine = StartCoroutine(FlashCycle());
    }

    /// <summary>
    /// 停止闪现效果
    /// </summary>
    public void StopFlashing()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }

        // 强制隐藏
        if (horrorRenderer != null)
            horrorRenderer.enabled = false;
        
        isFlashing = false;
    }

    /// <summary>
    /// 闪现循环主逻辑
    /// </summary>
    private IEnumerator FlashCycle()
    {
        while (true)
        {
            if (!alwaysShow)
            {
                // 随机等待下次闪现的间隔（无规律感核心）
                float waitTime = Random.Range(minInterval, maxInterval);
                yield return new WaitForSeconds(waitTime);
            }

            // 随机触发连续闪现（增加不可预测性）
            bool isConsecutive = Random.value < consecutiveChance;
            int flashCount = isConsecutive ? Random.Range(2, maxConsecutiveCount + 1) : 1;

            // 执行闪现序列
            for (int i = 0; i < flashCount; i++)
            {
                // 显示：启用SpriteRenderer
                ShowHorrorSprite();
                
                // 随机显示时长（极短时间增强突然感）
                float showTime = Random.Range(minShowTime, maxShowTime);
                yield return new WaitForSeconds(showTime);

                // 隐藏：禁用SpriteRenderer
                HideHorrorSprite();

                // 连续闪现的间隔（比正常间隔短，制造急促感）
                if (i < flashCount - 1)
                {
                    float shortWait = Random.Range(0.08f, 0.2f); // 更短的间隔强化压迫感
                    yield return new WaitForSeconds(shortWait);
                }
            }
        }
    }

    /// <summary>
    /// 显示恐怖背景（启用渲染）
    /// </summary>
    private void ShowHorrorSprite()
    {
        if (horrorRenderer != null && !isFlashing)
        {
            isFlashing = true;
            horrorRenderer.enabled = true;
        }
    }

    /// <summary>
    /// 隐藏恐怖背景（禁用渲染）
    /// </summary>
    private void HideHorrorSprite()
    {
        if (horrorRenderer != null && isFlashing)
        {
            isFlashing = false;
            horrorRenderer.enabled = false;
        }
    }

    // 确保销毁时停止协程
    void OnDestroy()
    {
        StopFlashing();
    }

    // 新增方法：设置是否一直显示
    public void SetAlwaysShow(bool value)
    {
        alwaysShow = value;
        if (horrorRenderer != null)
        {
            horrorRenderer.enabled = value;
        }
    }
}