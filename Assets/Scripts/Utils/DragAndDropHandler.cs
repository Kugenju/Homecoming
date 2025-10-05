using UnityEngine;
using System;
using UnityEngine.Events;
/// <summary>
/// 通用拖拽系统
/// 支持任意带有 ClickableItem 的物体进行拖拽
/// 可设置有效投放区域（DroppableZone），并触发自定义回调
/// </summary>
public class DragAndDropHandler : MonoBehaviour
{
    // 单例模式：全局唯一拖拽处理器
    public static DragAndDropHandler Instance { get; private set; }

    [Header("拖拽设置")]
    public Camera eventCamera; // 用于屏幕转世界坐标的摄像机
    public float dragOffsetZ = 10f; // 控制拖拽对象在 Z 轴偏移（避免与其他UI/物体遮挡）

    [Header("视觉反馈")]
    public bool useSmoothMovement = true;
    public float smoothSpeed = 8f;

    // 当前拖拽状态
    private ClickableItem _currentDragItem;
    private Vector3 _offset;
    private bool _isDragging = false;
    private Transform _originalParent; // 记录原始父对象（用于返回）
    private Vector3 _originalPosition;

    // 回调事件（可用于外部监听）
    [Serializable]
    public class DragEvent : UnityEvent<ClickableItem> { }

    public DragEvent OnDragStart;
    public DragEvent OnDragEnd;
    public UnityEvent<ClickableItem, DroppableZone> OnItemDropped;

    // ------------------------------
    // Unity 生命周期函数
    // ------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 默认使用主摄像机
        if (eventCamera == null)
            eventCamera = Camera.main;
    }

    private void Update()
    {
        if (_isDragging)
        {
            HandleDragging();
        }
    }

    // ------------------------------
    // 外部接口：由 ClickableItem 触发
    // ------------------------------

    /// <summary>
    /// 开始拖拽（由 ClickableItem 调用）
    /// </summary>
    /// <param name="item">被拖拽的物体</param>
    /// <param name="clickPointInWorld">鼠标点击时的世界坐标</param>
    public void StartDrag(ClickableItem item, Vector3 clickPointInWorld)
    {
        if (item == null || !item.isDraggable || !item.isUsable) return;

        _currentDragItem = item;
        _originalParent = item.transform.parent;
        _originalPosition = item.transform.position;

        // 进入拖拽状态
        _isDragging = true;

        // 计算拖拽偏移量（保持鼠标相对位置）
        Vector3 worldPos = GetMouseWorldPosition();
        _offset = _currentDragItem.GetPosition() - worldPos;
        _offset.z = dragOffsetZ; // 稍微提起来一点

        // 提升层级（可选：移动到Canvas顶层或临时父对象）
        if (_currentDragItem.transform is RectTransform rectTransform)
        {
            rectTransform.SetAsLastSibling();
        }
        else
        {
            _currentDragItem.transform.SetPositionAndRotation(
                _currentDragItem.transform.position,
                Quaternion.Euler(0, 0, UnityEngine.Random.Range(-5f, 5f)) // 轻微旋转增加真实感
            );
        }

        // 触发开始事件
        OnDragStart?.Invoke(_currentDragItem);

        // 禁用原点击行为
        _currentDragItem.SetUsable(false);
    }

    /// <summary>
    /// 结束拖拽（通常由松开鼠标或碰撞触发）
    /// </summary>
    public void EndDrag()
    {
        if (!_isDragging) return;

        // 尝试投放到某个区域
        DroppableZone validZone = FindValidDropZone();

        if (validZone != null)
        {
            // 投放成功：执行投放逻辑
            validZone.OnItemDrop(_currentDragItem);
            OnItemDropped?.Invoke(_currentDragItem, validZone);
        }
        else
        {
            // 投放失败：返回原位
            ReturnToOriginalPosition();
        }

        // 清理状态
        OnDragEnd?.Invoke(_currentDragItem);
        _isDragging = false;
        _currentDragItem = null;
    }

    // ------------------------------
    // 内部逻辑
    // ------------------------------

    private void HandleDragging()
    {
        Vector3 targetPosition = GetMouseWorldPosition() + _offset;

        if (useSmoothMovement)
        {
            _currentDragItem.transform.position = Vector3.Lerp(
                _currentDragItem.transform.position,
                targetPosition,
                Time.deltaTime * smoothSpeed
            );
        }
        else
        {
            _currentDragItem.transform.position = targetPosition;
        }
    }

    /// <summary>
    /// 获取鼠标当前在世界空间中的位置
    /// </summary>
    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = dragOffsetZ; // 使用固定深度
        return eventCamera.ScreenToWorldPoint(mouseScreenPos);
    }

    /// <summary>
    /// 查找当前是否悬停在一个有效的投放区
    /// </summary>
    private DroppableZone FindValidDropZone()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(GetMouseWorldPosition(), 0.5f);
        foreach (var hit in hits)
        {
            DroppableZone zone = hit.GetComponent<DroppableZone>();
            if (zone != null && zone.CanAcceptItem(_currentDragItem))
            {
                return zone;
            }
        }
        return null;
    }

    /// <summary>
    /// 返回原始位置（投放失败）
    /// </summary>
    private void ReturnToOriginalPosition()
    {
        if (_currentDragItem == null) return;

        _currentDragItem.transform.position = _originalPosition;
        _currentDragItem.transform.SetParent(_originalParent);
    }

    // ------------------------------
    // 公共方法（供外部调用）
    // ------------------------------

    /// <summary>
    /// 是否正在拖拽某个物体
    /// </summary>
    public bool IsDragging() => _isDragging;

    /// <summary>
    /// 获取当前被拖拽的物品
    /// </summary>
    public ClickableItem GetCurrentDragItem() => _currentDragItem;

    /// <summary>
    /// 强制结束拖拽（例如顾客离开时）
    /// </summary>
    public void ForceEndDrag()
    {
        if (_isDragging)
        {
            ReturnToOriginalPosition();
            OnDragEnd?.Invoke(_currentDragItem);
            _isDragging = false;
            _currentDragItem = null;
        }
    }
}
