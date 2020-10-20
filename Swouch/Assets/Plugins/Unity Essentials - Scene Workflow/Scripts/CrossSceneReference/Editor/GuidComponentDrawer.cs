using UnityEditor;
using UnityEngine;
using unityEssentials.sceneWorkflow.extension;

namespace unityEssentials.crossSceneReference
{
    [CustomEditor(typeof(GuidComponent))]
    public class GuidComponentDrawer : Editor
    {
        private GuidComponent guidComp;

        public override void OnInspectorGUI()
        {
            if (guidComp == null)
            {
                guidComp = (GuidComponent)target;
            }

            // Draw label
            EditorGUILayout.LabelField("Guid:", guidComp.GetGuid().ToString());
            //guidComp.GuidDescriptionType = ExtGUIEnums.EnumPopup(guidComp.GuidDescriptionType, guidComp, out bool valueHasChanged);

        }
    }
}