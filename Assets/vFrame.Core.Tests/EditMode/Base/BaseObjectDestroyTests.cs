using System;
using NUnit.Framework;
using vFrame.Core;

namespace vFrame.Core.Tests.EditMode.Base
{
    public class BaseObjectDestroyTests
    {
        [Test]
        public void Destroy_ClearsCreatedStateAndMarksDestroyed_WhenObjectWasCreated() {
            var sut = new TrackingBaseObject();

            sut.Create();
            Assert.DoesNotThrow(() => sut.AssertNotDestroyed());

            sut.Destroy();

            Assert.That(sut.OnDestroyCallCount, Is.EqualTo(1));
            Assert.That(sut.Created, Is.False);
            Assert.That(sut.Destroyed, Is.True);
            Assert.Throws<BaseObjectDestroyedException>(() => sut.AssertNotDestroyed());
        }

        [Test]
        public void Destroy_CallsOnDestroyOnlyOnce_WhenInvokedMultipleTimes() {
            var sut = new TrackingBaseObject();

            sut.Create();

            sut.Destroy();
            sut.Destroy();

            Assert.That(sut.OnDestroyCallCount, Is.EqualTo(1));
            Assert.That(sut.Created, Is.False);
            Assert.That(sut.Destroyed, Is.True);
        }

        [Test]
        public void Destroy_TerminatesOwnedChildState_WhenOwnerIsDestroyed() {
            var child = new TrackingBaseObject();
            var sut = new OwningBaseObject(child);

            child.Create();
            sut.Create();

            sut.Destroy();

            Assert.That(child.OnDestroyCallCount, Is.EqualTo(1));
            Assert.That(child.Created, Is.False);
            Assert.That(child.Destroyed, Is.True);
        }

        [Test]
        public void Destroy_TerminatesGroupedOwnedState_ThroughUnifiedCleanupBoundary() {
            var first = new TrackingBaseObject();
            var second = new TrackingBaseObject();
            var sut = new GroupOwningBaseObject();

            first.Create();
            second.Create();
            sut.Create();
            sut.Add(first);
            sut.Add(second);

            sut.Destroy();

            Assert.That(first.OnDestroyCallCount, Is.EqualTo(1));
            Assert.That(second.OnDestroyCallCount, Is.EqualTo(1));
            Assert.That(first.Destroyed, Is.True);
            Assert.That(second.Destroyed, Is.True);
        }

        [Test]
        public void Destroy_RemainsTerminal_AndDoesNotAllowRecreate() {
            var sut = new TrackingBaseObject();

            sut.Create();
            sut.Destroy();

            Assert.Throws<BaseObjectDestroyedException>(() => sut.Create());
            Assert.That(sut.OnCreateCallCount, Is.EqualTo(1));
            Assert.That(sut.OnDestroyCallCount, Is.EqualTo(1));
            Assert.That(sut.Created, Is.False);
            Assert.That(sut.Destroyed, Is.True);
        }

        [Test]
        public void Destroy_KeepsDestroyedState_WhenOwnedCleanupThrows() {
            var owned = new ThrowingDestroyable();
            var sut = new OwningBaseObject(owned);

            sut.Create();

            var exception = Assert.Throws<InvalidOperationException>(() => sut.Destroy());

            Assert.That(exception.Message, Is.EqualTo(ThrowingDestroyable.ErrorMessage));
            Assert.That(sut.Created, Is.False);
            Assert.That(sut.Destroyed, Is.True);
        }

        [Test]
        public void Destroy_TerminatesChildLifetimeThroughGroupedCleanupBoundary() {
            var child = new TrackingDestroyable();
            var sut = new ChildLifetimeOwningBaseObject();

            sut.Create();
            sut.AddToChild(child);

            sut.Destroy();

            Assert.That(child.DestroyCallCount, Is.EqualTo(1));
            Assert.That(child.Destroyed, Is.True);
        }

        [Test]
        public void Lifetime_Destroy_ExecutesSameLifetimeActionsInReverseOrder() {
            var lifetime = new Lifetime();
            var calls = new System.Collections.Generic.List<string>();

            lifetime.Add(() => calls.Add("first"));
            lifetime.Add(() => calls.Add("second"));

            lifetime.Destroy();

            Assert.That(calls, Is.EqualTo(new[] { "second", "first" }));
        }

        [Test]
        public void Lifetime_Destroy_TerminatesSameLifetimeOwnedDestroyables() {
            var lifetime = new Lifetime();
            var destroyable = new TrackingDestroyable();

            lifetime.Add(destroyable);

            lifetime.Destroy();

            Assert.That(destroyable.DestroyCallCount, Is.EqualTo(1));
            Assert.That(destroyable.Destroyed, Is.True);
        }

        private sealed class TrackingBaseObject : BaseObject
        {
            public int OnCreateCallCount { get; private set; }

            public int OnDestroyCallCount { get; private set; }

            public void AssertNotDestroyed() {
                ThrowIfDestroyed();
            }

            protected override void OnCreate() {
                OnCreateCallCount++;
            }

            protected override void OnDestroy() {
                OnDestroyCallCount++;
            }
        }

        private sealed class OwningBaseObject : BaseObject
        {
            private readonly IDestroyable _owned;

            public OwningBaseObject(IDestroyable owned) {
                _owned = owned;
            }

            protected override void OnCreate() {
                Own(_owned);
            }

            protected override void OnDestroy() { }
        }

        private sealed class GroupOwningBaseObject : BaseObject
        {
            private ILifetime _group;

            public void Add(IDestroyable destroyable) {
                _group.Add(destroyable);
            }

            protected override void OnCreate() {
                _group = Lifetime.CreateChild();
            }

            protected override void OnDestroy() { }
        }

        private sealed class ChildLifetimeOwningBaseObject : BaseObject
        {
            private ILifetime _child;

            public void AddToChild(IDestroyable destroyable) {
                _child.Add(destroyable);
            }

            protected override void OnCreate() {
                _child = Lifetime.CreateChild();
            }

            protected override void OnDestroy() { }
        }

        private sealed class ThrowingDestroyable : IDestroyable
        {
            public const string ErrorMessage = "Owned cleanup failed.";

            public bool Destroyed { get; private set; }

            public void Destroy() {
                Destroyed = true;
                throw new InvalidOperationException(ErrorMessage);
            }

            public void Dispose() {
                Destroy();
            }
        }

        private sealed class TrackingDestroyable : IDestroyable
        {
            public bool Destroyed { get; private set; }

            public int DestroyCallCount { get; private set; }

            public void Destroy() {
                DestroyCallCount++;
                Destroyed = true;
            }

            public void Dispose() {
                Destroy();
            }
        }
    }
}
