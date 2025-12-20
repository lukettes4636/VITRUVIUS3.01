# Quick Test Guide - Nemesis AI

## Immediate Testing Steps

### 1. Add Nemesis to Scene
1. Create empty GameObject named "Nemesis"
2. Add `NemesisAI.cs` script
3. Add `NemesisDetectionHelper.cs` script (auto-added if missing)
4. Add `NemesisTester.cs` script (auto-added if missing)

### 2. Configure Detection
1. Set Detection Layer Mask to include:
   - Default
   - Player (for Player1/Player2)
   - NPC
2. Set Sound Blocker Layer to include:
   - Default
   - Walls
   - Obstacles

### 3. Assign Test Objects
1. In NemesisTester component, assign:
   - Player1 GameObject (tag: "Player1")
   - Player2 GameObject (tag: "Player2")
   - NPC GameObject (tag: "NPC")

### 4. Test Detection
**Right-click on NemesisTester component in Inspector:**
- "Test Player1 Detection" - Tests if Nemesis can see Player1
- "Test Player2 Detection" - Tests if Nemesis can see Player2
- "Test NPC Detection" - Tests if Nemesis can see NPC
- "Reset Nemesis State" - Resets to patrol mode

### 5. Expected Behavior
- **Green Debug Line**: Target is detected and visible
- **Red Debug Line**: Target is blocked by obstacle
- **Nemesis Movement**: Will chase detected target
- **Collision**: Nemesis will collide with targets (not traverse)

### 6. Troubleshooting
- **No Detection**: Check layer masks and target tags
- **Traversal Issues**: Ensure Nemesis has Rigidbody and Capsule Collider
- **No Movement**: Check NavMesh baking and agent settings

## Key Features Working
✅ Multi-target detection (Player1, Player2, NPC)
✅ Priority targeting (NPC > Players)
✅ Collision prevention (no traversal)
✅ Sound detection system
✅ Horror animator integration
✅ GameOverManager integration