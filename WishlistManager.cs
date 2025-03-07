using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;

public class WishlistManager : MonoBehaviour
{
    [System.Serializable]
    public class StarredThemesData
    {
        public List<string> starredThemeNames = new List<string>();
    }

    public Sprite starOn, starOff; // �ٻ��� �Դ/�Դ
    public List<GameObject> starButtons = new List<GameObject>();
    public List<GameObject> showButtons = new List<GameObject>();
    public Transform buttonParent;
    public Transform starButtonParent;
    private GameObject allHideObjectWhenNoStarTheme;

    private string saveFilePath;

    private void Awake()
    {
        saveFilePath = Application.persistentDataPath + "/starredThemes.json";
        //Set icon in theme
        if (buttonParent != null)
        {
            Button[] allButtons = buttonParent.GetComponentsInChildren<Button>(true); // �� Button �ء���
            foreach (var button in allButtons)
            {
                if (button.CompareTag("StarIcon"))
                {
                    showButtons.Add(button.gameObject);
                    // ����һ����������� starButtons �������
                    bool isStarred = starButtons.Exists(obj => obj.name == button.name);

                    // ���¡ SetupStarButton ������觤�� isStarred
                    SetupStarButton(button, isStarred);
                }
               
            }
        }
    }
    private void Start()
    {
        LoadStarredThemes();
    }

    public void SetApplyButton(GameObject button)
    {
        allHideObjectWhenNoStarTheme = button;
        Debug.Log("SetApplyButton: " + (allHideObjectWhenNoStarTheme != null ? allHideObjectWhenNoStarTheme.name : "NULL"));
        allHideObjectWhenNoStarTheme.SetActive(false);
    }

    public void SetupStarButton(Button mainButton, bool isStarred)
    {
        // ��Ǩ���������ҧ�����ٻ��� (Image)
        Transform starTransform = mainButton.transform.Find("StarButton");
        Image starImage;
        Button starButton;

        if (starTransform == null)
        {
            // ����ѧ����� ������ҧ����
            GameObject starObj = new GameObject("StarButton");
            starObj.transform.SetParent(mainButton.transform, false);
            starObj.transform.localScale = Vector3.one;

            // ��駤�� RectTransform ����Ѻ���˹觻������
            RectTransform rectTransform = starObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(1, 1);  // �Դ�����Һ�
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(1, 1);
            rectTransform.anchoredPosition = new Vector2(-6, -6); // ��Ѻ����������㹻�����ѡ
            rectTransform.sizeDelta = new Vector2(20, 20); // ��˹���Ҵ�������

            // ���� Image ��� Button
            starImage = starObj.AddComponent<Image>();
            starImage.color = new Color(0,250,255);
            starButton = starObj.AddComponent<Button>();

            // ��駤�� Sprite �������
            starImage.sprite = isStarred ? starOn : starOff;

            // ������ѧ��ѹ ToggleStar ����������ѹ�١������ԧ �
            starButton.onClick.AddListener(() =>
            {
                //Debug.Log("Star button clicked on: " + mainButton.name);
                ToggleStar(mainButton.gameObject, starImage);
            });
            //Debug.Log("Added new star button to: " + mainButton.name);
        }
        else
        {
            // ������������� ����������
            starImage = starTransform.GetComponent<Image>();
            starButton = starTransform.GetComponent<Button>();

            if (starButton != null)
            {
                starButton.onClick.RemoveAllListeners();
                starButton.onClick.AddListener(() =>
                {
                    //Debug.Log("Star button clicked (existing) on: " + mainButton.name);
                    ToggleStar(mainButton.gameObject, starImage);
                });
                //Debug.Log("Updated existing star button event on: " + mainButton.name);
            }
        }

        // �Ӥѭ: �ѻവ�ͤ͹����������ʴ�ʶҹ�����ش
        starImage.sprite = isStarred ? starOn : starOff;
    }


