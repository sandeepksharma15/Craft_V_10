using System.Numerics;

namespace Craft.Domain;

/// <summary>
/// Base class for immutable numeric value objects.
/// </summary>
public abstract class NumericValueObject<TValue, TSelf> : SingleValueObject<TValue>
    where TValue : struct, INumber<TValue>
    where TSelf : NumericValueObject<TValue, TSelf>
{
    protected NumericValueObject(TValue value) : base(value)
    {
        Ensure(IsValid(value), ValidationMessage);
    }

    /// <summary>
    /// Validation logic implemented by the derived class.
    /// </summary>
    protected virtual bool IsValid(TValue value)
        => true;

    /// <summary>
    /// Validation message.
    /// </summary>
    protected virtual string ValidationMessage
        => "Invalid numeric value.";

    public override string ToString()
        => Value.ToString()!;

    public static implicit operator TValue(NumericValueObject<TValue, TSelf> value)
        => value.Value;
}
