using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using UnityEngine;
using GLTFast;
using System.Linq;
using UnityEngine.Networking;

public class GrabUri : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Hello!");
        
        
       // #if UNITY_ANDROID
        string[] glb = Directory.GetFiles(Application.streamingAssetsPath).ToArray(); // Include "jar:file://"
        byte[] dataA = File.ReadAllBytes(glb[0]);                   //A for Android
        if (glb.Length != 0)
        {
            gameObject.GetComponent<MeshRenderer>().material.color = Color.green;
        }
        LoadGltfBinaryFromMemory(glb[0], dataA);  
        //StartCoroutine(GetRequest(glb[0]));
      //  #endif
        
       // #if UNITY_EDITOR
       // 
        //Debug.Log(glb[0]);
       // byte[] data = File.ReadAllBytes(glb[0]);
       // LoadGltfBinaryFromMemory(glb[0], data);
       // #endif
      
        
    }
     
    IEnumerator GetRequest(string uri)
    { 
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            
            yield return webRequest.SendWebRequest();

            gameObject.GetComponent<MeshRenderer>().material.color = Color.blue;
        
            string fileUri = uri;
            Debug.Log("File Uri: " + fileUri);
            byte[] data = webRequest.downloadHandler.data;
            if (data.Length != 0)
            {
                gameObject.GetComponent<MeshRenderer>().material.color = Color.green;
            }
            LoadGltfBinaryFromMemory(fileUri, data);
        }
    }
    
    async void LoadGltfBinaryFromMemory(string fileUri, byte[] data)
    {
        var gltf = new GltfImport();
        bool success = await gltf.LoadGltfBinary(
            data, 
            // The URI of the original data is important for resolving relative URIs within the glTF
            new Uri(fileUri)
        );
        if (success) {
            success = await gltf.InstantiateMainSceneAsync(transform);
            Debug.Log("Texture Count: " + gltf.TextureCount);
            
            
            List<Mesh> meshes = new List<Mesh>();
            List<Texture> textures = new List<Texture>();
            
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
            GameObject tempObj = Instantiate(new GameObject());
            tempObj.AddComponent<MeshFilter>().mesh = meshes[0];
            tempObj.AddComponent<MeshRenderer>().material.mainTexture = textures[0];
            tempObj.transform.position = new Vector3(0, 0, 0);
            tempObj.transform.rotation = Quaternion.Euler(-90, 0, 0);
            tempObj.transform.localScale = new Vector3(1, -1, 1);
            tempObj.transform.localScale = tempObj.transform.localScale / 4;
            tempObj.GetComponent<MeshRenderer>().material.shader = Shader.Find("Universal Render Pipeline/Unlit");
        }
    }

   
}
