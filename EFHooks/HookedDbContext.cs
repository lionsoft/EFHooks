using System.Data.Entity;
using System.Data.Common;

namespace EFHooks
{
    /// <summary>
    /// An Entity Framework DbContext that can be hooked into by registering EFHooks.IHook objects.
    /// </summary>
    public abstract partial class HookedDbContext : DbContext
    {
        public DbContextHooks Hooks { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HookedDbContext" /> class, initializing empty lists of hooks.
        /// </summary>
        protected HookedDbContext()
        {
            Hooks = new DbContextHooks(this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HookedDbContext" /> class, filling <see cref="Hooks"/>.
        /// </summary>
        /// <param name="hooks">The hooks.</param>
        protected HookedDbContext(IHook[] hooks)
        {
            Hooks = new DbContextHooks(this, hooks);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HookedDbContext" /> class, using the specified <paramref name="nameOrConnectionString"/>, initializing empty lists of hooks.
        /// </summary>
        /// <param name="nameOrConnectionString">The name or connection string.</param>
        protected HookedDbContext(string nameOrConnectionString): base(nameOrConnectionString)
        {
            Hooks = new DbContextHooks(this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HookedDbContext" /> class, using the specified <paramref name="nameOrConnectionString"/>, filling <see cref="Hooks"/>.
        /// </summary>
        /// <param name="hooks">The hooks.</param>
        /// <param name="nameOrConnectionString">The name or connection string.</param>
        protected HookedDbContext(IHook[] hooks, string nameOrConnectionString): base(nameOrConnectionString)
        {
            Hooks = new DbContextHooks(this, hooks);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HookedDbContext" /> class using the an existing connection to connect 
        /// to a database. The connection will not be disposed when the context is disposed. (see <see cref="DbContext"/> overloaded constructor)
        /// </summary>
        /// <param name="existingConnection">An existing connection to use for the new context.</param>
        /// <param name="contextOwnsConnection">If set to true the connection is disposed when the context is disposed, otherwise the caller must dispose the connection.</param>
        /// <remarks>Main reason for allowing this, is to enable reusing another database connection. (For instance one that is profiled by Miniprofiler (http://miniprofiler.com/)).</remarks>
        protected HookedDbContext(DbConnection existingConnection, bool contextOwnsConnection)
            : base(existingConnection, contextOwnsConnection)
        {
            Hooks = new DbContextHooks(this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HookedDbContext" /> class using the an existing connection to connect 
        /// to a database. The connection will not be disposed when the context is disposed. (see <see cref="DbContext"/> overloaded constructor)
        /// </summary>
        /// <param name="hooks">The hooks.</param>
        /// <param name="existingConnection">An existing connection to use for the new context.</param>
        /// <param name="contextOwnsConnection">If set to true the connection is disposed when the context is disposed, otherwise the caller must dispose the connection.</param>
        /// <remarks>Main reason for allowing this, is to enable reusing another database connection. (For instance one that is profiled by Miniprofiler (http://miniprofiler.com/)).</remarks>
        protected HookedDbContext(IHook[] hooks, DbConnection existingConnection, bool contextOwnsConnection)
            : this(existingConnection, contextOwnsConnection)
        {
            Hooks = new DbContextHooks(this, hooks);
        }

        /// <summary>
        /// Registers a hook to run before a database action occurs.
        /// </summary>
        /// <param name="hook">The hook to register.</param>
        public void RegisterHook(IPreActionHook hook)
        {
            Hooks.Add(hook);
        }

        /// <summary>
        /// Registers a hook to run after a database action occurs.
        /// </summary>
        /// <param name="hook">The hook to register.</param>
        public void RegisterHook(IPostActionHook hook)
        {
            Hooks.Add(hook);
        }

        /// <summary>
        /// Registers a hook to run after a database load occurs.
        /// </summary>
        /// <param name="hook">The hook to register.</param>
        public void RegisterHook(IPostLoadHook hook)
        {
            Hooks.Add(hook);
        }

        /// <summary>
        /// Saves all changes made in this context to the underlying database.
        /// </summary>
        /// <returns>
        /// The number of objects written to the underlying database.
        /// </returns>
        public override int SaveChanges()
        {
            return Hooks.SaveChanges(base.SaveChanges);
        }
    }
}