using UnityEngine;

public class TimeColumn : MonoBehaviour
{
    public float totalTime = 180f;
    private float currentTime = 0f;
    private bool isCompleted = false;

    void Update()
    {
        // 当时间未耗尽且未完成时更新
        if (!isCompleted && currentTime < totalTime)
        {
            currentTime += Time.deltaTime;
            float progress = Mathf.Clamp01(currentTime / totalTime);
            
            // 直接修改当前组件的X轴缩放
            transform.localScale = new Vector3(
                progress,
                transform.localScale.y,
                transform.localScale.z
            );
            
            // 当达到总时长时标记完成
            isCompleted = Mathf.Approximately(progress, 1f);
        }
    }

    // 可选：提供手动重置功能
    public void ResetTimer()
    {
        currentTime = 0f;
        isCompleted = false;
        transform.localScale = new Vector3(0f, transform.localScale.y, transform.localScale.z);
    }
}