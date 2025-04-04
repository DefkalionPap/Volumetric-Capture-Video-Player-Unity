using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using GLTFast;
using System.Threading.Tasks;
public class LoadVideo : MonoBehaviour
{
    #region Fields

    private int meshCount;
    private int index;
    private byte[] data;
    string[] sequence;
    private string fileUri;
    [SerializeField] private Vector3 videoPosition = new Vector3(0, 0, 0);
    [SerializeField] private Quaternion videoRotation = Quaternion.Euler(270, 0, 0);
   // [SerializeField] private Vector3 videoScale;
    [SerializeField]private List<Mesh> firstMeshes;
    [SerializeField] List<Mesh> secondMeshes;
    [SerializeField] List<Texture> firstTextures;
    [SerializeField] List<Texture> secondTextures;
    private int playList = -1;
    bool complete = true;

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

    public int Playlist
    {
        get { return playList; }
        set { playList = value; }
    }
    
    #endregion
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        gameObject.transform.position = videoPosition;
        gameObject.transform.rotation = videoRotation;
        gameObject.transform.localScale = gameObject.transform.localScale / 4;
        //gameObject.transform.localScale = videoScale;
        BetterStreamingAssets.Initialize();
        sequence = BetterStreamingAssets.GetFiles("/", "*.glb", SearchOption.AllDirectories);
        if (sequence.Length == 0)
        {
            Debug.Log("No glb files found");
        }
            
        if (sequence.Length == 1)
        {
            await playSingleVideo();
        }
            
        if (sequence.Length > 1)
        {
            Debug.Log("Playing multiple gltf files");
            await InitializeLists();
        }
            
    }

    // Update is called once per frame
    async void Update()
    {
        if (sequence.Length > 1) // Checks if there is more than one video to play
        {
            if (index >= 2 && index < sequence.Length && gameObject.GetComponent<videoPlayer2>().Loaded == false && complete == true)
            {
                complete = false;
                //Play first video
                await SendMeshesTextures();
                gameObject.GetComponent<videoPlayer2>().Loaded = true;
                gameObject.GetComponent<videoPlayer2>().RenderedFrames = 0;
            
                //Load second video
                fileUri = Application.streamingAssetsPath + "/" + sequence[index];
                await LoadGltfBinaryFromMemory(fileUri);
                index++;

                playList *= -1;
                complete = true;
            }

            if (index >= sequence.Length && gameObject.GetComponent<videoPlayer2>().Loop == true)
            {
               Loop();
            }
        }

        if (sequence.Length == 1 && gameObject.GetComponent<videoPlayer2>().Loop)
        {
            if (gameObject.GetComponent<videoPlayer2>().Loaded == false)
            {
                gameObject.GetComponent<videoPlayer2>().RenderedFrames = 0;
                gameObject.GetComponent<videoPlayer2>().Loaded = true;
            }
        }
    }

    async Task playSingleVideo()
    {
        Debug.Log("Playing single glb file");
        string fileUri = Application.streamingAssetsPath + "/" + sequence[index];
        await LoadGltfBinaryFromMemory(fileUri);
        for (int i = 0; i < meshCount; i++)
        {
            gameObject.GetComponent<videoPlayer2>().Meshes.Add(firstMeshes[i]);
            gameObject.GetComponent<videoPlayer2>().Textures.Add(firstTextures[i]);
        }
        gameObject.GetComponent<videoPlayer2>().Loaded = true;
    }
    async Task Loop()
    {
        Debug.Log("Loop will start soon");
        index = 0;
        
    }
    async Task InitializeLists()
    {
        index = 0;
        fileUri = Application.streamingAssetsPath + "/" + sequence[index];
        await LoadGltfBinaryFromMemory(fileUri);

        index = 1;
        fileUri = Application.streamingAssetsPath + "/" + sequence[index];
        await LoadGltfBinaryFromMemory(fileUri);
        
        gameObject.GetComponent<videoPlayer2>().Loaded = true;

        index = 2;
    }

    async Task SendMeshesTextures()
    {
        if (playList == 1)
        {
            gameObject.GetComponent<videoPlayer2>().Meshes.Clear();
            gameObject.GetComponent<videoPlayer2>().Textures.Clear();
            for (int i = 0; i < firstMeshes.Count; i++)
            {
                gameObject.GetComponent<videoPlayer2>().Meshes.Add(firstMeshes[i]);
                gameObject.GetComponent<videoPlayer2>().Textures.Add(firstTextures[i]);
            }
        }

        if (playList == -1)
        {
            gameObject.GetComponent<videoPlayer2>().Meshes.Clear();
            gameObject.GetComponent<videoPlayer2>().Textures.Clear();
            for (int i = 0; i < secondMeshes.Count; i++)
            {
                    gameObject.GetComponent<videoPlayer2>().Meshes.Add(secondMeshes[i]);
                    gameObject.GetComponent<videoPlayer2>().Textures.Add(secondTextures[i]);
            }
        }
        
    }

    async Task LoadGltfBinaryFromMemory(string fileUri)
    {
        #region Loads glTF or GLB file

        data = BetterStreamingAssets.ReadAllBytes(sequence[index]);
        var gltf = new GltfImport();
        bool success = await gltf.LoadGltfBinary(data, new Uri(fileUri));

        #endregion

        if (success)
        {
            Debug.Log("Loading video: " + index);
            #region Shows if video is loaded and loads meshes and textures

            Mesh[] vcMeshes = gltf.GetMeshes();
            meshCount = vcMeshes.Length;

            List<Texture> textures = new List<Texture>();
            for (int i = 0; i < gltf.TextureCount; i++)
            {
                textures.Add(gltf.GetTexture(i));
            }

            #endregion

            #region Set first and second videos

            if (index == 0)
            {
                for (int i = 0; i < meshCount; i++)
                {
                    firstMeshes.Add(vcMeshes[i]);
                    firstTextures.Add(gltf.GetTexture(i));
                }
                //index++;
            }

            if (index == 1)
            {
                for (int i = 0; i < meshCount; i++)
                {
                    secondMeshes.Add(vcMeshes[i]);
                    secondTextures.Add(gltf.GetTexture(i));
                }

                index++;
            }

            #endregion
            
            
            if (index >= 2 && playList == 1)
            {
                secondMeshes.Clear();
                secondTextures.Clear();
                for (int i = 0; i < meshCount; i++)
                {
                    secondMeshes.Add(vcMeshes[i]);
                    secondTextures.Add(gltf.GetTexture(i));
                }
            }

            if (index >= 2 && playList == -1)
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
        else
        {
            Debug.Log("Could not load gltf file");
        }
    }
}
