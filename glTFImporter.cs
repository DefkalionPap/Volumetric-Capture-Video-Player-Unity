using UnityEngine;
using System.IO;

public class OrganizeImporter : MonoBehaviour
{
    #region Fields
    GameObject glTFObject;
    public string filePath;
    public Vector3 position = Vector3.zero;
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    public bool Close_Plugin = false;

    Vector3[] objectVertices;
    Vector2[] objectUV;
    int[] objectTriangles;
    #endregion


    #region Properties
    public Vector3[] ObjectVertices
    {
        get { return objectVertices; }
        set { objectVertices = value; }
    }
    public Vector2[] ObjectUV
    {
        get { return objectUV; }
        set { objectUV = value; }
    }
    public int[] ObjectTriangles
    {
        get { return objectTriangles; }
        set { objectTriangles = value; }
    }
    #endregion

    #region Methods
    public void Start()
    {
        Debug.Log("File path is: " + filePath);
        if (File.Exists(filePath))
        {
            glTFReader();
            SpawnObject();
        }                             //&& File extention is .gltf or .glb
        else
        {
            Debug.Log("File not found :(");
        }
    } // Runs at first frame
    public void Update()
    {
        UpdateObjectMesh();
        DestroyPlugin(Close_Plugin);
    } // Runs every frame
    public void DestroyPlugin(bool Close_Plugin)
    {
        if (Close_Plugin)
        {
            Destroy(gameObject);
        }
        
    } // Destroys plugin*
    public void glTFReader()
    {
        // Checks and reads glTF file from path the user specified         Use JsonUtility.FromJson and JsonUtility.FromJsonOverwrite
        // Open glTF file
        // Deserialize Json
        // Find Buffer?
        //Update vertices
        //Update UVs
        //Update triangles

        //Testing playground
        Debug.Log("File found :)");
        //File.ReadAllBytes(filePath);
    } // Reads glTF file
    public void SpawnObject()
    {
        //Creates glTFObject and places it in position
        glTFObject = new GameObject("glTF Object");
        glTFObject.transform.position = position;

        //Adds components to glTF object
        glTFObject.AddComponent<MeshFilter>();
        glTFObject.AddComponent<MeshRenderer>();
        meshRenderer = glTFObject.GetComponent<MeshRenderer>();
        meshFilter = glTFObject.GetComponent<MeshFilter>();
        Mesh glTFMesh = glTFObject.GetComponent<MeshFilter>().mesh;                                //Creates mesh, inspired by documentation



    } // Spawns object
    public void UpdateObjectMesh()
    {
        //Update gameObject meshes for every frame https://bit.ly/unitydocs_mesh
        //GetComponent<MeshFilter>().mesh = glTFMesh;           //Line from documentation
        // Get vertices, uv, and triangles from glTF
        //glTFMesh.vertices = objectVertices;
        //glTFMesh.uv = objectUV;
        //glTFMesh.triangles = objectTriangles;
    } // Updates object's mesh and material
   
    #endregion
}

#region Classes
public class ScenesAndNodes : MonoBehaviour
{
    
}
public class Meshes : MonoBehaviour
{

}
public class Buffers : MonoBehaviour
{
    int byteLength;                               //Iclude bufferviews and accessors
    string uri;
}
public class Materials : MonoBehaviour
{

}
public class TexturesImagesSamplers : MonoBehaviour
{

}
#endregion
