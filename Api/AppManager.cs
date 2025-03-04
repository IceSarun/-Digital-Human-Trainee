using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AppManager : MonoBehaviour //����Ѻ�Ѵ��÷ء Script ���ਤ
{
    public MicrophoneManager microphone_manager;
    public SoundManager sound_manager;

    private void Start()
    {
        //���ҧ����������Ѻ��������§ �ҡ��������������� ���ҡ����ը����ҧ���������
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
