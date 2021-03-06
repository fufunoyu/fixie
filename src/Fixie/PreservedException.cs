﻿namespace Fixie
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Packages an exception thrown by a method invoked via reflection,
    /// so that the original exception's stack trace is preserved.
    /// </summary>
    public class PreservedException : Exception
    {
        public Exception OriginalException { get; }

        public PreservedException(TargetInvocationException targetInvocationException)
            => OriginalException = targetInvocationException.InnerException!;
    }
}