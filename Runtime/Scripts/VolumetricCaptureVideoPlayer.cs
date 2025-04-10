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
    [SerializeField] private string fileName;
    [SerializeField] private Vector3 videoPosition = new Vector3(0, 0, 0);
    [SerializeField] Quaternion videoRotation = Quaternion.Euler(0, 0, 0);
    [SerializeField] Vector3 videoScale = new Vector3(1, 1, 1);
    [SerializeField] int FPS = 30;
    [SerializeField] bool loop = true;
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
        
        BetterStreamingAssets.Initialize();
        if (BetterStreamingAssets.GetFiles("/", fileName + "*", SearchOption.AllDirectories).Length == 0)
        {
            Debug.Log("File not found");
        }
        else
        {
            string filePath = BetterStreamingAssets.GetFiles("/", fileName + "*" , SearchOption.AllDirectories)[0];
            if (filePath.EndsWith(".glb") || filePath.EndsWith(".gltf"))
            {
                string fileUri = Application.streamingAssetsPath + "/" + filePath;
                await LoadGltfBinaryFromMemory(fileUri, filePath);
                Loaded = true;
            }
        }
    }

    public void FixedUpdate() 
    {
        #region Set Transform

        gameObject.transform.position = videoPosition;
        gameObject.transform.rotation = videoRotation;
        gameObject.transform.localScale = videoScale;

        #endregion

        #region Loop

        if (renderedFrames == Meshes.Count - 1 && Loop && loaded == false)
        {
            renderedFrames = 0;
            loaded = true;
        }
        else if (renderedFrames == Meshes.Count - 1 && Loop == false)
        {
            loaded = false;
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
            frameDuration = 1f / FPS; //AI assisted line
            if (timer >= frameDuration)
            {
                if (renderedFrames < meshes.Count) //AI assisted line
                {
                    meshFilter.mesh = meshes[renderedFrames];
                    renderer.material.mainTexture = textures[renderedFrames];
                    renderedFrames++;
                    timer -= frameDuration; // Reset timer and Keep the overflow to carry forward the remainder - AI assisted line
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
            bool success = await gltf.LoadGltfBinary(data, new Uri(fileUri));

        #endregion

            if (success)
            {
                #region Shows if video is loaded and loads meshes and textures
                
                    int meshCount = gltf.GetMeshes().Length;
                    for (int i = 0; i < meshCount; i++)
                    {
                        Meshes.Add(gltf.GetMesh(i, 0));                                                    // What is meshNumeration?
                        Textures.Add(gltf.GetTexture(i));
                    }
                    Loaded = true;
                    
                #endregion
            }
            else
            {
                Debug.Log("Could not load gltf file");
            }
    }
}
