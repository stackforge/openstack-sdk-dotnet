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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Provides extra functions to be used in place of NetFx 4.5 task standard static methods.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix",
        Justification = "This naming is by convention for this usage as set by other standard async targeting packages. [tgs]")]
    public static class TaskEx
    {
        /// <summary>
        /// Returns a task that has been completed.
        /// </summary>
        /// <returns>
        /// A completed task.
        /// </returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Justification = "In this case the pattern is correct.  [TGS]")]
        public static Task GetCompletedTask()
        {
            Task retval = null;
            try
            {
                retval = new Task(() => { });
            }
            catch (Exception)
            {
                if (!ReferenceEquals(retval, null))
                {
                    retval.Dispose();
                }
                throw;
            }
            retval.Start();
            retval.Wait();
            return retval;
        }

        /// <summary>
        /// Creates a task that runs the specified action.
        /// </summary>
        /// <param name="action">The action to execute asynchronously.</param>
        /// <returns>
        /// A task that represents the completion of the action.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="action"/> argument is null.</exception>
        [DebuggerNonUserCode]
        public static Task Run(Action action)
        {
            return TaskEx.Run(action, CancellationToken.None);
        }

        /// <summary>
        /// Creates a task that runs the specified action.
        /// </summary>
        /// <param name="action">The action to execute.</param><param name="cancellationToken">The CancellationToken to use to request cancellation of this task.</param>
        /// <returns>
        /// A task that represents the completion of the action.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="action"/> argument is null.</exception>
        [DebuggerNonUserCode]
        public static Task Run(Action action, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(action, cancellationToken, TaskCreationOptions.None, TaskScheduler.Default);
        }

        /// <summary>
        /// Creates a task that runs the specified function.
        /// </summary>
        /// <param name="function">The function to execute asynchronously.</param>
        /// <typeparam name="TResult">
        /// The result type of the function.
        /// </typeparam>
        /// <returns>
        /// A task that represents the completion of the action.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="function"/> argument is null.</exception>
        [DebuggerNonUserCode]
        public static Task<TResult> Run<TResult>(Func<TResult> function)
        {
            return TaskEx.Run<TResult>(function, CancellationToken.None);
        }

        /// <summary>
        /// Creates a task that runs the specified function.
        /// </summary>
        /// <param name="function">The action to execute.</param><param name="cancellationToken">The CancellationToken to use to cancel the task.</param>
        /// <typeparam name="TResult">
        /// The result type of the function.
        /// </typeparam>
        /// <returns>
        /// A task that represents the completion of the action.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="function"/> argument is null.</exception>
        [DebuggerNonUserCode]
        public static Task<TResult> Run<TResult>(Func<TResult> function, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew<TResult>(function, cancellationToken, TaskCreationOptions.None, TaskScheduler.Default);
        }

        /// <summary>
        /// Creates a task that runs the specified function.
        /// </summary>
        /// <param name="function">The action to execute asynchronously.</param>
        /// <returns>
        /// A task that represents the completion of the action.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="function"/> argument is null.</exception>
        [DebuggerNonUserCode]
        public static Task Run(Func<Task> function)
        {
            return TaskEx.Run(function, CancellationToken.None);
        }

        /// <summary>
        /// Creates a task that runs the specified function.
        /// </summary>
        /// <param name="function">The function to execute.</param><param name="cancellationToken">The CancellationToken to use to request cancellation of this task.</param>
        /// <returns>
        /// A task that represents the completion of the function.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="function"/> argument is null.</exception>
        [DebuggerNonUserCode]
        public static Task Run(Func<Task> function, CancellationToken cancellationToken)
        {
            return TaskExtensions.Unwrap(TaskEx.Run<Task>(function, cancellationToken));
        }

        /// <summary>
        /// Creates a task that runs the specified function.
        /// </summary>
        /// <param name="function">The function to execute asynchronously.</param>
        /// <typeparam name="TResult">
        /// The result type of the function.
        /// </typeparam>
        /// <returns>
        /// A task that represents the completion of the action.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="function"/> argument is null.</exception>
        [DebuggerNonUserCode]
        public static Task<TResult> Run<TResult>(Func<Task<TResult>> function)
        {
            return TaskEx.Run<TResult>(function, CancellationToken.None);
        }

        /// <summary>
        /// Creates a task that runs the specified function.
        /// </summary>
        /// <param name="function">The action to execute.</param><param name="cancellationToken">The CancellationToken to use to cancel the task.</param>
        /// <typeparam name="TResult">
        /// The result type of the function.
        /// </typeparam>
        /// <returns>
        /// A task that represents the completion of the action.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="function"/> argument is null.</exception>
        [DebuggerNonUserCode]
        public static Task<TResult> Run<TResult>(Func<Task<TResult>> function, CancellationToken cancellationToken)
        {
            return TaskExtensions.Unwrap<TResult>(TaskEx.Run<Task<TResult>>(function, cancellationToken));
        }
    }
}
