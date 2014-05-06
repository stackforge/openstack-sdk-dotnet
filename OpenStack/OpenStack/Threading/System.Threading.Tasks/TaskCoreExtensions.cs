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
    using System.IO;
    using System.Diagnostics;

    /// <summary>
    /// Provides core extension methods for task.
    /// </summary>
    [DebuggerNonUserCode]
    public static class TaskCoreExtensions
    {
        /// <summary>
        /// Reads all characters from the current position to the end of the TextReader
        ///             and returns them as one string asynchronously.
        /// </summary>
        /// <param name="source">The source reader.</param>
        /// <returns>
        /// A Task that represents the asynchronous operation.
        /// </returns>
        [DebuggerNonUserCode]
        public static Task<string> ReadToEndAsync(this TextReader source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            else
            {
                return Task.Factory.StartNew<string>((Func<string>)(() => source.ReadToEnd()), CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
            }
        }

        /// <summary>
        /// Gets an awaiter used to await this Task.
        /// </summary>
        /// <param name="task">
        /// The task object being extended.
        /// </param>
        /// <returns>
        /// An awaiter instance.
        /// </returns>
        [DebuggerNonUserCode]
        public static TaskAwaiter GetAwaiter(this Task task)
        {
            return new TaskAwaiter(task);
        }

        /// <summary>
        /// Gets an awaiter used to await this Task&lt;TResult%gt;.
        /// </summary>
        /// <typeparam name="T">
        /// The return type for the task.
        /// </typeparam>
        /// <param name="task">
        /// The task object being extended.
        /// </param>
        /// <returns>
        /// An awaiter instance.
        /// </returns>
        [DebuggerNonUserCode]
        public static TaskAwaiter<T> GetAwaiter<T>(this Task<T> task)
        {
            return new TaskAwaiter<T>(task);
        }

        /// <summary>
        ///  Schedules the continuation onto the <see cref="Task" /> associated with this <see cref="TaskAwaiter" /> .
        /// </summary>
        /// <param name="continuation"> The action to invoke when the await operation completes. </param>
        [DebuggerNonUserCode]
        internal static void CompletedInternal(Action continuation)
        {
            var scheduler = TaskScheduler.Current;
            Task.Factory.StartNew(state => ((Action)state)(),
                                  continuation,
                                  CancellationToken.None,
                                  TaskCreationOptions.None,
                                  scheduler);
        }
    }
}