using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using System.IO;

[System.Serializable]
public class ObjectColorData
{
    public string objectName;
    public string colorHex; // บันทึกเป็น HEX เช่น "#FF5733"
}

[System.Serializable]
public class ObjectColorSaveData
{
    public List<ObjectColorData> objectColors = new List<ObjectColorData>();
}


public class ColorPickAdvance : MonoBehaviour
{
    

    [SerializeField]
    private RawImage hueImage, saturationImage;

    public RawImage outputImage;

    public Slider hueSlider;

    public TMP_InputField hexInputFeild, rInput, gInput, bInput;

    private Texture2D hueTexture, svTexture, outputTexture;

    [SerializeField]
    MeshRenderer changeThisColor;

    public PanelManager PM;
    //public GameObject thisPanel;
    public GameObject resetButton;

    [SerializeField]
    private Color colorForResetToDefaults = Color.white;

    //ตรวจสอบทุก Object
    public List<GameObject> allObject = new List<GameObject>();
    public Transform objectParent;
    public float currentHue, currentSaturation, currentVal;

    //จำสี สำหรับ Reset 
    private Dictionary<GameObject, Color> defaultColors = new Dictionary<GameObject, Color>();
    
    public void Awake()
    {
        SetAllObject();
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

    private void SetAllObject()
    {
        if (objectParent != null)
        {
            // ดึง Transform ของลูกทั้งหมดใน iconParent
            Transform[] allChildren = objectParent.GetComponentsInChildren<Transform>(true);
            foreach (var child in allChildren)
            {
                if (child.gameObject.CompareTag("Selectable"))
                {
                    allObject.Add(child.gameObject);
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
        //currentHue = 0;
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
        //currentSaturation = 0;
        //currentVal = 1;

        saturationImage.texture = svTexture;

    }

    private void CreateOutputImage()
    {
        outputTexture = new Texture2D(1, 16);
        outputTexture.wrapMode = TextureWrapMode.Clamp;
        outputTexture.name = "outputTexture";
        UpdateOutputImage();

    }

    public void UpdateOutputImage()
    {
        Color currentColour = Color.HSVToRGB(currentHue, currentSaturation, currentVal);
        if(outputTexture == null)
        {
            CreateOutputImage();
        }

        for (int i = 0; i < outputTexture.height; i++)
        {
            outputTexture.SetPixel(0, i, currentColour);
        }
        outputTexture.Apply();
        outputImage.texture = outputTexture;

        // อัปเดตค่าใน UI ผ่าน PanelManager
        PM.UpdateUI(currentColour);
        changeThisColor.material.color = currentColour;
        //changeThisColor.material.SetColor("_BaseColor", currentColour);
        //changeThisColor.GetComponent<MeshRenderer>().material.color = currentColour;

    }

    public void SetSV(float s, float v)
    {
        currentSaturation = s;
        currentVal = v;
        UpdateOutputImage();
    }

    public void SetObject(MeshRenderer sObject)
    {
        if (sObject == null)
        {
            Debug.LogWarning("SetObject called with a null MeshRenderer.");
            return;
        }

        changeThisColor = sObject;
        Color currentColour = changeThisColor.material.color;

        // ตรวจสอบ hueSlider.value ถ้ายังไม่ได้เซ็ต ให้ใช้ค่าปัจจุบันของสีแทน
        if (hueSlider.value == 0)
        {
            Color.RGBToHSV(currentColour, out currentHue, out currentSaturation, out currentVal);
            hueSlider.value = currentHue; // กำหนดค่าให้ slider
        }
        else
        {
            currentHue = hueSlider.value;
        }

        PM.UpdateUI(currentColour);
        resetButton.SetActive(currentColour != colorForResetToDefaults);

    }


    public void UpdateSVIImage()
    {
        currentHue = hueSlider.value;
        if (svTexture == null) { 
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
            PM.UpdateUI(newCol);
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

            Debug.Log("OK: R = " + r + ", G = " + g + ", B = " + b);

            // สร้างสีจากค่า R, G, B
            Color newCol = new Color(r / 255f, g / 255f, b / 255f);
            PM.UpdateUI(newCol);
        }

    }

    public void ApplyForThis()
    {
        if (changeThisColor != null)
        {
            // กำหนดค่าสีที่เลือกให้กับ Material ของ MeshRenderer
            changeThisColor.material.color = Color.HSVToRGB(currentHue, currentSaturation, currentVal);

            // แสดง Debug Log เพื่อเช็คว่ามีการเปลี่ยนสีหรือไม่
            //Debug.Log($"Applied color to {changeThisColor.gameObject.name}: " + changeThisColor.material.color);

            // เปิดปุ่ม Reset ถ้าสีที่เปลี่ยนไม่ใช่สีเริ่มต้น
            if (defaultColors.ContainsKey(changeThisColor.gameObject) &&
                changeThisColor.material.color != defaultColors[changeThisColor.gameObject])
            {
                resetButton.SetActive(true);
            }
            else
            {
                resetButton.SetActive(false);
            }
        }
        SaveColorsToJson(); // บันทึกสีทั้งหมดที่เปลี่ยนแปลง

        // ปิดพาเนล UI
        //thisPanel.SetActive(false);
        PM.ClosePanels();
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

                if (changeThisColor != null)
                {
                    changeThisColor.material.color = preset;
                    PM.UpdateUI(preset);
                }
            }
        }

    }
    public void ResetColor()
    {
        changeThisColor.material.color = colorForResetToDefaults;
        PM.UpdateUI(colorForResetToDefaults);
    }

    public void LoadColorsFromJson()
    {
        string path = Application.persistentDataPath + "/objectColors.json";

        if (!File.Exists(path))
        {
            Debug.LogWarning("ไม่พบไฟล์ JSON สำหรับโหลดสี");
            return;
        }

        string json = File.ReadAllText(path);
        ObjectColorSaveData loadedData = JsonUtility.FromJson<ObjectColorSaveData>(json);
        Debug.Log(json);
        foreach (var colorData in loadedData.objectColors)
        {
            foreach (GameObject obj in allObject) // ค้นหาวัตถุที่อยู่ใน allObject
            {
                //Debug.Log("name = "+ colorData.objectName + " , name object = "+ obj.name);
                if (obj != null && obj.name == colorData.objectName)
                {
                    MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
                    if (renderer != null && ColorUtility.TryParseHtmlString("#" + colorData.colorHex, out Color loadedColor))
                    {
                        renderer.material.color = loadedColor;
                        Debug.Log($"โหลดสี {loadedColor} ไปที่ {obj.name}");
                    }
                }
            }
        }
    }

    public void SaveColorsToJson()
    {
        ObjectColorSaveData saveData = new ObjectColorSaveData();

        // วนลูปผ่านทุก object ใน allObject
        foreach (GameObject obj in allObject)
        {
            if (obj != null)
            {
                // พยายามดึง Component MeshRenderer ของ object นี้
                MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    // สร้างข้อมูลสีสำหรับ object นี้ โดยใช้สีปัจจุบันของ material
                    ObjectColorData colorData = new ObjectColorData
                    {
                        objectName = obj.name,
                        colorHex = ColorUtility.ToHtmlStringRGB(renderer.material.color)
                    };
                    saveData.objectColors.Add(colorData);
                }
            }
        }

        // แปลงข้อมูล saveData เป็น JSON และเขียนลงไฟล์
        string json = JsonUtility.ToJson(saveData, true);
        //Debug.Log(json);
        File.WriteAllText(Application.persistentDataPath + "/objectColors.json", json);
    }

}

