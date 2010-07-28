/*
Copyright (c) 2005-2010 Scott Willeke (scott@willeke.com)
Licensed under the MIT license: http://www.opensource.org/licenses/mit-license.php
*/
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using EnvDTE;
using Microsoft.VisualStudio.CommandBars;
using PingPoet.SlashDocs.Configuration;
using PingPoet.SlashDocs.Diagnostics;
using PingPoet.SlashDocs.DteTools;
using PingPoet.SlashDocs.IO;
using PingPoet.SlashDocs.UI;
using PingPoet.SlashDocs.Xml;

namespace PingPoet.SlashDocs.Commands
{
	/// <summary>
	/// Implements a command opens an external slashdoc file based on the active selection in a text editor.
	/// </summary>
	internal sealed class OpenSlashDocFileCommand : AddInCommand
	{
		public OpenSlashDocFileCommand(DTE application, AddIn owner)
			:base(application, owner, Resources.GetString(StringID.OpenSlashDocFileCaption), Resources.GetString(StringID.OpenSlashDocFileToolTip))
		{
			base.IconID = 101;
			base.IconIsCustom = true;
		}

		public override EnvDTE.Command Install()
		{
			EnvDTE.Command cmd = base.Install();
			CommandBars commandBars = (CommandBars)Application.CommandBars;
			CommandBar bar = commandBars["Code Window"];
			if (bar == null)
			{
				Diagnostics.DiagnosticService.TraceError("Code Window CommandBar not available.");
				Debug.Fail("Cannot find proper command bar!");
				return cmd;
			}
			cmd.AddControl(bar, 1);
			return cmd;
		}

		#region private helper methods
		private bool IsCodeWindowActive()
		{
			return 
				Application.ActiveWindow.Type == vsWindowType.vsWindowTypeDocument
				&& Application.ActiveWindow.Document.Language == "CSharp";
		}


		private TextSelection GetActiveTextSelection()
		{
			TextSelection sel = null;
			Window window = Application.ActiveWindow;
			return GetTextSelectionFromWindow(window, sel);
		}

		private static TextSelection GetTextSelectionFromWindow(Window window, TextSelection sel)
		{
			try
			{
				sel = window.Selection as TextSelection;
			}
			catch (Exception eSelection)
			{
				DiagnosticService.TraceError("An error occured accessing Active window selection unavailable. Exception:'{0}'", eSelection.ToString());
			}
			return sel;
		}

		private CodeElement GetActiveCodeElement()
		{
			TextSelection selection = GetActiveTextSelection();
			if (selection == null)
				return null;
 
			CodeElement codeElement = null;
			try
			{
				/* FYI: If the specified code element type is not at the EditPoint/TextPoint 
				 * location, then CodeElement returns null. So we check for multiple different 
				 * types of code elements. CodeElement is a shortcut for 
				 * TextPoint.Parent.Parent.ProjectItem.CodeModel.CodeElementFromPoint(TextPoint, <scope>)
				 */ 
				codeElement = selection.ActivePoint.get_CodeElement(vsCMElement.vsCMElementFunction);
				if (codeElement == null)
					codeElement = selection.ActivePoint.get_CodeElement(vsCMElement.vsCMElementProperty);
				if (codeElement == null)
					codeElement = selection.ActivePoint.get_CodeElement(vsCMElement.vsCMElementClass);
				if (codeElement == null)
					codeElement = selection.ActivePoint.get_CodeElement(vsCMElement.vsCMElementInterface);
				if (codeElement == null)
					codeElement = selection.ActivePoint.get_CodeElement(vsCMElement.vsCMElementStruct);
				if (codeElement == null)
					codeElement = selection.ActivePoint.get_CodeElement(vsCMElement.vsCMElementEnum);
			}
			catch (Exception eCodeElement)
			{
				DiagnosticService.TraceError("An error occured accessing selection's code element: '{0}'", eCodeElement);
			}
			return codeElement;
		}

		#endregion

