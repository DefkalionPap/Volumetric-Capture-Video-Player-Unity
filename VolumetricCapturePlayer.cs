using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class VolumetricCapturePlayer : MonoBehaviour
{

    [SerializeField] int FPS = 30;
    [SerializeField] bool loop = true;
    bool over = true;
    int renderedFrames = 0;
    private float timer = 0;
    float frameDuration;
    [SerializeField] List<Mesh> meshes = new List<Mesh>();
    [SerializeField] List<Texture> textures = new List<Texture>();
    bool loaded = true;
    private MeshFilter meshFilter;
    private Renderer renderer;
    
    public List<Mesh> Meshes { get { return meshes; } set { meshes = value; } }
    public List<Texture> Textures { get { return textures; } set { textures = value; } }
    public bool Over { get { return over; } set { over = value; } }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        //gameObject.GetComponent<MeshRenderer>().material.shader = Shader.Find("Universal Render Pipeline/Unlit");
        gameObject.transform.rotation = Quaternion.Euler(-90, 0, 270);
        meshFilter = gameObject.GetComponent<MeshFilter>();
        renderer = gameObject.GetComponent<Renderer>();
    }

    public void FixedUpdate()
    {
        if (over)
        {
            timer += Time.fixedDeltaTime;
            frameDuration = 1f / FPS; //AI assisted line
            

            if (timer >= frameDuration && loaded)
            {
                if (renderedFrames < meshes.Count) //AI assisted line
                {
                    
                    meshFilter.mesh = meshes[renderedFrames];
                    renderer.material.mainTexture = textures[renderedFrames];
                    renderedFrames++;
                    timer -= frameDuration; // Reset timer and Keep the overflow to carry forward the remainder - AI assisted line
                }
            }

            if (loop == true && renderedFrames == meshes.Count)
                renderedFrames = 0;
        }
    }
}
