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
namespace System.Threading.Tasks
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Provides an object that waits for the completion of an asynchronous task.
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes",
        Justification = "Reference equality is correct in this case. [tgs]")]
    [DebuggerNonUserCode]
    public struct TaskAwaiter : INotifyCompletion
    {
        private readonly Task task;

        /// <summary>
        /// Initializes a new instance of the TaskAwaiter structure.
        /// </summary>
        /// <param name="task">
        /// The task.
        /// </param>
        [DebuggerNonUserCode]
        public TaskAwaiter(Task task)
        {
            this.task = task;
        }

        /// <summary>
        /// Gets a task scheduler.
        /// </summary>
        [DebuggerNonUserCode]
        public static TaskScheduler TaskScheduler
        {
            get
            {
                return TaskSchedularHelper.TaskScheduler;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the asynchronous task has completed.
        /// </summary>
        [DebuggerNonUserCode]
        public bool IsCompleted
        {
            get { return this.task.IsCompleted; }
        }

        /// <summary>
        /// Sets the action to perform when the TaskAwaiter object stops waiting for the asynchronous task to complete.
        /// </summary>
        /// <param name="continuation">
        /// The action to perform when the wait operation completes.
        /// </param>
        [DebuggerNonUserCode]
        public void OnCompleted(Action continuation)
        {
            TaskCoreExtensions.CompletedInternal(continuation);
        }

        /// <summary>
        /// Ends the wait for the completion of the asynchronous task.
        /// </summary>
        [DebuggerNonUserCode]
        public void GetResult()
        {
            try
            {
                this.task.Wait();
            }
            catch (AggregateException ex)
            {
                throw ex.InnerExceptions[0];
            }
        }
    }
}