		public override bool Execute(EnvDTE.vsCommandExecOption executeOption)
		{
			if (!IsCodeWindowActive())
				return false;
			
			CodeElement codeElement = GetActiveCodeElement();
			if (codeElement == null)
			{
				DiagnosticService.TraceError("No code element for current cursor position.");
				return false;
			}

			EnvDTE.Project project = Application.ActiveDocument.ProjectItem.ContainingProject;
			ProjectSettings projectSettings = new ProjectSettings(project);

			string codeElementKindString = Enum.GetName(typeof(vsCMElement), codeElement.Kind);
			DiagnosticService.TraceInfo("Active CodeElemnet Name:'{0}' kind:'{1}'.", codeElement.FullName, codeElementKindString);
			

			string sourceCodeFullFilePath = Application.ActiveDocument.FullName;
			string codeFileName = Path.GetFileName(sourceCodeFullFilePath);

			string slashDocFileFullPathName;
			string slashDocXPath;
			#region Get the SlashDoc File
			// does it contain an <include file='...' path='...' statement?
			bool isIncludeStatementPresent = GetSlashDocFromCodeElementDocCommentInclude(codeElement, out slashDocFileFullPathName, out slashDocXPath);
			// see if the include file exists...
			if (!isIncludeStatementPresent || !File.Exists(slashDocFileFullPathName))
			{
				IWin32Window owner = new Win32WindowFromHwnd(this.Application.MainWindow.HWnd);
				string docCommentIncludeFileBasePath = projectSettings.DocCommentIncludeFileBasePath;
				FileNamingDialog slashDocNamingPrompt = new FileNamingDialog(sourceCodeFullFilePath, docCommentIncludeFileBasePath, projectSettings.SlashDocPathMatchPattern, projectSettings.SlashDocPathReplacePattern, codeElement, new GenDocFilePathFromCodeFilePathHandler(this.GenDocFilePathFromCodeFilePath));
				try
				{
					DialogResult result = slashDocNamingPrompt.ShowDialog(owner);
					if (result == DialogResult.OK)
					{
						projectSettings.SlashDocPathMatchPattern = slashDocNamingPrompt.MatchExpression;
						projectSettings.SlashDocPathReplacePattern = slashDocNamingPrompt.ReplaceExpression;
						projectSettings.DocCommentIncludeFileBasePath = slashDocNamingPrompt.DocCommentIncludeFileBasePath;
						slashDocFileFullPathName = slashDocNamingPrompt.SlashdocFilePath;
						//create the file as an empty xml file.
						if (!File.Exists(slashDocFileFullPathName))
						{
							XmlDocument templateDoc = GetDefaultTemplate();
							string directoryPart = Path.GetDirectoryName(slashDocFileFullPathName);
							if (!Directory.Exists(directoryPart))
								Directory.CreateDirectory(directoryPart);
							//TODO: load slashdoc template default from config file or something.
							templateDoc.Save(slashDocFileFullPathName);
						}
					}
					else
						return true;
				}
				finally
				{
					if (slashDocNamingPrompt != null)
						slashDocNamingPrompt.Close();
					slashDocNamingPrompt = null;
				}
			}
			#endregion

			#region Get the Line Info of the element or generate the new slashdoc content:
			int lineNumber, linePosition;

			if (slashDocXPath == null || slashDocXPath.Length == 0)
				slashDocXPath = CodeModelTools.GetXPathForCodeElement(codeElement);
			bool isXmlElementInSlashDocFileFound = XmlTools.GetTextLocationOfXmlNode(slashDocFileFullPathName, slashDocXPath, out lineNumber, out linePosition);
			
			if (!isXmlElementInSlashDocFileFound)
			{
				//create element
				XmlDocument targetDoc = new XmlDocument();
				targetDoc.Load(slashDocFileFullPathName);
				// Copy the CodeElement's documentation into it
				CreateSlashDocForCodeElement(targetDoc, codeElement);
				XmlTextWriter formattedWriter = new XmlTextWriter(slashDocFileFullPathName, Encoding.UTF8);
				try					
				{
					formattedWriter.Formatting=Formatting.Indented;
					targetDoc.Save(formattedWriter);
					targetDoc = null;
				}
				finally
				{
					if (formattedWriter != null)
					{
						formattedWriter.Close();
						formattedWriter =null;
					}
				}
			}
			if (!isIncludeStatementPresent)
			{
				//Replace the CodeElement's documentation in source code with <include... />
				string includeStatement = CreateDocCommentIncludeElement(slashDocFileFullPathName, projectSettings.DocCommentIncludeFileBasePath, codeElement);
				string docComment = includeStatement;

				// if there is docs for this member in the target slashdoc file + docs in the sourcode + no include (or we wouldn't have got this far): 
				//    Be careful to not overwrite existing docs in the source code:
				if (isXmlElementInSlashDocFileFound)
				{
					bool hasExistingDocsInSourceCode = !Util.IsNullOrEmpty(CodeModelTools.GetContentFromDocComment(CodeModelTools.DocCommentFromCodeElement(codeElement)));
					if (hasExistingDocsInSourceCode)
					{
						// append existing docs
						docComment += "\r\n" + CodeModelTools.GetContentFromDocComment(CodeModelTools.DocCommentFromCodeElement(codeElement));
					}
					/* NOTE: Wicked Visual Studio puts an artifical root element of <doc> in the 
					 * response *to* and from the code element. It does not appear in the actual text 
					 * editor. Apparently this is to be able to deal with a well-formed rooted XML doc 
					 * and not deal with fragments. As of VS2010 this was still the behavior. 
					 * 
					 * Thus here, we wrap the specified comment in the <doc></doc> elemnet.
					 */
					docComment = "<doc>" + docComment + "</doc>";
				}
				CodeModelTools.SetDocComment(codeElement, docComment);
				//Get the line info for the code element
				isXmlElementInSlashDocFileFound = XmlTools.GetTextLocationOfXmlNode(slashDocFileFullPathName, slashDocXPath, out lineNumber, out linePosition);
			}
			else
			{
				// Element found, but aparently the include statement does not exist or it's contained xpath is invalid. Not doing anything for this case now since it's an unusual use case.
			}
			#endregion
			
			Window slashDocWindow = Application.ItemOperations.OpenFile(slashDocFileFullPathName, Constants.vsViewKindPrimary);
			slashDocWindow.Activate();
			TextSelection textSelection = slashDocWindow.Document.Selection as TextSelection;
			if (textSelection != null && (lineNumber > 0 && linePosition > 0))
			{
				textSelection.MoveToLineAndOffset(lineNumber, linePosition, false);
				textSelection.TopPoint.TryToShow(vsPaneShowHow.vsPaneShowCentered,  null);
			}

			// move the selection to the lineNumber/linePosition.
			return true;
		}


