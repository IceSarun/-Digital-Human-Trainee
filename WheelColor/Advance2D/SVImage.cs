using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SVImage : MonoBehaviour, IDragHandler, IPointerClickHandler
{
    [SerializeField]
    private Image pickerImage;
    private RawImage Svimage;
    private ColorPick CC;
    private RectTransform rectTransform, pickerTransfrom;

    private void Awake()
    {
        Svimage = GetComponent<RawImage>();
        CC = FindObjectOfType<ColorPick>();
        rectTransform = GetComponent<RectTransform>();

        pickerTransfrom = pickerImage.GetComponent<RectTransform>();
        pickerTransfrom.position = new Vector2(-(rectTransform.sizeDelta.x * 0.5f), -(rectTransform.sizeDelta.y * 0.5f) );
    }

    void UpdateColor(PointerEventData eventData) {
        Vector3 pos = rectTransform.InverseTransformPoint(eventData.position);
        float deltaX = rectTransform.sizeDelta.x * 0.5f;
        float deltaY = rectTransform.sizeDelta.y * 0.5f;

        if (pos.x < -deltaX) {
            pos.x = deltaX;
        }
        else if (pos.x > deltaX) {
            pos.x = deltaX;
        }
        if (pos.y < -deltaY) {
            pos.y = deltaY;
        }
        else if (pos.y > deltaY) {
            pos.y = deltaY; 
        }
        float x = pos.x + deltaX;
        float y = pos.y + deltaY;

        float xNorm = x / rectTransform.sizeDelta.x;
        float yNorm = y / rectTransform.sizeDelta.y;

        pickerTransfrom.localPosition= pos;
        pickerImage.color = Color.HSVToRGB(0,0,1- yNorm);
        CC.SetSV(xNorm, yNorm);
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdateColor(eventData);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        UpdateColor(eventData);
    }
    public void UpdatePickerPosition(Vector2 position)
    {
        pickerTransfrom.localPosition = position;
    }
}
