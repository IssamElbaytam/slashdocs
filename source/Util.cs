/*
Copyright (c) 2005-2010 Scott Willeke (scott@willeke.com)
Licensed under the MIT license: http://www.opensource.org/licenses/mit-license.php
*/
using System;

namespace PingPoet.SlashDocs
{
	/// <summary>
	/// Provides misc utility functions.
	/// </summary>
	public sealed class Util
	{
		private Util()
		{
		}

		/// <summary>
		/// Returns true if the specified value is null or empty.
		/// </summary>
		public static bool IsNullOrEmpty(string value)
		{
			return value == null || value.Length == 0;
		}
	}
}
