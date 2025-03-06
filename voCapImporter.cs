using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class voCapImporter : MonoBehaviour
{

    #region Fields
    [SerializeField] private GameObject glbObject;
    [SerializeField] private bool addObject;
    Renderer _rend;
    Material _mat;
    [SerializeField] List<Texture> voCapTextures = new List<Texture>();
    [SerializeField] List<Mesh> voCapMeshes = new List<Mesh>();
    MeshFilter _meshFilter;
    string path;
    #endregion

    #region Methods
    void Update()
    {
        if (glbObject != null)
        {
            SpawnObject();
        }
    }

    public void SpawnObject()
    {
        if (addObject)
        {
            gameObject.name = glbObject.name;
            gameObject.AddComponent<MeshFilter>();
            gameObject.AddComponent<MeshRenderer>();
            path = AssetDatabase.GetAssetPath(glbObject);
            Debug.Log(path);
            
            AddMeshes();
            SetFirstMesh();
            SetFirstTexture();
            transform.rotation = Quaternion.Euler(-90, 0, 180);
            addObject = false;
        }
    } // Spawns object

    public void SetFirstMesh()
    {
        _meshFilter = gameObject.GetComponent<MeshFilter>();
        _meshFilter.mesh = voCapMeshes[0];

    }

    public void SetFirstTexture()
    {
        _rend = gameObject.GetComponent<Renderer>();
        _mat = _rend.material;
        _mat.mainTexture = voCapTextures[0];
    }

    public void AddMeshes()               //Still writing
    {
        string[] meshArray = AssetDatabase.GetDependencies(path);
        for (int i = 0; i < meshArray.Count(); i++)
        {
            Mesh tempMesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshArray[i]);
            voCapMeshes.Add(tempMesh);
        }
        
        string[] textArray = AssetDatabase.GetDependencies(path);
        for (int i = 0; i < textArray.Count(); i++)
        {
            Texture tempText = AssetDatabase.LoadAssetAtPath<Texture>(textArray[i]);
            voCapTextures.Add(tempText);
        }
    }
    #endregion
}
