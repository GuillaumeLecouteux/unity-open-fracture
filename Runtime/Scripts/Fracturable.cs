using System.Diagnostics;
using Sirenix.OdinInspector;
using UnityEngine;

namespace OpenFracture
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Fracture))]
    public class Fracturable : MonoBehaviour
    {
        [Header("Required Bindings")]
        [SerializeField]
        [Required]
        private FracturableProfile _fracturableProfile;

        [SerializeField]
        private Fracture _fracture;

        [Header("Optional Binding for Baked Fracture")]
        [SerializeField]
        private FragmentRoot _fragmentRoot;

        [ShowInInspector]
        public bool HasFragments => _fragmentRoot? _fragmentRoot.HasFragments : false;

        private void Awake()
        {
            // disable and reset fragment root
            if (_fragmentRoot == null)
            {
                _fragmentRoot = Instantiate<FragmentRoot>(original: _fracturableProfile.fragmentRootPrefab, parent: this.transform);
                _fragmentRoot.name = ($"{this.name}-Fragments");
            }
            _fragmentRoot.gameObject.SetActive(false);
            _fragmentRoot.transform.localPosition = Vector3.zero;
            _fragmentRoot.transform.localRotation = Quaternion.identity;
            _fragmentRoot.transform.localScale = Vector3.one;

            CopyFractureProfile();
        }

        private void OnEnable()
        {
            _fracture.fractureComplete += OnFragmentCompletion;
        }

        private void OnDisable()
        {
            _fracture.fractureComplete -= OnFragmentCompletion;
        }

        [Button]
        public void Fracture()
        {
            if (!enabled)
                return;
            _fragmentRoot.transform.parent = this.transform.parent; // move one level up to avoid being destroyed into oblivion.
            if (HasFragments)
                ActivateBakedFracture();
            else
            {
                _fragmentRoot.gameObject.SetActive(true);
                _fracture.CauseFracture();
            }
        }

        private void ActivateBakedFracture()
        {
            _fragmentRoot.gameObject.SetActive(true);
            if (_fracturableProfile.destroyOriginal)
                Destroy(this.gameObject);
            else
                this.gameObject.SetActive(false);
            OnFragmentCompletion();
        }

        private void OnFragmentCompletion()
        {
            _fragmentRoot.OnFragmentCompletion();
        }

        [Button]
        private void CopyFractureProfile()
        {
            _fracture.FragmentRoot = this._fragmentRoot;
            _fracture._triggerOptions = _fracturableProfile.triggerOptions;
            _fracture._fractureOptions = _fracturableProfile.fractureOptions;
            _fracture._refractureOptions = _fracturableProfile.refractureOptions;
            _fracture.DestroyOriginal = _fracturableProfile.destroyOriginal;
            _fracture.FragmentRootPrefab = _fracturableProfile.fragmentRootPrefab;
            _fracture.FragmentPrefab = _fracturableProfile.fragmentPrefab;
        }

        void OnCollisionEnter(Collision collision)
        {
            if (_fracturableProfile.triggerOptions.triggerType == TriggerType.Collision)
            {
                if (collision.contactCount > 0)
                {
                    // Collision force must exceed the minimum force (F = I / T)
                    var contact = collision.contacts[0];
                    float collisionForce = collision.impulse.magnitude / Time.fixedDeltaTime;

                    // Colliding object tag must be in the set of allowed collision tags if filtering by tag is enabled
                    bool tagAllowed = _fracturableProfile.triggerOptions.IsTagAllowed(contact.otherCollider.gameObject.tag);

                    // Object is unfrozen if the colliding object has the correct tag (if tag filtering is enabled)
                    // and the collision force exceeds the minimum collision force.
                    if (collisionForce > _fracturableProfile.triggerOptions.minimumCollisionForce &&
                       (!_fracturableProfile.triggerOptions.filterCollisionsByTag || tagAllowed))
                    {
                        Fracture();
                    }
                }
            }
        }

        void OnTriggerEnter(Collider collider)
        {
            if (_fracturableProfile.triggerOptions.triggerType == TriggerType.Trigger)
            {
                // Colliding object tag must be in the set of allowed collision tags if filtering by tag is enabled
                bool tagAllowed = _fracturableProfile.triggerOptions.IsTagAllowed(collider.gameObject.tag);

                if (!_fracturableProfile.triggerOptions.filterCollisionsByTag || tagAllowed)
                {
                    Fracture();
                }
            }
        }
    }
}
