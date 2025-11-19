# GitHub Copilot Instructions for UWAFT CAV Game Project

## Project Context
This is a Unity-based Connected and Autonomous Vehicle (CAV) game project. The project uses Unity 2D features, addressables, input system, and multiplayer capabilities.

## MCP Server Usage Guidelines

### When to Use Exa MCP
**Use Exa MCP for web search and real-time information retrieval:**
- When you need to search for current documentation, tutorials, or best practices
- When researching solutions to errors or bugs that might have recent fixes
- When looking for up-to-date Unity API changes or package updates
- When you need code examples from recent projects or repositories
- When researching new Unity features, packages, or third-party libraries
- When investigating compatibility issues between Unity versions or packages

**Example use cases:**
- "Search for recent Unity addressables performance optimization techniques"
- "Find solutions for Unity input system mobile touch handling"
- "Research best practices for Unity WebGL builds"
- "Look up recent Unity multiplayer networking patterns"

### When to Use Context7 MCP (Upstash)
**Use Context7 MCP for library-specific, version-aware documentation:**
- When you need accurate, version-specific API documentation for a library
- When working with specific versions of packages (Unity, C# frameworks, etc.)
- To avoid hallucinated or deprecated APIs
- When you need code examples that match exact package versions
- To get up-to-date documentation that matches the project's dependencies

**Example use cases:**
- "Get Unity Addressables 2.7.3 documentation"
- "Fetch React Unity WebGL integration documentation"
- "Retrieve Unity Input System 1.14.0 API reference"
- "Get Cinemachine 3.1.3 documentation for camera control"

**How to use Context7:**
1. First resolve the library ID: specify the library name
2. Then fetch documentation: provide topic or specific feature you need
3. Always specify version when available for most accurate results

### When to Use Unity MCP
**Use Unity MCP for direct Unity Editor interactions and Unity-specific operations:**
- When you need to create, read, or modify Unity scripts (C#)
- When managing Unity GameObjects, prefabs, or scenes
- When you need to inspect or modify Unity assets
- When executing Unity Editor menu commands
- When running Unity tests (EditMode or PlayMode)
- When checking Unity console for compilation errors or warnings
- When managing Unity project structure and resources
- When validating Unity scripts for syntax or compilation issues

**Example use cases:**
- "Create a new MonoBehaviour script for vehicle controller"
- "Add a Camera and Directional Light to the current scene"
- "List all scripts in the Assets/Scripts folder"
- "Check Unity console for compilation errors"
- "Run Unity EditMode tests"
- "Create a prefab from a GameObject"
- "Validate the PlayerController.cs script"
- "Execute Unity menu item Build Settings"

**Important Unity MCP workflows:**
- After creating or modifying scripts, always check the Unity console for errors
- Wait for compilation to complete before using new components
- Always include Camera and Directional Light in new scenes
- Use forward slashes (/) in all Unity paths for cross-platform compatibility
- All paths are relative to the Assets/ folder unless specified otherwise

## Coding Standards and Best Practices

### Unity-Specific Guidelines
- Follow Unity naming conventions: PascalCase for public members, camelCase for private
- Use `[SerializeField]` for private fields that need Inspector visibility
- Prefer composition over inheritance when possible
- Keep MonoBehaviour classes focused on Unity-specific logic
- Cache component references in Awake() or Start() to avoid repeated GetComponent() calls
- Use object pooling for frequently instantiated/destroyed objects
- Implement proper cleanup in OnDestroy() for event subscriptions and resources

### C# Best Practices
- Use C# 9.0+ features where appropriate (pattern matching, record types, etc.)
- Prefer readonly fields and properties when data shouldn't change
- Use async/await for asynchronous operations
- Implement proper error handling with try-catch blocks
- Use nullable reference types to prevent null reference exceptions
- Follow SOLID principles for maintainable, testable code

### Performance Considerations
- Minimize allocations in Update(), FixedUpdate(), and LateUpdate()
- Use object pooling for particle effects and projectiles
- Optimize physics with proper layer collision matrix settings
- Use addressables for efficient asset loading and memory management
- Profile regularly using Unity Profiler to identify bottlenecks
- Consider using Jobs System and Burst compiler for performance-critical code

### Version Control and Workflow
- Write clear, descriptive commit messages
- Keep commits focused and atomic
- Test changes before committing
- Use meaningful branch names that describe the feature or fix

## Project-Specific Preferences
- Target platform: WebGL with potential for other platforms
- Input handling: Use Unity Input System (not legacy Input Manager)
- Asset management: Use Addressables for resource loading
- Scene management: Implement proper scene loading/unloading strategies
- UI: Use Unity UI (uGUI) with proper anchoring and scaling

## Testing Approach
- Write unit tests for game logic that doesn't depend on Unity
- Use Unity Test Framework for Unity-specific functionality
- Test in WebGL build regularly to catch platform-specific issues
- Validate input handling on both desktop and touch devices

## Documentation Standards
- Add XML documentation comments (///) for public APIs
- Include inline comments for complex or non-obvious logic
- Update README.md when adding new features or changing setup procedures
- Document known issues and workarounds

## MCP Integration Workflow
1. **For general research**: Start with Exa MCP to find current best practices and solutions
2. **For specific library documentation**: Use Context7 MCP to get version-accurate API references
3. **For Unity Editor operations**: Use Unity MCP for creating, modifying, and managing Unity assets
4. **Combine MCPs**: Use multiple MCPs in sequence for comprehensive solutions
   - Example: Use Exa to research approach → Context7 to get exact API → Unity MCP to implement in editor

## Error Handling and Debugging
- When encountering Unity package resolution errors, check manifest.json and packages-lock.json
- For compilation errors, use Unity MCP to check the console and validate scripts
- When packages conflict, prefer removing explicit version constraints to allow Unity's dependency resolver to work
- Use Exa MCP to search for solutions to Unity-specific errors
- Always check Unity version compatibility when adding new packages

## Response Style
- Be concise but complete in explanations
- Provide working code examples when relevant
- Explain Unity-specific concepts when they might not be obvious
- Suggest optimizations when you notice performance concerns
- Point out potential issues before they become problems
