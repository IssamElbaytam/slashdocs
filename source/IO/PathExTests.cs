/*
Copyright (c) 2005-2010 Scott Willeke (scott@willeke.com)
Licensed under the MIT license: http://www.opensource.org/licenses/mit-license.php
*/
using System;
using NUnit.Framework;

namespace PingPoet.SlashDocs.IO
{
	[TestFixture]
	public class PathExTests
	{
		
		[Test]
		public void GetFullPathRelativeTo()
		{
			string relativeTo, relative, expected;

			relativeTo = @"d:\dir1\dir2\dir3";
			relative = @"..\image1.png";
			expected = @"d:\dir1\dir2\image1.png";
			AssertGetFullPathRelativeTo(expected, relative, relativeTo);

			relativeTo = @"d:\dir1\dir2\dir3";
			relative = @"../image1.png";
			expected = @"d:\dir1\dir2\image1.png";
			AssertGetFullPathRelativeTo(expected, relative, relativeTo);

			relativeTo = @"d:\dir1\dir2\dir3";
			relative = @"../image1";
			expected = @"d:\dir1\dir2\image1";
			AssertGetFullPathRelativeTo(expected, relative, relativeTo);
		}

		[Test]
		public void GetFullPathRelativeTo2()
		{
			string relativeTo, relative, expected;

			relativeTo = @"d:\a\a\a";
			relative = @"../b";
			expected = @"d:\a\a\b";
			AssertGetFullPathRelativeTo(expected, relative, relativeTo);

			relativeTo = @"d:\a\a\a";
			relative = @"../a";
			expected = @"d:\a\a\a";
			AssertGetFullPathRelativeTo(expected, relative, relativeTo);

			relativeTo = @"d:\a\a\a";
			relative = @"../../../";
			expected = @"d:\";
			AssertGetFullPathRelativeTo(expected, relative, relativeTo);

			relativeTo = @"d:\a\a\a";
			relative = @"../../../p.png";
			expected = @"d:\p.png";
			AssertGetFullPathRelativeTo(expected, relative, relativeTo);
		}

		[Test]
		public void GetFullPathRelativeTo3()
		{
			string relativeTo, relative, expected;

			relativeTo = @"d:\a\a";
			relative = @"b";
			expected = @"d:\a\a\b";
			AssertGetFullPathRelativeTo(expected, relative, relativeTo);
		}

		private void AssertGetFullPathRelativeTo(string expected, string relative, string relativeTo)
		{
			string actual = PathEx.GetFullPathRelativeTo(relative, relativeTo);
			Assert.AreEqual(expected, actual);
		}


		[Test]
		public void GetPathRelativeTo()
		{
			string expected, path, relativeTo;

			path = @"C:\test\aaa\bbb";
			relativeTo = @"C:\test\ccc";
			expected = @"..\aaa\bbb";
			AssertGetPathRelativeTo(expected, path, relativeTo);

			path = @"C:\test\aaa\bbb\";
			relativeTo = @"C:\test\ccc\";
			expected = @"..\aaa\bbb\";
			AssertGetPathRelativeTo(expected, path, relativeTo);

			path = @"C:\test\aaa\bbb";
			relativeTo = @"C:\test\ccc\";
			expected = @"..\aaa\bbb";
			AssertGetPathRelativeTo(expected, path, relativeTo);
			
			path = @"C:\test\aaa\bbb\";
			relativeTo = @"C:\test\ccc";
			expected = @"..\aaa\bbb\";
			AssertGetPathRelativeTo(expected, path, relativeTo);

			relativeTo = @"C:\project\source\asm1";
			path = @"C:\project\docs\asm1";
			expected = @"..\..\docs\asm1";
			AssertGetPathRelativeTo(expected, path, relativeTo);

			

			//C:\Projects\stryker\Source\Imports\Access
			//C:\Projects\stryker\Source\Imports\Access\SlashDocs\ReportGeneration.slashdoc
			relativeTo =	@"C:\Projects\stryker\Source\Imports\Access";
			path =			@"C:\Projects\stryker\Source\Imports\Access\SlashDocs\ReportGeneration.slashdoc";
			expected = @"SlashDocs\ReportGeneration.slashdoc";
			AssertGetPathRelativeTo(expected, path, relativeTo);
		}

		[Test]
		public void GetPathRelativeToWithFile()
		{
			string expected, path, relativeTo;

			relativeTo = @"C:\project\source\asm1";
			path = @"C:\project\docs\asm1\file.myext";
			expected = @"..\..\docs\asm1\file.myext";
			AssertGetPathRelativeTo(expected, path, relativeTo);

			relativeTo = @"C:\project\source\asm1\";
			path = @"C:\project\docs\asm1\file.myext";
			expected = @"..\..\docs\asm1\file.myext";
			AssertGetPathRelativeTo(expected, path, relativeTo);
		}

		[Test]
		public void GetPathRelativeToUncommonRoot()
		{
			string expected, path, relativeTo;
			relativeTo = @"C:\project\source\asm1";
			path = @"D:\project\docs\asm1";
			expected = @"D:\project\docs\asm1";

			AssertGetPathRelativeTo(expected, path, relativeTo);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void GetPathRelativeToBadArg1Empty()
		{
			string expected, path, relativeTo;
			relativeTo = @"C:\project\source\asm1";
			path = @"";
			expected = @"FAILFAILFAILFAILFAIL";
			AssertGetPathRelativeTo(expected, path, relativeTo);
		}
		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void GetPathRelativeToBadArg1Null()
		{
			string expected, path, relativeTo;
			relativeTo = @"C:\project\source\asm1";
			path = null;
			expected = @"FAILFAILFAILFAILFAIL";
			AssertGetPathRelativeTo(expected, path, relativeTo);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void GetPathRelativeToBadArg2Empty()
		{
			string expected, path, relativeTo;
			relativeTo = @"";
			path = @"D:\project\docs\asm1";
			expected = @"FAILFAILFAILFAILFAIL";
			AssertGetPathRelativeTo(expected, path, relativeTo);
		}
		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void GetPathRelativeToBadArg2Null()
		{
			string expected, path, relativeTo;
			relativeTo = null;
			path = @"D:\project\docs\asm1";
			expected = @"FAILFAILFAILFAILFAIL";
			AssertGetPathRelativeTo(expected, path, relativeTo);
		}

		private void AssertGetPathRelativeTo(string expected, string path, string relativeTo)
		{
			string actual = PathEx.GetPathRelativeTo(path, relativeTo);
			Assert.AreEqual(expected, actual);
		}
	}
}