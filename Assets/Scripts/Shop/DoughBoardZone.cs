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

        // 只接受“面团”
        return item.CompareTag("Dough") && occupiedSpots.Count < spawnPositions.Count;
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

    public bool IsSpotEmpty(int index)
    {
        return GetDoughSkinAt(index) == null;
    }

    public int GetFirstEmptyIndex()
    {
        for (int i = 0; i < occupiedSpots.Count; i++)
        {
            if (IsSpotEmpty(i)) return i;
        }
        return -1;
    }

    public void UpdateOccupiedSpot(int index, GameObject newObject)
    {
        if (index < 0 || index >= occupiedSpots.Count) return;

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
