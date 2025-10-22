using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic; // 引入泛型集合命名空间

/// <summary>
/// 可点击物品组件
/// 支持鼠标悬停、点击、双击和拖拽交互
/// 需要与 DragAndDropHandler 配合使用
/// </summary>
public class ClickableItem : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [Header("基础设置")]
    public bool isDraggable = false;       // 是否可拖拽
    public bool isUsable = true;           // 是否可用（影响所有交互状态）
    public float clickCooldown = 0.3f;     // 点击冷却时间

    [Header("视觉效果")]
    public bool showHighlightOnHover = true;
    public Color hoverColor = new Color(1f, 1f, 1f, 0.2f); // 悬停颜色(透明叠加)
    private Material originalMaterial;
    private Color originalColor;

    [Header("事件配置")]
    public UnityEvent OnClicked;           // 单击事件
    public UnityEvent OnDoubleClicked;     // 双击事件（可选配置）
    public UnityEvent OnHoverEnter;        // 悬停进入
    public UnityEvent OnHoverExit;         // 悬停退出
    public UnityEvent OnDragStart;         // 拖拽开始事件
    public UnityEvent OnDragEnd;           // 拖拽结束事件

    // 状态变量
    private bool canClick = true;
    private bool isHovering = false;
    private float lastClickTime = 0f;
    private const float DoubleClickThreshold = 0.3f; // 双击判断时间阈值
    private Dictionary<Renderer, int> originalSortingOrders = new Dictionary<Renderer, int>(); // 存储原始层级

    // 组件引用
    private Collider2D _collider2D;
    private Renderer _renderer; // 通用 Renderer (支持 SpriteRenderer / MeshRenderer)
    private CanvasGroup _canvasGroup;

    // ------------------------------
    // Unity 生命周期方法
    // ------------------------------

    protected virtual void Awake()
    {
        // 获取必要组件
        _collider2D = GetComponent<Collider2D>();
        _renderer = GetComponent<Renderer>();
        _canvasGroup = GetComponent<CanvasGroup>();

        if (_renderer != null)
        {
            originalMaterial = _renderer.material; // 保存原始材质
            originalColor = _renderer.material.color;
        }

        Debug.Log($"{gameObject.name} 组件状态: Collider2D={_collider2D != null}, Renderer={_renderer != null}");

        if (OnDragStart == null)
            OnDragStart = new UnityEngine.Events.UnityEvent();

        if (OnDragEnd == null)
            OnDragEnd = new UnityEngine.Events.UnityEvent();
    }

    protected virtual void Update()
    {
        // 双击检测冷却
        if (!canClick && Time.time - lastClickTime > DoubleClickThreshold)
        {
            canClick = true;
        }
    }

    // ------------------------------
    // 指针事件接口实现（Unity EventSystem 自动调用）
    // ------------------------------

    /// <summary>
    /// 鼠标进入物体区域
    /// </summary>
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if (!isUsable)
        {
            Debug.Log($"物品不可用 isUsable = false,触发在 {gameObject.name}");
            return;
        }

        isHovering = true;
        OnHoverEnter?.Invoke();

        ApplyHoverVisuals();
    }

    /// <summary>
    /// 鼠标离开物体区域
    /// </summary>
    public virtual void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        OnHoverExit?.Invoke();

        RemoveHoverVisuals();
    }

    /// <summary>
    /// 鼠标按下
    /// </summary>
    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if (!isUsable || !canClick) return;

        if (isDraggable)
        {
            // 可拖拽物品的按下事件由拖拽系统处理
            return;
        }

        HandleClick();
    }

    /// <summary>
    /// 鼠标释放
    /// </summary>
    public virtual void OnPointerUp(PointerEventData eventData)
    {
        if (!isUsable || !canClick) return;

        if (!isDraggable)
        {
            HandleClick();
        }
    }

    /// <summary>
    /// 开始拖拽（Unity EventSystem 接口）
    /// </summary>
    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        if (!isUsable || !isDraggable) return;

        OnDragStart?.Invoke();

        // 记录原始层级并增加1000
        SetRenderersSortingOrder(1000);

        // 通知拖拽管理器开始拖拽
        if (DragAndDropHandler.Instance != null)
        {
            Vector3 worldPos = GetWorldPositionFromEventData(eventData);
            DragAndDropHandler.Instance.StartDrag(this, worldPos);
        }
    }

    /// <summary>
    /// 拖拽中（Unity EventSystem 接口）
    /// </summary>
    public virtual void OnDrag(PointerEventData eventdata)
    {
        if (!isDraggable) return;

        // 拖拽中的处理由 draganddrophandler 负责
    }

    /// <summary>
    /// 结束拖拽（Unity EventSystem 接口）
    /// </summary>
    public virtual void OnEndDrag(PointerEventData eventData)
    {
        if (!isDraggable) return;

        OnDragEnd?.Invoke();

        // 恢复原始层级
        RestoreRenderersSortingOrder();

        // 通知拖拽管理器结束拖拽
        if (DragAndDropHandler.Instance != null)
        {
            DragAndDropHandler.Instance.EndDrag();
        }
    }

    // ------------------------------
    // 视觉效果方法
    // ------------------------------

    private void ApplyHoverVisuals()
    {
        if (showHighlightOnHover && _renderer != null)
        {
            // 创建材质实例以避免影响其他物体
            _renderer.material = new Material(_renderer.material);
            Color targetColor = originalColor + hoverColor;
            targetColor.a = Mathf.Min(1f, targetColor.a);
            _renderer.material.color = targetColor;
        }

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0.9f;
        }
    }

    private void RemoveHoverVisuals()
    {
        if (showHighlightOnHover && _renderer != null && originalMaterial != null)
        {
            _renderer.material.color = originalColor;
            // 恢复原始材质
            _renderer.material = originalMaterial;
        }

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 1f;
        }
    }

    // ------------------------------
    // 层级调整方法
    // ------------------------------

    /// <summary>
    /// 调整当前物体及子物体的渲染层级
    /// </summary>
    private void SetRenderersSortingOrder(int offset)
    {
        originalSortingOrders.Clear();
        // 获取所有子物体（包括自身）的Renderer组件，true表示包含非激活状态的
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        
        foreach (var renderer in renderers)
        {
            // 记录原始层级
            originalSortingOrders[renderer] = renderer.sortingOrder;
            // 应用偏移
            renderer.sortingOrder += offset;
        }
    }

    /// <summary>
    /// 恢复当前物体及子物体的原始渲染层级
    /// </summary>
    private void RestoreRenderersSortingOrder()
    {
        foreach (var kvp in originalSortingOrders)
        {
            // 恢复原始层级
            kvp.Key.sortingOrder = kvp.Value;
        }
        originalSortingOrders.Clear();
    }

    // ------------------------------
    // 点击处理逻辑
    // ------------------------------

    private void HandleClick()
    {
        float timeSinceLastClick = Time.time - lastClickTime;

        // 检测双击
        if (timeSinceLastClick <= DoubleClickThreshold)
        {
            Debug.Log("双击检测成功");
            OnDoubleClicked?.Invoke();
            canClick = false;
            lastClickTime = 0f;
            StartCoroutine(ResetClickStateAfterDelay(clickCooldown));
            return;
        }

        // 单机处理
        Debug.Log("单击处理");
        OnClicked?.Invoke();
        lastClickTime = Time.time;
        canClick = false;
        StartCoroutine(ResetClickStateAfterDelay(clickCooldown));
    }

    private IEnumerator ResetClickStateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        canClick = true;
    }

    // ------------------------------
    // 工具方法
    // ------------------------------

    private Vector3 GetWorldPositionFromEventData(PointerEventData eventData)
    {
        if (eventData == null) return Vector3.zero;

        Vector3 screenPos = eventData.position;
        screenPos.z = 10f; // 默认Z轴偏移
        return Camera.main.ScreenToWorldPoint(screenPos);
    }

    // ------------------------------
    // 外部调用接口
    // ------------------------------

    /// <summary>
    /// 强制触发点击事件（可用于程序控制）
    /// </summary>
    public virtual void ForceClick()
    {
        if (isUsable && canClick)
        {
            HandleClick();
        }
    }

    /// <summary>
    /// 设置物品可用状态
    /// </summary>
    public virtual void SetUsable(bool usable)
    {
        isUsable = usable;
        if (_collider2D != null)
        {
            _collider2D.enabled = usable;
        }
        if (!usable && isHovering)
        {
            OnPointerExit(null); // 清理悬停状态
        }
    }

    /// <summary>
    /// 获取当前是否处于悬停状态
    /// </summary>
    public bool IsHovering() => isHovering;

    /// <summary>
    /// 获取物品位置（世界坐标）
    /// </summary>
    public Vector3 GetPosition() => transform.position;

    /// <summary>
    /// 手动触发悬停进入（用于程序控制）
    /// </summary>
    public virtual void ManualPointerEnter()
    {
        OnPointerEnter(null);
    }

    /// <summary>
    /// 手动触发悬停退出（用于程序控制）
    /// </summary>
    public virtual void ManualPointerExit()
    {
        OnPointerExit(null);
    }
}