using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using UnityEditor;

public class Customer : DroppableZone
{
    private const int FixedFine = 3;
    public float waitTime = 8f;
    private float timer;
    public List<Dish> RequiredDish { get; set; }
    private int dishIndex = -1;
    public bool isUpdate = false;
    public GameObject UsedPrefab { get; set; } // 新增：记录使用的预制体
    public GameObject dialogBox;
    private GameObject currentDialog;
    public GameObject flame;
    private GameObject currentFlame;
    public GameObject candle;
    private GameObject currentCandle;

    private PlayerData playerData;

    // 新增淡入淡出相关变量
    [SerializeField] public float fadeDuration = 1.0f; // 淡入淡出持续时间
    private SpriteRenderer spriteRenderer;
    private bool isFading = false;
    public Vector3 offset = new Vector3(50, 50, 0); // 偏移量

    private List<GameObject> dishInstances = new List<GameObject>();
    private float dialogWidth => currentDialog != null ? 
        currentDialog.GetComponent<SpriteRenderer>().bounds.size.x : 0;
    private float dialogHeight => currentDialog != null ? 
        currentDialog.GetComponent<SpriteRenderer>().bounds.size.y : 0;

    private void Start()
    {
        // 获取SpriteRenderer组件
        spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
        if (spriteRenderer == null) {
            Debug.LogError("SpriteRenderer is null.");
        }
        // 初始设置为透明
        spriteRenderer.color = new Color(1, 1, 1, 0);
        
        // 启动淡入协程
        StartCoroutine(FadeIn());

        timer = waitTime;

        playerData = PlayerData.Instance; // 实例化PlayerData

        // RenderRequiredDishes();
    }
    
    private void RenderRequiredDishes()
    {
        if (RequiredDish == null || RequiredDish.Count == 0 || currentDialog == null) {
            DestroySprite();
            return;
        }
        
        // 清除旧实例
        foreach (var dish in dishInstances) Destroy(dish);
        dishInstances.Clear();
        
        // 获取dialog的尺寸信息
        var dialogRenderer = currentDialog.GetComponent<SpriteRenderer>();
        if (dialogRenderer == null) return;
        
        // 根据数量处理
        if (RequiredDish.Count == 1)
        {
            CreateDishInstance(RequiredDish[0], 
                dialogHeight * 0.5f, 
                dialogRenderer);
        }
        else if (RequiredDish.Count == 2)
        {
            CreateDishInstance(RequiredDish[0], 
                dialogHeight * 1/3f, 
                dialogRenderer);
            CreateDishInstance(RequiredDish[1], 
                dialogHeight * 2/3f, 
                dialogRenderer);
        }
    }
    
    private void CreateDishInstance(Dish dish, float yOffset, SpriteRenderer dialogRenderer)
    {
        if (dish == null || dish.prefab == null) return;
        
        // 计算位置
        var position = currentDialog.transform.position + 
            new Vector3(dialogWidth * 0.5f, yOffset, 0);
        
        // 实例化
        var instance = Instantiate(dish.prefab, position, Quaternion.identity);
        
        // 设置层级
        SetSortingLayer(instance, currentDialog);
        // var instanceRenderer = instance.GetComponent<SpriteRenderer>();
        // if (instanceRenderer != null && dialogRenderer != null)
        // {
        //     // instanceRenderer.sortingLayer = dialogRenderer.sortingLayer;
        //     instanceRenderer.sortingOrder = dialogRenderer.sortingOrder + 1;
        // }
        
        // 缩放控制
        ScaleDishToFit(instance);
        
        dishInstances.Add(instance);
    }

    private void SetSortingLayer(GameObject obj, GameObject reference)
    {
        var renderer = obj.GetComponent<SpriteRenderer>();
        var referenceRenderer = reference.GetComponent<SpriteRenderer>();
        
        if (renderer != null && referenceRenderer != null)
        {
            // renderer.sortingLayer = referenceRenderer.sortingLayer;
            renderer.sortingOrder = referenceRenderer.sortingOrder + 1;
        }
    }
    
