using UnityEngine;

namespace Ximmerse.XR
{
    /// <summary>
    /// Attach this Minimum max vector attribute to a vector2 property
    /// </summary>
    public class MinValueAttribute : PropertyAttribute
    {
        public enum valueType
        {
            Float,
            Int,
        }

        public valueType _valueType = valueType.Float;
        public float minValue;

        public string label = "";

        public MinValueAttribute(int min)
        {
            this.minValue = min;
            _valueType = valueType.Int;
        }

        public MinValueAttribute(float min)
        {
            this.minValue = min;
            _valueType = valueType.Float;
        }

        public MinValueAttribute(string label, int min)
        {
            this.minValue = min;
            this.label = label;
            _valueType = valueType.Int;
        }

        public MinValueAttribute(string label, float min)
        {
            this.minValue = min;
            this.label = label;
            _valueType = valueType.Float;
        }
    }
}