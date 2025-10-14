using System.Collections.Generic;
using UnityEngine;

public class Money : MonoBehaviour
{
    public List<GameObject> digitPrefabs; // 0-9数字预制体（编辑器拖拽赋值）
    private PlayerData playerData;
    private int currentMoney = -1; // 初始值-1确保首次更新
    private List<GameObject> currentDigits = new List<GameObject>();

    void Start()
    {
        playerData = PlayerData.Instance; // 实例化PlayerData
        UpdateMoneyDisplay();
    }

    void Update()
    {
        UpdateMoneyDisplay();
    }

    private void UpdateMoneyDisplay()
    {
        int newMoney = playerData.GetMoney();
        
        // 金钱未变化时直接返回
        if (newMoney == currentMoney) return;
        
        currentMoney = newMoney;
        
        // 销毁旧数字对象
        ClearCurrentDigits();
        
        // 分解数字（从个位开始存储）
        List<int> digits = ParseMoneyToDigits(newMoney);
        
        // 创建新数字对象
        SpawnDigitObjects(digits);
    }

    private List<int> ParseMoneyToDigits(int money)
    {
        List<int> digits = new List<int>();
        
        // 处理0的特殊情况
        if (money == 0)
        {
            digits.Add(0);
            return digits;
        }
        
        // 分解数字（个位在列表首位）
        while (money > 0)
        {
            digits.Add(money % 10);
            money /= 10;
        }
        
        // 限制最多4位（千位）
        return digits.Count > 4 ? digits.GetRange(0, 4) : digits;
    }

    private void SpawnDigitObjects(List<int> digits)
    {
        for (int i = 0; i < digits.Count; i++)
        {
            int digitValue = digits[i];
            // 确保数字有效（0-9）
            digitValue = Mathf.Clamp(digitValue, 0, 9);
            
            // 实例化数字对象
            GameObject digitObj = Instantiate(
                digitPrefabs[digitValue], 
                transform // 设置为当前对象的子对象
            );
            
            // 计算偏移位置（个位i=0无偏移，十位i=1偏移-0.2）
            float offset = -0.2f * i;
            digitObj.transform.localPosition += new Vector3(offset, 0, 0);
            
            currentDigits.Add(digitObj);
        }
    }

    private void ClearCurrentDigits()
    {
        // 反向销毁避免索引问题
        for (int i = currentDigits.Count - 1; i >= 0; i--)
        {
            if (currentDigits[i] != null)
            {
                Destroy(currentDigits[i]);
            }
            currentDigits.RemoveAt(i);
        }
    }

    private void OnDestroy()
    {
        // 确保销毁时清理所有子对象
        ClearCurrentDigits();
    }
}