using UnityEngine;

namespace HSM
{
    public class Hide : State
    {
        private PlayerContext ctx;
        private HidingSpotInteractable activeHidingSpot;
        private int playerSortingOrder;
        private int hidingSpotSortingOrder;
        private bool hasSwappedSortingOrder;

        public Hide(StateMachine m, State parent, PlayerContext ctx) : base(m, parent)
        {
            this.ctx = ctx;
            Add(new ColorPhaseActivity(ctx.renderer){
                enterColor = Color.cyan,
            });
            Add(new AnimatorBoolActivity(ctx.anim, "Hide", true, false));
        }

        protected override void OnEnter()
        {
            SwapSortingOrderWithHidingSpot();
            
            ctx.isHiding = true;
            ctx.currentSpeedMultiplier = ctx.stats.HideSpeedMultiplier;
            ctx.currentNoiseRadius = 0f;
            ctx.currentFootstepInterval = 0f;
            ctx.currentStaminaDrainMultiplier = 0f;
            ctx.currentStaminaBreathDrainMultiplier = ctx.stats.HideStaminaBreathDrainMultiplier;
            ctx.velocity = Vector2.zero;
        }

        protected override void OnExit()
        {
            RestoreSortingOrderWithHidingSpot();

            ctx.isHiding = false;
            ctx.velocity.y = 0f;
            ctx.currentStaminaDrainMultiplier = 1f;
            ctx.currentNoiseRadius = 0f;
            ctx.currentFootstepInterval = 0f;
            ctx.currentStaminaDrainMultiplier = 0f;
            ctx.currentStaminaBreathDrainMultiplier = 0f;
        }

        protected override void OnUpdate(float deltaTime)
        {
            ctx.velocity = Vector2.zero;
            base.OnUpdate(deltaTime);
        }

        protected override State GetTransition()
        {
            if (!ctx.isHiding)
            {
                if (ctx.WantsCrouch) return Machine != null ? Machine.GetState<Crouch>() : null;
                if (ctx.HasMovementIntent) return Machine != null ? Machine.GetState<Move>() : null;
                return Machine != null ? Machine.GetState<Idle>() : null;
            }

            return null;
        }

        private void SwapSortingOrderWithHidingSpot()
        {
            hasSwappedSortingOrder = false;
            activeHidingSpot = ctx.activeHidingSpot;

            if (ctx.renderer == null || activeHidingSpot == null)
                return;

            var hidingSpotRenderer = activeHidingSpot.VisualRenderer;
            if (hidingSpotRenderer == null)
                return;

            playerSortingOrder = ctx.renderer.sortingOrder;
            hidingSpotSortingOrder = hidingSpotRenderer.sortingOrder;

            ctx.renderer.sortingOrder = hidingSpotSortingOrder;
            hidingSpotRenderer.sortingOrder = playerSortingOrder;
            hasSwappedSortingOrder = true;
        }

        private void RestoreSortingOrderWithHidingSpot()
        {
            if (hasSwappedSortingOrder)
            {
                if (ctx.renderer != null)
                    ctx.renderer.sortingOrder = playerSortingOrder;

                var hidingSpotRenderer = activeHidingSpot != null ? activeHidingSpot.VisualRenderer : null;
                if (hidingSpotRenderer != null)
                    hidingSpotRenderer.sortingOrder = hidingSpotSortingOrder;
            }

            hasSwappedSortingOrder = false;
            activeHidingSpot = null;
            ctx.activeHidingSpot = null;
        }
    }
}
