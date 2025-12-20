#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;




public class HorrorDiagnosticTool : EditorWindow
{
    private GameObject selectedEnemy;
    private string diagnosticResults = "";
    private Vector2 scrollPosition;
    private bool showVisibilitySection = true;
    private bool showDetectionSection = true;
    private bool showAttackSection = true;
    
    [MenuItem("Tools/Diagnose Horror Enemy Issues")]
    public static void ShowWindow()
    {
        var window = GetWindow<HorrorDiagnosticTool>("Horror Enemy Diagnostic");
        window.minSize = new Vector2(500, 600);
    }
    
    void OnGUI()
    {
        GUILayout.Label("Horror Enemy Diagnostic Tool", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        
        GUILayout.Label("Step 1: Select Enemy", EditorStyles.boldLabel);
        selectedEnemy = EditorGUILayout.ObjectField("Enemy GameObject:", selectedEnemy, typeof(GameObject), true) as GameObject;
        
        if (selectedEnemy == null)
        {
            EditorGUILayout.HelpBox("Please select an enemy GameObject in the scene.", MessageType.Info);
            return;
        }
        
        GUILayout.Space(10);
        
        
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        
        
        showVisibilitySection = EditorGUILayout.Foldout(showVisibilitySection, "🔍 Visibility Issues (Horror1_LP)", true);
        if (showVisibilitySection)
        {
            DrawVisibilitySection();
        }
        
        GUILayout.Space(10);
        
        
        showDetectionSection = EditorGUILayout.Foldout(showDetectionSection, "👁 Detection Issues", true);
        if (showDetectionSection)
        {
            DrawDetectionSection();
        }
        
        GUILayout.Space(10);
        
        
        showAttackSection = EditorGUILayout.Foldout(showAttackSection, "⚔ Attack Issues", true);
        if (showAttackSection)
        {
            DrawAttackSection();
        }
        
        GUILayout.EndScrollView();
        
        GUILayout.Space(20);
        
        
        GUILayout.Label("Quick Fix Actions", EditorStyles.boldLabel);
        
        if (GUILayout.Button("🚀 Run Complete Diagnostic", GUILayout.Height(30)))
        {
            RunCompleteDiagnostic();
        }
        
        if (GUILayout.Button("🔧 Fix All Issues", GUILayout.Height(30)))
        {
            FixAllIssues();
        }
        
        
        if (!string.IsNullOrEmpty(diagnosticResults))
        {
            GUILayout.Space(10);
            GUILayout.Label("Diagnostic Results:", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(diagnosticResults, MessageType.Info);
        }
    }
    
    void DrawVisibilitySection()
    {
        EditorGUILayout.HelpBox("Checks for Horror1_LP model visibility issues during roar and animations.", MessageType.Info);
        
        if (GUILayout.Button("🔍 Find Horror1_LP Model"))
        {
            FindHorrorModel();
        }
        
        if (GUILayout.Button("📊 Check Visibility Status"))
        {
            CheckVisibilityStatus();
        }
        
        if (GUILayout.Button("🛠 Fix Visibility Issues"))
        {
            FixVisibilityIssues();
        }
        
        if (GUILayout.Button("➕ Add Visibility Fix Script"))
        {
            AddVisibilityFixScript();
        }
    }
    
    void DrawDetectionSection()
    {
        EditorGUILayout.HelpBox("Checks for player and NPC detection issues.", MessageType.Info);
        
        if (GUILayout.Button("🔍 Check Detection Components"))
        {
            CheckDetectionComponents();
        }
        
        if (GUILayout.Button("📊 Test Player Detection"))
        {
            TestPlayerDetection();
        }
        
        if (GUILayout.Button("🎯 Test NPC Detection"))
        {
            TestNPCDetection();
        }
        
        if (GUILayout.Button("🔊 Test Sound Detection"))
        {
            TestSoundDetection();
        }
    }
    
    void DrawAttackSection()
    {
        EditorGUILayout.HelpBox("Checks for attack functionality issues.", MessageType.Info);
        
        if (GUILayout.Button("🔍 Check Attack Components"))
        {
            CheckAttackComponents();
        }
        
        if (GUILayout.Button("⚔ Test Attack Animation"))
        {
            TestAttackAnimation();
        }
        
        if (GUILayout.Button("🎯 Test Attack Damage"))
        {
            TestAttackDamage();
        }
    }
    
    void RunCompleteDiagnostic()
    {
        diagnosticResults = "=== COMPLETE DIAGNOSTIC RESULTS ===\n\n";
        
        
        FindHorrorModel();
        CheckVisibilityStatus();
        CheckDetectionComponents();
        CheckAttackComponents();
        
        diagnosticResults += "\n=== DIAGNOSTIC COMPLETE ===\n";
        diagnosticResults += "Check the individual sections above for detailed information and fixes.";
    }
    
    void FixAllIssues()
    {
        diagnosticResults = "=== APPLYING ALL FIXES ===\n\n";
        
        
        FixVisibilityIssues();
        AddVisibilityFixScript();
        
        diagnosticResults += "\n=== ALL FIXES APPLIED ===\n";
        diagnosticResults += "Please test the enemy in Play Mode to verify the fixes.";
    }
    
    void FindHorrorModel()
    {
        diagnosticResults += "=== FINDING HORROR MODEL ===\n";
        
        
        GameObject horrorModel = null;
        
        
        horrorModel = GameObject.Find("Horror1_LP");
        if (horrorModel != null)
        {
            diagnosticResults += "✓ Found Horror1_LP by exact name\n";
        }
        else
        {
            
            Transform foundTransform = selectedEnemy.transform.Find("Horror1_LP");
            if (foundTransform != null)
            {
                horrorModel = foundTransform.gameObject;
                diagnosticResults += "✓ Found Horror1_LP in enemy children\n";
            }
            else
            {
                
                Transform[] allTransforms = selectedEnemy.GetComponentsInChildren<Transform>(true);
                foreach (Transform t in allTransforms)
                {
                    if (t.name.Contains("Horror") && t.name.Contains("LP"))
                    {
                        horrorModel = t.gameObject;
                        diagnosticResults += $"✓ Found Horror model: {t.name}\n";
                        break;
                    }
                }
            }
        }
        
        if (horrorModel == null)
        {
            diagnosticResults += "✗ Could not find Horror1_LP model!\n";
            diagnosticResults += "  Suggestions: Check the model name, ensure it's active, or manually assign it.\n";
        }
        else
        {
            diagnosticResults += $"✓ Horror model found: {horrorModel.name}\n";
            diagnosticResults += $"  Active: {horrorModel.activeSelf}\n";
            
            
            Renderer[] renderers = horrorModel.GetComponentsInChildren<Renderer>(true);
            diagnosticResults += $"  Renderers found: {renderers.Length}\n";
            
            int enabledRenderers = 0;
            foreach (Renderer renderer in renderers)
            {
                if (renderer.enabled) enabledRenderers++;
            }
            diagnosticResults += $"  Enabled renderers: {enabledRenderers}/{renderers.Length}\n";
        }
    }
    
    void CheckVisibilityStatus()
    {
        diagnosticResults += "\n=== VISIBILITY STATUS ===\n";
        
        Transform[] allTransforms = selectedEnemy.GetComponentsInChildren<Transform>(true);
        List<GameObject> horrorObjects = new List<GameObject>();
        
        
        foreach (Transform t in allTransforms)
        {
            if (t.name.Contains("Horror") || t.name.Contains("LP"))
            {
                horrorObjects.Add(t.gameObject);
            }
        }
        
        if (horrorObjects.Count == 0)
        {
            diagnosticResults += "✗ No Horror-related objects found\n";
            return;
        }
        
        foreach (GameObject obj in horrorObjects)
        {
            diagnosticResults += $"\nObject: {obj.name}\n";
            diagnosticResults += $"  Active: {obj.activeSelf}\n";
            diagnosticResults += $"  Layer: {obj.layer}\n";
            
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
            diagnosticResults += $"  Renderers: {renderers.Length}\n";
            
            foreach (Renderer renderer in renderers)
            {
                diagnosticResults += $"    - {renderer.name}: {(renderer.enabled ? "✓ Enabled" : "✗ Disabled")}\n";
            }
            
            SkinnedMeshRenderer[] skinnedRenderers = obj.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            if (skinnedRenderers.Length > 0)
            {
                diagnosticResults += $"  Skinned Mesh Renderers: {skinnedRenderers.Length}\n";
                foreach (SkinnedMeshRenderer smr in skinnedRenderers)
                {
                    diagnosticResults += $"    - {smr.name}: {(smr.enabled ? "✓ Enabled" : "✗ Disabled")}\n";
                    if (smr.bones == null || smr.bones.Length == 0)
                    {
                        diagnosticResults += $"      ⚠ Warning: No bones assigned!\n";
                    }
                }
            }
        }
    }
    
    void FixVisibilityIssues()
    {
        diagnosticResults += "\n=== FIXING VISIBILITY ISSUES ===\n";
        
        Transform[] allTransforms = selectedEnemy.GetComponentsInChildren<Transform>(true);
        int fixedCount = 0;
        
        foreach (Transform t in allTransforms)
        {
            if (t.name.Contains("Horror") || t.name.Contains("LP"))
            {
                GameObject obj = t.gameObject;
                
                
                if (!obj.activeSelf)
                {
                    obj.SetActive(true);
                    diagnosticResults += $"✓ Activated GameObject: {obj.name}\n";
                    fixedCount++;
                }
                
                
                Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
                foreach (Renderer renderer in renderers)
                {
                    if (!renderer.enabled)
                    {
                        renderer.enabled = true;
                        diagnosticResults += $"✓ Enabled renderer: {renderer.name}\n";
                        fixedCount++;
                    }
                }
            }
        }
        
        if (fixedCount == 0)
        {
            diagnosticResults += "✓ No visibility issues found\n";
        }
        else
        {
            diagnosticResults += $"✓ Fixed {fixedCount} visibility issues\n";
        }
    }
    
    void AddVisibilityFixScript()
    {
        diagnosticResults += "\n=== ADDING VISIBILITY FIX SCRIPT ===\n";
        
        
        HorrorCompleteSolution existingSolution = selectedEnemy.GetComponent<HorrorCompleteSolution>();
        HorrorModelVisibilityFix existingFix = selectedEnemy.GetComponent<HorrorModelVisibilityFix>();
        HorrorRoarDebugger existingDebugger = selectedEnemy.GetComponent<HorrorRoarDebugger>();
        
        if (existingSolution != null || existingFix != null || existingDebugger != null)
        {
            diagnosticResults += "✓ Visibility fix scripts already exist\n";
            return;
        }
        
        
        HorrorCompleteSolution solution = selectedEnemy.AddComponent<HorrorCompleteSolution>();
        diagnosticResults += "✓ Added HorrorCompleteSolution script\n";
        
        
        solution.SetAutoFixOnStart(true);
        solution.SetMonitorVisibility(true);
        solution.SetFixDuringRoar(true);
        solution.SetFixDuringAttack(true);
        solution.SetDebugLogs(true);
        solution.SetShowGizmos(true);
        
        diagnosticResults += "✓ Configured visibility fix script\n";
        diagnosticResults += "  - Auto-fix on start: Enabled\n";
        diagnosticResults += "  - Monitor visibility: Enabled\n";
        diagnosticResults += "  - Fix during roar: Enabled\n";
        diagnosticResults += "  - Fix during attack: Enabled\n";
    }
    
    void CheckDetectionComponents()
    {
        diagnosticResults += "\n=== CHECKING DETECTION COMPONENTS ===\n";
        
        
        NemesisAI nemesisAI = selectedEnemy.GetComponent<NemesisAI>();
        if (nemesisAI != null)
        {
            diagnosticResults += "✓ NemesisAI script found\n";
            diagnosticResults += $"  Detection Radius: {nemesisAI.detectionRadius}\n";
            diagnosticResults += $"  Attack Range: {nemesisAI.attackRange}\n";
            diagnosticResults += $"  Sound Detection Radius: {nemesisAI.soundDetectionRadius}\n";
        }
        else
        {
            diagnosticResults += "✗ NemesisAI script not found!\n";
        }
        
        
        NemesisDetectionHelper detectionHelper = selectedEnemy.GetComponent<NemesisDetectionHelper>();
        if (detectionHelper != null)
        {
            diagnosticResults += "✓ NemesisDetectionHelper found\n";
        }
        else
        {
            diagnosticResults += "! NemesisDetectionHelper not found (optional)\n";
        }
        
        
        if (nemesisAI != null)
        {
            diagnosticResults += $"  Detection Layer Mask: {nemesisAI.detectionLayerMask.value}\n";
            diagnosticResults += $"  Sound Blocker Layer: {nemesisAI.soundBlockerLayer.value}\n";
        }
    }
    
    void TestPlayerDetection()
    {
        diagnosticResults += "\n=== TESTING PLAYER DETECTION ===\n";
        
        
        GameObject player1 = GameObject.FindGameObjectWithTag("Player1");
        GameObject player2 = GameObject.FindGameObjectWithTag("Player2");
        
        if (player1 == null && player2 == null)
        {
            diagnosticResults += "✗ No players found in scene!\n";
            diagnosticResults += "  Make sure players have Player1 or Player2 tags.\n";
            return;
        }
        
        if (player1 != null)
        {
            diagnosticResults += $"✓ Found Player1: {player1.name}\n";
            diagnosticResults += $"  Distance from enemy: {Vector3.Distance(selectedEnemy.transform.position, player1.transform.position)}\n";
            
            var playerHealth = player1.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                diagnosticResults += $"  Health: {(playerHealth.IsDead ? "Dead" : "Alive")}\n";
            }
        }
        
        if (player2 != null)
        {
            diagnosticResults += $"✓ Found Player2: {player2.name}\n";
            diagnosticResults += $"  Distance from enemy: {Vector3.Distance(selectedEnemy.transform.position, player2.transform.position)}\n";
            
            var playerHealth = player2.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                diagnosticResults += $"  Health: {(playerHealth.IsDead ? "Dead" : "Alive")}\n";
            }
        }
    }
    
