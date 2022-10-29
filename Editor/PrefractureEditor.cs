using UnityEditor;

namespace OpenFracture
{
    [CustomEditor(typeof(Prefracture))]
    [CanEditMultipleObjects]
    public class PrefractureEditor : Editor
    {
        // Empty editor required for custom property drawers to work properly
    }
}