using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ProxyDragTag;

/// <summary>
/// 标记需要代理拖拽的物品，提供丰富的配置选项
/// </summary>
public class ProxyDragTag : MonoBehaviour
{
    public enum DropBehavior
    {
        None,
        ReturnToOriginal,    // 回归原位置
        DestroyProxy,        // 销毁代理
        StayAtDropPosition,  // 保留在放置位置
        CustomBehavior       // 自定义行为
    }

    [Header("代理设置")]
    [Tooltip("拖拽时显示的代理预制体")]
    public GameObject proxyPrefab;

    [Tooltip("成功放置后的处理方式（覆盖全局设置）")]
    public DropBehavior successDropBehavior = DropBehavior.CustomBehavior;

    [Tooltip("取消放置后的处理方式（覆盖全局设置）")]
    public DropBehavior cancelDropBehavior = DropBehavior.CustomBehavior;

    [Header("视觉效果")]
    [Tooltip("拖拽时原物体是否隐藏")]
    public bool hideOriginalDuringDrag = true;

    [Tooltip("代理的缩放比例")]
    public Vector3 proxyScale = Vector3.one;

    [Tooltip("代理的透明度")]
    [Range(0f, 1f)] public float proxyAlpha = 0.8f;


    [Header("高级设置")]
    [Tooltip("是否复制原物体的SpriteRenderer属性")]
    public bool copySpriteProperties = true;

    [Tooltip("是否启用拖拽旋转效果")]
    public bool enableRotationEffect = true;

    [Tooltip("最大旋转角度")]
    public float maxRotationAngle = 15f;

    [System.NonSerialized] private bool _isProxyActive;
    [System.NonSerialized] private GameObject _currentProxy;

    // 属性封装（提供只读访问）
    public bool IsProxyActive { get { return _isProxyActive; } }
    public GameObject CurrentProxy { get { return _currentProxy; } }

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;
    private bool originalActiveState;

    public GameObject CreateProxy(Vector3 position)
    {
        //Debug.Log("触发创建代理");
        if (proxyPrefab == null)
        {
            Debug.LogWarning($"ProxyPrefab is null on {gameObject.name}");
            return null;
        }

        // 保存原物体状态
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalScale = transform.localScale;
        originalActiveState = gameObject.activeSelf;

        // 创建代理实例（直接赋值给字段）
        _currentProxy = Instantiate(proxyPrefab, position, Quaternion.identity);
        _currentProxy.name = $"{gameObject.name}_Proxy";

        // 应用代理设置
        ApplyProxySettings(_currentProxy);

        // 处理原物体
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
        // 应用缩放
        proxy.transform.localScale = proxyScale;

        // 应用透明度
        SpriteRenderer proxyRenderer = proxy.GetComponent<SpriteRenderer>();
        if (proxyRenderer != null)
        {
            Color color = proxyRenderer.color;
            color.a = proxyAlpha;
            proxyRenderer.color = color;
        }

        // 复制Sprite属性
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

        // 添加旋转效果组件
        if (enableRotationEffect)
        {
            var rotationEffect = proxy.AddComponent<ProxyRotationEffect>();
            rotationEffect.maxAngle = maxRotationAngle;
        }
    }

    /// <summary>
    /// 处理拖拽结束
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

     

        // 恢复原物体
        if (hideOriginalDuringDrag)
        {
            gameObject.SetActive(originalActiveState);
        }

        _isProxyActive = false;
        _currentProxy = null;
    }


    /// <summary>
    /// 取消拖拽（强制结束）
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
            // 保持当前旋转和缩放
        }
    }

    private void HandleCustomBehavior(bool dropSuccessful, Vector3 dropPosition)
    {
        // 这里可以扩展自定义行为
        // 例如触发事件、播放动画等
        Debug.Log($"Custom drop behavior: successful={dropSuccessful}, position={dropPosition}");

        // 默认行为：成功时保留，取消时返回
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
        // 清理资源
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
// 对于工具类物品：
// dropBehavior = ReturnToOriginal
// successDropBehavior = CustomBehavior (保持默认)
// cancelDropBehavior = ReturnToOriginal

// 对于消耗品类物品：
// dropBehavior = DestroyProxy  
// successDropBehavior = DestroyProxy
// cancelDropBehavior = ReturnToOriginal

// 对于需要精确定位的物品：
// dropBehavior = StayAtDropPosition
// hideOriginalDuringDrag = true