    private void ScaleDishToFit(GameObject dishInstance)
    {
        var renderer = dishInstance.GetComponent<SpriteRenderer>();
        if (renderer == null) return;
        
        var bounds = renderer.bounds;
        var maxDimension = Mathf.Max(bounds.size.x, bounds.size.y);
        var maxAllowed = dialogHeight * 0.3f;
        
        if (maxDimension > maxAllowed || maxDimension < maxAllowed / 2)
        {
            var scale = maxAllowed / maxDimension;
            dishInstance.transform.localScale *= scale;
            // Debug.Log($"[Customer ScaleDishToFit] Scale {scale} applied to {dishInstance.name}.");
        }
    }

    // 新增淡入协程
    private IEnumerator FadeIn()
    {
        // 新增：记录原始位置
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + offset;
        transform.position = targetPosition; // 初始放在偏移位置
    
        Color spriteColor = spriteRenderer.color;
        float fadeSpeed = 1f / fadeDuration;
        float fadeTimer = 0f;
        float moveTimer = 0f;
    
        while (fadeTimer < fadeDuration)
        {
            fadeTimer += Time.deltaTime;
            moveTimer += Time.deltaTime;
            
            // 淡入效果
            float alpha = Mathf.Lerp(0f, 1f, fadeTimer * fadeSpeed);
            spriteRenderer.color = new Color(spriteColor.r, spriteColor.g, spriteColor.b, alpha);
            
            // 新增移动效果（匀速）
            transform.position = Vector3.Lerp(
                targetPosition, 
                startPosition, 
                moveTimer / fadeDuration
            );
            
            yield return null;
        }
    
        // 确保最终位置正确
        transform.position = startPosition;

        // 淡入完成后创建dialogBox（在人物右侧）
        if(dialogBox != null) {
            // 获取预制件原始位置信息
            Vector3 prefabOrigin = dialogBox.transform.position;
            
            // 创建新位置
            Vector3 spawnPos = new Vector3(
                transform.position.x + 120f,
                prefabOrigin.y,
                prefabOrigin.z
            );
            
            // 实例化并保持层级
            currentDialog = Instantiate(dialogBox, spawnPos, dialogBox.transform.rotation);
        }

        // 新增flame和candle的实例化
        if (dialogBox != null) {
            // 计算基础位置
            Vector3 basePos = currentDialog.transform.position;
            float offsetX = dialogWidth * 0.8f;
            
            // 实例化flame
            if (flame != null) {
                currentFlame = Instantiate(flame, 
                    new Vector3(basePos.x + offsetX - 5f, flame.transform.position.y, flame.transform.position.z),
                    flame.transform.rotation);
                SetSortingLayer(currentFlame, currentDialog);
            }
            
            // 实例化candle
            if (candle != null) {
                currentCandle = Instantiate(candle, 
                    new Vector3(basePos.x + offsetX, candle.transform.position.y, candle.transform.position.z),
                    candle.transform.rotation);
                SetSortingLayer(currentCandle, currentDialog);
            }
        }

        RenderRequiredDishes();
    }

    private void Update()
    {
        if (!isUpdate || isFading) return;

        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            OnTimeout();
            isUpdate = false;
        }

