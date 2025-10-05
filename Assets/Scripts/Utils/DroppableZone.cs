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

    protected virtual void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        if (_renderer != null)
        {
            _originalColor = _renderer.color;
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