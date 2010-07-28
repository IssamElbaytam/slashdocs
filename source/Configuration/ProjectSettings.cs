/*
Copyright (c) 2005-2010 Scott Willeke (scott@willeke.com)
Licensed under the MIT license: http://www.opensource.org/licenses/mit-license.php
*/
using System;
using System.Diagnostics;
using System.IO;

namespace PingPoet.SlashDocs.Configuration
{
	/// <summary>
	/// Utility class to help save and load project specific settings.
	/// </summary>
	internal class ProjectSettings
	{
		const string VarName_SlashDocPathMatchPattern = "PingPoet_SlashDocs_SlashDocPathMatchPattern";
		const string VarName_SlashDocPathReplacePattern = "PingPoet_SlashDocs_SlashDocPathReplacePattern";
		const string VarName_DocCommentIncludeFileBasePath = "PingPoet_SlashDocs_DocCommentIncludeFileBasePath";
		private readonly EnvDTE.Project _project;

		public ProjectSettings(EnvDTE.Project project)
		{
			if (project == null)
				throw new ArgumentNullException("project");
			_project = project;
		}

		/// <summary>
		/// A regular expression used to match the portions of the source code file name+path that are used for generating the slashdoc file name+path.
		/// <seealso cref="SlashDocPathReplacePattern"/>
		/// </summary>
		public string SlashDocPathMatchPattern
		{
			get
			{
				if (!ProjectVariableExists(VarName_SlashDocPathMatchPattern))
				{
					//provide a default: 
					string projectPath = Path.GetDirectoryName(_project.FullName);
					string regexsafeProjectPath = projectPath.Replace(@"\", @"\\");
					//eg: ^C:\\projects\\stryker\\Source\\([a-zA-Z\\0-9 ,-_]+)\\IDataProvidersManager.cs$ 
					//'"', '<', '>', '|', '\0', '\b', '\x0010', '\x0011', '\x0012', '\x0014', '\x0015', '\x0016', '\x0017', '\x0018', '\x0019'
					//0 (zero) through 31
					//				const string charsZeroThroughThirtyOne = "\x0000\x0001\x0002\x0003\x0004\x0005\x0006\x0007\x0008\x0009"
					//						+ "\x0010\x0011\x0012\x0013\x0014\x0015\x0016\x0017\x0018\x0019"
					//						+ "\x0020\x0021\x0022\x0023\x0024\x0025\x0026\x0027\x0028\x0029"
					//						+ "\x0030\x0031";
					const string charsZeroThroughThirtyOne  ="";
					const string invalidPathCharsRegexSafe = charsZeroThroughThirtyOne + "<>:\"/\\\\|";
					const string regexMatchTrailingFilename = "(?<filename>[^" + invalidPathCharsRegexSafe + @"]+)\.cs$";
					//const string matchFileName = ;
					string docPathMatchPattern = "^" + regexsafeProjectPath + @"(?<path>(\\[a-zA-Z\\0-9 ,-_]+)*\\)" + regexMatchTrailingFilename;	
					return docPathMatchPattern;
				}
				object value = _project.Globals[VarName_SlashDocPathMatchPattern];
				return Convert.ToString(value);
			}
			set
			{
				_project.Globals[VarName_SlashDocPathMatchPattern] = value;
				_project.Globals.set_VariablePersists(VarName_SlashDocPathMatchPattern, true);
			}
		}

		private bool ProjectVariableExists(string variableName)
		{
			bool variableExists;
			try
			{
				variableExists = _project.Globals.get_VariableExists(variableName);
			}
			catch (Exception e)
			{	//vs2005 started throwing exceptions on me now:
				Debug.WriteLine("Error checking for project variable:" + e.ToString());
				variableExists = false;//??
			}
			return variableExists;
		}

		/// <summary>
		/// A regular expression used as a replacement expression used to generate the slashdoc file name+path.
		/// Used in conjunction with <see cref="SlashDocPathMatchPattern"/>
		/// <see cref="SlashDocPathMatchPattern"/>
		/// </summary>
		public string SlashDocPathReplacePattern
		{
			get
			{
				if (!ProjectVariableExists(VarName_SlashDocPathReplacePattern))
				{
					// provide a default:
					string projectPath = Path.GetDirectoryName(_project.FullName);
					string docPathReplacePattern = projectPath + @"\" + @"SlashDocs" + @"${path}${filename}.slashdoc";
					return docPathReplacePattern;
				}
				object value = _project.Globals[VarName_SlashDocPathReplacePattern];
				return Convert.ToString(value);
			}
			set
			{
				_project.Globals[VarName_SlashDocPathReplacePattern] = value;
				_project.Globals.set_VariablePersists(VarName_SlashDocPathReplacePattern, true);
			}
		}

		/// <summary>
		/// The base path that slashdoc file name & path is relative to when generating the path in include statements.
		/// </summary>
		public string DocCommentIncludeFileBasePath
		{
			get
			{
				if (!ProjectVariableExists(VarName_DocCommentIncludeFileBasePath))
					return Path.GetDirectoryName(_project.FullName);
				object value = _project.Globals[VarName_DocCommentIncludeFileBasePath];
				return Convert.ToString(value);
			}
			set
			{
				_project.Globals[VarName_DocCommentIncludeFileBasePath] = value;
				_project.Globals.set_VariablePersists(VarName_DocCommentIncludeFileBasePath, true);
			}
		}
	}
}