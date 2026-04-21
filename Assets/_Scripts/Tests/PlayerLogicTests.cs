using System;
using NUnit.Framework;

namespace Tests
{
    // Inline test-doubles, повторяющие алгоритмы из PlayerHealth, FallDamage,
    // PlayerContext.CanRun и Inventory, но без зависимости от MonoBehaviour.
    // Логика скопирована 1-в-1 из продакшн-классов.

    // ── Mirrors PlayerHealth (PlayerHealth.cs) ────────────────────────────────

    class HealthTracker
    {
        public readonly float Max;
        public float Current { get; private set; }
        public bool IsAlive  { get; private set; } = true;
        public bool DiedFired;

        public HealthTracker(float max) { Max = max; Current = max; }

        public void TakeDamage(float damage)
        {
            if (!IsAlive || damage <= 0f) return;
            Current = Math.Max(0f, Current - damage);
            if (Current <= 0f) Die();
        }

        public void Die()
        {
            if (!IsAlive) return;
            IsAlive   = false;
            DiedFired = true;
        }
    }

    // ── Mirrors FallDamage.ApplyFallDamage() (FallDamage.cs) ─────────────────

    static class FallDamageCalculator
    {
        public static float Compute(
            float fallHeight,
            float minHeight,
            float damagePerUnit,
            float lethalHeight,
            bool  didRoll,
            float rollMultiplier)
        {
            if (fallHeight < minHeight)    return 0f;
            if (fallHeight >= lethalHeight) return float.MaxValue;   // летальное

            float damage = (fallHeight - minHeight) * damagePerUnit;
            if (didRoll) damage *= rollMultiplier;
            return damage;
        }
    }

    // ── Mirrors Inventory.Add / Remove / Has (Inventory.cs) ──────────────────

    class SimpleInventory
    {
        class Entry { public string ItemId; public int MaxStack; public int Count; }
        readonly System.Collections.Generic.List<Entry> _entries = new();

        public void Add(string itemId, int maxStack, int count = 1)
        {
            int remaining = count;
            foreach (var e in _entries)
            {
                if (e.ItemId != itemId) continue;
                int space = maxStack - e.Count;
                if (space <= 0) continue;
                int toAdd = Math.Min(space, remaining);
                e.Count   += toAdd;
                remaining -= toAdd;
                if (remaining <= 0) break;
            }
            while (remaining > 0)
            {
                int toAdd = Math.Min(maxStack, remaining);
                _entries.Add(new Entry { ItemId = itemId, MaxStack = maxStack, Count = toAdd });
                remaining -= toAdd;
            }
        }

        public bool Has(string itemId, int count = 1)
        {
            int total = 0;
            foreach (var e in _entries)
                if (e.ItemId == itemId) total += e.Count;
            return total >= count;
        }

        public int StackCount(string itemId)
        {
            int n = 0;
            foreach (var e in _entries)
                if (e.ItemId == itemId) n++;
            return n;
        }
    }

    // ── Mirrors PlayerStaminaController + PlayerBreathController ─────────────
    // (UpdateStamina / RegenStamina / UpdateBreath — без MonoBehaviour)

    class StaminaTracker
    {
        public float Max;
        public float Current;
        public float DrainPerSecond;
        public float RegenPerSecond;
        public float RegenMovingMultiplier;
        public float MinToRun;
        public float MinToHoldBreath;
        public float BreathDrainPerSecond;

        public float DrainMultiplier;       // ctx.currentStaminaDrainMultiplier
        public float BreathDrainMultiplier; // ctx.currentStaminaBreathDrainMultiplier
        public bool  IsHoldingBreath;
        public bool  IsGrounded;
        public bool  IsMoving;

        public bool CanRun        => Current > MinToRun;
        public bool CanHoldBreath => BreathDrainMultiplier > 0f && DrainMultiplier == 0f;

        // Mirrors PlayerStaminaController.UpdateStamina()
        public void UpdateStamina(float dt)
        {
            if (IsHoldingBreath) return;

            float consumption = DrainPerSecond * DrainMultiplier * dt;
            if (consumption > 0f)
                Current = Math.Max(0f, Current - consumption);
            else if (IsGrounded)
                RegenStamina(dt);
        }

        // Mirrors PlayerStaminaController.RegenStamina()
        void RegenStamina(float dt)
        {
            float mult  = IsMoving ? RegenMovingMultiplier : 1f;
            float regen = RegenPerSecond * mult * dt;
            Current = Math.Min(Max, Current + regen);
        }

