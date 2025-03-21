using System.Collections.Generic;
using UnityEngine;

public class videoPlayer2 : MonoBehaviour
{

    #region Fields
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

    #region Properties
    public List<Mesh> Meshes { get { return meshes; } set { meshes = value; } }
    public List<Texture> Textures { get { return textures; } set { textures = value; } }
    public bool Loaded { get { return loaded; } set { loaded = value; } }
    #endregion
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        
        meshFilter = gameObject.GetComponent<MeshFilter>();
        renderer = gameObject.GetComponent<Renderer>();
        
    }

    public void FixedUpdate() 
    {
        if (renderedFrames == meshes.Count && loaded)
        {
            //Debug.Log("Loaded is: " + loaded);
            if (gameObject.GetComponent<RoundRobbin2>().FirstMeshes.Count > 0 &&
                gameObject.GetComponent<RoundRobbin2>().SecondMeshes.Count > 0)
            {
                gameObject.GetComponent<RoundRobbin2>().Playlist *= -1;
                gameObject.GetComponent<RoundRobbin2>().Call = true;
                loaded = false;
                renderedFrames = 0;
            }
            
        }
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
        }
    }
    
}

