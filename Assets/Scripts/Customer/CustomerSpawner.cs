using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 客户生成管理器，负责控制客户生成逻辑和间隔
/// </summary>
public class CustomerSpawner : MonoBehaviour
{
    [Header("预制体配置")]
    public List<GameObject> customerPrefabs; // 使用列表管理多个预制体
    public List<Dish> dishes; // 菜品列表
    
    [Header("生成设置")]
    public float initialSpawnInterval = 15f;  // 初始生成间隔
    public float minSpawnInterval = 0.5f;      // 最小生成间隔
    public float intervalDecreaseRate = 0.5f;  // 间隔减少速率

    private RandomParameterGenerator paramGenerator;  // 随机参数生成器
    private float currentSpawnInterval;  // 当前生成间隔
    private float spawnTimer;            // 生成计时器
    private int currentCustomerCount = 0; // 当前客户数量
    private const int MaxCustomers = 4;   // 最大客户数量

    /// <summary>
    /// 初始化生成器和管理器
    /// </summary>
    private void Start()
    {
        currentSpawnInterval = initialSpawnInterval;
        spawnTimer = currentSpawnInterval;
        // // 菜单系统初始化
        // if (menuSystem == null)
        // {
        //     menuSystem = ScriptableObject.CreateInstance<MenuSystem>();
        //     menuSystem.InitializeDefaultMenu();
        // }

        paramGenerator = new RandomParameterGenerator(dishes, customerPrefabs);
    }

    /// <summary>
    /// 每帧更新，处理生成逻辑
    /// </summary>
    private void Update()
    {
        // 达到最大客户数时停止生成
        if (currentCustomerCount >= MaxCustomers) return;
        
        // 倒计时生成间隔
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0)
        {
            SpawnCustomer();
            spawnTimer = currentSpawnInterval;
            // 动态调整生成间隔
            currentSpawnInterval = Mathf.Max(minSpawnInterval, 
                                          currentSpawnInterval - intervalDecreaseRate);
        }
    }

    /// <summary>
    /// 生成单个客户实例
    /// </summary>
    private void SpawnCustomer()
    {
        var randomParams = paramGenerator.GenerateRandomParameters(minSpawnInterval / currentSpawnInterval - 0.05);
        if (randomParams.requiredDish == null) return;
        // 如果没有可用预制体则跳过生成
        if (randomParams.prefab == null) {
            Debug.LogWarning("No available prefabs for spawn");
            spawnTimer = currentSpawnInterval;
            return;
        }
        if (randomParams.position.x < 0) {
            paramGenerator.ReleasePrefab(randomParams.prefab);
            return;
        }
        // Debug.Log(randomParams.position);
        GameObject customerObj = Instantiate(
            randomParams.prefab,
            randomParams.position,
            Quaternion.identity
        );
        Customer customer = customerObj.GetComponent<Customer>();
        
        if (customer != null) {
            customer.RequiredDish = randomParams.requiredDish;
            customer.UsedPrefab = randomParams.prefab;
            currentCustomerCount++;
            customer.isUpdate = true;
        }
    }
    public void OnCustomerDestroyed(GameObject usedPrefab)
    {
        currentCustomerCount = Mathf.Max(0, currentCustomerCount - 1);
        paramGenerator.ReleasePrefab(usedPrefab); // 释放预制体
    }
}

[System.Serializable]
public class Dish{
    public int price;
    public string dishName;
    public GameObject prefab;
}