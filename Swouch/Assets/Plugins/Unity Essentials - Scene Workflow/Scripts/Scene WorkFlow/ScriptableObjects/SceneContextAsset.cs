using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace unityEssentials.sceneWorkflow
{
    [CreateAssetMenu(fileName = "Context", menuName = "UnityEssensials/Scene Workflow/Context")]
    public class SceneContextAsset : ScriptableObject
    {
        public string NameContext = "Scene List";
        public List<ExtSceneReference> SceneToLoad = new List<ExtSceneReference>();
    }
}