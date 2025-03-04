using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour //Script ����Ѻ�Ǻ������§
{
    [SerializeField]
    private AudioSource buttonSound;
    [SerializeField]
    private AudioClip ON_sound;
    [SerializeField]
    private AudioClip OFF_sound;

    public AppManager appManager;


    //������Ѻ������§�͹�Դ����⿹
    public void openMicSound()
    {
        buttonSound.PlayOneShot(ON_sound);
    }

    //������Ѻ������§�͹�Դ����⿹
    public void closeMicSound()
    {
        buttonSound.PlayOneShot(OFF_sound);
    }
}
