# AGENTS.md - Coding Guidelines for Agentic AI

## Build/Test Commands

### Unity Project
Unity builds are done through Unity Editor. No command-line build is configured.

To check for compilation errors:
1. Open Unity Editor
2. Check Console window for any compilation errors
3. All C# scripts are located in `Assets/Scripts/`

## Code Style Guidelines

### C# / Unity Code Style

#### Naming Conventions
- **Classes/Structs**: PascalCase (e.g., `GameState`, `TurnManager`)
- **Interfaces**: PascalCase with `I` prefix (e.g., `ITurnManager`, `IEventManager`)
- **Methods**: PascalCase (e.g., `EndTurn()`, `ProcessEvents()`)
- **Properties**: PascalCase (e.g., `CurrentTurn`, `IsComplete`)
- **Private fields**: underscore prefix + camelCase (e.g., `_gameState`, `_turnManager`)
- **Public fields**: PascalCase (avoid public fields, use properties)
- **Constants**: PascalCase (e.g., `MaxTurns`, `DefaultMoney`)
- **Enums**: PascalCase, values use PascalCase
- **Events**: PascalCase with `On` prefix or `Event` suffix

#### Imports & Namespaces
```csharp
// System imports first
using System;
using System.Collections.Generic;
using System.Linq;

// Unity imports second (only in Unity-specific files)
using UnityEngine;
using UnityEngine.SceneManagement;

// Project imports third (ordered by layer)
using DessertKingdom.Core.Domain;
using DessertKingdom.Core.Services;
using DessertKingdom.Core.Data;
using DessertKingdom.Adapters.Interfaces;
using DessertKingdom.Adapters.Unity;
using DessertKingdom.Views;

namespace DessertKingdom.Core.Domain  // Match folder structure
{
    // Code here
}
```

#### Types & Nullability
- Use `Nullable<enable>` in all .csproj files (already configured)
- Always check for null on Unity components and event handlers
- Use `?.` null-conditional operator for safe navigation
- Use `??` null-coalescing operator for defaults
- Value types: `int`, `float`, `bool`, `string` (not wrappers)

#### Formatting
- Use 4 spaces for indentation
- Opening braces on same line for methods/classes
- Single blank line between methods
- Max line length: 120 characters
- Trailing commas in multi-line collections

#### Comments & Documentation
```csharp
/// <summary>
/// Brief description of what the method does
/// </summary>
/// <param name="paramName">Description of parameter</param>
/// <returns>Description of return value</returns>
public ReturnType MethodName(ParamType paramName)
{
    // Implementation
}

// Single-line comments use //
/* Multi-line comments
   use this format */
```

#### Error Handling
- Use exceptions for exceptional cases only
- Return `bool` success/failure for expected error cases
- Log errors with `Debug.LogError()` in Unity
- Log warnings with `Debug.LogWarning()` for recoverable issues
- Use `try-catch` for external operations (file I/O, network)

#### Unity Specific
- Mark serialized fields with `[SerializeField]`
- Use `[Header("Section Name")]` to organize Inspector fields
- Implement `MonoBehaviour` lifecycle methods: `Awake()`, `Start()`, `OnDestroy()`
- Use `DontDestroyOnLoad()` for singletons
- Cache component references in `Awake()` or `Start()`

#### Architecture Patterns
- **Headless POCO**: All game logic in Core/ is Unity-independent
- **Adapter Pattern**: Unity-specific code isolated in Adapters/Unity/
- **Core Layer**: Pure C# with no Unity dependencies
- **View Layer**: Unity MonoBehaviours for UI only
- **Events**: Use C# events with `Action<T>` delegates
- **Repositories**: Data access abstraction in Core/Data/
- **ScriptableObjects**: Store game data in Resources/GameData/

#### File Organization
```
Assets/Scripts/
├── Core/               # Pure C# game logic (NO Unity dependencies)
│   ├── Domain/         # Models, Enums
│   ├── Services/       # Business logic
│   └── Data/           # Repositories, data access
├── Adapters/           # Unity integration layer
│   ├── Interfaces/     # Adapter contracts
│   └── Unity/          # Unity implementations
├── Controllers/        # MonoBehaviour controllers (thin)
└── Views/              # UI components
```

