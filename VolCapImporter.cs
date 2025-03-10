using UnityEngine;
using System.Collections;
using GLTFast;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class VolCapImporter : MonoBehaviour
{
    
    #region Fields
    GameObject glbObject;
    [SerializeField] private float FPS = 30;
    [SerializeField] private bool loop = true;
    bool loaded = false;
    int renderedFrames = 0;
    private float timer = 0;
    float frameDuration;
    private List<Mesh> volCapMeshes = new List<Mesh>() ;
    List<Texture> volCapTextures = new List<Texture>();
    [SerializeField] List<GameObject> glTFObjects;
    string path;
    int objectIndex = 0;
    #endregion

    #region Methods
    
    void Start()
    {
        glbObject = glTFObjects[objectIndex];
        path = Application.dataPath + AssetDatabase.GetAssetPath( glbObject ).Replace( "Assets", "" );                     // Gets asset's absolute path
        Debug.Log( path );
        gameObject.transform.rotation = Quaternion.Euler( -90, 0, 180 );
        
        //Load File
        LoadGltfBinaryFromMemory();
    }

    public void FixedUpdate() 
    { 
        timer += Time.fixedDeltaTime;
        frameDuration = 1f / FPS; //AI assisted line
        
        //if (renderedFrames >= volCapMeshes.Count && !loop && objectIndex < glTFObjects.Count - 1)
       // {
            //objectIndex++;
            //glbObject = glTFObjects[objectIndex];
            //path = Application.dataPath + AssetDatabase.GetAssetPath( glbObject ).Replace( "Assets", "" );                     // Gets asset's absolute path
            //Debug.Log( path );
            //gameObject.transform.rotation = Quaternion.Euler( -90, 0, 180 );
            // //Load File
            //LoadGltfBinaryFromMemory();
      //  }
        if (timer >= frameDuration && loaded)
        {
            if (renderedFrames < volCapMeshes.Count) //AI assisted line
            {
                MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
                Renderer renderer = gameObject.GetComponent<Renderer>();
                meshFilter.mesh = volCapMeshes[renderedFrames];
                renderer.material.mainTexture = volCapTextures[renderedFrames];
                renderedFrames++;
                timer -= frameDuration; // Reset timer and Keep the overflow to carry forward the remainder - AI assisted line
            }
        }
    }
    
    async void LoadGltfBinaryFromMemory() {
        
        byte[] data;
        using (StreamReader streamReader = new StreamReader(path))
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                streamReader.BaseStream.CopyTo(memoryStream);
                data = memoryStream.ToArray();
            }
        }
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
        
        AddComponents();
       
    }

    public void AddComponents()
    {
        //Add components
        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();
        gameObject.GetComponent<MeshRenderer>().material.shader = Shader.Find("Universal Render Pipeline/Unlit");
    }

    #endregion
}
