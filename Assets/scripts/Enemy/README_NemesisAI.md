# Nemesis AI System for Unity URP Top-Down Game

## Overview
This nemesis AI system creates an implacable, Resident Evil 3-style enemy that relentlessly pursues and attacks players and NPCs in your Unity URP top-down game.

## Features
- **Implacable Pursuit**: The nemesis never gives up and will continue hunting targets until they are eliminated
- **Multi-Target Detection**: Detects and targets Player1, Player2, and NPCs
- **Sound Detection**: Responds to character movement sounds and noise emissions
- **Visual Detection**: Uses line-of-sight to detect targets within range
- **Priority Targeting**: NPCs have highest priority, followed by players
- **Dynamic Speed**: Walks during patrol, runs when chasing, sprints when very close to target
- **Attack System**: Melee attacks with cooldown and damage dealing
- **Memory System**: Remembers last known positions and searches when targets are lost
- **State Machine**: Patrol → Alert → Chase → Attack → Search states
- **Animation Integration**: Works with Unity Animator system (Horror controller)
- **NavMesh Navigation**: Uses Unity's NavMesh system for pathfinding

## Files Created

### Core AI Scripts
1. **NemesisAI.cs** - Basic nemesis AI implementation
2. **NemesisAI_Enhanced.cs** - Enhanced version with advanced features
3. **NemesisSoundDetector.cs** - Sound detection system for characters

### Setup and Configuration
4. **NemesisSceneSetup.cs** - Scene setup helper for DaVinciP1 scene

## Installation Instructions

### 1. Add Scripts to Your Project
Copy all the script files to your `Assets/scripts/Enemy/` folder.

### 2. Scene Setup
Add the `NemesisSceneSetup.cs` script to an empty GameObject in your DaVinciP1 scene.

### 3. Configure the Setup Script
In the Inspector:
- Assign your nemesis prefab (or leave empty to create from scratch)
- Set the spawn position for the nemesis
- Configure audio clips for attack, detection, and roar sounds
- Adjust detection ranges and speeds as needed

### 4. Tag Your GameObjects
Make sure your scene has properly tagged objects:
- Player1: Tag as "Player1"
- Player2: Tag as "Player2" 
- NPC: Tag as "NPC"

### 5. Setup Navigation
Ensure your scene has:
- NavMeshSurface component
- Properly baked NavMesh
- Walls and obstacles on appropriate layers

### 6. Setup Audio
Configure the nemesis audio system:
- Assign attack sound effects
- Assign detection sound effects
- Assign roar sound effects (optional)
- Configure 3D audio settings

## Configuration Options

### Detection Settings
```csharp
public float visualDetectionRadius = 25f;    // How far nemesis can see
public float soundDetectionRadius = 35f;     // How far nemesis can hear
public float attackRange = 2.5f;             // Melee attack range
```

### Movement Settings
```csharp
public float walkSpeed = 3.5f;               // Patrol speed
public float chaseSpeed = 5.5f;              // Chase speed
public float sprintSpeed = 7f;               // Sprint speed (when very close)
```

### AI Behavior
```csharp
public float searchDuration = 8f;              // How long to search for lost target
public float memoryDuration = 15f;           // How long to remember target position
public float targetSwitchDelay = 2f;         // Delay before switching targets
```

### Targeting Priority
```csharp
public float npcPriority = 3f;               // NPC priority (highest)
public float playerPriority = 2f;            // Player priority (medium)
public float soundPriority = 1f;             // Sound priority (lowest)
```

## Usage Instructions

### Basic Usage
1. Add `NemesisSceneSetup` to your scene
2. Click "Setup Scene" in the Inspector
3. The nemesis will automatically spawn and begin patrolling
4. Players and NPCs will be detected and pursued

### Advanced Usage
Use the enhanced AI script (`NemesisAI_Enhanced.cs`) for:
- More sophisticated state management
- Better sound detection
- Improved target switching
- Enhanced animation support

### Testing
Use the "Test Nemesis Detection" button to force the nemesis to become alert and test the detection system.

### Debugging
Enable debug gizmos to visualize:
- Detection ranges (red = visual, yellow = sound, magenta = attack)
- Current target (red line)
- Last known position (orange line)
- Current AI state (white text)

## Integration with Existing Systems

### GameOver Manager
The nemesis AI integrates with your existing GameOverManager:
- NPC death triggers GameOver
- Both players dead triggers GameOver
- Nemesis stops targeting dead characters

### Health Systems
Works with existing health systems:
- PlayerHealth component for players
- NPCHealth component for NPCs
- Damage dealing through TakeDamage() methods

### Animation System
Integrates with Unity's Animator system:
- Requires "Horror" animator controller
- Uses parameters: Walk, Run, Attack, Detected, Search
- Supports trigger animations for detection and attacks

## Performance Considerations

### Optimization Tips
- Adjust `aiUpdateInterval` to balance responsiveness vs performance
- Use appropriate detection ranges (not too large)
- Limit the number of simultaneous nemesis enemies
- Use object pooling for audio sources

### Memory Management
- Sounds are cleaned up automatically after 3 seconds
- Target references are cleared when targets die
- State machines prevent unnecessary calculations

## Troubleshooting

### Nemesis Not Moving
- Check NavMesh baking
- Ensure NavMeshAgent component is present
- Verify destination positions are valid

### Not Detecting Targets
- Check object tags (Player1, Player2, NPC)
- Verify detection layer masks
- Check line of sight (walls blocking view)

### Animation Issues
- Ensure Horror animator controller is assigned
- Check animation parameter names
- Verify animator component is present

### Audio Issues
- Check audio source settings
- Verify audio clips are assigned
- Check 3D audio settings and falloff

## Future Enhancements
- Multiple nemesis instances
- Advanced pathfinding (jumping, climbing)
- Weapon pickup and usage
- Environmental interaction
- Advanced sound propagation
- Machine learning integration

## Support
For issues or questions, check the Unity console for error messages and ensure all components are properly configured according to this documentation.