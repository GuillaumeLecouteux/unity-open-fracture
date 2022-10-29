using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Scripting;

namespace OpenFracture
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(Rigidbody))]
    public class Fracture : MonoBehaviour
    {
        [SerializeField]
        private bool _destroyOriginal = true;
        public bool DestroyOriginal { get => _destroyOriginal; set => _destroyOriginal = value; }
        [SerializeField]
        private FragmentRoot _fragmentRootPrefab;

        public FragmentRoot FragmentRootPrefab { get => _fragmentRootPrefab; set => _fragmentRootPrefab = value; }
        [SerializeField]
        private Fragment _fragmentPrefab;
        public Fragment FragmentPrefab { get => _fragmentPrefab; set => _fragmentPrefab = value; }

        [SerializeField]
        private FragmentRoot _fragmentRoot;
        public FragmentRoot FragmentRoot { get => _fragmentRoot; set => _fragmentRoot = value; }
        
        public TriggerOptions _triggerOptions;
        public FractureOptions _fractureOptions;
        public RefractureOptions _refractureOptions;
        public CallbackOptions _callbackOptions;

        /// <summary>
        /// The number of times this fragment has been re-fractured.
        /// </summary>
        [HideInInspector]
        public int currentRefractureCount = 0;

        [ContextMenu("Print Mesh Info")]
        public void PrintMeshInfo()
        {
            var mesh = this.GetComponent<MeshFilter>().mesh;
            Debug.Log("Positions");

            var positions = mesh.vertices;
            var normals = mesh.normals;
            var uvs = mesh.uv;

            for (int i = 0; i < positions.Length; i++)
            {
                Debug.Log($"Vertex {i}");
                Debug.Log($"POS | X: {positions[i].x} Y: {positions[i].y} Z: {positions[i].z}");
                Debug.Log($"NRM | X: {normals[i].x} Y: {normals[i].y} Z: {normals[i].z} LEN: {normals[i].magnitude}");
                Debug.Log($"UV  | U: {uvs[i].x} V: {uvs[i].y}");
                Debug.Log("");
            }
        }

        [ExecuteInEditMode]
        [ContextMenu("CauseFracture")]
        public void CauseFracture()
        {
            _callbackOptions.CallOnFracture(null, gameObject, transform.position);
            this.ComputeFracture();
        }

        void OnValidate()
        {
            if (this.transform.parent != null)
            {
                // When an object is fractured, the fragments are created as children of that object's parent.
                // Because of this, they inherit the parent transform. If the parent transform is not scaled
                // the same in all axes, the fragments will not be rendered correctly.
                var scale = this.transform.parent.localScale;
                if ((scale.x != scale.y) || (scale.x != scale.z) || (scale.y != scale.z))
                {
                    Debug.LogWarning($"Warning: Parent transform of fractured object must be uniformly scaled in all axes or fragments will not render correctly.", this.transform);
                }
            }
        }

        /// <summary>
        /// Compute the fracture and create the fragments
        /// </summary>
        /// <returns></returns>
        private void ComputeFracture()
        {
            var mesh = this.GetComponent<MeshFilter>().sharedMesh;

            if (mesh != null)
            {
                // If the fragment root object has not yet been created, create it now
                if (this._fragmentRoot == null)
                {
                    // Create a game object to contain the fragments
                    this._fragmentRoot = Instantiate<FragmentRoot>(original: _fragmentRootPrefab, parent: this.transform.parent);
                    this._fragmentRoot.name = ($"{this.name}-Fragments");
                }
                // Each fragment will handle its own scale
                this._fragmentRoot.transform.position = this.transform.position;
                this._fragmentRoot.transform.rotation = this.transform.rotation;
                this._fragmentRoot.transform.localScale = UnityEngine.Vector3.one;

                var fragmentTemplate = CreateFragmentTemplate();

                if (_fractureOptions.asynchronous)
                {
                    StartCoroutine(Fragmenter.FractureAsync(
                        this.gameObject,
                        this._fractureOptions,
                        fragmentTemplate,
                        this._fragmentRoot.transform,
                        () =>
                        {
                            // Done with template, destroy it
                            GameObject.Destroy(fragmentTemplate);

                            // Deactivate the original object
                            this.gameObject.SetActive(false);

                            // Fire the completion callback
                            if ((this.currentRefractureCount == 0) ||
                                (this.currentRefractureCount > 0 && this._refractureOptions.invokeCallbacks))
                            {
                                if (_callbackOptions.onCompleted != null)
                                {
                                    _callbackOptions.onCompleted.Invoke();
                                }
                            }
                            if (_destroyOriginal)
                                GameObject.Destroy(this.gameObject);
                        }
                    ));
                }
                else
                {
                    Fragmenter.Fracture(this.gameObject,
                                        this._fractureOptions,
                                        fragmentTemplate,
                                        this._fragmentRoot.transform);

                    // Done with template, destroy it
                    GameObject.Destroy(fragmentTemplate);

                    // Deactivate the original object
                    this.gameObject.SetActive(false);

                    // Fire the completion callback
                    if ((this.currentRefractureCount == 0) ||
                        (this.currentRefractureCount > 0 && this._refractureOptions.invokeCallbacks))
                    {
                        if (_callbackOptions.onCompleted != null)
                        {
                            _callbackOptions.onCompleted.Invoke();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates a template object which each fragment will derive from
        /// </summary>
        /// <param name="preFracture">True if this object is being pre-fractured. This will freeze all of the fragments.</param>
        /// <returns></returns>
        private GameObject CreateFragmentTemplate()
        {
            // If pre-fracturing, make the fragments children of this object so they can easily be unfrozen later.
            // Otherwise, parent to this object's parent
            GameObject obj = new GameObject();
            obj.name = "Fragment";
            obj.tag = this.tag;

            // Update mesh to the new sliced mesh
            obj.AddComponent<MeshFilter>();

            // Add materials. Normal material goes in slot 1, cut material in slot 2
            var meshRenderer = obj.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterials = new Material[2] {
            this.GetComponent<MeshRenderer>().sharedMaterial,
            this._fractureOptions.insideMaterial
        };

            // Copy collider properties to fragment
            var thisCollider = this.GetComponent<Collider>();
            var fragmentCollider = obj.AddComponent<MeshCollider>();
            fragmentCollider.convex = true;
            fragmentCollider.sharedMaterial = thisCollider.sharedMaterial;
            fragmentCollider.isTrigger = thisCollider.isTrigger;

            // Copy rigid body properties to fragment
            var thisRigidBody = this.GetComponent<Rigidbody>();
            var fragmentRigidBody = obj.AddComponent<Rigidbody>();
            fragmentRigidBody.velocity = thisRigidBody.velocity;
            fragmentRigidBody.angularVelocity = thisRigidBody.angularVelocity;
            fragmentRigidBody.drag = thisRigidBody.drag;
            fragmentRigidBody.angularDrag = thisRigidBody.angularDrag;
            fragmentRigidBody.useGravity = thisRigidBody.useGravity;

            // If refracturing is enabled, create a copy of this component and add it to the template fragment object
            if (_refractureOptions.enableRefracturing &&
               (this.currentRefractureCount < _refractureOptions.maxRefractureCount))
            {
                CopyFractureComponent(obj);
            }

            return obj;
        }

        /// <summary>
        /// Convenience method for copying this component to another component
        /// </summary>
        /// <param name="obj">The GameObject to copy the component to</param>
        private void CopyFractureComponent(GameObject obj)
        {
            var fractureComponent = obj.AddComponent<Fracture>();

            fractureComponent._triggerOptions = this._triggerOptions;
            fractureComponent._fractureOptions = this._fractureOptions;
            fractureComponent._refractureOptions = this._refractureOptions;
            fractureComponent._callbackOptions = this._callbackOptions;
            fractureComponent.currentRefractureCount = this.currentRefractureCount + 1;
            fractureComponent._fragmentRoot = this._fragmentRoot;
        }
    }
}