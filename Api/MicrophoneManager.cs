using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;
using UnityEngine.UI;

public class MicrophoneManager : MonoBehaviour //Script สำหรับควบคุมการทำงานของไมโครโฟน
{
    public Button microphoneBtn;

    [SerializeField] 
    private Sprite mic_ON_sprite;
    [SerializeField]
    private Sprite mic_OFF_sprite;

    public AudioSource _audioSource;

    public AppManager appManager;

    void Start()
    {
        microphoneBtn.onClick.AddListener(() => { openMicrophone(); });
    }

   private void openMicrophone() //เปิดไมโครโฟนและทำการอัดไฟล์เสียง
    {
        microphoneBtn.image.sprite = mic_ON_sprite;
        appManager.sound_manager.openMicSound();
        _audioSource.clip = Microphone.Start(null, true, 10, AudioSettings.outputSampleRate);
        _audioSource.outputAudioMixerGroup = null;
        microphoneBtn.onClick.RemoveAllListeners();
        microphoneBtn.onClick.AddListener(() => { closeMicrophone(); });
    }

    private void closeMicrophone() //ปิดไมโครโฟนหลังอัดไฟล์เสียงเสร็จ
    {
        microphoneBtn.image.sprite = mic_OFF_sprite;
        appManager.sound_manager.closeMicSound();
        Microphone.End(null);
        microphoneBtn.onClick.RemoveAllListeners();
        microphoneBtn.onClick.AddListener(() => { openMicrophone(); });
        saveRecord();
    }

    private void saveRecord() //ทำการเซฟไฟล์ที่อัดได้ไว้บนเครื่อง (ที่สำหรับการเซฟไฟล์เสียงปรับที่ Script SavWav.cs)
    {
        SavWav.Save("Record.wav", _audioSource.clip);
        Debug.Log("Save");
    }

    private void deleteRecord() //ใช้สำหรับลบไฟล์เสียงที่อัดเมื่อใช้งานเสร็จแล้ว
    {
        string fileName = "Record.wav";
        // Get the full path to the file
        string filePath = Path.Combine(Application.streamingAssetsPath + "/Recordings/", fileName);
        
        // Check if the file exists
        if (File.Exists(filePath))
        {
            // Delete the file
            File.Delete(filePath);
            Debug.Log("File deleted: " + filePath);
        }
        else
        {
            Debug.LogWarning("File not found: " + filePath);
        }
    }
}