        // 新增火焰动画逻辑
        if (currentFlame != null)
        {
            // 获取初始位置信息
            Vector3 basePos = flame.transform.position;
            
            // 计算动态Y坐标
            float animatedY = basePos.y - (1f - timer/waitTime) * 200f;
            
            // 更新火焰位置
            currentFlame.transform.position = new Vector3(
                currentFlame.transform.position.x,
                animatedY,
                currentFlame.transform.position.z
            );
        }
    }

    private void OnTimeout()
    {
        Debug.Log("Customer timeout! Applying fixed fine.");
        playerData.SpendMoney(FixedFine);
        DestroySprite();
    }

    public void DestroySprite()
    {
        if(currentDialog != null) 
        {
            Destroy(currentDialog);
        }
        foreach (var dish in dishInstances) Destroy(dish);
        // 新增flame/candle销毁
        if (currentFlame != null) Destroy(currentFlame);
        if (currentCandle != null) Destroy(currentCandle);
        dishInstances.Clear();
        StartCoroutine(FadeOutAndDestroy());
    }

    // 新增淡出协程
    private IEnumerator FadeOutAndDestroy()
    {
        // 新增：记录原始位置
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + offset;
    
        Color spriteColor = spriteRenderer.color;
        float fadeSpeed = 1f / fadeDuration;
        float fadeTimer = 0f;
        float moveTimer = 0f;
    
        while (fadeTimer < fadeDuration)
        {
            fadeTimer += Time.deltaTime;
            moveTimer += Time.deltaTime;
            
            // 淡出效果
            float alpha = Mathf.Lerp(1f, 0f, fadeTimer * fadeSpeed);
            spriteRenderer.color = new Color(spriteColor.r, spriteColor.g, spriteColor.b, alpha);
            
            // 新增移动效果（匀速）
            transform.position = Vector3.Lerp(
                startPosition, 
                targetPosition, 
                moveTimer / fadeDuration
            );
            
            yield return null;
        }
    
        // 确保最终位置正确
        transform.position = targetPosition;

        // 通知Spawner并传递使用的预制体
        CustomerSpawner spawner = FindObjectOfType<CustomerSpawner>();
        spawner?.OnCustomerDestroyed(UsedPrefab);

        // 最终销毁
        Destroy(gameObject);
    }

    public override bool CanAcceptItem(ClickableItem item)
    {
        // 0. 如果item.name中有"碗"字，直接返回false
        if (item.name.Contains("碗"))
        {
            return false;
        }
        dishIndex = -1;

        // 使用for循环替代foreach
        for (int i = 0; i < RequiredDish.Count; i++)
        {
            var dish = RequiredDish[i];
            string dishName = dish.dishName;
            string itemName = item.name;

            // 1. 如果两个name中都有“糕”字，返回true
            if (dishName.Contains("糕") && itemName.Contains("糕"))
            {
                dishIndex = i;
                return true;
            }

            // 2. 如果两个name中都有“包”字，返回true
            if (dishName.Contains("包") && itemName.Contains("包"))
            {
                dishIndex = i;
                return true;
            }

            // 3. 如果两个name中都有“全四种”，返回true
            if (dishName.Contains("全四种") && itemName.Contains("全四种"))
            {
                dishIndex = i;
                return true;
            }

            // 4. 统计"血""葱""豆"三个字的数目并判断条件
            int dishBlood = CountChar(dishName, '血');
            int itemBlood = CountChar(itemName, '血');
            int dishOnion = CountChar(dishName, '葱');
            int itemOnion = CountChar(itemName, '葱');
            int dishBean = CountChar(dishName, '豆');
            int itemBean = CountChar(itemName, '豆');

            int sumItem = itemBlood + itemOnion + itemBean;

            if (dishBlood == itemBlood + 1 
                && dishOnion == itemOnion 
                && dishBean == itemBean 
                && sumItem > 0)
            {
                dishIndex = i;
                return true;
            }
        }

        // 5. 最后返回false
        return false;
    }

    // 辅助方法：统计字符串中指定字符的出现次数
    private int CountChar(string str, char c)
    {
        int count = 0;
        foreach (char ch in str)
        {
            if (ch == c)
            {
                count++;
            }
        }
        return count;
    }

    public override void OnItemDrop(ClickableItem item)
    {
        base.OnItemDrop(item);

        Destroy(item.gameObject);

        if (dishIndex >= 0 && dishIndex <= RequiredDish.Count)
        {
            playerData.AddMoney(RequiredDish[dishIndex].price);
            RequiredDish.RemoveAt(dishIndex);
            dishIndex = -1;
            RenderRequiredDishes();
        }

        // 示例：销毁原料，生成新对象（如包馅）
        // 由子类具体实现
    }
}