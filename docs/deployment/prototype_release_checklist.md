# Prototype Release Checklist

With this update, ConceptGuard XR has moved from a document-only idea to a prototype structure that connects XR manipulation, circuit analysis, misconception feedback, and XR visualization. Before paid release, the items below must be verified.

## Demo-Ready Criteria

- The FastAPI API runs through Docker Compose.
- Users can grab components and connect terminals in the Unity XR scene.
- `POST /analyze` returns closed-circuit status, open-circuit status, series/parallel candidates, current, voltage, and brightness values.
- Unity receives `xr_scene.current_flow` and displays current flow with LineRenderer.
- The feedback panel displays questions, counterexamples, or direct explanations based on misconception risk.

## Required Productization Before Sale

1. **Device validation**: Validate FPS, thermals, controller input, and hand-tracking input on target devices such as Meta Quest 3 and Quest Pro.
2. **Content validation**: Align circuit-misconception rules with textbook and teacher-review standards.
3. **Legal and privacy**: Add a privacy policy and consent flow when storing student voice or behavior logs.
4. **Safeguards**: Restrict AI feedback to verified rubric-based responses, not unsupported claims of correctness.
5. **Operations**: Add teacher accounts, school tenants, session-retention rules, and log-deletion features.
6. **Payment and licensing**: Implement school-seat licensing, trials, and refund policies separately.

## Recommended Commercial Positioning

- Sell it as an **XR-based circuit misconception diagnostic lab**, not as a complete AI teacher.
- The initial target should be school or academy pilots, gifted-education centers, science museums, and edtech PoCs rather than broad B2C sales.
- For the first version, pilot packages or institutional licenses are lower-risk than monthly subscriptions.
