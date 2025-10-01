using UnityEngine;
using System.Collections.Generic;

public class MenuSystem : MonoBehaviour
{
  // 原料类型枚举
  public enum IngredientType
  {
    ColdCake,           // 冷糕（桂花糕原料）
    DuckBlood,          // 鸭血
    DuckMeat,           // 鸭肉
    DuckOffal,          // 鸭杂
    NoodleSoupBase,     // 粉丝汤底
    Wrapper,            // 皮（小笼包原料）
    MeatFilling         // 肉馅
  }

  // 菜品类型枚举
  public enum DishType
  {
    OsmanthusCake,      // 桂花糕
    DuckBloodNoodleSoup,// 鸭血粉丝汤
    XiaoLongBao         // 小笼包
  }

  // 原料数据结构
  [System.Serializable]
  public class Ingredient
  {
    public IngredientType type;
    public string name;
  }

  // 菜品数据结构
  [System.Serializable]
  public class Dish
  {
    public DishType type;
    public string name;
    public float price;
    public float preparationTime;
    public List<IngredientType> requiredIngredients;

    // 桂花糕特有属性：一次可以加热的数量
    public int batchHeatingAmount = 1;

    // 鸭血粉丝汤特有属性：配料组合价格和时间
    public Dictionary<int, float> soupCombinationPrices;
    public Dictionary<int, float> soupCombinationTimes;
  }

  // 所有原料列表
  public List<Ingredient> allIngredients = new List<Ingredient>();

  // 所有菜品字典
  public Dictionary<DishType, Dish> menuDishes = new Dictionary<DishType, Dish>();

  private void Awake()
  {
    InitializeIngredients();
    InitializeMenu();
    Debug.Log("MenuSystem initialized.");
  }

  // 初始化所有原料
  private void InitializeIngredients()
  {
    allIngredients = new List<Ingredient>
        {
            new Ingredient { type = IngredientType.ColdCake, name = "冷糕" },
            new Ingredient { type = IngredientType.DuckBlood, name = "鸭血" },
            new Ingredient { type = IngredientType.DuckMeat, name = "鸭肉" },
            new Ingredient { type = IngredientType.DuckOffal, name = "鸭杂" },
            new Ingredient { type = IngredientType.NoodleSoupBase, name = "粉丝汤底" },
            new Ingredient { type = IngredientType.Wrapper, name = "皮" },
            new Ingredient { type = IngredientType.MeatFilling, name = "肉馅" }
        };
  }

  // 初始化菜单
  private void InitializeMenu()
  {
    // 桂花糕
    var osmanthusCake = new Dish
    {
      type = DishType.OsmanthusCake,
      name = "桂花糕",
      price = 2f,
      preparationTime = 2f,
      batchHeatingAmount = 2, // 一次可以加热两块
      requiredIngredients = new List<IngredientType> { IngredientType.ColdCake }
    };

    // 鸭血粉丝汤
    var duckBloodNoodleSoup = new Dish
    {
      type = DishType.DuckBloodNoodleSoup,
      name = "鸭血粉丝汤",
      price = 0f, // 实际价格由配料组合决定
      preparationTime = 0f, // 无明确单独制作时长要求
      requiredIngredients = new List<IngredientType>
            {
                IngredientType.DuckBlood,
                IngredientType.DuckMeat,
                IngredientType.DuckOffal,
                IngredientType.NoodleSoupBase
            },
      soupCombinationPrices = new Dictionary<int, float>
            {
                { 1, 2f },  // 单配料
                { 2, 3f },  // 双配料
                { 3, 4f }   // 全配料
            },
      soupCombinationTimes = new Dictionary<int, float>
            {
                { 1, 1.5f }, // 单配料制作时间
                { 2, 2.5f }, // 双配料制作时间
                { 3, 3.5f }  // 全配料制作时间
            }
    };

    // 小笼包
    var xiaoLongBao = new Dish
    {
      type = DishType.XiaoLongBao,
      name = "小笼包",
      price = 4f, // 修改为4元
      preparationTime = 4f,
      requiredIngredients = new List<IngredientType>
            {
                IngredientType.Wrapper,
                IngredientType.MeatFilling
            }
    };

    // 添加到菜单字典
    menuDishes = new Dictionary<DishType, Dish>
        {
            { DishType.OsmanthusCake, osmanthusCake },
            { DishType.DuckBloodNoodleSoup, duckBloodNoodleSoup },
            { DishType.XiaoLongBao, xiaoLongBao }
        };
  }

  // 获取菜品信息的方法
  public Dish GetDishInfo(DishType dishType)
  {
    if (menuDishes.TryGetValue(dishType, out Dish dish))
    {
      return dish;
    }
    Debug.LogWarning($"Dish type {dishType} not found in menu.");
    return null;
  }

  // 计算鸭血粉丝汤价格的方法（根据配料数量）
  public float CalculateSoupPrice(int ingredientCount)
  {
    if (menuDishes.TryGetValue(DishType.DuckBloodNoodleSoup, out Dish soupDish) &&
        soupDish.soupCombinationPrices != null)
    {
      // 确保配料数量在有效范围内（1-3）
      int clampedCount = Mathf.Clamp(ingredientCount, 1, 3);
      if (soupDish.soupCombinationPrices.TryGetValue(clampedCount, out float price))
      {
        return price;
      }
    }
    return 0f;
  }

  // 计算鸭血粉丝汤制作时间的方法（根据配料数量）
  public float CalculateSoupPreparationTime(int ingredientCount)
  {
    if (menuDishes.TryGetValue(DishType.DuckBloodNoodleSoup, out Dish soupDish) &&
        soupDish.soupCombinationTimes != null)
    {
      // 确保配料数量在有效范围内（1-3）
      int clampedCount = Mathf.Clamp(ingredientCount, 1, 3);
      if (soupDish.soupCombinationTimes.TryGetValue(clampedCount, out float time))
      {
        return time;
      }
    }
    return 0f;
  }
}