using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using UnityEngine;
using GLTFast;
using System.Linq;
using UnityEngine.Networking;
using Better.StreamingAssets;

public class Load : MonoBehaviour
{
    private GameObject tempObj;
    [SerializeField] string[] sequence;
 // [SerializeField] string[] absPath;
    List<Mesh> meshes = new List<Mesh>();
    List<Texture> textures = new List<Texture>();
    private bool over = false;
    bool last = false;
    private bool loadVideo = false;
    
    public string[] Sequence { get => sequence; set => sequence = value; }
    public List<Mesh> Meshes { get => meshes; set => meshes = value; }
    public List<Texture> Textures { get => textures; set => textures = value; }
    void Start()
    {
        
        tempObj = GameObject.FindGameObjectWithTag("Video");
        tempObj.name = "Volumetric Capture Video";
        tempObj.transform.position = Vector3.zero;
        tempObj.transform.rotation = Quaternion.Euler(-90,0,0);
        tempObj.transform.localScale = tempObj.transform.localScale / 4;
        
        
        
        
        
        
        BetterStreamingAssets.Initialize();
        sequence = BetterStreamingAssets.GetFiles("/", "*.glb", SearchOption.AllDirectories);
        
        Debug.Log(Application.streamingAssetsPath + "/" + sequence[0]);
        
        byte[] data;

        for (int i = 0; i < sequence.Length; i++)
        {
            if (i == sequence.Length - 1)
            {
                last = true;
            }
            data = BetterStreamingAssets.ReadAllBytes(sequence[i]);
            LoadGltfBinaryFromMemory(Application.streamingAssetsPath + "/" + sequence[i], data, last);
        }
        
    }

    void Update()
    {
        if (last && loadVideo)
        {
            
            //tempObj.AddComponent<MeshFilter>();
            //tempObj.AddComponent<MeshRenderer>();
            tempObj.AddComponent<VolumetricCapturePlayer>();
            tempObj.GetComponent<VolumetricCapturePlayer>().Meshes = meshes;
            tempObj.GetComponent<VolumetricCapturePlayer>().Textures = textures;
            tempObj.GetComponent<VolumetricCapturePlayer>().Over = over;
            loadVideo = false;
        }
    }
    
    async void LoadGltfBinaryFromMemory(string fileUri, byte[] data, bool last)
    {
        var gltf = new GltfImport();
        bool success = await gltf.LoadGltfBinary(
            data, 
            // The URI of the original data is important for resolving relative URIs within the glTF
            new Uri(fileUri)
        );
        if (success)
        {
            Debug.Log("Video loaded");
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
            over = true;
        }

        if (last)
            loadVideo = true;

    }

   
}
