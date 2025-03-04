using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using System.IO;
//using UnityEngine.UIElements;

[System.Serializable]
public class ObjectColorData
{
    public string objectName;
    public string colorHex; // �ѹ�֡�� HEX �� "#FF5733"
}

[System.Serializable]
public class ObjectColorSaveData
{
    public List<ObjectColorData> objectColors = new List<ObjectColorData>();
}


public class ColorPickAdvance : MonoBehaviour
{
    public float currentHue, currentSaturation, currentVal;

    [SerializeField]
    private RawImage hueImage, saturationImage, outputImage;

    [SerializeField]
    private Slider hueSlider;

    [SerializeField]
    private TMP_InputField hexInputFeild, rInput, gInput, bInput;

    private Texture2D hueTexture, svTexture, outputTexture;

    [SerializeField]
    MeshRenderer changeThisColor;

    //���� ����Ѻ Reset 
    private Dictionary<GameObject, Color> defaultColors = new Dictionary<GameObject, Color>();

    public PanelManager PM;
    public GameObject thisPanel;
    public GameObject resetButton;
    [SerializeField]
    private Color colorForResetToDefaults = Color.white;

    //��Ǩ�ͺ�ء Object
    public List<GameObject> allObject = new List<GameObject>();
    public Transform objectParent;

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
            // �֧ Transform �ͧ�١������� iconParent
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

    /*private void SaveDefault()
    {
        resetButton.SetActive(false);
        if (changeThisColor != null)
        {
            // �纤����������鹢ͧ�ѵ��
            Color originalColor = changeThisColor.material.color;
            if (!defaultColors.ContainsKey(changeThisColor.gameObject))
            {
                defaultColors[changeThisColor.gameObject] = originalColor;
            }
        }
    }*/

