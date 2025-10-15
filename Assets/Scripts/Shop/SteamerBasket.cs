using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SteamerBasket : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    
    public enum FoodType { None, Bun, OsmanthusCake }
    public FoodType currentFood = FoodType.None;

    [Header("Ԥ����ӳ��")]
    public GameObject bunPrefab;          // ������
    public GameObject cookedBunPrefab;   // �����
    public GameObject cakePrefab;         // �𻨸⣨��ʼ���У�

    [Header("��������")]
    public Animation steamAnimation;     // ��������
    public Animation lidOpenAnimation;   // ���Ƕ���
    public float cookTime = 5f;         // ����ʱ�䣨�룩

    [Header("��ʼ״̬")]
    public FoodType initialFood = FoodType.None; // ��ʼ�Ƿ���ʳ�

    private GameObject foodInstance;
    private bool isCooking = false;
    private float cookingTimer = 0f;
    private bool canOpenLid = false;

    private void Awake()
    {
        if (initialFood != FoodType.None)
        {
            SpawnFood(initialFood, true); // ֱ���������
        }
    }

    private void Update()
    {
        if (isCooking)
        {
            cookingTimer += Time.deltaTime;
            if (cookingTimer >= cookTime)
            {
                FinishCooking();
            }
        }
    }

    // ����������
    public bool CanAcceptRawBun() => currentFood == FoodType.None && !isCooking;

    public void AddRawBun(GameObject rawBun)
    {
        if (!CanAcceptRawBun()) return;

        Destroy(rawBun);
        currentFood = FoodType.Bun;
        isCooking = true;
        cookingTimer = 0f;
        canOpenLid = false;

        // ��ʾ��������
        steamAnimation?.Play();

        Debug.Log("��ʼ�����ӣ���ʱ " + cookTime + " ��");
    }

    private void FinishCooking()
    {
        isCooking = false;
        steamAnimation?.Stop();

        // �滻Ϊ�����
        if (foodInstance) Destroy(foodInstance);
        foodInstance = Instantiate(cookedBunPrefab, transform);
        foodInstance.transform.localPosition = Vector3.zero;

        canOpenLid = true;
        Debug.Log("���������ˣ�");
    }

    // ������������ǲ鿴
    public void OnPointerClick(PointerEventData eventData)
    {
        if (canOpenLid && lidOpenAnimation != null && !lidOpenAnimation.isPlaying)
        {
            lidOpenAnimation.Play();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // �����ͣ���ſ��Ƕ�������ѡ��
        if (canOpenLid && lidOpenAnimation != null && !lidOpenAnimation.isPlaying)
        {
            lidOpenAnimation.Play();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // �����Ҫ�Զ��ظǣ����������� Reverse ����
    }

    // �ڲ�����������ʳ����ڳ�ʼ״̬��
    private void SpawnFood(FoodType type, bool isCooked)
    {
        GameObject prefab = isCooked ? cookedBunPrefab : bunPrefab;
        if (type == FoodType.OsmanthusCake)
            prefab = cakePrefab;

        if (prefab != null)
        {
            foodInstance = Instantiate(prefab, transform);
            foodInstance.transform.localPosition = Vector3.zero;
        }

        currentFood = type;
        canOpenLid = true;
    }

    // �ⲿ���ã���������
    public void ResetBasket()
    {
        if (foodInstance) Destroy(foodInstance);
        if (steamAnimation) steamAnimation.Stop();
        currentFood = FoodType.None;
        isCooking = false;
        cookingTimer = 0f;
        canOpenLid = false;
    }
}
