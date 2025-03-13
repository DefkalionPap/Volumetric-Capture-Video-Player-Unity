using System;
using System.Collections.Generic;
using UnityEngine;

public class VolumetricCapturePlayer : MonoBehaviour
{

    [SerializeField] int FPS = 30;
    [SerializeField] bool loop = false;
    [SerializeField] List<GameObject> sequence = new List<GameObject>();
    bool over = true;
    int renderedFrames = 0;
    private float timer = 0;
    float frameDuration;
    [SerializeField] List<Mesh> volCapMeshes = new List<Mesh>();
    [SerializeField] List<Texture> volCapTextures = new List<Texture>();
    bool loaded = true;
    
    public List<GameObject> Sequence { get { return sequence; } set { sequence = value; } }
    public List<Texture> SequenceTextures { get { return volCapTextures; } set { volCapTextures = value; } }
    public List<Mesh> SequenceMeshes { get { return volCapMeshes; } set { volCapMeshes = value; } }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Awake()
    {
        gameObject.GetComponent<MeshRenderer>().material.shader = Shader.Find("Universal Render Pipeline/Unlit");
        gameObject.transform.rotation = Quaternion.Euler(-90, 0, 270);
    }

    public void FixedUpdate()
    {
        if (over)
        {
            timer += Time.fixedDeltaTime;
            frameDuration = 1f / FPS; //AI assisted line

            if (timer >= frameDuration && loaded)
            {
                if (renderedFrames < volCapMeshes.Count) //AI assisted line
                {
                    MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
                    Renderer renderer = gameObject.GetComponent<Renderer>();
                    meshFilter.mesh = volCapMeshes[renderedFrames];
                    renderer.material.mainTexture = volCapTextures[renderedFrames];
                    renderedFrames++;
                    timer -= frameDuration; // Reset timer and Keep the overflow to carry forward the remainder - AI assisted line
                }
            }

            if (loop == true && renderedFrames == volCapMeshes.Count)
                renderedFrames = 0;
        }
    }
    
    
}