#### Architecture Principle
```csharp
// Core: Pure C# - NO Unity references
namespace DessertKingdom.Core.Domain
{
    public class GameState
    {
        public GameTurn Turn { get; }
        public CharacterStats Character { get; }
        // Pure C# logic only
    }
}

// Adapters: Bridge between Core and Unity
namespace DessertKingdom.Adapters.Unity
{
    public class UnityGamePresenter : MonoBehaviour, IGamePresenter
    {
        private GameState _gameState;  // From Core
        
        public void DisplayTurn(GameTurn turn)
        {
            // Unity-specific presentation logic
            turnText.text = $"{turn.CurrentAge}세 {turn.CurrentMonth}월";
        }
    }
}

// Controllers: Thin layer wiring everything together
namespace DessertKingdom.Controllers
{
    public class GameController : MonoBehaviour
    {
        private GameState _gameState;           // Core
        private ITurnManager _turnManager;      // Core
        private IGamePresenter _presenter;      // Adapter
        
        void Start()
        {
            _gameState = new GameState();
            _turnManager = new TurnManager(_gameState, eventManager);
            _presenter = GetComponent<UnityGamePresenter>();
        }
    }
}
```

#### Common Patterns
```csharp
// Singleton pattern (for Unity managers)
public static GameController Instance { get; private set; }
void Awake()
{
    if (Instance != null && Instance != this)
    {
        Destroy(gameObject);
        return;
    }
    Instance = this;
    DontDestroyOnLoad(gameObject);
}

// Event subscription/unsubscription
void Start() => SubscribeToEvents();
void OnDestroy() => UnsubscribeFromEvents();

// Repository usage
private IActivityRepository _activityRepo;
_activityRepo = new DatabaseActionRepository(actionDatabase);
var activities = _activityRepo.GetAll();
```

## Project Structure

### Main Projects
- **Unity Core**: Pure C# game logic in `Assets/Scripts/Core/` (netstandard2.1, Unity Assembly Definition)
  - Unity 프로젝트에서 직접 사용
  - `DessertKingdom.Core.asmdef`로 관리
  - Unity Editor 내에서 빌드/컴파일
  
- **Standalone Core**: Pure C# game logic in `Implementation/Core/` (net8.0, NO Unity dependency)
  - Unity 외부 프로젝트(Simulation, TestConsole)에서 참조
  - Unity와 직접 연결되지 않음
  
- **Unity**: Assembly-CSharp (Unity project, View layer only)
  - Unity Core를 참조

### Key Namespaces
- `DessertKingdom.Core.Domain` - Models and enums (POCO)
- `DessertKingdom.Core.Services` - Business logic
- `DessertKingdom.Core.Data` - Data access interfaces
- `DessertKingdom.Adapters.Interfaces` - Contracts between Core and Unity
- `DessertKingdom.Adapters.Unity` - Unity-specific implementations
- `DessertKingdom.Controllers` - Thin MonoBehaviour controllers
- `DessertKingdom.Views` - UI MonoBehaviours

## Architecture Rules

### 1. Core Layer (Pure C#)
- NO Unity references allowed
- All game logic, rules, calculations
- Domain models as POCO (Plain Old C# Objects)
- Services for business logic
- Interfaces for data access
- **Unity 개발 시**: `Assets/Scripts/Core/` 사용 (netstandard2.1)
- **Standalone 개발 시**: `Implementation/Core/` 사용 (net8.0)

### 2. Adapter Layer
- Interfaces in Adapters/Interfaces/ (Core-compatible)
- Unity implementations in Adapters/Unity/
- Bridges Core logic with Unity presentation

### 3. View Layer (Unity)
- MonoBehaviours for UI only
- No game logic - only presentation
- Receives updates via events/adapters
- Sends user input to Core through adapters

### 4. Data Flow
```
User Input → Controller → Adapter → Core Logic
                                      ↓
UI Update ← View ← Adapter ← Core Events
```

## Notes
- This is a Unity simulation game ("Dessert Kingdom")
- Architecture: Headless POCO + Adapter Pattern
- Unity Core (`Assets/Scripts/Core/`) is Unity-independent for testability and portability within Unity
- Standalone Core (`Implementation/Core/`) is completely separate and used by Simulation/TestConsole projects
- Unity is View-only, contains no game logic
- Target Framework: netstandard2.1 for Unity Core, .NET 8.0 for Standalone Core
- CSV/Excel data imported to ScriptableObjects via ExcelConverter
