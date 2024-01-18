using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Original source: https://github.com/chgatla-microsoft/QRTracking
/// </summary>
namespace QRTracking
{
    /// <summary>
    /// Visualizes detected QR codes by instantiating qrCodePrefabs
    /// Modification adds, removes and updates ObjectFrames to SessionState detections list
    /// </summary>
    public class QRCodesObjectFrameInjector : MonoBehaviour
    {
        internal class GameObjectFrame
        {
            public GameObject gameObject;
            public TrackedObject objectFrame;
        }

        [SerializeField]
        private Transform CharucoFrame;

        // Prefab should contain a SpatialGraphNodeTracker
        public SpatialGraphNodeTracker qrCodePrefab;

        private System.Collections.Generic.SortedDictionary<System.Guid, GameObjectFrame> qrCodesGameObjectFrames = new SortedDictionary<System.Guid, GameObjectFrame>();
        private bool clearExisting = false;

        struct ActionData
        {
            public enum Type
            {
                Added,
                Updated,
                Removed
            };
            public Type type;
            public Microsoft.MixedReality.QR.QRCode qrCode;

            public ActionData(Type type, Microsoft.MixedReality.QR.QRCode qRCode) : this()
            {
                this.type = type;
                qrCode = qRCode;
            }
        }

        private System.Collections.Generic.Queue<ActionData> pendingActions = new Queue<ActionData>();

        // Use this for initialization
        void Start()
        {
            Debug.Log("QRCodesVisualizer start");

            QRCodesManager.Instance.QRCodesTrackingStateChanged += Instance_QRCodesTrackingStateChanged;
            QRCodesManager.Instance.QRCodeAdded += Instance_QRCodeAdded;
            QRCodesManager.Instance.QRCodeUpdated += Instance_QRCodeUpdated;
            QRCodesManager.Instance.QRCodeRemoved += Instance_QRCodeRemoved;
        }

        private void OnDestroy()
        {
            if (QRCodesManager.Instance != null)
            {
                QRCodesManager.Instance.QRCodeRemoved -= Instance_QRCodeRemoved;
                QRCodesManager.Instance.QRCodeUpdated -= Instance_QRCodeUpdated;
                QRCodesManager.Instance.QRCodeAdded -= Instance_QRCodeAdded;
                QRCodesManager.Instance.QRCodesTrackingStateChanged -= Instance_QRCodesTrackingStateChanged;
            }
        }

        private void Instance_QRCodesTrackingStateChanged(object sender, bool status)
        {
            if (!status)
            {
                clearExisting = true;
            }
        }

        private void Instance_QRCodeAdded(object sender, QRCodeEventArgs<Microsoft.MixedReality.QR.QRCode> e)
        {
            lock (pendingActions)
            {
                pendingActions.Enqueue(new ActionData(ActionData.Type.Added, e.Data));
            }
        }

        private void Instance_QRCodeUpdated(object sender, QRCodeEventArgs<Microsoft.MixedReality.QR.QRCode> e)
        {
            lock (pendingActions)
            {
                pendingActions.Enqueue(new ActionData(ActionData.Type.Updated, e.Data));
            }
        }

        private void Instance_QRCodeRemoved(object sender, QRCodeEventArgs<Microsoft.MixedReality.QR.QRCode> e)
        {
            lock (pendingActions)
            {
                pendingActions.Enqueue(new ActionData(ActionData.Type.Removed, e.Data));
            }
        }

        private void HandleEvents()
        {
            lock (pendingActions)
            {
                while (pendingActions.Count > 0)
                {
                    var action = pendingActions.Dequeue();
                    if (action.type == ActionData.Type.Added || (action.type == ActionData.Type.Updated && !qrCodesGameObjectFrames.ContainsKey(action.qrCode.Id)))
                    {
                        GameObject qrCodeObject = Instantiate(qrCodePrefab.gameObject, new Vector3(0, 0, 0), Quaternion.identity);
                        qrCodeObject.GetComponent<SpatialGraphNodeTracker>().Id = action.qrCode.SpatialGraphNodeId;
                        var simple = qrCodeObject.GetComponent<QRCodeSimple>();
                        if (simple)
                        {
                            simple.qrCode = action.qrCode;
                        }

                        var objectFrame = new TrackedObject()
                        {
                            id = TrackedObject.IdCount++,
                            label = action.qrCode.Data
                        };

                        qrCodesGameObjectFrames.Add(action.qrCode.Id,
                            new GameObjectFrame()
                            {
                                gameObject = qrCodeObject,
                                objectFrame = objectFrame
                            });


                        SessionState.TrackedObjects.Add(objectFrame);
                    }
                    else if (action.type == ActionData.Type.Removed)
                    {
                        if (qrCodesGameObjectFrames.ContainsKey(action.qrCode.Id))
                        {
                            SessionState.TrackedObjects.Remove(qrCodesGameObjectFrames[action.qrCode.Id].objectFrame);
                            Destroy(qrCodesGameObjectFrames[action.qrCode.Id].gameObject);
                            qrCodesGameObjectFrames.Remove(action.qrCode.Id);                            
                        }
                    }
                }
            }

            if (clearExisting)
            {
                clearExisting = false;
                foreach (var obj in qrCodesGameObjectFrames)
                {
                    Destroy(obj.Value.gameObject);
                    SessionState.TrackedObjects.Remove(obj.Value.objectFrame);
                }
                qrCodesGameObjectFrames.Clear();
            }
        }

        // Update is called once per frame
        void Update()
        {
            HandleEvents();

            // Convert QR World Coordinates to local coordinates in Charuco Frame
            foreach (var gof in qrCodesGameObjectFrames)
            {
                gof.Value.objectFrame.position = CharucoFrame.InverseTransformPoint(gof.Value.gameObject.transform.position);
                gof.Value.objectFrame.rotation = Quaternion.Inverse(CharucoFrame.rotation) * gof.Value.gameObject.transform.rotation * Quaternion.AngleAxis(90, Vector3.right);
            }
        }
    }
}