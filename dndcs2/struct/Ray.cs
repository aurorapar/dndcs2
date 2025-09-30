using System.Numerics;
using System.Runtime.InteropServices;

namespace Dndcs2.@struct;

/// <summary>
/// Represents a polymorphic ray shape used in spatial queries and collision detection.
/// This structure can represent a line, sphere, hull (AABB), capsule, or mesh—depending on the constructor used.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public struct Ray
{
    /// <summary>
    /// The ray data interpreted as an axis-aligned bounding box (AABB).
    /// </summary>
    [FieldOffset(0)] public Hull Hull;
    [FieldOffset(40)] public RayType Type = RayType.Hull;

    /// <summary>
    /// Initializes a ray as a hull (AABB) if bounds are not equal, otherwise defaults to a line.
    /// </summary>
    /// <param name="mins">Minimum bounding box coordinates.</param>
    /// <param name="maxs">Maximum bounding box coordinates.</param>
    public Ray(Vector3 mins, Vector3 maxs)
    {
        Hull = new Hull { Mins = mins, Maxs = maxs };     
    }
}

public enum RayType
{
    /// <summary>
    /// An axis-aligned bounding box (AABB) used for volume-based tracing.
    /// </summary>
    Hull,
}