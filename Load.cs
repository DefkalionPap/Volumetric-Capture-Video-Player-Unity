using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using GLTFast;
using System;
using UnityEditor;


public class Load : MonoBehaviour
{
    bool loaded = false;
    [SerializeField] List<Mesh> volCapMeshes = new List<Mesh>();
    [SerializeField] List<Texture> volCapTextures = new List<Texture>();
    int objectIndex = 0;
    int successCount = 0;
    bool over = false;
    bool hasComponents = false;
    List<string> filePath = new List<string>();
    public string folder;
    string path;
    [SerializeField] List<GameObject> sequence = new List<GameObject>();
    string temp;
    GameObject instObject;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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
        
        LoadGltfBinaryFromMemory();
        
    }

    // Update is called once per frame
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
                        bool success = await gltf.LoadGltfBinary(data, new Uri(filePath[objectIndex]));                 //The URI of the original data is important for resolving relative URIs within the glTF
                    
                        // Debug.Log(success);
                        if (success)
                            loaded = true;
                        successCount++;
                        Debug.Log("Video Loaded: " + successCount);
                        int meshCount;
                        Mesh[] meshes = gltf.GetMeshes();
                        meshCount = meshes.Length;
        
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

                        
                        if (objectIndex < sequence.Count - 1)
                            objectIndex++;
                        if (objectIndex == sequence.Count - 1)
                            over = true;
                    }
                    // Instantiates object
                    instObject = Instantiate(gameObject); // Creates prefab with meshes and textures
                    Destroy(instObject.GetComponent<Load>()); // Disables loader script
                    instObject.AddComponent<MeshRenderer>();
                    instObject.AddComponent<MeshFilter>();
                    instObject.AddComponent<VolumetricCapturePlayer>(); // Adds video player
                    instObject.GetComponent<VolumetricCapturePlayer>().Sequence = sequence;
                    instObject.GetComponent<VolumetricCapturePlayer>().SequenceMeshes = volCapMeshes;
                    instObject.GetComponent<VolumetricCapturePlayer>().SequenceTextures = volCapTextures;
                    //    instObject.GetComponent<VolumetricCapturePlayer>().Over = over;
                    //   instObject.GetComponent<VolumetricCapturePlayer>().Loaded = loaded;
                    string prefabPath = "Assets/Prefabs/" + instObject.name + ".prefab";
                    PrefabUtility.SaveAsPrefabAssetAndConnect(instObject, prefabPath, InteractionMode.UserAction);
                }
    
}
