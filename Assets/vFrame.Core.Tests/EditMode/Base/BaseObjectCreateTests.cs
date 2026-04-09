using System;
using NUnit.Framework;
using vFrame.Core;

namespace vFrame.Core.Tests.EditMode.Base
{
    public class BaseObjectCreateTests
    {
        [Test]
        public void Create_SetsCreatedState_WhenOnCreateSucceeds() {
            var sut = new SuccessfulBaseObject();

            Assert.That(sut.Created, Is.False);
            Assert.That(sut.Destroyed, Is.False);
            Assert.Throws<BaseObjectNotCreatedException>(() => sut.AssertCreated());

            sut.Create();

            Assert.That(sut.OnCreateCallCount, Is.EqualTo(1));
            Assert.That(sut.Created, Is.True);
            Assert.That(sut.Destroyed, Is.False);
            Assert.DoesNotThrow(() => sut.AssertCreated());
        }

        [Test]
        public void Create_KeepsUncreatedState_WhenOnCreateThrows() {
            var sut = new ThrowingBaseObject();

            var exception = Assert.Throws<InvalidOperationException>(() => sut.Create());

            Assert.That(exception.Message, Is.EqualTo(ThrowingBaseObject.ErrorMessage));
            Assert.That(sut.OnCreateCallCount, Is.EqualTo(1));
            Assert.That(sut.Created, Is.False);
            Assert.That(sut.Destroyed, Is.False);
            Assert.Throws<BaseObjectNotCreatedException>(() => sut.AssertCreated());
        }

        [Test]
        public void GenericCreate_SetsCreatedStateAndForwardsArgument_WhenOnCreateSucceeds() {
            var sut = new SuccessfulGenericBaseObject();

            Assert.That(sut.Created, Is.False);
            Assert.That(sut.Destroyed, Is.False);
            Assert.Throws<BaseObjectNotCreatedException>(() => sut.AssertCreated());

            sut.Create(123);

            Assert.That(sut.OnCreateCallCount, Is.EqualTo(1));
            Assert.That(sut.LastValue, Is.EqualTo(123));
            Assert.That(sut.Created, Is.True);
            Assert.That(sut.Destroyed, Is.False);
            Assert.DoesNotThrow(() => sut.AssertCreated());
        }

        [Test]
        public void GenericCreate_KeepsUncreatedState_WhenOnCreateThrows() {
            var sut = new ThrowingGenericBaseObject();

            var exception = Assert.Throws<InvalidOperationException>(() => sut.Create(123));

            Assert.That(exception.Message, Is.EqualTo(ThrowingGenericBaseObject.ErrorMessage));
            Assert.That(sut.OnCreateCallCount, Is.EqualTo(1));
            Assert.That(sut.ReceivedValue, Is.EqualTo(123));
            Assert.That(sut.Created, Is.False);
            Assert.That(sut.Destroyed, Is.False);
            Assert.Throws<BaseObjectNotCreatedException>(() => sut.AssertCreated());
        }

        private sealed class SuccessfulBaseObject : BaseObject
        {
            public int OnCreateCallCount { get; private set; }

            public void AssertCreated() {
                ThrowIfNotCreated();
            }

            protected override void OnCreate() {
                OnCreateCallCount++;
            }

            protected override void OnDestroy() { }
        }

        private sealed class ThrowingBaseObject : BaseObject
        {
            public const string ErrorMessage = "OnCreate failed.";

            public int OnCreateCallCount { get; private set; }

            public void AssertCreated() {
                ThrowIfNotCreated();
            }

            protected override void OnCreate() {
                OnCreateCallCount++;
                throw new InvalidOperationException(ErrorMessage);
            }

            protected override void OnDestroy() { }
        }

        private sealed class SuccessfulGenericBaseObject : BaseObject<int>
        {
            public int OnCreateCallCount { get; private set; }

            public int LastValue { get; private set; }

            public void AssertCreated() {
                ThrowIfNotCreated();
            }

            protected override void OnCreate(int arg1) {
                OnCreateCallCount++;
                LastValue = arg1;
            }

            protected override void OnDestroy() { }
        }

        private sealed class ThrowingGenericBaseObject : BaseObject<int>
        {
            public const string ErrorMessage = "Generic OnCreate failed.";

            public int OnCreateCallCount { get; private set; }

            public int ReceivedValue { get; private set; }

            public void AssertCreated() {
                ThrowIfNotCreated();
            }

            protected override void OnCreate(int arg1) {
                OnCreateCallCount++;
                ReceivedValue = arg1;
                throw new InvalidOperationException(ErrorMessage);
            }

            protected override void OnDestroy() { }
        }
    }
}
