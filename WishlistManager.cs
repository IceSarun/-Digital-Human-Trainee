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

    public Sprite starOn, starOff; // รูปดาว เปิด/ปิด
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
            Button[] allButtons = buttonParent.GetComponentsInChildren<Button>(true); // หา Button ทุกตัว
            foreach (var button in allButtons)
            {
                if (button.CompareTag("StarIcon"))
                {
                    showButtons.Add(button.gameObject);
                    // เช็คว่าปุ่มนี้อยู่ใน starButtons หรือไม่
                    bool isStarred = starButtons.Exists(obj => obj.name == button.name);

                    // เรียก SetupStarButton พร้อมส่งค่า isStarred
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
        // ตรวจหาหรือสร้างปุ่มรูปดาว (Image)
        Transform starTransform = mainButton.transform.Find("StarButton");
        Image starImage;
        Button starButton;

        if (starTransform == null)
        {
            // ถ้ายังไม่มี ให้สร้างใหม่
            GameObject starObj = new GameObject("StarButton");
            starObj.transform.SetParent(mainButton.transform, false);
            starObj.transform.localScale = Vector3.one;

            // ตั้งค่า RectTransform สำหรับตำแหน่งปุ่มดาว
            RectTransform rectTransform = starObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(1, 1);  // ชิดมุมขวาบน
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(1, 1);
            rectTransform.anchoredPosition = new Vector2(-6, -6); // ขยับให้อยู่ภายในปุ่มหลัก
            rectTransform.sizeDelta = new Vector2(20, 20); // กำหนดขนาดปุ่มดาว

            // เพิ่ม Image และ Button
            starImage = starObj.AddComponent<Image>();
            starImage.color = new Color(0,250,255);
            starButton = starObj.AddComponent<Button>();

            // ตั้งค่า Sprite เริ่มต้น
            starImage.sprite = isStarred ? starOn : starOff;

            // เชื่อมฟังก์ชัน ToggleStar ให้แน่ใจว่ามันถูกเพิ่มจริง ๆ
            starButton.onClick.AddListener(() =>
            {
                //Debug.Log("Star button clicked on: " + mainButton.name);
                ToggleStar(mainButton.gameObject, starImage);
            });
            //Debug.Log("Added new star button to: " + mainButton.name);
        }
        else
        {
            // ถ้ามีอยู่แล้ว ให้ใช้ตัวเดิม
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

        // สำคัญ: อัปเดตไอคอนให้แน่ใจว่าแสดงสถานะล่าสุด
        starImage.sprite = isStarred ? starOn : starOff;
    }


    public void ToggleStar(GameObject parentButton, Image starIcon)
    {
        // หาปุ่มที่มีชื่อเดียวกันใน starButtons และ showButtons
        GameObject starButton = starButtons.Find(obj => obj.name == parentButton.name);
        GameObject showButton = showButtons.Find(obj => obj.name == parentButton.name);

        bool isStarred = starButton != null && starButton.activeSelf; // ปุ่ม star เปิดอยู่ไหม?

        if (isStarred)
        {
            // ซ่อนปุ่มและลบออกจากรายการ
            if (starButton != null)
            {
                starButtons.Remove(starButton);
                Destroy(starButton);
                //Debug.Log("Removed star button: " + parentButton.name);
            }
            // เปลี่ยนไอคอนดาวของปุ่มหลักให้เป็น off
            UpdateStarIcon(parentButton, starOff);
            //Debug.Log("Unstarred: " + parentButton.name);
        }
        else
        {
            // ถ้ายังไม่มีปุ่ม starButton ให้สร้างใหม่
            if (starButton == null)
            {
                GameObject newStarButton = Instantiate(parentButton, starButtonParent);
                newStarButton.name = parentButton.name;
                starButtons.Add(newStarButton); // เพิ่มลงในรายการที่ติดดาว

                // หาไอคอนดาวและเปลี่ยนเป็นรูปดาวเต็มดวง
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

            // เปลี่ยนไอคอนดาวของปุ่มหลักให้เป็น on
            UpdateStarIcon(parentButton, starOn);
            //Debug.Log("Starred: " + parentButton.name);
        }
        SaveStarredThemes();
        UpdateApplyButtonInStarPanel();
    }

    void UpdateStarIcon(GameObject button, Sprite newSprite)
    {
        // เปลี่ยนไอคอนดาวในปุ่มหลัก
        Image starIcon = button.transform.Find("StarButton")?.GetComponent<Image>();
        if (starIcon) starIcon.sprite = newSprite;

        // เปลี่ยนไอคอนดาวในปุ่มที่อยู่ใน starButtons (ถ้ามี)
        GameObject starButton = starButtons.Find(obj => obj.name == button.name);
        if (starButton)
        {
            Image starButtonIcon = starButton.transform.Find("StarButton")?.GetComponent<Image>();
            if (starButtonIcon) starButtonIcon.sprite = newSprite;
        }

        // เปลี่ยนไอคอนดาวในปุ่มที่อยู่ใน showButtons (ถ้ามี)
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
        Debug.Log("บันทึกธีมที่ติดดาวลงไฟล์ JSON แล้ว: " + saveFilePath);
    }

    private void LoadStarredThemes()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            StarredThemesData data = JsonUtility.FromJson<StarredThemesData>(json);

            Debug.Log("โหลดข้อมูลจาก JSON: " + json);
            // ตรวจสอบว่าข้อมูล JSON มีธีมที่ติดดาวหรือไม่
            if (data == null || data.starredThemeNames == null || data.starredThemeNames.Count == 0)
            {
                Debug.LogWarning("ไม่มีธีมที่ติดดาวใน JSON");
                return;
            }
            Debug.Log("จำนวนธีมที่ติดดาว: " + data.starredThemeNames.Count);

            foreach (string themeName in data.starredThemeNames)
            {
                GameObject originalButton = FindButtonByName(themeName);

                if (originalButton != null)
                {
                    // สร้างปุ่มใหม่ใน starButtonParent
                    GameObject newStarButton = Instantiate(originalButton, starButtonParent);
                    newStarButton.name = originalButton.name;
                    starButtons.Add(newStarButton);

                    // อัปเดตปุ่มดาว
                    Button newButtonComponent = newStarButton.GetComponent<Button>();
                    if (newButtonComponent != null)
                    {
                        SetupStarButton(newButtonComponent, true);
                    }

                    newStarButton.SetActive(true);
                    UpdateStarIcon(originalButton, starOn);
                }

            }

            Debug.Log("โหลดธีมที่ติดดาวเสร็จสิ้น");
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
