using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem.Android;

[CreateAssetMenu(fileName = "VolCapData", menuName = "VolCapData")]
public class VolCapData : ScriptableObject
{
    public List<GameObject> sequence = new List<GameObject>();

}