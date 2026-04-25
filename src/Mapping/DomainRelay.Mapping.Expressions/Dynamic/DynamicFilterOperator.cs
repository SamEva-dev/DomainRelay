namespace DomainRelay.Mapping.Expressions.Dynamic;

/// <summary>
/// Represents a supported dynamic filter operation.
/// </summary>
public enum DynamicFilterOperator
{
    /// <summary>
    /// Checks whether the member is equal to the specified value.
    /// </summary>
    Equals = 0,

    /// <summary>
    /// Checks whether the member is not equal to the specified value.
    /// </summary>
    NotEquals = 1,

    /// <summary>
    /// Checks whether the member is greater than the specified value.
    /// </summary>
    GreaterThan = 2,

    /// <summary>
    /// Checks whether the member is greater than or equal to the specified value.
    /// </summary>
    GreaterThanOrEqual = 3,

    /// <summary>
    /// Checks whether the member is less than the specified value.
    /// </summary>
    LessThan = 4,

    /// <summary>
    /// Checks whether the member is less than or equal to the specified value.
    /// </summary>
    LessThanOrEqual = 5,

    /// <summary>
    /// Checks whether a string member contains the specified value.
    /// </summary>
    StringContains = 6
}