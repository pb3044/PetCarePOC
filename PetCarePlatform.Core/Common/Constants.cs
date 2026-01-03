namespace PetCarePlatform.Core.Common
{
    /// <summary>
    /// Application-wide constants.
    /// </summary>
    public static class Constants
    {
        public const int DefaultPageSize = 10;
        public const int MaxPageSize = 100;
        
        public static class ErrorCodes
        {
            public const string EntityNotFound = "ENTITY_NOT_FOUND";
            public const string ValidationError = "VALIDATION_ERROR";
            public const string BusinessRuleViolation = "BUSINESS_RULE_VIOLATION";
            public const string Unauthorized = "UNAUTHORIZED";
            public const string Conflict = "CONFLICT";
            public const string InternalError = "INTERNAL_ERROR";
        }
    }
}

