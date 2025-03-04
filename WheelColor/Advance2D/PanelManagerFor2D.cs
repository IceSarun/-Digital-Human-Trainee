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
    private bool isPanelOpen = false; // ʶҹТͧ Panel
    private bool checkObject = false; // ��Ǩ�ͺ�ѵ�ط�衴
    private GameObject selectedObject;
    //private bool checkSettingButton = false; // ��Ǩ�ͺ�ѵط�衴����� setting ���
    
    //Preset Color
    private Color selectedColor; // �������������Ѻ���Ӥ����
    public Color[] colorPresets = new Color[10]; // �������٧�ش 10 ��
    private int presetCount = 0; // �ӹǹ�շ��ѹ�֡� Preset
    public Image[] colorPresetUI = new Image[10];


    private void Start()
    {
        panelColor.SetActive(false);
        for (int i = 0; i< colorPresetUI.Length ;i++) {
            colorPresetUI[i].gameObject.SetActive(false);
        }

        if (iconParent != null)
        {
            // �֧ Transform �ͧ�١������� iconParent
            Transform[] allChildren = iconParent.GetComponentsInChildren<Transform>(true);
            foreach (var child in allChildren)
            {
                // ��Ǩ�ͺ��� GameObject ������� "SelectedIcon" �������
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

        selectedColor = uiImage.color;

        // ��Ǩ�ͺ����շ�����͡������������������������
        for (int i = 0; i < presetCount; i++)
        {
            if (colorPresets[i] == selectedColor)
            {
                //Debug.Log("Color already exists in presets.");
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

        //Debug.Log("Color saved: " + selectedColor);
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
            if (buttonObject == clickedObject)
            {
                continue; // ������Ƿ��١��
            }

            buttonObject.gameObject.SetActive(false);

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
}
