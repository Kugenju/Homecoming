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

    [Header("������ק")]
    [Tooltip("�Ƿ����ô�����ק����")]
    public bool enableProxyDragging = true;

    // ��ǰ��ק״̬
    private ClickableItem _currentDragItem;
    private ProxyDragTag _currentProxyTag;
    private GameObject _currentDragProxy;
    private Vector3 _offset;
    private bool _isDragging = false;
    private Transform _originalParent;
    private Vector3 _originalPosition;

    // �ص��¼����������ⲿ������
    [System.Serializable] public class DragEvent : UnityEngine.Events.UnityEvent<ClickableItem> { }
    [System.Serializable] public class ProxyDragEvent : UnityEngine.Events.UnityEvent<ClickableItem, GameObject> { }

    public DragEvent OnDragStart;
    public DragEvent OnDragEnd;
    public ProxyDragEvent OnProxyDragStart;
    public ProxyDragEvent OnProxyDragEnd;
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
 

    public void StartDrag(ClickableItem item, Vector3 clickPointInWorld)
    {
        if (item == null || !item.isDraggable || !item.isUsable) return;

        _currentDragItem = item;
        _originalParent = item.transform.parent;
        _originalPosition = item.transform.position;

        // ��������ק
        if (enableProxyDragging)
        {
            _currentProxyTag = item.GetComponent<ProxyDragTag>();
            if (_currentProxyTag != null && _currentProxyTag.proxyPrefab != null)
            {
                StartProxyDrag(clickPointInWorld);
                return;
            }
        }

        // ������ק
        StartNormalDrag(clickPointInWorld);
    }

    private void StartProxyDrag(Vector3 clickPointInWorld)
    {
        _currentDragProxy = _currentProxyTag.CreateProxy(clickPointInWorld);

        if (_currentDragProxy != null)
        {
            // ���ô���Ĳ㼶
            SetProxyRenderOrder(_currentDragProxy);

            // ����ƫ���������ڴ���
            Vector3 worldPos = GetMouseWorldPosition();
            _offset = _currentDragProxy.transform.position - worldPos;
            _offset.z = dragOffsetZ;

            _isDragging = true;
            OnProxyDragStart?.Invoke(_currentDragItem, _currentDragProxy);
            OnDragStart?.Invoke(_currentDragItem);

            // ����ԭ�������ײ��
            _currentDragItem.SetUsable(false);
        }
        else
        {
            // ������ʧ�ܣ����˵�������ק
            StartNormalDrag(clickPointInWorld);
        }
    }

    private void StartNormalDrag(Vector3 clickPointInWorld)
    {
        _isDragging = true;

        // ����ƫ����
        Vector3 worldPos = GetMouseWorldPosition();
        _offset = _currentDragItem.GetPosition() - worldPos;
        _offset.z = dragOffsetZ;

        // ������Ⱦ˳��
        if (_currentDragItem.transform is RectTransform rectTransform)
        {
            rectTransform.SetAsLastSibling();
        }

        OnDragStart?.Invoke(_currentDragItem);
    }



    /// <summary>
    /// ������ק��ͨ�����ɿ�������ײ������
    /// </summary>
    /// <summary>
    /// ������ק
    /// </summary>
    public void EndDrag()
    {
        if (!_isDragging) return;

        bool dropSuccessful = false;
        DroppableZone validZone = FindValidDropZone();

        Debug.Log($"����Ͷ�� {_currentDragItem.name} ������ {(validZone != null ? validZone.name : "��Ч����")}, �������{_currentProxyTag}");

        if (validZone != null && validZone.CanAcceptItem(_currentDragItem))
        {
            dropSuccessful = true;
            validZone.OnItemDrop(_currentDragItem);
            OnItemDropped?.Invoke(_currentDragItem, validZone);
        }

        // ���������ק����
        if (_currentProxyTag != null)
        {
            Debug.Log("���������߼�");
            Vector3 dropPosition = GetMouseWorldPosition();
            _currentProxyTag.HandleDragEnd(dropSuccessful, dropPosition);
            OnProxyDragEnd?.Invoke(_currentDragItem, _currentDragProxy);
        }
        else if (!dropSuccessful)
        {
            // ������ק��ʧ��ʱ����ԭλ��
            ReturnToOriginalPosition();
        }

        // ����״̬
        CleanupDragState(dropSuccessful);
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

        if (_currentProxyTag != null && _currentDragProxy != null)
        {
            // ������ק�˶�
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
            // ������ק�˶�
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
        // ȷ��������ʾ����ǰ��
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
        //    // ֻ��������ק��ʧ��ʱ�Żָ�����״̬
        //    _currentDragItem.SetUsable(true);
        //}
        //else if (_currentProxyTag == null && dropSuccessful)
        //{
        //    // ������ק�ɹ�����Ʒ�Ѿ�������������
        //}

        _currentDragItem.SetUsable(true);

        OnDragEnd?.Invoke(_currentDragItem);
        _isDragging = false;
        _currentDragItem = null;
        _currentProxyTag = null;
        _currentDragProxy = null;
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
