using UnityEngine;
using System.Collections;
using GLTFast;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
public class VolCapImporter : MonoBehaviour
{
    [SerializeField] GameObject glbObject;
    [SerializeField] private float FPS = 30;
    [SerializeField] private bool loop = true;
    bool loaded = false;
    int renderedFrames = 0;
    private List<Mesh> volCapMeshes = new List<Mesh>() ;
    List<Texture> volCapTextures = new List<Texture>();
    string path;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        path = Application.dataPath + AssetDatabase.GetAssetPath( glbObject ).Replace( "Assets", "" );                     // Gets asset's absolute path
        Debug.Log( path );
        gameObject.transform.rotation = Quaternion.Euler( -90, 0, 180 );
        
        //Load File
        LoadGltfBinaryFromMemory();
    }
    
    public void FixedUpdate()
    {
        if (loaded)
        {
            if (renderedFrames < volCapMeshes.Count - 1)
            {
                for (int i = 0; i < FPS; i++)
                {
                    gameObject.GetComponent<MeshFilter>().mesh = volCapMeshes[renderedFrames];
                    gameObject.GetComponent<Renderer>().material.mainTexture = volCapTextures[renderedFrames];
                    Debug.Log("Rendered Frames: " + renderedFrames);
                    renderedFrames++;
                }
            }
            else if (renderedFrames == volCapMeshes.Count - 1 && loop == true)                //Loops video
            {
                
                renderedFrames = 0;
            }
        }
    }
    
    async void LoadGltfBinaryFromMemory() {
        
        // Code from documentation starts here
        var filePath = path;
        byte[] data = File.ReadAllBytes(filePath);
        var gltf = new GltfImport();
        bool success = await gltf.LoadGltfBinary(
            data,
            // The URI of the original data is important for resolving relative URIs within the glTF
            new Uri(filePath)
        );
        // Code from documentation ends here
        
        // Show values on console
        Debug.Log(success);
        if (success)   
            loaded = true;
        
        Debug.Log("Scene Count: " + gltf.SceneCount);
        Debug.Log("Material Count: " + gltf.MaterialCount);
        Debug.Log("Texture Count: " + gltf.TextureCount);
        Debug.Log("Image Count: " + gltf.ImageCount);
        int meshCount;
        Mesh[] meshes = gltf.GetMeshes();
        meshCount = meshes.Length;
        Debug.Log("Mesh Count: " + meshCount);
        
        //Load meshes
        for (int i = 0; i < meshCount; i++)
        {
            volCapMeshes.Add(meshes[i]);
        }
        
        //Load textures
        for (int i = 0; i < gltf.TextureCount; i++)
        {
            volCapTextures.Add(gltf.GetTexture(i));
        }
        
       //Add components
       gameObject.AddComponent<MeshFilter>();
       gameObject.AddComponent<MeshRenderer>();
       
       //Set first mesh and texture
       gameObject.GetComponent<MeshFilter>().mesh = volCapMeshes[0];
       gameObject.GetComponent<Renderer>().material.mainTexture = volCapTextures[0];
       gameObject.GetComponent<Renderer>().material.shader = Shader.Find("Universal Render Pipeline/Unlit");
       
       //Update Mesh
       //Update Texture

    }

    
   
}
