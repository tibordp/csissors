using System;
using System.Diagnostics;

namespace Csissors.Utilities
{
    public struct Instant: IComparable<Instant>, IEquatable<Instant> {
        private readonly static long Frequency = Stopwatch.Frequency;
        public readonly long Ticks;
        public Instant(long ticks) {
            Ticks = ticks;
        }

        public static Instant operator+(Instant lhs, TimeSpan rhs) => new Instant(lhs.Ticks + (long)(rhs.TotalSeconds * Frequency));
        public static Instant operator-(Instant lhs, TimeSpan rhs) => new Instant(lhs.Ticks - (long)(rhs.TotalSeconds * Frequency));
        public static TimeSpan operator-(Instant lhs, Instant rhs) => TimeSpan.FromSeconds((double)(lhs.Ticks - rhs.Ticks) / Frequency);

        public static bool operator <(Instant left, Instant right) => left.Ticks < right.Ticks;
        public static bool operator <=(Instant left, Instant right) => left.Ticks <= right.Ticks;
        public static bool operator >(Instant left, Instant right) => left.Ticks > right.Ticks;
        public static bool operator >=(Instant left, Instant right) => left.Ticks >= right.Ticks;
        public static bool operator ==(Instant left, Instant right) => left.Ticks == right.Ticks;
        public static bool operator !=(Instant left, Instant right) => left.Ticks != right.Ticks;
        
        public static Instant Now => new Instant(Stopwatch.GetTimestamp());
        public DateTimeOffset AsUtcDateTimeOffset => DateTimeOffset.UtcNow + (this - Now);
        public DateTimeOffset AsDateTimeOffset => DateTimeOffset.Now + (this - Now);
        public DateTime AsDateTime => DateTime.Now + (this - Now);
        public DateTimeOffset AsUtcDateTime => DateTime.Now + (this - Now);
        public int CompareTo(Instant other) => Ticks.CompareTo(other.Ticks);

        public bool Equals(Instant other)
        {
            return Ticks.Equals(other.Ticks);
        }

        public override bool Equals(object obj)
        {
            if (obj is Instant instant)
                return Ticks == instant.Ticks;
            return false;
        }

        public override int GetHashCode()
        {
            return Ticks.GetHashCode();
        }

        public override string ToString()
        {
            return AsDateTimeOffset.ToString();
        }
    }
}