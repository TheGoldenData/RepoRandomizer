using System;

namespace REPORandomizer.Core
{
    public class EffectId : IEquatable<EffectId>
    {
        public string Value { get; }

        public EffectId(string value)
        {
            Value = value;
        }

        public bool Equals(EffectId other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as EffectId);
        }

        public override int GetHashCode()
        {
            return (Value != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(Value) : 0);
        }

        public static EffectId Of(string value) => new EffectId(value);

        public override string ToString() => Value;

        public static bool operator ==(EffectId left, EffectId right) => Equals(left, right);
        public static bool operator !=(EffectId left, EffectId right) => !Equals(left, right);
    }
}
