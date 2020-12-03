﻿namespace Libiada.Database.Attributes
{
    using Libiada.Database;
    using System;
    using Attribute = System.Attribute;

    /// <summary>
    /// The nature attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class NatureAttribute : Attribute
    {
        /// <summary>
        /// Nature attribute value.
        /// </summary>
        public readonly Nature Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="NatureAttribute"/> class.
        /// </summary>
        /// <param name="value">
        /// The Nature value.
        /// </param>
        public NatureAttribute(Nature value)
        {
            if (!Enum.IsDefined(typeof(Nature), value))
            {
                throw new ArgumentException("Nature attribute value is not valid nature", nameof(value));
            }

            Value = value;
        }
    }
}
