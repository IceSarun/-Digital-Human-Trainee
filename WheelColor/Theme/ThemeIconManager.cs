using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using System.Security;
using System.Text.RegularExpressions;

public class ThemeIconManager : MonoBehaviour
{
    [System.Serializable]
    public class Theme
    {
        public string themeName; // ชื่อธีม
        public string resourcePath; // ชื่อโฟลเดอร์ใน Resources
        public Color textColor; // สีของข้อความ
        public Sprite[] allSprites; // โหลด Sprite อัตโนมัติ
        public Sprite backgroundSprite;
        public Color defaultIconColor = Color.white;

        public List<string> adjectives = new List<string>();
        public LockSystem lockItem;
    }

    public Image themeImageBG;
    public Image starThemeImageBG;

    [Header("Dynamic Elements")]
    public List<Image> allIcons = new List<Image>(); // ไอคอนทั้งหมด
    private List<TMP_Text> allTexts = new List<TMP_Text>(); // ข้อความทั้งหมดที่มี Tag = "TextInPanel"
    private List<Image> allIconsInEditPanel = new List<Image>(); // ไอคอนที่อยู่ในหน้า edit สิ่งต่างๆ
    public Theme selectedTheme; // ธีมที่เลือกอยู่
    private Theme preveiwTheme; // ธีมที่กำลังดู
    public Theme previousTheme; // เก็บธีมที่เลือก ไว้ดึงค่ากลับมาหลังจากผู้ใช้ preview
    private TMP_Text preveiw;

    [Header("Set Theme")]
    public List<Theme> allThemes = new List<Theme>();
    public Theme defaultTheme;

    [Header("Parent Containers (Optional)")]
    public Transform iconParent;
    public Transform textParent;

    public ThemeData themeData; // อ้างอิงไปยัง ScriptableObject
    //จดจำค่าสีเมื่อเป็นธีม default
    private Dictionary<string, Dictionary<string, Color>> themeIconColors = new Dictionary<string, Dictionary<string, Color>>();
    private string currentTheme = ""; // เก็บธีมปัจจุบัน

    [Header("Search Theme")]//search
    public TMP_InputField searchInput; // ช่องค้นหาธีม
    public Transform themeListParent; // พาเรนต์ของรายการธีมที่ค้นพบ

    public TMP_Text textInDoneButton;
    public TMP_Text textInStarDoneButton;

    public TMP_Text presentThemeNameShow;

    public TMP_Text selectedThemNameShow;
    public TMP_Text selectedStarThem; 
    
    public Button priceButton;
    public Button priceStarButton;

    public Money myMoney;

    public WishlistManager wishlistManager;
    public GameObject applyButtonInStarPanel;
    // สำหรับเปิดโหมดพรีวิว
    public GameObject InPreveiwMode;
    public EditorManager EM;

