using UnityEngine;

public class MusicTest : MonoBehaviour
{
    // 要切换到的音乐索引（对应MusicManager中musicClips列表的索引）
    // [Tooltip("切换目标音乐在MusicManager的musicClips列表中的索引")]
    // public int targetMusicIndex = 0;

    // 新增：点击时播放的音效
    [Tooltip("点击行为触发的音效")]
    public AudioClip clickSound;

    // private float timer;
    // private bool isWaiting = true;

    // private void Awake()
    // {
    //     // 初始化计时器
    //     timer = 0;
    // }

    private void Update()
    {
        // // 等待3秒后执行切换音乐逻辑
        // if (isWaiting && MusicManager.Instance != null)
        // {
        //     timer += Time.deltaTime;
            
        //     if (timer >= 3f)
        //     {
        //         SwitchTargetMusic();
        //         isWaiting = false; // 只执行一次
        //     }
        // }
        // else if (MusicManager.Instance == null && isWaiting)
        // {
        //     Debug.LogWarning("MusicManager实例不存在，请确保已在场景中创建");
        //     isWaiting = false; // 避免重复打印警告
        // }

        // 检测点击行为（鼠标左键或触屏点击）
        if (Input.GetMouseButtonDown(0)) // 0表示鼠标左键，触屏设备也会触发
        {
            PlayClickSound();
        }
    }

    /// <summary>
    /// 切换到目标音乐
    /// </summary>
    // private void SwitchTargetMusic()
    // {
    //     if (MusicManager.Instance != null)
    //     {
    //         MusicManager.Instance.ChangeMusicByIndex(targetMusicIndex);
    //         Debug.Log($"已在{timer:F2}秒后切换到音乐索引: {targetMusicIndex}");
    //     }
    // }

    /// <summary>
    /// 播放点击音效
    /// </summary>
    private void PlayClickSound()
    {
        // 检查音效和管理器是否有效
        if (clickSound != null && MusicManager.Instance != null)
        {
            MusicManager.Instance.PlaySound(clickSound);
        }
        else if (clickSound == null)
        {
            Debug.LogWarning("未分配点击音效，请在Inspector中设置clickSound");
        }
    }
}