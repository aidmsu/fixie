﻿namespace Fixie
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Internal;

    public static class MethodInfoExtensions
    {
        /// <summary>
        /// Execute the given method against the given instance of its class.
        /// </summary>
        /// <returns>
        /// For void methods, returns null.
        /// For synchronous methods, returns the value returned by the test method.
        /// For async Task methods, returns null after awaiting the Task.
        /// For async Task<![CDATA[<T>]]> methods, returns the Result T after awaiting the Task.
        /// </returns>
        public static object Execute(this MethodInfo method, object instance, params object[] parameters)
        {
            bool isDeclaredAsync = method.IsAsync();

            if (isDeclaredAsync && method.IsVoid())
                throw new NotSupportedException(
                    "Async void methods are not supported. Declare async methods with a " +
                    "return type of Task to ensure the task actually runs to completion.");

            if (method.ContainsGenericParameters)
                throw new Exception("Could not resolve type parameters for generic method.");

            object result;

            try
            {
                result = method.Invoke(instance, parameters != null && parameters.Length == 0 ? null : parameters);
            }
            catch (TargetInvocationException exception)
            {
                throw new PreservedException(exception.InnerException);
            }

            if (!isDeclaredAsync)
                return result;

            var task = (Task) result;
            try
            {
                task.Wait();
            }
            catch (AggregateException exception)
            {
                throw new PreservedException(exception.InnerExceptions.First());
            }

            if (method.ReturnType.IsGenericType)
            {
                var property = task.GetType().GetProperty("Result", BindingFlags.Instance | BindingFlags.Public);

                return property.GetValue(task, null);
            }

            return null;
        }
    }
}