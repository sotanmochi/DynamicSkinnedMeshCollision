using UnityEngine;

namespace DynamicSkinnedMeshCollision.Samples
{
    public static class MeshSurfacePointTrackingProvider
    {
        public const int DefaultLayerMask = ~(1 << 2); // All layers except "Ignore Raycast"

        public static bool Find(Ray ray, out MeshSurfacePointAnchor pointAnchor, float maxDistance = Mathf.Infinity, int layerMask = DefaultLayerMask)
        {
            pointAnchor = null;
            
            if (Physics.Raycast(ray, out var raycastHit, maxDistance, layerMask))
            {
                var dsmc = raycastHit.transform.GetComponentInParent<DynamicSkinnedMeshCollider>();
                if (dsmc != null)
                {               
                    if (FindClosestBoneByWeight(dsmc.SkinnedMeshRenderer.bones, dsmc.Mesh, raycastHit.triangleIndex, out var closestBone))
                    {
                        var positionOffset = closestBone.InverseTransformPoint(raycastHit.point);
                        pointAnchor = new MeshSurfacePointAnchor(closestBone, positionOffset, dsmc.Mesh, raycastHit.triangleIndex);
                        return true;
                    }

                    if (FindClosestBoneByDistance(dsmc.SkinnedMeshRenderer.bones, raycastHit.point, out closestBone))
                    {
                        var positionOffset = closestBone.InverseTransformPoint(raycastHit.point);
                        pointAnchor = new MeshSurfacePointAnchor(closestBone, positionOffset, dsmc.Mesh, raycastHit.triangleIndex);
                        return true;
                    }
                }

                var meshCollider = raycastHit.transform.GetComponentInParent<MeshCollider>();
                if (meshCollider != null)
                {
                    var positionOffset = meshCollider.transform.InverseTransformPoint(raycastHit.point);
                    pointAnchor = new MeshSurfacePointAnchor(meshCollider.transform, positionOffset, meshCollider.sharedMesh, raycastHit.triangleIndex);
                    return true;
                }
            }
            
            return false;
        }

        public static bool FindClosestBoneByDistance(Transform[] bones, Vector3 point, out Transform closestBone)
        {
            closestBone = null;

            if (bones == null || bones.Length == 0)
            {
                return false;
            }

            var minDistance = float.MaxValue;
            foreach (var bone in bones)
            {
                var distance = Vector3.Distance(point, bone.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestBone = bone;
                }
            }

            return closestBone != null;
        }

        public static bool FindClosestBoneByWeight(Transform[] bones, Mesh mesh, int triangleIndex, out Transform closestBone)
        {
            closestBone = null;

            if (mesh == null || triangleIndex < 0 || triangleIndex >= mesh.triangles.Length / 3)
            {
                return false;
            }

            var boneWeightsPerVertex = mesh.boneWeights;
            var vertIndices = new int[]
            {
                mesh.triangles[triangleIndex * 3],
                mesh.triangles[triangleIndex * 3 + 1],
                mesh.triangles[triangleIndex * 3 + 2]
            };

            var accumulatedBoneWeights = new float[bones.Length];
            foreach (var vertIndex in vertIndices)
            {
                if (vertIndex < 0 || vertIndex >= boneWeightsPerVertex.Length)
                {
                    return false;
                }

                var boneWeight = boneWeightsPerVertex[vertIndex];
                if (boneWeight.weight0 > 0)
                {
                    accumulatedBoneWeights[boneWeight.boneIndex0] += boneWeight.weight0;
                }
                if (boneWeight.weight1 > 0)
                {
                    accumulatedBoneWeights[boneWeight.boneIndex1] += boneWeight.weight1;
                }
                if (boneWeight.weight2 > 0)
                {
                    accumulatedBoneWeights[boneWeight.boneIndex2] += boneWeight.weight2;
                }
                if (boneWeight.weight3 > 0)
                {
                    accumulatedBoneWeights[boneWeight.boneIndex3] += boneWeight.weight3;
                }
            }
            
            var maxWeight = 0f;
            var closestBoneIndex = -1;
            for (int i = 0; i < accumulatedBoneWeights.Length; i++)
            {
                if (accumulatedBoneWeights[i] > maxWeight)
                {
                    maxWeight = accumulatedBoneWeights[i];
                    closestBoneIndex = i;
                }
            }

            if (closestBoneIndex < 0 || closestBoneIndex >= bones.Length)
            {
                return false;
            }

            closestBone = bones[closestBoneIndex];
            return true;
        }
    }
}
