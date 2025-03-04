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

public class RespondJson //(***) จัดการข้อมูลที่ได้จาก Response จาก API
{
    public string bot_id;
    public string intent;
    public List_Obj[] messages;
    public string source;
}
[System.Serializable]

public class List_Obj
{
    public string audio_url;
    public string mid;
    public string text;
    public string timestamp;
    public string type;
}

public class ApiManager : MonoBehaviour //คอยจัดการ การทำงานระหว่าง Unity และ API
{
    private string api_url = "https://asia-southeast1-botnoiasr.cloudfunctions.net/voicebotBU";
    [SerializeField] private TMP_Text displayText;
    [SerializeField] private Button okayBtn;
    [SerializeField] private TMP_Text btnText;
    private byte[] fileContents = null;
    private string response;
    public AudioSource myAudio;
    public TMP_InputField myInput ;
    
    // 4 Type API 
    enum APIType {VtV, VtT, TtV, TtT }
    [SerializeField] private APIType apiType;

    public void clickBtn() //ฟังก์ชันสำหรับปุ่มที่หน้าจอ
    {
        Debug.Log("sendAPI");
        Debug.Log(apiType.ToString());
        if (apiType.ToString() == "VtV") {
            StartCoroutine(sendRequestAPIVoiceToVoice());
        }
        else if (apiType.ToString() == "VtT") {
            StartCoroutine(sendRequestAPIVoiceToText());
        }
        else if (apiType.ToString() == "TtV") {
            StartCoroutine(sendRequestAPITextToVoice());
        }
        else if (apiType.ToString() == "TtT") {
            StartCoroutine(sendRequestAPITextToText());
        }

    }

    private IEnumerator sendRequestAPIVoiceToVoice() //จัดการข้อมูลต่างๆ ในโปรเจคก่อนส่ง API
    {
        btnText.text = "Sending Request";

        //เรียกไฟล์เสียงที่อัดไว้ จาก Script : microphoneManager
        string fileName = "Record.wav"; 
        string filePath = Path.Combine(Application.streamingAssetsPath + "/Recordings/", fileName);

        //เช็คว่ามีไฟล์เสียงแล้วหรือยัง
        if (!File.Exists(filePath))
        {
            Debug.LogError($"File {filePath} not found!");
        }

        // Read file in binary mode (ใช้สำหรับอ่านข้อมูลจากไฟล์เสียง ออกมาเป็น Bytes)
        using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            // -----------------------------------------------------------------
            fileContents = new byte[fileStream.Length];
            fileStream.Read(fileContents, 0, (int)fileStream.Length);
        }
        // -----------------------------------------------------------------
        Debug.Log($"File {filePath} contains {fileContents.Length} bytes.");
        string hexString = BitConverter.ToString(fileContents);

