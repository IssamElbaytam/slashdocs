/*
Copyright (c) 2005-2010 Scott Willeke (scott@willeke.com)
Licensed under the MIT license: http://www.opensource.org/licenses/mit-license.php
*/
using System;
using System.IO;
using System.Text;
using PingPoet.SlashDocs.Diagnostics;

namespace PingPoet.SlashDocs.IO
{
	/// <summary>
	/// Provides utility functions for working with Paths.
	/// </summary>
	internal sealed class PathEx
	{
		private PathEx()
		{
		}

		/// <summary>
		/// Returns an array of the characters that can be used as directory separator characters.
		/// </summary>
		public static char[] DirectorySeparatorChars
		{
			get
			{
				return new char[]{Path.DirectorySeparatorChar,  Path.AltDirectorySeparatorChar};
			}
		}

		/// <summary>
		/// Resolves a relative path relative to a specified path.
		/// </summary>
		/// <param name="relativePath">The relative path.</param>
		/// <param name="relativeTo">The path that the relative path is relative to.</param>
		/// <returns>A fully qualified path relative to the specified path.</returns>
		public static string GetFullPathRelativeTo(string relativePath, string relativeTo)
		{
			if (Path.IsPathRooted(relativePath))
				return relativePath;

			string relativeDirectory = relativePath;
			DirectoryInfo relativeToDirectory = new DirectoryInfo(relativeTo);

			// is it a rooted relative path?
			if (relativeDirectory[0] == Path.DirectorySeparatorChar || relativeDirectory[0] == Path.AltDirectorySeparatorChar)
			{
				relativeToDirectory = relativeToDirectory.Root;
			}
			else 
			{
				// does it begin with one or more pairs of parent path indicators "../"?
				string pathWithoutParentPathIndicators;
				int parentPathCount = CountParentPathIndicators(relativeDirectory, out pathWithoutParentPathIndicators);
				if (parentPathCount > 0)
				{
					relativeDirectory = pathWithoutParentPathIndicators;
					while (parentPathCount > 0)
					{
						relativeToDirectory = relativeToDirectory.Parent;
						parentPathCount--;
					}
				}
			}
			return Path.Combine(relativeToDirectory.FullName, relativeDirectory);
		}

		/// <summary>
		/// Returns the number of parent path indicators (such as "../" or "..\").
		/// </summary>
		/// <param name="path">The path to search for parent path indicators in.</param>
		/// <returns>The number of parent path indicators.</returns>
		private static int CountParentPathIndicators(string path, out string pathWithoutParentPathIndicators)
		{
			int indicatorCount = 0;
			path = path.TrimStart();
			int len = path.Length;
			int pos = 0;
			
			pathWithoutParentPathIndicators = path;
			while (pos+2 < len)
			{
				if (pos < len && path[pos] == '.'
					&& pos+1 < len && path[pos+1] == '.'
					&& pos+2 < len && IsDirectorySeparatorChar(path[pos+2])
					)
				{
					indicatorCount++;
					pos += 3;
					pathWithoutParentPathIndicators = path.Substring(pos);
				}
				else
					break;
			}
			return indicatorCount;
		}

		/// <summary>
		/// Returns a relative path for the specified path that is relative to the specified base path.
		/// </summary>
		/// <param name="path">The path to get a relative path for.</param>
		/// <param name="relativeTo">The path that the returned path should be relative to.</param>
		/// <returns>A path relative to the specified <paramref cref="relativeTo"/> if possible, or a fully qualified path.</returns>
		public static string GetPathRelativeTo(string path, string relativeTo)
		{
			if (path == null)
				throw new ArgumentNullException("path");
			if (path.Length == 0)
				throw new ArgumentException("Empty argument", "path");
			if (relativeTo == null)
				throw new ArgumentNullException("relativeTo");
			if (relativeTo.Length == 0)
				throw new ArgumentException("Empty argument", "relativeTo");

			//do they have the same root? If not, return a simple qualified path.
			if (!string.Equals(Path.GetPathRoot(path), Path.GetPathRoot(relativeTo)))
				return path;
			
			
			// find a common base between path & relativePath
			int minLength = Math.Min(path.Length, relativeTo.Length);
			int lengthOfCommonBase=-1;
			for (int position = 0; position < minLength; position++)
			{
				if (path[position] == relativeTo[position])
				{
					if (IsDirectorySeparatorChar(path[position]))
						lengthOfCommonBase = position;
					else if  (relativeTo.Length -1 == position) // path may be a subdirectory of the relativeTo directory, this catches that case
						lengthOfCommonBase = position + 1;
					continue;
				}
				else
				{	// char at this pos is not equal
					DiagnosticService.Assert(lengthOfCommonBase >= 0, "expected at least equal roots");
                    break;//common base is: path.Substring(0, lengthOfCommonBase);
				}
			}

			// count the number of directories between the common base and the relativeTo path (if any)
			int countParentDirectories = 0;
			int lastDirectorySeparator=lengthOfCommonBase;
			for (int currentPosition = lengthOfCommonBase+1; currentPosition < relativeTo.Length; currentPosition++)
			{
				// if the current character is a directory separator character or it's the last character in the string
				//    and there are characters between this character and the last directorySeparator then count a parent directory
				if  ( (IsDirectorySeparatorChar(relativeTo[currentPosition]) || currentPosition == relativeTo.Length-1)
					&& currentPosition > lastDirectorySeparator)
				{
					countParentDirectories++;
				}
			}
			
			// generate this many 'parentPath' indicators ("..\")
			StringBuilder newRelativePath = new StringBuilder();
			for (int parentNum=0; parentNum < countParentDirectories; parentNum++)
			{
				newRelativePath.Append("..").Append(Path.DirectorySeparatorChar);
			}
            
			//append the portion of the specified path after the common base
			newRelativePath.Append(path.Substring(lengthOfCommonBase+1));
			return newRelativePath.ToString();
		}

		/// <summary>
		/// Determines if the specified character is a directory seperator character.
		/// </summary>
		public static bool IsDirectorySeparatorChar(char character)
		{
			return (character == Path.DirectorySeparatorChar || character == Path.AltDirectorySeparatorChar);
		}

	}
}