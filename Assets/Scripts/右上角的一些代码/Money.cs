using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Money : MonoBehaviour
{
    public List<GameObject> digitPrefabs; // 0-9数字预制体（必须带SpriteRenderer组件）
    public bool isImage = true; // 是否用UI图片显示（从预制体的SpriteRenderer取图）
    public float digitSpacing = -50f; // 间距（UI模式为像素，非UI为单位）
    private PlayerData playerData;
    private int currentMoney = -1;
    private List<GameObject> currentDigits = new List<GameObject>();

    void Start()
    {
        // 校验预制体是否包含SpriteRenderer
        if (digitPrefabs.Count != 10)
        {
            Debug.LogError("digitPrefabs必须包含0-9共10个预制体！");
            enabled = false;
            return;
        }
        foreach (var prefab in digitPrefabs)
        {
            if (prefab.GetComponent<SpriteRenderer>() == null)
            {
                Debug.LogError($"预制体{prefab.name}缺少SpriteRenderer组件！");
                enabled = false;
                return;
            }
        }

        playerData = PlayerData.Instance;
        UpdateMoneyDisplay();
    }

    void Update()
    {
        UpdateMoneyDisplay();
    }

    private void UpdateMoneyDisplay()
    {
        int newMoney = playerData.GetMoney();
        if (newMoney == currentMoney) return;

        currentMoney = newMoney;
        ClearCurrentDigits();
        List<int> digits = ParseMoneyToDigits(newMoney);
        SpawnDigitObjects(digits);
    }

    private List<int> ParseMoneyToDigits(int money)
    {
        List<int> digits = new List<int>();
        if (money == 0)
        {
            digits.Add(0);
            return digits;
        }
        while (money > 0)
        {
            digits.Add(money % 10);
            money /= 10;
        }
        return digits.Count > 4 ? digits.GetRange(0, 4) : digits;
    }

    private void SpawnDigitObjects(List<int> digits)
    {
        for (int i = 0; i < digits.Count; i++)
        {
            int digitValue = Mathf.Clamp(digits[i], 0, 9);
            GameObject digitObj;

            if (isImage)
            {
                // 图片模式：创建UI Image，从预制体的SpriteRenderer取图
                digitObj = new GameObject($"Digit_UI_{digitValue}");
                digitObj.transform.SetParent(transform, false); // 父对象应为Canvas子对象

                // 添加UI Image组件
                Image digitImage = digitObj.AddComponent<Image>();
                // 从预制体的SpriteRenderer中获取精灵图片
                digitImage.sprite = digitPrefabs[digitValue].GetComponent<SpriteRenderer>().sprite;

                // 调整UI位置和尺寸
                RectTransform rect = digitObj.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(-digitSpacing, - 2 * digitSpacing); // 图片尺寸（可根据实际调整）
                rect.anchoredPosition = new Vector2(digitSpacing * i, 0); // 横向排列
            }
            else
            {
                // 非图片模式：直接实例化预制体（使用其SpriteRenderer渲染）
                digitObj = Instantiate(digitPrefabs[digitValue], transform);
                digitObj.transform.localPosition += new Vector3(digitSpacing * i, 0, 0);
            }

            currentDigits.Add(digitObj);
        }
    }

    private void ClearCurrentDigits()
    {
        foreach (var digit in currentDigits)
        {
            if (digit != null) Destroy(digit);
        }
        currentDigits.Clear();
    }

    private void OnDestroy()
    {
        ClearCurrentDigits();
    }
}