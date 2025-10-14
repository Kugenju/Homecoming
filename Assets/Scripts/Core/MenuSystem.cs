// using UnityEngine;
// using System.Collections.Generic;

// [CreateAssetMenu(fileName = "MenuSystem", menuName = "Restaurant Game/Menu System", order = 1)]
// public class MenuSystem : ScriptableObject
// {
//   [System.Serializable]
//   public class Ingredient
//   {
//     public string name;
//     public Sprite icon; // 可选：用于UI显示
//   }

//   [System.Serializable]
//   public class Dish
//   {
//     public string dishName;
//     public Sprite icon;
//     public int price;
//     public float cookingTime;
//     public int maxSimultaneousCooking; // 0表示无限制，1表示一次一个，2表示一次两个等
//     public List<Ingredient> requiredIngredients;
//     [TextArea] public string description;
//   }

//   public List<Ingredient> allIngredients = new List<Ingredient>();
//   public List<Dish> menuItems = new List<Dish>();

//   public void InitializeDefaultMenu()
//   {
//     allIngredients.Clear();
//     menuItems.Clear();

//     // 添加所有原料
//     Ingredient coldCake = new Ingredient { name = "冷糕" };
//     Ingredient duckBlood = new Ingredient { name = "鸭血" };
//     Ingredient duckMeat = new Ingredient { name = "鸭肉" };
//     Ingredient duckOffal = new Ingredient { name = "鸭杂" };
//     Ingredient soupBase = new Ingredient { name = "粉丝汤底" };
//     Ingredient wrapper = new Ingredient { name = "皮" };
//     Ingredient meatFilling = new Ingredient { name = "肉馅" };

//     allIngredients.AddRange(new Ingredient[] {
//             coldCake, duckBlood, duckMeat, duckOffal, soupBase, wrapper, meatFilling
//         });

//     // 1. 桂花糕
//     Dish osmanthusCake = new Dish
//     {
//       dishName = "桂花糕",
//       price = 2,
//       cookingTime = 2f,
//       maxSimultaneousCooking = 2,
//       requiredIngredients = new List<Ingredient> { coldCake },
//       description = "每次加热花费2秒，一次可以同时加热两块"
//     };

//     // 2. 鸭血粉丝汤 - 7种固定组合（硬编码）
//     Dish[] duckSoupDishes = CreateAllDuckSoupDishes();

//     // 3. 小笼包
//     Dish xiaoLongBao = new Dish
//     {
//       dishName = "小笼包",
//       price = 4,
//       cookingTime = 4f,
//       maxSimultaneousCooking = 1,
//       requiredIngredients = new List<Ingredient> { wrapper, meatFilling },
//       description = "在蒸笼中需要加热4秒"
//     };

//     menuItems.AddRange(new Dish[] { osmanthusCake });
//     menuItems.AddRange(duckSoupDishes);
//     menuItems.Add(xiaoLongBao);
//   }

//   // 硬编码所有7种鸭血粉丝汤组合
//   private Dish[] CreateAllDuckSoupDishes()
//   {
//     // 获取所有配料引用
//     Ingredient soupBase = allIngredients.Find(x => x.name == "粉丝汤底");
//     Ingredient duckBlood = allIngredients.Find(x => x.name == "鸭血");
//     Ingredient duckMeat = allIngredients.Find(x => x.name == "鸭肉");
//     Ingredient duckOffal = allIngredients.Find(x => x.name == "鸭杂");

//     // 定义所有7种组合
//     return new Dish[]
//     {
//             // 1. 单配料
//             new Dish
//             {
//                 dishName = "鸭血粉丝汤（鸭血）",
//                 price = 3,
//                 cookingTime = 2.5f,
//                 maxSimultaneousCooking = 1,
//                 requiredIngredients = new List<Ingredient> { soupBase, duckBlood },
//                 description = "粉丝汤底 + 鸭血"
//             },
//             new Dish
//             {
//                 dishName = "鸭血粉丝汤（鸭肉）",
//                 price = 3,
//                 cookingTime = 2.5f,
//                 maxSimultaneousCooking = 1,
//                 requiredIngredients = new List<Ingredient> { soupBase, duckMeat },
//                 description = "粉丝汤底 + 鸭肉"
//             },
//             new Dish
//             {
//                 dishName = "鸭血粉丝汤（鸭杂）",
//                 price = 3,
//                 cookingTime = 2.5f,
//                 maxSimultaneousCooking = 1,
//                 requiredIngredients = new List<Ingredient> { soupBase, duckOffal },
//                 description = "粉丝汤底 + 鸭杂"
//             },

//             // 2. 双配料
//             new Dish
//             {
//                 dishName = "鸭血粉丝汤（鸭血+鸭肉）",
//                 price = 4,
//                 cookingTime = 3f,
//                 maxSimultaneousCooking = 1,
//                 requiredIngredients = new List<Ingredient> { soupBase, duckBlood, duckMeat },
//                 description = "粉丝汤底 + 鸭血 + 鸭肉"
//             },
//             new Dish
//             {
//                 dishName = "鸭血粉丝汤（鸭血+鸭杂）",
//                 price = 4,
//                 cookingTime = 3f,
//                 maxSimultaneousCooking = 1,
//                 requiredIngredients = new List<Ingredient> { soupBase, duckBlood, duckOffal },
//                 description = "粉丝汤底 + 鸭血 + 鸭杂"
//             },
//             new Dish
//             {
//                 dishName = "鸭血粉丝汤（鸭肉+鸭杂）",
//                 price = 4,
//                 cookingTime = 3f,
//                 maxSimultaneousCooking = 1,
//                 requiredIngredients = new List<Ingredient> { soupBase, duckMeat, duckOffal },
//                 description = "粉丝汤底 + 鸭肉 + 鸭杂"
//             },

//             // 3. 三配料
//             new Dish
//             {
//                 dishName = "鸭血粉丝汤（鸭血+鸭肉+鸭杂）",
//                 price = 5,
//                 cookingTime = 3.5f,
//                 maxSimultaneousCooking = 1,
//                 requiredIngredients = new List<Ingredient> { soupBase, duckBlood, duckMeat, duckOffal },
//                 description = "粉丝汤底 + 鸭血 + 鸭肉 + 鸭杂"
//             }
//     };
//   }
// }