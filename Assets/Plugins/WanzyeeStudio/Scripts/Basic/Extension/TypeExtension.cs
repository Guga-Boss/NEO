
/*WWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWW*\     (   (     ) )
|/                                                      \|       )  )   _((_
||  (c) Wanzyee Studio  < wanzyeestudio.blogspot.com >  ||      ( (    |_ _ |=n
|\                                                      /|   _____))   | !  ] U
\.ZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZ./  (_(__(S)   |___*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace WanzyeeStudio.Extension{

	/// <summary>
	/// Include extension methods about operation of <c>System.Type</c>.
	/// </summary>
	public static class TypeExtension{

		/// <summary>
		/// The dictionary of built-in types with pretty name.
		/// </summary>
		private static Dictionary<Type, string> _builtinNames = new Dictionary<Type, string>(){
			{typeof(void), "void"},
			{typeof(bool), "bool"},
			{typeof(byte), "byte"},
			{typeof(char), "char"},
			{typeof(decimal), "decimal"},
			{typeof(double), "double"},
			{typeof(float), "float"},
			{typeof(int), "int"},
			{typeof(long), "long"},
			{typeof(object), "object"},
			{typeof(sbyte), "sbyte"},
			{typeof(short), "short"},
			{typeof(string), "string"},
			{typeof(uint), "uint"},
			{typeof(ulong), "ulong"},
			{typeof(ushort), "ushort"}
		};

		/// <summary>
		/// Get a pretty readable name of the type, even generic, optional to use the full name.
		/// </summary>
		/// 
		/// <remarks>
		/// This doesn't handle anonymous types.
		/// </remarks>
		/// 
		/// <returns>The pretty name.</returns>
		/// <param name="type">Type.</param>
		/// <param name="full">If set to <c>true</c> use the full name.</param>
		/// 
		/*
		 * Reference page below, add array type name, used if a list nests with array.
		 * http://stackoverflow.com/q/6402864
		 * http://stackoverflow.com/q/1533115
		 */
		public static string GetPrettyName(this Type type, bool full = false){
			
			if(null == type) throw new ArgumentNullException("type");

			if(type.IsArray){
				var _e = type.GetElementType().GetPrettyName(full);
				return string.Format("{0}[{1}]", _e, new string(',', type.GetArrayRank() - 1));
			}

			if(!full){

				if(_builtinNames.ContainsKey(type)) return _builtinNames[type];
				if(!type.Name.Contains("`")) return type.Name;

				var _t = typeof(Nullable<>);
				var _n = (type != _t && type.IsGenericType && _t == type.GetGenericTypeDefinition());
				if(_n) return type.GetGenericArguments()[0].GetPrettyName() + "?";

			}

			var _g = type.IsGenericType && !Regex.IsMatch(type.FullName, @"(\A|\.|\+)\W");
			return _g ? GetPrettyNameInternalGeneric(type, full) : (full ? type.FullName : type.Name);

		}

		/// <summary>
		/// Get a pretty name of generic type, sub method for <c>GetPrettyName()</c>.
		/// Change to type name format and wrap argument types with angle brackets.
		/// </summary>
		/// <returns>The pretty name.</returns>
		/// <param name="type">Type.</param>
		/// <param name="full">If set to <c>true</c> full name.</param>
		private static string GetPrettyNameInternalGeneric(Type type, bool full){
			
			var _n = full ? type.FullName : type.Name;
			if(_n.Contains("[[")) _n = _n.Remove(_n.IndexOf("[["));

			var _g = type.GetGenericArguments();
			var _a = _g.Select(_v => _v.IsGenericParameter ? string.Empty : _v.GetPrettyName(full));
			var _c = _a.Count();

			foreach(var _v in Regex.Matches(_n, @"`\d+").Cast<Match>().Reverse()){

				var _t = int.Parse(_v.Value.Substring(1));
				_c -= _t;

				var _s = string.Join(", ", _a.Skip(_c).Take(_t).ToArray());
				_n = _n.Remove(_v.Index, _v.Length).Insert(_v.Index, "<" + _s + ">");

			}

			return Regex.Replace(_n, @" (?=[,>])", string.Empty);

		}

		/// <summary>
		/// Get the default value of the type, just like <c>default(T)</c>.
		/// </summary>
		/// <returns>The default value.</returns>
		/// <param name="type">Type.</param>
		public static object GetDefault(this Type type){

			if(null == type) throw new ArgumentNullException("type");

			if(!type.IsValueType || null != Nullable.GetUnderlyingType(type)) return null;

			else return Activator.CreateInstance(type);

		}
		
		/// <summary>
		/// Get the parent hierarchy array, sorted from self to root type.
		/// </summary>
		/// <returns>The parent hierarchy array.</returns>
		/// <param name="type">Type.</param>
		public static Type[] GetParents(this Type type){
			
			var _result = new List<Type>();

			for(var t = type; null != t; t = t.BaseType) _result.Add(t);

			return _result.ToArray();
			
		}

		/// <summary>
		/// Get all child types, excluding self, optional to find deep or directly inheritance only.
		/// </summary>
		/// <returns>The child types.</returns>
		/// <param name="type">Type.</param>
		/// <param name="deep">If set to <c>true</c> deep.</param>
		public static Type[] GetChildren(this Type type, bool deep = false){

			var _t = AppDomain.CurrentDomain.GetAssemblies().SelectMany(_v => _v.GetTypes());

			return (deep ? _t.Where(_v => _v.IsSubclassOf(type)) : _t.Where(_v => _v.BaseType == type)).ToArray();

		}
		
		/// <summary>
		/// Return the element type of an array or list type, otherwise <c>null</c>.
		/// </summary>
		/// <returns>The element type.</returns>
		/// <param name="type">Type.</param>
		/*
		 * http://stackoverflow.com/q/906499
		 */
		public static Type GetItemType(this Type type){

			if(!typeof(IList).IsAssignableFrom(type)) return null;

			if(type.IsArray) return type.GetElementType();

			var _t = type.GetInterfaces().FirstOrDefault(
				_v => _v.IsGenericType && _v.GetGenericTypeDefinition() == typeof(IEnumerable<>)
			);

			return (null == _t) ? null : _t.GetGenericArguments()[0];

		}

		/// <summary>
		/// Determine if able to create an instance of the type.
		/// </summary>
		/// 
		/// <remarks>
		/// Optional to throw an exception message or just return <c>false</c> if invalid.
		/// This only checks some basic conditions and might be not precise.
		/// </remarks>
		/// 
		/// <remarks>
		/// The current conditions below:
		/// 	1. Return <c>false</c> only if it's interface, abstract, generic definition, delegate.
		/// 	2. Recurse to check the element type of an array type.
		/// 	3. Recurse to check the generic arguments of a list or dictionary type.
		/// </remarks>
		/// 
		/// <returns><c>true</c>, if creatable, <c>false</c> otherwise.</returns>
		/// <param name="type">Type.</param>
		/// <param name="exception">Flag to throw an exception or return <c>false</c>.</param>
		/// 
		public static bool IsCreatable(this Type type, bool exception = false){

			try{

				if(null == type) throw new Exception("Argument cannot be null.{0}");

				if(type.IsInterface) throw new Exception("Can't create interface {0}.");
				if(type.IsAbstract) throw new Exception("Can't create abstract type {0}.");

				if(type.IsGenericTypeDefinition) throw new Exception("Can't create generic definition {0}.");
				if(typeof(Delegate).IsAssignableFrom(type)) throw new Exception("Can't create delegete {0}.");

				if(type.IsArray) return type.GetElementType().IsCreatable(exception);
				if(!type.IsGenericType) return true;

				var _d = type.GetGenericTypeDefinition();
				if(typeof(List<>) != _d && typeof(Dictionary<,>) != _d) return true;
				else return type.GetGenericArguments().All(_v => _v.IsCreatable(exception));

			}catch(Exception e){

				if(!exception) return false;
				throw new ArgumentException(string.Format(e.Message, type.GetPrettyName(true)), "type");

			}

		}

	}

}
