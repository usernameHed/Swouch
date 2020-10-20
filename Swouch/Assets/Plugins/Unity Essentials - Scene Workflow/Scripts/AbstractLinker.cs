using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using unityEssentials.sceneWorkflow.extension;

namespace unityEssentials.sceneWorkflow
{
    public abstract class AbstractLinker : SingletonMono<AbstractLinker>
    {
        public abstract void InitFromEditor();
        public abstract void InitFromPlay();
    }
}