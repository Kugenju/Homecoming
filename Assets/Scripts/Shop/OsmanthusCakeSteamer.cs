using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
/// <summary>
/// 桂花糕蒸笼：全自动
/// 初始开盖 → 有熟糕 → 拖走后等待 → 自动闭盖蒸煮 → 开盖 → 淡入生成新糕
/// </summary>
[RequireComponent(typeof(Animator))]
public class OsmanthusCakeSteamer : SteamerBase
{
    [Header("预制体配置")]
    public GameObject cakePrefab;           // 桂花糕预制体

    [Header("时间配置")]
    public float regenDelay = 3f;           // 被拿走后延迟再生
    public float cookTime = 5f;             // 蒸煮时间

    private GameObject currentCake;

    protected override void Start()
    {
        base.Start();
        // 初始生成一个桂花糕（开盖后）
        Invoke(nameof(SpawnCake), 0.5f); // 稍微延迟确保开启动画完成
    }

    public override void OnFoodAdded(ClickableItem item)
    {
        // 桂花糕蒸笼不接受放入食物
    }

    public override void StartCooking()
    {
        // 不需要外部触发，由内部流程控制
        StartCoroutine(CookRoutine());
    }

    private IEnumerator CookRoutine()
    {
        yield return new WaitForSeconds(cookTime);
        OnCookingComplete(); // 触发基类逻辑：开盖 + 可交互
    }

    private void SpawnCake()
    {
        if (cakePrefab == null || foodParent == null) return;

        // 销毁旧的（防止叠加）
        if (currentCake != null)
            Destroy(currentCake);

        currentCake = Instantiate(cakePrefab, foodParent);
        currentCake.transform.localPosition = Vector3.zero;
        currentCake.SetActive(false);

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
        // 食物被拖走后，开始再生流程
        if (currentCake != null)
        {
            Destroy(currentCake);
            currentCake = null;
        }

        // 延迟后开始闭盖蒸煮
        StartCoroutine(RegenAfterDelay());
    }

    private IEnumerator RegenAfterDelay()
    {
        yield return new WaitForSeconds(regenDelay);
        PlayCloseAnimation();
        StartSteaming();
        currentState = State.Cooking;
        OnCookingStartEvent?.Invoke();

        yield return new WaitForSeconds(cookTime);
        OnCookingComplete(); // 开盖 + 可交互
        SpawnCake(); // 重新生成（淡入）
    }
}