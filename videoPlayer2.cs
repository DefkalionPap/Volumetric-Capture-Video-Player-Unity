using System.Collections.Generic;
using UnityEngine;

public class videoPlayer2 : MonoBehaviour
{

    #region Original player fields
    [SerializeField] int FPS = 30;
    [SerializeField] bool loop = true;
    bool over = true;
    int renderedFrames = 0;
    private float timer = 0;
    float frameDuration;
    [SerializeField] List<Mesh> meshes = new List<Mesh>();
    [SerializeField] List<Texture> textures = new List<Texture>();
    bool loaded = false;
    private MeshFilter meshFilter;
    private Renderer renderer;
    #endregion
// next is true when the video is played

    public List<Mesh> Meshes { get { return meshes; } set { meshes = value; } }
    public List<Texture> Textures { get { return textures; } set { textures = value; } }
    public bool Loaded { get { return loaded; } set { loaded = value; } }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        //gameObject.GetComponent<MeshRenderer>().material.shader = Shader.Find("Universal Render Pipeline/Unlit");
        
        meshFilter = gameObject.GetComponent<MeshFilter>();
        renderer = gameObject.GetComponent<Renderer>();
    }

    public void FixedUpdate() 
    {
        if (loaded)
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
            if (renderedFrames == meshes.Count)
            {
                renderedFrames = 0;
                loaded = false;
                Debug.Log("Loaded is: " + loaded);
                gameObject.GetComponent<RoundRobbin2>().Call = true;
            }
        }
        
    }
}