		/// <summary>
		/// Creates a doccomment corresponding to the specified codeelement in the provided document.
		/// </summary>
		/// <param name="targetDoc">The document to add the slashdoc to.</param>
		/// <param name="codeElement">The CodeElement to create a doc comment for.</param>
		private void CreateSlashDocForCodeElement(XmlDocument targetDoc, CodeElement codeElement)
		{
			if (targetDoc == null)
				throw new ArgumentNullException("targetDoc");
			if (codeElement == null)
				throw new ArgumentNullException("codeElement");
			
			//NOTE: Using static slashdoc schema. It would be nice to make it support a customizable schema.
			// get/create  /Libraries
			XmlNode librariesNode;
			librariesNode= targetDoc.SelectSingleNode("/Libraries");
			if (librariesNode == null)
			{
				targetDoc.CreateElement("Libraries");
				targetDoc.AppendChild(librariesNode);
			}
			DiagnosticService.Assert(librariesNode != null, "Libraries node not initialized");
			// /Libraries/Types
			XmlNode typesNode = targetDoc.SelectSingleNode("/Libraries/Types");
			if (typesNode == null)
			{
				typesNode = targetDoc.CreateElement("Types");
				librariesNode.AppendChild(typesNode);
			}
			DiagnosticService.Assert(typesNode != null, "Types node not initialized");
			// /Libraries/Types/Type name=<ThisType>
			string namespaceName, memberName, typeName;
			CodeModelTools.GetMemberAndTypeFromCodeElement(codeElement, out typeName, out memberName, out namespaceName);
			XmlNode typeNode = typesNode.SelectSingleNode(string.Format("Type[@name=\"{0}\"]", typeName));
			if (typeNode == null)
			{
				typeNode = targetDoc.CreateElement("Type");
				XmlAttribute attrib = targetDoc.CreateAttribute("name");
				attrib.Value = typeName;
				typeNode.Attributes.Append(attrib);
				typesNode.AppendChild(typeNode);
			}
			DiagnosticService.Assert(typeNode != null, "Type node not initialized");

		
			XmlNode docsNode;
			// is this a member?
			if (memberName != null && memberName.Length > 0)
			{
				XmlNode membersNode = typeNode.SelectSingleNode("Members");
				if (membersNode == null)
				{
					membersNode = targetDoc.CreateElement("Members");
					typeNode.AppendChild(membersNode);
				}
				DiagnosticService.Assert(membersNode != null, "members node not initialized");
				XmlNode memberNode = membersNode.SelectSingleNode(string.Format("Member[@name=\"{0}\"]", memberName));
				if (memberNode == null)
				{
					memberNode = targetDoc.CreateElement("Member");
					XmlAttribute attrib = targetDoc.CreateAttribute("name");
					attrib.Value = memberName;
					memberNode.Attributes.Append(attrib);
					membersNode.AppendChild(memberNode);
				}
				DiagnosticService.Assert(memberNode != null, "memberNode not initialized");
				docsNode = memberNode.SelectSingleNode("Docs");
				if (docsNode == null)
				{
					docsNode = targetDoc.CreateElement("Docs");
					memberNode.AppendChild(docsNode);
				}
			}
			else
			{
				// it's only a Type, not a member
				docsNode = targetDoc.CreateElement("Docs");
				typeNode.AppendChild(docsNode);
			}
			DiagnosticService.Assert(docsNode != null, "doc node not initialized");
			// copy the existing documentation to the slashdoc file
			string existingDocComment = CodeModelTools.DocCommentFromCodeElement(codeElement);
			// the root element is <doc>, we want it's inner xml
			string existingXmlDocsToCopy = CodeModelTools.GetContentFromDocComment(existingDocComment);
			if (!Util.IsNullOrEmpty(existingXmlDocsToCopy))
				docsNode.InnerXml = existingXmlDocsToCopy;
			else
				docsNode.InnerText = " ";//this prevents a <Docs />
		}

