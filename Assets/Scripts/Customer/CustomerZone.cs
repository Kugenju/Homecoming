using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class CustomerZone : DroppableZone
{
    [Header("客户配置")]
    public Customer currentCustomer; // 直接引用当前客户
    public UnityEvent onOrderCompleted;
    public UnityEvent onOrderFailed;

    //private void Update()
    //{
    //    // 检查客户是否存在且超时
    //    if (currentCustomer != null && currentCustomer.isUpdate)
    //    {
    //        // 可以通过事件监听客户的超时，或者在这里检测
    //    }
    //}

    //public override bool CanAcceptItem(ClickableItem item)
    //{
    //    if (item == null || currentCustomer == null) return false;

    //    // 检查是否是汤碗且准备就绪
    //    SoupBowlZone soupBowl = item.GetComponentInParent<SoupBowlZone>();
    //    if (soupBowl != null && soupBowl.IsReadyToServe())
    //    {
    //        return CheckDishMatch(soupBowl);
    //    }

    //    return false;
    //}

    ///// <summary>
    ///// 检查汤碗中的菜品是否与客户需求匹配
    ///// </summary>
    //private bool CheckDishMatch(SoupBowlZone soupBowl)
    //{
    //    if (currentCustomer == null || currentCustomer.RequiredDish == null)
    //        return false;

    //    // 获取汤碗中的菜品信息（需要根据实际实现调整）
    //    // 这里假设SoupBowlZone有一个方法可以获取对应的Dish
    //    Dish servedDish = soupBowl.GetServedDish();

    //    if (servedDish != null)
    //    {
    //        // 检查是否匹配客户需求的任意一个菜品
    //        foreach (Dish requiredDish in currentCustomer.RequiredDish)
    //        {
    //            if (requiredDish != null && servedDish.dishName == requiredDish.dishName)
    //            {
    //                return true;
    //            }
    //        }
    //    }

    //    return false;
    //}

    //public override void OnItemDrop(ClickableItem item)
    //{
    //    base.OnItemDrop(item);

    //    if (item == null || currentCustomer == null) return;

    //    SoupBowlZone soupBowl = item.GetComponentInParent<SoupBowlZone>();
    //    if (soupBowl != null && soupBowl.IsReadyToServe() && CheckDishMatch(soupBowl))
    //    {
    //        // 订单完成处理
    //        OnOrderCompleted();
    //    }
    //    else
    //    {
    //        // 订单失败处理
    //        OnOrderFailed();
    //    }
    //}

    ///// <summary>
    ///// 设置当前服务的客户
    ///// </summary>
    //public void SetCustomer(Customer customer)
    //{
    //    currentCustomer = customer;

    //    if (customer != null)
    //    {
    //        // 注册客户事件
    //        // 可以根据需要添加事件监听
    //    }
    //}

    ///// <summary>
    ///// 清除当前客户
    ///// </summary>
    //public void ClearCustomer()
    //{
    //    currentCustomer = null;
    //}

    ///// <summary>
    ///// 订单完成处理
    ///// </summary>
    //private void OnOrderCompleted()
    //{
    //    if (currentCustomer != null)
    //    {
    //        // 调用客户的TryServe方法
    //        Dish servedDish = GetServedDishFromBowl(); // 需要实现这个方法

    //        if (currentCustomer.TryServe(servedDish))
    //        {
    //            // 触发完成事件
    //            onOrderCompleted?.Invoke();
    //            Debug.Log("客户订单完成！");

    //            // 清除客户引用
    //            ClearCustomer();
    //        }
    //    }
    //}

    ///// <summary>
    ///// 订单失败处理
    ///// </summary>
    //private void OnOrderFailed()
    //{
    //    onOrderFailed?.Invoke();
    //    Debug.Log("订单不匹配！");
    //}

    ///// <summary>
    ///// 从汤碗中获取服务的菜品（需要根据实际SoupBowlZone实现调整）
    ///// </summary>
    //private Dish GetServedDishFromBowl()
    //{
    //    // 这里需要根据实际的SoupBowlZone实现来获取对应的Dish
    //    // 示例实现：
    //    // 1. 可以根据汤碗中的配料组合确定菜品
    //    // 2. 或者SoupBowlZone直接存储了对应的Dish引用

    //    // 临时返回null，需要根据实际游戏逻辑实现
    //    return null;
    //}

    ///// <summary>
    ///// 获取当前客户的需求信息（用于UI显示等）
    ///// </summary>
    //public List<Dish> GetCustomerRequirements()
    //{
    //    return currentCustomer?.RequiredDish;
    //}

    ///// <summary>
    ///// 获取剩余等待时间（用于UI显示等）
    ///// </summary>
    //public float GetRemainingTime()
    //{
    //    // 需要Customer类暴露timer信息，或者通过其他方式获取
    //    // 这里假设Customer类有一个公共方法或属性
    //    return currentCustomer != null ? currentCustomer.GetRemainingTime() : 0f;
    //}
}