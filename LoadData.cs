using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LoadData : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] VolCapData volCapData;
    [SerializeField] List<GameObject> sequence;
    bool loadedList;
    
    public bool LoadedList { get => loadedList; set => loadedList = value; }
    public List<GameObject> Sequence { get => sequence; set => sequence = value; }
    void Start()
    {
        sequence = volCapData.sequence;
        if (sequence.Count > 0)                                                                                             //Might need to change to sequence.Count >= 0  
            loadedList = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
