using UnityEngine;

public class ItemInteractable : Interactable
{
    [SerializeField] private ItemData rewardItem;
    [SerializeField] private int count = 1;
    [SerializeField] private float respawnTime = 0f; // 0 = не возрождается
    
    public override void OnInteract(Interactor interactor)
    {
        if (interactor is PlayerInteractor player)
        {
            var inventory = player.Context?.inventory;
            if (inventory != null && rewardItem != null)
            {
                inventory.Add(rewardItem, count);
                
                if (respawnTime > 0)
                {
                    gameObject.SetActive(false);
                    Invoke(nameof(Respawn), respawnTime);
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }
    }
    
    private void Respawn()
    {
        gameObject.SetActive(true);
    }
}