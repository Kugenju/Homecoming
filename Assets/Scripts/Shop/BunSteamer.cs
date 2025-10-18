using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// 包子蒸笼：手动填满触发
/// 开盖 → 放入指定数量生包子 → 自动闭盖蒸煮 → 开盖 → 可一次性拖走所有熟包子 → 重置
/// </summary>
[RequireComponent(typeof(Animator))]
public class BunSteamer : SteamerBase
{
    [Header("配置")]
    public GameObject rawBunPrefab;           // 用于检测 Tag
    public GameObject cookedBunPrefab;        // 熟包子预制体
    public int requiredBuns = 3;               // 需要多少个生包子
    public float cookTime = 8f;

    [Header("包子生成位置与大小")]
    public Transform[] bunPositions;          // 预设位置锚点数组（在 foodParent 下）
    public Vector3 bunScale = Vector3.one;    // 包子缩放大小

    private List<ClickableItem> rawBuns = new List<ClickableItem>();
    private List<GameObject> cookedBuns = new List<GameObject>();

    [Header("标签设置")]
    public string rawBunTag = "RawBun"; // 必须与生包子的 Tag 一致

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
            rawBuns.Add(item);
            Destroy(item.gameObject);
            Debug.Log($"✅ 生包子已放入，当前数量: {rawBuns.Count}/{requiredBuns}");

            if (rawBuns.Count >= requiredBuns)
            {
                Debug.Log("✅ 生包子已满，开始蒸煮！");
                StartCooking();
            }
        }
    }

    public override void StartCooking()
    {
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

        // 清理旧包子
        foreach (var bun in cookedBuns) Destroy(bun);
        cookedBuns.Clear();

        // 生成熟包子
        for (int i = 0; i < requiredBuns; i++)
        {
            // 选择位置：有预设位置则使用，否则随机
            Vector3 spawnPos;
            if (bunPositions != null && bunPositions.Length > 0)
            {
                Transform targetPos = bunPositions[i % bunPositions.Length]; // 循环使用
                spawnPos = targetPos.localPosition;
            }
            else
            {
                // 降级为随机位置
                spawnPos = Random.insideUnitCircle * 0.2f;
            }

            GameObject bun = Instantiate(cookedBunPrefab, foodParent);
            bun.transform.localPosition = spawnPos;
            bun.transform.localScale = bunScale; // ✅ 应用自定义缩放

            // 确保它有 ClickableItem 组件
            ClickableItem clickable = bun.GetComponent<ClickableItem>();
            if (clickable == null)
                clickable = bun.AddComponent<ClickableItem>();
            clickable.isDraggable = true;
            clickable.isUsable = true;

            //// 注册移除事件，用于检测是否被拿走
            //clickable.OnItemRemovedFromWorld.AddListener(() =>
            //{
            //    // 检查是否所有包子都被拿走
            //    if (AreAllBunsTaken())
            //    {
            //        OnFoodTaken(); // 触发重置
            //    }
            //});

            cookedBuns.Add(bun);
        }

        OnCookingCompleteEvent?.Invoke();
    }

    /// <summary>
    /// 检查是否所有熟包子都被拿走了
    /// </summary>
    private bool AreAllBunsTaken()
    {
        // 只要有一个还在，就返回 false
        foreach (GameObject bun in cookedBuns)
        {
            if (bun != null) return false;
        }
        return true;
    }

    protected override void OnFoodTaken()
    {
        // 所有熟包子被拿走
        rawBuns.Clear();
        currentState = State.Idle;
        isInteractable = false;
        Debug.Log("包子蒸笼已重置，等待下一轮");
    }
}