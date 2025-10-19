using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class WrapBunVideoController : MonoBehaviour
{
    [Header("视频设置")]
    public VideoClip videoClip;
    public RawImage rawImage;

    [Header("大小控制")]
    public Vector2 videoSize = new Vector2(300, 200);

    private VideoPlayer videoPlayer;
    private RenderTexture runtimeRenderTexture; // 运行时创建

    void Awake()
    {
        videoPlayer = gameObject.GetComponent<VideoPlayer>();
        if (videoPlayer == null)
            videoPlayer = gameObject.AddComponent<VideoPlayer>();

        if (videoClip != null)
            videoPlayer.clip = videoClip;

        videoPlayer.renderMode = VideoRenderMode.APIOnly;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;

        runtimeRenderTexture = new RenderTexture(640, 360, 0);
        runtimeRenderTexture.name = "Runtime_WrapBun_RenderTexture";

        if (rawImage != null)
        {
            rawImage.texture = runtimeRenderTexture;
            videoPlayer.targetTexture = runtimeRenderTexture;

            // 设置大小
            rawImage.rectTransform.sizeDelta = videoSize;
        }

        Debug.Log("包包子视频控制器已初始化");
    }

    public void Play()
    {
        videoPlayer?.Play();
    }

    public void Pause()
    {
        videoPlayer?.Pause();
    }

    public void Stop()
    {
        videoPlayer?.Stop();
    }

    public void SetSize(Vector2 newSize)
    {
        videoSize = newSize;
        if (rawImage != null)
        {
            rawImage.rectTransform.sizeDelta = newSize;
        }
    }

    private void OnDestroy()
    {
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
        }

        if (runtimeRenderTexture != null)
        {
            Destroy(runtimeRenderTexture);
            runtimeRenderTexture = null;
        }

        Debug.Log("包包子视频资源已清理");
    }
}