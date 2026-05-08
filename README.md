# MediCure — Intelligent Hospital Management System

A C# / ASP.NET Core Hospital Management System with integrated 
AI features, built as a QA portfolio and research project.

## Research Focus
Bridging the gap between hospital management and patients by 
addressing three core problems:
- Patient records going missing
- Long patient wait times  
- Poor communication between patients and hospital staff

## AI Components
| Component | Type | Purpose |
|---|---|---|
| No-Show Prediction | ML Model (Random Forest) | Predicts appointment no-show risk |
| Smart Scheduling | LLM (Anthropic API) | Suggests optimal appointment slots |
| Patient Communication Assistant | LLM (Anthropic API) | Generates personalised reminders |

## Tech Stack
- Backend: C# / ASP.NET Core 8
- Database: SQLite + Entity Framework Core 8
- AI/ML: ONNX Runtime + Anthropic API
- Testing: NUnit, Postman, Playwright

## Modules
- Patient Management
- Doctor Management
- Appointment Management
- Prescription Management
- Billing Management
- Lab Report Management

## QA Layer
| Tool | Coverage |
|---|---|
| NUnit | Unit tests — service logic, edge cases |
| Postman | API tests — status codes, validation |
| Playwright | E2E tests — user workflows |
| Manual | Role-based access control testing |

## Related Repository
[NoShow Prediction Model](https://github.com/ilma-wafa/NoShowModel)

## Model Setup
The `noshow_model.onnx` file is excluded from this repo (119MB).
To generate it:
1. Clone [NoShowModel repo](https://github.com/ilma-wafa/NoShowModel)
2. Download dataset from Kaggle
3. Run `python train_model.py`
4. Copy `noshow_model.onnx` into this project root

## API Documentation
Run the project and navigate to:
```
http://localhost:5239/swagger
```