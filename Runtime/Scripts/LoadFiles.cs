using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Linq;
using GLTFast;
using System.Threading.Tasks;
using Unity.Collections.LowLevel.Unsafe;

public class LoadFiles : MonoBehaviour
{
    #region Fields

    private int meshCount;
    private int index;
    private byte[] data;
    string[] sequence;
    private string fileUri;
    [SerializeField]private List<Mesh> firstMeshes;
    [SerializeField] List<Mesh> secondMeshes;
    [SerializeField] List<Texture> firstTextures;
    [SerializeField] List<Texture> secondTextures;
    private int playList = -1;
    bool complete = true;

    #endregion
    bool listsInitialized = false;
    List<GltfImport> gltfImports = new List<GltfImport>();
    

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

    public int Playlist
    {
        get { return playList; }
        set { playList = value; }
    }
    
    #endregion
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        BetterStreamingAssets.Initialize();
        sequence = BetterStreamingAssets.GetFiles("/", "*.glb" , SearchOption.AllDirectories);
        if (sequence.Length == 0)
        {
            Debug.Log("No glb files found");
        }
            
        if (sequence.Length == 1)
        {
            Debug.Log("Playing single glb file");
            await LoadSingleVideo();
        }
            
        if (sequence.Length > 1)
        {
            Debug.Log("Playing multiple gltf files");
            await InitializeLists();
            listsInitialized = true;
        }
            
    }

    // Update is called once per frame
    async void Update()

    #region MyRegion
    /*
     * if (index >= sequence.Length && gameObject.GetComponent<VideoPlayer>().Loop == false)
       {

       }

       if (sequence.Length == 1 && gameObject.GetComponent<VideoPlayer>().Loop && gameObject.GetComponent<VideoPlayer>().Loaded == false)
       {
           gameObject.GetComponent<VideoPlayer>().RenderedFrames = 0;
           gameObject.GetComponent<VideoPlayer>().Loaded = true;


       }
     */
        #endregion
    
    {
        if (index == sequence.Length  && !gameObject.GetComponent<VideoPlayer>().Loop)
        {
           gameObject.GetComponent<VideoPlayer>().Loaded = true;
        }
        if (sequence.Length > 1)                            
        {
            if (listsInitialized && index <= sequence.Length - 1 && 
                gameObject.GetComponent<VideoPlayer>().Loaded == false && complete == true)
            {
                Debug.Log("Files" + gltfImports.Count);
                complete = false;
               
                await SendMeshesTextures();
                gameObject.GetComponent<VideoPlayer>().Loaded = true;
                gameObject.GetComponent<VideoPlayer>().RenderedFrames = 0;
                if (index < sequence.Length - 1)
                {
                    fileUri = Application.streamingAssetsPath + "/" + sequence[index];
                    await LoadGltfBinaryFromMemory(fileUri);
                    index++; 
                }
                else if (index == sequence.Length - 1 && gameObject.GetComponent<VideoPlayer>().Loop)
                {
                    Debug.Log("Loop starting");
                    index = 0;
                    fileUri = Application.streamingAssetsPath + "/" + sequence[index];
                    await LoadGltfBinaryFromMemory(fileUri);
                    index++; 
                }
                
                playList *= -1;
                complete = true;
                Debug.Log(index);
            }
            
        }
        
    }

    async Task LoadSingleVideo()
    {
        
        string fileUri = Application.streamingAssetsPath + "/" + sequence[index];
        await LoadGltfBinaryFromMemory(fileUri);
        for (int i = 0; i < meshCount; i++)
        {
            gameObject.GetComponent<VideoPlayer>().Meshes.Add(firstMeshes[i]);
            gameObject.GetComponent<VideoPlayer>().Textures.Add(firstTextures[i]);
        }
        gameObject.GetComponent<VideoPlayer>().Loaded = true;
    }

    async Task InitializeLists()
    {
        index = 0;
        fileUri = Application.streamingAssetsPath + "/" + sequence[index];
        await LoadGltfBinaryFromMemory(fileUri);

        playList *= -1;
        index = 1;
        fileUri = Application.streamingAssetsPath + "/" + sequence[index];
        await LoadGltfBinaryFromMemory(fileUri);
        playList *= -1;
        
        gameObject.GetComponent<VideoPlayer>().Loaded = true;

        index = 2;
    }

    async Task SendMeshesTextures()
    {
        if (playList == 1)
        {
            gameObject.GetComponent<VideoPlayer>().Meshes.Clear();
            gameObject.GetComponent<VideoPlayer>().Textures.Clear();
            for (int i = 0; i < firstMeshes.Count; i++)
            {
                gameObject.GetComponent<VideoPlayer>().Meshes.Add(firstMeshes[i]);
                gameObject.GetComponent<VideoPlayer>().Textures.Add(firstTextures[i]);
            }
        }

        if (playList == -1)
        {
            gameObject.GetComponent<VideoPlayer>().Meshes.Clear();
            gameObject.GetComponent<VideoPlayer>().Textures.Clear();
            for (int i = 0; i < secondMeshes.Count; i++)
            {
                    gameObject.GetComponent<VideoPlayer>().Meshes.Add(secondMeshes[i]);
                    gameObject.GetComponent<VideoPlayer>().Textures.Add(secondTextures[i]);
            }
        }
        
    }

    async Task LoadGltfBinaryFromMemory(string fileUri)
    {
        if (index < sequence.Length)
        {
            #region Loads glTF or GLB file

            data = BetterStreamingAssets.ReadAllBytes(sequence[index]);
            var gltf = new GltfImport();
            bool success = await gltf.LoadGltfBinary(data, new Uri(fileUri));

            #endregion

            if (success)
            {
                #region Shows if video is loaded and loads meshes and textures
                
                
                meshCount = gltf.GetMeshes().Length;


                #endregion

                if (playList == -1)
                {
                    firstMeshes.Clear();
                    firstTextures.Clear();
                    for (int i = 0; i < meshCount; i++)
                    {
                        firstMeshes.Add(gltf.GetMesh(i, 0));                                                 // What is meshNumeration?
                        firstTextures.Add(gltf.GetTexture(i));
                    }
                }

                if (playList == 1)
                {
                    secondMeshes.Clear();
                    secondTextures.Clear();
                    for (int i = 0; i < meshCount; i++)
                    {
                        secondMeshes.Add(gltf.GetMesh(i, 0));
                        secondTextures.Add(gltf.GetTexture(i));
                    }
                }

                #region Garbage Collection

                gltfImports.Add(gltf);
                data = null;
                
                if (gltfImports.Count >= 2)
                {
                    gltfImports[gltfImports.Count - 2].Dispose();
                    gltfImports.RemoveAt(gltfImports.Count - 2);
                }
                

                #endregion
                
                

            }
            else
            {
                Debug.Log("Could not load gltf file");
            }
        }
        
    }
}