    void TestNPCDetection()
    {
        diagnosticResults += "\n=== TESTING NPC DETECTION ===\n";
        
        
        GameObject npc = GameObject.FindGameObjectWithTag("NPC");
        
        if (npc == null)
        {
            diagnosticResults += "✗ No NPC found in scene!\n";
            diagnosticResults += "  Make sure NPCs have NPC tag.\n";
            return;
        }
        
        diagnosticResults += $"✓ Found NPC: {npc.name}\n";
        diagnosticResults += $"  Distance from enemy: {Vector3.Distance(selectedEnemy.transform.position, npc.transform.position)}\n";
        
        var npcHealth = npc.GetComponent<NPCHealth>();
        if (npcHealth != null)
        {
            diagnosticResults += $"  Health: {(npcHealth.IsDead ? "Dead" : "Alive")}\n";
        }
    }
    
    void TestSoundDetection()
    {
        diagnosticResults += "\n=== TESTING SOUND DETECTION ===\n";
        
        
        PlayerNoiseEmitter[] playerNoiseEmitters = FindObjectsOfType<PlayerNoiseEmitter>();
        NPCNoiseEmitter[] npcNoiseEmitters = FindObjectsOfType<NPCNoiseEmitter>();
        
        diagnosticResults += $"Player Noise Emitters: {playerNoiseEmitters.Length}\n";
        diagnosticResults += $"NPC Noise Emitters: {npcNoiseEmitters.Length}\n";
        
        foreach (PlayerNoiseEmitter emitter in playerNoiseEmitters)
        {
            diagnosticResults += $"  Player Noise: {emitter.name} - Radius: {emitter.currentNoiseRadius}\n";
        }
        
        foreach (NPCNoiseEmitter emitter in npcNoiseEmitters)
        {
            diagnosticResults += $"  NPC Noise: {emitter.name} - Radius: {emitter.currentNoiseRadius}\n";
        }
    }
    
