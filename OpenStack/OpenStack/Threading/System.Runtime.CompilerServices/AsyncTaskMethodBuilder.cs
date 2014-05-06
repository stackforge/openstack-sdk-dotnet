// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not
// use this file except in compliance with the License.  You may obtain a copy
// of the License at http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY IMPLIED
// WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE,
// MERCHANTABLITY OR NON-INFRINGEMENT.
// 
// See the Apache Version 2.0 License for specific language governing
// permissions and limitations under the License.
namespace System.Runtime.CompilerServices
{
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a builder for asynchronous methods that return a task.
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes",
        Justification = "Reference equality is correct in this case. [tgs]")]
    [DebuggerNonUserCode]
    public struct AsyncTaskMethodBuilder
    {
        private TaskCompletionSource<object> tcs;

        /// <summary>
        /// Gets the task for this builder.
        /// </summary>
        public Task Task
        {
            get
            {
                return this.tcs.Task;
            }
        }

        /// <summary>
        /// Creates an instance of the AsyncTaskMethodBuilder class.
        /// </summary>
        /// <returns>
        /// A new instance of the builder.
        /// </returns>
        [DebuggerNonUserCode]
        public static AsyncTaskMethodBuilder Create()
        {
            AsyncTaskMethodBuilder b;
            b.tcs = new TaskCompletionSource<object>();
            return b;
        }

        /// <summary>
        /// Begins running the builder with the associated state machine.
        /// </summary>
        /// <typeparam name="TStateMachine">
        /// The type of the state machine.
        /// </typeparam>
        /// <param name="stateMachine">
        /// The state machine instance, passed by reference.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", Justification = "Per design of the framework. [tgs]")]
        [DebuggerNonUserCode]
        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            stateMachine.MoveNext();
        }

        /// <summary>
        /// Associates the builder with the specified state machine.
        /// </summary>
        /// <param name="stateMachine">
        /// The state machine instance to associate with the builder.
        /// </param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "stateMachine",
            Justification = "Required to maintain the expected pattern. [tgs].")]
        [DebuggerNonUserCode]
        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            // Method is not implemented as it is not needed for our purpose.
            throw new NotImplementedException();
        }

        /// <summary>
        /// Schedules the state machine to proceed to the next action when the specified awaiter completes.
        /// </summary>
        /// <typeparam name="TAwaiter">
        /// The type of the awaiter.
        /// </typeparam>
        /// <typeparam name="TStateMachine">
        /// The type of the state machine.
        /// </typeparam>
        /// <param name="awaiter">
        /// The awaiter.
        /// </param>
        /// <param name="stateMachine">
        /// The state machine.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", Justification = "Per design of the framework. [tgs]")]
        [DebuggerNonUserCode]
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            awaiter.OnCompleted(stateMachine.MoveNext);
        }

        /// <summary>
        /// Schedules the state machine to proceed to the next action when the specified awaiter completes. 
        /// This method can be called from partially trusted code.
        /// <para>
        /// NOTE: As this assembly does not support APTC, this method is simply a call to AwaitOnCompleted.
        /// </para>
        /// </summary>
        /// <typeparam name="TAwaiter">
        /// The type of the awaiter.
        /// </typeparam>
        /// <typeparam name="TStateMachine">
        /// The type of the state machine.
        /// </typeparam>
        /// <param name="awaiter">
        /// The awaiter.
        /// </param>
        /// <param name="stateMachine">
        /// The state machine.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", Justification = "Per design of the framework. [tgs]")]
        [DebuggerNonUserCode]
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            awaiter.OnCompleted(stateMachine.MoveNext);
        }

        /// <summary>
        /// Marks the task as successfully completed.
        /// </summary>
        [DebuggerNonUserCode]
        public void SetResult()
        {
            this.tcs.SetResult(null);
        }

        /// <summary>
        /// Marks the task as failed and binds the specified exception to the task.
        /// </summary>
        /// <param name="exception">
        /// The exception to bind to the task.
        /// </param>
        [DebuggerNonUserCode]
        public void SetException(Exception exception)
        {
            this.tcs.SetException(exception);
        }
    }
}