using System.Reflection;
using System;

namespace VapSRClient.Extensions;

internal static class TypeExtensions
{
    internal static T GetF<T>(this Type obj, string field, object instance) {
        return (T)obj.GetField(field, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic).GetValue(instance);
    }

    internal static void SetF(this Type obj, string field, object instance, object value) {
        obj.GetField(field, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic).SetValue(instance, value);
    }

    internal static object CallMethod(this Type obj, string method, object instance, object?[] param) {
        return obj.GetMethod(method, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic).Invoke(instance, param);
    }
}