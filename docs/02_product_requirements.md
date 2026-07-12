# Product Requirements Document

## 1. Users

- Middle-school or introductory high-school physics learners
- Science teachers
- After-school or edtech lab-session operators

## 2. User Stories

- As a student, I want to connect circuits directly and understand why a bulb lights up.
- As a student, I want to know where I was confused instead of receiving the correct answer immediately.
- As a teacher, I want to review which misconceptions students repeat after class.
- As a developer, I want to analyze learning logs and improve the feedback policy.

## 3. MVP Feature Priorities

| Priority | Feature | Completion Criteria |
|---|---|---|
| P0 | XR component placement and connection | A student can build a closed circuit with a battery, bulb, and wires. |
| P0 | Circuit-graph conversion | Unity connection state is sent to the server as a JSON graph. |
| P0 | Circuit judgment | The server returns whether the circuit is closed and whether it is series or parallel. |
| P0 | Feedback policy | An MRR-based feedback mode is selected. |
| P1 | LLM explanation analysis | Concept tags and error candidates are extracted from the learner explanation. |
| P1 | Counterexample simulation | Series and parallel comparisons are visualized in XR. |
| P2 | Teacher dashboard | Misconceptions and correction success rates are shown by session. |

## 4. Non-Functional Requirements

- Analysis response time: target under 2 seconds for the MVP.
- Fail-closed analysis: never replace an API failure with fabricated or cached success output.
- Privacy: store data with anonymous session IDs.
- Classroom usability: complete the tutorial in under 5 minutes.
- Connection transparency: show configuration and network failures clearly and disable analysis until the API is healthy.
