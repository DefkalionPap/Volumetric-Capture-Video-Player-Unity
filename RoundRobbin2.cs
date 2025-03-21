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
    private int playList = -1;

    private bool loadingOver;

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
    public int Playlist { get { return playList; } set { playList = value; } }
    #endregion
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameObject.transform.rotation = Quaternion.Euler(-90, 0, 270);
        gameObject.transform.localScale = gameObject.transform.localScale / 4;
        
        #region Get Sequence Paths

        BetterStreamingAssets.Initialize();
        sequence = BetterStreamingAssets.GetFiles("/", "*.glb", SearchOption.AllDirectories);

        #endregion

        #region Loads first video's meshes and textures
        index = 0;
        fileUri = Application.streamingAssetsPath + "/" + sequence[index];
        LoadGltfBinaryFromMemory(fileUri, index);
        #endregion

    }

    // Update is called once per frame
    void Update()
    {
        if (call && gameObject.GetComponent<videoPlayer2>().Loaded == false)
        { 
            //playList *= -1;
            if (playList == 1)
            {
                Debug.Log("Entered Update");
                gameObject.GetComponent<videoPlayer2>().Meshes = firstMeshes;
                gameObject.GetComponent<videoPlayer2>().Textures = firstTextures;
                gameObject.GetComponent<videoPlayer2>().Loaded = true;
                index++;
                fileUri = Application.streamingAssetsPath + "/" + sequence[index];
                LoadGltfBinaryFromMemory(fileUri, index);
                call = false;
            }

            if (playList == -1)
            {
                Debug.Log("Entered Update");
                gameObject.GetComponent<videoPlayer2>().Meshes = secondMeshes;
                gameObject.GetComponent<videoPlayer2>().Textures = secondTextures;
                gameObject.GetComponent<videoPlayer2>().Loaded = true;
                index++;
                fileUri = Application.streamingAssetsPath + "/" + sequence[index];
                LoadGltfBinaryFromMemory(fileUri, index);
                call = false;
            }
            
        }
    }

    async void LoadGltfBinaryFromMemory(string fileUri, int index2)
    {
        #region Loads glTF or GLB file

        data = BetterStreamingAssets.ReadAllBytes(sequence[index2]);
        var gltf = new GltfImport();
        bool success = await gltf.LoadGltfBinary(data, new Uri(fileUri));

        #endregion
        
        if (success)
        {
            Debug.Log("Loading video: " + index2);
            
            #region Shows if video is loaded and loads meshes

            int meshCount;
            Mesh[] vcMeshes = gltf.GetMeshes();
            meshCount = vcMeshes.Length;

            List<Texture> textures = new List<Texture>();
            for (int i = 0; i < gltf.TextureCount; i++)
            {
                textures.Add(gltf.GetTexture(i));
            }
            #endregion
            
            #region Sets first and second videos' meshes and textures

            if (index2 == 0)
            {
                for (int i = 0; i < meshCount; i++)
                {
                    firstMeshes.Add(vcMeshes[i]);
                    firstTextures.Add(gltf.GetTexture(i));
                }
                call = true;
            }

            if (index2 == 1)
            {
                for (int i = 0; i < meshCount; i++)
                {
                    secondMeshes.Add(vcMeshes[i]);
                    secondTextures.Add(gltf.GetTexture(i));
                }
            }
            #endregion
            
            #region Updates other list

            if (index2 > 1)
            {
                if (playList == 1)
                {
                    secondMeshes.Clear();
                    secondTextures.Clear();
                    for (int i = 0; i < meshCount; i++)
                    {
                        secondMeshes.Add(vcMeshes[i]);
                        secondTextures.Add(gltf.GetTexture(i));
                    }
                    
                }
                else if (playList == -1)
                {
                    firstMeshes.Clear();
                    firstTextures.Clear();
                    for (int i = 0; i < meshCount; i++)
                    {
                        firstMeshes.Add(vcMeshes[i]);
                        firstTextures.Add(gltf.GetTexture(i));
                    }
                }
            }
            #endregion
        }
    }
}
