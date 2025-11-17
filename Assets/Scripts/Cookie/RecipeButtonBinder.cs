using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class RecipeButtonBinder : MonoBehaviour
{
    [Header("配置")]
    public CookieRecipe recipe; // 拖入对应的配方资产
    public CookieMiniGameController controller;

    private Button _button;

    void Awake()
    {
        _button = GetComponent<Button>();
        if (_button != null && controller != null && recipe != null)
        {
            _button.onClick.AddListener(OnButtonClick);
        }
        else
        {
            Debug.LogError($"[RecipeButtonBinder] Missing Button, Controller, or Recipe on {name}");
        }
    }

    void OnButtonClick()
    {
        controller.OnClickRecipe(recipe);
    }

#if UNITY_EDITOR
    void Reset()
    {
        if (controller == null)
        {
            controller = GetComponentInParent<CookieMiniGameController>();
        }
    }
#endif
}