// //-----------------------------------------------------------------------
// // <copyright file="EnumerableSerializerFactory.cs" company="Asynkron HB">
// //     Copyright (C) 2015-2016 Asynkron HB All rights reserved
// // </copyright>
// //-----------------------------------------------------------------------

using PCLReflectionExtensions;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Wire.Extensions;
using Wire.ValueSerializers;

namespace Wire.SerializerFactories
{
    public class EnumerableSerializerFactory : ValueSerializerFactory
    {
        public override bool CanSerialize(Serializer serializer, Type type)
        {
            //TODO: check for constructor with IEnumerable<T> param

            if (!type.GetMethods().Any(m => m.Name == "AddRange" || m.Name == "Add"))
                return false;

            if (type.GetProperty("Count") == null)
                return false;

            var isGenericEnumerable = GetEnumerableType(type) != null;
            if (isGenericEnumerable)
                return true;

            if (typeof(ICollection).IsAssignableFrom(type))
                return true;

            return false;
        }

        public override bool CanDeserialize(Serializer serializer, Type type)
        {
            return CanSerialize(serializer, type);
        }

        private static Type GetEnumerableType(Type type)
        {
            return type
                .GetTypeInfo()
                .ImplementedInterfaces
                .Where(
                    intType =>
                        intType.IsGenericType() &&
                        intType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                .Select(intType => intType.GetTypeInfo().GenericTypeArguments[0])
                .FirstOrDefault();
        }

        public override ValueSerializer BuildSerializer(Serializer serializer, Type type,
            ConcurrentDictionary<Type, ValueSerializer> typeMapping)
        {
            var x = new ObjectSerializer(serializer.Options.FieldSelector, type);
            typeMapping.TryAdd(type, x);
            var preserveObjectReferences = serializer.Options.PreserveObjectReferences;

            var elementType = GetEnumerableType(type) ?? typeof(object);
            var elementSerializer = serializer.GetSerializerByType(elementType);

            var countProperty = type.GetProperty("Count");
            var addRange = type.GetMethod("AddRange");
            var add = type.GetMethod("Add");

            Func<object, int> countGetter = o => (int) countProperty.GetValue(o);


            ObjectReader reader = (stream, session) =>
            {
                var instance = Activator.CreateInstance(type);
                if (preserveObjectReferences)
                {
                    session.TrackDeserializedObject(instance);
                }

                var count = stream.ReadInt32(session);

                if (addRange != null)
                {
                    var items = Array.CreateInstance(elementType, count);
                    for (var i = 0; i < count; i++)
                    {
                        var value = stream.ReadObject(session);
                        items.SetValue(value, i);
                    }
                    //HACK: this needs to be fixed, codegenerated or whatever

                    addRange.Invoke(instance, new object[] {items});
                    return instance;
                }
                if (add != null)
                {
                    for (var i = 0; i < count; i++)
                    {
                        var value = stream.ReadObject(session);
                        add.Invoke(instance, new[] {value});
                    }
                }


                return instance;
            };

            ObjectWriter writer = (stream, o, session) =>
            {
                if (preserveObjectReferences)
                {
                    session.TrackSerializedObject(o);
                }
                Int32Serializer.WriteValueImpl(stream, countGetter(o), session);
                var enumerable = o as IEnumerable;
                // ReSharper disable once PossibleNullReferenceException
                foreach (var value in enumerable)
                {
                    stream.WriteObject(value, elementType, elementSerializer, preserveObjectReferences, session);
                }
            };
            x.Initialize(reader, writer);
            return x;
        }
    }
}