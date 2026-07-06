# Evaluation Plan

## Evaluation Questions

1. Does ConceptGuard XR improve misconception correction rates compared with a simple LLM tutor?
2. Does including XR manipulation logs improve misconception detection accuracy?
3. Does adaptive feedback intensity improve correction success without reducing learner inquiry engagement?

## Study Design

- Pre-concept test -> XR learning session -> post-concept test -> delayed test after one week
- Comparison groups: simple LLM, direct-answer tutor, XR visualization only, and ConceptGuard XR
- Quantitative metrics: FCR, MRR reduction, SCD reduction, hint usage, and retry count
- Qualitative metrics: student interviews, teacher observations, and hint-usefulness surveys

## Demo Ablation

- A: Remove manipulation logs
- B: Remove natural-language explanation analysis
- C: Remove feedback adaptation
- D: Remove RAG-based evidence explanations
- Full: Include all features
