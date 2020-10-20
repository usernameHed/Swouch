using System;
using UnityEngine;
using UnityEditor;
using System.Reflection;

#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#else
using UnityEngine.Experimental.UIElements;
#endif

namespace unityEssentials.timeScale.toolbarExtent
{
	public static class ToolbarCallback
	{
		private static Type _toolbarType = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");
		private static Type _guiViewType = typeof(Editor).Assembly.GetType("UnityEditor.GUIView");
		private static PropertyInfo _viewVisualTree = _guiViewType.GetProperty("visualTree",
			BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		private static FieldInfo _imguiContainerOnGui = typeof(IMGUIContainer).GetField("m_OnGUIHandler",
			BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		private static ScriptableObject _currentToolbar;

		/// <summary>
		/// Callback for toolbar OnGUI method.
		/// </summary>
		public static Action OnToolbarGUI;

		static ToolbarCallback()
		{
			EditorApplication.update -= OnUpdate;
			EditorApplication.update += OnUpdate;
		}

		private static void OnUpdate()
		{
			if (MustGetToolbar())
			{
				//Find the first toolBar
				UnityEngine.Object[] toolbars = Resources.FindObjectsOfTypeAll(_toolbarType);
				_currentToolbar = toolbars.Length > 0 ? (ScriptableObject)toolbars[0] : null;

				if (_currentToolbar != null)
				{
					// Get it's visual tree
					VisualElement visualTree = (VisualElement)_viewVisualTree.GetValue(_currentToolbar, null);

					// Get first child which 'happens' to be toolbar IMGUIContainer
					IMGUIContainer container = (IMGUIContainer)visualTree[0];

					// (Re)attach handler
					Action handler = (Action)_imguiContainerOnGui.GetValue(container);
					handler -= OnGUI;
					handler += OnGUI;
					_imguiContainerOnGui.SetValue(container, handler);
				}
			}
		}

		/// <summary>
		/// toolbar is a ScriptableObject and gets deleted when layout changes
		/// </summary>
		/// <returns></returns>
		private static bool MustGetToolbar()
		{
			return _currentToolbar == null && !EditorApplication.isCompiling;
		}

		private static void OnGUI()
		{
			OnToolbarGUI?.Invoke();
		}
	}
}