        // Mirrors PlayerBreathController.UpdateBreath()
        public void UpdateBreath(bool holdInput, float dt)
        {
            if (!CanHoldBreath) { IsHoldingBreath = false; return; }

            if (IsHoldingBreath)
            {
                if (!holdInput || Current <= 0f) { IsHoldingBreath = false; return; }
            }
            else if (holdInput && Current >= MinToHoldBreath)
            {
                IsHoldingBreath = true;
            }

            if (IsHoldingBreath)
            {
                float drain = BreathDrainPerSecond * Math.Max(0f, BreathDrainMultiplier);
                Current = Math.Max(0f, Current - drain * dt);
                if (Current <= 0f) IsHoldingBreath = false;
            }
        }
    }

    // ── Tests ─────────────────────────────────────────────────────────────────

    [TestFixture]
    public class PlayerLogicTests
    {
        // 1. TakeDamage уменьшает текущее HP
        [Test]
        public void Health_TakeDamage_ReducesCurrentHealth()
        {
            var h = new HealthTracker(100f);

            h.TakeDamage(30f);

            Assert.That(h.Current, Is.EqualTo(70f));
        }

        // 2. HP не может стать отрицательным
        [Test]
        public void Health_TakeDamage_ClampsToZero()
        {
            var h = new HealthTracker(50f);

            h.TakeDamage(999f);

            Assert.That(h.Current, Is.EqualTo(0f));
        }

        // 3. Смерть: событие срабатывает и isAlive становится false
        [Test]
        public void Health_LethalDamage_SetsDiedAndIsAlive()
        {
            var h = new HealthTracker(100f);

            h.TakeDamage(100f);

            Assert.That(h.IsAlive,   Is.False);
            Assert.That(h.DiedFired, Is.True);
        }

        // 4. Урон игнорируется после смерти
        [Test]
        public void Health_TakeDamage_IgnoredAfterDeath()
        {
            var h = new HealthTracker(100f);
            h.Die();

            h.TakeDamage(50f);

            Assert.That(h.Current, Is.EqualTo(100f));
        }

        // 5. Нулевой и отрицательный урон не изменяет HP
        [Test]
        public void Health_TakeDamage_NonPositive_IsIgnored()
        {
            var h = new HealthTracker(100f);

            h.TakeDamage(0f);
            h.TakeDamage(-10f);

            Assert.That(h.Current, Is.EqualTo(100f));
        }

        // 6. Падение ниже минимальной высоты не наносит урона
        [Test]
        public void FallDamage_BelowMinHeight_DealsNoDamage()
        {
            float dmg = FallDamageCalculator.Compute(
                fallHeight: 5f, minHeight: 10f, damagePerUnit: 5f,
                lethalHeight: 40f, didRoll: false, rollMultiplier: 0.35f);

            Assert.That(dmg, Is.EqualTo(0f));
        }

        // 7. Летальная высота → урон float.MaxValue (мгновенная смерть)
        [Test]
        public void FallDamage_AtLethalHeight_IsLethal()
        {
            float dmg = FallDamageCalculator.Compute(
                fallHeight: 40f, minHeight: 10f, damagePerUnit: 5f,
                lethalHeight: 40f, didRoll: false, rollMultiplier: 0.35f);

            Assert.That(dmg, Is.EqualTo(float.MaxValue));
        }

        // 8. Формула урона: (height − minHeight) × damagePerUnit
        [Test]
        public void FallDamage_Formula_MatchesExpected()
        {
            // (20 - 10) * 5 = 50
            float dmg = FallDamageCalculator.Compute(
                fallHeight: 20f, minHeight: 10f, damagePerUnit: 5f,
                lethalHeight: 40f, didRoll: false, rollMultiplier: 0.35f);

            Assert.That(dmg, Is.EqualTo(50f).Within(0.001f));
        }

        // 9. Кувырок при приземлении уменьшает урон
        [Test]
        public void FallDamage_WithRoll_DamageIsReduced()
        {
            float withoutRoll = FallDamageCalculator.Compute(20f, 10f, 5f, 40f, false, 0.35f);
            float withRoll    = FallDamageCalculator.Compute(20f, 10f, 5f, 40f, true,  0.35f);

            Assert.That(withRoll, Is.LessThan(withoutRoll));
            Assert.That(withRoll, Is.EqualTo(withoutRoll * 0.35f).Within(0.001f));
        }

        // ── Stamina ───────────────────────────────────────────────────────────

