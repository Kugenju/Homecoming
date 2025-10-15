using UnityEngine;
using System.Collections;

public class FillingTargetZone : DroppableZone
{
    [Header("配置")]
    public string requiredFillingTag = "MeatFilling";
    public Vector3 fillingOffset = new Vector3(0, 0.5f, 0); // 肉馅偏移量（居中）
    public float packDelay = 0.5f; // 包包子延迟（模拟动作）


    [Header("动画与预制体")]
    public Animation packAnimation; // 包包子动画组件
    public GameObject packedBunPrefab; // 包好的“生包子”预制体
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

        // 隐藏当前所有视觉部分（面皮+肉馅）
        SetVisualActive(false);

        // 播放音效
        if (packSound) audioSource.PlayOneShot(packSound);

        // 启动打包协程
        StartCoroutine(PackBunAfterDelay(item));
    }

    private IEnumerator PackBunAfterDelay(ClickableItem fillingItem)
    {
        yield return new WaitForSeconds(packDelay);

        // 销毁肉馅
        //Destroy(fillingItem.gameObject);
       
        // 停止隐藏，准备播放动画


        // 播放包包子动画
        if (packAnimation != null)
        {
            packAnimation.Play();
            yield return new WaitForSeconds(packAnimation.clip.length);
        }
        else
        {
            yield return new WaitForSeconds(0.8f); // 默认时长
        }
        SetVisualActive(true);
        // 动画结束，替换为“生包子”
        Vector3 pos = transform.position;
        Quaternion rot = transform.rotation;
        Transform parent = transform.parent;

        Destroy(gameObject); // 销毁面皮+动画

        // 实例化“生包子”
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
        // 可拖拽
        var clickable = bun.GetComponent<ClickableItem>();
        if (clickable) clickable.SetUsable(true);
    }

    private void SetVisualActive(bool active)
    {
        // 隐藏/显示所有渲染器
        foreach (var renderer in GetComponentsInChildren<Renderer>())
            renderer.enabled = active;
        foreach (var sr in GetComponentsInChildren<SpriteRenderer>())
            sr.enabled = active;
    }
}