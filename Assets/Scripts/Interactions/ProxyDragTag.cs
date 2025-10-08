using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ProxyDragTag;

/// <summary>
/// �����Ҫ������ק����Ʒ���ṩ�ḻ������ѡ��
/// </summary>
public class ProxyDragTag : MonoBehaviour
{
    public enum DropBehavior
    {
        None,
        ReturnToOriginal,    // �ع�ԭλ��
        DestroyProxy,        // ���ٴ���
        StayAtDropPosition,  // �����ڷ���λ��
        CustomBehavior       // �Զ�����Ϊ
    }

    [Header("��������")]
    [Tooltip("��קʱ��ʾ�Ĵ���Ԥ����")]
    public GameObject proxyPrefab;

    [Tooltip("�ɹ����ú�Ĵ���ʽ������ȫ�����ã�")]
    public DropBehavior successDropBehavior = DropBehavior.CustomBehavior;

    [Tooltip("ȡ�����ú�Ĵ���ʽ������ȫ�����ã�")]
    public DropBehavior cancelDropBehavior = DropBehavior.CustomBehavior;

    [Header("�Ӿ�Ч��")]
    [Tooltip("��קʱԭ�����Ƿ�����")]
    public bool hideOriginalDuringDrag = true;

    [Tooltip("��������ű���")]
    public Vector3 proxyScale = Vector3.one;

    [Tooltip("�����͸����")]
    [Range(0f, 1f)] public float proxyAlpha = 0.8f;


    [Header("�߼�����")]
    [Tooltip("�Ƿ���ԭ�����SpriteRenderer����")]
    public bool copySpriteProperties = true;

    [Tooltip("�Ƿ�������ק��תЧ��")]
    public bool enableRotationEffect = true;

    [Tooltip("�����ת�Ƕ�")]
    public float maxRotationAngle = 15f;

    [System.NonSerialized] private bool _isProxyActive;
    [System.NonSerialized] private GameObject _currentProxy;

    // ���Է�װ���ṩֻ�����ʣ�
    public bool IsProxyActive { get { return _isProxyActive; } }
    public GameObject CurrentProxy { get { return _currentProxy; } }

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;
    private bool originalActiveState;

    public GameObject CreateProxy(Vector3 position)
    {
        //Debug.Log("������������");
        if (proxyPrefab == null)
        {
            Debug.LogWarning($"ProxyPrefab is null on {gameObject.name}");
            return null;
        }

        // ����ԭ����״̬
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalScale = transform.localScale;
        originalActiveState = gameObject.activeSelf;

        // ��������ʵ����ֱ�Ӹ�ֵ���ֶΣ�
        _currentProxy = Instantiate(proxyPrefab, position, Quaternion.identity);
        _currentProxy.name = $"{gameObject.name}_Proxy";

        // Ӧ�ô�������
        ApplyProxySettings(_currentProxy);

        // ����ԭ����
        if (hideOriginalDuringDrag)
        {
            gameObject.SetActive(false);
        }

        _isProxyActive = true;
        //Debug.Log($"Created proxy for {gameObject.name} at {position}, _isProxyActive={_isProxyActive}, _currentProxy.name: {_currentProxy}");
        return _currentProxy;
    }

    private void ApplyProxySettings(GameObject proxy)
    {
        // Ӧ������
        proxy.transform.localScale = proxyScale;

        // Ӧ��͸����
        SpriteRenderer proxyRenderer = proxy.GetComponent<SpriteRenderer>();
        if (proxyRenderer != null)
        {
            Color color = proxyRenderer.color;
            color.a = proxyAlpha;
            proxyRenderer.color = color;
        }

        // ����Sprite����
        if (copySpriteProperties)
        {
            SpriteRenderer originalRenderer = GetComponent<SpriteRenderer>();
            if (originalRenderer != null && proxyRenderer != null)
            {
                proxyRenderer.sprite = originalRenderer.sprite;
                proxyRenderer.color = new Color(
                    originalRenderer.color.r,
                    originalRenderer.color.g,
                    originalRenderer.color.b,
                    proxyAlpha
                );
            }
        }

        // �����תЧ�����
        if (enableRotationEffect)
        {
            var rotationEffect = proxy.AddComponent<ProxyRotationEffect>();
            rotationEffect.maxAngle = maxRotationAngle;
        }
    }

