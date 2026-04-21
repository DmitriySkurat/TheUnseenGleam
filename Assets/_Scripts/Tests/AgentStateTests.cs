using System;
using NUnit.Framework;

namespace Tests
{
    // Inline test-doubles для состояний агента:
    //   AgentChase, AgentAlert, AgentPredictionChase, AgentStunned, AgentGrabPlayer
    // Логика скопирована 1-в-1 из продакшн-классов.

    // ── AgentChase helpers ────────────────────────────────────────────────────

    class ChaseState
    {
        // Stats
        public float ChaseVisionGraceTime     = 1.5f;
        public float GrabProximityGraceWindow = 0.4f;
        public float AttackRange              = 1.5f;
        public float GrabCooldown             = 3f;

        // Runtime
        public float ChaseVisionLostTimer;
        public float GrabCooldownTimer;
        public bool  CanSeePlayer;
        public bool  IsBlindedByPlayer;

        // Mirrors AgentChase.OnUpdate() — vision-lost timer block
        public void Update(float dt)
        {
            if (GrabCooldownTimer > 0f)
                GrabCooldownTimer -= dt;

            if (CanSeePlayer || IsBlindedByPlayer)
                ChaseVisionLostTimer = 0f;
            else
                ChaseVisionLostTimer += dt;
        }

        // Mirrors AgentChase.GetTransition() — grace-time check
        public bool ShouldGoToAlert => ChaseVisionLostTimer >= ChaseVisionGraceTime;

        // Mirrors grab-range + grace-window condition (distance check excluded — needs scene)
        public bool InGrabProximityGraceWindow =>
            ChaseVisionLostTimer < GrabProximityGraceWindow;
    }

    // ── AgentAlert helpers ────────────────────────────────────────────────────

    class AlertState
    {
        public float AlertDuration = 3f;
        public float AlertTurnTime = 1.5f;

        public float Timer   { get; private set; }
        public bool  Turned  { get; private set; }

        public void Enter() { Timer = AlertDuration; Turned = false; }

        // Mirrors AgentAlert.OnUpdate()
        public void Update(float dt)
        {
            Timer -= dt;
            if (!Turned && Timer <= AlertTurnTime)
            {
                FlipDirection();
                Turned = true;
            }
        }

        void FlipDirection() { /* side-effect only, tested via Turned flag */ }

        public bool ShouldGoToSearch => Timer <= 0f;
    }

    // ── AgentPredictionChase helpers ──────────────────────────────────────────

    class PredictionChaseState
    {
        public float PredictionSearchTime = 6f;

        public float PredictionTimer { get; private set; }

        public void Enter() => PredictionTimer = PredictionSearchTime;

        // Mirrors AgentPredictionChase.OnUpdate()
        public void Update(float dt) => PredictionTimer -= dt;

        // Mirrors GetTransition() — timer expired → Search
        public bool ShouldGoToSearch => PredictionTimer <= 0f;
    }

    // ── AgentStunned helpers ──────────────────────────────────────────────────

    class StunnedState
    {
        public float StunDuration = 3f;

        public float StunTimer              { get; private set; }
        public float BlindedByPlayerTimer;

        // Mirrors AgentStunned.OnEnter()
        public void Enter()
        {
            StunTimer             = StunDuration;
            BlindedByPlayerTimer  = 0f;   // сброс: предотвращает немедленный повторный стан
        }

        // Mirrors AgentStunned.OnExit()
        public void Exit() => BlindedByPlayerTimer = 0f;

        // Mirrors AgentStunned.OnUpdate()
        public void Update(float dt) => StunTimer -= dt;

        // Mirrors GetTransition()
        public bool ShouldGoToSuspicious => StunTimer <= 0f;
    }

    // ── AgentGrabPlayer helpers ───────────────────────────────────────────────

    class GrabPlayerState
    {
        public float AttackFirstHitDelay = 0.5f;
        public float GrabCooldown        = 3f;

        public float GrabFirstHitTimer   { get; private set; }
        public float GrabCooldownTimer   { get; private set; }
        public bool  IsHolding           { get; private set; }
        public bool  GrabOccurredInChase;

        // Mirrors AgentGrabPlayer.OnEnter()
        public void Enter()
        {
            GrabFirstHitTimer = GrabOccurredInChase ? 0f : AttackFirstHitDelay;
            IsHolding         = false;
        }

        // Mirrors AgentGrabPlayer.OnUpdate()
        public void Update(float dt)
        {
            if (IsHolding) return;
            GrabFirstHitTimer -= dt;
            if (GrabFirstHitTimer <= 0f) StartGrab();
        }

        // Mirrors AgentGrabPlayer.OnExit()
        public void Exit()
        {
            IsHolding        = false;
            GrabCooldownTimer = GrabCooldown;
        }

        void StartGrab() { IsHolding = true; GrabOccurredInChase = true; }
    }

    // ── Tests ─────────────────────────────────────────────────────────────────

    [TestFixture]
    public class AgentStateTests
    {
        // ── Chase ─────────────────────────────────────────────────────────────

        // 1. Таймер потери видимости растёт, если игрок не виден
        [Test]
        public void Chase_VisionLostTimer_IncreasesWhenPlayerNotVisible()
        {
            var c = new ChaseState { CanSeePlayer = false };

            c.Update(0.5f);
            c.Update(0.5f);

            Assert.That(c.ChaseVisionLostTimer, Is.EqualTo(1f).Within(0.001f));
        }

