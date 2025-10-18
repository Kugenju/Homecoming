using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// 蒸笼基类 - 使用 Animator 和 ParticleSystem 控制盖子与蒸汽
/// 子类实现具体逻辑：桂花糕蒸笼（自动）、包子蒸笼（手动填满）
/// </summary>
public abstract class SteamerBase : DroppableZone
{
    public enum State { Idle, Opening, Ready, Closing, Cooking }

    [Header("组件引用")]
    public Animator lidAnimator;           // 盖子动画控制器
    public ParticleSystem steamParticles;  // 蒸汽粒子系统
    public Transform foodParent;           // 食物放置的父物体

    [Header("运行时状态")]
    protected State currentState = State.Idle;
    protected bool isInteractable = false; // 是否允许玩家拖走食物

    [Header("事件回调")]
    public UnityEvent OnOpenEvent;         // 开盖完成
    public UnityEvent OnCloseEvent;        // 关盖完成
    public UnityEvent OnCookingStartEvent; // 开始蒸煮
    public UnityEvent OnCookingCompleteEvent; // 蒸煮完成

    // 动画参数 Hash
    private static readonly int OpenHash = Animator.StringToHash("Open");
    private static readonly int CloseHash = Animator.StringToHash("Close");

    protected override void Awake()
    {
        base.Awake(); // 必须调用父类 Awake

        if (lidAnimator == null)
            lidAnimator = GetComponent<Animator>();

        if (foodParent == null)
            foodParent = transform;

        // 初始化粒子系统
        if (steamParticles != null)
        {
            var main = steamParticles.main;
            main.loop = false;
            steamParticles.Stop();
        }

        string zoneLayer = dropZoneLayer ?? "DropZones";
        int layer = LayerMask.NameToLayer(zoneLayer);
        if (layer != -1)
        {
            gameObject.layer = layer;
        }
    }

    protected virtual void Start()
    {
        PlayOpenAnimation(); // 初始播放开盖动画
    }

    // —————— 动画控制 ——————
    protected void PlayOpenAnimation()
    {
        if (lidAnimator == null) return;
        lidAnimator.SetBool(OpenHash, true);
        lidAnimator.SetBool(CloseHash, false);
        currentState = State.Opening;
        OnOpenEvent?.Invoke();
    }

    protected void PlayCloseAnimation()
    {
        if (lidAnimator == null) return;
        lidAnimator.SetBool(OpenHash, false);
        lidAnimator.SetBool(CloseHash, true);
        currentState = State.Closing;
        OnCloseEvent?.Invoke();
    }

    // —————— 蒸汽控制 ——————
    protected void StartSteaming()
    {
        if (steamParticles != null && !steamParticles.isPlaying)
            steamParticles.Play();
    }

    protected void StopSteaming()
    {
        if (steamParticles != null && steamParticles.isPlaying)
            steamParticles.Stop();
    }

    // —————— 抽象方法 ——————
    public abstract void OnFoodAdded(ClickableItem item);   // 放入食物
    public abstract void StartCooking();                    // 开始蒸煮
    protected abstract void OnFoodTaken();                  // 食物被拿走

    // —————— 重写 DroppableZone 的 CanAcceptItem ——————
    public override bool CanAcceptItem(ClickableItem item)
    {
        // 由子类决定是否接受
        return base.CanAcceptItem(item);
    }

    // —————— 重写 OnItemDrop：当物品被拖入并松手 ——————
    public override void OnItemDrop(ClickableItem item)
    {
        base.OnItemDrop(item);
        Debug.Log($"物品被放入蒸笼: {item.name}");

        // 转发给子类处理
        OnFoodAdded(item);
    }

    // —————— 动画事件回调 ——————
    public void OnAnimation_OpenComplete()
    {
        currentState = State.Ready;
    }

    public void OnAnimation_CloseComplete()
    {
        currentState = State.Cooking;
    }

    protected virtual void OnCookingComplete()
    {
        StopSteaming();
        PlayOpenAnimation();
        currentState = State.Ready;
        isInteractable = true;
        OnCookingCompleteEvent?.Invoke();
    }

    // —————— 交互支持 ——————

}