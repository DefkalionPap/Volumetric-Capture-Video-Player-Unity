using System.Collections.Generic;
using UnityEngine;

public class VideoPlayer : MonoBehaviour
{

    #region Fields
    
    bool over = true;
    int renderedFrames = 0;
    private float timer = 0;
    float frameDuration;
    [SerializeField]List<Mesh> meshes = new List<Mesh>();
    List<Texture> textures = new List<Texture>();
    [SerializeField] private Vector3 videoPosition = new Vector3(0, 0, 0);
    [SerializeField] Quaternion videoRotation = Quaternion.Euler(270, 0, 0);
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
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        meshFilter = gameObject.GetComponent<MeshFilter>();
        renderer = gameObject.GetComponent<Renderer>();
    }

    public void FixedUpdate() 
    {
        gameObject.transform.position = videoPosition;
        gameObject.transform.rotation = videoRotation;
        if (renderedFrames >= meshes.Count && loaded)
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
    }
    
}

