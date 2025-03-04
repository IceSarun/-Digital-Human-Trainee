using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Money : MonoBehaviour
{
    public float moneyInWallet = 100f;
    public TMP_Text showMoneyText;

 
    private void Update()
    {
        showMoneyText.text = moneyInWallet.ToString() + " ß";
    }
    public void AddMoney() {
        moneyInWallet += 10;
    }
    public void RemoveMoney() { 
        moneyInWallet-= 10;
    }
        
    public void ResetMoney()
    {
        moneyInWallet = 100;
    }

    public void PurchaseItem(float count) { 
        moneyInWallet-= count;
    }
}
