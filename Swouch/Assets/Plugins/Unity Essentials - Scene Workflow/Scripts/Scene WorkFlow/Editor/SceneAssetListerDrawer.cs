using UnityEditor;
using UnityEngine;
using unityEssentials.sceneWorkflow.extension.propertyAttribute.generic;

namespace unityEssentials.sceneWorkflow
{
    [CustomPropertyDrawer(typeof(SceneContextAsset), true)]
    public class SceneAssetListerDrawer : GenericScriptableObjectPropertyDrawer<SceneContextAsset>
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = base.GetPropertyHeight(property, label);
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float newY = DrawGUI(position, property, label);
        }
    }
}