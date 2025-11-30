using System;

namespace Swing
{
	public static class StringExtensions
	{
#if SUPPORT_RESHARPER
		[JetBrains.Annotations.StringFormatMethod("_this")]
#endif
		public static string format( this string _this, params object[] _args )
		{
			return string.Format(_this, _args);
		}

		public static bool isNullOrEmpty( this string _this )
		{
			return string.IsNullOrEmpty(_this);
		}

		public static string removeStart( this string _this, string _prefix,
		                                  StringComparison _comparisonType = StringComparison.CurrentCulture )
		{
			return _this.StartsWith(_prefix, _comparisonType) ? _this.Substring(_prefix.Length) : _this;
		}
		public static string removeEnd( this string _this, string _suffix,
		                                StringComparison _comparisonType = StringComparison.CurrentCulture )
		{
			return _this.EndsWith(_suffix, _comparisonType) ? _this.Substring(0, _this.Length - _suffix.Length) : _this;
		}
	}
}
