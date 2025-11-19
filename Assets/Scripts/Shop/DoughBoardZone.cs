using UnityEngine;
using System.Collections.Generic;

public class DoughBoardZone : DroppableZone
{
    [Header("面皮预设位置")]
    public List<Transform> spawnPositions = new List<Transform>(); // 预设的面皮位置

    [Header("预制体配置")]
    public GameObject doughSkinPrefab; // 面皮展开后的预制体（带 ProxyDragTag）

    [Header("运行时状态")]
    private List<GameObject> occupiedSpots = new List<GameObject>(); // 当前被占用的位置

    protected override void Awake()
    {
        base.Awake();
        if (spawnPositions.Count == 0)
            Debug.LogWarning($"{name} 未设置任何 spawnPositions！");
    }

    public override bool CanAcceptItem(ClickableItem item)
    {
        if (item == null) return false;
        // 只接受“面团”标签，并且有空闲槽位
        return item.CompareTag("Dough") && GetFirstEmptyIndex() >= 0;
    }


    public override void OnItemDrop(ClickableItem item)
    {
        base.OnItemDrop(item);

        // 查找第一个空位
        for (int i = 0; i < spawnPositions.Count; i++)
        {
            if (i >= occupiedSpots.Count || occupiedSpots[i] == null)
            {
                PlaceDoughAt(i, item);
                break;
            }
        }
    }

    private void PlaceDoughAt(int index, ClickableItem item)
    {
        Vector3 pos = spawnPositions[index].position;

        // 实例化“面皮”预制体（展开后的状态）
        GameObject skinObj = Instantiate(doughSkinPrefab, pos, Quaternion.identity);
        skinObj.name = "DoughSkin_" + index;

        // 存储引用
        while (occupiedSpots.Count <= index) occupiedSpots.Add(null);
        occupiedSpots[index] = skinObj;

        // 销毁原“面团”
        //Destroy(item.gameObject);

        PlayFeedback();
    }

    public GameObject GetDoughSkinAt(int index)
    {
        if (index < 0 || index >= occupiedSpots.Count) return null;
        return occupiedSpots[index];
    }

    /// <summary>
    /// 判断指定槽位是否为空（支持索引超出 occupiedSpots 范围）
    /// </summary>
    public bool IsSpotEmpty(int index)
    {
        if (index < 0 || index >= spawnPositions.Count)
            return true; // 无效索引视为“空”

        // 如果 occupiedSpots 长度不够，说明该位置从未被占用 → 空
        if (index >= occupiedSpots.Count)
            return true;

        GameObject obj = occupiedSpots[index];
        // Unity 中已销毁对象 == null
        if (obj == null)
            return true;

        // 可选：如果对象被禁用，也视为可覆盖（根据需求）
        return !obj.activeInHierarchy;
    }

    /// <summary>
    /// 获取第一个真正空闲的槽位索引（基于 spawnPositions 数量）
    /// </summary>
    public int GetFirstEmptyIndex()
    {
        // 遍历所有预设槽位（不是 occupiedSpots.Count！）
        for (int i = 0; i < spawnPositions.Count; i++)
        {
            if (IsSpotEmpty(i))
                return i;
        }
        return -1;
    }

    public void UpdateOccupiedSpot(int index, GameObject newObject)
    {
        if (index < 0) return;

        // 确保列表足够长
        while (occupiedSpots.Count <= index)
        {
            occupiedSpots.Add(null);
        }

        occupiedSpots[index] = newObject;
    }

    public int FindSpotIndexByPosition(Vector3 position)
    {
        for (int i = 0; i < spawnPositions.Count; i++)
        {
            if (Vector3.Distance(spawnPositions[i].position, position) < 0.1f)
            {
                return i;
            }
        }
        return -1;
    }
}
