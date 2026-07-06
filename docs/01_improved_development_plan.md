# 2026 AI/XR Service Developer Contest: Improved Development Plan

## Project Summary

### 1. Project Overview

**Project Name**  
**ConceptGuard XR: An AI-Based XR Science Tutor That Reduces Misconception Reinforcement Risk**

**One-Line Definition**  
An inquiry-based AI tutor that analyzes both XR manipulation behavior and natural-language explanations, then adjusts feedback intensity before science misconceptions become reinforced.

**Core Problem**  
Many existing AI tutors only look at what students say or at their final answers. In electric-circuit learning, however, misconception diagnosis depends on how students connect components, whether they confuse series and parallel structures, and how their predictions differ from observations.

**Solution Approach**  
ConceptGuard XR combines circuit manipulation logs, graph structure, simulation results, and learner explanations to compute `Spatial-Conceptual Discrepancy (SCD)` and `Misconception Reinforcement Risk (MRR)`. Based on risk level, it selects hints, check questions, counterexample simulations, or direct explanations.

### 2. Team Roles

| Role | Owner | Deliverables |
|---|---|---|
| Project lead / algorithm | Shin Youngwon | CUG-XR algorithm, evaluation metrics, schedule and presentation lead |
| XR client | Kim Gyuri | Unity MR/VR manipulation, circuit component placement and connection, visualization UI |
| AI backend | Jang Hyunwoo | FastAPI, LLM/RAG, misconception scoring, feedback policy |
| 3D / scenarios / demo | Lee Youngin | Blender models, learning scenarios, demo video and storyboard |

### 3. Development Background and Purpose

#### Background
Generative AI tutors can provide fast explanations and customized feedback, but they often fail to identify which concept a student applied incorrectly. Electric circuits require integrated understanding of current, voltage, resistance, series/parallel structures, spatial connections, and physical laws. A student's words or final answer alone may miss important misconception signals.

#### Purpose
This project aims to build an AI/XR learning system that does not simply provide correct answers. Instead, it detects moments when a misconception may be repeated and reinforced, then adjusts feedback accordingly. Learners experiment directly in XR space, while the AI provides question-based and inquiry-based feedback grounded in mismatches between behavior and explanation.

#### Expected Effects
- Detect misconception signals that ordinary chatbots miss by combining manipulation data and natural-language explanations.
- Support embodied understanding of abstract concepts by visualizing current flow, voltage distribution, and brightness changes in XR.
- Provide teachers with dashboards for vulnerable concepts, repeated errors, and correction success rates.
- Offer safe and repeatable experiment experiences even in environments with limited science-lab equipment.

## Development Plan

### 1. Service Scenario

1. The learner places batteries, bulbs, resistors, switches, and wires in XR space to build a mission circuit.
2. The system converts component connections into a graph and calculates closed-circuit status, series/parallel structure, current, voltage, and brightness.
3. The learner explains, by voice or text, why the circuit was connected that way.
4. The AI extracts concept tags and confidence expressions from the explanation.
5. Misconception risk is calculated from mismatches among circuit structure, prediction, explanation, and simulation results.
6. Low risk leads to minimal hints; medium risk leads to check questions; high risk leads to counterexample simulations; very high risk leads to direct explanations and a request to try again.
7. After the session, the teacher reviews each student's misconception patterns and feedback effects in a report.

### 2. MVP Learning Missions

| Mission | Learning Goal | Representative Misconception | XR Feedback |
|---|---|---|---|
| M1. Build a circuit that lights a bulb | Understand closed-circuit conditions | Believing current flows with only one side connected | Highlight the break and do not visualize current flow |
| M2. Compare series and parallel brightness | Understand how connection structure relates to brightness | Generalizing that more bulbs always means dimmer bulbs | Color-code series/parallel graphs and compare brightness |
| M3. What happens when resistance changes? | Understand the relationship among resistance, current, and voltage | Believing resistance consumes current | Compare current before and after resistance and show a counterexample simulation |

### 3. Core Features

- **XR circuit manipulation**: Unity-based grabbing, placing, connecting, and rearranging of 3D circuit components
- **Circuit analysis engine**: Graph-based connection judgment, series/parallel classification, and simple current/voltage/brightness calculation
- **Multimodal misconception detection**: Analysis of manipulation sequence, circuit structure, learner explanation, and prediction values together
- **CUG-XR feedback policy**: Risk-based selection of hints, questions, counterexamples, and direct explanations
- **Teacher dashboard**: Error type, hint usage, correction success after feedback, and concept-level vulnerability display
- **Safe use of generative AI**: The LLM analyzes explanations and generates wording, while final correctness is determined by the circuit engine

### 4. Proposed Algorithm: CUG-XR

CUG-XR stands for `Concept-Uncertainty Guided XR Feedback`. Its key idea is to avoid treating a single learner behavior or explanation as the only evidence. Instead, it cross-checks multiple forms of evidence to decide the feedback strategy.

