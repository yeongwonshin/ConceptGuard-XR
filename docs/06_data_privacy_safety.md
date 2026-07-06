# Data Privacy and Safety

## Data Minimization

- Do not store students' real names.
- Store only session IDs, mission IDs, event logs, and error tags.
- Do not store raw voice recordings by default; use only STT results.

## AI Safeguards

- Limit the LLM to explanation analysis and wording assistance, not final answer grading.
- Block feedback that conflicts with circuit-engine verification results.
- When uncertainty is high, switch to a prompt such as "Let's check that again."
- Avoid stigmatizing wording in teacher reports.

## Educational Ethics

- Do not fix students as evaluation targets; present misconceptions as signals in the learning process.
- Center feedback on "What should we observe again?" rather than "You are wrong."
- Do not use learning data for purposes outside education.
