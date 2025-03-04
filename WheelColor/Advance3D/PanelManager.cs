using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems; // ใช้สำหรับตรวจสอบการคลิก UI
using UnityEngine.UI;

public class PanelManager : MonoBehaviour
{
    public GameObject panelColor;
    public GameObject panelTexture;
    public ColorPickAdvance CC;
    public FileTextureAdvance TF;

    //Editor
    public EditorManager editMode;

    private MeshRenderer meshRenderer;
    private Transform highLight;
    private Transform selection;
    private RaycastHit hit;
    private bool isPanelOpen = false; // สถานะของ Panel
    private bool checkObject = false;

    //Preset Color
    private Color selectedColor; // เพิ่มตัวแปรสำหรับจดจำค่าสี
    public Color[] colorPresets = new Color[10]; // เก็บสีได้สูงสุด 10 สี
    private int presetCount = 0; // จำนวนสีที่บันทึกใน Preset
    public Image[] colorPresetUI = new Image[10];

    //Preset Image
    public Texture[] imageTexture = new Texture[5];
    private int presetImageCount = 0;
    public Image[] texturePresetUI = new Image[5];


    private void Start()
    {
        panelColor.SetActive(false);
        panelTexture.SetActive(false);
        for (int i = 0; i<texturePresetUI.Length ;i++) {
            texturePresetUI[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < colorPresetUI.Length; i++)
        {
            colorPresetUI[i].gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (editMode.checkColor3DModePanel == true) {
            if (highLight != null)
            {
                highLight.gameObject.GetComponent<Outline>().enabled = false;
                highLight = null;
            }

            if (isPanelOpen) return; // หาก Panel เปิดอยู่ หยุดการทำงานของไฮไลท์

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (!EventSystem.current.IsPointerOverGameObject() && Physics.Raycast(ray, out hit))
            {
                highLight = hit.transform;
                if (highLight.CompareTag("Selectable") && highLight != selection)
                {
                    if (highLight.gameObject.GetComponent<Outline>() != null)
                    {
                        highLight.gameObject.GetComponent<Outline>().enabled = true;
                    }
                    else
                    {
                        Outline outline = highLight.gameObject.AddComponent<Outline>();
                        outline.enabled = true;
                        highLight.gameObject.GetComponent<Outline>().OutlineColor = Color.red;
                        highLight.gameObject.GetComponent<Outline>().OutlineWidth = 7.0f;
                    }
                }
                else
                {
                    highLight = null;
                }

                // ตรวจสอบว่าคลิกบน UI หรือไม่
                if (Input.GetMouseButtonDown(0))
                {
                    if (highLight)
                    {
                        if (selection != null)
                        {
                            selection.gameObject.GetComponent<Outline>().enabled = false;
                        }
                        selection = hit.transform;
                        selection.gameObject.GetComponent<Outline>().enabled = true;
                        highLight = null;
                    }
                    else
                    {
                        if (selection)
                        {
                            selection.gameObject.GetComponent<Outline>().enabled = false;
                            selection = null;
                        }
                    }

                    GameObject clickedObject = hit.collider.gameObject;
                    meshRenderer = clickedObject.GetComponent<MeshRenderer>();

                    if (meshRenderer != null && !isPanelOpen)
                    {
                        // แสดง Panel และกำหนดสถานะ
                        panelColor.SetActive(true);
                        panelTexture.SetActive(true);
                        isPanelOpen = true;

                        // ตั้งค่า Object
                        CC.SetObject(meshRenderer);
                        TF.SetObject(clickedObject);
                        checkObject = true;

                    }
                }
            }
        }
        
    }

    // ฟังก์ชันสำหรับปิด Panel
    public void ClosePanels()
    {
        //เปิดปุ่ม reset 
        // ปิด Panel
        //Debug.Log(checkObject.ToString());
        if (checkObject)
        {
            SaveColor();
        }
        if (TF.getCheckUpload()) {

            SaveTexture();
        }

        panelColor.SetActive(false);
        panelTexture.SetActive(false);
        isPanelOpen = false;
        checkObject = false;
        TF.ResetCheckDoneButton();

        // ปิดไฮไลท์ของ selection
        if (selection != null)
        {
            selection.gameObject.GetComponent<Outline>().enabled = false;
            selection = null;
        }

        // ปิดไฮไลท์ของ highLight
        if (highLight != null)
        {
            highLight.gameObject.GetComponent<Outline>().enabled = false;
            highLight = null;
        }
    }

    public void SaveColor()
    {
        // ตรวจสอบว่า selectedObject มีค่าและมี Renderer component หรือไม่
        if (meshRenderer == null)
        {
            return;
        }

        Renderer objectRenderer = meshRenderer.GetComponent<Renderer>();
        if (objectRenderer == null)
        {
            return;
        }

        // ดึงสีจาก Material ของวัตถุ
        selectedColor = objectRenderer.material.color;

        // ตรวจสอบว่าสีที่เลือกอยู่ในอาร์เรย์แล้วหรือไม่
        for (int i = 0; i < presetCount; i++)
        {
            if (colorPresets[i] == selectedColor)
            {
                // Debug.Log("Color already exists in presets.");
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

        Debug.Log("Color saved: " + selectedColor);
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
                // ซ่อน Image หรือรีเซ็ตเป็นสีเริ่มต้น
                colorPresetUI[i].gameObject.SetActive(false);
            }
        }

        // อัปเดตสีของวัตถุ 3D (ถ้ามีการเลือกวัตถุ)
        if (meshRenderer != null)
        {
            Renderer objectRenderer = meshRenderer.GetComponent<Renderer>();
            if (objectRenderer != null)
            {
                objectRenderer.material.color = selectedColor;
            }
        }
    }

    public void SaveTexture() {
        // ตรวจสอบว่า selectedObject มีค่าและมี Renderer component หรือไม่
        if (meshRenderer == null)
        {
            return;
        }

        Renderer objectRenderer = meshRenderer.GetComponent<Renderer>();
        if (objectRenderer == null)
        {
            return;
        }

        Texture currentTexture = objectRenderer.material.mainTexture;
        
        // ตรวจสอบว่าสีที่เลือกอยู่ในอาร์เรย์แล้วหรือไม่
        for (int i = 0; i < presetCount; i++)
        {
            if (imageTexture[i] != null && imageTexture[i] == currentTexture)
            {
                // Debug.Log("Color already exists in presets.");
                return; // ออกจากฟังก์ชันหากพบสีซ้ำ
            }
        }

        // ถ้าจำนวน Texture เกินขนาดอาร์เรย์ ให้เลื่อนค่าทั้งหมดไปทางซ้าย
        if (presetImageCount >= imageTexture.Length)
        {
            for (int i = 1; i < imageTexture.Length; i++)
            {
                imageTexture[i - 1] = imageTexture[i];
            }

            imageTexture[imageTexture.Length - 1] = currentTexture;
        }
        else
        {
            imageTexture[presetImageCount]= currentTexture;
            presetImageCount++;
        }

        Debug.Log("Texture saved: " + currentTexture.name);
        UpdateTexturePreset();

    }

    public void UpdateTexturePreset()
    {
        for (int i = 0; i < texturePresetUI.Length; i++)
        {
            if (i < presetImageCount && imageTexture[i] is Texture2D texture2D)
            {
                // แปลง Texture2D เป็น Sprite
                Sprite newSprite = Sprite.Create(
                    texture2D,
                    new Rect(0, 0, texture2D.width, texture2D.height),
                    new Vector2(0.5f, 0.5f)
                );

                // แสดง Sprite ใน Image
                texturePresetUI[i].gameObject.SetActive(true);
                texturePresetUI[i].sprite = newSprite;
            }
            
        }

    }


    public void DoneButton()
    {
        panelTexture.SetActive(false);

    }
}