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
        if (glbObject != null) 
        {
            SpawnObject();
        } 
    }
    public void SpawnObject()
    {
        if (addObject)
        {
            //Remember to add components
            Instantiate(glbObject);
            addObject = false;
        }
    } // Spawns object
    #endregion
}
