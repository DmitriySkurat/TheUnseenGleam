using System;
using NUnit.Framework;

namespace Tests
{
    // Inline test-doubles, повторяющие алгоритмы агента из:
    //   AgentSuspicious.cs, AgentRoot.cs, AgentChase.cs, AgentContext.cs
    // Логика скопирована 1-в-1 из продакшн-классов.

    // ── Mirrors suspicion state data (AgentContext) ───────────────────────────

    enum SuspicionSourceTest { None, Vision, Noise }

    class AgentSuspicionState
    {
        // Stats
        public float SuspicionTimeOnSight = 1.5f;
        public float SuspicionTimeOnNoise = 2.5f;

        // Runtime
        public SuspicionSourceTest Source;
        public float  Timer;
        public int    NoisesHeardDuringSuspicion;
        public bool   PendingNoiseAlert;
        public bool   CanSeePlayer;

        // Mirrors AgentSuspicious.OnEnter() — vision branch
        public void EnterFromVision()
        {
            Source                      = SuspicionSourceTest.Vision;
            Timer                       = SuspicionTimeOnSight;
            NoisesHeardDuringSuspicion  = 0;
            PendingNoiseAlert           = false;
        }

        // Mirrors AgentSuspicious.OnEnter() — noise branch
        public void EnterFromNoise()
        {
            Source                      = SuspicionSourceTest.Noise;
            Timer                       = SuspicionTimeOnNoise;
            NoisesHeardDuringSuspicion  = 1;
            PendingNoiseAlert           = false;
        }

        // Mirrors AgentSuspicious.OnUpdate()
        public void Update(float dt)
        {
            if (PendingNoiseAlert)
            {
                NoisesHeardDuringSuspicion++;
                PendingNoiseAlert = false;
            }

            // Noise → upgrade to vision when player appears
            if (Source == SuspicionSourceTest.Noise && CanSeePlayer)
            {
                Source = SuspicionSourceTest.Vision;
                Timer  = SuspicionTimeOnSight;
            }

            if (Timer > 0f) Timer -= dt;
        }

        // Mirrors the Noise-branch in AgentSuspicious.GetTransition()
        public bool ShouldGoToSearch =>
            Source == SuspicionSourceTest.Noise && NoisesHeardDuringSuspicion > 1;
    }

    // ── Mirrors blinding / stun logic (AgentRoot + AgentContext) ─────────────

    class AgentBlindingState
    {
        public float BlindDurationToStun        = 2f;
        public float BlindedByPlayerReactionTime = 0.3f;

        public float BlindedByPlayerTimer;

        // Mirrors AgentRoot.GetTransition() — global stun check
        public bool ShouldStun => BlindedByPlayerTimer >= BlindDurationToStun;

        // Mirrors AgentBlindedByPlayer — reaction guard
        public bool HasReacted => BlindedByPlayerTimer >= BlindedByPlayerReactionTime;
    }

    // ── Mirrors grab-state conditions (AgentContext / AgentRoot) ─────────────

    class AgentGrabState
    {
        public float GrabCooldown = 3f;

        public float CooldownTimer;
        public bool  IsGrabbingPlayer;
        public bool  PlayerIsGrabbed;
        public float GrabReactionTimer;

        // Mirrors AgentRoot.GetTransition() — return-to-patrol condition
        public bool ShouldReturnToPatrol =>
            !IsGrabbingPlayer &&
            PlayerIsGrabbed  &&
            GrabReactionTimer <= 0f;

        // Grab is available when cooldown expired
        public bool CanGrab => CooldownTimer <= 0f;
    }

    // ── Mirrors patrol-point logic (AgentContext) ─────────────────────────────

    class AgentPatrolState
    {
        public float   WaitTimer;
        public float   PatrolSpeed;
        public int     PatrolPointCount;

        // Mirrors AgentContext.IsWaitingAtPoint
        public bool IsWaitingAtPoint => WaitTimer > 0f;
    }

    // ── Tests ─────────────────────────────────────────────────────────────────

