using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class NvmParser : MonoBehaviour
{
    public string nvmFilePath; // NVM dosyasının yolu
    public GameObject cameraPrefab; // Camera Prefab'i Unity Inspector'dan atayın

    void Start()
    {
        if (string.IsNullOrEmpty(nvmFilePath))
        {
            Debug.LogError("NVM file path is not set.");
            return;
        }

        ParseNvmFile(nvmFilePath);
    }

    void ParseNvmFile(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError($"NVM file not found at path: {path}");
            return;
        }

        string[] lines = File.ReadAllLines(path);

        // NVM Dosyasının Minimum Satır Kontrolü
        if (lines.Length < 2)
        {
            Debug.LogError("NVM file is invalid or too short.");
            return;
        }

        // Kamera Sayısını Oku
        string cameraCountLine = lines[1].Trim();
        if (string.IsNullOrEmpty(cameraCountLine))
        {
            Debug.LogError("Camera count line is empty or invalid.");
            return;
        }

        if (!int.TryParse(cameraCountLine, out int cameraCount))
        {
            Debug.LogError($"Failed to parse camera count. Line content: {cameraCountLine}");
            return;
        }

        Debug.Log($"Total Cameras: {cameraCount}");

        // Çarpan ile pozisyon ölçekleme
        const float scaleFactor = 0.00001f;

        for (int i = 2; i < 2 + cameraCount; i++)
        {
            if (i >= lines.Length || string.IsNullOrWhiteSpace(lines[i]))
            {
                Debug.LogWarning($"Skipping empty or invalid line: {i}");
                continue;
            }

            string[] parts = lines[i].Split(new char[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 9)
            {
                Debug.LogWarning($"Invalid camera line format at line {i}: {lines[i]}");
                continue;
            }

            // Kamera Pozisyonu ve Rotasyonu
            string cameraName = parts[0];
            Vector3 position = new Vector3(
                float.Parse(parts[2]) * scaleFactor, // Çarpan ile küçült
                float.Parse(parts[3]) * scaleFactor,
                float.Parse(parts[4]) * scaleFactor
            );

            Quaternion rotation = new Quaternion(
                float.Parse(parts[5]),
                float.Parse(parts[6]),
                float.Parse(parts[7]),
                float.Parse(parts[8])
            );

            Debug.Log($"Camera {cameraName}: Position {position}, Rotation {rotation}");

            CreateCamera(cameraName, position, rotation);
        }
    }

    void CreateCamera(string cameraName, Vector3 position, Quaternion rotation)
    {
        if (cameraPrefab == null)
        {
            Debug.LogError("Camera prefab is not assigned!");
            return;
        }

        // Normalize et ve sahne ölçeğine uyarla
        Vector3 normalizedPosition = position * 0.01f; // Pozisyon ölçeklendirme
        Quaternion normalizedRotation = Quaternion.Normalize(rotation); // Rotasyon normalize

        // Prefab'ı sahneye instantiate edin
        GameObject cameraObj = Instantiate(cameraPrefab);
        cameraObj.name = cameraName;

        // Normalize edilmiş pozisyon ve rotasyonu ayarla
        cameraObj.transform.position = normalizedPosition;
        cameraObj.transform.rotation = normalizedRotation;

        AssignImageToCamera(cameraObj, cameraName);

        Debug.Log($"{cameraName} created at Position {normalizedPosition} with Rotation {normalizedRotation}");
    }

    void AssignImageToCamera(GameObject cameraObj, string imageName)
    {
        // Plane (düzlem) oluştur ve kameranın önüne yerleştir
        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.transform.SetParent(cameraObj.transform); // Kameraya bağlı hale getir
        plane.transform.localPosition = new Vector3(0, 0, 2); // Kameradan 2 birim uzaklıkta
        plane.transform.localRotation = Quaternion.identity;
        plane.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f); // Ölçek

        // Resmi yükle ve düzleme uygula
        string imagePath = $"Resources/{imageName}";
        Material material = new Material(Shader.Find("Unlit/Texture"));
        Texture2D texture = LoadTexture(imagePath);
        if (texture != null)
        {
            material.mainTexture = texture;
            plane.GetComponent<Renderer>().material = material;
        }
        else
        {
            Debug.LogError($"Failed to load texture: {imagePath}");
        }
    }

    Texture2D LoadTexture(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError($"Image file not found: {path}");
            return null;
        }

        byte[] fileData = File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2, 2);
        if (texture.LoadImage(fileData))
        {
            return texture;
        }
        return null;
    }


}
