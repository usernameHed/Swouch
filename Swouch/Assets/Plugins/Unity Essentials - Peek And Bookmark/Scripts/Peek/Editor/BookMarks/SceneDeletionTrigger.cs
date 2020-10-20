using UnityEngine;
using UnityEditor;
using System.IO;
using unityEssentials.peek.toolbarExtent;
using System.Collections.Generic;

namespace unityEssentials.peek
{
    public class SceneDeletionTrigger : UnityEditor.AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            try
            {
                foreach (string deletedAsset in deletedAssets)
                {
                    string extension = Path.GetExtension(deletedAsset);
                    if (string.Equals(extension, ".unity"))
                    {
                        ToolsButton.PeekTool.PeekLogic.DeleteBookMarkedGameObjectLinkedToScene();
                        return;
                    }
                }
            }
            catch { }
        }
    }
}