    private void Awake()
    {
        foreach (var theme in allThemes)
        {
            if (theme.lockItem != null)
            {
                theme.lockItem.themeIconManager = this; // ให้ LockSystem อ้างอิง ThemeIconManager
            }
        }

        //Set Preveiw Mode
        preveiwTheme = defaultTheme;
        InPreveiwMode.SetActive(false);
        GameObject newTextObject = new GameObject("DynamicTMP");
        preveiw = newTextObject.AddComponent<TextMeshProUGUI>();

        //Set icon in theme
        if (iconParent != null)
        {
            Image[] allImagesInChildren = iconParent.GetComponentsInChildren<Image>(true);
            foreach (var image in allImagesInChildren)
            {
                if (image.CompareTag("iconChange") || image.CompareTag("SelectedIcon"))
                {
                    allIcons.Add(image);
                }
                else if (image.CompareTag("iconInEditPanel")) {
                    allIconsInEditPanel.Add(image);
                }
            }
        }

        // ค้นหาข้อความทั้งหมดที่มี Tag = "TextInPanel" (รวมถึงที่ไม่แอคทีฟ)
        if (textParent != null)
        {
            TMP_Text[] allTextsInChildren = textParent.GetComponentsInChildren<TMP_Text>(true);
            foreach (var text in allTextsInChildren)
            {
                if (text.CompareTag("TextInPanel"))
                {
                    allTexts.Add(text);
                }
            }
        }
      
        // false ปุ่มราคาไว้ก่อน
        priceButton.gameObject.SetActive(false);
        priceStarButton.gameObject.SetActive(false);
        // โหลด Sprite สำหรับแต่ละธีม
        foreach (var theme in allThemes)
        {
            theme.allSprites = Resources.LoadAll<Sprite>(theme.resourcePath);
            if (theme.allSprites == null || theme.allSprites.Length == 0)
            {
                Debug.LogWarning($"ไม่มี Sprite ใน Resource Path: {theme.resourcePath}");
            }
            if (theme.lockItem.isLocked == true) 
            {
                theme.lockItem.CreateLockIcon();
            }

        }

        // โหลด sprite Default Theme
        defaultTheme.allSprites = Resources.LoadAll<Sprite>(defaultTheme.resourcePath);

        // โหลดธีมล่าสุดจาก PlayerPrefs และตั้งค่าทันที
        string lastTheme = PlayerPrefs.GetString("SelectedTheme", defaultTheme.themeName);
        themeData.selectedThemeName = lastTheme;

        if (lastTheme != defaultTheme.themeName)
        {
            presentThemeNameShow.text = lastTheme;
            selectedThemNameShow.text = lastTheme;
            SetTheme(lastTheme);
        }
        else
        {
            presentThemeNameShow.text = defaultTheme.themeName;
            selectedThemNameShow.text = defaultTheme.themeName;
            SetDefault();
        }
        selectedStarThem.text = selectedThemNameShow.text;
        if (defaultTheme != null)
        {
            themeIconColors[defaultTheme.themeName] = new Dictionary<string, Color>();
        }

        //ส่งปุ่ม apply in star panel ไปให้ wishlist manager
        wishlistManager.SetApplyButton(applyButtonInStarPanel) ;
    }

    public void SetDefault()
    {

        selectedTheme = defaultTheme;
        // บันทึกธีมล่าสุดลงใน ScriptableObject
        themeData.selectedThemeName = selectedTheme.themeName;

        PlayerPrefs.SetString("SelectedTheme", selectedTheme.themeName);
        PlayerPrefs.Save();

        ApplyThemeToUI();
        CanclePanel();

    }

    public void PreviewTheme(TMP_Text textInput)
    {
        string theme = textInput.text;
        Debug.Log("Previewing theme: " + theme);
        // แสดงธ๊มที่ผู้เล่นกำลังเลือกดู
        selectedThemNameShow.text = theme;
        selectedStarThem.text = selectedThemNameShow.text;
        textInDoneButton.text = "Apply";
        Theme InputTheme = allThemes.Find(t => t.themeName == theme);

        if (InputTheme != null)
        {   
            // preveiw theme ที่กำลังเลือกดู
            preveiwTheme = InputTheme;
            Debug.Log("Setting previewTheme to: " + preveiwTheme.themeName);
            themeImageBG.sprite = preveiwTheme.backgroundSprite; // อัปเดต sprite ตรงนี้
            
            // ปุ่ม done จะแสดงข้อความ apply เมื่อปลดล็อคธีมแล้ว
            if (InputTheme.lockItem.isLocked == true)
            { 
                textInDoneButton.text = "Preveiw";
            }
            priceButton.gameObject.SetActive(InputTheme.lockItem.isLocked);
            priceStarButton.gameObject.SetActive(InputTheme.lockItem.isLocked);

        }
        else {

            // preveiw theme ที่กำลังเลือกดู
            preveiwTheme = defaultTheme;
            themeImageBG.sprite = defaultTheme.backgroundSprite;
            // เปิดปุ่มราคาเมื่อ กดดูปุ่มที่ถูกล็อค
            priceButton.gameObject.SetActive(false);
            priceStarButton.gameObject.SetActive(false);
        }
        starThemeImageBG.sprite = themeImageBG.sprite;
        textInStarDoneButton.text = textInDoneButton.text;
    }

