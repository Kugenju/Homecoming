using UnityEngine;
using System.Collections.Generic;

public class DoughBoardZone : DroppableZone
{
    [Header("��ƤԤ��λ��")]
    public List<Transform> spawnPositions = new List<Transform>(); // Ԥ�����Ƥλ��

    [Header("Ԥ��������")]
    public GameObject doughSkinPrefab; // ��Ƥչ�����Ԥ���壨�� ProxyDragTag��

    [Header("����ʱ״̬")]
    private List<GameObject> occupiedSpots = new List<GameObject>(); // ��ǰ��ռ�õ�λ��

    protected override void Awake()
    {
        base.Awake();
        if (spawnPositions.Count == 0)
            Debug.LogWarning($"{name} δ�����κ� spawnPositions��");
    }

    public override bool CanAcceptItem(ClickableItem item)
    {
        if (item == null) return false;

        // ֻ���ܡ����š�
        return item.CompareTag("Dough") && occupiedSpots.Count < spawnPositions.Count;
    }

    public override void OnItemDrop(ClickableItem item)
    {
        base.OnItemDrop(item);

        // ���ҵ�һ����λ
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

        // ʵ��������Ƥ��Ԥ���壨չ�����״̬��
        GameObject skinObj = Instantiate(doughSkinPrefab, pos, Quaternion.identity);
        skinObj.name = "DoughSkin_" + index;

        // �洢����
        while (occupiedSpots.Count <= index) occupiedSpots.Add(null);
        occupiedSpots[index] = skinObj;

        // ����ԭ�����š�
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
