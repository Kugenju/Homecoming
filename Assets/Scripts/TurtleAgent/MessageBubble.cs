using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Image))]
public class MessageBubble : MonoBehaviour
{
    [Header("气泡样式")]
    public Sprite playerBubbleSprite;
    public Sprite aiBubbleSprite;
    public Color playerBubbleColor = new Color(0.2f, 0.6f, 1f, 1f);
    public Color aiBubbleColor = new Color(0.9f, 0.9f, 0.9f, 1f);

    [Header("文本颜色")]
    public Color playerTextColor = Color.white;
    public Color aiTextColor = Color.black;

    public void SetupBubbleStyle(bool isPlayer)
    {
        Image bubbleImage = GetComponent<Image>();
        TMP_Text textComponent = GetComponentInChildren<TMP_Text>();

        if (bubbleImage != null)
        {
            bubbleImage.sprite = isPlayer ? playerBubbleSprite : aiBubbleSprite;
            bubbleImage.color = isPlayer ? playerBubbleColor : aiBubbleColor;
            bubbleImage.type = Image.Type.Sliced;
        }

        if (textComponent != null)
        {
            textComponent.color = isPlayer ? playerTextColor : aiTextColor;
        }
    }
}