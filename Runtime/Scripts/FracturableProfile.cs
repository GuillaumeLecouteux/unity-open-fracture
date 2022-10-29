using UnityEngine;

namespace OpenFracture
{
    [CreateAssetMenu(fileName = "FracturableProfile", menuName = "OpenFracture")]
    public class FracturableProfile : ScriptableObject
    {
        [SerializeField]
        public bool destroyOriginal;
        [SerializeField]
        public FragmentRoot fragmentRootPrefab;
        [SerializeField]
        public Fragment fragmentPrefab;
        [SerializeField]
        public TriggerOptions triggerOptions;
        [SerializeField]
        public FractureOptions fractureOptions;
        [SerializeField]
        public RefractureOptions refractureOptions;
    }
}