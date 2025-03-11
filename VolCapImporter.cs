using UnityEngine;
using System.Collections;
using GLTFast;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class glTFList : MonoBehaviour
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
    int successCount = 0;
    bool over = false;
    bool hasComponents = false;
    #endregion

    #region Methods
    
    void Start()
    {
        
        gameObject.transform.rotation = Quaternion.Euler( -90, 0, 180 );
        
        //Load File
        LoadGltfBinaryFromMemory();
    }

    public void FixedUpdate() 
    {
        if (over)
        {
            timer += Time.fixedDeltaTime;
            frameDuration = 1f / FPS; //AI assisted line
        
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

            if (loop = true && renderedFrames == volCapMeshes.Count)
                renderedFrames = 0;
        }
        
        
    }
    
    async void LoadGltfBinaryFromMemory() {

        while (objectIndex < glTFObjects.Count - 1)
        {
            glbObject = glTFObjects[objectIndex];
                    path = Application.dataPath + AssetDatabase.GetAssetPath( glbObject ).Replace( "Assets", "" );                     // Gets asset's absolute path
                 //   Debug.Log( path );
                    
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
            
                    #region Debug
            
                   // Debug.Log(success);
                    if (success)   
                        loaded = true;
                    successCount++;
                    Debug.Log("Video Loaded: " + successCount);
                    int meshCount;
                    Mesh[] meshes = gltf.GetMeshes();
                    meshCount = meshes.Length;
                   
            
                    #endregion
                    // Show values on console
            
                    #region Load Meshes and Textures
            
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
            
                    #endregion
                    
                    if (objectIndex < glTFObjects.Count - 1)
                        objectIndex++;
                    if (objectIndex == glTFObjects.Count - 1)
                        over = true;
        }
        
    }

    public void AddComponents()
    {
        if (hasComponents == false)
        {
            //Add components
            gameObject.AddComponent<MeshFilter>();
            gameObject.AddComponent<MeshRenderer>();
            gameObject.GetComponent<MeshRenderer>().material.shader = Shader.Find("Universal Render Pipeline/Unlit");
            hasComponents = true;
        }
    }

    #endregion
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    

    
   
}
