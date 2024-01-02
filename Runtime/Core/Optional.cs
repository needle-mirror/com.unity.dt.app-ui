using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.Core
{
    [Serializable]
    public struct Optional<T> : IEquatable<Optional<T>>
    {
        [SerializeField]
        bool isSet;
        
        [SerializeField]
        T value;
        
        public bool IsSet => isSet;

        public T Value => value;

        public Optional(T value)
        {
            this.value = value;
            isSet = true;
        }
        
        public static Optional<T> none => default;
        
        public static implicit operator Optional<T>(T value) => new (value);

        public bool Equals(Optional<T> other)
        {
            return isSet == other.isSet && EqualityComparer<T>.Default.Equals(value, other.value);
        }

        public override bool Equals(object obj)
        {
            return obj is Optional<T> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(IsSet, Value);
        }
        
        public static bool operator ==(Optional<T> left, Optional<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Optional<T> left, Optional<T> right)
        {
            return !left.Equals(right);
        }
    }
    
    [Serializable]
    public struct OptionalEnum<T> : IEquatable<OptionalEnum<T>>
        where T : Enum
    {
        [SerializeField]
        bool isSet;
        
        [SerializeField]
        T value;
        
        public bool IsSet => isSet;

        public T Value => value;

        public OptionalEnum(T value)
        {
            this.value = value;
            isSet = true;
        }
        
        public static OptionalEnum<T> none => default;
        
        public static implicit operator OptionalEnum<T>(T value) => new (value);

        public bool Equals(OptionalEnum<T> other)
        {
            return isSet == other.isSet && value.Equals(other.value);
        }

        public override bool Equals(object obj)
        {
            return obj is OptionalEnum<T> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(IsSet, Value);
        }
        
        public static bool operator ==(OptionalEnum<T> left, OptionalEnum<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(OptionalEnum<T> left, OptionalEnum<T> right)
        {
            return !left.Equals(right);
        }
    }
    
    public class OptionalField<T> : BaseField<Optional<T>>
    {
        public new static readonly string ussClassName = "unity-optional-field";

        public OptionalField(string label, VisualElement visualInput)
            : base(label, visualInput)
        {
            AddToClassList(ussClassName);
        }
    }

    public class OptionalEnumField<T> : BaseField<OptionalEnum<T>>
        where T : Enum
    {
        public new static readonly string ussClassName = "unity-optional-field";

        public OptionalEnumField(string label, VisualElement visualInput)
            : base(label, visualInput)
        {
            AddToClassList(ussClassName);
        }
    }
}
