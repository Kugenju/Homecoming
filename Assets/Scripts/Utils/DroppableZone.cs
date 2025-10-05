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

    protected virtual void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        if (_renderer != null)
        {
            _originalColor = _renderer.color;
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