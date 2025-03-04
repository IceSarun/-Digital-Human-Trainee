using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems; // ������Ѻ��Ǩ�ͺ��ä�ԡ UI
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
    private bool isPanelOpen = false; // ʶҹТͧ Panel
    private bool checkObject = false;

    //Preset Color
    private Color selectedColor; // �������������Ѻ���Ӥ����
    public Color[] colorPresets = new Color[10]; // �������٧�ش 10 ��
    private int presetCount = 0; // �ӹǹ�շ��ѹ�֡� Preset
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

            if (isPanelOpen) return; // �ҡ Panel �Դ���� ��ش��÷ӧҹ�ͧ���ŷ�

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

                // ��Ǩ�ͺ��Ҥ�ԡ�� UI �������
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
                        // �ʴ� Panel ��С�˹�ʶҹ�
                        panelColor.SetActive(true);
                        panelTexture.SetActive(true);
                        isPanelOpen = true;

                        // ��駤�� Object
                        CC.SetObject(meshRenderer);
                        TF.SetObject(clickedObject);
                        checkObject = true;

                    }
                }
            }
        }
        
    }

    // �ѧ��ѹ����Ѻ�Դ Panel
    public void ClosePanels()
    {
        //�Դ���� reset 
        // �Դ Panel
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

        // �Դ���ŷ�ͧ selection
        if (selection != null)
        {
            selection.gameObject.GetComponent<Outline>().enabled = false;
            selection = null;
        }

        // �Դ���ŷ�ͧ highLight
        if (highLight != null)
        {
            highLight.gameObject.GetComponent<Outline>().enabled = false;
            highLight = null;
        }
    }

    public void SaveColor()
    {
        // ��Ǩ�ͺ��� selectedObject �դ������� Renderer component �������
        if (meshRenderer == null)
        {
            return;
        }

        Renderer objectRenderer = meshRenderer.GetComponent<Renderer>();
        if (objectRenderer == null)
        {
            return;
        }

        // �֧�ըҡ Material �ͧ�ѵ��
        selectedColor = objectRenderer.material.color;

        // ��Ǩ�ͺ����շ�����͡������������������������
        for (int i = 0; i < presetCount; i++)
        {
            if (colorPresets[i] == selectedColor)
            {
                // Debug.Log("Color already exists in presets.");
                return; // �͡�ҡ�ѧ��ѹ�ҡ���ի��
            }
        }

        // ��Ҩӹǹ���Թ��Ҵ�ͧ�������� �������͹��ҷ�����价ҧ����
        if (presetCount >= colorPresets.Length)
        {
            for (int i = 1; i < colorPresets.Length; i++)
            {
                colorPresets[i - 1] = colorPresets[i];
            }

            // ����������㹵��˹��ش���¢ͧ��������
            colorPresets[colorPresets.Length - 1] = selectedColor;
        }
        else
        {
            // ����ѧ����Թ��Ҵ�ͧ�������� ���������㹵��˹觶Ѵ�
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
                // �ʴ��շ��١�ѹ�֡� colorPresets
                colorPresetUI[i].gameObject.SetActive(true);
                colorPresetUI[i].color = colorPresets[i];
            }
            else
            {
                // ��͹ Image �������������������
                colorPresetUI[i].gameObject.SetActive(false);
            }
        }

        // �ѻവ�բͧ�ѵ�� 3D (����ա�����͡�ѵ��)
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
        // ��Ǩ�ͺ��� selectedObject �դ������� Renderer component �������
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
        
        // ��Ǩ�ͺ����շ�����͡������������������������
        for (int i = 0; i < presetCount; i++)
        {
            if (imageTexture[i] != null && imageTexture[i] == currentTexture)
            {
                // Debug.Log("Color already exists in presets.");
                return; // �͡�ҡ�ѧ��ѹ�ҡ���ի��
            }
        }

        // ��Ҩӹǹ Texture �Թ��Ҵ�������� �������͹��ҷ�����价ҧ����
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
                // �ŧ Texture2D �� Sprite
                Sprite newSprite = Sprite.Create(
                    texture2D,
                    new Rect(0, 0, texture2D.width, texture2D.height),
                    new Vector2(0.5f, 0.5f)
                );

                // �ʴ� Sprite � Image
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