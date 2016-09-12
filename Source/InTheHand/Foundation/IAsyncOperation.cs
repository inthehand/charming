// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IAsyncOperation.cs" company="In The Hand Ltd">
//   Copyright (c) 2016 In The Hand Ltd, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Windows.Foundation
{
    /// <summary>
    /// Represents an asynchronous operation, which returns a result upon completion. 
    /// This is the return type for many Windows Runtime asynchronous methods that have results but don't report progress.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public interface IAsyncOperation<TResult>
    {
        /// <summary>
        /// Returns the results of the operation. 
        /// </summary>
        TResult GetResults();

        /// <summary>
        /// Gets or sets the method that handles the operation completed notification.
        /// </summary>
        AsyncOperationCompletedHandler<TResult> Completed { get; set; }
    }

    /// <summary>
    /// Represents a method that handles the completed event of an asynchronous operation.
    /// </summary>
    /// <param name="asyncInfo">The asynchronous operation.</param>
    /// <param name="asyncStatus">One of the enumeration values.</param>
    public delegate void AsyncOperationCompletedHandler<TResult>(IAsyncOperation<TResult> asyncInfo, AsyncStatus asyncStatus);
}