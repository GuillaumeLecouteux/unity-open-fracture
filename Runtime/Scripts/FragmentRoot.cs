using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;
using Debug = UnityEngine.Debug;
using System;

namespace OpenFracture
{
    public class FragmentRoot : MonoBehaviour
    {
        [SerializeField]
        private UnityEvent _enableEvent;

        [SerializeField]
        private UnityEvent _fragmentCompletionEvent;
        public bool HasFragments => transform.childCount > 0;

        private Component[] _rigidbodies;

        private void OnEnable()
        {
            _enableEvent?.Invoke();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            int nbChildren = transform.childCount;
            int nbFragments = 0;

            foreach (Fragment f in GetComponentsInChildren<Fragment>())
                nbFragments++;

            if(nbChildren != nbFragments)
                Debug.LogWarningFormat("FragmentRoot {0} has {1} children and {2} fragments.", this.name, nbChildren, nbFragments);
        }
#endif

        public void OnFragmentCompletion()
        {
            _fragmentCompletionEvent?.Invoke();
        }

        public void UpdateFragmentsPhysics(bool enablePhysics = false)
        {
            _rigidbodies = GetComponentsInChildren<Rigidbody>();

            foreach (Rigidbody rb in _rigidbodies)
                rb.isKinematic = !enablePhysics;
        }

    }
}