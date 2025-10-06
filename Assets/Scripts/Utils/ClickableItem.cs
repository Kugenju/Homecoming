using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
/// <summary>
/// 可点击物体基类
/// 用于食材、碗、蒸笼、顾客等交互对象
/// 支持点击、双击、拖拽（需配合 DragAndDropHandler）
/// </summary>
public class ClickableItem : MonoBehaviour,IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("基础设置")]
    public bool isDraggable = false;       // 是否可拖拽
    public bool isUsable = true;           // 是否可交互（可动态关闭）
    public float clickCooldown = 0.3f;     // 防误触：点击后冷却时间

    [Header("视觉反馈")]
    public bool showHighlightOnHover = true;
    public Color hoverColor = new Color(1f, 1f, 1f, 0.2f); // 高亮颜色(后期调整）
    private Material originalMaterial;
    private Color originalColor;

    [Header("事件回调")]
    public UnityEvent OnClicked;           // 单击事件
    public UnityEvent OnDoubleClicked;     // 双击事件（可选）
    public UnityEvent OnHoverEnter;        // 鼠标进入
    public UnityEvent OnHoverExit;         // 鼠标离开

    // 状态控制
    private bool canClick = true;
    private bool isHovering = false;
    private float lastClickTime = 0f;
    private const float DoubleClickThreshold = 0.3f; // 双击判定时间窗口

    // 组件缓存
    private Collider2D _collider2D;
    private Renderer _renderer; // 通用 Renderer，兼容 SpriteRenderer / MeshRenderer
    private CanvasGroup _canvasGroup;

    // ------------------------------
    // Unity 生命周期函数
    // ------------------------------

    protected virtual void Awake()
    {
        //Debug.Log($"ClickableItem Awake on {gameObject.name}");
        // 缓存常用组件
        _collider2D = GetComponent<Collider2D>();
        _renderer = GetComponent<Renderer>();
        _canvasGroup = GetComponent<CanvasGroup>();

        if (_renderer != null)
        {
            originalMaterial = _renderer.material; // 保存原始材质
            originalColor = _renderer.material.color;
        }
        Debug.Log($"{gameObject.name} 组件状态: Collider2D={_collider2D != null}, Renderer={_renderer != null}");
    }

    protected virtual void Update()
    {
        // 双击检测
        if (!canClick && Time.time - lastClickTime > DoubleClickThreshold)
        {
            canClick = true;
        }
    }

    // ------------------------------
    // 鼠标交互方法（通过 EventSystem 或射线调用）
    // ------------------------------

    /// <summary>
    /// 鼠标进入
    /// </summary>
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"OnPointerEnter 被调用 on {gameObject.name}");
        if (!isUsable)
        {
            Debug.Log("但 isUsable = false，被忽略");
            return;
        }

        isHovering = true;
        OnHoverEnter?.Invoke();

        if (showHighlightOnHover && _renderer != null)
        {
            // 创建新的材质实例避免影响其他物体
            _renderer.material = new Material(_renderer.material);
            Color targetColor = originalColor + hoverColor;
            targetColor.a = Mathf.Min(1f, targetColor.a);
            _renderer.material.color = targetColor;
            Debug.Log($"高亮颜色应用: {targetColor}");
        }

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0.9f;
        }
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log($"OnPointerExit 被调用 on {gameObject.name}");
        isHovering = false;
        OnHoverExit?.Invoke();

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

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log($"OnPointerDown 被调用 on {gameObject.name}");
        if (!isUsable || !canClick) return;

        if (isDraggable)
        {
            return;
        }

        HandleClick();
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log($"OnPointerUp 被调用 on {gameObject.name}");
        if (!isUsable || !canClick) return;

        if (!isDraggable)
        {
            HandleClick();
        }
    }
    /// <summary>
    /// 鼠标进入（供外部调用）
    /// </summary>
    public virtual void OnPointerEnter()
    {
        // 这个方法可以保留供外部代码调用，但内部事件处理使用接口版本
        OnPointerEnter(null);
    }

    /// <summary>
    /// 鼠标离开（供外部调用）
    /// </summary>
    public virtual void OnPointerExit()
    {
        OnPointerExit(null);
    }

    /// <summary>
    /// 鼠标按下（供外部调用）
    /// </summary>
    public virtual void OnPointerDown()
    {
        OnPointerDown(null);
    }

    /// <summary>
    /// 鼠标抬起（供外部调用）
    /// </summary>
    public virtual void OnPointerUp()
    {
        OnPointerUp(null);
    }

    // ------------------------------
    // 核心点击逻辑
    // ------------------------------

    private void HandleClick()
    {
        float timeSinceLastClick = Time.time - lastClickTime;

        // 判定双击
        if (timeSinceLastClick <= DoubleClickThreshold)
        {
            OnDoubleClicked?.Invoke();
            canClick = false;
            lastClickTime = 0f;
            Invoke(nameof(ResetClickState), clickCooldown);
            return;
        }

        // 单击
        OnClicked?.Invoke();
        lastClickTime = Time.time;
        canClick = false;
        Invoke(nameof(ResetClickState), clickCooldown);
    }

    private void ResetClickState()
    {
        canClick = true;
    }

    // ------------------------------
    // 公共方法（供外部调用）
    // ------------------------------

    /// <summary>
    /// 强制触发点击事件（调试或自动流程使用）
    /// </summary>
    public virtual void ForceClick()
    {
        if (isUsable && canClick)
        {
            HandleClick();
        }
    }

    /// <summary>
    /// 启用/禁用交互
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
            OnPointerExit(); // 清理悬停状态
        }
    }

    /// <summary>
    /// 获取当前是否在鼠标悬停状态
    /// </summary>
    public bool IsHovering() => isHovering;

    /// <summary>
    /// 获取对象的世界坐标位置（用于移动或定位）
    /// </summary>
    public Vector3 GetPosition()
    {
        return transform.position;
    }
}