    public void ApplyTheme() {
        //Debug.Log(selectedThemNameShow.text);
        if (selectedThemNameShow.text == "Default")
        {
            SetDefault();
        }
        else {
            SetTheme(selectedThemNameShow.text);
        }
        CanclePanel();
    }

    public void SetTheme(string name)
    {
        selectedTheme = allThemes.Find(t => t.themeName == name );

        if (selectedTheme == null)
        {
            selectedTheme = defaultTheme;
        }
        if (textInDoneButton.text == "Preveiw") {
            //Debug.Log(preveiwTheme.themeName);
            preveiwTheme = selectedTheme;  // อัปเดต preveiwTheme
            themeImageBG.sprite = preveiwTheme.backgroundSprite; // อัปเดต sprite
            starThemeImageBG.sprite = themeImageBG.sprite;
            ApplyThemeToPreviewUI(preveiwTheme);
            InPreveiwMode.SetActive(true);
        }
        else if (textInDoneButton.text == "Apply") {
            // บันทึกธีมล่าสุดลงใน ScriptableObject และ PlayerPrefs
            themeData.selectedThemeName = name;
            PlayerPrefs.SetString("SelectedTheme", name);
            PlayerPrefs.Save();
            ApplyThemeToUI();
        }

        
    }

    // เรียกฟังก์ชันนี้เมื่อออกจาก Editor Mode 
    public void RestorePreviousTheme()
    {
        selectedTheme = previousTheme;
        //Debug.Log("previous " +selectedTheme.themeName);
        ApplyThemeToUI();
        //Debug.Log("preview " +preveiwTheme.themeName);
        preveiw.text = preveiwTheme.themeName;
        PreviewTheme(preveiw);

    }

    private void ApplyThemeToUI()
    {
        previousTheme = selectedTheme;
        presentThemeNameShow.text = selectedTheme.themeName;
        themeImageBG.sprite = selectedTheme.backgroundSprite;
        starThemeImageBG.sprite = themeImageBG.sprite;

        foreach (var text in allTexts)
        {
            if (text != null)
            {
                text.color = selectedTheme.textColor;
            }
        }

        foreach (var icon in allIcons)
        {
            if (selectedTheme.themeName == defaultTheme.themeName) //  ถ้าเป็นธีม Default โหลดสีเดิม
            {
                if (themeIconColors.ContainsKey(defaultTheme.themeName) && themeIconColors[defaultTheme.themeName].ContainsKey(icon.name))
                {
                    Debug.Log("Icon Color " + icon.color.ToString());
                    icon.color = themeIconColors[defaultTheme.themeName][icon.name]; // โหลดสีเก่ากลับมา
                }
            }
            else // ถ้าไม่ใช่ธีม Default ทำให้ไอคอนเป็นสีขาว
            {

                icon.color = Color.white; //  เปลี่ยนเป็นสีขาว
            }

            foreach (var sprite in selectedTheme.allSprites)
            {
                if (icon.name == sprite.name)
                {
                    icon.sprite = sprite;
                }
            }
        }

        foreach (var icon in allIconsInEditPanel)
        {
            icon.color = selectedTheme.textColor;

        }

        PlayerPrefs.Save();
    }

    private void ApplyThemeToPreviewUI(Theme themeToPreveiw)
    {
        themeImageBG.sprite = themeToPreveiw.backgroundSprite;
        starThemeImageBG.sprite = themeImageBG.sprite;
        foreach (var text in allTexts)
        {
            if (text != null)
            {
                text.color = themeToPreveiw.textColor;
            }
        }

        foreach (var icon in allIcons)
        {
            icon.color = Color.white; //  เปลี่ยนเป็นสีขาว
            foreach (var sprite in selectedTheme.allSprites)
            {
                if (icon.name == sprite.name)
                {
                    icon.sprite = sprite;
                }
            }
        }
    }


