using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ColorPickButtonAdvance : MonoBehaviour
{
    public float currentHue, currentSaturation, currentVal;

    [SerializeField]
    private RawImage hueImage, saturationImage, outputImage;
    public GameObject uiElement;
    public GameObject[] allUIElement;

    [SerializeField]
    private Slider hueSlider;

    [SerializeField]
    private TMP_InputField hexInputFeild, rInput, gInput, bInput;

    private Texture2D hueTexture, svTexture, outputTexture;



    private void Start()
    {
        CreateHueImage();
        CreateSVImage();
        CreateOutputImage();
        UpdateOutputImage();

    }

    private void CreateHueImage()
    {

        hueTexture = new Texture2D(1, 16);
        hueTexture.wrapMode = TextureWrapMode.Clamp;
        hueTexture.name = "HueTexture";

        for (int i = 0; i < hueTexture.height; i++)
        {
            hueTexture.SetPixel(0, i, Color.HSVToRGB((float)i / hueTexture.height, 1, 1));

        }

        hueTexture.Apply();
        currentHue = 0;

        hueImage.texture = hueTexture;

    }

    private void CreateSVImage()
    {
        svTexture = new Texture2D(16, 16);
        svTexture.wrapMode = TextureWrapMode.Clamp;
        svTexture.name = "SatValTecture";

        for (int y = 0; y < svTexture.height; y++)
        {
            for (int x = 0; x < svTexture.width; x++)
            {

                svTexture.SetPixel(x, y, Color.HSVToRGB(currentHue, (float)x / svTexture.width, (float)y / svTexture.height));

            }
        }

        svTexture.Apply();
        currentSaturation = 0;
        currentVal = 1;

        saturationImage.texture = svTexture;

    }

    private void CreateOutputImage()
    {
        outputTexture = new Texture2D(1, 16);
        outputTexture.wrapMode = TextureWrapMode.Clamp;
        outputTexture.name = "outputTexture";

        Color currentColour = Color.HSVToRGB(currentHue, currentSaturation, currentVal);

        for (int i = 0; i < outputTexture.height; i++)
        {
            outputTexture.SetPixel(0, i, currentColour);
        }
        outputTexture.Apply();
        outputImage.texture = outputTexture;

    }

    public void UpdateOutputImage()
    {
        Color currentColour = Color.HSVToRGB(currentHue, currentSaturation, currentVal);

        for (int i = 0; i < outputTexture.height; i++)
        {
            outputTexture.SetPixel(0, i, currentColour);
        }
        outputTexture.Apply();

        //Input Hex
        hexInputFeild.text = ColorUtility.ToHtmlStringRGB(currentColour);
        //Input RGB
        rInput.text = Mathf.RoundToInt(currentColour.r * 255).ToString();
        gInput.text = Mathf.RoundToInt(currentColour.g * 255).ToString();
        bInput.text = Mathf.RoundToInt(currentColour.b * 255).ToString();


        if (uiElement != null)
        {
            //Debug.Log("pass");
            Image uiImage = uiElement.GetComponent<Image>();
            if (uiImage != null)
            {
                uiImage.color = currentColour; // เปลี่ยนสีของ UI Image
                //Debug.Log("change");
            }

        }


    }

    public void SetSVButton(float s, float v)
    {
        currentSaturation = s;
        currentVal = v;
        UpdateOutputImage();
    }

    public void SetUIElement(GameObject sObject)
    {
        if (sObject == null)
        {
            //Debug.LogWarning("SetObject called with a null MeshRenderer.");
            return;
        }

        uiElement = sObject;
        //Debug.Log($"คุณกดที่: {uiElement.name}, ประเภท: {uiElement.GetType()}");
        SetColorFromObject();
    }

    public void SetColorFromObject()
    {
        Image uiImage = uiElement.GetComponent<Image>();
        Color currentColour = uiImage.color;


        Color.RGBToHSV(currentColour, out currentHue, out currentSaturation, out currentVal);
        hueSlider.value = currentHue;

        hexInputFeild.text = ColorUtility.ToHtmlStringRGB(currentColour);
        rInput.text = Mathf.RoundToInt(currentColour.r * 255).ToString();
        gInput.text = Mathf.RoundToInt(currentColour.g * 255).ToString();
        bInput.text = Mathf.RoundToInt(currentColour.b * 255).ToString();

        SVIImage sviImage = FindObjectOfType<SVIImage>();
        if (sviImage != null)
        {
            RectTransform rectTransform = sviImage.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                float deltaX = rectTransform.sizeDelta.x * 0.5f;
                float deltaY = rectTransform.sizeDelta.y * 0.5f;

                float pickerX = (currentSaturation * rectTransform.sizeDelta.x) - deltaX;
                float pickerY = (currentVal * rectTransform.sizeDelta.y) - deltaY;

                // อัปเดตตำแหน่ง picker
                sviImage.UpdatePickerPosition(new Vector2(pickerX, pickerY));
            }
        }
        outputImage.color = currentColour;
    }

    public void UpdateSVIImage()
    {
        currentHue = hueSlider.value;
        for (int y = 0; y < svTexture.height; y++)
        {
            for (int x = 0; x < svTexture.width; x++)
            {
                svTexture.SetPixel(x, y, Color.HSVToRGB(currentHue, (float)x / svTexture.width, (float)y / svTexture.height));
            }
        }
        svTexture.Apply();
        UpdateOutputImage();
    }

    public void OntextInput()
    {
        if (hexInputFeild.text.Length < 6) { return; }

        Color newCol;

        if (ColorUtility.TryParseHtmlString("#" + hexInputFeild.text, out newCol))
        {

            Color.RGBToHSV(newCol, out currentHue, out currentSaturation, out currentVal);

            hueSlider.value = currentHue;
            hexInputFeild.text = "";
            SVIImage sviImage = FindObjectOfType<SVIImage>();
            if (sviImage != null)
            {
                RectTransform rectTransform = sviImage.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    float deltaX = rectTransform.sizeDelta.x * 0.5f;
                    float deltaY = rectTransform.sizeDelta.y * 0.5f;

                    float pickerX = (currentSaturation * rectTransform.sizeDelta.x) - deltaX;
                    float pickerY = (currentVal * rectTransform.sizeDelta.y) - deltaY;

                    // อัปเดตตำแหน่ง picker
                    sviImage.UpdatePickerPosition(new Vector2(pickerX, pickerY));
                }
            }
            UpdateOutputImage();
        }
    }

    public void OntexRGB()
    {
        int r, g, b;
        Color newCol;
        // ตรวจสอบว่าข้อความใน InputField สามารถแปลงเป็นตัวเลขได้
        if (int.TryParse(rInput.text, out r) && int.TryParse(gInput.text, out g) && int.TryParse(bInput.text, out b))
        {
            r = Mathf.Clamp(r, 0, 255);
            g = Mathf.Clamp(g, 0, 255);
            b = Mathf.Clamp(b, 0, 255);

            Debug.Log("OK: R = " + r + ", G = " + g + ", B = " + b);

            // สร้างสีจากค่า R, G, B
            newCol = new Color(r / 255f, g / 255f, b / 255f);
            Color.RGBToHSV(newCol, out currentHue, out currentSaturation, out currentVal);
            hueSlider.value = currentHue;
            SVIImage sviImage = FindObjectOfType<SVIImage>();
            if (sviImage != null)
            {
                RectTransform rectTransform = sviImage.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    float deltaX = rectTransform.sizeDelta.x * 0.5f;
                    float deltaY = rectTransform.sizeDelta.y * 0.5f;

                    float pickerX = (currentSaturation * rectTransform.sizeDelta.x) - deltaX;
                    float pickerY = (currentVal * rectTransform.sizeDelta.y) - deltaY;

                    // อัปเดตตำแหน่ง picker
                    sviImage.UpdatePickerPosition(new Vector2(pickerX, pickerY));
                }
            }
            UpdateOutputImage();
        }

    }

    public void PickColorFromPreset(GameObject button)
    {
        if (button != null)
        {
            // ดึง Image Component ของปุ่มที่ถูกกด
            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {

                Color preset = buttonImage.color;
                Debug.Log("Button Color: " + preset);

                if (uiElement != null)
                {
                    Image uiImage = uiElement.GetComponent<Image>();
                    if (uiImage != null)
                    {
                        uiImage.color = preset;
                        //Debug.Log("Updated uiElement color to: " + preset);
                        Color.RGBToHSV(preset, out currentHue, out currentSaturation, out currentVal);
                        hueSlider.value = currentHue;

                        hexInputFeild.text = ColorUtility.ToHtmlStringRGB(preset);
                        rInput.text = Mathf.RoundToInt(preset.r * 255).ToString();
                        gInput.text = Mathf.RoundToInt(preset.g * 255).ToString();
                        bInput.text = Mathf.RoundToInt(preset.b * 255).ToString();

                        SVIImage sviImage = FindObjectOfType<SVIImage>();
                        if (sviImage != null)
                        {
                            RectTransform rectTransform = sviImage.GetComponent<RectTransform>();
                            if (rectTransform != null)
                            {
                                float deltaX = rectTransform.sizeDelta.x * 0.5f;
                                float deltaY = rectTransform.sizeDelta.y * 0.5f;

                                float pickerX = (currentSaturation * rectTransform.sizeDelta.x) - deltaX;
                                float pickerY = (currentVal * rectTransform.sizeDelta.y) - deltaY;

                                // อัปเดตตำแหน่ง picker
                                sviImage.UpdatePickerPosition(new Vector2(pickerX, pickerY));
                            }
                        }
                        outputImage.color = preset;

                    }
                }
            }
        }

    }
}
