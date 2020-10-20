using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;

namespace gravityFields.editor.tools
{
    // Tagging a class with the EditorTool attribute and no target type registers a global tool. Global tools are valid for any selection, and are accessible through the top left toolbar in the editor.
    [EditorTool("Platform Tool")]
    class PlatformTool : EditorTool
    {
        private GUIContent _icon;

        public override GUIContent toolbarIcon
        {
            get
            {
                if (_icon == null)
                {
                    _icon = EditorGUIUtility.IconContent("d_Transform Icon");
                }
                return (_icon);
            }
        }

        /// <summary>
        /// This is called for each window that your tool is active in. Put the functionality of your tool here.
        /// </summary>
        /// <param name="window"></param>
        public override void OnToolGUI(EditorWindow window)
        {
            EditorGUI.BeginChangeCheck();

            Vector3 position = Tools.handlePosition;

            using (new Handles.DrawingScope(Color.green))
            {
                position = Handles.Slider(position, Vector3.up);
            }

            if (EditorGUI.EndChangeCheck())
            {
                Vector3 delta = position - Tools.handlePosition;

                Undo.RecordObjects(Selection.transforms, "Move Platform");

                foreach (Transform transform in Selection.transforms)
                {
                    transform.position += delta;
                }
            }
        }
    }
}