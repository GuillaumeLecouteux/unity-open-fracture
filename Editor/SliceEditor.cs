using UnityEditor;

namespace OpenFracture
{
    [CustomEditor(typeof(Slice))]
    [CanEditMultipleObjects]
    public class SliceEditor : Editor
    {
        // Empty editor required for custom property drawers to work properly
    }
}