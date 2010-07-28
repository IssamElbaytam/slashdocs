/*
Copyright (c) 2005-2010 Scott Willeke (scott@willeke.com)
Licensed under the MIT license: http://www.opensource.org/licenses/mit-license.php
*/
using System;
using System.Diagnostics;
using System.Globalization;
using EnvDTE;
using EnvDTE80;
using PingPoet.SlashDocs.Diagnostics;

namespace PingPoet.SlashDocs.Commands
{
	/// <summary>
	/// Represents a command with in this AddIn.
	/// </summary>
	internal abstract class AddInCommand
	{
		private readonly DTE _application;
		private readonly AddIn _owner;
		private readonly string _caption;
		private readonly string _toolTipText;
		private int _iconID;
		private bool _iconIsCustom;

		/// <summary>
		/// Allows derrived classes to create an instance of this class.
		/// </summary>
		/// <param name="application">The value for <see cref="Application"/>.</param>
		/// <param name="owner">The value for <see cref="Owner"/>.</param>
		/// <param name="caption">The value for <see cref="Caption"/>.</param>
		protected AddInCommand(DTE application, AddIn owner, string caption, string toolTipText)
		{
			if (application == null)
				throw new ArgumentNullException("application");
			if (owner == null)
				throw new ArgumentNullException("owner");
			if (caption == null)
				throw new ArgumentNullException("caption");
			else if (caption.Length == 0)
				throw new ArgumentException("Argument cannot be empty.", "toolTipText");
			if (toolTipText == null)
				throw new ArgumentNullException("toolTipText");
			else if (toolTipText.Length == 0)
				throw new ArgumentException("Argument cannot be empty.", "toolTipText");

			_application = application;
			_owner = owner;
			_caption = caption;
			_toolTipText = toolTipText;
			_iconID = 0;
			_iconIsCustom = false;
		}

		/// <summary>
		/// Returns a reference to the application this command is associated with.
		/// </summary>
		protected DTE Application
		{
			get { return _application; }
		}

		/// <summary>
		/// The AddIn instance that owns this command handler.
		/// </summary>
		private AddIn Owner
		{
			get { return _owner; }
		}

		/// <summary>
		/// Returns the name of this command.
		/// </summary>
		public string Name
		{
			get { return this.GetType().Name; }
		}

		/// <summary>
		/// Specifies the text representing this command that should be displayed to users.
		/// </summary>
		public string Caption
		{
			get { return _caption; }
		}

		/// <summary>
		/// Specifies text to be displayed as a tool tip for this command.
		/// </summary>
		public string ToolTipText
		{
			get { return _toolTipText; }
		}

		/// <summary>
		/// Specifies an image/icon ID for this command.
		/// </summary>
		/// <remarks>
		/// Smily is 59.
		/// See http://blogs.msdn.com/craigskibo/archive/2004/01/08/48814.aspx
		/// </remarks>
		public int IconID
		{
			get { return _iconID; }
			set { _iconID = value; }
		}

		/// <summary>
		/// Set to true if using a satellite DLL and <see cref="IconID"/> specifies a custom resource ID.
		/// </summary>
		public bool IconIsCustom
		{
			get { return _iconIsCustom; }
			set { _iconIsCustom = value; }
		}

		/// <summary>
		/// Adds the <see cref="EnvDTE.Command"/> to the environment if it is not present already.
		/// </summary>
		/// <remarks>
		///	The base classes implementation of this method uses the <see cref="EnvDTE.Commands.AddNamedCommand"/> method to install the command, and returns it.
		///	The derrived class should override this method and place the command on an appropriate commandbar via <see cref="Application.CommandBars"/>. 
		/// </remarks>
		public virtual EnvDTE.Command Install()
		{
			object[] contextGuids = new object[] { };
			Command cmd;
			try
			{
				Commands2 commands = (Commands2) Application.Commands;
				cmd = commands.AddNamedCommand2(this.Owner, this.Name, this.Caption, this.ToolTipText, !this.IconIsCustom, this.IconID, ref contextGuids, (int)vsCommandStatus.vsCommandStatusSupported | (int)vsCommandStatus.vsCommandStatusEnabled, (int)vsCommandStyle.vsCommandStylePictAndText, vsCommandControlType.vsCommandControlTypeButton);
			}
			catch (Exception e)
			{
				Debug.Fail("Exception occured while installing VS command:" + e.ToString());
				throw;
			}
			Debug.Assert(cmd != null, "VS Failed to add the command!");
			return cmd;
		}

		/// <summary>
		/// Removes the <see cref="EnvDTE.Command"/> from the environment.
		/// </summary>
		/// <remarks>The implementation of this method should normally use the <see cref="M:EnvDTE.Command.Delete"/> method.</remarks>
		public void Uninstall()
		{
			foreach (EnvDTE.Command command in this.Application.Commands)
			{
				if (0 == string.Compare(this.Name, command.Name, false, CultureInfo.InvariantCulture))
				{
					command.Delete();
					return;
				}
			}
		}

		///<summary>
		/// Implements QueryStatus for this command.
		/// </summary>
		/// <summary>
		/// Implements <see cref="IDTCommandTarget.QueryStatus"/>:
		/// Returns the current status of the specified named command, whether it is enabled, disabled, hidden, and so forth.
		/// </summary>
		/// <param name="neededText">A <see cref="T:vsCommandStatusTextWanted"/> constant specifying if information is returned from the check, and if so, what type of information is returned.</param>
		/// <param name="statusOption">A <see cref="T:vsCommandStatus"/> specifying the current status of the command.</param>
		/// <param name="commandText">The text to return if <see cref="T:vsCommandStatusTextWantedStatus"/> is specified.</param>
		public abstract void QueryStatus(EnvDTE.vsCommandStatusTextWanted neededText, ref vsCommandStatus statusOption, ref object commandText);
		/// <summary>
		/// Implements <see cref="IDTCommandTarget.Exec"/>:
		/// Executes the specified named command.
		/// </summary>
		/// <param name="executeOption">A <see cref="T:vsCommandExecOption"/> constant specifying the execution options.</param>
		/// <returns>True indicates that the command was implemented/handled. False indicates it was not.</returns>
		public abstract bool Execute(EnvDTE.vsCommandExecOption executeOption);
	}
}
