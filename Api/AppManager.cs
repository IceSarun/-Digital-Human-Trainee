using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AppManager : MonoBehaviour //สำหรับจัดการทุก Script ในโปรเจค
{
    public MicrophoneManager microphone_manager;
    public SoundManager sound_manager;

    private void Start()
    {
        //สร้างโฟลเดอร์สำหรับเก็บไฟล์เสียง หากมีอยู่จะไม่ทำอะไร แต่หากไม่มีจะสร้างโฟลเดอร์ให้
        try 
        {
            if (!Directory.Exists(Application.streamingAssetsPath + "/Recordings/"))
            {
                Directory.CreateDirectory(Application.streamingAssetsPath + "/Recordings/");
            }

        }
        catch (IOException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
