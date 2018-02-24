﻿namespace Fixie.Samples
{
    using System;
    using System.Linq;
    using System.Reflection;

    public static class TypeExtensions
    {
        public static void Execute(this Type testClass, object instance, Func<MethodInfo, bool> condition)
        {
            var query = testClass
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(condition);

            foreach (var q in query)
                q.Execute(instance);
        }

        public static void Execute(this RunContext runContext, object instance, string methodName)
            => runContext.TestClass.Execute(instance, x => x.Name == methodName);

        public static void Execute<TAttribute>(this RunContext runContext, object instance) where TAttribute : Attribute
            => runContext.TestClass.Execute(instance, x => x.HasOrInherits<TAttribute>());
    }
}