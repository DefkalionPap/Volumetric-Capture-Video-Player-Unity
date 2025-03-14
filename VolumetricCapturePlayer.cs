using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
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
    [SerializeField] List<Mesh> meshes = new List<Mesh>();
    [SerializeField] List<Texture> textures = new List<Texture>();
    bool loaded = true;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        sequence = gameObject.GetComponent<LoadData>().Sequence;
        meshes = gameObject.GetComponent<LoadMeshesAndTextures>().Meshes;
        textures = gameObject.GetComponent<LoadMeshesAndTextures>().Textures;
        gameObject.GetComponent<MeshRenderer>().material.shader = Shader.Find("Universal Render Pipeline/Unlit");
        gameObject.transform.rotation = Quaternion.Euler(-90, 0, 270);
    }

    public void FixedUpdate()
    {
        over = gameObject.GetComponent<LoadMeshesAndTextures>().Over;
        if (over)
        {
            timer += Time.fixedDeltaTime;
            frameDuration = 1f / FPS; //AI assisted line

            if (timer >= frameDuration && loaded)
            {
                if (renderedFrames < meshes.Count) //AI assisted line
                {
                    MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
                    Renderer renderer = gameObject.GetComponent<Renderer>();
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
