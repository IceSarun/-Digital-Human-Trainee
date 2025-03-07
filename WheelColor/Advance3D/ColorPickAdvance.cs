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
    public string colorHex; // �ѹ�֡�� HEX �� "#FF5733"
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

    //��Ǩ�ͺ�ء Object
    public List<GameObject> allObject = new List<GameObject>();
    public Transform objectParent;
    public float currentHue, currentSaturation, currentVal;

    //���� ����Ѻ Reset 
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

        // �ѻവ���� UI ��ҹ PanelManager
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
        // ��Ǩ�ͺ��Ң�ͤ���� InputField ����ö�ŧ�繵���Ţ��
        if (int.TryParse(rInput.text, out r) && int.TryParse(gInput.text, out g) && int.TryParse(bInput.text, out b))
        {
            r = Mathf.Clamp(r, 0, 255);
            g = Mathf.Clamp(g, 0, 255);
            b = Mathf.Clamp(b, 0, 255);

            Debug.Log("OK: R = " + r + ", G = " + g + ", B = " + b);

            // ���ҧ�ըҡ��� R, G, B
            Color newCol = new Color(r / 255f, g / 255f, b / 255f);
            PM.UpdateUI(newCol);
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
        //thisPanel.SetActive(false);
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

}

