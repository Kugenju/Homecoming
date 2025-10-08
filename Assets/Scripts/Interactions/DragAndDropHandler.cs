using UnityEngine;
using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;
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

    [Header("代理拖拽")]
    [Tooltip("是否启用代理拖拽功能")]
    public bool enableProxyDragging = true;

    // 当前拖拽状态
    private ClickableItem _currentDragItem;
    private ProxyDragTag _currentProxyTag;
    private GameObject _currentDragProxy;
    private Vector3 _offset;
    private bool _isDragging = false;
    private Transform _originalParent;
    private Vector3 _originalPosition;

    // 回调事件（可用于外部监听）
    [System.Serializable] public class DragEvent : UnityEngine.Events.UnityEvent<ClickableItem> { }
    [System.Serializable] public class ProxyDragEvent : UnityEngine.Events.UnityEvent<ClickableItem, GameObject> { }

    public DragEvent OnDragStart;
    public DragEvent OnDragEnd;
    public ProxyDragEvent OnProxyDragStart;
    public ProxyDragEvent OnProxyDragEnd;
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
    /// 开始拖拽 - 适配器方法（用于UnityEvent，只需要一个参数）
    /// 这是为了解决UnityEvent参数匹配问题而添加的方法
    /// </summary>
    /// <param name="eventData">指针事件数据</param>
 

    public void StartDrag(ClickableItem item, Vector3 clickPointInWorld)
    {
        if (item == null || !item.isDraggable || !item.isUsable) return;

        _currentDragItem = item;
        _originalParent = item.transform.parent;
        _originalPosition = item.transform.position;

        // 检查代理拖拽
        if (enableProxyDragging)
        {
            _currentProxyTag = item.GetComponent<ProxyDragTag>();
            if (_currentProxyTag != null && _currentProxyTag.proxyPrefab != null)
            {
                StartProxyDrag(clickPointInWorld);
                return;
            }
        }

        // 正常拖拽
        StartNormalDrag(clickPointInWorld);
    }

    private void StartProxyDrag(Vector3 clickPointInWorld)
    {
        _currentDragProxy = _currentProxyTag.CreateProxy(clickPointInWorld);

        if (_currentDragProxy != null)
        {
            // 设置代理的层级
            SetProxyRenderOrder(_currentDragProxy);

            // 计算偏移量（基于代理）
            Vector3 worldPos = GetMouseWorldPosition();
            _offset = _currentDragProxy.transform.position - worldPos;
            _offset.z = dragOffsetZ;

            _isDragging = true;
            OnProxyDragStart?.Invoke(_currentDragItem, _currentDragProxy);
            OnDragStart?.Invoke(_currentDragItem);

            // 禁用原物体的碰撞器
            _currentDragItem.SetUsable(false);
        }
        else
        {
            // 代理创建失败，回退到正常拖拽
            StartNormalDrag(clickPointInWorld);
        }
    }

    private void StartNormalDrag(Vector3 clickPointInWorld)
    {
        _isDragging = true;

        // 计算偏移量
        Vector3 worldPos = GetMouseWorldPosition();
        _offset = _currentDragItem.GetPosition() - worldPos;
        _offset.z = dragOffsetZ;

        // 设置渲染顺序
        if (_currentDragItem.transform is RectTransform rectTransform)
        {
            rectTransform.SetAsLastSibling();
        }

        OnDragStart?.Invoke(_currentDragItem);
    }



    /// <summary>
    /// 结束拖拽（通常由松开鼠标或碰撞触发）
    /// </summary>
    /// <summary>
    /// 结束拖拽
    /// </summary>
    public void EndDrag()
    {
        if (!_isDragging) return;

        bool dropSuccessful = false;
        DroppableZone validZone = FindValidDropZone();

        Debug.Log($"尝试投放 {_currentDragItem.name} 到区域 {(validZone != null ? validZone.name : "无效区域")}, 代理情况{_currentProxyTag}");

        if (validZone != null && validZone.CanAcceptItem(_currentDragItem))
        {
            dropSuccessful = true;
            validZone.OnItemDrop(_currentDragItem);
            OnItemDropped?.Invoke(_currentDragItem, validZone);
        }

        // 处理代理拖拽结束
        if (_currentProxyTag != null)
        {
            Debug.Log("触发代理逻辑");
            Vector3 dropPosition = GetMouseWorldPosition();
            _currentProxyTag.HandleDragEnd(dropSuccessful, dropPosition);
            OnProxyDragEnd?.Invoke(_currentDragItem, _currentDragProxy);
        }
        else if (!dropSuccessful)
        {
            // 正常拖拽且失败时返回原位置
            ReturnToOriginalPosition();
        }

        // 清理状态
        CleanupDragState(dropSuccessful);
    }

    /// <summary>
    /// 结束拖拽 - 适配器方法（用于UnityEvent）
    /// </summary>
    /// <param name="eventData">指针事件数据（可选）</param>
    public void EndDragWithEventData(BaseEventData eventData = null)
    {
        EndDrag();
    }



    // ------------------------------
    // 内部逻辑
    // ------------------------------

    private void HandleDragging()
    {
        Vector3 targetPosition = GetMouseWorldPosition() + _offset;

        if (_currentProxyTag != null && _currentDragProxy != null)
        {
            // 代理拖拽运动
            if (useSmoothMovement)
            {
                _currentDragProxy.transform.position = Vector3.Lerp(
                    _currentDragProxy.transform.position,
                    targetPosition,
                    Time.deltaTime * smoothSpeed
                );
            }
            else
            {
                _currentDragProxy.transform.position = targetPosition;
            }
        }
        else if (_currentDragItem != null)
        {
            // 正常拖拽运动
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
    }

    private void SetProxyRenderOrder(GameObject proxy)
    {
        // 确保代理显示在最前面
        SpriteRenderer proxyRenderer = proxy.GetComponent<SpriteRenderer>();
        if (proxyRenderer != null)
        {
            proxyRenderer.sortingOrder = 1000;
        }
    }

    private void CleanupDragState(bool dropSuccessful)
    {
        //if (!dropSuccessful && _currentProxyTag == null)
        //{
        //    // 只有正常拖拽且失败时才恢复可用状态
        //    _currentDragItem.SetUsable(true);
        //}
        //else if (_currentProxyTag == null && dropSuccessful)
        //{
        //    // 正常拖拽成功，物品已经被放置区域处理
        //}

        _currentDragItem.SetUsable(true);

        OnDragEnd?.Invoke(_currentDragItem);
        _isDragging = false;
        _currentDragItem = null;
        _currentProxyTag = null;
        _currentDragProxy = null;
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
        _currentDragItem.transform.SetPositionAndRotation(
                       _currentDragItem.transform.position,
                                 Quaternion.identity
                                         );
        _currentDragItem.SetUsable(true);
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
