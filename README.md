# Dessert Princess

2D 육성 시뮬레이션 게임 프로젝트입니다. Unity 화면/입력 계층과 순수 C# Core 로직을 분리하고, 랜덤/LLM 전략 기반 자동 플레이 시뮬레이터로 턴 진행, 이벤트, 엔딩, 경제 밸런스를 검증할 수 있도록 구성했습니다.

- Portfolio Page: https://sum1932.github.io/PrincessGrow/
- Repository: https://github.com/sum1932/PrincessGrow

![Dessert Princess title](docs/assets/title.jpg)

## Overview

- Genre: 2D, visual novel, simulation, casual
- Period: 2026.03.27 - 2026.05.31
- Engine: Unity 6 (`6000.2.15f1`)
- Language: C#
- Architecture: Headless POCO, Clean Architecture, Ports & Adapters
- Data: CSV / JSON driven content pipeline
- Simulation: Random strategy and LLM player strategy

## Responsibilities

- Core domain design for turn progression, stats, economy, events, inventory, and ending evaluation
- Unity-independent C# Core implementation for simulation and testability
- Unity adapter/view layer that presents Core state without embedding gameplay rules in UI code
- CSV/JSON repository layer for activity, event, and ending data
- Automated play simulation for balance checks and QA exploration
- LLM strategy integration for AI-driven playtesting

## Screenshots

![Main menu](docs/assets/main-menu.png)

![Schedule selection](docs/assets/schedule.png)

![Monthly result](docs/assets/monthly-result.png)

## Technical Highlights

### Unity-independent Core

Core gameplay rules are implemented as plain C# domain models and services, so the same game logic can run both inside Unity and in standalone console simulations.

Key files:

- `Assets/Scripts/Core/Domain/GameState.cs`
- `Assets/Scripts/Core/Services/TurnManager.cs`
- `Assets/Scripts/Core/Services/EventManager.cs`
- `Assets/Scripts/Core/Services/EndingJudge.cs`

### Ports & Adapters

Unity UI depends on interfaces such as `IGamePresenter` and `IGameInput`, while Core logic does not know about Unity scenes, prefabs, or input devices.

Key files:

- `Assets/Scripts/Adapters/Interfaces/IGamePresenter.cs`
- `Assets/Scripts/Adapters/Interfaces/IGameInput.cs`
- `Assets/Scripts/Adapters/Unity/UnityGamePresenter.cs`
- `Assets/Scripts/Controllers/GameController.cs`

### Data-driven Content

Activities, events, endings, NPC dialogue, and item data are managed through CSV/JSON and converted into domain objects through repository implementations.

Key files:

- `Assets/Scripts/Core/Data/IRepositories.cs`
- `Assets/Scripts/Core/Data/CsvActivityRepository.cs`
- `Assets/Scripts/Repositories/DatabaseActionRepository.cs`
- `Assets/Scripts/ExcelConverter/`

### Automated Simulation and LLM Playtesting

The standalone implementation can run repeated full-game simulations using interchangeable strategies. This makes it possible to compare random play, human-guided play, and LLM-guided play without opening Unity.

Key files:

- `Implementation/Simulation/GameSimulator.cs`
- `Implementation/TestConsole/LLMStrategy.cs`
- `Implementation/TestConsole/ILLMClient.cs`
- `Implementation/TestConsole/OpenAIClient.cs`
- `Implementation/TestConsole/GeminiClient.cs`
- `Implementation/TestConsole/KimiClient.cs`

## Repository Structure

```text
Assets/Scripts/
├── Core/               # Unity-independent gameplay logic
├── Adapters/           # Interfaces and Unity adapter implementations
├── Controllers/        # Thin MonoBehaviour orchestration layer
├── Views/              # UI presentation components
└── ExcelConverter/     # CSV/Excel data import tooling

Implementation/
├── Core/               # Standalone .NET Core logic
├── Simulation/         # Automated simulation runner
└── TestConsole/        # Interactive and LLM playtest console

docs/                   # GitHub Pages portfolio page
```

## Run Notes

Unity gameplay is intended to run through the Unity Editor. Standalone Core and simulation projects are under `Implementation/`.

API keys for LLM clients are not included in this repository. Use local environment variables or local configuration files that are excluded by `.gitignore`.