		/// <summary>
		/// Creates a &lt;include...&gt; element to be used as a doc comment for the specified code element.
		/// </summary>
		/// <param name="slashdocFilePath">The fully qualified path to the slashdoc file to be included.</param>
		/// <param name="docCommentIncludeFileBasePath">The path that the include attribute's relative path value will be relative to.</param>
		/// <param name="codeElement">The <see cref="CodeElement"/> that the include statement is for.</param>
		/// <returns>A string containing the complete include element.</returns>
		internal static string CreateDocCommentIncludeElement(string slashdocFilePath, string docCommentIncludeFileBasePath, CodeElement codeElement)
		{
			if (slashdocFilePath == null || slashdocFilePath.Length == 0)
				throw new ArgumentNullException("slashdocFilePath");
			string fileAttributeValue = PathEx.GetPathRelativeTo(slashdocFilePath, docCommentIncludeFileBasePath );
			string pathAttributeValue = CodeModelTools.GetXPathForCodeElement(codeElement);
			return "<include file='" + fileAttributeValue + "' path='" + pathAttributeValue + "' />";
		}

		/// <summary>
		/// Gets the slashdoc filename and xpath from the specified codeElementDocComment.
		/// </summary>
		/// <param name="codeElement">The CodeElement to get the documentation comment from.</param>
		/// <param name="slashDocXPath">If the return value is true, the xpath statement found directly from the include statement.</param>
		/// <returns>True if an &lt;include&gt; statement was found, false otherwise.</returns>
		private static bool GetSlashDocFromCodeElementDocCommentInclude(CodeElement codeElement, out string slashdocFullPathName, out string slashDocXPath)
		{
			string codeElementDocComment = CodeModelTools.DocCommentFromCodeElement(codeElement);
			if (codeElementDocComment == null || codeElementDocComment.Length == 0)
			{
				slashdocFullPathName = "";
				slashDocXPath = "";
				return false;
			}

			XmlDocument xmlDocComment = new XmlDocument();
			try
			{
				xmlDocComment.LoadXml(codeElementDocComment);
			}
			catch (XmlException eDocCommentXml)
			{
				DiagnosticService.TraceError("Error parsing comment. Error:'{0}'", eDocCommentXml.Message);
				slashdocFullPathName = "";
				slashDocXPath = "";
				return false;
			}
			// get relative filename+path & xpath from <include ...>:
			XmlNode fileAttribute = xmlDocComment.SelectSingleNode("/doc/include/@file");
			XmlNode pathAttribute = xmlDocComment.SelectSingleNode("/doc/include/@path");
			if (fileAttribute == null || pathAttribute == null)
			{
				slashDocXPath = "";
				slashdocFullPathName = "";
				return false;
			}
			//TODO: change the relative file to a qualified file:
			string relativeFile = fileAttribute.Value;
			string relativeTo = Path.GetDirectoryName(codeElement.ProjectItem.ContainingProject.FullName);
			slashdocFullPathName = PathEx.GetFullPathRelativeTo(relativeFile, relativeTo);
			slashDocXPath = pathAttribute.Value;
			return true;
		}

