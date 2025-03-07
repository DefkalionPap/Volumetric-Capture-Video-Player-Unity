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
    private float timer = 0;
    float frameDuration;
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
        timer += Time.fixedDeltaTime;
        frameDuration = 1f / FPS; //AI Assisted
        if (timer >= frameDuration && loaded)
        {
            if (renderedFrames < volCapMeshes.Count)
            {
                MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
                Renderer renderer = gameObject.GetComponent<Renderer>();
                meshFilter.mesh = volCapMeshes[renderedFrames];
                renderer.material.mainTexture = volCapTextures[renderedFrames];
                renderedFrames++;
                timer -= frameDuration; // Reset timer and Keep the overflow to carry forward the remainder - AI Assisted 
            }
            else if (renderedFrames >= volCapMeshes.Count && loop)
            {
                renderedFrames = 0;
            }
        }
    }
    
    async void LoadGltfBinaryFromMemory() {
        
        byte[] data = File.ReadAllBytes(path);
        var gltf = new GltfImport();
        bool success = await gltf.LoadGltfBinary(
            data,
            // The URI of the original data is important for resolving relative URIs within the glTF
            new Uri(path)
        );
        
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
