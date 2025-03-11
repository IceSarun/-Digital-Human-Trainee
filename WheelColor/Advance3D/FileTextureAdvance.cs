using System.Collections;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

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
#if UNITY_EDITOR
        path = EditorUtility.OpenFilePanel("Select Image", "", "png,jpg");
        GetImage();
#endif
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

        foreach (GameObject obj in allObjects)
        {
            MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
            string texturePaths = "";

            if (renderer != null && renderer.material.mainTexture != null)
            {
                if (obj == objectForTest && !string.IsNullOrEmpty(path))
                {
                    texturePaths = path;
                }
                else
                {
#if UNITY_EDITOR
                    string editorPath = AssetDatabase.GetAssetPath(renderer.material.mainTexture);
                    if (!string.IsNullOrEmpty(editorPath))
                    {
                        texturePaths = editorPath;
                    }
#endif
                }
            }

            // แก้ไข: ถ้า texturePaths เป็นค่าว่าง ให้เก็บค่าเดิมที่บันทึกไว้ก่อนหน้า
            if (string.IsNullOrEmpty(texturePaths) && File.Exists(saveFilePath))
            {
                string json = File.ReadAllText(saveFilePath);
                TextureDataList previousData = JsonUtility.FromJson<TextureDataList>(json);
                TextureData existingData = previousData.textureDataList.Find(x => x.objectName == obj.name);

                if (existingData != null)
                {
                    texturePaths = existingData.texturePath; // ใช้ค่าที่เคยบันทึกไว้
                }
            }

            dataList.textureDataList.Add(new TextureData
            {
                objectName = obj.name,
                texturePath = texturePaths
            });

            Debug.Log($"Saving: {obj.name} -> {texturePaths}");
        }
        string newJson = JsonUtility.ToJson(dataList, true);
        File.WriteAllText(saveFilePath, newJson);

        Debug.Log($"Saved JSON: {newJson}");
    }

    // โหลดข้อมูลให้ทุกวัตถุ
    public void LoadTextureData()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            Debug.Log($"Loaded JSON: {json}");
            TextureDataList dataList = JsonUtility.FromJson<TextureDataList>(json);

            foreach (TextureData data in dataList.textureDataList)
            {
                //Debug.Log("1");
                GameObject obj = GameObject.Find(data.objectName);
                if (obj != null)
                {
                    //Debug.Log("2");
                    MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
                    if (renderer != null)
                    {
                        //Debug.Log("3");
                        if (!string.IsNullOrEmpty(data.texturePath))
                        {
                            //Debug.Log(renderer.name + " " + data.texturePath);
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
        string fullPath = "file:///" + texturePath;
        Debug.Log("0");
        Debug.Log($"Loading texture for {renderer.gameObject.name} from {fullPath}");

        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(fullPath))
        {
            Debug.Log("Before yield return request.SendWebRequest()");
            // wait for second แปปนึง
            StartCoroutine(WaitForLoadTexture());
            yield return request.SendWebRequest();
            Debug.Log("After yield return request.SendWebRequest()");

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("1");
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                if (texture != null)
                {
                    Debug.Log("2");
                    renderer.material.mainTexture = texture;
                    Debug.Log($"Applied texture to {renderer.gameObject.name} from {texturePath}");
                }
                Debug.Log($"Loaded texture for {renderer.gameObject.name} from {texturePath}");
            }
            else
            {
                Debug.LogError("Error loading saved texture: " + request.error);
            }
        }
    }

    public void PickTextureFromPreset(GameObject button)
    {
        if (button != null)
        {
            // ดึง Image Component ของปุ่มที่ถูกกด
            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null && buttonImage.sprite != null)
            {
                // ดึง Texture2D จาก Sprite
                Texture2D selectedTexture = buttonImage.sprite.texture;

                if (objectForTest != null)
                {
                    MeshRenderer objectRenderer = objectForTest.GetComponent<MeshRenderer>();
                    if (objectRenderer != null)
                    {
                        // เปลี่ยน Texture ของวัตถุที่เลือก
                        objectRenderer.material.mainTexture = selectedTexture;

                        // อัปเดต UI หรือสถานะอื่น ๆ ถ้าจำเป็น
                        checkDoneButton = true;
                        SaveTextureData(); // บันทึกการเปลี่ยนแปลง
                    }
                }
            }
            else
            {
                Debug.LogWarning("Button does not have a valid Image with a Sprite.");
            }
        }
    }

    IEnumerator WaitForLoadTexture() {
        yield return new WaitForSeconds(15);
    }
}