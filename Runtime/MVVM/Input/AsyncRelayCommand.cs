// This file draws upon the concepts found in the AsyncRelayCommand implementation from the MVVM Toolkit library (CommunityToolkit/dotnet),
// more information in Third Party Notices.md

#nullable enable
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Unity.AppUI.MVVM
{
    /// <summary>
    /// Options for <see cref="AsyncRelayCommand"/>.
    /// </summary>
    [Flags]
    public enum AsyncRelayCommandOptions
    {
        /// <summary>
        /// No options.
        /// </summary>
        None,
        
        /// <summary>
        /// Allow concurrent executions.
        /// </summary>
        AllowConcurrentExecutions
    }
    
    /// <summary>
    /// A command that mirrors the functionality of <see cref="RelayCommand"/>, with the addition of
    /// accepting a <see cref="Func{TResult}"/> returning a <see cref="Task"/> as the execute
    /// action, and providing an <see cref="executionTask"/> property that notifies changes when
    /// <see cref="ExecuteAsync"/> is invoked and when the returned <see cref="Task"/> completes.
    /// </summary>
    public class AsyncRelayCommand : IAsyncRelayCommand
    {
        readonly Func<Task>? m_Execute;

        readonly Func<CancellationToken, Task>? m_CancellableExecute;

        readonly Func<bool>? m_CanExecute;

        readonly AsyncRelayCommandOptions m_Options;

        CancellationTokenSource? m_CancellableTokenSource;
        
        Task? m_ExecutionTask;

        /// <summary>
        /// The arguments for the <see cref="PropertyChanged"/> event raised by the <see cref="executionTask"/> property.
        /// </summary>
        public static readonly PropertyChangedEventArgs ExecutionTaskChangedEventArgs = new (nameof(executionTask));
        
        /// <summary>
        /// The arguments for the <see cref="PropertyChanged"/> event raised by the <see cref="isRunning"/> property.
        /// </summary>
        public static readonly PropertyChangedEventArgs IsRunningChangedEventArgs = new (nameof(isRunning));
        
        /// <summary>
        /// The arguments for the <see cref="PropertyChanged"/> event raised by the <see cref="canBeCancelled"/> property.
        /// </summary>
        public static readonly PropertyChangedEventArgs CanBeCanceledChangedEventArgs = new (nameof(canBeCancelled));
        
        /// <summary>
        /// The arguments for the <see cref="PropertyChanged"/> event raised by the <see cref="isCancellationRequested"/> property.
        /// </summary>
        public static readonly PropertyChangedEventArgs IsCancellationRequestedChangedEventArgs = new (nameof(isCancellationRequested));
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncRelayCommand"/> class.
        /// </summary>
        /// <param name="execute"> The <see cref="Func{TResult}"/> to invoke when <see cref="ExecuteAsync"/> is called.</param>
        /// <exception cref="ArgumentNullException"> If the execute argument is null.</exception>
        public AsyncRelayCommand(Func<Task> execute)
        {
            m_Execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncRelayCommand"/> class.
        /// </summary>
        /// <param name="execute"> The <see cref="Func{TResult}"/> to invoke when <see cref="ExecuteAsync"/> is called.</param>
        /// <param name="options"> The <see cref="AsyncRelayCommandOptions"/> to use.</param>
        /// <exception cref="ArgumentNullException"> If the execute argument is null.</exception>
        public AsyncRelayCommand(Func<Task> execute, AsyncRelayCommandOptions options)
        {
            m_Execute = execute ?? throw new ArgumentNullException(nameof(execute));
            m_Options = options;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncRelayCommand"/> class.
        /// </summary>
        /// <param name="cancellableExecute"> The <see cref="Func{TResult}"/> to invoke when <see cref="ExecuteAsync"/> is called.</param>
        /// <exception cref="ArgumentNullException"> If the execute argument is null.</exception>
        public AsyncRelayCommand(Func<CancellationToken, Task> cancellableExecute)
        {
            m_CancellableExecute = cancellableExecute ?? throw new ArgumentNullException(nameof(cancellableExecute));
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncRelayCommand"/> class.
        /// </summary>
        /// <param name="cancellableExecute"> The <see cref="Func{TResult}"/> to invoke when <see cref="ExecuteAsync"/> is called.</param>
        /// <param name="options"> The <see cref="AsyncRelayCommandOptions"/> to use.</param>
        /// <exception cref="ArgumentNullException"> If the execute argument is null.</exception>
        public AsyncRelayCommand(Func<CancellationToken, Task> cancellableExecute, AsyncRelayCommandOptions options)
        {
            m_CancellableExecute = cancellableExecute ?? throw new ArgumentNullException(nameof(cancellableExecute));
            m_Options = options;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncRelayCommand"/> class.
        /// </summary>
        /// <param name="execute"> The <see cref="Func{TResult}"/> to invoke when <see cref="ExecuteAsync"/> is called.</param>
        /// <param name="canExecute"> The <see cref="Func{TResult}"/> to invoke when <see cref="CanExecute()"/> is called.</param>
        /// <exception cref="ArgumentNullException"> If the execute argument is null.</exception>
        public AsyncRelayCommand(Func<Task> execute, Func<bool> canExecute)
        {
            m_Execute = execute ?? throw new ArgumentNullException(nameof(execute));
            m_CanExecute = canExecute ?? throw new ArgumentNullException(nameof(canExecute));
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncRelayCommand"/> class.
        /// </summary>
        /// <param name="execute"> The <see cref="Func{TResult}"/> to invoke when <see cref="ExecuteAsync"/> is called.</param>
        /// <param name="canExecute"> The <see cref="Func{TResult}"/> to invoke when <see cref="CanExecute()"/> is called.</param>
        /// <param name="options"> The <see cref="AsyncRelayCommandOptions"/> to use.</param>
        /// <exception cref="ArgumentNullException"> If the execute argument is null.</exception>
        public AsyncRelayCommand(Func<Task> execute, Func<bool> canExecute, AsyncRelayCommandOptions options)
        {
            m_Execute = execute ?? throw new ArgumentNullException(nameof(execute));
            m_CanExecute = canExecute ?? throw new ArgumentNullException(nameof(canExecute));
            m_Options = options;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncRelayCommand"/> class.
        /// </summary>
        /// <param name="cancellableExecute"> The <see cref="Func{TResult}"/> to invoke when <see cref="ExecuteAsync"/> is called.</param>
        /// <param name="canExecute"> The <see cref="Func{TResult}"/> to invoke when <see cref="CanExecute()"/> is called.</param>
        /// <exception cref="ArgumentNullException"> If the execute argument is null.</exception>
        public AsyncRelayCommand(Func<CancellationToken, Task> cancellableExecute, Func<bool> canExecute)
        {
            m_CancellableExecute = cancellableExecute ?? throw new ArgumentNullException(nameof(cancellableExecute));
            m_CanExecute = canExecute ?? throw new ArgumentNullException(nameof(canExecute));
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncRelayCommand"/> class.
        /// </summary>
        /// <param name="cancellableExecute"> The <see cref="Func{TResult}"/> to invoke when <see cref="ExecuteAsync"/> is called.</param>
        /// <param name="canExecute"> The <see cref="Func{TResult}"/> to invoke when <see cref="CanExecute()"/> is called.</param>
        /// <param name="options"> The <see cref="AsyncRelayCommandOptions"/> to use.</param>
        /// <exception cref="ArgumentNullException"> If the execute argument is null.</exception>
        public AsyncRelayCommand(Func<CancellationToken, Task> cancellableExecute, Func<bool> canExecute, AsyncRelayCommandOptions options)
        {
            m_CancellableExecute = cancellableExecute ?? throw new ArgumentNullException(nameof(cancellableExecute));
            m_CanExecute = canExecute ?? throw new ArgumentNullException(nameof(canExecute));
            m_Options = options;
        }

        /// <summary>
        /// Determines whether this <see cref="AsyncRelayCommand"/> can execute in its current state.
        /// </summary>
        /// <param name="parameter"> The parameter to use when determining whether this <see cref="AsyncRelayCommand"/> can execute.</param>
        /// <returns> <see langword="true"/> if this <see cref="AsyncRelayCommand"/> can execute; otherwise, <see langword="false"/>.</returns>
        public bool CanExecute(object? parameter)
        {
            var canExecute = m_CanExecute?.Invoke() != false;
            return canExecute && 
                ((m_Options & AsyncRelayCommandOptions.AllowConcurrentExecutions) != 0 || executionTask is not { IsCompleted: false });
        }
        
        /// <summary>
        /// Determines whether this <see cref="AsyncRelayCommand"/> can execute in its current state.
        /// </summary>
        /// <returns> <see langword="true"/> if this <see cref="AsyncRelayCommand"/> can execute; otherwise, <see langword="false"/>.</returns>
        public bool CanExecute() => CanExecute(null);

        /// <summary>
        /// Executes the <see cref="AsyncRelayCommand"/> on the current command target.
        /// </summary>
        /// <param name="parameter"> The parameter to use when executing the <see cref="AsyncRelayCommand"/>.</param>
        public void Execute(object? parameter)
        {
            ExecuteAsync(parameter);
        }
        
        /// <summary>
        /// Executes the <see cref="AsyncRelayCommand"/> on the current command target.
        /// </summary>
        public void Execute() => Execute(null);

        /// <summary>
        /// Event that is raised when changes occur that affect whether or not the <see cref="AsyncRelayCommand"/> should execute.
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event.
        /// </summary>
        public void NotifyCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Event that is raised when the <see cref="executionTask"/> property changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets the <see cref="Task"/> that represents the asynchronous operation.
        /// </summary>
        public Task? executionTask
        {
            get => m_ExecutionTask;
            set
            {
                if (ReferenceEquals(m_ExecutionTask, value))
                    return;

                m_ExecutionTask = value;
                PropertyChanged?.Invoke(this, AsyncRelayCommand.ExecutionTaskChangedEventArgs);
                PropertyChanged?.Invoke(this, AsyncRelayCommand.IsRunningChangedEventArgs);

                var isAlreadyCompletedOrNull = value?.IsCompleted ?? true;

                if (m_CancellableTokenSource is not null)
                {
                    PropertyChanged?.Invoke(this, AsyncRelayCommand.CanBeCanceledChangedEventArgs);
                    PropertyChanged?.Invoke(this, AsyncRelayCommand.IsCancellationRequestedChangedEventArgs);
                }

                if (isAlreadyCompletedOrNull)
                    return;

                static async void MonitorTask(AsyncRelayCommand command, Task task)
                {
                    await task;
                    if (ReferenceEquals(command.executionTask, task))
                    {
                        command.PropertyChanged?.Invoke(command, AsyncRelayCommand.ExecutionTaskChangedEventArgs);
                        command.PropertyChanged?.Invoke(command, AsyncRelayCommand.IsRunningChangedEventArgs);

                        if (command.m_CancellableTokenSource is not null)
                        {
                            command.PropertyChanged?.Invoke(command, AsyncRelayCommand.CanBeCanceledChangedEventArgs);
                        }

                        if ((command.m_Options & AsyncRelayCommandOptions.AllowConcurrentExecutions) == 0)
                        {
                            command.CanExecuteChanged?.Invoke(command, EventArgs.Empty);
                        }
                    }
                }
                
                MonitorTask(this, value!);
            }
        }

        /// <summary>
        /// Gets the <see cref="CancellationTokenSource"/> that can be used to cancel the asynchronous operation.
        /// </summary>
        public bool canBeCancelled => isRunning && m_CancellableTokenSource is {IsCancellationRequested: false};

        /// <summary>
        /// Gets the <see cref="CancellationTokenSource"/> that can be used to cancel the asynchronous operation.
        /// </summary>
        public bool isCancellationRequested => m_CancellableTokenSource is {IsCancellationRequested: true};

        /// <summary>
        /// Gets the <see cref="CancellationTokenSource"/> that can be used to cancel the asynchronous operation.
        /// </summary>
        public bool isRunning => executionTask is {IsCompleted: false};

        /// <summary>
        /// Executes the <see cref="AsyncRelayCommand"/> on the current command target.
        /// </summary>
        /// <param name="parameter"> The parameter to use when executing the <see cref="AsyncRelayCommand"/>.</param>
        /// <returns> The <see cref="Task"/> that represents the asynchronous operation.</returns>
        public Task ExecuteAsync(object? parameter)
        {
            Task task;

            if (m_Execute is not null)
            {
                // Non cancelable command delegate
                task = executionTask = m_Execute();
            }
            else
            {
                // Cancel the previous operation, if one is pending
                m_CancellableTokenSource?.Cancel();

                var cancellationTokenSource = m_CancellableTokenSource = new CancellationTokenSource();

                // Invoke the cancelable command delegate with a new linked token
                task = executionTask = m_CancellableExecute!(cancellationTokenSource.Token);
            }

            // If concurrent executions are disabled, notify the can execute change as well
            if ((m_Options & AsyncRelayCommandOptions.AllowConcurrentExecutions) == 0)
            {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }

            return task;
        }

        /// <summary>
        /// Cancels the <see cref="AsyncRelayCommand"/> on the current command target.
        /// </summary>
        public void Cancel()
        {
            if (m_CancellableTokenSource is { IsCancellationRequested: false } cancellationTokenSource)
            {
                cancellationTokenSource.Cancel();
                PropertyChanged?.Invoke(this, CanBeCanceledChangedEventArgs);
                PropertyChanged?.Invoke(this, IsCancellationRequestedChangedEventArgs);
            }
        }
    }

    /// <summary>
    /// A generic command that provides a more specific version of <see cref="AsyncRelayCommand"/>.
    /// </summary>
    /// <typeparam name="T">The type of parameter being passed as input to the callbacks.</typeparam>
    public class AsyncRelayCommand<T> : IAsyncRelayCommand<T>
    {
        readonly Predicate<T?>? m_CanExecute;

        readonly Func<T?, Task>? m_Execute;
        
        readonly Func<T?, CancellationToken, Task>? m_CancellableExecute;
        
        readonly AsyncRelayCommandOptions m_Options;

        CancellationTokenSource? m_CancellableTokenSource;

        Task? m_ExecutionTask;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncRelayCommand{T}"/> class.
        /// </summary>
        /// <param name="execute"> The <see cref="Func{T, Task}"/> to invoke when the command is executed.</param>
        /// <exception cref="ArgumentNullException"> <paramref name="execute"/> is null.</exception>
        public AsyncRelayCommand(Func<T?, Task> execute)
        {
            m_Execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncRelayCommand{T}"/> class.
        /// </summary>
        /// <param name="execute"> The <see cref="Func{T, Task}"/> to invoke when the command is executed.</param>
        /// <param name="options"> The <see cref="AsyncRelayCommandOptions"/> to use when executing the command.</param>
        /// <exception cref="ArgumentNullException"> <paramref name="execute"/> is null.</exception>
        public AsyncRelayCommand(Func<T?, Task> execute, AsyncRelayCommandOptions options)
        {
            m_Execute = execute ?? throw new ArgumentNullException(nameof(execute));
            m_Options = options;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncRelayCommand{T}"/> class.
        /// </summary>
        /// <param name="cancellableExecute"> The <see cref="Func{T, CancellationToken, Task}"/> to invoke when the command is executed.</param>
        /// <exception cref="ArgumentNullException"> <paramref name="cancellableExecute"/> is null.</exception>
        public AsyncRelayCommand(Func<T?, CancellationToken, Task> cancellableExecute)
        {
            m_CancellableExecute = cancellableExecute ?? throw new ArgumentNullException(nameof(cancellableExecute));
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncRelayCommand{T}"/> class.
        /// </summary>
        /// <param name="cancellableExecute"> The <see cref="Func{T, CancellationToken, Task}"/> to invoke when the command is executed.</param>
        /// <param name="options"> The <see cref="AsyncRelayCommandOptions"/> to use when executing the command.</param>
        /// <exception cref="ArgumentNullException"> <paramref name="cancellableExecute"/> is null.</exception>
        public AsyncRelayCommand(Func<T?, CancellationToken, Task> cancellableExecute, AsyncRelayCommandOptions options)
        {
            m_CancellableExecute = cancellableExecute ?? throw new ArgumentNullException(nameof(cancellableExecute));
            m_Options = options;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncRelayCommand{T}"/> class.
        /// </summary>
        /// <param name="execute"> The <see cref="Func{T, Task}"/> to invoke when the command is executed.</param>
        /// <param name="canExecute"> The <see cref="Predicate{T}"/> to invoke to determine whether the command can execute.</param>
        /// <exception cref="ArgumentNullException"> <paramref name="execute"/> or <paramref name="canExecute"/> is null.</exception>
        public AsyncRelayCommand(Func<T?, Task> execute, Predicate<T?> canExecute)
        {
            m_Execute = execute ?? throw new ArgumentNullException(nameof(execute));
            m_CanExecute = canExecute ?? throw new ArgumentNullException(nameof(canExecute));
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncRelayCommand{T}"/> class.
        /// </summary>
        /// <param name="execute"> The <see cref="Func{T, CancellationToken, Task}"/> to invoke when the command is executed.</param>
        /// <param name="canExecute"> The <see cref="Predicate{T}"/> to invoke to determine whether the command can execute.</param>
        /// <param name="options"> The <see cref="AsyncRelayCommandOptions"/> to use when executing the command.</param>
        /// <exception cref="ArgumentNullException"> <paramref name="execute"/> or <paramref name="canExecute"/> is null.</exception>
        public AsyncRelayCommand(Func<T?, Task> execute, Predicate<T?> canExecute, AsyncRelayCommandOptions options)
        {
            m_Execute = execute ?? throw new ArgumentNullException(nameof(execute));
            m_CanExecute = canExecute ?? throw new ArgumentNullException(nameof(canExecute));
            m_Options = options;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncRelayCommand{T}"/> class.
        /// </summary>
        /// <param name="cancellableExecute"> The <see cref="Func{T, CancellationToken, Task}"/> to invoke when the command is executed.</param>
        /// <param name="canExecute"> The <see cref="Predicate{T}"/> to invoke to determine whether the command can execute.</param>
        /// <exception cref="ArgumentNullException"> <paramref name="cancellableExecute"/> or <paramref name="canExecute"/> is null.</exception>
        public AsyncRelayCommand(Func<T?, CancellationToken, Task> cancellableExecute, Predicate<T?> canExecute)
        {
            m_CancellableExecute = cancellableExecute ?? throw new ArgumentNullException(nameof(cancellableExecute));
            m_CanExecute = canExecute ?? throw new ArgumentNullException(nameof(canExecute));
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncRelayCommand{T}"/> class.
        /// </summary>
        /// <param name="cancellableExecute"> The <see cref="Func{T, CancellationToken, Task}"/> to invoke when the command is executed.</param>
        /// <param name="canExecute"> The <see cref="Predicate{T}"/> to invoke to determine whether the command can execute.</param>
        /// <param name="options"> The <see cref="AsyncRelayCommandOptions"/> to use when executing the command.</param>
        /// <exception cref="ArgumentNullException"> <paramref name="cancellableExecute"/> or <paramref name="canExecute"/> is null.</exception>
        public AsyncRelayCommand(Func<T?, CancellationToken, Task> cancellableExecute, Predicate<T?> canExecute, AsyncRelayCommandOptions options)
        {
            m_CancellableExecute = cancellableExecute ?? throw new ArgumentNullException(nameof(cancellableExecute));
            m_CanExecute = canExecute ?? throw new ArgumentNullException(nameof(canExecute));
            m_Options = options;
        }

        /// <summary>
        /// Occurs when changes occur that affect whether the command should execute.
        /// </summary>
        /// <param name="parameter"> The parameter to use when determining whether the command can execute.</param>
        /// <returns> True if this command can be executed; otherwise, false.</returns>
        public Task ExecuteAsync(T? parameter)
        {
            Task task;

            if (m_Execute is not null)
            {
                task = executionTask = m_Execute(parameter);
            }
            else
            {
                m_CancellableTokenSource = new CancellationTokenSource();
                task = executionTask = m_CancellableExecute!(parameter, m_CancellableTokenSource.Token);
            }

            if ((m_Options & AsyncRelayCommandOptions.AllowConcurrentExecutions) == 0)
            {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }

            return task;
        }

        /// <summary>
        /// Determines whether this <see cref="AsyncRelayCommand{T}"/> can execute in its current state.
        /// </summary>
        /// <param name="parameter"> The parameter to use when determining whether the command can execute.</param>
        /// <returns> True if this command can be executed; otherwise, false.</returns>
        /// <exception cref="InvalidOperationException"> <see cref="AsyncRelayCommand{T}.ExecuteAsync(T)"/> has not been called.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanExecute(object? parameter)
        {
            if (parameter is null && default(T) is not null)
                return false;

            if (!RelayCommand<T>.TryGetCommandArg(parameter, out var result))
                throw new InvalidOperationException("");

            return CanExecute(result);
        }

        /// <summary>
        /// Determines whether this <see cref="AsyncRelayCommand{T}"/> can execute in its current state.
        /// </summary>
        /// <param name="parameter"> The parameter to use when determining whether the command can execute.</param>
        /// <exception cref="InvalidOperationException"> <see cref="AsyncRelayCommand{T}.ExecuteAsync(T)"/> has not been called.</exception>
        public void Execute(object parameter)
        {
            if (!RelayCommand<T>.TryGetCommandArg(parameter, out var result))
                throw new InvalidOperationException("");
            
            Execute(result);
        }

        /// <summary>
        /// Event raised when the <see cref="AsyncRelayCommand{T}.CanExecute(object)"/> property changes.
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Raises <see cref="AsyncRelayCommand{T}.CanExecuteChanged"/>.
        /// </summary>
        public void NotifyCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Event raised when the <see cref="AsyncRelayCommand{T}.executionTask"/> property changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets a value indicating whether this <see cref="AsyncRelayCommand{T}"/> is currently running.
        /// </summary>
        public Task? executionTask
        {
            get => m_ExecutionTask;
            set
            {
                if (ReferenceEquals(m_ExecutionTask, value))
                    return;

                m_ExecutionTask = value;
                PropertyChanged?.Invoke(this, AsyncRelayCommand.ExecutionTaskChangedEventArgs);
                PropertyChanged?.Invoke(this, AsyncRelayCommand.IsRunningChangedEventArgs);

                var isAlreadyCompletedOrNull = value?.IsCompleted ?? true;

                if (m_CancellableTokenSource is not null)
                {
                    PropertyChanged?.Invoke(this, AsyncRelayCommand.CanBeCanceledChangedEventArgs);
                    PropertyChanged?.Invoke(this, AsyncRelayCommand.IsCancellationRequestedChangedEventArgs);
                }

                if (isAlreadyCompletedOrNull)
                    return;

                static async void MonitorTask(AsyncRelayCommand<T> command, Task task)
                {
                    await task;
                    if (ReferenceEquals(command.executionTask, task))
                    {
                        command.PropertyChanged?.Invoke(command, AsyncRelayCommand.ExecutionTaskChangedEventArgs);
                        command.PropertyChanged?.Invoke(command, AsyncRelayCommand.IsRunningChangedEventArgs);

                        if (command.m_CancellableTokenSource is not null)
                        {
                            command.PropertyChanged?.Invoke(command, AsyncRelayCommand.CanBeCanceledChangedEventArgs);
                        }

                        if ((command.m_Options & AsyncRelayCommandOptions.AllowConcurrentExecutions) == 0)
                        {
                            command.CanExecuteChanged?.Invoke(command, EventArgs.Empty);
                        }
                    }
                }
                
                MonitorTask(this, value!);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="AsyncRelayCommand{T}"/> can be cancelled.
        /// </summary>
        public bool canBeCancelled => isRunning && m_CancellableTokenSource is { IsCancellationRequested: false };

        /// <summary>
        /// Gets a value indicating whether this <see cref="AsyncRelayCommand{T}"/> has been cancelled.
        /// </summary>
        public bool isCancellationRequested => m_CancellableTokenSource is {IsCancellationRequested: true};

        /// <summary>
        /// Gets a value indicating whether this <see cref="AsyncRelayCommand{T}"/> is currently running.
        /// </summary>
        public bool isRunning => executionTask is {IsCompleted: false};

        /// <summary>
        /// Gets a value indicating whether this <see cref="AsyncRelayCommand{T}"/> can be executed.
        /// </summary>
        /// <param name="parameter"> The parameter to use when determining whether the command can execute.</param>
        /// <returns> True if this command can be executed; otherwise, false.</returns>
        /// <exception cref="InvalidOperationException"> <see cref="AsyncRelayCommand{T}.ExecuteAsync(T)"/> has not been called.</exception>
        public Task ExecuteAsync(object? parameter)
        {
            if (!RelayCommand<T>.TryGetCommandArg(parameter, out var result))
                throw new InvalidOperationException("");

            return ExecuteAsync(result);
        }

        /// <summary>
        /// Cancels the current execution of this <see cref="AsyncRelayCommand{T}"/>.
        /// </summary>
        public void Cancel()
        {
            if (m_CancellableTokenSource is {IsCancellationRequested: false} tokenSource)
            {
                tokenSource.Cancel();
                PropertyChanged?.Invoke(this, AsyncRelayCommand.CanBeCanceledChangedEventArgs);
                PropertyChanged?.Invoke(this, AsyncRelayCommand.IsCancellationRequestedChangedEventArgs);
            }
        }

        /// <summary>
        /// Executes the <see cref="AsyncRelayCommand{T}"/> synchronously on the current thread.
        /// </summary>
        /// <param name="parameter"> The parameter to use when executing the command.</param>
        public void Execute(T? parameter)
        {
            ExecuteAsync(parameter);
        }

        /// <summary>
        /// Determines whether this <see cref="AsyncRelayCommand{T}"/> can execute in its current state.
        /// </summary>
        /// <param name="parameter"> The parameter to use when determining whether the command can execute.</param>
        /// <returns> True if this command can be executed; otherwise, false.</returns>
        public bool CanExecute(T? parameter)
        {
            var canExecute = m_CanExecute?.Invoke(parameter) != false;
            return canExecute && ((m_Options & AsyncRelayCommandOptions.AllowConcurrentExecutions) != 0 ||
                executionTask is not {IsCompleted: false});
        }
    }
}
