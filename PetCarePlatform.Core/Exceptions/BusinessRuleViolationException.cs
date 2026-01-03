namespace PetCarePlatform.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when a business rule is violated.
    /// </summary>
    public class BusinessRuleViolationException : Exception
    {
        public string RuleName { get; }

        public BusinessRuleViolationException(string ruleName, string message)
            : base(message)
        {
            RuleName = ruleName;
        }

        public BusinessRuleViolationException(string ruleName, string message, Exception innerException)
            : base(message, innerException)
        {
            RuleName = ruleName;
        }
    }
}
