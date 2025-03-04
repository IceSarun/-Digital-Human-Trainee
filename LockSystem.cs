using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

public class LockSystem : MonoBehaviour
{
    public float price;
    public bool isLocked;
    public Sprite lockImage;
    public Money myMoney;
    public Transform thisObject;
    public ThemeIconManager themeIconManager;

    public void BuyItem(TMP_Text itemPrice, System.Action onPurchaseSuccess)
    {
        string numberOnly = Regex.Replace(itemPrice.text, "[^0-9]", "");
        
        if (float.TryParse(numberOnly.Trim(), out float itemCost))
        {
            if (myMoney.moneyInWallet >= itemCost)
            {
                myMoney.PurchaseItem(itemCost);
                isLocked = false;
                themeIconManager.priceButton.gameObject.SetActive(false);
                themeIconManager.priceStarButton.gameObject.SetActive(false);
                themeIconManager.textInDoneButton.text = "Apply";
                themeIconManager.textInDoneButton.text = themeIconManager.textInDoneButton.text;

                // ลบไอคอนล็อก
                UnLockIcon();
                // เรียก callback เมื่อซื้อสำเร็จ (อาจจะเป็นการปลดล็อคธีมหรือไอเท็มอื่นๆ)
                onPurchaseSuccess?.Invoke();

                
            }
        }
    }

    public void CreateLockIcon()
    {
        if (thisObject == null)
        {
            //Debug.LogWarning("thisObject เป็น null, ไม่สามารถสร้าง Lock Icon ได้");
            return;
        }

        GameObject lockIcon = new GameObject($"Lock_{thisObject.name}");
        lockIcon.transform.SetParent(thisObject, false);
        lockIcon.transform.localScale = Vector3.one;

        Image lockImageComponent = lockIcon.AddComponent<Image>();
        lockImageComponent.sprite = lockImage;
        lockImageComponent.color = new Color(0, 0, 0, 1);

        RectTransform parentRect = thisObject.GetComponent<RectTransform>();
        RectTransform rectTransform = lockIcon.GetComponent<RectTransform>();

        // ปรับขนาดให้เล็กกว่าขนาดของ parent 70%
        float newWidth = parentRect.rect.width * 0.35f;
        float newHeight = parentRect.rect.height * 0.8f;
        rectTransform.sizeDelta = new Vector2(newWidth, newHeight);

        // จัดวางให้อยู่ตรงกลาง
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = new Vector2(0f, 2f);

        // อัปเดตปุ่มราคา
        if (themeIconManager.priceButton != null && themeIconManager.priceStarButton != null)
        {
;           TMP_Text priceText = themeIconManager.priceButton.GetComponentInChildren<TMP_Text>();
            TMP_Text priceStarText = themeIconManager.priceStarButton.GetComponentInChildren<TMP_Text>();
            if (priceText != null && priceStarText != null)
            {
                priceText.text = $"{price} ฿";
                priceStarText.text = $"{price} ฿";
            }
        }
    }

    public void UnLockIcon()
    {
        if (thisObject == null)
        {
            //Debug.LogWarning("thisObject เป็น null, ไม่สามารถลบ Lock Icon ได้");
            return;
        }

        Transform lockIcon = thisObject.Find($"Lock_{thisObject.name}");

        if (lockIcon != null)
        {
            Destroy(lockIcon.gameObject);
        }
        // เพิ่มโค้ดปลดล็อกที่ StarButton ด้วย
        if (themeIconManager != null)
        {
            // หา StarButton ที่ตรงกับ ShowButton นี้
            GameObject starButton = themeIconManager.wishlistManager.starButtons.Find(obj => obj.name == thisObject.name);

            if (starButton != null)
            {
                Transform starLockIcon = starButton.transform.Find($"Lock_{starButton.name}");
                if (starLockIcon != null)
                {
                    Destroy(starLockIcon.gameObject);
                }
            }
        }
    }
}