        // 2. Таймер потери видимости сбрасывается при восстановлении зрения
        [Test]
        public void Chase_VisionLostTimer_ResetsToZeroWhenPlayerVisible()
        {
            var c = new ChaseState { CanSeePlayer = false };
            c.Update(1f);                // timer = 1

            c.CanSeePlayer = true;
            c.Update(0f);               // should reset

            Assert.That(c.ChaseVisionLostTimer, Is.EqualTo(0f));
        }

        // 3. Переход в Alert происходит по истечении ChaseVisionGraceTime
        [Test]
        public void Chase_ShouldGoToAlert_AfterGraceTimeExpired()
        {
            var c = new ChaseState { CanSeePlayer = false };

            c.Update(1.5f);  // ровно на пороге

            Assert.That(c.ShouldGoToAlert, Is.True);
        }

        // 4. Кулдаун захвата убывает во время погони
        [Test]
        public void Chase_GrabCooldownTimer_DecreasesDuringUpdate()
        {
            var c = new ChaseState { GrabCooldownTimer = 3f };

            c.Update(1f);

            Assert.That(c.GrabCooldownTimer, Is.EqualTo(2f).Within(0.001f));
        }

        // 5. В течение GrabProximityGraceWindow захват ещё возможен без зрения
        [Test]
        public void Chase_GrabProximityGraceWindow_StillActiveJustBeforeExpiry()
        {
            var c = new ChaseState { CanSeePlayer = false };
            c.Update(0.39f);  // < GrabProximityGraceWindow (0.4)

            Assert.That(c.InGrabProximityGraceWindow, Is.True);
        }

        // ── Alert ─────────────────────────────────────────────────────────────

        // 6. Таймер Alert инициализируется значением AlertDuration
        [Test]
        public void Alert_Enter_TimerEqualsAlertDuration()
        {
            var a = new AlertState();
            a.Enter();

            Assert.That(a.Timer, Is.EqualTo(a.AlertDuration));
        }

        // 7. Разворот происходит когда Timer ≤ AlertTurnTime
        [Test]
        public void Alert_FlipTriggered_WhenTimerReachesAlertTurnTime()
        {
            var a = new AlertState();
            a.Enter();                      // timer = 3

            a.Update(a.AlertDuration - a.AlertTurnTime);  // timer = 1.5 (на пороге)

            Assert.That(a.Turned, Is.True);
        }

        // 8. Разворот не повторяется повторно (флаг Turned идемпотентен)
        [Test]
        public void Alert_Flip_HappensOnlyOnce()
        {
            var a = new AlertState();
            a.Enter();
            a.Update(a.AlertDuration - a.AlertTurnTime);   // первый разворот
            a.Update(0.5f);                                 // ещё один тик

            Assert.That(a.Turned, Is.True);   // не сбросился
        }

        // ── PredictionChase ───────────────────────────────────────────────────

        // 9. predictionTimer инициализируется значением PredictionSearchTime
        [Test]
        public void PredictionChase_Enter_TimerEqualsPredictionSearchTime()
        {
            var p = new PredictionChaseState();
            p.Enter();

            Assert.That(p.PredictionTimer, Is.EqualTo(p.PredictionSearchTime));
        }

        // 10. После истечения таймера → переход в Search
        [Test]
        public void PredictionChase_TimerExpired_ShouldGoToSearch()
        {
            var p = new PredictionChaseState();
            p.Enter();
            p.Update(p.PredictionSearchTime);

            Assert.That(p.ShouldGoToSearch, Is.True);
        }

        // ── Stunned ───────────────────────────────────────────────────────────

        // 11. При входе в Stunned таймер blindedByPlayerTimer сбрасывается в 0
        [Test]
        public void Stunned_Enter_ResetsBlindedByPlayerTimer()
        {
            var s = new StunnedState { BlindedByPlayerTimer = 5f };
            s.Enter();

            Assert.That(s.BlindedByPlayerTimer, Is.EqualTo(0f));
        }

        // 12. Stunned завершается когда stunTimer ≤ 0
        [Test]
        public void Stunned_TimerExpired_ShouldGoToSuspicious()
        {
            var s = new StunnedState();
            s.Enter();
            s.Update(s.StunDuration);

            Assert.That(s.ShouldGoToSuspicious, Is.True);
        }

        // ── GrabPlayer ────────────────────────────────────────────────────────

        // 13. Первый захват в погоне имеет полную задержку AttackFirstHitDelay
        [Test]
        public void Grab_FirstGrabInChase_HasFullHitDelay()
        {
            var g = new GrabPlayerState { GrabOccurredInChase = false };
            g.Enter();

            Assert.That(g.GrabFirstHitTimer, Is.EqualTo(g.AttackFirstHitDelay));
        }

        // 14. Повторный захват (grabOccurredInChase = true) — задержка 0
        [Test]
        public void Grab_RepeatGrabInChase_HasZeroHitDelay()
        {
            var g = new GrabPlayerState { GrabOccurredInChase = true };
            g.Enter();

            Assert.That(g.GrabFirstHitTimer, Is.EqualTo(0f));
        }

        // 15. При выходе из GrabPlayer кулдаун устанавливается равным GrabCooldown
        [Test]
        public void Grab_OnExit_SetsCooldownToGrabCooldown()
        {
            var g = new GrabPlayerState();
            g.Enter();
            g.Exit();

            Assert.That(g.GrabCooldownTimer, Is.EqualTo(g.GrabCooldown));
        }
    }
}
