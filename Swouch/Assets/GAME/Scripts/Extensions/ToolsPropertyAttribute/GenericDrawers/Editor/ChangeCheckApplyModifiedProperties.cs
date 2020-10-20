
using System;
using UnityEditor;

namespace swouch.tools.propertyAttribute.generic
{
    public class ChangeCheckApplyModifiedProperties : IDisposable
    {
        private SerializedObject _serializedObject;

        public ChangeCheckApplyModifiedProperties(SerializedObject serializedObject)
        {
            _serializedObject = serializedObject;
            EditorGUI.BeginChangeCheck();
        }

        public void Dispose()
        {
            if (EditorGUI.EndChangeCheck())
            {
                _serializedObject.ApplyModifiedProperties();
            }
        }
    }

    public class ChangeCheckApplyModifiedPropertiesWithoutUndo : IDisposable
    {
        private SerializedObject _serializedObject;

        public ChangeCheckApplyModifiedPropertiesWithoutUndo(SerializedObject serializedObject)
        {
            _serializedObject = serializedObject;
            EditorGUI.BeginChangeCheck();
        }

        public void Dispose()
        {
            if (EditorGUI.EndChangeCheck())
            {
                _serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }
}