using Slothsoft.UnityExtensions;
using UnityEngine;

namespace FollowYourDreams {
    [ExecuteAlways]
    abstract class DimensionEnumController : MonoBehaviour {
        [Header("Setup")]
        [SerializeField, Expandable]
        protected GameManager manager;

        Dimension previousDimension;

        [ContextMenu(nameof(Start))]
        protected void Start() {
            if (manager) {
                previousDimension = manager.currentDimension;
                OnSetDimension(previousDimension);
            }
        }

        protected void FixedUpdate() {
            CheckDimension();
        }
        protected void Update() {
            CheckDimension();
        }
        protected void LateUpdate() {
            CheckDimension();
        }

        void CheckDimension() {
            if (manager) {
                if (previousDimension != manager.currentDimension) {
                    previousDimension = manager.currentDimension;
                    OnSetDimension(previousDimension);
                }
            }
        }
        protected abstract void OnSetDimension(Dimension dimension);
    }
}
