using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using GLTFast;
public class VolumetricCaptureVideoPlayer : MonoBehaviour
{
    #region Fields
    int renderedFrames = 0;
    private float timer = 0;
    float frameDuration;
    List<Mesh> meshes = new List<Mesh>();
    List<Texture> textures = new List<Texture>();
    [SerializeField] private List<String> fileName = new List<String>();
    [SerializeField] int FPS = 30;
    [SerializeField] bool loop = true;
    int meshCount = 0;
    bool loaded = false;
    private MeshFilter meshFilter;
    private Renderer renderer;
    #endregion

    #region Properties
    public List<Mesh> Meshes { get { return meshes; } set { meshes = value; } }
    public List<Texture> Textures { get { return textures; } set { textures = value; } }
    public bool Loaded { get { return loaded; } set { loaded = value; } }
    public bool Loop { get { return loop; } set { loop = value; } }
    public int RenderedFrames { get { return renderedFrames; } set { renderedFrames = value; } }
    #endregion
    
    private void Awake()
    {
        if (gameObject.GetComponent<MeshFilter>() == null)
        {
            gameObject.AddComponent<MeshFilter>();
        }

        if (gameObject.GetComponent<Renderer>() == null)
        {
            gameObject.AddComponent<MeshRenderer>();
        }
    }
    async void Start()
    {
        meshFilter = gameObject.GetComponent<MeshFilter>();
        renderer = gameObject.GetComponent<Renderer>();
        renderer.material.shader = Shader.Find("glTF/Unlit");

        if (fileName.Count == 0)
        {
            Debug.Log("No file names provided. Add file names and try again.");
        }
        
        else
        {
            BetterStreamingAssets.Initialize();
            for (int i = 0; i < fileName.Count; i++)
            {
                if (BetterStreamingAssets.GetFiles("/", fileName[i] + "*", SearchOption.AllDirectories).Length == 0)
                {
                    Debug.Log("File not found: " + fileName[i]);
                }
                else
                {
                    string filePath = BetterStreamingAssets.GetFiles("/", fileName[i] + "*" , SearchOption.AllDirectories)[0];
                    if (filePath.EndsWith(".glb") || filePath.EndsWith(".gltf"))
                    {
                        string fileUri = Application.streamingAssetsPath + "/" + filePath;
                        await LoadGltfBinaryFromMemory(fileUri, filePath);
                    }
                }
            }
            loaded = true;
        }
    }
    public void FixedUpdate() 
    {
        #region Loop
        if (renderedFrames == Meshes.Count - 1 && loop)
        {
            renderedFrames = 0;
            loaded = true;
        }
        
        #endregion
        #region Play Video
        if (renderedFrames >= meshes.Count - 1 && loaded)
        {
            loaded = false;
        }
        
        if (loaded && renderedFrames < meshes.Count)
        {
            timer += Time.fixedDeltaTime;
            frameDuration = 1f / FPS;
            if (timer >= frameDuration)
            {
                if (renderedFrames < meshes.Count)
                {
                    meshFilter.mesh = meshes[renderedFrames];
                    renderer.material.mainTexture = textures[renderedFrames];
                    renderedFrames++;
                    timer -= frameDuration;
                }
            }
        }
        #endregion
    }
    async Task LoadGltfBinaryFromMemory(string fileUri, string filePath)
    {
        #region Loads glTF or GLB file
        byte[] data = BetterStreamingAssets.ReadAllBytes(filePath);
        var gltf = new GltfImport();
        Debug.Log("Loading video...");
        bool success = await gltf.LoadGltfBinary(data, new Uri(fileUri));
        #endregion

            if (success)
            {
                #region Shows if video is loaded and loads meshes and textures
                meshCount = gltf.GetMeshes().Length;
                for (int i = 0; i < meshCount; i++)
                {
                    Meshes.Add(gltf.GetMesh(i, 0));                                                    
                    Textures.Add(gltf.GetTexture(i));
                }
                Debug.Log("Video loaded");
                #endregion
            }
            
            else
            {
                Debug.Log("Could not load gltf file");
            }
    }
}
