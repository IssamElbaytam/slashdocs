/*
Copyright (c) 2005-2010 Scott Willeke (scott@willeke.com)
Licensed under the MIT license: http://www.opensource.org/licenses/mit-license.php
*/
using System;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using EnvDTE;
using PingPoet.SlashDocs.Diagnostics;

namespace PingPoet.SlashDocs.DteTools
{
	/// <summary>
	/// Utility functions for working with the DTE CodeModel.
	/// </summary>
	internal class CodeModelTools
	{
		private CodeModelTools()
		{
		}

		/// <summary>
		/// Gets the XPath statement to find the specified CodeElement's documentation in a slashdoc file.
		/// </summary>
		/// <param name="codeElement">The code element</param>
		/// <returns></returns>
		public static string GetXPathForCodeElement(CodeElement codeElement)
		{
			if (codeElement == null)
				return "";
			string typeName;
			string memberName;
			string namespaceName;
			CodeModelTools.GetMemberAndTypeFromCodeElement(codeElement, out typeName, out memberName, out namespaceName);
			string xPath = "";
			if (memberName != null && memberName.Length > 0)
				xPath = string.Format("/Libraries/Types/Type[@name=\"{0}\"]/Members/Member[@name=\"{1}\"]/Docs", typeName, memberName);
			else
				xPath = string.Format("/Libraries/Types/Type[@name=\"{0}\"]/Docs", typeName);
			return xPath;
		}

		/// <summary>
		/// Returns the inline documenation embedded in the source with the specified code element.
		/// </summary>
		/// <param name="codeElement">The code element to obtain the documentation for.</param>
		/// <returns>The documentation.</returns>
		public static string DocCommentFromCodeElement(CodeElement codeElement)
		{
			string docComment = null;
			GetSetDocComment(codeElement, ref docComment);
			return docComment;
		}

		
		public static void SetDocComment(CodeElement codeElement, string docComment)
		{
			if (docComment == null)
				docComment = "";
			GetSetDocComment(codeElement, ref docComment);
		}

		/// <summary>
		/// DocComments from CodeElement objects are nested within a &lt;doc&gt; root element. This method returns only the content within that root &lt;doc&gt; element.
		/// </summary>
		public static string GetContentFromDocComment(string docComment)
		{
			try
			{
				XmlDocument dom = new XmlDocument();
				dom.LoadXml(docComment);
				return dom.DocumentElement.InnerXml;
			}
			catch (XmlException )
			{ //empty? bad docs?
				return docComment;
			}
			
		}
		
		/// <summary>
		/// Gets or sets the docComment on the specified codeElement.
		/// </summary>
		/// <param name="codeElement">The <see cref="CodeElement"/> to get or set the DocComment for.</param>
		/// <param name="docComment">
		/// If <paramref cref="docComment"/> is null it will return the codeElement's DocComment. 
		/// Otherwise the CodeElement's DocComment will be set to the specified value.
		/// </param>
		private static void GetSetDocComment(CodeElement codeElement, ref string docComment)
		{
			bool getDocComment = docComment == null;

			switch (codeElement.Kind)
			{
				case vsCMElement.vsCMElementFunction:
					{
						CodeFunction codeFunction = codeElement as CodeFunction;
						DiagnosticService.Assert(codeFunction != null, "expected CodeFunction");
						if (getDocComment)
							docComment = codeFunction.DocComment;
						else
							codeFunction.DocComment = docComment;
						break;
					}
				case vsCMElement.vsCMElementStruct:
				case vsCMElement.vsCMElementClass:
				case vsCMElement.vsCMElementInterface:
					{
						CodeType codeType = codeElement as CodeType;
						DiagnosticService.Assert(codeType != null, "expected CodeType");
						if (getDocComment)
							docComment = codeType.DocComment;
						else
							codeType.DocComment = docComment;
						break;
					}
				case vsCMElement.vsCMElementEnum:
					{
						CodeEnum codeEnum = codeElement as CodeEnum;
						DiagnosticService.Assert(codeEnum != null, "expected CodeEnum");
						if (getDocComment)
							docComment = codeEnum.DocComment;
						else
							codeEnum.DocComment = docComment;
						break;
					}
				case vsCMElement.vsCMElementProperty:
					{
						CodeProperty codeProperty = codeElement as CodeProperty;
						DiagnosticService.Assert(codeProperty != null, "Expected CodeProperty");
						if (getDocComment)
							docComment = codeProperty.DocComment;
						else
							codeProperty.DocComment = docComment;
						break;
					}
				case vsCMElement.vsCMElementVariable:
					{
						CodeVariable codeVariable = codeElement as CodeVariable;
						DiagnosticService.Assert(codeVariable != null, "Expected CodeVariable");
						if (getDocComment)
							docComment = codeVariable.DocComment;
						else
							codeVariable.DocComment = docComment;
						break;
					}
					//case vsCMElement.vsCMElementEvent:
				default:
					DiagnosticService.Assert(false, "Unexpected CodeElement Kind:'{0}'", codeElement.Kind);
					break;
			}
			return;
		}

