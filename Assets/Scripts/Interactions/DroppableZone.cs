using UnityEngine;

/// <summary>
/// 投放区域基类（如：肉馅区、蒸笼口、汤碗等）
/// 当拖拽物体进入此区域并释放时触发逻辑
/// </summary>
public class DroppableZone : MonoBehaviour
{
    [Header("基础设置")]
    public string acceptTag = "Ingredient"; // 可接受的标签（如"Meat", "Skin"等）
    public bool onlyAcceptSpecificItems = false;
    public ClickableItem specificItem; // 如果只接受特定物品

    [Header("视觉反馈")]
    public bool showHighlight = true;
    public Color highlightColor = Color.yellow;
    private Color _originalColor;
    private SpriteRenderer _renderer;

    [Header("层级管理")]
    [Tooltip("投放区域所在的层级")]
    public string dropZoneLayer = "DropZones";
    [Tooltip("可拖拽物品所在的层级")]
    public string draggableItemLayer = "DraggableItems";
    [Tooltip("是否自动设置层级")]
    public bool autoSetupLayers = true;

    protected virtual void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        if (_renderer != null)
        {
            _originalColor = _renderer.color;
        }

        if (autoSetupLayers)
        {
            SetupLayers();
        }
    }

    /// <summary>
    /// 设置投放区域和可拖拽物品的层级
    /// </summary>
    protected virtual void SetupLayers()
    {
        // 设置自身为投放区域层
        if (!string.IsNullOrEmpty(dropZoneLayer))
        {
            int layer = LayerMask.NameToLayer(dropZoneLayer);
            if (layer != -1)
            {
                gameObject.layer = layer;
            }
            else
            {
                Debug.LogWarning($"层级 '{dropZoneLayer}' 不存在，请检查层级设置");
            }
        }

        // 查找并设置可拖拽子物体的层级
        SetupDraggableChildren();
    }

    /// <summary>
    /// 设置可拖拽子物体的层级
    /// </summary>
    protected virtual void SetupDraggableChildren()
    {
        if (string.IsNullOrEmpty(draggableItemLayer)) return;

        int draggableLayer = LayerMask.NameToLayer(draggableItemLayer);
        if (draggableLayer == -1) return;

        // 查找所有可能有拖拽需求的子物体
        ClickableItem[] draggableItems = GetComponentsInChildren<ClickableItem>();
        foreach (ClickableItem item in draggableItems)
        {
            if (item.isDraggable)
            {
                SetLayerRecursively(item.gameObject, draggableLayer);
            }
        }
    }

    /// <summary>
    /// 为指定物体设置 ClickableItem 组件和层级
    /// </summary>
    public virtual void SetupDraggableObject(GameObject targetObject, bool makeDraggable = true)
    {
        if (targetObject == null) return;

        // 设置层级
        if (!string.IsNullOrEmpty(draggableItemLayer))
        {
            int layer = LayerMask.NameToLayer(draggableItemLayer);
            if (layer != -1)
            {
                SetLayerRecursively(targetObject, layer);
            }
        }

        // 设置 ClickableItem 组件
        ClickableItem clickableItem = targetObject.GetComponent<ClickableItem>();
        if (clickableItem == null)
        {
            clickableItem = targetObject.AddComponent<ClickableItem>();
        }

        if (makeDraggable)
        {
            clickableItem.isDraggable = true;
            clickableItem.isUsable = true;
        }

        // 确保有 Collider
        EnsureColliderExists(targetObject);
    }

    /// <summary>
    /// 确保物体有合适的 Collider
    /// </summary>
    protected virtual void EnsureColliderExists(GameObject targetObject)
    {
        Collider2D existingCollider = targetObject.GetComponent<Collider2D>();
        if (existingCollider != null) return;

        // 根据物体类型添加合适的 Collider
        SpriteRenderer spriteRenderer = targetObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            PolygonCollider2D polyCollider = targetObject.AddComponent<PolygonCollider2D>();
            polyCollider.isTrigger = true;
        }
        else
        {
            BoxCollider2D boxCollider = targetObject.AddComponent<BoxCollider2D>();
            boxCollider.isTrigger = true;
        }
    }

    /// <summary>
    /// 递归设置物体及其所有子物体的层级
    /// </summary>
    protected void SetLayerRecursively(GameObject obj, int layer)
    {
        if (obj == null) return;

        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }


    /// <summary>
    /// 判断该区域是否接受此物品
    /// </summary>
    public virtual bool CanAcceptItem(ClickableItem item)
    {
        if (item == null) return false;

        if (onlyAcceptSpecificItems)
        {
            return item == specificItem;
        }

        return item.CompareTag(acceptTag);
    }

    /// <summary>
    /// 当物品被投放到此区域时调用
    /// 子类应重写此方法实现具体逻辑
    /// </summary>
    public virtual void OnItemDrop(ClickableItem item)
    {
        Debug.Log($"{item.name} 被投放到 {name}");

        // 默认播放反馈
        PlayFeedback();

        // 示例：销毁原料，生成新对象（如包馅）
        // 由子类具体实现
    }

    /// <summary>
    /// 视觉反馈
    /// </summary>
    protected virtual void PlayFeedback()
    {
        if (showHighlight && _renderer != null)
        {
            _renderer.color = highlightColor;
            Invoke(nameof(ResetColor), 0.2f);
        }
    }

    private void ResetColor()
    {
        if (_renderer != null)
        {
            _renderer.color = _originalColor;
        }
    }
}