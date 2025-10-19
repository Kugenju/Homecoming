using UnityEngine;
using System.Collections;

public class FillingTargetZone : DroppableZone
{
    [Header("�������")]
    public string requiredFillingTag = "MeatFilling";
    public Vector3 fillingOffset = new Vector3(0, 0.5f, 0);
    public float packDelay = 0.5f;

    [Header("������Ч��")]
    public Animator packAnimator; // Animator ���
    public GameObject packedBunPrefab;
    public AudioClip packSound;

    [Header("��������")]
    public string packingBoolParameter = "IsPacking"; // Bool ��������

    private bool isPacked = false;
    private AudioSource audioSource;

    protected override void Awake()
    {
        base.Awake();
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();

        // ȷ��������ʼ״̬Ϊ Normal
        if (packAnimator != null)
        {
            packAnimator.SetBool(packingBoolParameter, false);
        }
    }

    public override bool CanAcceptItem(ClickableItem item)
    {
        return !isPacked && item != null && item.CompareTag(requiredFillingTag);
    }

    public override void OnItemDrop(ClickableItem item)
    {
        if (isPacked) return;

        base.OnItemDrop(item);

        // ���ص�ǰ�����Ӿ�����
        //SetVisualActive(false);

        // ������Ч
        if (packSound) audioSource.PlayOneShot(packSound);

        // ��ʼ���Э��
        StartCoroutine(PackBunAfterDelay(item));
    }

    private IEnumerator PackBunAfterDelay(ClickableItem fillingItem)
    {
        yield return new WaitForSeconds(packDelay);

        // ���Ŵ������ - ͨ������ Bool ������������״̬�л�
        if (packAnimator != null)
        {
            // ���� IsPacking Ϊ true�������� Normal �� PackAnimation �Ĺ���
            packAnimator.SetBool(packingBoolParameter, true);
            Debug.Log("�������������IsPacking = true");

            // �ȴ�����������ɲ�����
            yield return StartCoroutine(WaitForAnimationToComplete());
        }
        else
        {
            Debug.LogWarning("Animator not set! Using default delay.");
            yield return new WaitForSeconds(1.5f); // Ĭ���ӳ�
        }

        // ����������ɺ����ִ��
        CompletePackingProcess();
    }

    private IEnumerator WaitForAnimationToComplete()
    {
        if (packAnimator == null) yield break;

        // �ȴ�һ֡ȷ������״̬���л�
        yield return null;

        // ��ȡ��ǰ����״̬��Ϣ
        AnimatorStateInfo stateInfo = packAnimator.GetCurrentAnimatorStateInfo(0);

        // �ȴ������������
        float animationLength = stateInfo.length;
        float timer = 0f;

        while (timer < animationLength)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        // ����ȴ�һС��ʱ��ȷ��������ȫ����
        yield return new WaitForSeconds(0.1f);

        Debug.Log("��������������");
    }

    private void CompletePackingProcess()
    {
        // �ָ��Ӿ���ʾ
        SetVisualActive(true);

        // �滻Ϊ���õİ���Ԥ����
        Vector3 pos = transform.position;
        Quaternion rot = transform.rotation;
        Transform parent = transform.parent;

        // ������ǰ���ö���״̬�������Ҫ��
        if (packAnimator != null)
        {
            packAnimator.SetBool(packingBoolParameter, false);
        }

        Destroy(gameObject); // ���ٵ�ǰ����

        // ʵ�������õİ���
        GameObject bun = Instantiate(packedBunPrefab, pos, rot, parent);
        bun.name = "RawBun";

        // �������Ű��ϵ�ռλ��Ϣ
        DoughBoardZone doughBoard = FindObjectOfType<DoughBoardZone>();
        if (doughBoard != null)
        {
            int spotIndex = doughBoard.FindSpotIndexByPosition(transform.position);
            if (spotIndex >= 0)
            {
                doughBoard.UpdateOccupiedSpot(spotIndex, bun);
            }
        }

        // �����°���Ϊ����״̬
        var clickable = bun.GetComponent<ClickableItem>();
        if (clickable) clickable.SetUsable(true);
    }

    private void SetVisualActive(bool active)
    {
        // ��ʾ/����������Ⱦ��
        foreach (var renderer in GetComponentsInChildren<Renderer>())
            renderer.enabled = active;
        foreach (var sr in GetComponentsInChildren<SpriteRenderer>())
            sr.enabled = active;
    }

    // ��ѡ���ڽű�����ʱ���ö���״̬
    private void OnDisable()
    {
        if (packAnimator != null)
        {
            packAnimator.SetBool(packingBoolParameter, false);
        }
    }
}