		/// <summary>
		/// Gets the type, member and namespace of the specified code element. If the code element represents a type (class, interface, enum, etc..), memberName will be an empty string.
		/// </summary>
		public static void GetMemberAndTypeFromCodeElement(CodeElement codeElement, out string typeName, out string memberName, out string namespaceName)
		{
			typeName = "";
			memberName = "";
			namespaceName = "";

			if (codeElement.IsCodeType)
			{
				CodeType codeType = (CodeType)codeElement;
				typeName = codeType.Name;
				namespaceName = codeType.Namespace.FullName;
				return;
			}
			
			switch (codeElement.Kind)
			{
				case vsCMElement.vsCMElementFunction:
				{
					CodeFunction codeFunction = codeElement as CodeFunction;
					DiagnosticService.Assert(codeFunction != null, "expected CodeFunction");
					if (codeFunction == null)
						return;
					CodeType parentType = codeFunction.Parent as CodeType;
					DiagnosticService.Assert(parentType != null, "expected parent type");
					typeName = parentType.Name;
					namespaceName = parentType.Namespace.FullName;
					

					//TODO: DETECT OVERLOADS VIA PARENT TYPE AND HANDLE OVERLOADS IN OUTPUT:
					if (!IsFunctionOverloaded(codeFunction))
						memberName= codeFunction.Name;
					else
						memberName = CodeModelTools.GetCodeFunctionSignature(codeFunction);
					break;
				}
				case vsCMElement.vsCMElementProperty:
				{
					//I couldn't cast it to CodeProperty2 in VS2005 either, but I read it works with VB and not C#: EnvDTE80.CodeProperty2 codeProperty = codeElement as EnvDTE80.CodeProperty2;
					CodeType parentType = null;
					if (codeElement is EnvDTE80.CodeProperty2)
					{
						EnvDTE80.CodeProperty2 codeProp2 = (EnvDTE80.CodeProperty2)codeElement;
						parentType = codeProp2.Parent2 as CodeType;
					}
					else
					{
						CodeProperty codeProperty = codeElement as CodeProperty;
						DiagnosticService.Assert(codeProperty != null, "Expected CodeProperty");
						if (codeProperty != null)
						{
							//NOTE: CodeProperty.Parent is broken for interfaces & structs. http://support.microsoft.com/default.aspx?scid=kb;en-us;555109
							//TODO: I think this can be replaced with a workaround by traversing codeelements in the file and keeping track of the parent class items, until you find the property.
							try
							{
								parentType = codeProperty.Parent as CodeType;
							}
							catch (InvalidCastException )
							{
								string msg =
									"Due to a problem in Visual Studio.NET, properties cannot be supported on interfaces and structures (http://support.microsoft.com/default.aspx?scid=kb;en-us;555109).";
								MessageBox.Show(msg, "Visual Studio Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
							}
						}
					}

					DiagnosticService.Assert(parentType != null, "expected parent type");
					typeName = parentType != null ? parentType.Name : "";
					namespaceName = parentType != null ? parentType.Namespace.FullName : "";
					memberName = codeElement.Name;
					break;
				}
				case vsCMElement.vsCMElementVariable:
				{
					CodeVariable codeVariable = codeElement as CodeVariable;
					DiagnosticService.Assert(codeVariable != null, "Expected CodeVariable");
					if (codeVariable == null)
						return;
					CodeType parentType = codeVariable.Parent as CodeType;
					DiagnosticService.Assert(parentType != null, "expected parent type");
					typeName = parentType.Name;
					namespaceName = parentType.Namespace.FullName;
					memberName = codeVariable.Name;
					break;
				}
				case vsCMElement.vsCMElementEvent:
				{
					string fullName = codeElement.FullName;
					DiagnosticService.TraceInfo(fullName);
					break;
				}
				default:
					DiagnosticService.TraceError("Unexpected CodeElement Kind:'{0}'", codeElement.Kind);
					break;
			}
		}

		/// <summary>
		/// Returns true if the specified function has overloads.
		/// </summary>
		/// <param name="codeFunction">The function to detect overloads for.</param>
		/// <returns>True if the specified function has additional overloads.</returns>
		private static bool IsFunctionOverloaded(CodeFunction codeFunction)
		{
			return codeFunction.Overloads.Count > 1;
		}

		/// <summary>
		/// Returns a unique signature for the specified codeFunction.
		/// </summary>
		/// <param name="codeFunction">The code function to get a signature for.</param>
		/// <returns>A unique signature.</returns>
		private static string GetCodeFunctionSignature(CodeFunction codeFunction)
		{
			string[] paramNames = new string[codeFunction.Parameters.Count];
			for (int paramIndex = 0; paramIndex < codeFunction.Parameters.Count; paramIndex++)
			{
				CodeParameter codeParam = codeFunction.Parameters.Item(paramIndex + 1) as CodeParameter;
				string typeName = codeParam.Type.AsString;
				paramNames[paramIndex] = typeName;
			}
			return string.Concat(codeFunction.Name, '(', string.Join(",", paramNames), ')');
		}
	}
}