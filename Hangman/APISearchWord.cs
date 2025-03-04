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

public class RespondJsonForSearch //(***) �Ѵ��â����ŷ����ҡ Response �ҡ API
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


    public void clickBtn() //�ѧ��ѹ����Ѻ�������˹�Ҩ�
    {
        Debug.Log("sendAPI");
        Debug.Log(countInput.text);
        StartCoroutine(sendRequestAPI());
    
    }
    private IEnumerator sendRequestAPI() //�Ѵ��â����ŵ�ҧ� ���ਤ��͹�� API
    {
        btnText.text = "���ѧ��Ŵ";

        //������բ�ͤ������������ѧ
        if (categoryInput.text == "" || (countInput.text == ""))
        {
            Debug.LogError($"input not found!");
        }

        //���������������Ѻ������Ѻ API (�ѧ����� JSON)
        string post_to_api = "{ " +
           "\"_id\": \"0001\", " +
           "\"customerId\": \"00001\", " +
           "\"botId\": \"64464df59f76af17c9ca0ed3\", " +
           "\"input\": { " +
               "\"type\": \"text\", " +
               "\"object\": { " +
                   "\"text\": " + JsonConvert.SerializeObject("�ͤ��Ѿ�������ѧ�������ǡѺ" + categoryInput.text + "�繨ӹǹ " + countInput.text + "�� Ẻ���� �·������ͧ�դ��������� ����ͧ���Ţ��˹�Ҥ��Ѿ����� �ͺ������Ѿ��� ����ͧ�դ������Ш��ӵͺ����ͧ�ըش full stop", Formatting.Indented) +
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

    public IEnumerator receiveResponseText(RespondJson respondJson) //�Ӣ����ŷ��Ѵ��������Ѻ Unity
    {
        //�óյ�ͧ��ù�������§�������ਤ (�ҡ�����Դ/ź �͡�����)
        btnText.text = "���ѧ��Ŵ";
        string audio_url = respondJson.messages[0].audio_url;
        UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(audio_url, AudioType.WAV);
        yield return www.SendWebRequest();
        displayText.text = respondJson.messages[0].text;
        //Debug.Log(displayText.text);

        //split
        //vocabsObjects = displayText.text.Split(", ");

        //myAudio.Play();
        btnText.text = "�������";


        //��Ǩ�ͺ������������� Input �������
        if (categoryInput.text != "" && countInput.text != "")
        {
            // ź���
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
