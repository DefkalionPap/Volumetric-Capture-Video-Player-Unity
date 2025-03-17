using System.Collections.Generic;
using UnityEngine;
using System.IO;
using GLTFast;
using System;
using UnityEditor;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class LoadMeshesAndTexturesAddressables : MonoBehaviour
{
    int objectIndex = 0;
    bool over = false;
    bool loaded = false;
    private int successCount = 0;
    private bool loadedList = false;
    List<string> filePaths = new List<string>();
    private string temp;
    private string path;
    private string prefabPath;

    [SerializeField] List<Texture> textures = new List<Texture>();
    [SerializeField] List<Mesh> meshes = new List<Mesh>();  
    

    private AsyncOperationHandle<GameObject> video;
    
    public List<Texture> Textures { get { return textures; } }
    public List<Mesh> Meshes { get { return meshes; } }
    public bool Over { get { return over; } set { over = value; } }
    public String PrefabPath { get { return prefabPath; } set { prefabPath = value; } }
    
    [SerializeField] private string m_Address;
    private AsyncOperationHandle<GameObject> m_Video;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_Address = "Video";
        m_Video = Addressables.LoadAssetAsync<GameObject>(m_Address);
        m_Video.Completed += OnVideoLoadComplete;
        OnVideoLoadComplete(m_Video);
    }

    

    private void OnDisable()
    {
       m_Video.Completed -= OnVideoLoadComplete;
    }

    private void OnVideoLoadComplete(AsyncOperationHandle<GameObject> asyncOperationHandle)
    {
        if (asyncOperationHandle.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log(Application.dataPath + "/Assets/Videos/peruk-24-1/" + m_Video.Result.ToString().Replace("(UnityEngine.GameObject)", "").Replace(" ", "") + ".glb");
            path = Application.dataPath.Replace("/Assets", "") + "/" + Addressables.LoadResourceLocationsAsync(m_Address).Result[0];
            Debug.Log(path);
            load(); 
        }
            
    }

    async void load()
    {
        Debug.Log("Load is running");
            byte[] data;
            temp = path;
            using (StreamReader streamReader = new StreamReader(temp))
            {
 
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    streamReader.BaseStream.CopyTo(memoryStream);
                    data = memoryStream.ToArray(); 
                }
            }

            var gltf = new GltfImport();
            bool success = await gltf.LoadGltfBinary(data, new Uri(temp));                 //The URI of the original data is important for resolving relative URIs within the glTF
                    
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

            //Set first texture and mesh
            gameObject.AddComponent<MeshFilter>().mesh = meshes[0];
            gameObject.AddComponent<MeshRenderer>().material.mainTexture = textures[0];
            gameObject.transform.position = new Vector3(0, 0.46f, 0);
            gameObject.transform.rotation = Quaternion.Euler(-90, 0, 0);
            gameObject.transform.localScale = new Vector3(1, -1, 1);
            gameObject.GetComponent<MeshRenderer>().material.shader = Shader.Find("Universal Render Pipeline/Unlit");
        
    }
}
