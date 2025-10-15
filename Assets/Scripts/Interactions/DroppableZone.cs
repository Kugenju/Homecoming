using UnityEngine;

/// <summary>
/// Ͷ��������ࣨ�磺�������������ڡ�����ȣ�
/// ����ק�������������ͷ�ʱ�����߼�
/// </summary>
public class DroppableZone : MonoBehaviour
{
    [Header("��������")]
    public string acceptTag = "Ingredient"; // �ɽ��ܵı�ǩ����"Meat", "Skin"�ȣ�
    public bool onlyAcceptSpecificItems = false;
    public ClickableItem specificItem; // ���ֻ�����ض���Ʒ

    [Header("�Ӿ�����")]
    public bool showHighlight = true;
    public Color highlightColor = Color.yellow;
    private Color _originalColor;
    private SpriteRenderer _renderer;

    [Header("�㼶����")]
    [Tooltip("Ͷ���������ڵĲ㼶")]
    public string dropZoneLayer = "DropZones";
    [Tooltip("����ק��Ʒ���ڵĲ㼶")]
    public string draggableItemLayer = "DraggableItems";
    [Tooltip("�Ƿ��Զ����ò㼶")]
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
    /// ����Ͷ������Ϳ���ק��Ʒ�Ĳ㼶
    /// </summary>
    protected virtual void SetupLayers()
    {
        // ��������ΪͶ�������
        if (!string.IsNullOrEmpty(dropZoneLayer))
        {
            int layer = LayerMask.NameToLayer(dropZoneLayer);
            if (layer != -1)
            {
                gameObject.layer = layer;
            }
            else
            {
                Debug.LogWarning($"�㼶 '{dropZoneLayer}' �����ڣ�����㼶����");
            }
        }

        // ���Ҳ����ÿ���ק������Ĳ㼶
        SetupDraggableChildren();
    }

    /// <summary>
    /// ���ÿ���ק������Ĳ㼶
    /// </summary>
    protected virtual void SetupDraggableChildren()
    {
        if (string.IsNullOrEmpty(draggableItemLayer)) return;

        int draggableLayer = LayerMask.NameToLayer(draggableItemLayer);
        if (draggableLayer == -1) return;

        // �������п�������ק�����������
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
    /// Ϊָ���������� ClickableItem ����Ͳ㼶
    /// </summary>
    public virtual void SetupDraggableObject(GameObject targetObject, bool makeDraggable = true)
    {
        if (targetObject == null) return;

        // ���ò㼶
        if (!string.IsNullOrEmpty(draggableItemLayer))
        {
            int layer = LayerMask.NameToLayer(draggableItemLayer);
            if (layer != -1)
            {
                SetLayerRecursively(targetObject, layer);
            }
        }

        // ���� ClickableItem ���
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

        // ȷ���� Collider
        EnsureColliderExists(targetObject);
    }

    /// <summary>
    /// ȷ�������к��ʵ� Collider
    /// </summary>
    protected virtual void EnsureColliderExists(GameObject targetObject)
    {
        Collider2D existingCollider = targetObject.GetComponent<Collider2D>();
        if (existingCollider != null) return;

        // ��������������Ӻ��ʵ� Collider
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
    /// �ݹ��������弰������������Ĳ㼶
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
    /// �жϸ������Ƿ���ܴ���Ʒ
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
    /// ����Ʒ��Ͷ�ŵ�������ʱ����
    /// ����Ӧ��д�˷���ʵ�־����߼�
    /// </summary>
    public virtual void OnItemDrop(ClickableItem item)
    {
        Debug.Log($"{item.name} ��Ͷ�ŵ� {name}");

        // Ĭ�ϲ��ŷ���
        PlayFeedback();

        // ʾ��������ԭ�ϣ������¶�������ڣ�
        // ���������ʵ��
    }

    /// <summary>
    /// �Ӿ�����
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