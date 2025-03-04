using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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
        path = EditorUtility.OpenFilePanel("Overwrite with image", "", "png,jpg");
        GetImage();
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
