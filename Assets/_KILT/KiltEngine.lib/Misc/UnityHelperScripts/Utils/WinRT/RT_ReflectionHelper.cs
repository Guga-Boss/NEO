#if (UNITY_WINRT || UNITY_WP_8_1) && !UNITY_EDITOR && !UNITY_WP8
#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Collections;
using System.Globalization;
using System.Text;
using System.Linq;

namespace Kilt.RT.Reflection
{
    internal enum MemberTypes
    {
        Property,
        Field,
        Event,
        Method,
        Other
    }

    [Flags]
    internal enum BindingFlags
    {
        Default = 0,
        IgnoreCase = 1,
        DeclaredOnly = 2,
        Instance = 4,
        Static = 8,
        Public = 16,
        NonPublic = 32,
        FlattenHierarchy = 64,
        InvokeMethod = 256,
        CreateInstance = 512,
        GetField = 1024,
        SetField = 2048,
        GetProperty = 4096,
        SetProperty = 8192,
        PutDispProperty = 16384,
        ExactBinding = 65536,
        PutRefDispProperty = 32768,
        SuppressChangeType = 131072,
        OptionalParamBinding = 262144,
        IgnoreReturn = 16777216
    }

    public static class ReflectionHelper
    {
        public static T GetAttribute<T>(object attributeProvider) where T : System.Attribute
        {
            return GetAttribute<T>(attributeProvider, true);
        }

		public static T GetAttribute<T>(object attributeProvider, bool inherit) where T : System.Attribute
        {
            T[] attributes = GetAttributes<T>(attributeProvider, inherit);

            return (attributes != null) ? attributes.SingleOrDefault() : null;
        }

		public static T[] GetAttributes<T>(object provider, bool inherit) where T : System.Attribute
        {
            if (provider is Type)
                return ((Type)provider).GetTypeInfo().GetCustomAttributes<T>(inherit).ToArray();

            if (provider is Assembly)
                return ((Assembly)provider).GetCustomAttributes<T>().ToArray();

            if (provider is MemberInfo)
                return ((MemberInfo)provider).GetCustomAttributes<T>(inherit).ToArray();

            if (provider is Module)
                return ((Module)provider).GetCustomAttributes<T>().ToArray();

            if (provider is ParameterInfo)
                return ((ParameterInfo)provider).GetCustomAttributes<T>(inherit).ToArray();

            throw new Exception("Cannot get attributes from '{0}'.");
        }
    }
}
#endif