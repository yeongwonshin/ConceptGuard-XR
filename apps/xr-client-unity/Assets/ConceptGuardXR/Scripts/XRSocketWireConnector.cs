using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace ConceptGuardXR
{
    public sealed class XRSocketWireConnector : MonoBehaviour
    {
        [SerializeField] private XRSocketInteractor fromSocket;
        [SerializeField] private XRSocketInteractor toSocket;
        [SerializeField] private XRWireConnection wirePrefab;
        [SerializeField] private CircuitGraphBuilder graphBuilder;

        private XRWireConnection activeWire;

        private void OnEnable()
        {
            if (fromSocket != null) fromSocket.selectEntered.AddListener(OnSocketChanged);
            if (toSocket != null) toSocket.selectEntered.AddListener(OnSocketChanged);
            if (fromSocket != null) fromSocket.selectExited.AddListener(OnSocketExited);
            if (toSocket != null) toSocket.selectExited.AddListener(OnSocketExited);
        }

        private void OnDisable()
        {
            if (fromSocket != null) fromSocket.selectEntered.RemoveListener(OnSocketChanged);
            if (toSocket != null) toSocket.selectEntered.RemoveListener(OnSocketChanged);
            if (fromSocket != null) fromSocket.selectExited.RemoveListener(OnSocketExited);
            if (toSocket != null) toSocket.selectExited.RemoveListener(OnSocketExited);
        }

        private void OnSocketChanged(SelectEnterEventArgs _)
        {
            TryCreateOrUpdateWire();
        }

        private void OnSocketExited(SelectExitEventArgs _)
        {
            if (activeWire != null)
            {
                Destroy(activeWire.gameObject);
                activeWire = null;
                graphBuilder?.RecordEvent("disconnect");
            }
        }

        private void TryCreateOrUpdateWire()
        {
            var fromNode = GetSelectedNode(fromSocket);
            var toNode = GetSelectedNode(toSocket);
            if (fromNode == null || toNode == null || fromNode == toNode) return;

            if (activeWire == null)
            {
                activeWire = wirePrefab != null
                    ? Instantiate(wirePrefab, transform)
                    : new GameObject("XRWireConnection").AddComponent<XRWireConnection>();
            }
            activeWire.Bind(fromNode, toNode, fromNode.GetClosestTerminal(fromSocket.transform.position), toNode.GetClosestTerminal(toSocket.transform.position));
            graphBuilder?.RecordEvent("connect", $"{{\"from\":\"{fromNode.ComponentId}\",\"to\":\"{toNode.ComponentId}\"}}");
        }

        private XRComponentNode GetSelectedNode(XRSocketInteractor socket)
        {
            if (socket == null || !socket.hasSelection) return null;
            var interactable = socket.GetOldestInteractableSelected();
            if (interactable == null) return null;
            return interactable.transform.GetComponentInParent<XRComponentNode>();
        }
    }
}
