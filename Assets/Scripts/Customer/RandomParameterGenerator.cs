using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 随机参数生成器，负责生成客户位置和所需菜品
/// </summary>
public class RandomParameterGenerator
{
    // 位置约束参数
    public readonly float minX, maxX, minY, maxY;
    public readonly float minDistance, normalStdDev;
    
    private readonly List<Dish> dishes;
    private readonly List<float> recentXValues = new List<float>();
    private readonly List<GameObject> availablePrefabs; // 可用的预制体列表
    private readonly HashSet<GameObject> usedPrefabs = new HashSet<GameObject>(); // 已使用的预制体
    
    /// <summary>
    /// 构造函数，初始化生成器
    /// </summary>
    public RandomParameterGenerator( 
        List<Dish> dishes,
        List<GameObject> prefabList)
    {
        this.dishes = dishes;
        availablePrefabs = prefabList ?? new List<GameObject>();
        
        // 位置约束参数初始化
        minX = 350f;
        maxX = 2400f;
        minY = 1350f;
        maxY = 1450f;
        minDistance = 350f;
        normalStdDev = 40f;  // 正态分布标准差
    }

    /// <summary>
    /// 生成随机参数元组
    /// </summary>
    /// <returns>(位置, 所需菜品, 预制体)</returns>
    public (Vector3 position, List<Dish> requiredDish, GameObject prefab) 
        GenerateRandomParameters(double probability, int dishIndexFirst, int dishIndexSecond)
    {
        List<Dish> requiredDish = new List<Dish>();
        if (dishIndexFirst == -1)
        {
            var dish = GetRandomDish();
            if (dish == null) {
                Debug.LogWarning("No dish available. Returning (0, null, null)");
                return (Vector3.zero, null, null);
            }
            requiredDish.Add(dish);
            // Debug.Log(probability);

            if (Random.Range(0.0f, 1.0f) < probability) {
                dish = GetRandomDish();
                if (dish == null) {
                    Debug.LogWarning("No dish available. Returning (0, null, null)");
                    return (Vector3.zero, null, null);
                }
                requiredDish.Add(dish);
            }
        } else if (dishIndexSecond == -1) 
        {
            var dish = GetRandomDish(dishIndexFirst);
            if (dish == null) {
                Debug.LogWarning("No dish available. Returning (0, null, null)");
                return (Vector3.zero, null, null);
            }
            requiredDish.Add(dish);
        } else 
        {
            var dish = GetRandomDish(dishIndexFirst, dishIndexSecond);
            if (dish == null) {
                Debug.LogWarning("No dish available. Returning (0, null, null)");
                return (Vector3.zero, null, null);
            }
            requiredDish.Add(dish);
        }
        
        // 获取可用预制体（排除已使用的）
        var usablePrefabs = availablePrefabs
            .Where(prefab => prefab != null && !usedPrefabs.Contains(prefab))
            .ToList();
        GameObject selectedPrefab = null;
        
        // 如果有可用预制体则随机选择
        if (usablePrefabs.Count > 0) {
            selectedPrefab = usablePrefabs[Random.Range(0, usablePrefabs.Count)];
            usedPrefabs.Add(selectedPrefab);
        }
        // Debug.Log(requiredDish.Count);
        return (GenerateConstrainedPosition(), requiredDish, selectedPrefab);
    }

    /// <summary>
    /// 生成约束位置
    /// </summary>
    private Vector3 GenerateConstrainedPosition()
    {
        float y = GenerateNormalDistributedY();
        float x = GenerateConstrainedX();
        return new Vector3(x, y, 0);
    }

    /// <summary>
    /// 生成符合正态分布的Y坐标
    /// </summary>
    private float GenerateNormalDistributedY()
    {
        float mean = (minY + maxY) * 0.5f;  // 均值计算
        float value;
        const int maxAttempts = 100;
        int attempts = 0;

        // 生成符合约束的Y值
        do
        {
            value = getNumberInNormalDistribution(mean, normalStdDev);
            attempts++;
        } 
        while ((value < minY || value > maxY) && attempts < maxAttempts);

        return Mathf.Clamp(value, minY, maxY);
    }

