using System.Collections;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Unity.AppUI.MVVM;

#pragma warning disable 1998

namespace Unity.AppUI.Tests.MVVM
{
    [TestFixture]
    [TestOf(typeof(AsyncRelayCommand))]
    class AsyncRelayCommandTests
    {
        [Test]
        public void CanExecute()
        {
            var command = new AsyncRelayCommand(async () => { }, () => true);
            Assert.IsTrue(command.CanExecute());

            command = new AsyncRelayCommand(async () => { });
            Assert.IsTrue(command.CanExecute());

            var canExecute = false;
            var executed = 0;

            command = new AsyncRelayCommand(async () =>
            {
                executed++;
            }, () => canExecute);
            Assert.IsFalse(command.CanExecute());
            Assert.AreEqual(0, executed);
            command.Execute();
            Assert.AreEqual(0, executed);

            var changed = 0;
            command.CanExecuteChanged += (sender, args) =>
            {
                changed++;
            };

            canExecute = true;
            command.NotifyCanExecuteChanged();

            Assert.AreEqual(1, changed);
            Assert.IsTrue(command.CanExecute());

            command.Execute();
            Assert.AreEqual(1, executed);
        }

        [Test]
        public void CanExecuteWithType()
        {
            var command = new AsyncRelayCommand<int>(async i => { }, i => true);
            Assert.IsTrue(command.CanExecute(1));

            command = new AsyncRelayCommand<int>(async i => { });
            Assert.IsTrue(command.CanExecute(1));

            var canExecute = false;

            var executed = 0;
            command = new AsyncRelayCommand<int>(async i =>
            {
                executed++;
            }, i => canExecute);
            Assert.IsFalse(command.CanExecute(1));
            Assert.AreEqual(0, executed);
            command.Execute(1);
            Assert.AreEqual(0, executed);

            var changed = 0;
            command.CanExecuteChanged += (sender, args) =>
            {
                changed++;
            };

            canExecute = true;
            command.NotifyCanExecuteChanged();

            Assert.AreEqual(1, changed);
            Assert.IsTrue(command.CanExecute((object)1));

            command.Execute((object)1);
            Assert.AreEqual(1, executed);
        }

        [Test]
        public void GenericAllowConcurrentExecutions_CancelsPreviousRun()
        {
            CancellationToken first = default, second = default;
            var count = 0;
            var command = new AsyncRelayCommand<string>((s, token) =>
            {
                if (count++ == 0) first = token; else second = token;
                return Task.Delay(-1, token);
            }, AsyncRelayCommandOptions.AllowConcurrentExecutions);

            _ = command.ExecuteAsync("a");
            _ = command.ExecuteAsync("b");

            Assert.IsTrue(first.IsCancellationRequested);
            Assert.IsFalse(second.IsCancellationRequested);

            command.Cancel();
            Assert.IsTrue(second.IsCancellationRequested);
        }

        [UnityTest]
        public IEnumerator CancelledRun_RaisesCompletionNotifications()
        {
            var command = new AsyncRelayCommand(token => Task.Delay(-1, token));
            var canExecuteChanged = 0;
            var isRunningChanged = 0;
            command.CanExecuteChanged += (_, _) => canExecuteChanged++;
            command.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(command.isRunning)) isRunningChanged++; };

            _ = command.ExecuteAsync(null);
            Assert.AreEqual(1, canExecuteChanged);
            command.Cancel();

            for (var i = 0; i < 10; i++)
                yield return null;

            Assert.AreEqual(2, canExecuteChanged);
            Assert.AreEqual(2, isRunningChanged);
            Assert.IsFalse(command.isRunning);
            Assert.IsTrue(command.CanExecute());
        }

        [UnityTest]
        public IEnumerator GenericCancelledRun_RaisesCompletionNotifications()
        {
            var command = new AsyncRelayCommand<string>((s, token) => Task.Delay(-1, token));
            var canExecuteChanged = 0;
            command.CanExecuteChanged += (_, _) => canExecuteChanged++;

            _ = command.ExecuteAsync("a");
            command.Cancel();

            for (var i = 0; i < 10; i++)
                yield return null;

            Assert.AreEqual(2, canExecuteChanged);
            Assert.IsTrue(command.CanExecute("a"));
        }

        [UnityTest]
        public IEnumerator FaultedRunWithOperationCanceledException_IsStillLogged()
        {
            // A cancellation exception that did not come from cancelling this run, such as a timeout.
            var source = new TaskCompletionSource<bool>();
            var command = new AsyncRelayCommand(() => source.Task);
            var canExecuteChanged = 0;
            command.CanExecuteChanged += (_, _) => canExecuteChanged++;

            _ = command.ExecuteAsync(null);
            LogAssert.Expect(LogType.Exception, new Regex("OperationCanceledException.*user timeout"));
            source.SetException(new System.OperationCanceledException("user timeout"));

            for (var i = 0; i < 10; i++)
                yield return null;

            Assert.AreEqual(2, canExecuteChanged);
            Assert.IsFalse(command.isRunning);
        }

        [UnityTest]
        public IEnumerator GenericFaultedRunWithOperationCanceledException_IsStillLogged()
        {
            var source = new TaskCompletionSource<bool>();
            var command = new AsyncRelayCommand<string>(s => source.Task);
            var canExecuteChanged = 0;
            command.CanExecuteChanged += (_, _) => canExecuteChanged++;

            _ = command.ExecuteAsync("a");
            LogAssert.Expect(LogType.Exception, new Regex("OperationCanceledException.*user timeout"));
            source.SetException(new System.OperationCanceledException("user timeout"));

            for (var i = 0; i < 10; i++)
                yield return null;

            Assert.AreEqual(2, canExecuteChanged);
            Assert.IsFalse(command.isRunning);
        }
    }
}

#pragma warning restore 1998
