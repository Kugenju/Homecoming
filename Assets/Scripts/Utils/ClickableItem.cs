using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
/// <summary>
/// �ɵ���������
/// ����ʳ�ġ��롢�������˿͵Ƚ�������
/// ֧�ֵ����˫������ק������� DragAndDropHandler��
/// </summary>
public class ClickableItem : MonoBehaviour,IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
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
        //Debug.Log($"ClickableItem Awake on {gameObject.name}");
        // ���泣�����
        _collider2D = GetComponent<Collider2D>();
        _renderer = GetComponent<Renderer>();
        _canvasGroup = GetComponent<CanvasGroup>();

        if (_renderer != null)
        {
            originalMaterial = _renderer.material; // ����ԭʼ����
            originalColor = _renderer.material.color;
        }
        Debug.Log($"{gameObject.name} ���״̬: Collider2D={_collider2D != null}, Renderer={_renderer != null}");
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
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"OnPointerEnter ������ on {gameObject.name}");
        if (!isUsable)
        {
            Debug.Log("�� isUsable = false��������");
            return;
        }

        isHovering = true;
        OnHoverEnter?.Invoke();

        if (showHighlightOnHover && _renderer != null)
        {
            // �����µĲ���ʵ������Ӱ����������
            _renderer.material = new Material(_renderer.material);
            Color targetColor = originalColor + hoverColor;
            targetColor.a = Mathf.Min(1f, targetColor.a);
            _renderer.material.color = targetColor;
            Debug.Log($"������ɫӦ��: {targetColor}");
        }

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0.9f;
        }
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log($"OnPointerExit ������ on {gameObject.name}");
        isHovering = false;
        OnHoverExit?.Invoke();

        if (showHighlightOnHover && _renderer != null && originalMaterial != null)
        {
            _renderer.material.color = originalColor;
            // �ָ�ԭʼ����
            _renderer.material = originalMaterial;
        }

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 1f;
        }
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log($"OnPointerDown ������ on {gameObject.name}");
        if (!isUsable || !canClick) return;

        if (isDraggable)
        {
            return;
        }

        HandleClick();
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log($"OnPointerUp ������ on {gameObject.name}");
        if (!isUsable || !canClick) return;

        if (!isDraggable)
        {
            HandleClick();
        }
    }
    /// <summary>
    /// �����루���ⲿ���ã�
    /// </summary>
    public virtual void OnPointerEnter()
    {
        // ����������Ա������ⲿ������ã����ڲ��¼�����ʹ�ýӿڰ汾
        OnPointerEnter(null);
    }

    /// <summary>
    /// ����뿪�����ⲿ���ã�
    /// </summary>
    public virtual void OnPointerExit()
    {
        OnPointerExit(null);
    }

    /// <summary>
    /// ��갴�£����ⲿ���ã�
    /// </summary>
    public virtual void OnPointerDown()
    {
        OnPointerDown(null);
    }

    /// <summary>
    /// ���̧�𣨹��ⲿ���ã�
    /// </summary>
    public virtual void OnPointerUp()
    {
        OnPointerUp(null);
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