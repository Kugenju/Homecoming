using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// 包子蒸笼：手动填满触发
/// 开盖 → 放入生包子（销毁 + 生成显示占位）→ 显示在蒸笼中 → 放够数量 → 自动关盖蒸煮 → 开盖 → 变为熟包子 → 拖走 → 重置
/// </summary>
[RequireComponent(typeof(Animator))]
public class BunSteamer : SteamerBase
{
    [Header("配置")]
    public GameObject rawBunPrefab;           // 用于生成“显示用”的生包子（视觉占位）
    public GameObject cookedBunPrefab;        // 熟包子预制体
    public int requiredBuns = 3;               // 需要多少个
    public float cookTime = 8f;

    [Header("包子生成位置与大小")]
    public Transform[] bunPositions;          // 预设位置锚点
    public Vector3 bunScale = Vector3.one;    // 包子缩放

    private List<GameObject> visualRawBuns = new List<GameObject>(); // 显示用的生包子
    private List<GameObject> cookedBuns = new List<GameObject>();   // 蒸好后的熟包子

    [Header("标签设置")]
    public string rawBunTag = "RawBun"; // 必须与生包子 Tag 一致

    protected override void Awake()
    {
        base.Awake();
        acceptTag = rawBunTag;
        onlyAcceptSpecificItems = false;
    }

    protected override void Start()
    {
        base.Start();
        currentState = State.Idle;
        isInteractable = false;
    }

    public override void OnFoodAdded(ClickableItem item)
    {
        if (currentState != State.Idle && currentState != State.Ready) return;

        if (item.CompareTag(rawBunTag))
        {
            // 🔴 第一步：销毁原始生包子
            Destroy(item.gameObject);
            Debug.Log($"✅ 原始生包子已销毁");

            // ✅ 第二步：在蒸笼中生成一个“显示用”的生包子
            SpawnVisualRawBun();

            // 检查是否已满
            if (visualRawBuns.Count >= requiredBuns)
            {
                Debug.Log("✅ 显示包子已满，开始蒸煮！");
                StartCooking(); // ✅ 触发关盖动画
            }
        }
    }

    /// <summary>
    /// 生成一个用于显示的生包子（视觉占位）
    /// </summary>
    private void SpawnVisualRawBun()
    {
        if (rawBunPrefab == null)
        {
            Debug.LogError("❌ rawBunPrefab 未赋值！");
            return;
        }

        Vector3 spawnPos = GetNextAvailablePosition();

        GameObject visualBun = Instantiate(rawBunPrefab, foodParent);
        visualBun.transform.localPosition = spawnPos;
        visualBun.transform.localScale = bunScale;

        var clickable = visualBun.GetComponent<ClickableItem>();
        if (clickable == null)
            clickable = visualBun.AddComponent<ClickableItem>();

        clickable.isDraggable = false;
        clickable.isUsable = false;

        visualRawBuns.Add(visualBun);

        Debug.Log($"已生成显示用生包子，当前数量: {visualRawBuns.Count}/{requiredBuns}");
    }

    public override void StartCooking()
    {
        Debug.Log("播放关盖动画");
        PlayCloseAnimation();     
        StartSteaming();          
        OnCookingStartEvent?.Invoke();
        currentState = State.Cooking;

        StartCoroutine(CookRoutine());
    }

    private IEnumerator CookRoutine()
    {
        yield return new WaitForSeconds(cookTime);
        OnCookingComplete();
    }

    protected override void OnCookingComplete()
    {
        StopSteaming();
        PlayOpenAnimation();
        currentState = State.Ready;
        isInteractable = true;

        
        foreach (var bun in visualRawBuns)
        {
            if (bun != null)
                Destroy(bun);
        }
        visualRawBuns.Clear();

        // 清理旧熟包子（防止叠加）
        foreach (var bun in cookedBuns)
        {
            if (bun != null)
                Destroy(bun);
        }
        cookedBuns.Clear();

        for (int i = 0; i < requiredBuns; i++)
        {
            Vector3 spawnPos;
            if (bunPositions != null && bunPositions.Length > 0)
            {
                spawnPos = bunPositions[i % bunPositions.Length].localPosition;
            }
            else
            {
                spawnPos = Random.insideUnitCircle * 0.2f;
            }

            GameObject bun = Instantiate(cookedBunPrefab, foodParent);
            bun.transform.localPosition = spawnPos;
            bun.transform.localScale = bunScale;

            var clickable = bun.GetComponent<ClickableItem>();
            if (clickable == null)
                clickable = bun.AddComponent<ClickableItem>();
            clickable.isDraggable = true;
            clickable.isUsable = true;

            //clickable.OnItemRemovedFromWorld.AddListener(() =>
            //{
            //    if (AreAllBunsTaken())
            //    {
            //        OnFoodTaken();
            //    }
            //});

            cookedBuns.Add(bun);
        }

        OnCookingCompleteEvent?.Invoke();
    }

    /// <summary>
    /// 获取下一个可用位置
    /// </summary>
    private Vector3 GetNextAvailablePosition()
    {
        int index = visualRawBuns.Count;
        if (bunPositions != null && bunPositions.Length > 0)
        {
            return bunPositions[index % bunPositions.Length].localPosition;
        }
        return Random.insideUnitCircle * 0.2f;
    }

    private bool AreAllBunsTaken()
    {
        foreach (var bun in cookedBuns)
        {
            if (bun != null) return false;
        }
        return true;
    }

    protected override void OnFoodTaken()
    {
        cookedBuns.Clear();
        currentState = State.Idle;
        isInteractable = false;
        Debug.Log("蒸笼已重置");
    }
}