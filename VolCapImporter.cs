using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using GLTFast;
using System;
public class VolCapImporter : MonoBehaviour
{
    #region Fields

    [SerializeField] private float FPS = 30;
    [SerializeField] private bool loop = true;
    bool loaded = false;
    int renderedFrames = 0;
    private float timer = 0;
    float frameDuration;
    private List<Mesh> volCapMeshes = new List<Mesh>();
    List<Texture> volCapTextures = new List<Texture>();
    int objectIndex = 0;
    int successCount = 0;
    bool over = false;
    bool hasComponents = false;
    List<string> filePath = new List<string>();

    #endregion

    public string folder;
    string path;
    [SerializeField] List<GameObject> sequence = new List<GameObject>();
    string temp;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        #region New Code - Load from folder

        path = Application.dataPath + "/Resources/" + folder;
        string[] files = Directory.GetFiles(path).Where(s => s.EndsWith(".glb")).ToArray();

        // Debug log
        for (int i = 0; i < files.Length; i++)
        {
           // Debug.Log(files[i]);
        }

        // Get files
        for (int i = 0; i < files.Length; i++)
        {
            temp = folder + "/" + Path.GetFileNameWithoutExtension(files[i]);
            //Debug.Log(temp);
            filePath.Add(files[i]);
            //Debug.Log(filePath[i]);
            sequence.Add(Resources.Load<GameObject>(temp));
        }

        #endregion

        gameObject.transform.rotation = Quaternion.Euler(-90, 0, 180);
        LoadGltfBinaryFromMemory();

    }

    #region Rest

    
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

            if (loop == true && renderedFrames == volCapMeshes.Count)
                renderedFrames = 0;
        }
    }
        
    async void LoadGltfBinaryFromMemory()
                {

                    while (objectIndex < sequence.Count - 1)
                    {
                        
                        byte[] data;
                        temp = filePath[objectIndex];
                        using (StreamReader streamReader = new StreamReader(temp))
                        {
 
                            using (MemoryStream memoryStream = new MemoryStream())
                            {
                                streamReader.BaseStream.CopyTo(memoryStream);
                                data = memoryStream.ToArray(); 
                            }
                        }

                        var gltf = new GltfImport();
                        bool success =
                            await gltf.LoadGltfBinary(data,
                                new Uri(filePath[
                                    objectIndex])); //The URI of the original data is important for resolving relative URIs within the glTF
                    

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
        
                        if (objectIndex < sequence.Count - 1)
                            objectIndex++;
                        if (objectIndex == sequence.Count - 1)
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
                gameObject.GetComponent<MeshRenderer>().material.shader =
                    Shader.Find("Universal Render Pipeline/Unlit");
                hasComponents = true;
            }
        }

    #endregion
    }
