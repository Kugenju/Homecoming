using UnityEngine;
using System.Collections;

public class FillingTargetZone : DroppableZone
{
    [Header("����")]
    public string requiredFillingTag = "MeatFilling";
    public Vector3 fillingOffset = new Vector3(0, 0.5f, 0); // ����ƫ���������У�
    public float packDelay = 0.5f; // �������ӳ٣�ģ�⶯����


    [Header("������Ԥ����")]
    public Animation packAnimation; // �����Ӷ������
    public GameObject packedBunPrefab; // ���õġ������ӡ�Ԥ����
    public AudioClip packSound;

    private bool isPacked = false;
    private AudioSource audioSource;

    protected override void Awake()
    {
        base.Awake();
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
          
    }

    public override bool CanAcceptItem(ClickableItem item)
    {
        return !isPacked && item != null && item.CompareTag(requiredFillingTag);
    }

    public override void OnItemDrop(ClickableItem item)
    {
        if (isPacked) return;

        base.OnItemDrop(item);

        // ���ص�ǰ�����Ӿ����֣���Ƥ+���ڣ�
        SetVisualActive(false);

        // ������Ч
        if (packSound) audioSource.PlayOneShot(packSound);

        // �������Э��
        StartCoroutine(PackBunAfterDelay(item));
    }

    private IEnumerator PackBunAfterDelay(ClickableItem fillingItem)
    {
        yield return new WaitForSeconds(packDelay);

        // ��������
        //Destroy(fillingItem.gameObject);
       
        // ֹͣ���أ�׼�����Ŷ���


        // ���Ű����Ӷ���
        if (packAnimation != null)
        {
            packAnimation.Play();
            yield return new WaitForSeconds(packAnimation.clip.length);
        }
        else
        {
            yield return new WaitForSeconds(0.8f); // Ĭ��ʱ��
        }
        SetVisualActive(true);
        // �����������滻Ϊ�������ӡ�
        Vector3 pos = transform.position;
        Quaternion rot = transform.rotation;
        Transform parent = transform.parent;

        Destroy(gameObject); // ������Ƥ+����

        // ʵ�����������ӡ�
        GameObject bun = Instantiate(packedBunPrefab, pos, rot, parent);
        bun.name = "RawBun";

        DoughBoardZone doughBoard = FindObjectOfType<DoughBoardZone>();
        if (doughBoard != null)
        {
            int spotIndex = doughBoard.FindSpotIndexByPosition(transform.position);
            if (spotIndex >= 0)
            {
                doughBoard.UpdateOccupiedSpot(spotIndex, bun);
            }
        }
        // ����ק
        var clickable = bun.GetComponent<ClickableItem>();
        if (clickable) clickable.SetUsable(true);
    }

    private void SetVisualActive(bool active)
    {
        // ����/��ʾ������Ⱦ��
        foreach (var renderer in GetComponentsInChildren<Renderer>())
            renderer.enabled = active;
        foreach (var sr in GetComponentsInChildren<SpriteRenderer>())
            sr.enabled = active;
    }
}