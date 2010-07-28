/*
Copyright (c) 2005-2010 Scott Willeke (scott@willeke.com)
Licensed under the MIT license: http://www.opensource.org/licenses/mit-license.php
*/
using System;

namespace PingPoet.SlashDocs
{
	/// <summary>
	/// Provides resources for the addin.
	/// </summary>
	internal sealed class Resources
	{
		private Resources()
		{
		}

		

		public static string GetString(StringID id)
		{
			switch (id)
			{
				case StringID.OpenSlashDocFileCaption:
					return "&Open SlashDoc File";
				case StringID.OpenSlashDocFileToolTip:
					return "&Opens the external SlashDoc documentation file for the current method.";
				default:
					Diagnostics.DiagnosticService.TraceError("String resource with id '{0}' was not found.", id);
					return "";
			}
		}
	}

	public enum StringID
	{
		OpenSlashDocFileCaption,
		OpenSlashDocFileToolTip
	}
}