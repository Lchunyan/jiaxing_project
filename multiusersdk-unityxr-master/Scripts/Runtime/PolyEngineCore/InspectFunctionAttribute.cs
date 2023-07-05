using UnityEngine;

namespace Ximmerse.XR
{
    /// <summary>
    /// Attribute to indicate inspector drawer to draw a quick invoke button.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Method | System.AttributeTargets.Property)]
    public class InspectFunctionAttribute : PropertyAttribute
    {
        public string label = "";

        public InspectFunctionAttribute()
        {
        
        }

        public InspectFunctionAttribute(string label)
        {
            this.label = label;
        }
    }
}