#### Inputs
- `CircuitGraph`: component nodes and connection edges
- `ManipulationLog`: placement, connection, deletion, and retry order
- `SimulationState`: closed status, current, voltage, and brightness
- `LearnerExplanation`: learner voice or text explanation
- `Prediction`: learner prediction about brightness or current change

#### Intermediate Metrics
- `SCD`: Spatial-Conceptual Discrepancy, mismatch between spatial manipulation and conceptual explanation
- `CEI`: Core Error Impact, severity of a core concept error
- `RER`: Repeated Error Rate, repetition rate for the same error
- `UCS`: Uncertainty/Confidence Signal, conflict between confidence expressions and actual error
- `POD`: Prediction-Observation Difference, gap between prediction and observation

#### Outputs
- `MRR`: Misconception Reinforcement Risk score
- `FeedbackMode`: minimal_hint, check_question, counterexample_simulation, direct_explanation, retry_task

#### Policy Examples
| MRR Range | Feedback Type | Example |
|---|---|---|
| 0.00-0.25 | Minimal hint | "Can you check whether there is a closed path for current to pass through?" |
| 0.26-0.50 | Check question | "Are the two bulbs on the same path or on different paths?" |
| 0.51-0.75 | Counterexample simulation | "Let's switch between parallel and series circuits and compare the brightness difference." |
| 0.76-1.00 | Direct explanation + retry | "Current is not consumed by the bulb; it flows through the whole circuit. Try connecting it again." |

### 5. Technical Structure

| Area | Technology | Role |
|---|---|---|
| XR client | Unity, XR Interaction Toolkit, OpenXR, Meta XR SDK | Component manipulation, connection feedback, visualization |
| 3D assets | Blender, glTF/FBX | Circuit components, learning space, demo objects |
| API server | FastAPI, Pydantic | Session collection, analysis requests, feedback responses |
| Circuit engine | Python, NetworkX | Circuit graph analysis, series/parallel judgment, basic calculation |
| AI/RAG | LLM API, vector search | Explanation analysis, concept-card retrieval, feedback sentence generation |
| Dashboard | React/Next.js | Teacher reports and student-level diagnosis visualization |
| Storage | SQLite -> PostgreSQL | Session logs, misconception tags, feedback history |

### 6. Development Schedule

| Phase | Duration | Goal | Key Deliverables |
|---|---|---|---|
| Phase 1 | 1 week | Planning refinement | Three missions, misconception DB, evaluation rubric |
| Phase 2 | 2 weeks | XR manipulation MVP | Unity component placement/connection, basic UI, visualization |
| Phase 3 | 2 weeks | Analysis engine | Circuit-graph conversion, series/parallel judgment, current/voltage calculation |
| Phase 4 | 2 weeks | AI feedback | Explanation analysis, CUG-XR scoring, RAG hint generation |
| Phase 5 | 1 week | Dashboard/logging | Session logs, teacher reports, correction success rate |
| Phase 6 | 1 week | Validation/demo | Demo video, ablation, presentation materials, deployment package |

### 7. Validation Plan

#### Comparison Baselines
- Simple LLM tutor
- Direct-answer tutor
- Rule-based feedback tutor
- XR visualization-only content

#### Core Evaluation Metrics
- `FCR`: Feedback Correction Rate, correction success rate after feedback
- `MRR Reduction`: reduction in misconception reinforcement risk within a session
- `SCD Reduction`: reduction in manipulation-explanation mismatch
- `HUR`: Hint Usefulness Rating
- `OAI`: Over-Assistant Intervention, degree of excessive intervention
- `Delayed Retention`: performance on the same concept after one week

#### Ablation Studies
- Remove manipulation logs
- Remove natural-language explanation analysis
- Remove counterexample simulation
- Remove feedback-intensity adaptation
- Remove RAG-based evidence explanations

## Generative AI Usage Plan

■ Used  
□ Not used

Generative AI is used only for understanding student explanations and generating feedback wording, not as the final judge of correctness. Circuit correctness is determined by graph analysis and simulation logic to reduce hallucination risk.

### Scope of Use
- Extract core concepts such as current, voltage, resistance, series, parallel, and closed circuit from learner explanations
- Classify possible misconception candidates when manipulation logs and explanations conflict
- Generate short hints, check questions, and counterexample explanations grounded in RAG-based concept cards
- Summarize vulnerable concepts and repeated errors for teachers after a session

### Safety and Privacy Protection
- Do not store student names, contact information, school identifiers, or other direct identifiers; use anonymous session IDs.
- Convert voice input to text, then discard raw audio or provide a local-storage option.
- Provide LLM output only after passing prohibited-expression filters and circuit-verification checks.
- When uncertainty is high, output a re-check question instead of a definitive answer.
- In teacher reports, use wording centered on "concepts that need more inquiry" instead of stigmatizing student labels.

## Differentiation Summary

ConceptGuard XR is differentiated by not using XR as a simple visualization tool. XR manipulation itself becomes diagnostic data, and AI combines that data with natural-language explanations to calculate misconception reinforcement risk. In other words, this project expands educational XR from "XR that shows" to "XR that diagnoses and helps recover thinking."