        //เตรียมข้อมูลสำหรับส่งไปให้กับ API (ยังไม่เป็น JSON)
         string post_to_api = "{ " +
            "\"customerId\": \"00001\", " +
            "\"botId\": \"64464df59f76af17c9ca0ed3\", " +
            "\"input\": { " +
                "\"type\": \"audio\", " +
                "\"object\": { " +
                    "\"language\": \"th\", " +
                    "\"audioData\": \"" + hexString + "\", " +
                    "\"dataFormat\": \"hex\", " +
                    "\"audioFormat\": \"wav\" " +
                "} " +
            "}, " +
            "\"output\": { " +
                "\"type\": \"audio\", " +
                "\"object\": { " +
                    "\"speakerId\": \"โบ\" " +
                "} " +
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
            StartCoroutine(handleJSONVoice());
        }
        else //กรณีส่ง API ไม่สำเร็จ
        {
            Debug.LogError("API request failed: " + request.error);
        }
    }

    private IEnumerator sendRequestAPIVoiceToText() //จัดการข้อมูลต่างๆ ในโปรเจคก่อนส่ง API
    {
        btnText.text = "Sending Request";

        //เรียกไฟล์เสียงที่อัดไว้ จาก Script : microphoneManager
        string fileName = "Record.wav";
        string filePath = Path.Combine(Application.streamingAssetsPath + "/Recordings/", fileName);

        //เช็คว่ามีไฟล์เสียงแล้วหรือยัง
        if (!File.Exists(filePath))
        {
            Debug.LogError($"File {filePath} not found!");
        }

        // Read file in binary mode (ใช้สำหรับอ่านข้อมูลจากไฟล์เสียง ออกมาเป็น Bytes)
        using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            // -----------------------------------------------------------------
            fileContents = new byte[fileStream.Length];
            fileStream.Read(fileContents, 0, (int)fileStream.Length);
        }
        // -----------------------------------------------------------------
        Debug.Log($"File {filePath} contains {fileContents.Length} bytes.");
        string hexString = BitConverter.ToString(fileContents);

        //เตรียมข้อมูลสำหรับส่งไปให้กับ API (ยังไม่เป็น JSON)
        string post_to_api = "{ " +
           "\"customerId\": \"00001\", " +
           "\"botId\": \"64464df59f76af17c9ca0ed3\", " +
           "\"input\": { " +
               "\"type\": \"audio\", " +
               "\"object\": { " +
                   "\"language\": \"th\", " +
                   "\"audioData\": \"" + hexString + "\", " +
                   "\"dataFormat\": \"hex\", " +
                   "\"audioFormat\": \"wav\" " +
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

    private IEnumerator sendRequestAPITextToVoice() //จัดการข้อมูลต่างๆ ในโปรเจคก่อนส่ง API
    {
        btnText.text = "Sending Request";

        //เช็คว่ามีข้อความแล้วหรือยัง
        if (myInput.text == "")
        {
            Debug.LogError($"text not found!");
        }
      
        //เตรียมข้อมูลสำหรับส่งไปให้กับ API (ยังไม่เป็น JSON)
        string post_to_api = "{ " +
           "\"_id\": \"0001\", " +
           "\"customerId\": \"00001\", " +
           "\"botId\": \"64464df59f76af17c9ca0ed3\", " +
           "\"input\": { " +
               "\"type\": \"text\", " +
               "\"object\": { " +
                   "\"text\": " + JsonConvert.SerializeObject(myInput.text, Formatting.Indented)  +
               "} " +
           "}, " +
           "\"output\": { " +
               "\"type\": \"audio\", " +
               "\"object\": { " +
                   "\"speakerId\": \"โบ\" " +
               "} " +
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
            StartCoroutine(handleJSONVoice());
        }
        else //กรณีส่ง API ไม่สำเร็จ
        {
            Debug.LogError("API request failed: " + request.error);
        }
    }

    private IEnumerator sendRequestAPITextToText() //จัดการข้อมูลต่างๆ ในโปรเจคก่อนส่ง API
    {
        btnText.text = "Sending Request";
        //เช็คว่ามีข้อความแล้วหรือยัง
        if (myInput.text == "")
        {
            Debug.LogError($"text not found!");
        }

        //เตรียมข้อมูลสำหรับส่งไปให้กับ API (ยังไม่เป็น JSON)
        string post_to_api = "{ " +
           "\"_id\": \"0001\", " +
           "\"customerId\": \"00001\", " +
           "\"botId\": \"64464df59f76af17c9ca0ed3\", " +
           "\"input\": { " +
               "\"type\": \"text\", " +
               "\"object\": { " +
                   "\"text\": " + JsonConvert.SerializeObject(myInput.text, Formatting.Indented) +
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

    public IEnumerator handleJSONVoice() //ฟังก์ชันสำหรับจัดการข้อมูล Response ที่ได้จาก API เพื่อให้่ Unity นำไปใช้ได้ง่าย ส่วนการจัดข้อมูลลงในแต่ละส่วนเลื่อนขึ้นข้างบน (***)
    {
        RespondJson respondJson = new RespondJson();
        try
        {
            respondJson = JsonUtility.FromJson<RespondJson>(response);
            StartCoroutine(receiveResponseVoice(respondJson));
        }
        catch (Exception e)
        {
            Debug.Log("Exception = " + e);
            yield break;
        }
        yield return respondJson;
    }

    public IEnumerator receiveResponseVoice(RespondJson respondJson) //นำข้อมูลที่จัดแล้วมาใช้กับ Unity
    {
        //กรณีต้องการนำไฟล์เสียงมาใช้ในโปรเจค (หากไม่ใช้ปิด/ลบ ออกได้เลย)
        btnText.text = "Generate Sound";
        string audio_url = respondJson.messages[0].audio_url;
        UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(audio_url, AudioType.WAV);
        yield return www.SendWebRequest();
        myAudio.clip = DownloadHandlerAudioClip.GetContent(www);
        SavWav.Save("ResponseAudio", myAudio.clip);
        displayText.text = respondJson.messages[0].text;
        myAudio.Play();
        btnText.text = "SendAPI";

        
        //ระบุตำแหน่งไฟล์เสียงสังเคราะห์ที่โหลดมา
        string fileName = "ResponseAudio.wav";

        string filePath = Path.Combine(Application.streamingAssetsPath + "/Recordings/", fileName);

        //ตรวจสอบว่ามีไฟล์อยู่ในเครื่องหรือไม่
        if (File.Exists(filePath))
        {
            // ลบไฟล์
            File.Delete(filePath);
            Debug.Log("File deleted: " + filePath);
        }
        else
        {
            Debug.LogWarning("File not found: " + filePath);
        }

        //ตรวจสอบว่ามีไฟล์อยู่ใน Input หรือไม่
        if (myInput.text != "")
        {
            // ลบไฟล์
            myInput.text = string.Empty;
            Debug.Log("File deleted!");
        }
        else
        {
            Debug.LogWarning("Input not found");
        }
    }
    public IEnumerator receiveResponseText(RespondJson respondJson) //นำข้อมูลที่จัดแล้วมาใช้กับ Unity
    {
        //กรณีต้องการนำไฟล์เสียงมาใช้ในโปรเจค (หากไม่ใช้ปิด/ลบ ออกได้เลย)
        btnText.text = "Generate Sound";
        string audio_url = respondJson.messages[0].audio_url;
        UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(audio_url, AudioType.WAV);
        yield return www.SendWebRequest();
        //myAudio.clip = DownloadHandlerAudioClip.GetContent(www);
        //SavWav.Save("ResponseAudio", myAudio.clip);
        displayText.text = respondJson.messages[0].text;
        //myAudio.Play();
        btnText.text = "SendAPI";


        //ตรวจสอบว่ามีไฟล์อยู่ใน Input หรือไม่
        if (myInput.text != "")
        {
            // ลบไฟล์
            myInput.text = string.Empty;
            Debug.Log("File deleted!");
        }
        else
        {
            Debug.LogWarning("Input not found");
        }

        //ระบุตำแหน่งไฟล์เสียงสังเคราะห์ที่โหลดมา
        string fileName = "ResponseAudio.wav";

        string filePath = Path.Combine(Application.streamingAssetsPath + "/Recordings/", fileName);

        //ตรวจสอบว่ามีไฟล์อยู่ในเครื่องหรือไม่
        if (File.Exists(filePath))
        {
            // ลบไฟล์
            File.Delete(filePath);
            Debug.Log("File deleted: " + filePath);
        }
        else
        {
            Debug.LogWarning("File not found: " + filePath);
        }
    }
}
