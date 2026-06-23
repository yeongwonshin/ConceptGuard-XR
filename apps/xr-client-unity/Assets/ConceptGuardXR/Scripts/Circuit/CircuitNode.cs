using UnityEngine;

public enum CircuitNodeType { Battery, Bulb, Resistor, Switch, WireJunction }

public class CircuitNode : MonoBehaviour
{
    public string nodeId;
    public CircuitNodeType nodeType;
}