    public void CanclePanel()
    {
        //gameObject.SetActive(false);
        EM.ShowEditThemeAgain();

    }

    public void saveColorInThemeApplyAll(Color colorWant)
    {

        //Debug.Log("colorwant "+ colorWant.ToString());
        foreach (var icon in allIcons)
        {

            if (!themeIconColors[defaultTheme.themeName].ContainsKey(icon.name))
            {
                themeIconColors[defaultTheme.themeName][icon.name] = colorWant; // บันทึกสีเดิมของไอคอน
            }

        }

    }

    public void saveColorInThemeApplyThis(Color colorWant, GameObject iconSelected)
    {

        //Debug.Log("colorwant " + colorWant.ToString());
        foreach (var icon in allIcons)
        {

            if (iconSelected.name == icon.name)
            {
                themeIconColors[defaultTheme.themeName][icon.name] = colorWant; // บันทึกสีเดิมของไอคอน
            }
            else
            {
                themeIconColors[defaultTheme.themeName][icon.name] = icon.color; // โหลดสีเก่ากลับมา
            }
        }

    }

    //search theme by tag
    public List<Theme> SearchThemesByAdjective(string keyword)
    {
        List<Theme> matchingThemes = new List<Theme>(); // สร้าง List เปล่าไว้เก็บธีมที่ตรงเงื่อนไข

        foreach (Theme theme in allThemes) // วนลูปเช็คทุกธีม
        {
            foreach (string adj in theme.adjectives) // วนลูปเช็คทุกคำคุณศัพท์ของธีมนั้น
            {
                if (adj.ToLower().Contains(keyword.ToLower())) // ตรวจสอบว่าคำคุณศัพท์ตรงกับ keyword ไหม
                {
                    matchingThemes.Add(theme); // ถ้าตรง ให้เพิ่มธีมนี้เข้า List
                    break; // ออกจากลูปคำคุณศัพท์ เพราะเจอแล้ว
                }
            }
        }

        //check default theme ด้วย
        foreach (string adj in defaultTheme.adjectives) 
        {
            if (adj.ToLower().Contains(keyword.ToLower())) 
            {
                matchingThemes.Add(defaultTheme); 
                break; 
            }
        }

        return matchingThemes; 
    }

    //search manager 
    public void UpdateThemeSearchResults()
    {
        string keyword = searchInput.text;
        foreach (Transform child in themeListParent)
        {
            //Debug.Log(child.name);
            child.gameObject.SetActive(false);
        }

        // ค้นหาธีมที่ตรงกับคำค้นหา
        List<Theme> foundThemes = SearchThemesByAdjective(keyword);

        foreach (Transform child in themeListParent)
        {
            // ดึงข้อความจากปุ่ม
            string buttonLabel = "";

            TMP_Text buttonText = child.GetComponentInChildren<TMP_Text>(); 
            if (buttonText != null)
            {
                buttonLabel = buttonText.text;
            }
            else
            {
                TMP_Text tmpText = child.GetComponentInChildren<TMP_Text>(); 
                if (tmpText != null)
                {
                    buttonLabel = tmpText.text;
                }
            }

            // ตรวจสอบว่า themeName ตรงกับข้อความบนปุ่มหรือไม่
            foreach (Theme theme in foundThemes)
            {
                if (theme.themeName == buttonLabel)
                {
                    child.gameObject.SetActive(true); // แสดงปุ่มที่ตรงกับเงื่อนไข
                    //break; // เจอแล้วก็หยุด loop
                }
            }
        }

    }

    public void BuyTheme(TMP_Text themePrice) 
    {
        if (preveiwTheme != null && preveiwTheme.lockItem != null)
        {
            preveiwTheme.lockItem.BuyItem(themePrice, () =>
            {
                textInDoneButton.text = "Apply";
                textInStarDoneButton.text = textInDoneButton.text;
            });
        }
    }

}