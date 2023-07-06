using UnityEngine;

namespace Ximmerse.XR
{
    public class DisableEditingAttribute : PropertyAttribute
    {
        public string label;

        public DisableEditingAttribute()
        {
        }

        public DisableEditingAttribute(string label)
        {
            this.label = label;
        }
    }
}