// CustomEditor tool example.
// Shows a billboard at each vertex position on a selected mesh.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine.Rendering;

namespace gravityFields.editor.tools
{
    // By passing `typeof(MeshFilter)` as the second argument, we register VertexTool as a CustomEditor tool to be presented
    // when the current selection contains a MeshFilter component.
    [EditorTool("Show Vertices", typeof(MeshFilter))]
    public class VertexTool : EditorTool
    {
        private GUIContent _icon;
        private IEnumerable<TransformAndPositions> _vertices;

        private struct TransformAndPositions
        {
            public Transform Transform;
            public IList<Vector3> Positions;
        }

        public override GUIContent toolbarIcon
        {
            get
            {
                if (_icon == null)
                {
                    _icon = EditorGUIUtility.IconContent("AnimatorStateMachine Icon");
                }
                return (_icon);
            }
        }

        private void OnEnable()
        {
            //EditorTools.activeToolChanging += OnActiveToolWillChange;
            EditorTools.activeToolChanged += OnActiveToolDidChange;
        }

        private void OnDisable()
        {
            //EditorTools.activeToolChanging -= OnActiveToolWillChange;
            EditorTools.activeToolChanged -= OnActiveToolDidChange;
        }

        /// <summary>
        /// Called when an EditorTool is made the active tool.
        /// </summary>
        public void OnActiveToolDidChange()
        {
            if (EditorTools.IsActiveTool(this))
            {
                Selection.selectionChanged += RebuildVertexPositions;
                RebuildVertexPositions();
            }
            else
            {
                Selection.selectionChanged -= RebuildVertexPositions;
            }
        }

        private void RebuildVertexPositions()
        {
            _vertices = targets.Select(x =>
            {
                return new TransformAndPositions()
                {
                    Transform = ((MeshFilter)x).transform,
                    Positions = ((MeshFilter)x).sharedMesh.vertices
                };
            }).ToArray();
        }

        /// <summary>
        /// If you've implemented scene tools before, think of this like the `OnSceneGUI` method. This is where you put the
        /// implementation of your tool.
        /// </summary>
        /// <param name="window"></param>
        public override void OnToolGUI(EditorWindow window)
        {
            if (_vertices == null)
            {
                RebuildVertexPositions();
                return;
            }

            CompareFunction zTest = Handles.zTest;
            Handles.zTest = CompareFunction.LessEqual;

            foreach (TransformAndPositions entry in _vertices)
            {
                float size = HandleUtility.GetHandleSize(entry.Transform.position) * .04f;
                DrawHandleCaps(entry.Transform.localToWorldMatrix, entry.Positions, size);
            }

            Handles.zTest = zTest;
        }

        static void DrawHandleCaps(Matrix4x4 matrix, IList<Vector3> positions, float size)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            Vector3 sideways = (Camera.current == null ? Vector3.right : Camera.current.transform.right) * size;
            Vector3 up = (Camera.current == null ? Vector3.up : Camera.current.transform.up) * size;
            Color col = Handles.color * new Color(1, 1, 1, 0.99f);

            // After drawing the first dot cap, the handle material and matrix are set up, so there's no need to keep
            // resetting the state.
            Handles.DotHandleCap(0, matrix.MultiplyPoint(positions[0]), Quaternion.identity,
                HandleUtility.GetHandleSize(matrix.MultiplyPoint(positions[0])) * .05f, EventType.Repaint);

            GL.Begin(GL.QUADS);

            for (int i = 1, c = positions.Count; i < c; i++)
            {
                var position = matrix.MultiplyPoint(positions[i]);

                GL.Color(col);
                GL.Vertex(position + sideways + up);
                GL.Vertex(position + sideways - up);
                GL.Vertex(position - sideways - up);
                GL.Vertex(position - sideways + up);
            }

            GL.End();
        }
    }
}