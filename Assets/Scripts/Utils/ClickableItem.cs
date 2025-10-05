using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// �ɵ���������
/// ����ʳ�ġ��롢�������˿͵Ƚ�������
/// ֧�ֵ����˫������ק������� DragAndDropHandler��
/// </summary>
public class ClickableItem : MonoBehaviour
{
    [Header("��������")]
    public bool isDraggable = false;       // �Ƿ����ק
    public bool isUsable = true;           // �Ƿ�ɽ������ɶ�̬�رգ�
    public float clickCooldown = 0.3f;     // ���󴥣��������ȴʱ��

    [Header("�Ӿ�����")]
    public bool showHighlightOnHover = true;
    public Color hoverColor = new Color(1f, 1f, 1f, 0.2f); // ������ɫ(���ڵ�����
    private Material originalMaterial;
    private Color originalColor;

    [Header("�¼��ص�")]
    public UnityEvent OnClicked;           // �����¼�
    public UnityEvent OnDoubleClicked;     // ˫���¼�����ѡ��
    public UnityEvent OnHoverEnter;        // ������
    public UnityEvent OnHoverExit;         // ����뿪

    // ״̬����
    private bool canClick = true;
    private bool isHovering = false;
    private float lastClickTime = 0f;
    private const float DoubleClickThreshold = 0.3f; // ˫���ж�ʱ�䴰��

    // �������
    private Collider2D _collider2D;
    private Renderer _renderer; // ͨ�� Renderer������ SpriteRenderer / MeshRenderer
    private CanvasGroup _canvasGroup;

    // ------------------------------
    // Unity �������ں���
    // ------------------------------

    protected virtual void Awake()
    {
        // ���泣�����
        _collider2D = GetComponent<Collider2D>();
        _renderer = GetComponent<Renderer>();
        _canvasGroup = GetComponent<CanvasGroup>();

        if (_renderer != null)
        {
            originalColor = _renderer.material.color;
        }
    }

    protected virtual void Update()
    {
        // ˫�����
        if (!canClick && Time.time - lastClickTime > DoubleClickThreshold)
        {
            canClick = true;
        }
    }

    // ------------------------------
    // ��꽻��������ͨ�� EventSystem �����ߵ��ã�
    // ------------------------------

    /// <summary>
    /// ������
    /// </summary>
    public virtual void OnPointerEnter()
    {
        if (!isUsable) return;

        isHovering = true;
        OnHoverEnter?.Invoke();

        if (showHighlightOnHover && _renderer != null)
        {
            Color targetColor = originalColor + hoverColor;
            targetColor.a = Mathf.Min(1f, targetColor.a);
            _renderer.material.color = targetColor;
        }

        // �����UIԪ�أ�Ҳ���޸� CanvasGroup.alpha
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0.9f;
        }
    }

    /// <summary>
    /// ����뿪
    /// </summary>
    public virtual void OnPointerExit()
    {
        isHovering = false;
        OnHoverExit?.Invoke();

        if (showHighlightOnHover && _renderer != null)
        {
            _renderer.material.color = originalColor;
        }

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 1f;
        }
    }

    /// <summary>
    /// ��갴�£���ʼ�������ק��
    /// </summary>
    public virtual void OnPointerDown()
    {
        if (!isUsable || !canClick) return;

        // �������ק������ DragAndDropHandler ����
        if (isDraggable)
        {
            return; // ���ڴ˴��������߼�
        }

        // ������Ϊ�������
        HandleClick();
    }

    /// <summary>
    /// ���̧����ɵ����
    /// </summary>
    public virtual void OnPointerUp()
    {
        if (!isUsable || !canClick) return;

        // ����ǿ���ק������ DragHandler �д����ͷ�
        if (!isDraggable)
        {
            HandleClick();
        }
    }

    // ------------------------------
    // ���ĵ���߼�
    // ------------------------------

    private void HandleClick()
    {
        float timeSinceLastClick = Time.time - lastClickTime;

        // �ж�˫��
        if (timeSinceLastClick <= DoubleClickThreshold)
        {
            OnDoubleClicked?.Invoke();
            canClick = false;
            lastClickTime = 0f;
            Invoke(nameof(ResetClickState), clickCooldown);
            return;
        }

        // ����
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
    // �������������ⲿ���ã�
    // ------------------------------

    /// <summary>
    /// ǿ�ƴ�������¼������Ի��Զ�����ʹ�ã�
    /// </summary>
    public virtual void ForceClick()
    {
        if (isUsable && canClick)
        {
            HandleClick();
        }
    }

    /// <summary>
    /// ����/���ý���
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
            OnPointerExit(); // ������ͣ״̬
        }
    }

    /// <summary>
    /// ��ȡ��ǰ�Ƿ��������ͣ״̬
    /// </summary>
    public bool IsHovering() => isHovering;

    /// <summary>
    /// ��ȡ�������������λ�ã������ƶ���λ��
    /// </summary>
    public Vector3 GetPosition()
    {
        return transform.position;
    }
}