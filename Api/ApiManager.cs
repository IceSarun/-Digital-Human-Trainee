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

public class RespondJson //(***) �Ѵ��â����ŷ����ҡ Response �ҡ API
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

public class ApiManager : MonoBehaviour //��¨Ѵ��� ��÷ӧҹ�����ҧ Unity ��� API
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

    public void clickBtn() //�ѧ��ѹ����Ѻ�������˹�Ҩ�
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

    private IEnumerator sendRequestAPIVoiceToVoice() //�Ѵ��â����ŵ�ҧ� ���ਤ��͹�� API
    {
        btnText.text = "Sending Request";

        //���¡������§����Ѵ��� �ҡ Script : microphoneManager
        string fileName = "Record.wav"; 
        string filePath = Path.Combine(Application.streamingAssetsPath + "/Recordings/", fileName);

        //�������������§���������ѧ
        if (!File.Exists(filePath))
        {
            Debug.LogError($"File {filePath} not found!");
        }

        // Read file in binary mode (������Ѻ��ҹ�����Ũҡ������§ �͡���� Bytes)
        using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            // -----------------------------------------------------------------
            fileContents = new byte[fileStream.Length];
            fileStream.Read(fileContents, 0, (int)fileStream.Length);
        }
        // -----------------------------------------------------------------
        Debug.Log($"File {filePath} contains {fileContents.Length} bytes.");
        string hexString = BitConverter.ToString(fileContents);

        //���������������Ѻ������Ѻ API (�ѧ����� JSON)
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
                    "\"speakerId\": \"�\" " +
                "} " +
            "} " +
         "}";


        Debug.Log(post_to_api);

        //�Ӣ����Ũҡ��ҧ���ҷ������ JSON Format ����Ѻ����� API
        UnityWebRequest request = UnityWebRequest.Post(api_url, post_to_api);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(post_to_api);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        //�� Request ����� Response �ҡ API
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success) //�ó��� API ����� ��� API �� Response ��Ѻ������
        {
            Debug.Log("API response received: " + request.downloadHandler.text);
            response = request.downloadHandler.text;
            StartCoroutine(handleJSONVoice());
        }
        else //�ó��� API ��������
        {
            Debug.LogError("API request failed: " + request.error);
        }
    }

    private IEnumerator sendRequestAPIVoiceToText() //�Ѵ��â����ŵ�ҧ� ���ਤ��͹�� API
    {
        btnText.text = "Sending Request";

        //���¡������§����Ѵ��� �ҡ Script : microphoneManager
        string fileName = "Record.wav";
        string filePath = Path.Combine(Application.streamingAssetsPath + "/Recordings/", fileName);

        //�������������§���������ѧ
        if (!File.Exists(filePath))
        {
            Debug.LogError($"File {filePath} not found!");
        }

        // Read file in binary mode (������Ѻ��ҹ�����Ũҡ������§ �͡���� Bytes)
        using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            // -----------------------------------------------------------------
            fileContents = new byte[fileStream.Length];
            fileStream.Read(fileContents, 0, (int)fileStream.Length);
        }
        // -----------------------------------------------------------------
        Debug.Log($"File {filePath} contains {fileContents.Length} bytes.");
        string hexString = BitConverter.ToString(fileContents);

        //���������������Ѻ������Ѻ API (�ѧ����� JSON)
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

        //�Ӣ����Ũҡ��ҧ���ҷ������ JSON Format ����Ѻ����� API
        UnityWebRequest request = UnityWebRequest.Post(api_url, post_to_api);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(post_to_api);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        //�� Request ����� Response �ҡ API
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success) //�ó��� API ����� ��� API �� Response ��Ѻ������
        {
            Debug.Log("API response received: " + request.downloadHandler.text);
            response = request.downloadHandler.text;
            StartCoroutine(handleJSONText());
        }
        else //�ó��� API ��������
        {
            Debug.LogError("API request failed: " + request.error);
        }
    }

    private IEnumerator sendRequestAPITextToVoice() //�Ѵ��â����ŵ�ҧ� ���ਤ��͹�� API
    {
        btnText.text = "Sending Request";

        //������բ�ͤ������������ѧ
        if (myInput.text == "")
        {
            Debug.LogError($"text not found!");
        }
      
        //���������������Ѻ������Ѻ API (�ѧ����� JSON)
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
                   "\"speakerId\": \"�\" " +
               "} " +
           "} " +
        "}";

        Debug.Log(post_to_api);

        //�Ӣ����Ũҡ��ҧ���ҷ������ JSON Format ����Ѻ����� API
        UnityWebRequest request = UnityWebRequest.Post(api_url, post_to_api);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(post_to_api);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        //�� Request ����� Response �ҡ API
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success) //�ó��� API ����� ��� API �� Response ��Ѻ������
        {
            Debug.Log("API response received: " + request.downloadHandler.text);
            response = request.downloadHandler.text;
            StartCoroutine(handleJSONVoice());
        }
        else //�ó��� API ��������
        {
            Debug.LogError("API request failed: " + request.error);
        }
    }

    private IEnumerator sendRequestAPITextToText() //�Ѵ��â����ŵ�ҧ� ���ਤ��͹�� API
    {
        btnText.text = "Sending Request";
        //������բ�ͤ������������ѧ
        if (myInput.text == "")
        {
            Debug.LogError($"text not found!");
        }

        //���������������Ѻ������Ѻ API (�ѧ����� JSON)
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

        //�Ӣ����Ũҡ��ҧ���ҷ������ JSON Format ����Ѻ����� API
        UnityWebRequest request = UnityWebRequest.Post(api_url, post_to_api);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(post_to_api);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        //�� Request ����� Response �ҡ API
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success) //�ó��� API ����� ��� API �� Response ��Ѻ������
        {
            Debug.Log("API response received: " + request.downloadHandler.text);
            response = request.downloadHandler.text;
            StartCoroutine(handleJSONText());
        }
        else //�ó��� API ��������
        {
            Debug.LogError("API request failed: " + request.error);
        }
    }


    public IEnumerator handleJSONText() //�ѧ��ѹ����Ѻ�Ѵ��â����� Response �����ҡ API �������� Unity ���������� ��ǹ��èѴ������ŧ�������ǹ����͹��鹢�ҧ�� (***)
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

    public IEnumerator handleJSONVoice() //�ѧ��ѹ����Ѻ�Ѵ��â����� Response �����ҡ API �������� Unity ���������� ��ǹ��èѴ������ŧ�������ǹ����͹��鹢�ҧ�� (***)
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

    public IEnumerator receiveResponseVoice(RespondJson respondJson) //�Ӣ����ŷ��Ѵ��������Ѻ Unity
    {
        //�óյ�ͧ��ù�������§�������ਤ (�ҡ�����Դ/ź �͡�����)
        btnText.text = "Generate Sound";
        string audio_url = respondJson.messages[0].audio_url;
        UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(audio_url, AudioType.WAV);
        yield return www.SendWebRequest();
        myAudio.clip = DownloadHandlerAudioClip.GetContent(www);
        SavWav.Save("ResponseAudio", myAudio.clip);
        displayText.text = respondJson.messages[0].text;
        myAudio.Play();
        btnText.text = "SendAPI";

        
        //�кص��˹�������§�ѧ����������Ŵ��
        string fileName = "ResponseAudio.wav";

        string filePath = Path.Combine(Application.streamingAssetsPath + "/Recordings/", fileName);

        //��Ǩ�ͺ�����������������ͧ�������
        if (File.Exists(filePath))
        {
            // ź���
            File.Delete(filePath);
            Debug.Log("File deleted: " + filePath);
        }
        else
        {
            Debug.LogWarning("File not found: " + filePath);
        }

        //��Ǩ�ͺ������������� Input �������
        if (myInput.text != "")
        {
            // ź���
            myInput.text = string.Empty;
            Debug.Log("File deleted!");
        }
        else
        {
            Debug.LogWarning("Input not found");
        }
    }
    public IEnumerator receiveResponseText(RespondJson respondJson) //�Ӣ����ŷ��Ѵ��������Ѻ Unity
    {
        //�óյ�ͧ��ù�������§�������ਤ (�ҡ�����Դ/ź �͡�����)
        btnText.text = "Generate Sound";
        string audio_url = respondJson.messages[0].audio_url;
        UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(audio_url, AudioType.WAV);
        yield return www.SendWebRequest();
        //myAudio.clip = DownloadHandlerAudioClip.GetContent(www);
        //SavWav.Save("ResponseAudio", myAudio.clip);
        displayText.text = respondJson.messages[0].text;
        //myAudio.Play();
        btnText.text = "SendAPI";


        //��Ǩ�ͺ������������� Input �������
        if (myInput.text != "")
        {
            // ź���
            myInput.text = string.Empty;
            Debug.Log("File deleted!");
        }
        else
        {
            Debug.LogWarning("Input not found");
        }

        //�кص��˹�������§�ѧ����������Ŵ��
        string fileName = "ResponseAudio.wav";

        string filePath = Path.Combine(Application.streamingAssetsPath + "/Recordings/", fileName);

        //��Ǩ�ͺ�����������������ͧ�������
        if (File.Exists(filePath))
        {
            // ź���
            File.Delete(filePath);
            Debug.Log("File deleted: " + filePath);
        }
        else
        {
            Debug.LogWarning("File not found: " + filePath);
        }
    }
}
