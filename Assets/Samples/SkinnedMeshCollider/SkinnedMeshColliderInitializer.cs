using System.Collections.Generic;
using DynamicSkinnedMeshCollision;
using UnityEngine;

namespace Samples.SkinnedMeshCollider
{
    public sealed class SkinnedMeshColliderInitializer : MonoBehaviour
    {
        void Start()
        {
            Initialize();
        }

        public void Initialize()
        {
            var meshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var meshRenderer in meshRenderers)
            {
                var meshCollider = meshRenderer.GetOrAddComponent<DynamicSkinnedMeshCollider>();
            }
        }
    }
}
