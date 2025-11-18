using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ButtonLogic : MonoBehaviour
{
    [Header("核心按钮设置")]
    [Tooltip("需要监听点击的功能按钮列表")]
    public List<GameObject> buttons; // 功能按钮列表（按索引对应）
    public BackButton backButton; // 返回按钮

    [Header("亮度设置")]
    [Tooltip("选中时的亮度倍率（1=原始亮度，>1更亮）")]
    [Range(1.1f, 2f)] public float highlightBrightness = 1.5f;

    // 关联核心逻辑
    public CoreLogic coreLogic; 
    public int selectedButtonIndex = -1; // 选中的按钮索引
    public bool isEnd = false; // 是否结束
    private List<SpriteRenderer> buttonRenderers; // 功能按钮的渲染器列表
    private List<Color> originalColors; // 功能按钮的原始颜色
    private Coroutine currentCoroutine;


    void Start()
    {
        InitializeButtons();
    }

    void Update()
    {
        if (!backButton.isPaused && Input.GetMouseButtonDown(0) && !isEnd)
        {
            CheckClickArea();
        }
    }

    private void InitializeButtons()
    {
        buttonRenderers = new List<SpriteRenderer>();
        originalColors = new List<Color>();

        if (buttons == null || buttons.Count == 0)
        {
            Debug.LogWarning("未在buttons列表中添加任何功能按钮", this);
            return;
        }

        foreach (var btn in buttons)
        {
            if (btn == null)
            {
                buttonRenderers.Add(null);
                originalColors.Add(Color.white);
                Debug.LogWarning("buttons列表中存在空对象", this);
                continue;
            }

            var renderer = btn.GetComponent<SpriteRenderer>();
            buttonRenderers.Add(renderer);

            if (renderer == null)
            {
                originalColors.Add(Color.white);
                Debug.LogError($"功能按钮 {btn.name} 缺少SpriteRenderer组件", btn);
                continue;
            }

            originalColors.Add(renderer.color);

            if (btn.GetComponent<Collider2D>() == null)
            {
                btn.AddComponent<BoxCollider2D>();
                Debug.LogWarning($"功能按钮 {btn.name} 自动添加了BoxCollider2D", btn);
            }
        }
    }

    private void CheckClickArea()
    {
        if (backButton.isPaused) return;
        
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        if (hit.collider != null)
        {
            GameObject clickedObj = hit.collider.gameObject;

            // 处理功能按钮点击
            int funcBtnIndex = buttons.IndexOf(clickedObj);
            if (funcBtnIndex == -1) return;
            float xPos = (funcBtnIndex == 1 || funcBtnIndex == 0) ? 1 - 2 * funcBtnIndex : 0;
            // 调用协程时先停止已有实例
            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
            }
            currentCoroutine = StartCoroutine(LerpTargetObjectsXPosition(xPos));
            if (funcBtnIndex != -1 && funcBtnIndex < buttons.Count)
            {
                OnFunctionButtonClicked(funcBtnIndex);
                return;
            }
        }

        // 点击空白区域重置选中状态
        // ResetAllSelection();
    }

    private void OnFunctionButtonClicked(int index)
    {
        selectedButtonIndex = index;

        for (int i = 0; i < buttonRenderers.Count; i++)
        {
            var renderer = buttonRenderers[i];
            if (renderer == null) continue;

            if (i == index)
            {
                Color highlighted = originalColors[i] * highlightBrightness;
                highlighted.a = originalColors[i].a;
                renderer.color = highlighted;
            }
            else
            {
                renderer.color = originalColors[i];
            }
        }
    }

    private void ResetAllSelection()
    {
        selectedButtonIndex = -1;

        for (int i = 0; i < buttonRenderers.Count; i++)
        {
            var renderer = buttonRenderers[i];
            if (renderer != null)
            {
                renderer.color = originalColors[i];
            }
        }
    }

    private void OnDrawGizmos()
    {
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.yellow;
        style.fontSize = 14;
        UnityEditor.Handles.Label(transform.position + Vector3.up * 3f, 
            $"当前选中索引：{selectedButtonIndex}", style);
    }

    /// <summary>
    /// 平滑过渡目标对象的X坐标
    /// </summary>
    /// <param name="targetX">目标X坐标值</param>
    /// <param name="duration">过渡时长（秒）</param>
    private IEnumerator LerpTargetObjectsXPosition(float targetX, float duration = 0.25f)
    {
        if (coreLogic == null) yield break;

        // 记录初始位置
        Vector3 startPos1 = coreLogic.targetObject1 != null ? 
            coreLogic.targetObject1.transform.position : Vector3.zero;
        Vector3 startPos2 = coreLogic.targetObject2 != null ? 
            coreLogic.targetObject2.transform.position : Vector3.zero;

        float elapsed = 0f;
        duration *= Mathf.Abs(targetX - startPos1.x); // 计算实际过渡时间
        while (elapsed < duration)
        {
            // 计算插值比例（0→1）
            float t = elapsed / duration;
            // 可以使用Mathf.SmoothStep使过渡更平滑
            float smoothT = Mathf.SmoothStep(0, 1, t);

            // 更新第一个目标位置
            if (coreLogic.targetObject1 != null)
            {
                Vector3 newPos = new Vector3(
                    Mathf.Lerp(startPos1.x, targetX, smoothT),
                    startPos1.y,
                    startPos1.z
                );
                coreLogic.targetObject1.transform.position = newPos;
            }

            // 更新第二个目标位置
            if (coreLogic.targetObject2 != null)
            {
                Vector3 newPos = new Vector3(
                    Mathf.Lerp(startPos2.x, targetX, smoothT),
                    startPos2.y,
                    startPos2.z
                );
                coreLogic.targetObject2.transform.position = newPos;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 确保最终位置精确到位
        if (coreLogic.targetObject1 != null)
        {
            coreLogic.targetObject1.transform.position = new Vector3(targetX, startPos1.y, startPos1.z);
        }
        if (coreLogic.targetObject2 != null)
        {
            coreLogic.targetObject2.transform.position = new Vector3(targetX, startPos2.y, startPos2.z);
        }
    }
}