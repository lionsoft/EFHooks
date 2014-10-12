using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Common;
using System.Linq;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;

namespace EFHooks
{
    /// <summary>
    /// An Entity Framework DbContext that can be hooked into by registering EFHooks.IHook objects.
    /// </summary>
    public abstract partial class HookedDbContext : DbContext, IHookedDbContext
    {
        /// <summary>
        /// The pre-action hooks.
        /// </summary>
        protected IList<IPreActionHook> PreHooks;
        /// <summary>
        /// The post-action hooks.
        /// </summary>
        protected IList<IPostActionHook> PostHooks;

        /// <summary>
        /// The Post load hooks.
        /// </summary>
        protected IList<IPostLoadHook> PostLoadHooks;

        IList<IPreActionHook> IHookedDbContext.PreHooks { get { return PreHooks; } }
        IList<IPostActionHook> IHookedDbContext.PostHooks { get { return PostHooks; } }
        IList<IPostLoadHook> IHookedDbContext.PostLoadHooks { get { return PostLoadHooks; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="HookedDbContext" /> class, initializing empty lists of hooks.
        /// </summary>
        protected HookedDbContext()
        {
            PreHooks = new List<IPreActionHook>();
            PostHooks = new List<IPostActionHook>();
            PostLoadHooks = new List<IPostLoadHook>();
            ((IObjectContextAdapter)this).ObjectContext.ObjectMaterialized += ObjectMaterialized;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HookedDbContext" /> class, filling <see cref="PreHooks"/> and <see cref="PostHooks"/>.
        /// </summary>
        /// <param name="hooks">The hooks.</param>
        protected HookedDbContext(IHook[] hooks)
        {
            PreHooks = hooks.OfType<IPreActionHook>().ToList();
            PostHooks = hooks.OfType<IPostActionHook>().ToList();
            PostLoadHooks = hooks.OfType<IPostLoadHook>().ToList();
            ((IObjectContextAdapter)this).ObjectContext.ObjectMaterialized += ObjectMaterialized;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HookedDbContext" /> class, using the specified <paramref name="nameOrConnectionString"/>, initializing empty lists of hooks.
        /// </summary>
        /// <param name="nameOrConnectionString">The name or connection string.</param>
        protected HookedDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
            PreHooks = new List<IPreActionHook>();
            PostHooks = new List<IPostActionHook>();

            PostLoadHooks = new List<IPostLoadHook>();
            ((IObjectContextAdapter)this).ObjectContext.ObjectMaterialized += ObjectMaterialized;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HookedDbContext" /> class, using the specified <paramref name="nameOrConnectionString"/>, , filling <see cref="PreHooks"/> and <see cref="PostHooks"/>.
        /// </summary>
        /// <param name="hooks">The hooks.</param>
        /// <param name="nameOrConnectionString">The name or connection string.</param>
        protected HookedDbContext(IHook[] hooks, string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
            PreHooks = hooks.OfType<IPreActionHook>().ToList();
            PostHooks = hooks.OfType<IPostActionHook>().ToList();

            PostLoadHooks = hooks.OfType<IPostLoadHook>().ToList();
            ((IObjectContextAdapter)this).ObjectContext.ObjectMaterialized += ObjectMaterialized;
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
            PreHooks = new List<IPreActionHook>();
            PostHooks = new List<IPostActionHook>();

            PostLoadHooks = new List<IPostLoadHook>();
            ((IObjectContextAdapter)this).ObjectContext.ObjectMaterialized += ObjectMaterialized;
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
            foreach (var hook in hooks)
            {
                RegisterHook(hook as IPreActionHook);
                RegisterHook(hook as IPostActionHook);
                RegisterHook(hook as IPostLoadHook);
            }
        }

        /// <summary>
        /// Registers a hook to run before a database action occurs.
        /// </summary>
        /// <param name="hook">The hook to register.</param>
        public void RegisterHook(IPreActionHook hook)
        {
            if (hook != null)
                PreHooks.Add(hook);
        }

        /// <summary>
        /// Registers a hook to run after a database action occurs.
        /// </summary>
        /// <param name="hook">The hook to register.</param>
        public void RegisterHook(IPostActionHook hook)
        {
            if (hook != null)
                PostHooks.Add(hook);
        }

        /// <summary>
        /// Registers a hook to run after a database load occurs.
        /// </summary>
        /// <param name="hook">The hook to register.</param>
        public void RegisterHook(IPostLoadHook hook)
        {
            if (hook != null)
                PostLoadHooks.Add(hook);
        }

        /// <summary>
        /// Saves all changes made in this context to the underlying database.
        /// </summary>
        /// <returns>
        /// The number of objects written to the underlying database.
        /// </returns>
        public override int SaveChanges()
        {
            var hookExecution = new HookRunner(this);
            hookExecution.RunPreActionHooks();
            var result = base.SaveChanges();
            hookExecution.RunPostActionHooks();
            return result;
        }

        private void ObjectMaterialized(object sender, ObjectMaterializedEventArgs e)
        {
            var metadata = new HookEntityMetadata(EntityState.Unchanged, this);

            foreach (var postLoadHook in PostLoadHooks)
            {
                postLoadHook.HookObject(e.Entity, metadata);
            }
        }
    }
}