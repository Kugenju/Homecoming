using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SteamerBasket : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    
    public enum FoodType { None, Bun, OsmanthusCake }
    public FoodType currentFood = FoodType.None;

    [Header("预制体映射")]
    public GameObject bunPrefab;          // 生包子
    public GameObject cookedBunPrefab;   // 熟包子
    public GameObject cakePrefab;         // 桂花糕（初始就有）

    [Header("动画控制")]
    public Animation steamAnimation;     // 蒸汽动画
    public Animation lidOpenAnimation;   // 开盖动画
    public float cookTime = 5f;         // 蒸煮时间（秒）

    [Header("初始状态")]
    public FoodType initialFood = FoodType.None; // 初始是否有食物？

    private GameObject foodInstance;
    private bool isCooking = false;
    private float cookingTimer = 0f;
    private bool canOpenLid = false;

    private void Awake()
    {
        if (initialFood != FoodType.None)
        {
            SpawnFood(initialFood, true); // 直接生成熟的
        }
    }

    private void Update()
    {
        if (isCooking)
        {
            cookingTimer += Time.deltaTime;
            if (cookingTimer >= cookTime)
            {
                FinishCooking();
            }
        }
    }

    // 接收生包子
    public bool CanAcceptRawBun() => currentFood == FoodType.None && !isCooking;

    public void AddRawBun(GameObject rawBun)
    {
        if (!CanAcceptRawBun()) return;

        Destroy(rawBun);
        currentFood = FoodType.Bun;
        isCooking = true;
        cookingTimer = 0f;
        canOpenLid = false;

        // 显示蒸汽动画
        steamAnimation?.Play();

        Debug.Log("开始蒸包子，耗时 " + cookTime + " 秒");
    }

    private void FinishCooking()
    {
        isCooking = false;
        steamAnimation?.Stop();

        // 替换为熟包子
        if (foodInstance) Destroy(foodInstance);
        foodInstance = Instantiate(cookedBunPrefab, transform);
        foodInstance.transform.localPosition = Vector3.zero;

        canOpenLid = true;
        Debug.Log("包子蒸好了！");
    }

    // 点击蒸笼：开盖查看
    public void OnPointerClick(PointerEventData eventData)
    {
        if (canOpenLid && lidOpenAnimation != null && !lidOpenAnimation.isPlaying)
        {
            lidOpenAnimation.Play();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 鼠标悬停播放开盖动画（可选）
        if (canOpenLid && lidOpenAnimation != null && !lidOpenAnimation.isPlaying)
        {
            lidOpenAnimation.Play();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 如果需要自动关盖，可以在这里 Reverse 动画
    }

    // 内部方法：生成食物（用于初始状态）
    private void SpawnFood(FoodType type, bool isCooked)
    {
        GameObject prefab = isCooked ? cookedBunPrefab : bunPrefab;
        if (type == FoodType.OsmanthusCake)
            prefab = cakePrefab;

        if (prefab != null)
        {
            foodInstance = Instantiate(prefab, transform);
            foodInstance.transform.localPosition = Vector3.zero;
        }

        currentFood = type;
        canOpenLid = true;
    }

    // 外部调用：重置蒸笼
    public void ResetBasket()
    {
        if (foodInstance) Destroy(foodInstance);
        if (steamAnimation) steamAnimation.Stop();
        currentFood = FoodType.None;
        isCooking = false;
        cookingTimer = 0f;
        canOpenLid = false;
    }
}
