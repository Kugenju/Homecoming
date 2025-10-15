using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class CustomerZone : DroppableZone
{
    [Header("�ͻ�����")]
    public Customer currentCustomer; // ֱ�����õ�ǰ�ͻ�
    public UnityEvent onOrderCompleted;
    public UnityEvent onOrderFailed;

    //private void Update()
    //{
    //    // ���ͻ��Ƿ�����ҳ�ʱ
    //    if (currentCustomer != null && currentCustomer.isUpdate)
    //    {
    //        // ����ͨ���¼������ͻ��ĳ�ʱ��������������
    //    }
    //}

    //public override bool CanAcceptItem(ClickableItem item)
    //{
    //    if (item == null || currentCustomer == null) return false;

    //    // ����Ƿ���������׼������
    //    SoupBowlZone soupBowl = item.GetComponentInParent<SoupBowlZone>();
    //    if (soupBowl != null && soupBowl.IsReadyToServe())
    //    {
    //        return CheckDishMatch(soupBowl);
    //    }

    //    return false;
    //}

    ///// <summary>
    ///// ��������еĲ�Ʒ�Ƿ���ͻ�����ƥ��
    ///// </summary>
    //private bool CheckDishMatch(SoupBowlZone soupBowl)
    //{
    //    if (currentCustomer == null || currentCustomer.RequiredDish == null)
    //        return false;

    //    // ��ȡ�����еĲ�Ʒ��Ϣ����Ҫ����ʵ��ʵ�ֵ�����
    //    // �������SoupBowlZone��һ���������Ի�ȡ��Ӧ��Dish
    //    Dish servedDish = soupBowl.GetServedDish();

    //    if (servedDish != null)
    //    {
    //        // ����Ƿ�ƥ��ͻ����������һ����Ʒ
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
    //        // ������ɴ���
    //        OnOrderCompleted();
    //    }
    //    else
    //    {
    //        // ����ʧ�ܴ���
    //        OnOrderFailed();
    //    }
    //}

    ///// <summary>
    ///// ���õ�ǰ����Ŀͻ�
    ///// </summary>
    //public void SetCustomer(Customer customer)
    //{
    //    currentCustomer = customer;

    //    if (customer != null)
    //    {
    //        // ע��ͻ��¼�
    //        // ���Ը�����Ҫ����¼�����
    //    }
    //}

    ///// <summary>
    ///// �����ǰ�ͻ�
    ///// </summary>
    //public void ClearCustomer()
    //{
    //    currentCustomer = null;
    //}

    ///// <summary>
    ///// ������ɴ���
    ///// </summary>
    //private void OnOrderCompleted()
    //{
    //    if (currentCustomer != null)
    //    {
    //        // ���ÿͻ���TryServe����
    //        Dish servedDish = GetServedDishFromBowl(); // ��Ҫʵ���������

    //        if (currentCustomer.TryServe(servedDish))
    //        {
    //            // ��������¼�
    //            onOrderCompleted?.Invoke();
    //            Debug.Log("�ͻ�������ɣ�");

    //            // ����ͻ�����
    //            ClearCustomer();
    //        }
    //    }
    //}

    ///// <summary>
    ///// ����ʧ�ܴ���
    ///// </summary>
    //private void OnOrderFailed()
    //{
    //    onOrderFailed?.Invoke();
    //    Debug.Log("������ƥ�䣡");
    //}

    ///// <summary>
    ///// �������л�ȡ����Ĳ�Ʒ����Ҫ����ʵ��SoupBowlZoneʵ�ֵ�����
    ///// </summary>
    //private Dish GetServedDishFromBowl()
    //{
    //    // ������Ҫ����ʵ�ʵ�SoupBowlZoneʵ������ȡ��Ӧ��Dish
    //    // ʾ��ʵ�֣�
    //    // 1. ���Ը��������е��������ȷ����Ʒ
    //    // 2. ����SoupBowlZoneֱ�Ӵ洢�˶�Ӧ��Dish����

    //    // ��ʱ����null����Ҫ����ʵ����Ϸ�߼�ʵ��
    //    return null;
    //}

    ///// <summary>
    ///// ��ȡ��ǰ�ͻ���������Ϣ������UI��ʾ�ȣ�
    ///// </summary>
    //public List<Dish> GetCustomerRequirements()
    //{
    //    return currentCustomer?.RequiredDish;
    //}

    ///// <summary>
    ///// ��ȡʣ��ȴ�ʱ�䣨����UI��ʾ�ȣ�
    ///// </summary>
    //public float GetRemainingTime()
    //{
    //    // ��ҪCustomer�౩¶timer��Ϣ������ͨ��������ʽ��ȡ
    //    // �������Customer����һ����������������
    //    return currentCustomer != null ? currentCustomer.GetRemainingTime() : 0f;
    //}
}