using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class LevelTransitionTrigger : MonoBehaviour
{
    [Header("Transition Settings")]
    public string nextSceneName = "DaVinciP1";
    public float fadeDuration = 1.5f;

    [Header("Item Persistence")]
    public string requiredItem = "Flashlight";

    private static Dictionary<int, List<string>> savedItems = new Dictionary<int, List<string>>();
    private static Dictionary<int, List<string>> savedKeyCards = new Dictionary<int, List<string>>();
    private bool isTransitioning = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isTransitioning) return;

        
        PlayerIdentifier playerID = other.GetComponent<PlayerIdentifier>();
        if (playerID == null) playerID = other.GetComponentInParent<PlayerIdentifier>();

        if (playerID != null)
        {
            StartCoroutine(PerformTransition());
        }
    }

    private IEnumerator PerformTransition()
    {
        isTransitioning = true;

        
        SaveAllPlayersInventory();

        
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextSceneName);
        asyncLoad.allowSceneActivation = false;

        
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        asyncLoad.allowSceneActivation = true;
    }

    private void SaveAllPlayersInventory()
    {
        savedItems.Clear();
        savedKeyCards.Clear();

        PlayerIdentifier[] allPlayers = FindObjectsOfType<PlayerIdentifier>();
        foreach (var player in allPlayers)
        {
            PlayerInventory inventory = player.GetComponent<PlayerInventory>();
            if (inventory != null)
            {
                savedItems[player.playerID] = inventory.GetCollectedItems();
                savedKeyCards[player.playerID] = inventory.GetCollectedKeyCards();
                
                
                if (!savedItems[player.playerID].Contains(requiredItem))
                {
                    
                    
                }
            }
        }
    }

    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void OnSceneLoaded()
    {
        if (savedItems.Count == 0 && savedKeyCards.Count == 0) return;

        
        GameObject restorer = new GameObject("InventoryRestorer");
        restorer.AddComponent<InventoryRestorerHelper>();
    }

    private class InventoryRestorerHelper : MonoBehaviour
    {
        private int attempts = 0;
        private const int maxAttempts = 10;

        private void Start()
        {
            StartCoroutine(RestoreRoutine());
        }

        private IEnumerator RestoreRoutine()
        {
            while (attempts < maxAttempts)
            {
                PlayerIdentifier[] players = FindObjectsOfType<PlayerIdentifier>();
                if (players.Length > 0)
                {
                    foreach (var player in players)
                    {
                        if (savedItems.TryGetValue(player.playerID, out List<string> items))
                        {
                            PlayerInventory inventory = player.GetComponent<PlayerInventory>();
                            if (inventory != null)
                            {
                                List<string> keyCards = savedKeyCards.ContainsKey(player.playerID) ? 
                                    savedKeyCards[player.playerID] : new List<string>();
                                
                                inventory.RestoreInventory(keyCards, items);
                            }
                        }
                    }
                    
                    Destroy(gameObject);
                    yield break;
                }

                attempts++;
                yield return new WaitForSeconds(0.2f);
            }
            Destroy(gameObject);
        }
    }
}
