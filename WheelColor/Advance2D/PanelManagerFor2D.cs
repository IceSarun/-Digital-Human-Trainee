using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PanelManagerFor2D : MonoBehaviour
{
    [Header("UI Elements")]
    //Find button in scene 
    public GameObject panelColor;
    public Transform iconParent;

    [Header("Dependencies")]
    //Edit
    public EditorManager editorMode;
    public ColorPick CP;

    public List<GameObject> uiObject = new List<GameObject>();
    private bool isPanelOpen = false; // ʶҹТͧ Panel
    private bool checkObject = false; // ��Ǩ�ͺ�ѵ�ط�衴
    private GameObject selectedObject;

    [Header("Color Presets")]
    //Preset Color
    //private Color selectedColor; // �������������Ѻ���Ӥ����
    public Color[] colorPresets = new Color[10]; 
    private int presetCount = 0; // �ӹǹ�շ��ѹ�֡� Preset
    public Image[] colorPresetUI = new Image[10];

    private void Start()
    {
        panelColor.SetActive(false);
        InitializePresetUI();
        LoadAllIcons();
        
    }
    void Update()
    {
        HandleMouseClick();
        
    }
    private void HandleMouseClick()
    {
        if (Input.GetMouseButtonDown(0) && EventSystem.current.IsPointerOverGameObject() && !isPanelOpen && editorMode.checkColorModePanel)
        {
            selectedObject = EventSystem.current.currentSelectedGameObject;

            if (selectedObject != null)
            {
                HandleGameObjectClick(selectedObject);
            }

        }
    }

    // �ѧ��ѹ�Ѵ��ä�ԡ GameObject
    private void HandleGameObjectClick(GameObject clickedObject)
    {
        // ��Ǩ�ͺ��� GameObject �� Tag "SelectedIcon" �������
        if (!clickedObject.CompareTag("SelectedIcon"))
        {
            Debug.Log($"{clickedObject.name} does not have the 'SelectedIcon' tag.");
            return;
        }

        // �Դ�����ͺ�ͧ GameObject ����
        DisableSelectedIconButtons(clickedObject);
        checkObject = true;

        // ��Ǩ�ͺ����繻��� (Button) �������
        Button button = clickedObject.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.Invoke(); // ���¡ onClick �ҡ�ա�ü١���
        }

        // ��Ǩ�ͺ����� Image ���� SpriteRenderer ��������¹�������Դ panel
        if (clickedObject.GetComponent<Image>() || clickedObject.GetComponent<SpriteRenderer>())
        {
            isPanelOpen = true;
            panelColor.SetActive(true);
            CP.SetUIElement(clickedObject);
            Debug.Log($"Panel opened for {clickedObject.name}");
        }
        
    }

    private void LoadAllIcons()
    {
        if (iconParent != null)
        {
            Transform[] allChildren = iconParent.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in iconParent)
            {
                if (child.gameObject.CompareTag("SelectedIcon"))
                {
                    uiObject.Add(child.gameObject);
                }
            }
        }
    }

    private void InitializePresetUI()
    {
        foreach (var preset in colorPresetUI)
        {
            preset.gameObject.SetActive(false);
        }
    }

    public void SaveColor()
    {
        // ��Ǩ�ͺ��� selectedObject �դ������� Image component �������
        if (selectedObject == null)
        {
            return;
        }

        Image uiImage = selectedObject.GetComponent<Image>();
        if (uiImage == null)
        {
            return;
        }

        Color selectedColor = uiImage.color;

        // ��Ǩ�ͺ����շ�����͡������������������������
        for (int i = 0; i < presetCount; i++)
        {
            if (colorPresets[i] == selectedColor)
            {
                //Debug.Log("Color already exists in presets.");
                return; 
            }
        }

        if (presetCount >= colorPresets.Length)
        {
            for (int i = 1; i < colorPresets.Length; i++)
            {
                colorPresets[i - 1] = colorPresets[i];
            }

            colorPresets[colorPresets.Length - 1] = selectedColor;
        }
        else
        {
            colorPresets[presetCount] = selectedColor;
            presetCount++;
        }

        //Debug.Log("Color saved: " + selectedColor);
        UpdateColorPresetUI();
    }

    public void UpdateColorPresetUI()
    {
        for (int i = 0; i < colorPresetUI.Length; i++)
        {
            if (i < colorPresets.Length)
            {
                // �ʴ��շ��١�ѹ�֡� colorPresets
                colorPresetUI[i].gameObject.SetActive(true);
                colorPresetUI[i].color = colorPresets[i];
            }
            else
            {
                // ��駤�� Image ����ѧ�������������բ�� (�������������)
                //colorPresetUI[i].gameObject.SetActive(true); // �ѧ���ʴ� Image
                colorPresetUI[i].color = Color.white; // ������բ��
            }
        }
    }

    // �ѧ��ѹ�Դ�����ͺ�ͧ GameObject ���� ¡��鹵�Ƿ��١��
    private void DisableSelectedIconButtons(GameObject clickedObject)
    {
        foreach (var buttonObject in uiObject)
        {
            if (buttonObject != clickedObject)
            {
                buttonObject.gameObject.SetActive(false);
                //continue; // ������Ƿ��١��
            }

        }
    }

    // �ѧ��ѹ�Դ�����ͺ�ͧ GameObject ������
    private void EnableSelectedIconButtons()
    {
        foreach (var buttonObject in uiObject)
        {
            buttonObject.gameObject.SetActive(true);
        }
    }

    // �ѧ��ѹ�Դ Panel ��Ф׹��ҡ����ͺ
    public void ClosePanels()
    {
        if (checkObject)
        {
            SaveColor();
        }

        panelColor.SetActive(false);
        isPanelOpen = false;
        checkObject = false;

        // �Դ�����ͺ�ͧ GameObject ������
        EnableSelectedIconButtons();
    }

    public void UpdateUI(Color color)
    {
        Color.RGBToHSV(color, out CP.currentHue, out CP.currentSaturation, out CP.currentVal);
        CP.hueSlider.value = CP.currentHue;

        CP.hexInputFeild.text = ColorUtility.ToHtmlStringRGB(color);
        CP.rInput.text = Mathf.RoundToInt(color.r * 255).ToString();
        CP.gInput.text = Mathf.RoundToInt(color.g * 255).ToString();
        CP.bInput.text = Mathf.RoundToInt(color.b * 255).ToString();

        SVImage sviImage = FindObjectOfType<SVImage>();
        if (sviImage != null)
        {
            RectTransform rectTransform = sviImage.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                float deltaX = rectTransform.sizeDelta.x * 0.5f;
                float deltaY = rectTransform.sizeDelta.y * 0.5f;
                float pickerX = (CP.currentSaturation * rectTransform.sizeDelta.x) - deltaX;
                float pickerY = (CP.currentVal * rectTransform.sizeDelta.y) - deltaY;
                sviImage.UpdatePickerPosition(new Vector2(pickerX, pickerY));
            }
        }
        CP.outputImage.color = color;

    }
}
