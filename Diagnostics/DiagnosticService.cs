/*
Copyright (c) 2005-2010 Scott Willeke (scott@willeke.com)
Licensed under the MIT license: http://www.opensource.org/licenses/mit-license.php
*/
using System;
using System.Diagnostics;

namespace PingPoet.SlashDocs.Diagnostics
{
	/// <summary>
	/// Provides diagnostic and tracing services.
	/// </summary>
	internal sealed class DiagnosticService
	{
		private DiagnosticService()
		{
		}
		
		private static void TraceErrorCore(string callerName, string message, params object[] args)
		{
			string msg = string.Format(message, args);
			Debug.WriteLine(callerName + ": " + msg);
		}

		public static void TraceError(string message, params object[] args)
		{
			TraceErrorCore(GetCallerName(), message, args);
		}

		public static void TraceInfo(string message, params object[] args)
		{
			string msg = string.Format(message, args);
			Debug.WriteLine(GetCallerName() + ": " + msg);
		}

		private static string GetCallerName()
		{
			StackFrame frame = new StackFrame(2);
			return frame.GetMethod().ReflectedType.Name + '.' + frame.GetMethod().Name;
		}

		public static void Assert(bool condition, string message)
		{
			Assert(condition, message, new object[0]);
			
		}
		public static void Assert(bool condition, string message, params object[] args)
		{
			if (!condition)
			{
				string callerName = GetCallerName();
				if (args != null && args.Length > 0)
					message = string.Format(message, args);
				Debug.Assert(false, callerName + ": " + message);
				TraceErrorCore(callerName, message);
			}
		}
	}
}