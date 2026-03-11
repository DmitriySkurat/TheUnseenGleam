using UnityEngine;

public class HidingSpotInteractable : Interactable
{
    //public override bool IsComplex => true; 
     
    [Header("Hide Settings")]
    [SerializeField] private Transform hidePoint;

    public override void OnInteract(Interactor interactor)
    {
        if (interactor is not PlayerInteractor player) return;
        
        Debug.Log("Interacted with hiding spot!");

        var ctx = player.Context;
        if (ctx == null || !ctx.isCoverHideout) 
        {
            Debug.Log("Player is not in a cover hideout.");
            return;
        }

        if (!ctx.isHiding)
        {
            ctx.isHiding = true;
            ctx.activeHideSpot = this;
            
            Debug.Log("Player is now hiding in the spot.");

            SnapPlayerToHidePoint(player);

            return;
        }

        if (ctx.activeHideSpot == this)
        {
            ctx.isHiding = false;
            ctx.activeHideSpot = null;
            
            Debug.Log("Player has stopped hiding in the spot.");
            
            return;
        }
        
        Debug.Log("End of the function");
    }

    private void SnapPlayerToHidePoint(PlayerInteractor player)
    {
        var ctx = player.Context;
        if (ctx == null || ctx.transform == null) return;

        var target = hidePoint != null ? hidePoint : transform;
        var rb = player.GetComponent<Rigidbody2D>();

        var currentPos = rb != null ? rb.position : (Vector2)ctx.transform.position;
        var targetPos = new Vector2(target.position.x, currentPos.y);

        if (rb != null)
            rb.position = targetPos;
        else
            ctx.transform.position = targetPos;

        ctx.velocity = Vector2.zero;
        ctx.grounded = true;
        ctx.frameLeftGrounded = Time.time;
    }

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }
}
