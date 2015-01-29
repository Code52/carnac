﻿using System;

namespace Carnac.Logic
{

    /// <summary>
    /// Enables property notification.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class NotifyPropertyAttribute : Attribute
    {
        /// <summary>
        /// <b>true</b> to perform an equality check before firing property notification; otherwise <b>false</b>.
        /// </summary>
        /// <remarks>Defaults to <b>false</b>.</remarks>
        public bool PerformEqualityCheck { get; set; }

        /// <summary>
        /// The names of other properties to perform notification for.
        /// </summary>
        public string[] AlsoNotifyFor { get; set; }

        /// <summary>
        /// Gets the object's changed status.
        /// </summary>
        public bool SetIsChanged { get; set; }
    }
}