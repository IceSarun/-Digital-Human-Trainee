using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PanelManagerFor2D : MonoBehaviour
{
    //Find button in scene 
    public List<GameObject> uiObject = new List<GameObject>();
    public Transform iconParent;

    //Edit
    public EditorManager editerMode;

    public GameObject panelColor;
    //public ColorPickButtonAdvance CU;
    public ColorPick CP;
    private bool isPanelOpen = false; // สถานะของ Panel
    private bool checkObject = false; // ตรวจสอบวัตถุที่กด
    private GameObject selectedObject;
    //private bool checkSettingButton = false; // ตรวจสอบวัตุที่กดว่าใช่ setting ไหม
    
    //Preset Color
    private Color selectedColor; // เพิ่มตัวแปรสำหรับจดจำค่าสี
    public Color[] colorPresets = new Color[10]; // เก็บสีได้สูงสุด 10 สี
    private int presetCount = 0; // จำนวนสีที่บันทึกใน Preset
    public Image[] colorPresetUI = new Image[10];


    private void Start()
    {
        panelColor.SetActive(false);
        for (int i = 0; i< colorPresetUI.Length ;i++) {
            colorPresetUI[i].gameObject.SetActive(false);
        }

        if (iconParent != null)
        {
            // ดึง Transform ของลูกทั้งหมดใน iconParent
            Transform[] allChildren = iconParent.GetComponentsInChildren<Transform>(true);
            foreach (var child in allChildren)
            {
                // ตรวจสอบว่า GameObject นั้นมีแท็ก "SelectedIcon" หรือไม่
                if (child.gameObject.CompareTag("SelectedIcon"))
                {
                    uiObject.Add(child.gameObject);
                }
            }
        }
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject() && !isPanelOpen && editerMode.checkColorModePanel)
            {
                selectedObject = EventSystem.current.currentSelectedGameObject;

                if (selectedObject != null)
                {
                    HandleGameObjectClick(selectedObject);
                }
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
        Image image = clickedObject.GetComponent<Image>();
        SpriteRenderer spriteRenderer = clickedObject.GetComponent<SpriteRenderer>();

        if (image != null || spriteRenderer != null)
        {
            isPanelOpen = true;
            panelColor.SetActive(true);
            CP.SetUIElement(clickedObject);
            Debug.Log($"Panel opened for {clickedObject.name}");
        }
        else
        {
            Debug.Log($"{clickedObject.name} does not support panel interaction.");
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

        selectedColor = uiImage.color;

        // ตรวจสอบว่าสีที่เลือกอยู่ในอาร์เรย์แล้วหรือไม่
        for (int i = 0; i < presetCount; i++)
        {
            if (colorPresets[i] == selectedColor)
            {
                //Debug.Log("Color already exists in presets.");
                return; // ออกจากฟังก์ชันหากพบสีซ้ำ
            }
        }

        // ถ้าจำนวนสีเกินขนาดของอาร์เรย์ ให้เลื่อนค่าทั้งหมดไปทางซ้าย
        if (presetCount >= colorPresets.Length)
        {
            for (int i = 1; i < colorPresets.Length; i++)
            {
                colorPresets[i - 1] = colorPresets[i];
            }

            // เพิ่มสีใหม่ในตำแหน่งสุดท้ายของอาร์เรย์
            colorPresets[colorPresets.Length - 1] = selectedColor;
        }
        else
        {
            // ถ้ายังไม่เกินขนาดของอาร์เรย์ ให้เพิ่มสีในตำแหน่งถัดไป
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
            if (i < presetCount)
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
            if (buttonObject == clickedObject)
            {
                continue; // ข้ามตัวที่ถูกกด
            }

            buttonObject.gameObject.SetActive(false);

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
}
