using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour //Script สำหรับควบคุมเสียง
{
    [SerializeField]
    private AudioSource buttonSound;
    [SerializeField]
    private AudioClip ON_sound;
    [SerializeField]
    private AudioClip OFF_sound;

    public AppManager appManager;


    //ใช้สำหรับเล่นเสียงตอนเปิดไมโครโฟน
    public void openMicSound()
    {
        buttonSound.PlayOneShot(ON_sound);
    }

    //ใช้สำหรับเล่นเสียงตอนปิดไมโครโฟน
    public void closeMicSound()
    {
        buttonSound.PlayOneShot(OFF_sound);
    }
}
