using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;

public class HangmanController : MonoBehaviour
{
    [SerializeField] GameObject wordContainer;
    [SerializeField] GameObject keyboardContainer;
    [SerializeField] GameObject letterContainer;
    [SerializeField] GameObject[] hangmanStage;
    [SerializeField] GameObject letterButton;
    //[SerializeField] TextAsset possibleWord;
    public TMP_Text round;
    public TMP_Text score;
    private int scorePlayer = 0;
    private int countVocab = 0;

    private string word;
    private int incorrect, correct;
    private string[] vocabsObjects;
    public Button nextButtton;

    public void Start()
    {
        //Debug.Log(APISearchWord.vocabsObjects[0] + " Hello");
        Debug.Log(PlayerPrefs.GetString("vocab"));
       
        vocabsObjects = PlayerPrefs.GetString("vocab").Split(", ");
        round.text = "/ "  + vocabsObjects.Length.ToString() + " คะแนน";
        score.text = scorePlayer.ToString();
        InintialiseButtons();
        InitialiseGame();
    }

    private void InintialiseButtons() {
        for (int i = 65; i<=90 ;i++) {
            CreateButton(i);
        }

    }
    private void InitialiseGame() {
        incorrect = 0;
        correct = 0;
        nextButtton.gameObject.SetActive(false);

        foreach (Button child in keyboardContainer.GetComponentsInChildren<Button>()) {
            child.interactable = true;
        }
        foreach (Transform child in wordContainer.GetComponentInChildren<Transform>())
        {
            Destroy(child.gameObject);
        }
        foreach (GameObject stage in  hangmanStage) {
            stage.SetActive(false);
        }

        //generate new word
        word = generateWord().ToUpper();
        countVocab++;
        foreach (char letter in word) {
            var temp = Instantiate(letterContainer, wordContainer.transform);
            var textComponent = temp.GetComponentInChildren<TextMeshProUGUI>();
            if (letter == ' ')
            { // ตรวจสอบว่าเป็น white space หรือไม่
                textComponent.text = "#"; // แสดง white space
                textComponent.color = Color.green; // ตั้งสีเพื่อให้ดูว่าเปิดแล้ว
                correct++; // นับว่า white space ตรงกับคำทันที
            }
        }
    }
    private void CreateButton(int i) {
        GameObject temp = Instantiate(letterButton, keyboardContainer.transform);
        temp.GetComponentInChildren<TextMeshProUGUI>().text = ((char)i).ToString();
        temp.GetComponent<Button>().onClick.AddListener(delegate 
        { CheckLetter(((char)i).ToString()); 
        });
    }

    private string generateWord() {
        if (countVocab < vocabsObjects.Length) {
            return vocabsObjects[countVocab].Substring(0, vocabsObjects[countVocab].Length);
        }
        else
        {
            return "End";
        }

    }

    private void CheckLetter(string inputLetter) 
    {
        bool letterInWord = false;
        for (int i = 0; i< word.Length ;i++) 
        {
            if (inputLetter == word[i].ToString()) {
                letterInWord = true;
                correct++;
                wordContainer.GetComponentsInChildren<TextMeshProUGUI>()[i].text = inputLetter;
            }
        }

        if (letterInWord == false) { 
            incorrect++;
            hangmanStage[incorrect - 1].SetActive(true);
        }
        CheckOutCome();
    }

    private void CheckOutCome() {
        if (correct == word.Length) {
            for (int i=0 ; i < word.Length ; i++) {
                wordContainer.GetComponentsInChildren<TextMeshProUGUI>()[i].color = Color.green;
            }
            scorePlayer += 1;
            score.text = scorePlayer.ToString();
            nextButtton.gameObject.SetActive(true);
        }

        if (incorrect == hangmanStage.Length) {
            for (int i = 0; i < word.Length; i++)
            {
                var textComponent = wordContainer.GetComponentsInChildren<TextMeshProUGUI>()[i];
                textComponent.text = word[i].ToString(); // แสดงตัวอักษรที่ถูก
                if (word[i] == ' ')
                {
                    textComponent.text = "#"; // แสดง # แทน white space
                    textComponent.color = Color.red; // เปลี่ยนสี # เป็นสีแดง
                }
                else
                {
                    textComponent.color = Color.red; // เปลี่ยนสีตัวอักษรอื่นเป็นสีแดง
                }
            }
            nextButtton.gameObject.SetActive(true);
        }
    }

    public void nextButton() {
        //ดักกรณี index out of range
        if (countVocab >= vocabsObjects.Length)
        {
            SceneManager.LoadScene("SampleScene");
        }
        InitialiseGame();
    }

}
