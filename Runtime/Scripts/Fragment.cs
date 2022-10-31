using UnityEngine;

namespace OpenFracture
{
    public class Fragment : MonoBehaviour
    {
        [SerializeField]
        private bool _destroyParentOnDestroy;

        private void OnDestroy()
        {
            if (_destroyParentOnDestroy)
                Destroy(this.transform.parent);
        }
    }
}