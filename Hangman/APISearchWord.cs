using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json;
using UnityEngine.Windows;
using UnityEngine.SceneManagement;

public class RespondJsonForSearch //(***) จัดการข้อมูลที่ได้จาก Response จาก API
{
    public string bot_id;
    public string intent;
    public List_Obj[] messages;
    public string source;
}
[System.Serializable]

public class List_Obj_Search
{
    public string mid;
    public string text;
    public string timestamp;
    public string type;
}

public class APISearchWord : MonoBehaviour
{
    private string api_url = "https://asia-southeast1-botnoiasr.cloudfunctions.net/voicebotBU";
    [SerializeField] private TMP_Text displayText;
    [SerializeField] private Button okayBtn;
    [SerializeField] private TMP_Text btnText;
    private string response;
    public TMP_InputField categoryInput;
    public TMP_InputField countInput;
    //public static string[] vocabsObjects;
    public int objectIndex;


    public void clickBtn() //ฟังก์ชันสำหรับปุ่มที่หน้าจอ
    {
        Debug.Log("sendAPI");
        Debug.Log(countInput.text);
        StartCoroutine(sendRequestAPI());
    
    }
    private IEnumerator sendRequestAPI() //จัดการข้อมูลต่างๆ ในโปรเจคก่อนส่ง API
    {
        btnText.text = "กำลังโหลด";

        //เช็คว่ามีข้อความแล้วหรือยัง
        if (categoryInput.text == "" || (countInput.text == ""))
        {
            Debug.LogError($"input not found!");
        }

        //เตรียมข้อมูลสำหรับส่งไปให้กับ API (ยังไม่เป็น JSON)
        string post_to_api = "{ " +
           "\"_id\": \"0001\", " +
           "\"customerId\": \"00001\", " +
           "\"botId\": \"64464df59f76af17c9ca0ed3\", " +
           "\"input\": { " +
               "\"type\": \"text\", " +
               "\"object\": { " +
                   "\"text\": " + JsonConvert.SerializeObject("ขอคำศัพท์ภาษาอังกฤษเกี่ยวกับ" + categoryInput.text + "เป็นจำนวน " + countInput.text + "คำ แบบสุ่ม โดยที่ไม่ต้องมีคำแปลภาษาไทย ไม่ต้องมีเลขนำหน้าคำศัพท์และ ตอบมาแค่คำศัพท์พอ ไม่ต้องมีคำอื่นและจบคำตอบไม่ต้องมีจุด full stop", Formatting.Indented) +
               "} " +
           "}, " +
           "\"output\": { " +
               "\"type\": \"text\" " +
           "} " +
        "}";
        Debug.Log(post_to_api);

        //นำข้อมูลจากข้างต้นมาทำให้เป็น JSON Format สำหรับการส่ง API
        UnityWebRequest request = UnityWebRequest.Post(api_url, post_to_api);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(post_to_api);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        //ส่ง Request และรอ Response จาก API
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success) //กรณีส่ง API สำเร็จ และ API ส่ง Response กลับมาแล้ว
        {
            Debug.Log("API response received: " + request.downloadHandler.text);
            response = request.downloadHandler.text;
            StartCoroutine(handleJSONText());
        }
        else //กรณีส่ง API ไม่สำเร็จ
        {
            Debug.LogError("API request failed: " + request.error);
        }
    }

    public IEnumerator handleJSONText() //ฟังก์ชันสำหรับจัดการข้อมูล Response ที่ได้จาก API เพื่อให้่ Unity นำไปใช้ได้ง่าย ส่วนการจัดข้อมูลลงในแต่ละส่วนเลื่อนขึ้นข้างบน (***)
    {
        RespondJson respondJson = new RespondJson();
        try
        {
            respondJson = JsonUtility.FromJson<RespondJson>(response);
            StartCoroutine(receiveResponseText(respondJson));
        }
        catch (Exception e)
        {
            Debug.Log("Exception = " + e);
            yield break;
        }
        yield return respondJson;
    }

    public IEnumerator receiveResponseText(RespondJson respondJson) //นำข้อมูลที่จัดแล้วมาใช้กับ Unity
    {
        //กรณีต้องการนำไฟล์เสียงมาใช้ในโปรเจค (หากไม่ใช้ปิด/ลบ ออกได้เลย)
        btnText.text = "กำลังโหลด";
        string audio_url = respondJson.messages[0].audio_url;
        UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(audio_url, AudioType.WAV);
        yield return www.SendWebRequest();
        displayText.text = respondJson.messages[0].text;
        //Debug.Log(displayText.text);

        //split
        //vocabsObjects = displayText.text.Split(", ");

        //myAudio.Play();
        btnText.text = "เริ่มเกม";


        //ตรวจสอบว่ามีไฟล์อยู่ใน Input หรือไม่
        if (categoryInput.text != "" && countInput.text != "")
        {
            // ลบไฟล์
            categoryInput.text = string.Empty;
            countInput.text = string.Empty;
            Debug.Log("File deleted!");
        }
        else
        {
            Debug.LogWarning("Input not found");
        }

        PlayerPrefs.SetString("vocab",displayText.text);
        // load game hangman
        //SceneManager.LoadScene("hangman");
        SceneManager.LoadScene("Wheel");
    }


}
