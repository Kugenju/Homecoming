using UnityEngine;

public class Customer : MonoBehaviour
{
  // 固定罚款金额
  private const int FixedFine = 3;

  // 顾客精灵
  public Sprite sprite;

  // 顾客等待时间（秒）
  public float waitTime = 8f;
  private float timer;

  // 顾客需要的菜品（由Spawner设置）
  public MenuSystem.Dish RequiredDish { get; set; }

  public bool isUpdate = false;

  private void Awake()
  {
    // 输出当前 Customer 的所有属性
    Debug.Log($"[Customer Awake] 属性初始化:");
    Debug.Log($"- sprite: {sprite}");
    Debug.Log($"- waitTime: {waitTime}");
    Debug.Log($"- RequiredDish: {(RequiredDish != null ? RequiredDish.dishName : "null")}");
  }
  private void Start()
  {
    timer = waitTime;
  }

  private void Update()
  {
    if (!isUpdate) return;

    timer -= Time.deltaTime;

    if (timer <= 0)
    {
      OnTimeout();
    }
  }

  private void OnTimeout()
  {
    Debug.Log("Customer timeout! Applying fixed fine.");
    DestroySprite();
    PlayerData.Instance.SpendMoney(FixedFine);
  }

  private void DestroySprite()
  {
    if (sprite != null)
    {
      Destroy(sprite);
    }

    // 通知Spawner这个Customer被销毁了
    CustomerSpawner spawner = FindObjectOfType<CustomerSpawner>();
    spawner?.OnCustomerDestroyed();

    Destroy(gameObject);
  }

  /// <summary>
  /// 尝试服务顾客（检查Dish是否匹配）
  /// </summary>
  public bool TryServe(MenuSystem.Dish deliveredDish)
  {
    if (deliveredDish == null || RequiredDish == null)
      return false;

    // 严格匹配：Dish对象必须相同（或根据需求改为名称匹配）
    bool isMatch = (deliveredDish == RequiredDish) ||
                  (deliveredDish.dishName == RequiredDish.dishName);

    if (isMatch)
    {
      PlayerData.Instance.AddMoney(RequiredDish.price);
      DestroySprite();
      return true;
    }
    return false;
  }
}