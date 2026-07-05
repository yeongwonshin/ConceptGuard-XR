using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace ConceptGuardXR
{
    [RequireComponent(typeof(XRBaseInteractable))]
    public sealed class XRSwitchToggle : MonoBehaviour
    {
        [SerializeField] private XRComponentNode switchNode;
        [SerializeField] private Transform lever;
        [SerializeField] private Vector3 closedEuler = new Vector3(-25f, 0f, 0f);
        [SerializeField] private Vector3 openEuler = new Vector3(25f, 0f, 0f);
        [SerializeField] private CircuitGraphBuilder graphBuilder;

        private XRBaseInteractable interactable;

        private void Awake()
        {
            interactable = GetComponent<XRBaseInteractable>();
            if (switchNode == null) switchNode = GetComponentInParent<XRComponentNode>();
        }

        private void OnEnable()
        {
            interactable.selectEntered.AddListener(OnSelected);
        }

        private void OnDisable()
        {
            interactable.selectEntered.RemoveListener(OnSelected);
        }

        private void OnSelected(SelectEnterEventArgs _)
        {
            if (switchNode == null) return;
            switchNode.SwitchClosed = !switchNode.SwitchClosed;
            if (lever != null)
            {
                lever.localEulerAngles = switchNode.SwitchClosed ? closedEuler : openEuler;
            }
            graphBuilder?.RecordEvent(switchNode.SwitchClosed ? "switch_closed" : "switch_opened");
        }
    }
}
