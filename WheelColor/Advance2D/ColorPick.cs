using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;


//JSON
[System.Serializable]
public class IconColorData
{
    public string iconName;
    public string colorHex; // บันทึกเป็น HEX เช่น "#FF5733"
}

[System.Serializable]
public class IconColorSaveData
{
    public List<IconColorData> iconColors = new List<IconColorData>();
}


public class ColorPick : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField]
    private RawImage hueImage, saturationImage;

    public RawImage outputImage;

    public Slider hueSlider;

    public TMP_InputField hexInputFeild, rInput, gInput, bInput;

    [SerializeField]
    private Color colorForResetToDefaults = Color.white;

    [SerializeField]
    private GameObject resetButton;


    [Header("Dependencies")]
    public PanelManagerFor2D PM2;
    //ตรวจสอบ Theme
    public ThemeIconManager themeDatas;
    public List<GameObject> allButton = new List<GameObject>();
    public Transform iconParent;
    private Color colorWant;
    private Texture2D hueTexture, svTexture, outputTexture;
    private GameObject selectedButton; // ปุ่มที่เลือกสำหรับ Apply for This
    public float currentHue, currentSaturation, currentVal;

    //จำสี สำหรับ Reset 
    private Dictionary<GameObject, Color> defaultColors = new Dictionary<GameObject, Color>();

    private void Awake()
    {
        SetAllButton();
        LoadColorsFromJson();
    }

    private void Start()
    {
        //SaveDefault();
        CreateHueImage();
        CreateSVImage();
        CreateOutputImage();
        UpdateOutputImage();
    }

    private void SetAllButton()
    {
        resetButton.SetActive(false);
        if (iconParent != null)
        {
            // ดึง Transform ของลูกทั้งหมดใน iconParent
            Transform[] allChildren = iconParent.GetComponentsInChildren<Transform>(true);
            foreach (var child in allChildren)
            {
                // ตรวจสอบว่า GameObject นั้นมีแท็ก "SelectedIcon" หรือไม่
                if (child.gameObject.CompareTag("SelectedIcon"))
                {
                    allButton.Add(child.gameObject);
                }
            }
        }
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
        svTexture.name = "SatValTexture";

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
        outputTexture.name = "OutputTexture";
        UpdateOutputImage();
    }

    private void UpdateOutputImage()
    {
        Color currentColour = Color.HSVToRGB(currentHue, currentSaturation, currentVal);

        for (int i = 0; i < outputTexture.height; i++)
        {
            outputTexture.SetPixel(0, i, currentColour);
        }
        outputTexture.Apply();

        outputImage.texture = outputTexture;

        // Update Hex and RGB inputs
        PM2.UpdateUI(currentColour);
        colorWant = currentColour;
    }

    public void SetSV(float s, float v)
    {
        currentSaturation = s;
        currentVal = v;
        UpdateOutputImage();
    }

    public void SetUIElement(GameObject selectedObject) {
        if (selectedObject == null)
        {
            Debug.LogWarning("SetObject called with a null MeshRenderer.");
            return;
        }
        selectedButton = selectedObject;
        SetColorFromObject();
    }

    public void SetColorFromObject()
    {
        Image uiImage = selectedButton.GetComponent<Image>();
        if (uiImage != null)
        {
            Color currentColour = uiImage.color;

            resetButton.SetActive(currentColour != colorForResetToDefaults);
            PM2.UpdateUI(currentColour);
        }
    }

    public void UpdateSVIImage()
    {
        currentHue = hueSlider.value;
        if (svTexture == null)
        {
            CreateSVImage();
        }
        
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
            hexInputFeild.text = "";
            PM2.UpdateUI(newCol);
        }
    }

    public void OntexRGB()
    {
        int r, g, b;
        // ตรวจสอบว่าข้อความใน InputField สามารถแปลงเป็นตัวเลขได้
        if (int.TryParse(rInput.text, out r) && int.TryParse(gInput.text, out g) && int.TryParse(bInput.text, out b))
        {
            r = Mathf.Clamp(r, 0, 255);
            g = Mathf.Clamp(g, 0, 255);
            b = Mathf.Clamp(b, 0, 255);

            //Debug.Log("OK: R = " + r + ", G = " + g + ", B = " + b);

            // สร้างสีจากค่า R, G, B
            Color newCol = new Color(r / 255f, g / 255f, b / 255f);
            PM2.UpdateUI(newCol);
        }

    }


    public void ApplyForThis()
    {
        if (selectedButton != null)
        {
            Image uiImage = selectedButton.GetComponent<Image>();
            if (uiImage != null)
            {
                uiImage.color = colorWant;
                if (colorWant != Color.white)
                {
                    resetButton.SetActive(true);
                }
                else { 
                    resetButton.SetActive(false);
                }
                SaveColorsToJson(); // บันทึกสีที่เปลี่ยนแปลง
                themeDatas.saveColorInThemeApplyThis(colorWant,selectedButton);
                //Debug.Log($"Applied color to {selectedButton.name}");
            }
        }

        //thisPanel.SetActive(false);
        PM2.ClosePanels();
    }

    public void ApplyForAll()
    {

        foreach (GameObject button in allButton)
        {
            if (button != null)
            {
                Image uiImage = button.GetComponent<Image>();
                if (uiImage != null)
                {
                    uiImage.color = colorWant;
                }
            }
        }
        resetButton.SetActive(colorWant != colorForResetToDefaults);
        SaveColorsToJson(); // บันทึกสีทั้งหมดที่เปลี่ยนแปลง
        themeDatas.saveColorInThemeApplyAll(colorWant);
        //Debug.Log("Applied color to all buttons.");
        //thisPanel.SetActive(false);
        PM2.ClosePanels();
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
                //Debug.Log("Button Color: " + preset);

                if (selectedButton != null)
                {
                    Image uiImage = selectedButton.GetComponent<Image>();
                    if (uiImage != null)
                    {
                        uiImage.color = preset;
                        PM2.UpdateUI(preset);
                        //outputImage.color = preset;

                    }
                }
            }
        }

    }

    public void ResetColor()
    {
        Color currentColour = colorForResetToDefaults;
        Image buttonImage = selectedButton.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = currentColour; // รีเซ็ตสีให้เป็นค่าเริ่มต้น
            PM2.UpdateUI(currentColour);
        }

    }



    public void LoadColorsFromJson()
    {
        string path = Application.persistentDataPath + "/iconColors.json";

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            IconColorSaveData loadedData = JsonUtility.FromJson<IconColorSaveData>(json);

            foreach (var colorData in loadedData.iconColors)
            {
                foreach (GameObject button in allButton)
                {
                    if (button != null && button.name == colorData.iconName)
                    {
                        Image buttonImage = button.GetComponent<Image>();
                        if (buttonImage != null && ColorUtility.TryParseHtmlString("#" + colorData.colorHex, out Color loadedColor))
                        {
                            buttonImage.color = loadedColor;
                        }
                    }
                }
            }

            //Debug.Log("Loaded icon colors from JSON.");
        }
        
    }
    public void SaveColorsToJson()
    {
        IconColorSaveData saveData = new IconColorSaveData();

        foreach (GameObject button in allButton)
        {
            if (button != null)
            {
                Image buttonImage = button.GetComponent<Image>();
                if (buttonImage != null)
                {
                    IconColorData colorData = new IconColorData
                    {
                        iconName = button.name,
                        colorHex = ColorUtility.ToHtmlStringRGB(buttonImage.color)
                    };
                    saveData.iconColors.Add(colorData);
                }
            }
        }

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(Application.persistentDataPath + "/iconColors.json", json);
        //Debug.Log("Saved icon colors to JSON.");
    }
}
