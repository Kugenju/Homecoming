using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class MessageLayoutController : MonoBehaviour
{
    [Header("Layout Settings")]
    public bool isPlayerMessage = false;
    public float maxWidthPercentage = 0.7f;

    private RectTransform rectTransform;
    private HorizontalLayoutGroup horizontalLayout;
    private ContentSizeFitter sizeFitter;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        horizontalLayout = GetComponent<HorizontalLayoutGroup>();
        sizeFitter = GetComponent<ContentSizeFitter>();
    }

    private void Start()
    {
        SetupLayout();
    }

    private void SetupLayout()
    {
        if (rectTransform == null) return;

        // 设置锚点确保消息能靠边
        if (isPlayerMessage)
        {
            rectTransform.anchorMin = new Vector2(0.3f, 1f);  // 从30%宽度开始，确保靠右
            rectTransform.anchorMax = new Vector2(1f, 1f);
            rectTransform.pivot = new Vector2(1f, 1f);
        }
        else
        {
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(0.7f, 1f);  // 到70%宽度结束，确保靠左
            rectTransform.pivot = new Vector2(0f, 1f);
        }

        // 设置水平布局
        if (horizontalLayout != null)
        {
            horizontalLayout.childAlignment = isPlayerMessage ? TextAnchor.UpperRight : TextAnchor.UpperLeft;
            horizontalLayout.padding = new RectOffset(0, 0, 0, 0);
        }

        // 设置内容大小适配
        if (sizeFitter != null)
        {
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
    }

    private void OnRectTransformDimensionsChange()
    {
        SetupLayout();
    }
}