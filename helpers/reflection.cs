using VapSRClient.Extensions;
using System;

namespace VapSRClient.Reflection;

internal class ReflectionHelper
{
	private readonly Type ClassType;
	private readonly object obj;

	public ReflectionHelper(object Class) {
		ClassType = Class.GetType();
		obj = Class;
	}

	public T GetField<T>(string field) {
		return ClassType.GetF<T>(field, obj);
	}

	public void SetField(string field, object value) {
		ClassType.SetF(field, obj, value);
	}

	public T CallMethod<T>(string method, object?[] parameters) 
	{
		parameters ??= new object[0];
		return (T)ClassType.CallMethod(method, obj, parameters);
	}
}