    public void LoadColorsFromJson()
    {
        string path = Application.persistentDataPath + "/objectColors.json";

        if (!File.Exists(path))
        {
            Debug.LogWarning("��辺��� JSON ����Ѻ��Ŵ��");
            return;
        }

        string json = File.ReadAllText(path);
        ObjectColorSaveData loadedData = JsonUtility.FromJson<ObjectColorSaveData>(json);
        Debug.Log(json);
        foreach (var colorData in loadedData.objectColors)
        {
            foreach (GameObject obj in allObject) // �����ѵ�ط������� allObject
            {
                //Debug.Log("name = "+ colorData.objectName + " , name object = "+ obj.name);
                if (obj != null && obj.name == colorData.objectName)
                {
                    MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
                    if (renderer != null && ColorUtility.TryParseHtmlString("#" + colorData.colorHex, out Color loadedColor))
                    {
                        renderer.material.color = loadedColor;
                        Debug.Log($"��Ŵ�� {loadedColor} 价�� {obj.name}");
                    }
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

        //Input Hex
        hexInputFeild.text = ColorUtility.ToHtmlStringRGB(currentColour);
        //Input RGB
        rInput.text = Mathf.RoundToInt(currentColour.r * 255).ToString();
        gInput.text = Mathf.RoundToInt(currentColour.g * 255).ToString();
        bInput.text = Mathf.RoundToInt(currentColour.b * 255).ToString();

        
        changeThisColor.material.SetColor("_BaseColor", currentColour);
        changeThisColor.GetComponent<MeshRenderer>().material.color = currentColour;

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

        // ��Ǩ�ͺ hueSlider.value ����ѧ������� ������һѨ�غѹ�ͧ��᷹
        if (hueSlider.value == 0)
        {
            Color.RGBToHSV(currentColour, out currentHue, out currentSaturation, out currentVal);
            hueSlider.value = currentHue; // ��˹������� slider
        }
        else
        {
            currentHue = hueSlider.value;
        }

        UpdateUI(currentColour);
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
            UpdateUI(newCol);
        }
    }

    public void OntexRGB()
    {
        int r, g, b;
        // ��Ǩ�ͺ��Ң�ͤ���� InputField ����ö�ŧ�繵���Ţ��
        if (int.TryParse(rInput.text, out r) && int.TryParse(gInput.text, out g) && int.TryParse(bInput.text, out b))
        {
            r = Mathf.Clamp(r, 0, 255);
            g = Mathf.Clamp(g, 0, 255);
            b = Mathf.Clamp(b, 0, 255);

            Debug.Log("OK: R = " + r + ", G = " + g + ", B = " + b);

            // ���ҧ�ըҡ��� R, G, B
            Color newCol = new Color(r / 255f, g / 255f, b / 255f);
            UpdateUI(newCol);
        }

    }

    public void ApplyForThis()
    {
        if (changeThisColor != null)
        {
            // ��˹�����շ�����͡���Ѻ Material �ͧ MeshRenderer
            changeThisColor.material.color = Color.HSVToRGB(currentHue, currentSaturation, currentVal);

            // �ʴ� Debug Log ����������ա������¹���������
            //Debug.Log($"Applied color to {changeThisColor.gameObject.name}: " + changeThisColor.material.color);

            // �Դ���� Reset ����շ������¹��������������
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
        SaveColorsToJson(); // �ѹ�֡�շ������������¹�ŧ

        // �Դ���� UI
        thisPanel.SetActive(false);
        PM.ClosePanels();
    }


    public void PickColorFromPreset(GameObject button)
    {
        if (button != null)
        {
            // �֧ Image Component �ͧ�������١��
            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {

                Color preset = buttonImage.color;
                //Debug.Log("Button Color: " + preset);

                if (changeThisColor != null)
                {
                    changeThisColor.material.color = preset;
                    UpdateUI(preset);
                }
            }
        }

    }
    public void ResetColor()
    {
        /*Color defaultColor = colorForResetToDefaults;
        if (changeThisColor != null && defaultColors.ContainsKey(changeThisColor.gameObject))
        {
            // �֧�����������鹢ͧ�ѵ��
            //Color defaultColor = defaultColors[changeThisColor.gameObject];
            changeThisColor.material.color = defaultColor;
        }*/
        changeThisColor.material.color = colorForResetToDefaults;
        UpdateUI(colorForResetToDefaults);
    }


    public void SaveColorsToJson()
    {
        ObjectColorSaveData saveData = new ObjectColorSaveData();

        // ǹ�ٻ��ҹ�ء object � allObject
        foreach (GameObject obj in allObject)
        {
            if (obj != null)
            {
                // �������֧ Component MeshRenderer �ͧ object ���
                MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    // ���ҧ������������Ѻ object ��� �����ջѨ�غѹ�ͧ material
                    ObjectColorData colorData = new ObjectColorData
                    {
                        objectName = obj.name,
                        colorHex = ColorUtility.ToHtmlStringRGB(renderer.material.color)
                    };
                    saveData.objectColors.Add(colorData);
                }
            }
        }

        // �ŧ������ saveData �� JSON �����¹ŧ���
        string json = JsonUtility.ToJson(saveData, true);
        //Debug.Log(json);
        File.WriteAllText(Application.persistentDataPath + "/objectColors.json", json);
    }

    private void UpdateUI(Color color)
    {
        Color.RGBToHSV(color, out currentHue, out currentSaturation, out currentVal);
        hexInputFeild.text = ColorUtility.ToHtmlStringRGB(color);
        rInput.text = Mathf.RoundToInt(color.r * 255).ToString();
        gInput.text = Mathf.RoundToInt(color.g * 255).ToString();
        bInput.text = Mathf.RoundToInt(color.b * 255).ToString();
        hueSlider.value = currentHue;

        SVImageAdvance sviImage = FindObjectOfType<SVImageAdvance>();
        if (sviImage != null)
        {
            RectTransform rectTransform = sviImage.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                float deltaX = rectTransform.sizeDelta.x * 0.5f;
                float deltaY = rectTransform.sizeDelta.y * 0.5f;

                float pickerX = (currentSaturation * rectTransform.sizeDelta.x) - deltaX;
                float pickerY = (currentVal * rectTransform.sizeDelta.y) - deltaY;

                sviImage.UpdatePickerPosition(new Vector2(pickerX, pickerY));
            }
        }
        outputImage.color = color;
    }

}