		private XmlDocument GetDefaultTemplate()
		{
			XmlDocument doc = new XmlDocument();
			// TODO: access template documents from config files or something
			XmlElement libraries = doc.CreateElement("Libraries");
			XmlElement types = doc.CreateElement("Types");
			libraries.AppendChild(types);
			doc.AppendChild(libraries);
			return doc;
		}

		/// <summary>
		/// Defines the signature for a method that can generate a slashdoc filename from a source code filename & match/replace expressions.
		/// Generates the docfile name+path for the specified source code file name+path.
		/// </summary>
		/// <param name="codeFileFullPath">The full pathname of the source code file.</param>
		/// <param name="matchPatternRegex">The match pattern to use.</param>
		/// <param name="replacePatternRegex">The replace pattern to use.</param>
		/// <param name="foundMatch">True if the specified matchPatternRegex matched the codeFileFullPath. Otherwise false.</param>
		/// <returns>The target slashdoc file name+path.</returns>
		internal delegate string GenDocFilePathFromCodeFilePathHandler(string codeFileFullPath, string matchPatternRegex, string replacePatternRegex, out bool foundMatch);

		/// <summary>
		/// An implemenation of the <see cref="GenDocFilePathFromCodeFilePathHandler"/>.
		/// </summary>
		private string GenDocFilePathFromCodeFilePath(string codeFileFullPath, string matchPatternRegex, string replacePatternRegex, out bool foundMatch)
		{
			string codeFilePath = Path.GetDirectoryName(codeFileFullPath);//Application.ActiveDocument.Path;
			string codeFileName = Path.GetFileName(codeFileFullPath);

			EnvDTE.Project project = Application.ActiveDocument.ProjectItem.ContainingProject;
			string projectPath = Path.GetDirectoryName(project.FullName);

			#region info traces...
			DiagnosticService.TraceInfo("Code File Path:'{0}'.", codeFilePath);
			DiagnosticService.TraceInfo("Code File Name:'{0}'.", codeFileName);
			DiagnosticService.TraceInfo("Project Path:" + projectPath);
			#endregion 
	
			//TODO: open the slashdoc file:
			
			//TODO: display defaults in a UI and ask if they're ok:
	
			//todo: replace docPathPattern placeholders:
			string docFileFullName = "";
			try
			{
				Regex regex = new Regex(matchPatternRegex, RegexOptions.IgnoreCase);
				foundMatch = regex.IsMatch(Path.Combine(codeFilePath, codeFileName));
				docFileFullName = regex.Replace(Path.Combine(codeFilePath, codeFileName), replacePatternRegex);
			}
			catch (Exception eRegEx)
			{
				DiagnosticService.TraceError("Error exectuing slashdoc file patterns:'{0}'.", eRegEx.ToString());
				foundMatch = false;
			}
			DiagnosticService.TraceInfo("Doc file path='{0}'", docFileFullName);
			return docFileFullName;
		}


		public override void QueryStatus(EnvDTE.vsCommandStatusTextWanted neededText, ref EnvDTE.vsCommandStatus statusOption, ref object commandText)
		{
			switch (neededText)
			{
				case EnvDTE.vsCommandStatusTextWanted.vsCommandStatusTextWantedName:
					commandText = this.Caption;
					break;
				case EnvDTE.vsCommandStatusTextWanted.vsCommandStatusTextWantedStatus:
					commandText = "Ready";//???
					break;
				case EnvDTE.vsCommandStatusTextWanted.vsCommandStatusTextWantedNone:
					commandText = "";
					break;
			}
			//TODO: need to see if code window is active...
			statusOption = vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
		}
	}
}