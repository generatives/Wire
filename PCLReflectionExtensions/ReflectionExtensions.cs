using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PCLReflectionExtensions
{
    [Flags]
    public enum BindingFlags
    {
        Static = 1, Public = 2, NonPublic = 4, Instance = 8
    }

    public static class ReflectionExtensions
    {
        public static PropertyInfo GetProperty(this Type type, string name, BindingFlags flags)
        {
            return type.GetProperties(flags).FirstOrDefault(f => f.Name == name);
        }

        public static FieldInfo GetField(this Type type, string name, BindingFlags flags)
        {
            return type.GetFields(flags).FirstOrDefault(f => f.Name == name);
        }

        public static MethodInfo GetMethod(this Type type, string name, BindingFlags flags)
        {
            return type.GetMethods(flags).FirstOrDefault(f => f.Name == name);
        }

        public static PropertyInfo GetProperty(this Type type, string name)
        {
            return type.GetRuntimeProperty(name);
        }

        public static FieldInfo GetField(this Type type, string name)
        {
            return type.GetRuntimeField(name);
        }

        public static MethodInfo GetMethod(this Type type, string name)
        {
            return type.GetRuntimeMethods().FirstOrDefault(m => m.Name == name);
        }

        public static MethodInfo GetMethod(this Type type, string name, Type[] parameterTypes)
        {
            return type.GetRuntimeMethod(name, parameterTypes);
        }

        public static FieldInfo[] GetFields(this Type type, BindingFlags flags)
        {
            return type.GetRuntimeFields()
                .Where(f => TestBindingFlags(f, flags))
                .ToArray();
        }

        public static PropertyInfo[] GetProperties(this Type type, BindingFlags flags)
        {
            return type.GetRuntimeProperties()
                .Where(f => TestBindingFlags(f, flags))
                .ToArray();
        }

        public static MethodInfo[] GetMethods(this Type type, BindingFlags flags)
        {
            return type.GetRuntimeMethods()
                .Where(f => TestBindingFlags(f, flags))
                .ToArray();
        }

        public static MethodInfo[] GetMethods(this Type type)
        {
            return type.GetRuntimeMethods()
                .ToArray();
        }

        public static bool TestBindingFlags(FieldInfo info, BindingFlags flags)
        {
            if (HasBindingFlag(flags, BindingFlags.Instance) && !info.IsStatic)
            {
                return true;
            }

            if (HasBindingFlag(flags, BindingFlags.Static) && info.IsStatic)
            {
                return true;
            }

            if (HasBindingFlag(flags, BindingFlags.Public) && info.IsPublic)
            {
                return true;
            }

            if (HasBindingFlag(flags, BindingFlags.NonPublic) && !info.IsPublic)
            {
                return true;
            }

            return false;
        }

        public static bool TestBindingFlags(MethodInfo info, BindingFlags flags)
        {
            if(HasBindingFlag(flags, BindingFlags.Instance) && info.IsStatic)
            {
                return false;
            }

            if (HasBindingFlag(flags, BindingFlags.Static) && !info.IsStatic)
            {
                return false;
            }

            if (HasBindingFlag(flags, BindingFlags.Public) && !info.IsPublic)
            {
                return false;
            }

            if (HasBindingFlag(flags, BindingFlags.NonPublic) && info.IsPublic)
            {
                return false;
            }

            return true;
        }

        public static bool TestBindingFlags(PropertyInfo info, BindingFlags flags)
        {
            MethodInfo mInfo = info.GetMethod ?? info.SetMethod ?? null;
            if(mInfo == null)
            {
                throw new Exception("Property must have a get or set method");
            }

            return TestBindingFlags(mInfo, flags);
        }

        public static bool HasBindingFlag(BindingFlags flags, BindingFlags flag)
        {
            return (flags & flag) == flag;
        }

        public static bool IsGenericType(this Type type)
        {
            return type.GetTypeInfo().IsGenericType;
        }

        public static bool IsValueType(this Type type)
        {
            return type.GetTypeInfo().IsValueType;
        }

        public static ConstructorInfo GetConstructor(this Type type, Type[] parameterTypes)
        {
            return type.GetTypeInfo()
                .DeclaredConstructors
                .FirstOrDefault(c => c
                    .GetParameters()
                    .Select(p => p.ParameterType)
                    .SequenceEqual(parameterTypes));
        }

        public static bool IsAssignableFrom(this Type type, Type otherType)
        {
            return type.GetTypeInfo().IsAssignableFrom(otherType.GetTypeInfo());
        }

        public static bool IsSubclassOf(this Type type, Type otherType)
        {
            return type.GetTypeInfo().IsSubclassOf(otherType);
        }

        public static IEnumerable<Type> GetInterfaces(this Type type)
        {
            return type.GetTypeInfo().ImplementedInterfaces;
        }

        public static Type[] GetGenericArguments(this Type type)
        {
            return type.GetTypeInfo().GenericTypeArguments;
        }
    }
}
