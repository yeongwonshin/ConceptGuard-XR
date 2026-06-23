// Unity C# placeholder for sending circuit graph data to FastAPI.
// Attach this to a manager object in the XR scene.
using UnityEngine;

public class AnalyzeClient : MonoBehaviour
{
    public string apiBaseUrl = "http://localhost:8000";

    public void SendAnalyzeRequest(string circuitGraphJson)
    {
        Debug.Log("TODO: POST /analyze with circuit graph: " + circuitGraphJson);
    }
}
