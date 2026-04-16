using UnityEngine;


namespace Unity.VRTemplate
{
    [RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals.XRInteractorLineVisual))]
    public class LineVisualSnapThresholdOverride : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Snap threshold distance override value.")]
        float m_SnapThresholdDistance = 100f;

        void Awake()
        {
            // In future versions of the XR Interaction Toolkit, this will be exposed in the inspector and it'll be possible to disable this.
            var rayVisual = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals.XRInteractorLineVisual>();
            rayVisual.snapThresholdDistance = m_SnapThresholdDistance;
        }
    }
}