        StaminaTracker MakeStamina(float current = 100f) => new StaminaTracker
        {
            Max                  = 100f,
            Current              = current,
            DrainPerSecond       = 20f,
            RegenPerSecond       = 15f,
            RegenMovingMultiplier = 0.4f,
            MinToRun             = 35f,
            MinToHoldBreath      = 25f,
            BreathDrainPerSecond = 20f,
            IsGrounded           = true,
        };

        // 11. Стамина тратится пропорционально DrainMultiplier и deltaTime
        [Test]
        public void Stamina_Drain_ReducesCurrentByExpectedAmount()
        {
            var s = MakeStamina();
            s.DrainMultiplier = 1f;

            s.UpdateStamina(1f);   // 20 * 1 * 1 = 20

            Assert.That(s.Current, Is.EqualTo(80f).Within(0.001f));
        }

        // 12. Нулевой DrainMultiplier → стамина не тратится, а восстанавливается
        [Test]
        public void Stamina_ZeroDrainMultiplier_RegensOnGround()
        {
            var s = MakeStamina(50f);
            s.DrainMultiplier = 0f;

            s.UpdateStamina(1f);   // regen: 15 * 1 * 1 = 15

            Assert.That(s.Current, Is.EqualTo(65f).Within(0.001f));
        }

        // 13. Регенерация в движении умножается на MovingMultiplier
        [Test]
        public void Stamina_RegenWhileMoving_AppliesMovingMultiplier()
        {
            var s = MakeStamina(50f);
            s.DrainMultiplier = 0f;
            s.IsMoving = true;

            s.UpdateStamina(1f);   // 15 * 0.4 * 1 = 6

            Assert.That(s.Current, Is.EqualTo(56f).Within(0.001f));
        }

        // 14. Стамина не превышает Max при регенерации
        [Test]
        public void Stamina_Regen_ClampsToMax()
        {
            var s = MakeStamina(99f);
            s.DrainMultiplier = 0f;

            s.UpdateStamina(1f);   // regen 15, но max = 100

            Assert.That(s.Current, Is.EqualTo(100f));
        }

        // 15. Стамина не уходит в отрицательную при большом drain
        [Test]
        public void Stamina_Drain_ClampsToZero()
        {
            var s = MakeStamina(5f);
            s.DrainMultiplier = 1f;

            s.UpdateStamina(1f);   // нужно потратить 20, но есть только 5

            Assert.That(s.Current, Is.EqualTo(0f));
        }

        // 16. CanRun = false когда стамина ≤ MinToRun
        [Test]
        public void Stamina_CanRun_FalseWhenBelowThreshold()
        {
            var s = MakeStamina(35f);   // ровно на пороге

            Assert.That(s.CanRun, Is.False);
        }

        // 17. Задержка дыхания расходует стамину
        [Test]
        public void Stamina_HoldBreath_DrainsStamina()
        {
            var s = MakeStamina(100f);
            s.BreathDrainMultiplier = 1f;   // CanHoldBreath = true (DrainMultiplier == 0)
            s.DrainMultiplier       = 0f;

            s.UpdateBreath(holdInput: true, dt: 1f);   // начинает задержку
            s.UpdateBreath(holdInput: true, dt: 1f);   // тратит: 20 * 1 * 1 = 20

            Assert.That(s.IsHoldingBreath, Is.True);
            Assert.That(s.Current, Is.LessThan(100f));
        }

        // 18. Задержка дыхания прекращается при обнулении стамины
        [Test]
        public void Stamina_HoldBreath_StopsWhenStaminaDepleted()
        {
            var s = MakeStamina(1f);
            s.BreathDrainMultiplier = 1f;
            s.DrainMultiplier       = 0f;
            s.IsHoldingBreath       = true;

            s.UpdateBreath(holdInput: true, dt: 1f);   // тратит 20, но есть 1 → обнуляется

            Assert.That(s.IsHoldingBreath, Is.False);
            Assert.That(s.Current, Is.EqualTo(0f));
        }

        // 10. Предметы сверх maxStackSize создают новый стак в инвентаре
        [Test]
        public void Inventory_AddOverMaxStack_CreatesNewStack()
        {
            var inv = new SimpleInventory();
            const int maxStack = 5;

            inv.Add("stone", maxStack, count: 7);   // 5 в первом, 2 во втором

            Assert.That(inv.StackCount("stone"), Is.EqualTo(2));
            Assert.That(inv.Has("stone", 7),     Is.True);
        }
    }
}
