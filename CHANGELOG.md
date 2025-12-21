# Changelog

## [Unreleased] - 2025-12-21

### Removed
- `Assets/scripts/Testing/CrouchMovementTester.cs`: Unused test script.
- `Assets/scripts/Utils/GameStartFixer.cs`: Unused utility script.
- `Assets/scripts/Enemy/CompilationChecker.cs`: Unused test script.
- `Assets/scripts/Enemy/NemesisSceneSetup.cs`: Unused scene setup script.
- `Assets/scripts/Enemy/NemesisTester.cs`: Unused test script.
- `Assets/scripts/Enemy/NemesisValidator.cs`: Debug validation script.
- `Assets/scripts/Enemy/NemesisQuickValidator.cs`: Debug validation script.
- `Assets/scripts/Enemy/QUICK_TEST_GUIDE.md`: Documentation for test tools.
- `Assets/scripts/Menu/PauseManager.cs`: Unused pause management script.
- `Assets/Editor/AdjustSpotlightPosition.cs`: Obsolete one-time scene fix.
- `Assets/Editor/FixPauseMenuScaling.cs`: Obsolete one-time UI fix.
- `Assets/Editor/FixPisoLightmapUVs.cs`: Obsolete one-time asset fix.
- `Assets/Editor/ScaleMainMenuButtons.cs`: Obsolete one-time UI fix.
- `Assets/Editor/DisableStaticBatching.cs`: Obsolete one-time optimization tool.

### Added
- Essential Project Tools (Centralized under `Tools/` menu):
    - `ScriptUsageAudit.cs`: Identifies unused scripts based on GUID references.
    - `StripDebugLogs.cs`: Automated removal of debug logs and flags for production.
    - `StripComments.cs`: Utility for cleaning AI-generated comments and indices.
    - `MainMenuSetupTool.cs`: UI/Scene setup helper for the Main Menu.
    - `SceneOrganizer.cs`: Automation for scene object hierarchy organization.

### Optimized
- Finalized systemic cleanup of AI-generated content and debug visualizations:
    - Wrapped all `OnDrawGizmos`, `Debug.DrawRay`, and `Debug.DrawLine` calls in `#if UNITY_EDITOR` for production performance.
    - Scripts affected: `EnemyMonsterAI.cs`, `NemesisAI_Enhanced.cs`, `NemesisAI.cs`, `WeaponWallAvoidance.cs`, `FlashlightController_Enhanced.cs`, `NemesisSceneReset.cs`.
- Secured debug and editor-only utilities:
    - Wrapped `[ExecuteInEditMode]`, `[ContextMenu]`, and validation logic in production-safe preprocessor directives.
    - Scripts affected: `VLB_ImproveIntersection.cs`, `NemesisAnimatorSetup.cs`, `MainMenuThunderEffect.cs`.
- Completed comprehensive AI signature removal across the entire `Assets/scripts` directory.
- Refactored `ControlsButton.cs`:
    - Implemented `PauseController` caching to eliminate `FindObjectOfType` calls.
    - Cleaned up debug modes and redundant transition logic.
- Cleaned and normalized inventory scripts:
    - `HotbarController.cs`: Removed procedural Spanish comments, normalized formatting.
    - `InventoryHotbarUI.cs`: Removed procedural comments and updated to English headers.
- Removed AI-generated markers and numbered indices from project documentation:
    - `README_NemesisAI.md`
    - `README_NemesisAI_Enhanced.md`
- Removed AI indices and procedural comments from:
    - `MainMenuSetupTool.cs`
- Removed `Debug.Log` and debug flags from multiple production scripts:
    - `PuertaDobleAccion.cs`
    - `ButtonIndicatorController.cs`
    - `EnemyBrain.cs`
    - `EnemySenses.cs`
    - `NemesisDetectionHelper.cs`
    - `NPCDialogueDataManager.cs`
    - `DialogueAutoReset.cs`
    - `NemesisSoundDetector.cs`
- Refactored `EnemySenses.cs` for improved performance:
    - Implemented component caching for players and NPCs to avoid per-frame `GetComponent` calls.
- Centralized project cleanup tools in the Unity Editor menu (`Tools/Cleanup`).
- Organized `MainMenuSetupTool`, `SceneOrganizer`, and other utilities under consistent menu paths.
