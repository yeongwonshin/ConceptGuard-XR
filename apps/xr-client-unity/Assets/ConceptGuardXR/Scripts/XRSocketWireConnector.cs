using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

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
            if (fromSocket != null)
            {
                fromSocket.selectEntered.AddListener(OnSocketChanged);
                fromSocket.selectExited.AddListener(OnSocketExited);
            }

            if (toSocket != null)
            {
                toSocket.selectEntered.AddListener(OnSocketChanged);
                toSocket.selectExited.AddListener(OnSocketExited);
            }
        }

        private void OnDisable()
        {
            if (fromSocket != null)
            {
                fromSocket.selectEntered.RemoveListener(OnSocketChanged);
                fromSocket.selectExited.RemoveListener(OnSocketExited);
            }

            if (toSocket != null)
            {
                toSocket.selectEntered.RemoveListener(OnSocketChanged);
                toSocket.selectExited.RemoveListener(OnSocketExited);
            }
        }

        private void OnSocketChanged(SelectEnterEventArgs _)
        {
            TryCreateOrUpdateWire();
        }

        private void OnSocketExited(SelectExitEventArgs _)
        {
            if (activeWire == null)
            {
                return;
            }

            Destroy(activeWire.gameObject);
            activeWire = null;
            graphBuilder?.RecordEvent("disconnect");
        }

        private void TryCreateOrUpdateWire()
        {
            var fromNode = GetSelectedNode(fromSocket);
            var toNode = GetSelectedNode(toSocket);

            if (fromNode == null || toNode == null || fromNode == toNode)
            {
                return;
            }

            if (activeWire == null)
            {
                activeWire = wirePrefab != null
                    ? Instantiate(wirePrefab, transform)
                    : new GameObject("XRWireConnection").AddComponent<XRWireConnection>();
            }

            activeWire.Bind(
                fromNode,
                toNode,
                fromNode.GetClosestTerminal(fromSocket.transform.position),
                toNode.GetClosestTerminal(toSocket.transform.position)
            );

            graphBuilder?.RecordEvent(
                "connect",
                $"{{\"from\":\"{fromNode.ComponentId}\",\"to\":\"{toNode.ComponentId}\"}}"
            );
        }

        private XRComponentNode GetSelectedNode(XRSocketInteractor socket)
        {
            if (socket == null || !socket.hasSelection)
            {
                return null;
            }

            if (socket.interactablesSelected == null || socket.interactablesSelected.Count == 0)
            {
                return null;
            }

            IXRSelectInteractable interactable = socket.interactablesSelected[0];
            if (interactable == null || interactable.transform == null)
            {
                return null;
            }

            return interactable.transform.GetComponentInParent<XRComponentNode>();
        }
    }
}