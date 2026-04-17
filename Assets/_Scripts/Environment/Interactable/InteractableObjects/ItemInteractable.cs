using UnityEngine;

public class ItemInteractable : Interactable
{
    [SerializeField] private ItemData rewardItem;
    [SerializeField] private int count = 1;
    [SerializeField] private float respawnTime = 0f; // 0 = не возрождается

    public override void OnInteract(Interactor interactor)
    {
        if (!(interactor is PlayerInteractor player)) return;

        var inventory = player.Context?.inventory;
        var hotbar = player.Context?.hotbar;

        if (inventory == null || rewardItem == null) return;

        // For usable items check hotbar capacity before adding
        if (rewardItem.CanUse && hotbar != null && !hotbar.CanFitItem(rewardItem, count))
            return;

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

    private void Respawn()
    {
        gameObject.SetActive(true);
    }
}
