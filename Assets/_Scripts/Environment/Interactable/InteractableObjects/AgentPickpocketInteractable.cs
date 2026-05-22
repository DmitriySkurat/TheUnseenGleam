using UnityEngine;

/// <summary>
/// Place as a child of an Agent GameObject.
/// When the player interacts, they receive the configured item (e.g. a Key).
/// Deactivates itself after a successful pickpocket so it can only be done once.
/// </summary>
public class AgentPickpocketInteractable : Interactable
{
    [Header("Pickpocket Settings")]
    [SerializeField] private ItemData rewardItem;
    [SerializeField] private int count = 1;

    public override bool CanBeInteractedBy(Interactor interactor) =>
        interactor is PlayerInteractor;

    public override void OnInteract(Interactor interactor)
    {
        if (interactor is not PlayerInteractor player) return;

        var inventory = player.Context?.inventory;
        if (inventory == null || rewardItem == null) return;

        inventory.Add(rewardItem, count);
        gameObject.SetActive(false);
    }
}
