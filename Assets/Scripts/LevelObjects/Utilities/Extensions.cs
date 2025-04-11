using UnityEngine;

namespace LevelEditorSystem
{
    /// <summary>
    /// Extension methods for Vector3 and related utilities.
    /// </summary>
    public static class Extensions
    {
        public static Vector3 WithZ(this Vector3 v, float z)
        {
            return new Vector3(v.x, v.y, z);
        }

        public static bool InViewport(this Vector3 v)
        {
            return v.x >= 0f && v.x <= 1f && v.y >= 0f && v.y <= 1f;
        }
    }
}