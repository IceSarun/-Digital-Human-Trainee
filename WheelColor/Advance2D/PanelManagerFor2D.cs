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
    private bool isPanelOpen = false; // สถานะของ Panel
    private bool checkObject = false; // ตรวจสอบวัตถุที่กด
    private GameObject selectedObject;

    [Header("Color Presets")]
    //Preset Color
    //private Color selectedColor; // เพิ่มตัวแปรสำหรับจดจำค่าสี
    public Color[] colorPresets = new Color[10]; 
    private int presetCount = 0; // จำนวนสีที่บันทึกใน Preset
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

    // ฟังก์ชันจัดการคลิก GameObject
    private void HandleGameObjectClick(GameObject clickedObject)
    {
        // ตรวจสอบว่า GameObject มี Tag "SelectedIcon" หรือไม่
        if (!clickedObject.CompareTag("SelectedIcon"))
        {
            Debug.Log($"{clickedObject.name} does not have the 'SelectedIcon' tag.");
            return;
        }

        // ปิดการโต้ตอบของ GameObject อื่นๆ
        DisableSelectedIconButtons(clickedObject);
        checkObject = true;

        // ตรวจสอบว่าเป็นปุ่ม (Button) หรือไม่
        Button button = clickedObject.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.Invoke(); // เรียก onClick หากมีการผูกไว้
        }

        // ตรวจสอบว่ามี Image หรือ SpriteRenderer เพื่อเปลี่ยนสีหรือเปิด panel
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
        // ตรวจสอบว่า selectedObject มีค่าและมี Image component หรือไม่
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

        // ตรวจสอบว่าสีที่เลือกอยู่ในอาร์เรย์แล้วหรือไม่
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
                // แสดงสีที่ถูกบันทึกใน colorPresets
                colorPresetUI[i].gameObject.SetActive(true);
                colorPresetUI[i].color = colorPresets[i];
            }
            else
            {
                // ตั้งค่า Image ที่ยังไม่มีสีให้เป็นสีขาว (หรือสีเริ่มต้น)
                //colorPresetUI[i].gameObject.SetActive(true); // ยังคงแสดง Image
                colorPresetUI[i].color = Color.white; // ตั้งเป็นสีขาว
            }
        }
    }

    // ฟังก์ชันปิดการโต้ตอบของ GameObject อื่นๆ ยกเว้นตัวที่ถูกกด
    private void DisableSelectedIconButtons(GameObject clickedObject)
    {
        foreach (var buttonObject in uiObject)
        {
            if (buttonObject != clickedObject)
            {
                buttonObject.gameObject.SetActive(false);
                //continue; // ข้ามตัวที่ถูกกด
            }

        }
    }

    // ฟังก์ชันเปิดการโต้ตอบของ GameObject ทั้งหมด
    private void EnableSelectedIconButtons()
    {
        foreach (var buttonObject in uiObject)
        {
            buttonObject.gameObject.SetActive(true);
        }
    }

    // ฟังก์ชันปิด Panel และคืนค่าการโต้ตอบ
    public void ClosePanels()
    {
        if (checkObject)
        {
            SaveColor();
        }

        panelColor.SetActive(false);
        isPanelOpen = false;
        checkObject = false;

        // เปิดการโต้ตอบของ GameObject ทั้งหมด
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
