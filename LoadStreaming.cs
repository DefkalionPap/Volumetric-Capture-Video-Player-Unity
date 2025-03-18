using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using UnityEngine;
using GLTFast;
using System.Linq;
using UnityEngine.Networking;
using Better.StreamingAssets;

public class LoadStreaming : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // #if UNITY_ANDROID
        // Include "jar:file://"

        BetterStreamingAssets.Initialize();
        string[] glb = BetterStreamingAssets.GetFiles("/", "*.glb", SearchOption.AllDirectories);
        string absPath = Application.streamingAssetsPath + "/" + glb[0];
        
        Debug.Log(Application.streamingAssetsPath + "/" + glb[0]);
        byte[] data = BetterStreamingAssets.ReadAllBytes(glb[0]);
        
        LoadGltfBinaryFromMemory(absPath, data);   
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
           // GameObject tempObj = Instantiate(new GameObject());
           
            gameObject.AddComponent<MeshFilter>().mesh = meshes[0];
            gameObject.AddComponent<MeshRenderer>().material.mainTexture = textures[0];
            gameObject.transform.position = new Vector3(0, 0, 0);
            gameObject.transform.rotation = Quaternion.Euler(-90, 0, 0);
            gameObject.transform.localScale = new Vector3(1, -1, 1);
            gameObject.transform.localScale = gameObject.transform.localScale / 4;
            //gameObject.GetComponent<MeshRenderer>().material.shader = Shader.Find("Universal Render Pipeline/Unlit");
        }
    }

   
}
