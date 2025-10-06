using UnityEngine;
using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;
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
    /// ��ʼ��ק - ����������������UnityEvent��ֻ��Ҫһ��������
    /// ����Ϊ�˽��UnityEvent����ƥ���������ӵķ���
    /// </summary>
    /// <param name="eventData">ָ���¼�����</param>
    public void StartDragForUI()
    {
        Debug.Log("StartDragForUI called");

        // ����¼�ϵͳ�Ƿ����
        if (EventSystem.current == null)
        {
            Debug.LogWarning("EventSystem not found in scene!");
            return;
        }

        // ͨ����ǰ�¼�ϵͳ��ȡ�����Ϣ
        PointerEventData currentEventData = new PointerEventData(EventSystem.current);
        currentEventData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(currentEventData, results);

        Debug.Log($"Raycast found {results.Count} results");

        if (results.Count > 0)
        {
            for (int i = 0; i < results.Count; i++)
            {
                Debug.Log($"Result {i}: {results[i].gameObject.name}");
            }

            GameObject clickedObject = results[0].gameObject;
            Debug.Log($"Clicked object: {clickedObject.name}");

            ClickableItem item = clickedObject.GetComponent<ClickableItem>();
            if (item != null)
            {
                Debug.Log($"Found ClickableItem: {item.name}, Draggable: {item.isDraggable}, Usable: {item.isUsable}");

                if (item.isDraggable && item.isUsable)
                {
                    Vector3 worldPos = eventCamera.ScreenToWorldPoint(Input.mousePosition);
                    worldPos.z = dragOffsetZ;
                    Debug.Log($"Starting drag at position: {worldPos}");
                    StartDrag(item, worldPos);
                }
            }
            else
            {
                Debug.Log("No ClickableItem component found on clicked object");
            }
        }
        else
        {
            Debug.Log("No raycast results - check colliders and layers");
        }
    }

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

        // ������קƫ����
        Vector3 worldPos = GetMouseWorldPosition();
        _offset = _currentDragItem.GetPosition() - worldPos;
        _offset.z = dragOffsetZ; // ����һ�µ�Z��ƫ��

        // ������Ⱦ�㼶�����UIԪ�أ�
        if (_currentDragItem.transform is RectTransform rectTransform)
        {
            rectTransform.SetAsLastSibling();
        }
        else
        {
            // Ϊ2D���������΢��תЧ������ǿ��ק��
            _currentDragItem.transform.SetPositionAndRotation(
                _currentDragItem.transform.position,
                Quaternion.Euler(0, 0, UnityEngine.Random.Range(-5f, 5f))
            );
        }

        // ������ʼ��ק�¼�
        OnDragStart?.Invoke(_currentDragItem);

        // ��ԭ��Ʒ��Ϊ������״̬
        _currentDragItem.SetUsable(false);
    }

    /// <summary>
    /// ������ק��ͨ�����ɿ�������ײ������
    /// </summary>
    public void EndDrag()
    {
        if (!_isDragging) return;

        // ������Ч�ķ�������
        DroppableZone validZone = FindValidDropZone();

        if (validZone != null)
        {
            // ���óɹ���ִ�з����߼�
            validZone.OnItemDrop(_currentDragItem);
            OnItemDropped?.Invoke(_currentDragItem, validZone);
        }
        else
        {
            // ����ʧ�ܣ�����ԭλ��
            ReturnToOriginalPosition();
        }

        // ����״̬
        OnDragEnd?.Invoke(_currentDragItem);
        _isDragging = false;
        _currentDragItem = null;
    }

    /// <summary>
    /// ������ק - ����������������UnityEvent��
    /// </summary>
    /// <param name="eventData">ָ���¼����ݣ���ѡ��</param>
    public void EndDragWithEventData(BaseEventData eventData = null)
    {
        EndDrag();
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
        _currentDragItem.transform.SetPositionAndRotation(
                       _currentDragItem.transform.position,
                                 Quaternion.identity
                                         );
        _currentDragItem.SetUsable(true);
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