    /// <summary>
    /// 生成满足最小距离约束的X坐标
    /// </summary>
    private float GenerateConstrainedX()
    {
        float newX;
        const int maxAttempts = 100;
        int attempts = 0;
        // 保持最近记录
        while (recentXValues.Count > usedPrefabs.Count)
            recentXValues.RemoveAt(0);

        // 尝试生成有效X值
        do
        {
            newX = Random.Range(minX, maxX);
            attempts++;

            // 检查最近生成的X值距离约束
            bool valid = true;
            foreach (float x in recentXValues)
            {
                if (Mathf.Abs(newX - x) <= minDistance * 2)
                {
                    valid = false;
                    break;
                }
            }

            // 有效时记录并返回
            if (valid)
            {
                recentXValues.Add(newX);
                // Debug.Log(string.Join(", ", recentXValues));
                return newX;
            }
        } 
        while (attempts < maxAttempts * 2);

        // 尝试生成有效X值
        do
        {
            newX = Random.Range(minX, maxX);
            attempts++;

            // 检查最近生成的X值距离约束
            bool valid = true;
            foreach (float x in recentXValues)
            {
                if (Mathf.Abs(newX - x) <= minDistance)
                {
                    valid = false;
                    break;
                }
            }

            // 有效时记录并返回
            if (valid)
            {
                recentXValues.Add(newX);
                // Debug.Log(string.Join(", ", recentXValues));
                return newX;
            }
        } 
        while (attempts < maxAttempts);

        // 最大尝试次数后的回退方案
        Debug.LogWarning($"Failed to find valid X after {maxAttempts} attempts. ");
        return -1.0f;
    }

    /// <summary>
    /// 从菜单系统获取随机菜品
    /// </summary>
    private Dish GetRandomDish()
    {
        if (dishes.Count < 15)
                return dishes[Random.Range(0, dishes.Count)];
        
        if (Random.Range(0.0f, 1.0f) < 0.5f) {
            return dishes[Random.Range(0, 15)];
        } else {
            return dishes[Random.Range(15, dishes.Count)];
        }
        
        
        //Debug.LogError("Dishes not initialized or empty");
        //return null;
    }

    /// <summary>
    /// 从菜单系统获取随机菜品
    /// </summary>
    private Dish GetRandomDish(int index)
    {
        return dishes[index];
    }

    /// <summary>
    /// 从菜单系统获取随机菜品
    /// </summary>
    private Dish GetRandomDish(int firstIndex, int secondIndex)
    {
        return dishes[Random.Range(firstIndex, secondIndex)];
    }

    /// <summary>
    /// 释放已使用的预制体
    /// </summary>
    public void ReleasePrefab(GameObject prefab)
    {
        if (prefab != null) {
            usedPrefabs.Remove(prefab);
        }
    }

    // 有一个称为 Box-Muller (1958) 转换的算法能够将两个在区间（0,1] 的均匀分布转化为标准正态分布，其公式为：
    // y1 = sqrt( - 2 ln(u) ) cos( 2 pi v )
    // y2 = sqrt( - 2 ln(u) ) sin( 2 pi v )


    public float getNumberInNormalDistribution(float mean, float std_dev){
        return mean+(randomNormalDistribution()*std_dev);
    }

    public float randomNormalDistribution(){
        float u=0.0f, v=0.0f, w=0.0f, c=0.0f;
        do{
            u=Random.Range(-1.0f,1.0f);
            v=Random.Range(-1.0f,1.0f);
            w=u*u+v*v;
        }while(w==0.0f||w>=1.0f);
        c=Mathf.Sqrt((-2*Mathf.Log(w))/w);

        //返回2个标准正态分布的随机数，封装进一个数组返回
        //当然，因为这个函数运行较快，也可以扔掉一个
        //return [u*c,v*c];
        return u*c;
    }
}