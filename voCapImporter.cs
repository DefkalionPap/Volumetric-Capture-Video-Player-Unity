using UnityEngine;
public class VoCapImporter : MonoBehaviour
{
    #region Fields
    [SerializeField] private GameObject glbObject;
    [SerializeField] private bool addObject;
    #endregion
    
    #region Methods
    public void Update()
    {
        if (glbObject != null)       //Check if file is glTF or glb
        {
            SpawnObject();
        } 
    }
    public void SpawnObject()
    {
        if (addObject)
        {
            //Add components to file, then instantiate
            Instantiate(glbObject);
            addObject = false;
        }
    } // Spawns object
    #endregion
}
