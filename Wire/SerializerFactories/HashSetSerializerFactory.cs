// //-----------------------------------------------------------------------
// // <copyright file="HashSetSerializerFactory.cs" company="Asynkron HB">
// //     Copyright (C) 2015-2016 Asynkron HB All rights reserved
// // </copyright>
// //-----------------------------------------------------------------------

using PCLReflectionExtensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Wire.Extensions;
using Wire.ValueSerializers;

namespace Wire.SerializerFactories
{
    public class HashSetSerializerFactory : ValueSerializerFactory
    {
        public override bool CanSerialize(Serializer serializer, Type type) => IsInterface(type);

        private static bool IsInterface(Type type)
        {
            return type.IsGenericType() &&
                   type.GetGenericTypeDefinition() == typeof(HashSet<>);
        }

        public override bool CanDeserialize(Serializer serializer, Type type) => IsInterface(type);

        public override ValueSerializer BuildSerializer(Serializer serializer, Type type,
            ConcurrentDictionary<Type, ValueSerializer> typeMapping)
        {
            var preserveObjectReferences = serializer.Options.PreserveObjectReferences;
            var ser = new ObjectSerializer(serializer.Options.FieldSelector, type);
            typeMapping.TryAdd(type, ser);
            var elementType = type.GetGenericArguments()[0];
            var elementSerializer = serializer.GetSerializerByType(elementType);
            var readGeneric = GetType().GetMethod(nameof(ReadHashSet), BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(elementType);
            var writeGeneric = GetType().GetMethod(nameof(WriteHashSet), BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(elementType);

            ObjectReader reader = (stream, session) =>
            {
                var res = readGeneric.Invoke(null, new object[] {stream, session, preserveObjectReferences});
                return res;
            };

            ObjectWriter writer = (stream, obj, session) =>
            {

                writeGeneric.Invoke(null, new[] {obj, stream, session,elementType, elementSerializer, preserveObjectReferences});
            };
            ser.Initialize(reader, writer);

            return ser;
        }

        private static HashSet<T> ReadHashSet<T>(Stream stream, DeserializerSession session,bool preserveObjectReferences)
        {
            var set = new HashSet<T>();
            if (preserveObjectReferences)
            {
                session.TrackDeserializedObject(set);
            }
            var count = stream.ReadInt32(session);
            for (var i = 0; i < count; i++)
            {
                var item = (T)stream.ReadObject(session);
                set.Add(item);
            }
            return set;
        }

        private static void WriteHashSet<T>(HashSet<T> set, Stream stream, SerializerSession session, Type elementType,
            ValueSerializer elementSerializer, bool preserveObjectReferences)
        {
            if (preserveObjectReferences)
            {
                session.TrackSerializedObject(set);
            }
            // ReSharper disable once PossibleNullReferenceException
            Int32Serializer.WriteValueImpl(stream, set.Count, session);
            foreach (var item in set)
            {
                stream.WriteObject(item,elementType,elementSerializer, preserveObjectReferences, session);
            }
        }
    }
}