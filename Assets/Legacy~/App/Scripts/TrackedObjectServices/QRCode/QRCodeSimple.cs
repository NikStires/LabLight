using UnityEngine;

namespace QRTracking
{
    [RequireComponent(typeof(QRTracking.SpatialGraphNodeTracker))]
    public class QRCodeSimple : MonoBehaviour
    {
        private Microsoft.MixedReality.QR.QRCode code;
        public Microsoft.MixedReality.QR.QRCode qrCode
        {
            set
            {
                code = value;
                UpdateSize();
            }
        }


        [SerializeField]
        private GameObject Quad;

        private void UpdateSize()
        {
            // Update properties that change
            if (code != null && Quad != null)
            {
                Quad.transform.localPosition = new Vector3(code.PhysicalSideLength / 2.0f, code.PhysicalSideLength / 2.0f, 0.0f);
                Quad.transform.localScale = new Vector3(code.PhysicalSideLength, code.PhysicalSideLength, 0.005f);
            }
        }
    }
}