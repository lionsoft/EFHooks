﻿using System.Threading;
using System.Threading.Tasks;

namespace EFHooks
{
    partial class HookedDbContext
    {

        /// <summary>
        /// Asynchronously saves all changes made in this context to the underlying database.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        /// A task that represents the asynchronous save operation.
        /// The task result contains the number of objects written to the underlying database.
        /// </returns>
        /// <remarks>
        /// Multiple active operations on the same context instance are not supported.  Use 'await' to ensure
        /// that any asynchronous operations have completed before calling another method on this context.
        /// </remarks>
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return Hooks == null ? base.SaveChangesAsync(cancellationToken) : Hooks.SaveChangesAsync(base.SaveChangesAsync, cancellationToken);
        }

        public override Task<int> SaveChangesAsync()
        {
            return SaveChangesAsync(CancellationToken.None);
        }
    }
}