    public void ToggleStar(GameObject parentButton, Image starIcon)
    {
        // �һ�������ժ������ǡѹ� starButtons ��� showButtons
        GameObject starButton = starButtons.Find(obj => obj.name == parentButton.name);
        GameObject showButton = showButtons.Find(obj => obj.name == parentButton.name);

        bool isStarred = starButton != null && starButton.activeSelf; // ���� star �Դ�������?

        if (isStarred)
        {
            // ��͹�������ź�͡�ҡ��¡��
            if (starButton != null)
            {
                starButtons.Remove(starButton);
                Destroy(starButton);
                //Debug.Log("Removed star button: " + parentButton.name);
            }
            // ����¹�ͤ͹��Ǣͧ������ѡ����� off
            UpdateStarIcon(parentButton, starOff);
            //Debug.Log("Unstarred: " + parentButton.name);
        }
        else
        {
            // ����ѧ����ջ��� starButton ������ҧ����
            if (starButton == null)
            {
                GameObject newStarButton = Instantiate(parentButton, starButtonParent);
                newStarButton.name = parentButton.name;
                starButtons.Add(newStarButton); // ����ŧ���¡�÷��Դ���

                // ���ͤ͹����������¹���ٻ�������ǧ
                Button newButton = newStarButton.GetComponent<Button>();
                if (newButton != null)
                {
                    SetupStarButton(newButton, true);
                }
                newStarButton.SetActive(true);
            }
            else
            {
                starButton.SetActive(true);
            }

            // ����¹�ͤ͹��Ǣͧ������ѡ����� on
            UpdateStarIcon(parentButton, starOn);
            //Debug.Log("Starred: " + parentButton.name);
        }
        SaveStarredThemes();
        UpdateApplyButtonInStarPanel();
    }

    void UpdateStarIcon(GameObject button, Sprite newSprite)
    {
        // ����¹�ͤ͹���㹻�����ѡ
        Image starIcon = button.transform.Find("StarButton")?.GetComponent<Image>();
        if (starIcon) starIcon.sprite = newSprite;

        // ����¹�ͤ͹���㹻����������� starButtons (�����)
        GameObject starButton = starButtons.Find(obj => obj.name == button.name);
        if (starButton)
        {
            Image starButtonIcon = starButton.transform.Find("StarButton")?.GetComponent<Image>();
            if (starButtonIcon) starButtonIcon.sprite = newSprite;
        }

        // ����¹�ͤ͹���㹻����������� showButtons (�����)
        GameObject showButton = showButtons.Find(obj => obj.name == button.name);
        if (showButton)
        {
            Image showButtonIcon = showButton.transform.Find("StarButton")?.GetComponent<Image>();
            if (showButtonIcon) showButtonIcon.sprite = newSprite;
        }

    }

    void UpdateApplyButtonInStarPanel() {
        //Debug.Log("number" + starButtons.Count);
        if (starButtons.Count == 0) { 
            allHideObjectWhenNoStarTheme.gameObject.SetActive(false);
        }
        else
        {
            allHideObjectWhenNoStarTheme.gameObject.SetActive(true);
        }
    }

    private void SaveStarredThemes()
    {
        StarredThemesData data = new StarredThemesData();

        foreach (GameObject button in starButtons)
        {
            data.starredThemeNames.Add(button.name);
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log("�ѹ�֡������Դ���ŧ��� JSON ����: " + saveFilePath);
    }

    private void LoadStarredThemes()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            StarredThemesData data = JsonUtility.FromJson<StarredThemesData>(json);

            Debug.Log("��Ŵ�����Ũҡ JSON: " + json);
            // ��Ǩ�ͺ��Ң����� JSON �ո�����Դ����������
            if (data == null || data.starredThemeNames == null || data.starredThemeNames.Count == 0)
            {
                Debug.LogWarning("����ո�����Դ���� JSON");
                return;
            }
            Debug.Log("�ӹǹ������Դ���: " + data.starredThemeNames.Count);

            foreach (string themeName in data.starredThemeNames)
            {
                GameObject originalButton = FindButtonByName(themeName);

                if (originalButton != null)
                {
                    // ���ҧ��������� starButtonParent
                    GameObject newStarButton = Instantiate(originalButton, starButtonParent);
                    newStarButton.name = originalButton.name;
                    starButtons.Add(newStarButton);

                    // �ѻവ�������
                    Button newButtonComponent = newStarButton.GetComponent<Button>();
                    if (newButtonComponent != null)
                    {
                        SetupStarButton(newButtonComponent, true);
                    }

                    newStarButton.SetActive(true);
                    UpdateStarIcon(originalButton, starOn);
                }

            }

            Debug.Log("��Ŵ������Դ����������");
            UpdateApplyButtonInStarPanel();
        }
        
    }

    private GameObject FindButtonByName(string name)
    {
        foreach (GameObject button in showButtons)
        {
            //Debug.Log("Find: " + name + " in "+ button.name);

            if (button.name == name)
            {
                return button;
            }
        }
        return null;
    }

}