    [TestFixture]
    public class AgentLogicTests
    {
        // 1. Таймер подозрения от зрения = SuspicionTimeOnSight
        [Test]
        public void Suspicion_EnterFromVision_TimerEqualsSuspicionTimeOnSight()
        {
            var s = new AgentSuspicionState();

            s.EnterFromVision();

            Assert.That(s.Timer, Is.EqualTo(s.SuspicionTimeOnSight));
        }

        // 2. Таймер подозрения от шума = SuspicionTimeOnNoise
        [Test]
        public void Suspicion_EnterFromNoise_TimerEqualsSuspicionTimeOnNoise()
        {
            var s = new AgentSuspicionState();

            s.EnterFromNoise();

            Assert.That(s.Timer, Is.EqualTo(s.SuspicionTimeOnNoise));
        }

        // 3. Таймер подозрения уменьшается с каждым Update
        [Test]
        public void Suspicion_Update_TimerDecreasedByDeltaTime()
        {
            var s = new AgentSuspicionState();
            s.EnterFromVision();
            float before = s.Timer;

            s.Update(0.5f);

            Assert.That(s.Timer, Is.EqualTo(before - 0.5f).Within(0.001f));
        }

        // 4. Второй шум в окне подозрения → переход в Search
        [Test]
        public void Suspicion_TwoNoisesInWindow_ShouldGoToSearch()
        {
            var s = new AgentSuspicionState();
            s.EnterFromNoise();        // первый шум (count = 1)
            s.PendingNoiseAlert = true;
            s.Update(0f);              // поглощает второй шум (count = 2)

            Assert.That(s.ShouldGoToSearch, Is.True);
        }

        // 5. Один шум в окне подозрения → не переходим в Search
        [Test]
        public void Suspicion_OneNoiseInWindow_ShouldNotGoToSearch()
        {
            var s = new AgentSuspicionState();
            s.EnterFromNoise();        // count = 1

            Assert.That(s.ShouldGoToSearch, Is.False);
        }

        // 6. Ослепление >= BlindDurationToStun → агент должен получить стан
        [Test]
        public void Blinding_TimerAtStunThreshold_ShouldStun()
        {
            var b = new AgentBlindingState { BlindedByPlayerTimer = 2f };

            Assert.That(b.ShouldStun, Is.True);
        }

        // 7. Ослепление < BlindDurationToStun → стана нет
        [Test]
        public void Blinding_TimerBelowStunThreshold_ShouldNotStun()
        {
            var b = new AgentBlindingState { BlindedByPlayerTimer = 1.9f };

            Assert.That(b.ShouldStun, Is.False);
        }

        // 8. Агент реагирует только после истечения ReactionTime
        [Test]
        public void Blinding_ReactionTime_HasReactedOnlyAfterThreshold()
        {
            var b = new AgentBlindingState();

            b.BlindedByPlayerTimer = 0.2f;
            Assert.That(b.HasReacted, Is.False, "Too early");

            b.BlindedByPlayerTimer = 0.3f;
            Assert.That(b.HasReacted, Is.True, "At threshold");
        }

        // 9. Условие возврата на патруль: игрок схвачен ДРУГИМ агентом,
        //    текущий агент не захватывает, reaction timer истёк
        [Test]
        public void Grab_ShouldReturnToPatrol_WhenOtherAgentGrabbedAndReactionDone()
        {
            var g = new AgentGrabState
            {
                IsGrabbingPlayer  = false,
                PlayerIsGrabbed   = true,
                GrabReactionTimer = 0f,
            };

            Assert.That(g.ShouldReturnToPatrol, Is.True);
        }

        // 10. Агент на посту (патрульный таймер ожидания > 0)
        [Test]
        public void Patrol_WaitTimer_IsWaitingAtPointWhenTimerPositive()
        {
            var p = new AgentPatrolState { WaitTimer = 1.5f };
            Assert.That(p.IsWaitingAtPoint, Is.True);

            p.WaitTimer = 0f;
            Assert.That(p.IsWaitingAtPoint, Is.False);
        }
    }
}