    /// <summary>
    /// ������ק����
    /// </summary>
    public void HandleDragEnd(bool dropSuccessful, Vector3 dropPosition)
    {
        Debug.Log($"HandleDragEnd called on {gameObject.name}, successful={dropSuccessful},_isProxyActive={_isProxyActive}, _currentProxy={_currentProxy}");
        if (!_isProxyActive || _currentProxy == null) return;

        DropBehavior behavior = GetFinalDropBehavior(dropSuccessful);
        Debug.Log($"Drag ended on {gameObject.name}, successful={dropSuccessful}, behavior={behavior}");
        switch (behavior)
        {
            case DropBehavior.ReturnToOriginal:
                ReturnProxyToOriginal();
                break;

            case DropBehavior.DestroyProxy:
                DestroyProxy();
                break;

            case DropBehavior.StayAtDropPosition:
                KeepProxyAtPosition(dropPosition);
                break;

            case DropBehavior.CustomBehavior:
                HandleCustomBehavior(dropSuccessful, dropPosition);
                break;
        }

     

        // �ָ�ԭ����
        if (hideOriginalDuringDrag)
        {
            gameObject.SetActive(originalActiveState);
        }

        _isProxyActive = false;
        _currentProxy = null;
    }


    /// <summary>
    /// ȡ����ק��ǿ�ƽ�����
    /// </summary>
    public void CancelDrag()
    {
        if (_isProxyActive)
        {
            HandleDragEnd(false, originalPosition);
        }
    }

    private DropBehavior GetFinalDropBehavior(bool dropSuccessful)
    {
        if (dropSuccessful && successDropBehavior != DropBehavior.CustomBehavior)
            return successDropBehavior;

        if (!dropSuccessful && cancelDropBehavior != DropBehavior.CustomBehavior)
            return cancelDropBehavior;

        return DropBehavior.None;
    }

    private void ReturnProxyToOriginal()
    {
        if (_currentProxy != null)
        {
            _currentProxy.transform.position = originalPosition;
            _currentProxy.transform.rotation = originalRotation;
            _currentProxy.transform.localScale = originalScale;
            Destroy(_currentProxy, 0.1f);
        }
    }

    private void DestroyProxy()
    {
        if (_currentProxy != null)
        {
            Destroy(_currentProxy);
        }
    }

    private void KeepProxyAtPosition(Vector3 position)
    {
        if (_currentProxy != null)
        {
            _currentProxy.transform.position = position;
            // ���ֵ�ǰ��ת������
        }
    }

    private void HandleCustomBehavior(bool dropSuccessful, Vector3 dropPosition)
    {
        // ���������չ�Զ�����Ϊ
        // ���紥���¼������Ŷ�����
        Debug.Log($"Custom drop behavior: successful={dropSuccessful}, position={dropPosition}");

        // Ĭ����Ϊ���ɹ�ʱ������ȡ��ʱ����
        if (dropSuccessful)
        {
            KeepProxyAtPosition(dropPosition);
        }
        else
        {
            ReturnProxyToOriginal();
        }
    }

    private void OnDestroy()
    {
        // ������Դ
        if (_currentProxy != null)
        {
            Destroy(_currentProxy);
        }
    }
}

public class ProxyRotationEffect : MonoBehaviour
{
    public float maxAngle = 15f;
    public float rotationSpeed = 5f;

    private float currentRotation;
    private bool rotatingRight = true;

    private void Update()
    {
        if (rotatingRight)
        {
            currentRotation += rotationSpeed * Time.deltaTime;
            if (currentRotation >= maxAngle) rotatingRight = false;
        }
        else
        {
            currentRotation -= rotationSpeed * Time.deltaTime;
            if (currentRotation <= -maxAngle) rotatingRight = true;
        }

        transform.rotation = Quaternion.Euler(0, 0, currentRotation);
    }
}
// ���ڹ�������Ʒ��
// dropBehavior = ReturnToOriginal
// successDropBehavior = CustomBehavior (����Ĭ��)
// cancelDropBehavior = ReturnToOriginal

// ��������Ʒ����Ʒ��
// dropBehavior = DestroyProxy  
// successDropBehavior = DestroyProxy
// cancelDropBehavior = ReturnToOriginal

// ������Ҫ��ȷ��λ����Ʒ��
// dropBehavior = StayAtDropPosition
// hideOriginalDuringDrag = true