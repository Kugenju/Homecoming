using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SoupBowlZone : DroppableZone
{
    [Header("预制体配置")]
    public List<SoupState> soupStates = new List<SoupState>();

    [Header("运行时状态")]
    [SerializeField] private List<IngredientSO> currentIngredients = new List<IngredientSO>();
    private GameObject currentVisual; // 当前显示的汤
    private Transform bowlContainer; // 显示汤的容器
    private GameObject baseSoupObject;

    [Header("显示配置")]
    [Tooltip("所有动态生成的汤品预制体将应用此缩放")]
    public Vector3 targetScale = new Vector3(1f, 1f, 1f);


    [Header("音频反馈")]
    public AudioClip addToppingSound;

    private AudioSource audioSource;

    protected override void Awake()
    {
        base.Awake();

        // 查找默认的 Soup_Base（必须是子对象且名为 "Soup_Base"）
        Transform baseTransform = transform.Find("Bowl Container");
        if (baseTransform != null)
        {
            baseSoupObject = baseTransform.gameObject;
        }
        else
        {
            Debug.LogWarning("未找到名为 'Soup_Base' 的子对象，将使用动态容器。");
        }

        // 设置容器（用于放置动态预制体）
        bowlContainer = baseTransform ? baseTransform.parent : transform;

        // 初始化音频
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    public override bool CanAcceptItem(ClickableItem item)
    {
        if (item == null) return false;

        var ingredientComp = item.GetComponent<IngredientComponent>();
        if (ingredientComp == null) return false;

        // 防止重复添加
        if (currentIngredients.Contains(ingredientComp.ingredientData))
            return false;

        return item.CompareTag("Ingredient");
    }

    public override void OnItemDrop(ClickableItem item)
    {
        base.OnItemDrop(item);

        var ingredientComp = item.GetComponent<IngredientComponent>();
        if (ingredientComp == null) return;

        IngredientSO ing = ingredientComp.ingredientData;

        // 如果是汤底，重置
        if (ing.ingredientName == "粉丝汤底")
        {
            ResetBowl();
            AddIngredient(ing);
            PlayFeedback();
            return;
        }

        AddIngredient(ing);

        if (addToppingSound != null)
            audioSource.PlayOneShot(addToppingSound);

        UpdateVisualState();
    }

    private void AddIngredient(IngredientSO ingredient)
    {
        if (!currentIngredients.Contains(ingredient))
            currentIngredients.Add(ingredient);
    }

    private void UpdateVisualState()
    {
        Debug.Log("更新汤的外观");
        currentIngredients.Sort((a, b) => a.name.CompareTo(b.name));

        for (int i = 0; i < soupStates.Count; i++)
        {
            var state = soupStates[i];
            if (IsMatch(currentIngredients, state.requiredIngredients))
            {
                SwitchToState(i);
                return;
            }
        }

        // 默认保持当前状态（不切换）
    }

    private bool IsMatch(List<IngredientSO> current, List<IngredientSO> required)
    {
        if (current.Count != required.Count) return false;
        return required.All(current.Contains);
    }

    private void SwitchToState(int index)
    {
        if (index < 0 || index >= soupStates.Count) return;

        var state = soupStates[index];

        // 记录位置（使用容器的位置）
        Vector3 spawnPos = bowlContainer.position;
        Quaternion spawnRot = bowlContainer.rotation;

        // 销毁当前视觉对象（除了默认的 Soup_Base）
        if (currentVisual != null && currentVisual.name != "Soup_Base")
        {
            Destroy(currentVisual);
        }

        if (baseSoupObject != null)
        {
            baseSoupObject.SetActive(false);

        }

        // 实例化新预制体
        GameObject newVisual = Instantiate(state.visualPrefab, spawnPos, spawnRot, bowlContainer);
        newVisual.name = state.visualPrefab.name;

        Debug.Log($"切换到汤状态: {state.visualPrefab.name}, 状态名称{state.displayName}");
        newVisual.transform.localPosition = Vector3.zero;
        newVisual.transform.localRotation = Quaternion.identity;
        newVisual.transform.localScale = targetScale; // 👈 应用自定义缩放

        currentVisual = newVisual;
    }

    public void ResetBowl()
    {
        currentIngredients.Clear();

        if (baseSoupObject != null)
        {
            baseSoupObject.SetActive(true);
        }
        // 回到默认状态：保持 Soup_Base
        if (currentVisual != null && currentVisual.name != "Soup_Base")
        {
            Destroy(currentVisual);
            currentVisual = transform.Find("Soup_Base")?.gameObject;
        }
    }

    public List<IngredientSO> GetCurrentIngredients() => new List<IngredientSO>(currentIngredients);
    public bool IsReadyToServe() => currentIngredients.Count > 1;
}