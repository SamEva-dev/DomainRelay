namespace DomainRelay.Abstractions;

/// <summary>
/// Represents a void-like response for commands that conceptually return no data.
/// </summary>
/// <remarks>
/// Use <see cref="Unit"/> when a request must use the generic
/// <see cref="IRequest{TResponse}"/> form but does not need to return a business value.
/// </remarks>
/// <example>
/// <code>
/// public sealed record DeleteTenantCommand(Guid TenantId) : IRequest&lt;Unit&gt;;
/// </code>
/// </example>
public readonly struct Unit : IEquatable<Unit>
{
    /// <summary>
    /// Gets the singleton-like default value for <see cref="Unit"/>.
    /// </summary>
    public static readonly Unit Value = default;

    /// <summary>
    /// Determines whether this instance is equal to another <see cref="Unit"/> instance.
    /// </summary>
    /// <param name="other">The other <see cref="Unit"/> instance.</param>
    /// <returns>Always returns <see langword="true"/>.</returns>
    public bool Equals(Unit other) => true;

    /// <summary>
    /// Determines whether this instance is equal to the specified object.
    /// </summary>
    /// <param name="obj">The object to compare with this instance.</param>
    /// <returns><see langword="true"/> when <paramref name="obj"/> is a <see cref="Unit"/> value.</returns>
    public override bool Equals(object? obj) => obj is Unit;

    /// <summary>
    /// Returns the hash code for this value.
    /// </summary>
    /// <returns>Always returns <c>0</c>.</returns>
    public override int GetHashCode() => 0;

    /// <summary>
    /// Returns a display representation of this value.
    /// </summary>
    /// <returns>The string <c>()</c>.</returns>
    public override string ToString() => "()";
}