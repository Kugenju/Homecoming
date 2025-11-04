using UnityEngine;
using UnityEngine.Events;
using System.Collections;

/// <summary>
/// 桂花糕蒸笼：全自动（通过currentCake == null检测触发流程）
/// 初始开盖 → 有熟糕 → 拖走后（currentCake自动为null）→ 自动闭盖蒸煮 → 开盖 → 淡入生成新糕
/// </summary>
[RequireComponent(typeof(Animator))]
public class OsmanthusCakeSteamer : SteamerBase
{
    [Header("预制体配置")]
    public GameObject cakePrefab;           // 桂花糕预制体

    [Header("时间配置")]
    public float regenDelay = 0.5f;           // 被拿走后延迟再生
    public float cookTime = 5f;             // 蒸煮时间

    private GameObject currentCake;
    private bool isRegenerating = false;    // 防止重复触发再生流程

    protected override void Start()
    {
        base.Start();
        // 初始生成一个桂花糕（等待开启动画完成）
        Invoke(nameof(SpawnCake), 0.5f);
    }

    private void Update()
    {
        // 核心逻辑：检测蛋糕为空且不在再生状态时，触发再生流程
        if (currentCake == null && !isRegenerating && currentState == State.Ready)
        {
            StartCoroutine(RegenAfterDelay());
        }
    }

    public override void OnFoodAdded(ClickableItem item)
    {
        // 桂花糕蒸笼不接受外部放入食物
    }

    public override void StartCooking()
    {
        // 由内部流程触发蒸煮
        StartSteaming();
        currentState = State.Cooking;
        OnCookingStartEvent?.Invoke();
    }

    private void SpawnCake()
    {
        if (cakePrefab == null || foodParent == null) return;

        // 销毁旧的（防止叠加）
        if (currentCake != null)
            Destroy(currentCake);

        // 生成新蛋糕（不绑定移除监听器，完全通过currentCake == null检测）
        currentCake = Instantiate(cakePrefab, foodParent);
        currentCake.transform.localPosition = Vector3.zero;
        currentCake.SetActive(false);

        // 确保蛋糕可拖拽（如果预制体未默认设置）
        var clickable = currentCake.GetComponent<ClickableItem>();
        if (clickable != null)
        {
            clickable.isDraggable = true;
            clickable.isUsable = true;
        }

        // 淡入效果
        StartCoroutine(FadeInObject(currentCake));
    }

    private IEnumerator FadeInObject(GameObject obj)
    {
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            obj.SetActive(true);
            yield break;
        }

        Color color = sr.color;
        sr.color = new Color(color.r, color.g, color.b, 0);
        obj.SetActive(true);

        float duration = 1f;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            sr.color = new Color(color.r, color.g, color.b, t / duration);
            yield return null;
        }
        sr.color = color;
    }

    protected override void OnFoodTaken()
    {
        // 无需额外处理，完全依赖currentCake == null检测
        currentState = State.Ready;
    }

    private IEnumerator RegenAfterDelay()
    {
        isRegenerating = true;
        currentState = State.Idle; // 再生期间设为Idle，防止重复触发

        // 等待再生延迟
        yield return new WaitForSeconds(regenDelay);

        // 闭盖（等待动画完成后进入Cooking状态，依赖SteamerBase的OnAnimation_CloseComplete）
        PlayCloseAnimation();
        // yield return new WaitUntil(() => currentState == State.Cooking);

        // 开始蒸煮
        StartCooking();

        // 蒸煮倒计时
        yield return new WaitForSeconds(cookTime);

        // 蒸煮完成，开盖（自动进入Ready状态）
        OnCookingComplete();

        // 生成新蛋糕
        SpawnCake();

        // 重置再生状态
        isRegenerating = false;
        currentState = State.Ready;
    }

    protected override void OnCookingComplete()
    {
        base.OnCookingComplete(); // 调用基类逻辑：停止蒸汽、开盖、设置状态为Ready
    }
}