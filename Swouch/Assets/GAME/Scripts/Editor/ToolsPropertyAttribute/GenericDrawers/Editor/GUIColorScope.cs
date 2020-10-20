using System;
using UnityEngine;

namespace swouch.tools.propertyAttribute.generic
{
    public class GUIColorScope : IDisposable
    {
        private Color _originColor;

        public GUIColorScope(Color color)
        {
            _originColor = GUI.color;
            GUI.color = color;
        }

        ~GUIColorScope()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            GUI.color = _originColor;
            _originColor = default(Color);
        }
    }
}