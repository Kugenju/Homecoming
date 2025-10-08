using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections; // ��� Coroutine ֧��

/// <summary>
/// �ɵ����Ʒ���
/// ֧�������ͣ�������˫������ק����
/// ��Ҫ�� DragAndDropHandler ���ʹ��
/// </summary>
public class ClickableItem : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler,
    IBeginDragHandler,    // �����ק��ʼ�ӿ�
    IDragHandler,         // �����ק�нӿ�
    IEndDragHandler       // �����ק�����ӿ�
{
    [Header("��������")]
    public bool isDraggable = false;       // �Ƿ����ק
    public bool isUsable = true;           // �Ƿ���ã�Ӱ�����н���״̬��
    public float clickCooldown = 0.3f;     // �����ȴʱ��

    [Header("�Ӿ�Ч��")]
    public bool showHighlightOnHover = true;
    public Color hoverColor = new Color(1f, 1f, 1f, 0.2f); // ��ͣ��ɫ(͸������)
    private Material originalMaterial;
    private Color originalColor;

    [Header("�¼�����")]
    public UnityEvent OnClicked;           // �����¼�
    public UnityEvent OnDoubleClicked;     // ˫���¼�����ѡ���ã�
    public UnityEvent OnHoverEnter;        // ��ͣ����
    public UnityEvent OnHoverExit;         // ��ͣ�˳�
    public UnityEvent OnDragStart;         // ��ק��ʼ�¼�
    public UnityEvent OnDragEnd;           // ��ק�����¼�

    // ״̬����
    private bool canClick = true;
    private bool isHovering = false;
    private float lastClickTime = 0f;
    private const float DoubleClickThreshold = 0.3f; // ˫���ж�ʱ����ֵ

    // �������
    private Collider2D _collider2D;
    private Renderer _renderer; // ͨ�� Renderer (֧�� SpriteRenderer / MeshRenderer)
    private CanvasGroup _canvasGroup;

    // ------------------------------
    // Unity �������ڷ���
    // ------------------------------

    protected virtual void Awake()
    {
        // ��ȡ��Ҫ���
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
        // ˫�������ȴ
        if (!canClick && Time.time - lastClickTime > DoubleClickThreshold)
        {
            canClick = true;
        }
    }

    // ------------------------------
    // ָ���¼��ӿ�ʵ�֣�Unity EventSystem �Զ����ã�
    // ------------------------------

    /// <summary>
    /// ��������������
    /// </summary>
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        //Debug.Log($"OnPointerEnter ������ {gameObject.name}");
        if (!isUsable)
        {
            Debug.Log("��Ʒ������ isUsable = false");
            return;
        }

        isHovering = true;
        OnHoverEnter?.Invoke();

        ApplyHoverVisuals();
    }

    /// <summary>
    /// ����뿪��������
    /// </summary>
    public virtual void OnPointerExit(PointerEventData eventData)
    {
        //Debug.Log($"OnPointerExit ������ {gameObject.name}");
        isHovering = false;
        OnHoverExit?.Invoke();

        RemoveHoverVisuals();
    }

    /// <summary>
    /// ��갴��
    /// </summary>
    public virtual void OnPointerDown(PointerEventData eventData)
    {
        //Debug.Log($"OnPointerDown ������ {gameObject.name}");
        if (!isUsable || !canClick) return;

        if (isDraggable)
        {
            // ����ק��Ʒ�İ����¼�����קϵͳ����
            return;
        }

        HandleClick();
    }

    /// <summary>
    /// ����ͷ�
    /// </summary>
    public virtual void OnPointerUp(PointerEventData eventData)
    {
        //Debug.Log($"OnPointerUp ������ {gameObject.name}");
        if (!isUsable || !canClick) return;

        if (!isDraggable)
        {
            HandleClick();
        }
    }

    /// <summary>
    /// ��ʼ��ק��Unity EventSystem �ӿڣ�
    /// </summary>
    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        if (!isUsable || !isDraggable) return;

        //Debug.Log($"OnBeginDrag ������ {gameObject.name}");
        OnDragStart?.Invoke();

        // ֪ͨ��ק��������ʼ��ק
        if (DragAndDropHandler.Instance != null)
        {
            Vector3 worldPos = GetWorldPositionFromEventData(eventData);
            DragAndDropHandler.Instance.StartDrag(this, worldPos);
        }
    }

    /// <summary>
    /// ��ק�У�Unity EventSystem �ӿڣ�
    /// </summary>
    public virtual void OnDrag(PointerEventData eventdata)
    {
        if (!isDraggable) return;

        // ��ק�еĴ����� draganddrophandler ����
    }

    /// <summary>
    /// ������ק��Unity EventSystem �ӿڣ�
    /// </summary>
    public virtual void OnEndDrag(PointerEventData eventData)
    {
        if (!isDraggable) return;

        Debug.Log($"OnEndDrag ������ {gameObject.name}");
        OnDragEnd?.Invoke();

        // ֪ͨ��ק������������ק
        if (DragAndDropHandler.Instance != null)
        {
            DragAndDropHandler.Instance.EndDrag();
        }
    }

    // ------------------------------
    // �Ӿ�Ч������
    // ------------------------------

    private void ApplyHoverVisuals()
    {
        if (showHighlightOnHover && _renderer != null)
        {
            // ��������ʵ���Ա���Ӱ����������
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
            // �ָ�ԭʼ����
            _renderer.material = originalMaterial;
        }

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 1f;
        }
    }

    // ------------------------------
    // ��������߼�
    // ------------------------------

    private void HandleClick()
    {
        float timeSinceLastClick = Time.time - lastClickTime;

        // ���˫��
        if (timeSinceLastClick <= DoubleClickThreshold)
        {
            Debug.Log("˫�����ɹ�");
            OnDoubleClicked?.Invoke();
            canClick = false;
            lastClickTime = 0f;
            StartCoroutine(ResetClickStateAfterDelay(clickCooldown));
            return;
        }

        // ��������
        Debug.Log("��������");
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
    // ���߷���
    // ------------------------------

    private Vector3 GetWorldPositionFromEventData(PointerEventData eventData)
    {
        if (eventData == null) return Vector3.zero;

        Vector3 screenPos = eventData.position;
        screenPos.z = 10f; // Ĭ��Z��ƫ��
        return Camera.main.ScreenToWorldPoint(screenPos);
    }

    // ------------------------------
    // �ⲿ���ýӿ�
    // ------------------------------

    /// <summary>
    /// ǿ�ƴ�������¼��������ڳ�����ƣ�
    /// </summary>
    public virtual void ForceClick()
    {
        if (isUsable && canClick)
        {
            HandleClick();
        }
    }

    /// <summary>
    /// ������Ʒ����״̬
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
            OnPointerExit(null); // ������ͣ״̬
        }
    }

    /// <summary>
    /// ��ȡ��ǰ�Ƿ�����ͣ״̬
    /// </summary>
    public bool IsHovering() => isHovering;

    /// <summary>
    /// ��ȡ��Ʒλ�ã��������꣩
    /// </summary>
    public Vector3 GetPosition() => transform.position;

    /// <summary>
    /// �ֶ�������ͣ���루���ڳ�����ƣ�
    /// </summary>
    public virtual void ManualPointerEnter()
    {
        OnPointerEnter(null);
    }

    /// <summary>
    /// �ֶ�������ͣ�˳������ڳ�����ƣ�
    /// </summary>
    public virtual void ManualPointerExit()
    {
        OnPointerExit(null);
    }
}