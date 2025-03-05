using UnityEngine;
using System.IO;
using Unity.VisualScripting;
using UnityEngine.UI;

public class voCapImporter : MonoBehaviour
{
    #region Fields
    [SerializeField] bool ClosePlugin = false;
    [SerializeField] GameObject glTFObject = null;
    [SerializeField] Vector3 position = Vector3.zero;
    [SerializeField] Quaternion rotation = Quaternion.Euler(-90, 0, 0); 
    [SerializeField] private bool AddObject = false;
    private GameObject temp;
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
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
        temp = glTFObject;
    } // Runs at first frame
    public void Update()
    {
        if (glTFObject != null)       //Make sure it also checks if the file is glTF or glb
        
        {
            SpawnObject();
            UpdateObjectMesh();
        } 
        DestroyPlugin(ClosePlugin);
    } // Runs every frame
    public void DestroyPlugin(bool Close_Plugin)
    {
        if (Close_Plugin)
        {
            Destroy(gameObject);
        }
        
    } // Destroys plugin*
   
    public void SpawnObject()
    {
        if (AddObject)
        {
            Instantiate(glTFObject, position, rotation);
            AddObject = false;
        }
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
