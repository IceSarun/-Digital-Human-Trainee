using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

public class FileManager : MonoBehaviour
{
    public RawImage rawImage;
    public string path;
    [SerializeField]
    private GameObject[] objectForTest;
    [SerializeField]
    private Texture texture;
    private Renderer objectRenderer;
    

    public void OpenFileBrowser() {
#if UNITY_EDITOR
        path = EditorUtility.OpenFilePanel("Overwrite with image", "", "png,jpg");
        GetImage();
#endif
    }

    public void GetImage() {

        if (path != null) {
            UpdateImage();
        }
    }

    public void UpdateImage()
    {
        WWW www = new WWW("file:///" + path);
        rawImage.texture = www.texture;
        for (int i =0; i< objectForTest.Length; i++) {
            objectRenderer = objectForTest[i].GetComponent<Renderer>();
            objectRenderer.material.mainTexture = rawImage.texture;
        }

    }

  
}
