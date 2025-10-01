using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
  // Customer预制体
  public GameObject customerPrefab;

  // 生成位置
  public Vector3 spawnPoint = new Vector3(0, 0, 0);

  // 菜单系统（包含所有菜品）
  public MenuSystem menuSystem;

  // 初始生成间隔
  public float initialSpawnInterval = 5f;

  // 最小生成间隔
  public float minSpawnInterval = 1f;

  // 间隔减少速率
  public float intervalDecreaseRate = 0.01f;

  // 当前生成间隔
  private float currentSpawnInterval;

  // 计时器
  private float spawnTimer;

  // 当前场景中的Customer数量
  private int currentCustomerCount = 0;

  // 最大Customer数量
  private const int MaxCustomers = 5;

  private void Start()
  {
    currentSpawnInterval = initialSpawnInterval;
    spawnTimer = currentSpawnInterval;

    // 确保菜单系统已初始化
    if (menuSystem == null)
    {
      menuSystem = ScriptableObject.CreateInstance<MenuSystem>();
      menuSystem.InitializeDefaultMenu();
    }
  }

  private void Update()
  {
    if (currentCustomerCount >= MaxCustomers)
    {
      return;
    }

    spawnTimer -= Time.deltaTime;

    if (spawnTimer <= 0)
    {
      SpawnCustomer();
      spawnTimer = currentSpawnInterval;
      currentSpawnInterval = Mathf.Max(minSpawnInterval, currentSpawnInterval - intervalDecreaseRate);
    }
  }

  private void SpawnCustomer()
  {
    // if (customerPrefab == null || spawnPoint == null || menuSystem == null)
    // {
    //     Debug.LogError("Customer prefab, spawn point or menu system not assigned!");
    //     return;
    // }

    // 随机获取一道菜
    MenuSystem.Dish randomDish = GetRandomDish();
    if (randomDish == null)
    {
      Debug.LogError("No dishes available in the menu system!");
      return;
    }

    // 实例化Customer
    GameObject customerObj = Instantiate(customerPrefab, new Vector3(0, 2, 0), Quaternion.identity);
    Customer customer = customerObj.GetComponent<Customer>();

    if (customer != null)
    {
      // 设置Customer需要的菜品
      customer.RequiredDish = randomDish;
      currentCustomerCount++;
      Debug.Log("Spawned customer with dish: " + randomDish.dishName);
      customer.isUpdate = true;
    }
    else
    {
      Debug.Log("Customer prefab does not have Customer component!");
    }
  }

  /// <summary>
  /// 从菜单系统中随机获取一道菜（均等概率）
  /// </summary>
  private MenuSystem.Dish GetRandomDish()
  {
    if (menuSystem == null || menuSystem.menuItems == null || menuSystem.menuItems.Count == 0)
    {
      return null;
    }

    // 使用随机索引确保均等概率
    int randomIndex = Random.Range(0, menuSystem.menuItems.Count);
    return menuSystem.menuItems[randomIndex];
  }

  public void OnCustomerDestroyed()
  {
    currentCustomerCount = Mathf.Max(0, currentCustomerCount - 1);
  }
}