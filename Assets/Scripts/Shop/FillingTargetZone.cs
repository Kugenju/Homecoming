using UnityEngine;
using System.Collections;

public class FillingTargetZone : DroppableZone
{
    [Header("填充设置")]
    public string requiredFillingTag = "MeatFilling";
    public Vector3 fillingOffset = new Vector3(0, 0.5f, 0);
    public float packDelay = 0.5f;

    [Header("动画和效果")]
    public Animator packAnimator; // Animator 组件
    public GameObject packedBunPrefab;
    public AudioClip packSound;

    [Header("动画参数")]
    public string packingBoolParameter = "IsPacking"; // Bool 参数名称

    private bool isPacked = false;
    private AudioSource audioSource;

    protected override void Awake()
    {
        base.Awake();
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();

        // 确保动画初始状态为 Normal
        if (packAnimator != null)
        {
            packAnimator.SetBool(packingBoolParameter, false);
        }
    }

    public override bool CanAcceptItem(ClickableItem item)
    {
        return !isPacked && item != null && item.CompareTag(requiredFillingTag);
    }

    public override void OnItemDrop(ClickableItem item)
    {
        if (isPacked) return;

        base.OnItemDrop(item);

        // 隐藏当前物体视觉部分
        //SetVisualActive(false);

        // 播放音效
        if (packSound) audioSource.PlayOneShot(packSound);

        // 开始打包协程
        StartCoroutine(PackBunAfterDelay(item));
    }

    private IEnumerator PackBunAfterDelay(ClickableItem fillingItem)
    {
        yield return new WaitForSeconds(packDelay);

        // 播放打包动画 - 通过设置 Bool 参数触发动画状态切换
        if (packAnimator != null)
        {
            // 设置 IsPacking 为 true，触发从 Normal 到 PackAnimation 的过渡
            packAnimator.SetBool(packingBoolParameter, true);
            Debug.Log("触发打包动画，IsPacking = true");

            // 等待动画过渡完成并播放
            yield return StartCoroutine(WaitForAnimationToComplete());
        }
        else
        {
            Debug.LogWarning("Animator not set! Using default delay.");
            yield return new WaitForSeconds(1.5f); // 默认延迟
        }

        // 动画播放完成后继续执行
        CompletePackingProcess();
    }

    private IEnumerator WaitForAnimationToComplete()
    {
        if (packAnimator == null) yield break;

        // 等待一帧确保动画状态已切换
        yield return null;

        // 获取当前动画状态信息
        AnimatorStateInfo stateInfo = packAnimator.GetCurrentAnimatorStateInfo(0);

        // 等待动画播放完成
        float animationLength = stateInfo.length;
        float timer = 0f;

        while (timer < animationLength)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        // 额外等待一小段时间确保动画完全结束
        yield return new WaitForSeconds(0.1f);

        Debug.Log("打包动画播放完成");
    }

    private void CompletePackingProcess()
    {
        // 恢复视觉显示
        SetVisualActive(true);

        // 替换为包好的包子预制体
        Vector3 pos = transform.position;
        Quaternion rot = transform.rotation;
        Transform parent = transform.parent;

        // 在销毁前重置动画状态（如果需要）
        if (packAnimator != null)
        {
            packAnimator.SetBool(packingBoolParameter, false);
        }

        Destroy(gameObject); // 销毁当前物体

        // 实例化包好的包子
        GameObject bun = Instantiate(packedBunPrefab, pos, rot, parent);
        bun.name = "RawBun";

        // 更新面团板上的占位信息
        DoughBoardZone doughBoard = FindObjectOfType<DoughBoardZone>();
        if (doughBoard != null)
        {
            int spotIndex = doughBoard.FindSpotIndexByPosition(transform.position);
            if (spotIndex >= 0)
            {
                doughBoard.UpdateOccupiedSpot(spotIndex, bun);
            }
        }

        // 设置新包子为可用状态
        var clickable = bun.GetComponent<ClickableItem>();
        if (clickable) clickable.SetUsable(true);
    }

    private void SetVisualActive(bool active)
    {
        // 显示/隐藏所有渲染器
        foreach (var renderer in GetComponentsInChildren<Renderer>())
            renderer.enabled = active;
        foreach (var sr in GetComponentsInChildren<SpriteRenderer>())
            sr.enabled = active;
    }

    // 可选：在脚本禁用时重置动画状态
    private void OnDisable()
    {
        if (packAnimator != null)
        {
            packAnimator.SetBool(packingBoolParameter, false);
        }
    }
}