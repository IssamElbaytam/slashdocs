/*
Copyright (c) 2005-2010 Scott Willeke (scott@willeke.com)
Licensed under the MIT license: http://www.opensource.org/licenses/mit-license.php
*/
using System;
using System.Diagnostics;
using System.Xml;
using System.Xml.XPath;
using PingPoet.SlashDocs.Diagnostics;

namespace PingPoet.SlashDocs.Xml
{
	/// <summary>
	/// Provides utility functions for working with XML.
	/// </summary>
	internal class XmlTools
	{
		private XmlTools()
		{
		}

		/// <summary>
		/// Returns the line number and line position of the specified XML node.
		/// </summary>
		/// <param name="xmlFileFullPathName">The fully qualified path name to the XML file.</param>
		/// <param name="xpath">The XPath of the XML node to get the location of.</param>
		/// <param name="lineNumber">If the return value is true, the line number of the specified XML element.</param>
		/// <param name="linePosition">If the return value is true, the posiiton on the line of the specified XML element.</param>
		/// <returns>True if the element is found, otherwise false.</returns>
		public static bool GetTextLocationOfXmlNode(string xmlFileFullPathName, string xpath, out int lineNumber, out int linePosition)
		{
			bool isElementFound;
			XPathDocument xpathSlashDoc = new XPathDocument(xmlFileFullPathName);
			XPathNavigator navSlashDoc = xpathSlashDoc.CreateNavigator();
			XPathNodeIterator itr = navSlashDoc.Select(xpath);
			if (itr.Count > 0 && itr.MoveNext())
			{
				IXmlLineInfo lineInfo = itr.Current as IXmlLineInfo;
				DiagnosticService.Assert(lineInfo != null, "Line Information not available.");
				
				if (lineInfo != null)
				{
					lineNumber = lineInfo.LineNumber;
					linePosition = lineInfo.LinePosition;
				}
				else
				{
					lineNumber = -1;
					linePosition = -1;
				}
				Debug.WriteLine(itr.Current.Name + "(" + lineNumber + ","+ linePosition +")");
				isElementFound = true;
			}
			else
			{
				lineNumber = -1;
				linePosition = -1;
				isElementFound = false;
			}
			return isElementFound;
		}
	}
}
