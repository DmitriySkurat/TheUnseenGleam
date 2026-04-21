using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using HSM;

namespace Tests
{
    // ── Minimal concrete implementations for testing ──────────────────────────

    class RootState : State
    {
        public RootState(StateMachine m) : base(m) { }
    }

    class ChildStateA : State
    {
        public ChildStateA(StateMachine m, State parent) : base(m, parent) { }
    }

    class ChildStateB : State
    {
        public ChildStateB(StateMachine m, State parent) : base(m, parent) { }
    }

    class GrandChildState : State
    {
        public GrandChildState(StateMachine m, State parent) : base(m, parent) { }
    }

    class ConcreteActivity : Activity
    {
        public int ActivateCallCount;
        public int DeactivateCallCount;

        public override async Task ActivateAsync(CancellationToken ct)
        {
            ActivateCallCount++;
            await base.ActivateAsync(ct);
        }

        public override async Task DeactivateAsync(CancellationToken ct)
        {
            DeactivateCallCount++;
            await base.DeactivateAsync(ct);
        }
    }

    // ── Tests ─────────────────────────────────────────────────────────────────

    [TestFixture]
    public class HsmTests
    {
        StateMachine BuildMachine(out RootState root)
        {
            root = null;
            var tempRoot = new RootState(null!);
            var machine  = new StateMachine(tempRoot);
            root = tempRoot;
            return machine;
        }

        // 1. Leaf() on a state with no active child returns itself
        [Test]
        public void Leaf_NoActiveChild_ReturnsSelf()
        {
            var root    = new RootState(null!);
            var machine = new StateMachine(root);

            Assert.That(root.Leaf(), Is.SameAs(root));
        }

        // 2. Leaf() traverses to the deepest active child
        [Test]
        public void Leaf_WithNestedActiveChildren_ReturnsDeepest()
        {
            var root      = new RootState(null!);
            var machine   = new StateMachine(root);
            var childA    = new ChildStateA(machine, root);
            var grandChild = new GrandChildState(machine, childA);

            root.ActiveChild   = childA;
            childA.ActiveChild = grandChild;

            Assert.That(root.Leaf(), Is.SameAs(grandChild));
        }

        // 3. PathToRoot() yields the full ancestor chain in order
        [Test]
        public void PathToRoot_YieldsFullChain()
        {
            var root      = new RootState(null!);
            var machine   = new StateMachine(root);
            var childA    = new ChildStateA(machine, root);
            var grandChild = new GrandChildState(machine, childA);

            var path = grandChild.PathToRoot().ToList();

            Assert.That(path, Is.EqualTo(new List<State> { grandChild, childA, root }));
        }

        // 4. LCA of a node with itself is that node
        [Test]
        public void Lca_SameNode_ReturnsThatNode()
        {
            var root   = new RootState(null!);
            var machine = new StateMachine(root);

            var lca = TransitionSequencer.Lca(root, root);

            Assert.That(lca, Is.SameAs(root));
        }

        // 5. LCA of child and parent is the parent
        [Test]
        public void Lca_ChildAndParent_ReturnsParent()
        {
            var root   = new RootState(null!);
            var machine = new StateMachine(root);
            var child   = new ChildStateA(machine, root);

            var lca = TransitionSequencer.Lca(child, root);

            Assert.That(lca, Is.SameAs(root));
        }

        // 6. LCA of two siblings is their shared parent
        [Test]
        public void Lca_TwoSiblings_ReturnsParent()
        {
            var root   = new RootState(null!);
            var machine = new StateMachine(root);
            var a      = new ChildStateA(machine, root);
            var b      = new ChildStateB(machine, root);

            var lca = TransitionSequencer.Lca(a, b);

            Assert.That(lca, Is.SameAs(root));
        }

        // 7. StateMachine.GetState<T>() finds a registered state by exact type
        [Test]
        public void GetState_ExactType_ReturnsState()
        {
            var root    = new RootState(null!);
            var machine = new StateMachineBuilder(root).Build();

            var found = machine.GetState<RootState>();

            Assert.That(found, Is.SameAs(root));
        }

        // 8. StateMachine.GetState<T>() returns null when type not registered
        [Test]
        public void GetState_UnregisteredType_ReturnsNull()
        {
            var root    = new RootState(null!);
            var machine = new StateMachine(root);

            var found = machine.GetState<ChildStateA>();

            Assert.That(found, Is.Null);
        }

        // 9. Activity.ActivateAsync transitions mode from Inactive → Active
        [Test]
        public async Task Activity_Activate_TransitionsModeToActive()
        {
            var activity = new ConcreteActivity();

            Assert.That(activity.Mode, Is.EqualTo(ActivityMode.Inactive));

            await activity.ActivateAsync(CancellationToken.None);

            Assert.That(activity.Mode, Is.EqualTo(ActivityMode.Active));
        }

        // 10. Activity.ActivateAsync is idempotent — calling it twice only activates once
        [Test]
        public async Task Activity_ActivateTwice_OnlyActivatesOnce()
        {
            var activity = new ConcreteActivity();

            await activity.ActivateAsync(CancellationToken.None);
            await activity.ActivateAsync(CancellationToken.None); // should be a no-op

            Assert.That(activity.ActivateCallCount, Is.EqualTo(1));
            Assert.That(activity.Mode, Is.EqualTo(ActivityMode.Active));
        }
    }
}
