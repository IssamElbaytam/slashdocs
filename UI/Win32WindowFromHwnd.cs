/*
Copyright (c) 2005-2010 Scott Willeke (scott@willeke.com)
Licensed under the MIT license: http://www.opensource.org/licenses/mit-license.php
*/
using System;
using System.Windows.Forms;

namespace PingPoet.SlashDocs.UI
{
	/// <summary>
	/// Provides a <see cref="IWin32Window"/> from a specified hwnd.
	/// </summary>
	public sealed class Win32WindowFromHwnd : IWin32Window
	{
		private IntPtr _hWnd;

		public Win32WindowFromHwnd(Int32 hWnd)
		{
			_hWnd = new IntPtr(hWnd);
		}

		public Win32WindowFromHwnd(IntPtr hWnd)
		{
			_hWnd = hWnd;
		}

		public IntPtr Handle
		{
			get { return _hWnd; }
		}
	}
}
