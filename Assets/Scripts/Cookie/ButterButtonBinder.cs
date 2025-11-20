using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButterButtonBinder : MonoBehaviour
{
    public enum ButterType { Normal, Special }

    [Header("配置")]
    public ButterType butterType;
    public CookieMiniGameController controller; // 拖入你的控制器

    private Button _button;

    void Awake()
    {
        _button = GetComponent<Button>();
        if (_button != null && controller != null)
        {
            _button.onClick.AddListener(OnButtonClick);
        }
        else
        {
            Debug.LogError($"[ButterButtonBinder] Missing Button or Controller on {name}");
        }
    }

    void OnButtonClick()
    {
        switch (butterType)
        {
            case ButterType.Normal:
                controller.OnClickNormalButter();
                break;
            case ButterType.Special:
                controller.OnClickSpecialButter();
                break;
        }
    }

#if UNITY_EDITOR
    // 可选：在编辑器中自动查找同级或父级的控制器（提升体验）
    void Reset()
    {
        if (controller == null)
        {
            controller = GetComponentInParent<CookieMiniGameController>();
        }
    }
#endif
}