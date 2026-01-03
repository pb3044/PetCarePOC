namespace PetCarePlatform.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when an entity is not found.
    /// </summary>
    public class EntityNotFoundException : Exception
    {
        public string EntityName { get; }
        public object? EntityId { get; }

        public EntityNotFoundException(string entityName, object? entityId = null)
            : base($"Entity '{entityName}'{(entityId != null ? $" with ID '{entityId}'" : "")} was not found.")
        {
            EntityName = entityName;
            EntityId = entityId;
        }

        public EntityNotFoundException(string entityName, object? entityId, Exception innerException)
            : base($"Entity '{entityName}'{(entityId != null ? $" with ID '{entityId}'" : "")} was not found.", innerException)
        {
            EntityName = entityName;
            EntityId = entityId;
        }
    }
}
