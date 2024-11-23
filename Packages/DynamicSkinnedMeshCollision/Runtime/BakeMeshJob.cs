using Unity.Burst;
using Unity.Jobs;
using UnityEngine;

namespace DynamicSkinnedMeshCollision
{
    [BurstCompile]
    public struct BakeMeshJob : IJob
    {
        int _meshId;

        public BakeMeshJob(int meshId)
        {
            _meshId = meshId;
        }

        public void Execute()
        {
            Physics.BakeMesh(_meshId, false);
        }
    }
}
