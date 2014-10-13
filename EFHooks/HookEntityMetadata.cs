using System.Data.Entity;

namespace EFHooks
{
    /// <summary>
    /// Contains entity state, and an indication wether is has been changed.
    /// </summary>
    public class HookEntityMetadata
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HookEntityMetadata" /> class.
        /// </summary>
        /// <param name="hookType"></param>
        /// <param name="entry"></param>
        /// <param name="state">The state.</param>
        /// <param name="context">The optional existing context (I believe this is usable for migrations).</param>
        public HookEntityMetadata(HookType hookType, HookedEntityEntry entry, EntityState state, DbContext context = null)
        {
            HookType = hookType;
            Entry = entry;
            _state = state;
            CurrentContext = context;
        }

        public HookEntityMetadata(EntityState state, DbContext context = null)
        {
            _state = state;
            CurrentContext = context;
        }

        public HookType HookType { get; private set; }

        public HookedEntityEntry Entry { get; private set; }

        private EntityState _state;
        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        public EntityState State
        {
            get { return _state; }
            set
            {
                if (_state != value)
                {
                    _state = value;
                    HasStateChanged = true;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has state changed.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has state changed; otherwise, <c>false</c>.
        /// </value>
        public bool HasStateChanged { get; private set; }

        /// <summary>
        /// Container for wrapped context?
        /// </summary>
        /// <value>
        /// The current context.
        /// </value>
        public DbContext CurrentContext { get; private set; }
    }
}