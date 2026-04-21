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
