using System;
using UnityEngine;

namespace _Andre._Utils
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ConditionalFieldAttribute : PropertyAttribute
    {
        public string PropertyToCheck;

        public object CompareValue;
	 
        public ConditionalFieldAttribute(string propertyToCheck, object compareValue = null)
        {
            PropertyToCheck = propertyToCheck;
            CompareValue = compareValue;
        }
    }
}