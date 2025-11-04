﻿using UnityEngine;
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
    public Vector3 Raw_bunScale = Vector3.one; // 生包子缩放

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
            // 第一步：销毁原始生包子
            Destroy(item.gameObject);
            Debug.Log($"✅ 原始生包子已销毁");

            // 第二步：在蒸笼中生成一个“显示用”的生包子
            SpawnVisualRawBun();

            // 检查是否已满
            if (visualRawBuns.Count >= requiredBuns)
            {
                Debug.Log("显示包子已满，开始蒸煮！");
                StartCooking();
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
            Debug.LogError("rawBunPrefab 未赋值！");
            return;
        }

        Vector3 spawnPos = GetNextAvailablePosition();

        GameObject visualBun = Instantiate(rawBunPrefab, foodParent);
        visualBun.transform.localPosition = spawnPos;
        visualBun.transform.localScale = Raw_bunScale;

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

        // 清理旧对象
        foreach (var bun in visualRawBuns) Destroy(bun);
        visualRawBuns.Clear();
        foreach (var bun in cookedBuns) Destroy(bun);
        cookedBuns.Clear();

        // ✅ 创建父对象
        GameObject groupBuns = new GameObject("CookedBuns_Group包子组");
        groupBuns.transform.SetParent(foodParent);
        groupBuns.transform.localPosition = Vector3.zero;
        groupBuns.transform.localScale = Vector3.one; // 假设为 (1,1,1)

        //clickable.OnItemRemovedFromWorld.AddListener(() =>
        //{
        //    OnFoodTaken();
        //});

        // ✅ 用于合并所有包子的实际世界空间包围盒
        Bounds worldBounds = new Bounds();

        bool first = true;

        for (int i = 0; i < requiredBuns; i++)
        {
            Vector3 localPos;
            if (bunPositions != null && bunPositions.Length > 0)
            {
                localPos = bunPositions[i % bunPositions.Length].localPosition;
            }
            else
            {
                localPos = Random.insideUnitCircle * 0.2f;
            }

            GameObject bun = Instantiate(cookedBunPrefab, groupBuns.transform);
            bun.transform.localPosition = localPos;
            bun.transform.localScale = bunScale; // 比如 (0.8, 0.8, 0.8)

            // ✅ 获取包子自身的包围体积（考虑缩放）
            Bounds bunWorldBounds = GetRendererOrColliderBounds(bun);

            if (first)
            {
                worldBounds = bunWorldBounds;
                first = false;
            }
            else
            {
                worldBounds.Encapsulate(bunWorldBounds);
            }

            // 移除子包子的 ClickableItem
            var childClickable = bun.GetComponent<ClickableItem>();
            if (childClickable != null)
            {
                Destroy(childClickable);
            }
        }

        // ✅ 现在 worldBounds 是所有包子的合并世界包围盒

        // ✅ 添加 BoxCollider 到父对象
        BoxCollider2D collider = groupBuns.AddComponent<BoxCollider2D>();

        // ✅ 转换为父物体的局部空间
        Vector3 localCenter = groupBuns.transform.InverseTransformPoint(worldBounds.center);
        Vector3 localSize = groupBuns.transform.InverseTransformVector(worldBounds.size);

        collider.offset = localCenter;
        collider.size = localSize;

        Debug.Log($"📦 Collider 已设置：局部中心={collider.offset:F2}, 局部大小={collider.size:F2}");

        var clickable = groupBuns.AddComponent<ClickableItem>();
        clickable.isDraggable = true;
        clickable.isUsable = true;
        // ✅ 加入列表
        cookedBuns.Add(groupBuns);

        OnCookingCompleteEvent?.Invoke();

        Debug.Log($"✅ 生成 {requiredBuns} 个熟包子，Collider 精确包围所有包子");
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

    private Bounds GetRendererOrColliderBounds(GameObject obj)
    {
        Bounds bounds = new Bounds();
        bool hasBounds = false;

        // 优先使用 Renderer
        var renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            bounds = renderer.bounds;
            hasBounds = true;
        }

        // 其次尝试 Collider
        if (!hasBounds)
        {
            var col = obj.GetComponent<Collider>();
            if (col != null && col.enabled)
            {
                // 对于 BoxCollider 可以估算，但最好有 Renderer
                bounds = col.bounds;
                hasBounds = true;
            }
        }

        // 如果都没有，使用一个默认大小（比如 0.1 半径）
        if (!hasBounds)
        {
            bounds = new Bounds(obj.transform.position, new Vector3(0.1f, 0.1f, 0.1f));
        }

        return bounds;
    }
}