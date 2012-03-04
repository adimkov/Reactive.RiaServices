// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OperationException.cs" author="Anton Dimkov">
//   Copyright (c) Anton Dimkov 2012. All rights reserved.
// </copyright>
// <summary>
// Declaration of the OperationException class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Reactive.RiaServices
{
    using System;
    using System.ServiceModel.DomainServices.Client;

    /// <summary>
    /// Declaration of the OperationException class.
    /// </summary>
    public class OperationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OperationException"/> class.
        /// </summary>
        /// <param name="innerException">The inner exception.</param>
        /// <param name="operation">The operation.</param>
        public OperationException(Exception innerException, OperationBase operation)
            : base(innerException.Message, innerException)
        {
            Operation = operation;
        }

        /// <summary>
        /// Gets the failure operation.
        /// </summary>
        public OperationBase Operation { get; private set; }
             
    }
}