using UnityEngine;
using System;
using UnityEngine.Events;
/// <summary>
/// ͨ����קϵͳ
/// ֧��������� ClickableItem �����������ק
/// ��������ЧͶ������DroppableZone�����������Զ���ص�
/// </summary>
public class DragAndDropHandler : MonoBehaviour
{
    // ����ģʽ��ȫ��Ψһ��ק������
    public static DragAndDropHandler Instance { get; private set; }

    [Header("��ק����")]
    public Camera eventCamera; // ������Ļת��������������
    public float dragOffsetZ = 10f; // ������ק������ Z ��ƫ�ƣ�����������UI/�����ڵ���

    [Header("�Ӿ�����")]
    public bool useSmoothMovement = true;
    public float smoothSpeed = 8f;

    // ��ǰ��ק״̬
    private ClickableItem _currentDragItem;
    private Vector3 _offset;
    private bool _isDragging = false;
    private Transform _originalParent; // ��¼ԭʼ���������ڷ��أ�
    private Vector3 _originalPosition;

    // �ص��¼����������ⲿ������
    [Serializable]
    public class DragEvent : UnityEvent<ClickableItem> { }

    public DragEvent OnDragStart;
    public DragEvent OnDragEnd;
    public UnityEvent<ClickableItem, DroppableZone> OnItemDropped;

    // ------------------------------
    // Unity �������ں���
    // ------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Ĭ��ʹ���������
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
    // �ⲿ�ӿڣ��� ClickableItem ����
    // ------------------------------

    /// <summary>
    /// ��ʼ��ק���� ClickableItem ���ã�
    /// </summary>
    /// <param name="item">����ק������</param>
    /// <param name="clickPointInWorld">�����ʱ����������</param>
    public void StartDrag(ClickableItem item, Vector3 clickPointInWorld)
    {
        if (item == null || !item.isDraggable || !item.isUsable) return;

        _currentDragItem = item;
        _originalParent = item.transform.parent;
        _originalPosition = item.transform.position;

        // ������ק״̬
        _isDragging = true;

        // ������קƫ����������������λ�ã�
        Vector3 worldPos = GetMouseWorldPosition();
        _offset = _currentDragItem.GetPosition() - worldPos;
        _offset.z = dragOffsetZ; // ��΢������һ��

        // �����㼶����ѡ���ƶ���Canvas�������ʱ������
        if (_currentDragItem.transform is RectTransform rectTransform)
        {
            rectTransform.SetAsLastSibling();
        }
        else
        {
            _currentDragItem.transform.SetPositionAndRotation(
                _currentDragItem.transform.position,
                Quaternion.Euler(0, 0, UnityEngine.Random.Range(-5f, 5f)) // ��΢��ת������ʵ��
            );
        }

        // ������ʼ�¼�
        OnDragStart?.Invoke(_currentDragItem);

        // ����ԭ�����Ϊ
        _currentDragItem.SetUsable(false);
    }

    /// <summary>
    /// ������ק��ͨ�����ɿ�������ײ������
    /// </summary>
    public void EndDrag()
    {
        if (!_isDragging) return;

        // ����Ͷ�ŵ�ĳ������
        DroppableZone validZone = FindValidDropZone();

        if (validZone != null)
        {
            // Ͷ�ųɹ���ִ��Ͷ���߼�
            validZone.OnItemDrop(_currentDragItem);
            OnItemDropped?.Invoke(_currentDragItem, validZone);
        }
        else
        {
            // Ͷ��ʧ�ܣ�����ԭλ
            ReturnToOriginalPosition();
        }

        // ����״̬
        OnDragEnd?.Invoke(_currentDragItem);
        _isDragging = false;
        _currentDragItem = null;
    }

    // ------------------------------
    // �ڲ��߼�
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
    /// ��ȡ��굱ǰ������ռ��е�λ��
    /// </summary>
    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = dragOffsetZ; // ʹ�ù̶����
        return eventCamera.ScreenToWorldPoint(mouseScreenPos);
    }

    /// <summary>
    /// ���ҵ�ǰ�Ƿ���ͣ��һ����Ч��Ͷ����
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
    /// ����ԭʼλ�ã�Ͷ��ʧ�ܣ�
    /// </summary>
    private void ReturnToOriginalPosition()
    {
        if (_currentDragItem == null) return;

        _currentDragItem.transform.position = _originalPosition;
        _currentDragItem.transform.SetParent(_originalParent);
    }

    // ------------------------------
    // �������������ⲿ���ã�
    // ------------------------------

    /// <summary>
    /// �Ƿ�������קĳ������
    /// </summary>
    public bool IsDragging() => _isDragging;

    /// <summary>
    /// ��ȡ��ǰ����ק����Ʒ
    /// </summary>
    public ClickableItem GetCurrentDragItem() => _currentDragItem;

    /// <summary>
    /// ǿ�ƽ�����ק������˿��뿪ʱ��
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
