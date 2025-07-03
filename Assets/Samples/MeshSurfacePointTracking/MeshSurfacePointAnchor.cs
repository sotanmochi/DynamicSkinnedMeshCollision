using System;
using UnityEngine;

namespace DynamicSkinnedMeshCollision.Samples
{
    /// <summary>
    /// Represents a trackable point on the surface of the mesh.
    /// </summary>
    public sealed class MeshSurfacePointAnchor
    {
        /// <summary>
        /// The world space position of the point.
        /// </summary>
        public Vector3? Position => TrackingOrigin != null ? TrackingOrigin.TransformPoint(LocalPosition) : null;
        public Transform TrackingOrigin { get; private set; }
        public Vector3 LocalPosition { get; private set; }
        
        public Mesh Mesh { get; private set; }
        public int ClosestTriangleIndex { get; private set; }
        
        public MeshSurfacePointAnchor(Transform trackingOrigin, Vector3 localPosition, Mesh mesh, int closestTriangleIndex)
        {
            TrackingOrigin = trackingOrigin ?? throw new ArgumentNullException(nameof(trackingOrigin));
            LocalPosition = localPosition;
            Mesh = mesh ?? throw new ArgumentNullException(nameof(mesh));
            ClosestTriangleIndex = closestTriangleIndex;
        }
    }
}