    void CheckAttackComponents()
    {
        diagnosticResults += "\n=== CHECKING ATTACK COMPONENTS ===\n";
        
        
        Animator animator = selectedEnemy.GetComponent<Animator>();
        if (animator != null)
        {
            diagnosticResults += "✓ Animator found\n";
            
            
            bool hasAttackParam = false;
            for (int i = 0; i < animator.parameterCount; i++)
            {
                if (animator.GetParameter(i).name == "Attack")
                {
                    hasAttackParam = true;
                    break;
                }
            }
            
            if (hasAttackParam)
            {
                diagnosticResults += "✓ Attack parameter found in animator\n";
            }
            else
            {
                diagnosticResults += "✗ Attack parameter not found in animator!\n";
            }
        }
        else
        {
            diagnosticResults += "✗ Animator not found!\n";
        }
        
        
        NemesisAI nemesisAI = selectedEnemy.GetComponent<NemesisAI>();
        if (nemesisAI != null)
        {
            diagnosticResults += $"  Attack Sounds: {(nemesisAI.attackSounds != null ? nemesisAI.attackSounds.Length : 0)}\n";
            diagnosticResults += $"  Detection Sounds: {(nemesisAI.detectionSounds != null ? nemesisAI.detectionSounds.Length : 0)}\n";
        }
    }
    
