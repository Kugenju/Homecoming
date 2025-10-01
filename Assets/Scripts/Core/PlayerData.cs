using UnityEngine;
using System.IO;

public class PlayerData : MonoBehaviour
{
    [System.Serializable]
    private class PlayerStats
    {
        public int money;
        public int stamina;
        public float lastStaminaRecoveryTime; // 记录上次恢复体力的时间
    }

    private PlayerStats playerStats;
    private const string SaveFilePath = "/playerData.json";
    private const int MaxStamina = 10; // 体力最大值
    private const float StaminaRecoveryInterval = 300f; // 5分钟 = 300秒

    public static PlayerData Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadData();

            // 输出当前 Money 和体力
            Debug.Log($"Loaded Data - Money: {playerStats.money}, Stamina: {playerStats.stamina}");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // 每帧检查是否需要恢复体力
        TryRecoverStamina();
    }

    private void LoadData()
    {
        string path = Application.persistentDataPath + SaveFilePath;
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            playerStats = JsonUtility.FromJson<PlayerStats>(json);

            // 确保体力不超过最大值
            playerStats.stamina = Mathf.Clamp(playerStats.stamina, 0, MaxStamina);
        }
        else
        {
            playerStats = new PlayerStats
            {
                money = 0,
                stamina = MaxStamina, // 默认满体力
                lastStaminaRecoveryTime = Time.time // 初始化为当前时间
            };
            SaveData();
        }
    }

    private void SaveData()
    {
        string json = JsonUtility.ToJson(playerStats, true);
        File.WriteAllText(Application.persistentDataPath + SaveFilePath, json);
        Debug.Log($"Money: {playerStats.money}, Stamina: {playerStats.stamina}, LastStaminaRecoveryTime: {playerStats.lastStaminaRecoveryTime}");
    }

    private void TryRecoverStamina()
    {
        // 如果体力已满，不恢复
        if (playerStats.stamina >= MaxStamina)
        {
            playerStats.lastStaminaRecoveryTime = Time.time; // 重置计时器
            return;
        }

        // 计算距离上次恢复的时间
        float timeSinceLastRecovery = Time.time - playerStats.lastStaminaRecoveryTime;

        // 如果超过恢复间隔（5分钟），恢复 1 点体力
        if (timeSinceLastRecovery >= StaminaRecoveryInterval)
        {
            playerStats.stamina = Mathf.Clamp(playerStats.stamina + 1, 0, MaxStamina);
            playerStats.lastStaminaRecoveryTime = Time.time; // 更新恢复时间
            SaveData();
            Debug.Log($"Recovered 1 Stamina - Current Stamina: {playerStats.stamina}");
        }
    }

    // 修改钱的方法
    public void AddMoney(int amount)
    {
        playerStats.money += amount;
        SaveData();
    }

    public bool SpendMoney(int amount)
    {
        if (playerStats.money >= amount)
        {
            playerStats.money -= amount;
            SaveData();
            return true;
        }
        else {
            playerStats.money = 0;
            SaveData();
            return false;
        }
    }

    // 修改体力的方法
    public void AddStamina(int amount)
    {
        playerStats.stamina = Mathf.Clamp(playerStats.stamina + amount, 0, MaxStamina);
        playerStats.lastStaminaRecoveryTime = Time.time; // 手动恢复体力时重置计时器
        SaveData();
    }

    public bool ConsumeStamina(int amount)
    {
        if (playerStats.stamina >= amount)
        {
            playerStats.stamina -= amount;
            SaveData();
            return true;
        }
        return false;
    }

    // 获取数据的方法
    public int GetMoney() => playerStats.money;
    public int GetStamina() => playerStats.stamina;
    public float GetStaminaRecoveryTime() => Time.time - playerStats.lastStaminaRecoveryTime;
    public float GetStaminaRecoveryProgress()
    {
        float timeSinceLastRecovery = Time.time - playerStats.lastStaminaRecoveryTime;
        return Mathf.Clamp01(timeSinceLastRecovery / StaminaRecoveryInterval);
    }
}