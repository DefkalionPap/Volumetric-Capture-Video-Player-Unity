using UnityEditor;
using UnityEngine;

public class MenuTest : MonoBehaviour
{
    #if UNITY_EDITOR 
        [MenuItem("Volumetric Capture Video Player / Volumetric Capture Video", false, 10)]
        static void CreateVolCapVideo(MenuCommand menuCommand)
        {
            GameObject video = new GameObject("Volumetric Capture Video");
            video.AddComponent<VolumetricCaptureVideoPlayer>();
            video.AddComponent<MeshFilter>();
            video.AddComponent<MeshRenderer>();
            video.GetComponent<MeshRenderer>().material = new Material(Shader.Find("glTF/Unlit"));
            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(video, "Create " + video.name);
            Selection.activeObject = video;
        }
    #endif
}