    void TestAttackAnimation()
    {
        diagnosticResults += "\n=== TESTING ATTACK ANIMATION ===\n";
        
        Animator animator = selectedEnemy.GetComponent<Animator>();
        if (animator == null)
        {
            diagnosticResults += "✗ No animator found!\n";
            return;
        }
        
        
        if (!Application.isPlaying)
        {
            diagnosticResults += "⚠ Must be in Play Mode to test animations\n";
            return;
        }
        
        
        animator.SetTrigger("Attack");
        diagnosticResults += "✓ Attack animation triggered\n";
        diagnosticResults += "  Check the Game view to see if the attack animation plays correctly.\n";
    }
    
    void TestAttackDamage()
    {
        diagnosticResults += "\n=== TESTING ATTACK DAMAGE ===\n";
        
        NemesisAI nemesisAI = selectedEnemy.GetComponent<NemesisAI>();
        if (nemesisAI == null)
        {
            diagnosticResults += "✗ NemesisAI not found!\n";
            return;
        }
        
        diagnosticResults += $"  Attack Damage: {nemesisAI.attackDamage}\n";
        diagnosticResults += $"  Attack Range: {nemesisAI.attackRange}\n";
        diagnosticResults += $"  Attack Cooldown: {nemesisAI.attackCooldown}\n";
        
        if (Application.isPlaying)
        {
            diagnosticResults += "✓ In Play Mode - test by approaching the enemy\n";
        }
        else
        {
            diagnosticResults += "⚠ Enter Play Mode to test attack functionality\n";
        }
    }
}
#endif