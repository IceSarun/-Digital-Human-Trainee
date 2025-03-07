using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PanelManager : MonoBehaviour
{
    public GameObject panelColor;
    public GameObject panelTexture;
    public ColorPickAdvance CC;
    public FileTextureAdvance TF;
    public EditorManager editMode;

    private MeshRenderer meshRenderer;
    private Transform highLight;
    private Transform selection;
    private RaycastHit hit;
    private bool isPanelOpen = false;
    private bool checkObject = false;

    // Preset Color
    private Color selectedColor;
    private Color[] colorPresets = new Color[8];
    public Image[] colorPresetUI = new Image[8];
    private int presetCount = 0;

    // Preset Image
    private Texture[] imageTexture = new Texture[5];
    public Image[] texturePresetUI = new Image[5];
    private int presetImageCount = 0;

    private void Start()
    {
        panelColor.SetActive(false);
        panelTexture.SetActive(false);
        HidePresetUI();
    }

    private void HidePresetUI()
    {
        foreach (var ui in texturePresetUI) ui.gameObject.SetActive(false);
        foreach (var ui in colorPresetUI) ui.gameObject.SetActive(false);
    }

    public void SetObjectColor(MeshRenderer selectedRenderer)
    {
        CC.SetObject(selectedRenderer);
    }

    private void Update()
    {
        if (!editMode.checkColor3DModePanel) return;

        ResetHighlight();
        if (isPanelOpen) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!EventSystem.current.IsPointerOverGameObject() && Physics.Raycast(ray, out hit))
        {
            HandleHighlight(hit.transform);
            if (Input.GetMouseButtonDown(0)) HandleSelection();
        }
    }

    private void ResetHighlight()
    {
        if (highLight)
        {
            DisableOutline(highLight);
            highLight = null;
        }
    }

    private void HandleHighlight(Transform target)
    {
        if (target.CompareTag("Selectable") && target != selection)
        {
            EnableOutline(target);
            highLight = target;
        }
    }

    private void HandleSelection()
    {
        if (highLight)
        {
            if (selection) DisableOutline(selection);
            selection = highLight;
            EnableOutline(selection);
            highLight = null;
        }
        else
        {
            if (selection) DisableOutline(selection);
            selection = null;
        }

        meshRenderer = hit.collider.gameObject.GetComponent<MeshRenderer>();
        if (meshRenderer != null && !isPanelOpen)
        {
            OpenPanels(hit.collider.gameObject);
        }
    }

    private void OpenPanels(GameObject selectedObject)
    {
        panelColor.SetActive(true);
        panelTexture.SetActive(true);
        isPanelOpen = true;
        checkObject = true;

        CC.SetObject(meshRenderer);
        TF.SetObject(selectedObject);
    }

    public void ClosePanels()
    {
        if (checkObject) SaveColor();
        if (TF.getCheckUpload()) SaveTexture();

        panelColor.SetActive(false);
        panelTexture.SetActive(false);
        isPanelOpen = false;
        checkObject = false;
        TF.ResetCheckDoneButton();

        ResetHighlight();
        if (selection) DisableOutline(selection);
        selection = null;
    }

    public void SaveColor()
    {
        if (meshRenderer == null)
        {
            return;
        }

        Renderer objectRenderer = meshRenderer.GetComponent<Renderer>();
        if (objectRenderer == null)
        {
            return;
        }

        selectedColor = objectRenderer.material.color;

        for (int i = 0; i < presetCount; i++)
        {
            if (colorPresets[i] == selectedColor)
            {
                // Debug.Log("Color already exists in presets.");
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

        Debug.Log("Color saved: " + selectedColor);
        UpdateColorPresetUI();
    }

    private void UpdateColorPresetUI()
    {
        for (int i = 0; i < colorPresetUI.Length; i++)
        {
            if (i < colorPresets.Length)
            {
                colorPresetUI[i].gameObject.SetActive(true);
                colorPresetUI[i].color = colorPresets[i];
            }
            else
            {
                colorPresetUI[i].gameObject.SetActive(false);
            }
        }
    }

    public void SaveTexture()
    {
        if (meshRenderer == null) return;

        Renderer objectRenderer = meshRenderer.GetComponent<Renderer>();
        if (objectRenderer == null)
        {
            return;
        }
        Texture currentTexture = objectRenderer.material.mainTexture;
        for (int i = 0; i < presetCount; i++)
        {
            if (imageTexture[i] != null && imageTexture[i] == currentTexture)
            {
                // Debug.Log("Color already exists in presets.");
                return;
            }
        }
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
            imageTexture[presetImageCount] = currentTexture;
            presetImageCount++;
        }


        Debug.Log("Texture saved: " + currentTexture.name);
        UpdateTexturePreset();
    }

    private void UpdateTexturePreset()
    {
        for (int i = 0; i < texturePresetUI.Length; i++)
        {
            if (i < imageTexture.Length && imageTexture[i] is Texture2D texture2D)
            {
                texturePresetUI[i].gameObject.SetActive(true);
                texturePresetUI[i].sprite = Sprite.Create(
                    texture2D,
                    new Rect(0, 0, texture2D.width, texture2D.height),
                    new Vector2(0.5f, 0.5f)
                );
            }
            else
            {
                texturePresetUI[i].gameObject.SetActive(false);
            }
        }
    }

    public void ApplyColorChanges()
    {
        CC?.UpdateOutputImage();
    }

    public void DoneButton()
    {
        panelTexture.SetActive(false);
    }

    private void EnableOutline(Transform obj)
    {
        if (!obj.GetComponent<Outline>())
        {
            Outline outline = obj.gameObject.AddComponent<Outline>();
            outline.OutlineColor = Color.red;
            outline.OutlineWidth = 7.0f;
        }
        obj.GetComponent<Outline>().enabled = true;
    }

    private void DisableOutline(Transform obj)
    {
        if (obj.GetComponent<Outline>())
        {
            obj.GetComponent<Outline>().enabled = false;
        }
    }    
    public void UpdateUI(Color color)
    {
        Color.RGBToHSV(color, out CC.currentHue, out CC.currentSaturation, out CC.currentVal);

        CC.hexInputFeild.text = ColorUtility.ToHtmlStringRGB(color);
        CC.rInput.text = Mathf.RoundToInt(color.r * 255).ToString();
        CC.gInput.text = Mathf.RoundToInt(color.g * 255).ToString();
        CC.bInput.text = Mathf.RoundToInt(color.b * 255).ToString();
        CC.hueSlider.value = CC.currentHue;
        SVImageAdvance sviImage = FindObjectOfType<SVImageAdvance>();
        if (sviImage != null)
        {
            RectTransform rectTransform = sviImage.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                float deltaX = rectTransform.sizeDelta.x * 0.5f;
                float deltaY = rectTransform.sizeDelta.y * 0.5f;

                float pickerX = (CC.currentSaturation * rectTransform.sizeDelta.x) - deltaX;
                float pickerY = (CC.currentVal * rectTransform.sizeDelta.y) - deltaY;

                sviImage.UpdatePickerPosition(new Vector2(pickerX, pickerY));
            }
        }
        CC.outputImage.color = color;
    }
}
