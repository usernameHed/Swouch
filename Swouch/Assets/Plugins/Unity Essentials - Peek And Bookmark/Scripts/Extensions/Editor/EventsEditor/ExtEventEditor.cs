using UnityEngine;

namespace unityEssentials.peek.extensions.editor.eventEditor
{
    public static class ExtEventEditor
    {
        public static void Use()
        {
            if (Event.current.type != EventType.Layout && Event.current.type != EventType.Repaint)
            {
                Event.current.Use();
            }
        }
    }
}