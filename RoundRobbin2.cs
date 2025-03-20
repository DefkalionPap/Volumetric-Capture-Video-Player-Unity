using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using GLTFast;
public class RoundRobbin2 : MonoBehaviour
{
    
    #region Fields

    private int index;
    private byte[] data;
    private string[] sequence;
    private string fileUri;
    [SerializeField]private List<Mesh> firstMeshes;
    [SerializeField] List<Mesh> secondMeshes;
    [SerializeField] List<Texture> firstTextures;
    [SerializeField] List<Texture> secondTextures;
    bool call = false;

    #endregion
    
    #region Properties

    public int Index
    {
        get { return index; }
    }

    public List<Mesh> FirstMeshes
    {
        get { return firstMeshes; }
        set { firstMeshes = value; }
    }

    public List<Mesh> SecondMeshes
    {
        get { return secondMeshes; }
        set { secondMeshes = value; }
    }

    public List<Texture> FirstTextures
    {
        get { return firstTextures; }
        set { firstTextures = value; }
    }

    public List<Texture> SecondTextures
    {
        get { return secondTextures; }
        set { secondTextures = value; }
    }

    public bool Call
    {
        get { return call; }
        set { call = value; }
    }
    #endregion
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameObject.transform.rotation = Quaternion.Euler(-90, 0, 270);
        gameObject.transform.localScale = gameObject.transform.localScale / 4;
        
        #region Get Sequence Paths

        BetterStreamingAssets.Initialize();
        sequence = BetterStreamingAssets.GetFiles("/", "*.glb", SearchOption.AllDirectories);
        Debug.Log("Video Count is " + sequence.Length);

        #endregion

        #region Loads first and second videos' meshes

        for (int i = 0; i < 2; i++)
        {
            index = i;
            fileUri = Application.streamingAssetsPath + "/" + sequence[index];                                            // Absolute path
            data = BetterStreamingAssets.ReadAllBytes(sequence[index]);
            LoadGltfBinaryFromMemory(fileUri, data, index);
            //Load gltf first and second frame
        }

        #endregion

    }

    // Update is called once per frame
    void Update()
    {
        
        if (gameObject.GetComponent<videoPlayer2>().Loaded == false && index > 1 && index < sequence.Length && call == true)
        { 
            call = false;
            Debug.Log("Conditions are met");
            Debug.Log("Updating Textures and meshes");
            firstMeshes.Clear();
            firstTextures.Clear();
            for (int i = 0; i < secondMeshes.Count; i++)
            {
                firstMeshes.Add(secondMeshes[i]);
                firstTextures.Add(secondTextures[i]);
            }
            gameObject.GetComponent<videoPlayer2>().Meshes = firstMeshes;
            gameObject.GetComponent<videoPlayer2>().Textures = firstTextures;
            fileUri = Application.streamingAssetsPath + "/" + sequence[index];                                            // Absolute path
            data = BetterStreamingAssets.ReadAllBytes(sequence[index]);
            LoadGltfBinaryFromMemory(fileUri, data, index);
            
        }

        if (gameObject.GetComponent<videoPlayer2>().Loaded == false && index == sequence.Length && call == true)         //Loop
        {
            #region Loads first and second videos' meshes

            for (int i = 0; i < 2; i++)
            {
                index = i;
                fileUri = Application.streamingAssetsPath + "/" + sequence[index];                                            // Absolute path
                data = BetterStreamingAssets.ReadAllBytes(sequence[index]);
                LoadGltfBinaryFromMemory(fileUri, data, index);
                //Load gltf first and second frame
            }

            #endregion
        }

    }

    async void LoadGltfBinaryFromMemory(string fileUri, byte[] data, int index2)
    {
        #region Loads glTF or GLB file

        var gltf = new GltfImport();
        bool success = await gltf.LoadGltfBinary(
            data,
            // The URI of the original data is important for resolving relative URIs within the glTF
            new Uri(fileUri)
        );

        #endregion
        
        if (success)
        {
            gameObject.GetComponent<videoPlayer2>().Loaded = true;
            
            #region Shows if video is loaded and loads meshes

            Debug.Log("Video loaded " + index);
            int meshCount;
            Mesh[] vcMeshes = gltf.GetMeshes();
            meshCount = vcMeshes.Length;

            for (int i = 0; i < gltf.TextureCount; i++)
            {
                List<Texture> textures = new List<Texture>();
                textures.Add(gltf.GetTexture(i));
            }
            #endregion
            
            #region Sets first video's meshes and textures

            if (index2 == 0)
            {
                for (int i = 0; i < meshCount; i++)
                {
                    firstMeshes.Add(vcMeshes[i]);
                    firstTextures.Add(gltf.GetTexture(i));
                }
            }
            gameObject.GetComponent<videoPlayer2>().Meshes = firstMeshes;
            gameObject.GetComponent<videoPlayer2>().Textures = firstTextures;
            
            #endregion

            #region Sets second video's meshes and textures

            if (index2 == 1)
            {
                
                for (int i = 0; i < meshCount; i++)
                {
                    secondMeshes.Add(vcMeshes[i]);
                    secondTextures.Add(gltf.GetTexture(i));
                }
                gameObject.GetComponent<videoPlayer2>().Loaded = true;
                index++;

            }
            
            #endregion
            
            
            #region Updates first and second lists

            if (index > 1)
            {
                
                secondMeshes.Clear();
                secondTextures.Clear();
                for (int i = 0; i < meshCount; i++)
                {
                    secondMeshes.Add(vcMeshes[i]);
                    secondTextures.Add(gltf.GetTexture(i));
                }
                
                index++;
                
            }

            #endregion
            
            

        }
    }
}
