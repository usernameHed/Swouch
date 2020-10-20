﻿using UnityEditor;
using UnityEngine;
using unityEssentials.sceneWorkflow.extension.propertyAttribute.generic;

namespace unityEssentials.sceneWorkflow
{
    [CustomPropertyDrawer(typeof(ContextListerAsset), true)]
    public class GlobalScenesListerAssetDrawer : GenericScriptableObjectPropertyDrawer<ContextListerAsset>
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