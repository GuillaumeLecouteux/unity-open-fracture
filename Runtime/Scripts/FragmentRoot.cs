using UnityEngine;
using UnityEngine.Events;

namespace OpenFracture
{
    public class FragmentRoot : MonoBehaviour
    {
        [SerializeField]
        private UnityEvent _enableEvent;

        [SerializeField]
        private UnityEvent _fragmentCompletionEvent;
        public bool HasFragments => transform.childCount > 0;

        private void OnEnable()
        {
            _enableEvent?.Invoke();
        }

        private void OnFragmentCompletion()
        {
            _fragmentCompletionEvent?.Invoke();
        }

    }
}