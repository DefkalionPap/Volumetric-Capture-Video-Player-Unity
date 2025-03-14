using System.Collections.Generic;
using UnityEngine;
using System.IO;
using GLTFast;
using System;
using UnityEditor;

public class LoadMeshesAndTextures : MonoBehaviour
{
    int objectIndex = 0;
    bool over = false;
    bool loaded = false;
    private int successCount = 0;
    private bool loadedList = false;
    List<string> filePaths = new List<string>();
    private string temp;
    private string path;

    private List<GameObject> sequence = new List<GameObject>();
    [SerializeField] List<Texture> textures = new List<Texture>();
    [SerializeField] List<Mesh> meshes = new List<Mesh>();  
    
    
    public List<Texture> Textures { get { return textures; } }
    public List<Mesh> Meshes { get { return meshes; } }
    public bool Over { get { return over; } set { over = value; } }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sequence = gameObject.GetComponent<LoadData>().Sequence; 
        
        Debug.Log("Load Meshes And Textures is running.");
        Debug.Log(sequence.Count);
        
        loadedList = gameObject.GetComponent<LoadData>().LoadedList;
        
        sequence = gameObject.GetComponent<LoadData>().Sequence;
        
        // Get file paths
        for (int i = 0; i < sequence.Count; i++)
        {
            path = Application.dataPath + AssetDatabase.GetAssetPath(sequence[i]).Replace("Assets", "");                       // Replace(".glb", "")
            Debug.Log(path);
            filePaths.Add(path);
        } 
        
        
        if (loadedList)
            load();
    }

    async void load()
    {
        while (objectIndex < sequence.Count - 1 && over == false)
        {
                        
            byte[] data;
            temp = filePaths[objectIndex];
            using (StreamReader streamReader = new StreamReader(temp))
            {
 
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    streamReader.BaseStream.CopyTo(memoryStream);
                    data = memoryStream.ToArray(); 
                }
            }

            var gltf = new GltfImport();
            bool success = await gltf.LoadGltfBinary(data, new Uri(filePaths[objectIndex]));                 //The URI of the original data is important for resolving relative URIs within the glTF
                    
            // Debug.Log(success);
            if (success)
                loaded = true;
            successCount++;
            Debug.Log("Video Loaded: " + successCount);
            int meshCount;
            Mesh[] vcMeshes = gltf.GetMeshes();
            meshCount = vcMeshes.Length;
        
            //Load meshes
            for (int i = 0; i < meshCount; i++)
            {
                meshes.Add(vcMeshes[i]);
            }
        
            //Load textures
            for (int i = 0; i < gltf.TextureCount; i++)
            {
                textures.Add(gltf.GetTexture(i));
            }

                        
            if (objectIndex < sequence.Count - 1)
                objectIndex++;
            if (objectIndex >= sequence.Count - 1)
                over = true;
        }
    }
}
