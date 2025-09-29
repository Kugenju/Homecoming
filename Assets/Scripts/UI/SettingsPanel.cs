using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class SettingsPanel : MonoBehaviour
{
    // Start is called before the first frame update
    public Slider volumeSlider;
    public Dropdown qualityDropdown;

    private void Awake()
    {
        // 防止重复加载
        if (FindObjectsOfType<SettingsPanel>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        // 初始化
        volumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 0.8f);
        SetupQualityDropdown();
    }


    private void Start()
    {
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
    }

    private void SetupQualityDropdown()
    {
        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(QualitySettings.names.ToList());
        qualityDropdown.value = QualitySettings.GetQualityLevel();
    }

    public void OnVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("MasterVolume", value);
        AudioListener.volume = value;
        Debug.Log($"🔊 音量设置为: {value:F2}");
    }

    public void OnQualityChanged(int level)
    {
        QualitySettings.SetQualityLevel(level);
        Debug.Log($"🎨 画质设置为: {QualitySettings.names[level]}");
    }

    private void OnDestroy()
    {
        // 移除监听，防止内存泄漏
        if (volumeSlider != null)
            volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
        if (qualityDropdown != null)
            qualityDropdown.onValueChanged.RemoveListener(OnQualityChanged);
    }

}
