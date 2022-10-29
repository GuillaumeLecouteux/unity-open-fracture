using UnityEditor;

namespace OpenFracture
{
    [CustomEditor(typeof(Fracture))]
    [CanEditMultipleObjects]
    public class FractureEditor : Editor
    {
        // Empty editor required for custom property drawers to work properly
    }
}