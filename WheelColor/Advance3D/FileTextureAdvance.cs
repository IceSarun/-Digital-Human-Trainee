using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Security.AccessControl;

[System.Serializable]
public class TextureData
{
    public string objectName;  // เก็บชื่อ GameObject
    public string texturePath; // เก็บพาธ Texture (ถ้าว่างแปลว่าไม่มี Texture)
}

[System.Serializable]
public class TextureDataList
{
    public List<TextureData> textureDataList = new List<TextureData>();
}

public class FileTextureAdvance : MonoBehaviour
{
    public RawImage rawImage;
    public string path;
    [SerializeField]
    private GameObject objectForTest;
    private MeshRenderer objectRenderer;
    public bool checkDoneButton = false;

    private string saveFilePath;
    private List<GameObject> allObjects = new List<GameObject>(); // เก็บทุกวัตถุที่มี MeshRenderer

    public void Awake()
    {
        saveFilePath = Application.persistentDataPath + "/textureData.json";

        FindAllObjects();  // ค้นหาวัตถุทั้งหมดที่ต้องบันทึก
        LoadTextureData();
    }

    // ค้นหาวัตถุทั้งหมดในฉาก
    private void FindAllObjects()
    {
        // ดึง Transform ของลูกทั้งหมดใน iconParent
        MeshRenderer[] renderers = FindObjectsOfType<MeshRenderer>();
        foreach (var child in renderers)
        {
            if (child.gameObject.CompareTag("Selectable"))
            {
                allObjects.Add(child.gameObject);
            }

        }
    }

    public void OpenFileBrowser()
    {
        path = EditorUtility.OpenFilePanel("Select Image", "", "png,jpg");
        GetImage();
    }

    public void GetImage()
    {
        if (!string.IsNullOrEmpty(path))
        {
            UpdateImage();
        }
    }

    public void SetObject(GameObject sObject)
    {
        objectForTest = sObject;
        if (objectForTest != null)
        {
            objectRenderer = objectForTest.GetComponent<MeshRenderer>();
            if (objectRenderer != null && objectRenderer.material.mainTexture != null)
            {
                rawImage.texture = objectRenderer.material.mainTexture;
            }
        }
    }

    public void UpdateImage()
    {
        if (objectForTest != null)
        {
            StartCoroutine(LoadTextureCoroutine());
        }
    }

    private IEnumerator LoadTextureCoroutine()
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture("file:///" + path))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                rawImage.texture = texture;

                objectRenderer = objectForTest.GetComponent<MeshRenderer>();
                if (objectRenderer != null)
                {
                    objectRenderer.material.mainTexture = texture;
                    checkDoneButton = true;

                    //บันทึกข้อมูล Texture + ชื่อ Object
                    SaveTextureData();
                }
            }
            else
            {
                Debug.LogError("Error loading texture: " + request.error);
            }
        }
    }

    public bool getCheckUpload()
    {
        return checkDoneButton;
    }

    public void ResetCheckDoneButton()
    {
        checkDoneButton = false;
    }

    //บันทึกข้อมูลของทุกวัตถุ ไม่ใช่แค่ตัวที่เปลี่ยน Texture
    public void SaveTextureData()
    {
        TextureDataList dataList = new TextureDataList();

        //วนลูปทุกวัตถุแล้วบันทึก
        foreach (GameObject obj in allObjects)
        {
            MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
            string texturePaths = "";

            if (renderer != null && renderer.material.mainTexture != null)
            {
                if (obj == objectForTest)
                {
                    texturePaths = path;
                }
                else
                {
                    //ถ้าเป็นวัตถุอื่น ให้ใช้พาธปัจจุบันของ Texture (เฉพาะใน Editor)
                    #if UNITY_EDITOR
                    texturePaths = AssetDatabase.GetAssetPath(renderer.material.mainTexture);
                    #endif
                }
            }

            dataList.textureDataList.Add(new TextureData
            {
                objectName = obj.name,
                texturePath = texturePaths

            });
            Debug.Log(obj.name + texturePaths);
        }

        string json = JsonUtility.ToJson(dataList, true);
        File.WriteAllText(saveFilePath, json);

        Debug.Log("All texture data saved!");
    }

    // โหลดข้อมูลให้ทุกวัตถุ
    public void LoadTextureData()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            TextureDataList dataList = JsonUtility.FromJson<TextureDataList>(json);

            foreach (TextureData data in dataList.textureDataList)
            {
                GameObject obj = GameObject.Find(data.objectName);
                if (obj != null)
                {
                    MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
                    if (renderer != null)
                    {
                        if (!string.IsNullOrEmpty(data.texturePath))
                        {
                            StartCoroutine(ApplySavedTexture(renderer, data.texturePath));
                        }
                        else
                        {
                            Debug.Log($"No texture for {obj.name}, keeping default.");
                        }
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("No saved texture data found.");
        }
    }

    private IEnumerator ApplySavedTexture(MeshRenderer renderer, string texturePath)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture("file:///" + texturePath))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                renderer.material.mainTexture = texture;
                Debug.Log($"Loaded texture for {renderer.gameObject.name} from {texturePath}");
            }
            else
            {
                Debug.LogError("Error loading saved texture: " + request.error);
            }
        }
    }
}
