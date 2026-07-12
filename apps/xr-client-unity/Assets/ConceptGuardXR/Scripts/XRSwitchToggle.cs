using UnityEngine;

namespace ConceptGuardXR
{
    public sealed class XRSwitchToggle : MonoBehaviour
    {
        private XRComponentNode switchNode;
        private Transform lever;
        private CircuitGraphBuilder graphBuilder;
        private SessionEventLogger eventLogger;
        private Vector3 closedEuler = new Vector3(0f, 0f, -18f);
        private Vector3 openEuler = new Vector3(0f, 0f, 24f);

        public void Configure(
            XRComponentNode node,
            Transform leverTransform,
            CircuitGraphBuilder builder,
            SessionEventLogger logger)
        {
            switchNode = node;
            lever = leverTransform;
            graphBuilder = builder;
            eventLogger = logger;
            RefreshVisual();
        }

        public void Toggle()
        {
            if (switchNode == null)
            {
                return;
            }

            switchNode.SwitchClosed = !switchNode.SwitchClosed;
            RefreshVisual();
            var eventType = switchNode.SwitchClosed ? "switch_closed" : "switch_opened";
            if (eventLogger != null)
            {
                eventLogger.Log(eventType, $"{{\"component_id\":\"{switchNode.ComponentId}\"}}");
            }
            else
            {
                graphBuilder?.RecordEvent(eventType);
            }
        }

        private void RefreshVisual()
        {
            if (lever != null && switchNode != null)
            {
                lever.localEulerAngles = switchNode.SwitchClosed ? closedEuler : openEuler;
            }
        }
    }
}
