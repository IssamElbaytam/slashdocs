/*
Copyright (c) 2005-2010 Scott Willeke (scott@willeke.com)
Licensed under the MIT license: http://www.opensource.org/licenses/mit-license.php
*/
using System;
using System.Globalization;
using EnvDTE;
using PingPoet.SlashDocs.Diagnostics;


namespace PingPoet.SlashDocs.Commands
{
	/// <summary>
	/// Provides initialization, execution, and status handling for the commands.
	/// </summary>
	internal sealed class CommandHandler : IDTCommandTarget
	{
		private readonly AddIn _owner;
		private readonly DTE _application;
		private readonly AddInCommandCollection _commands = new AddInCommandCollection();

		/// <summary>
		/// Initializes a new instance of the <see cref="T:PingPoet.SlashDocs.Commands.CommandHandler"/> class.
		/// </summary>
		/// <param name="owner">The value for the <see cref="Owner"/> property.</param>
		/// <param name="application">The value for the <see cref="Application"/> property.</param>
		public CommandHandler(AddIn owner, DTE application)
		{
			if (owner == null)
				throw new ArgumentNullException("owner");
			if (application == null)
				throw new ArgumentNullException("application");
			_owner = owner;
			_application = application;

			AddDefaultCommands();
		}
		
		private void AddDefaultCommands()
		{
			this.Commands.Add(new OpenSlashDocFileCommand(this.Application, this.Owner));
		}

		/// <summary>
		/// The AddIn instance that owns this command handler.
		/// </summary>
		private AddIn Owner
		{
			get { return _owner; }
		}

		/// <summary>
		/// Returns a refernece to the DTE/IDE application.
		/// </summary>
		private DTE Application
		{
			get { return _application; }
		}

		/// <summary>
		/// The collection of supported commands.
		/// </summary>
		private AddInCommandCollection Commands
		{
			get { return _commands; }
		}

		private const string CommandPrefix = "PingPoet.SlashDocs.Connect.";

		private void FixCommandName(ref string cmdName)
		{
			if (0 == cmdName.IndexOf(CommandPrefix))
				cmdName = cmdName.Substring(CommandPrefix.Length);
		}

		#region IDTCommandTarget Members
		/// <summary>
		/// Implements <see cref="IDTCommandTarget.Exec"/>:
		/// Executes the specified named command.
		/// </summary>
		/// <param name="cmdName">The name of the command to execute.</param>
		/// <param name="executeOption">A <see cref="T:EnvDTE.vsCommandExecOption"/> constant specifying the execution options.</param>
		/// <param name="variantIn">A value passed to the command.</param>
		/// <param name="variantOut">A value passed back to the invoker Exec method after the command executes.</param>
		/// <param name="handled">True indicates that the command was implemented. False indicates it was not.</param>
		public void Exec(string cmdName, EnvDTE.vsCommandExecOption executeOption, ref object variantIn, ref object variantOut, ref bool handled)
		{
			FixCommandName(ref cmdName);

			AddInCommand command = this.Commands[cmdName];
			if (command == null)
			{
				DiagnosticService.TraceError("Exec: Command '{0}' is not implemented.", cmdName);
				handled = false;
				return;
			}
			handled = command.Execute(executeOption);
		}

		/// <summary>
		/// Implements <see cref="IDTCommandTarget.QueryStatus"/>:
		/// Returns the current status of the specified named command, whether it is enabled, disabled, hidden, and so forth.
		/// </summary>
		/// <param name="cmdName">The name of the command to check.</param>
		/// <param name="neededText">A <see cref="T:EnvDTE.vsCommandStatusTextWanted"/> constant specifying if information is returned from the check, and if so, what type of information is returned.</param>
		/// <param name="statusOption">A <see cref="T:EnvDTE.vsCommandStatus"/> specifying the current status of the command.</param>
		/// <param name="commandText">The text to return if <see cref="T:EnvDTE.vsCommandStatusTextWanted"/> is specified.</param>
		public void QueryStatus(string cmdName, EnvDTE.vsCommandStatusTextWanted neededText, ref EnvDTE.vsCommandStatus statusOption, ref object commandText)
		{
			FixCommandName(ref cmdName);

			AddInCommand command = this.Commands[cmdName];
			if (command == null)
			{
				DiagnosticService.TraceError("QueryStatus: Command '{0}' is not implemented.", cmdName);
				return;
			}
			command.QueryStatus(neededText, ref statusOption, ref commandText);
		}

		#endregion

		/// <summary>
		/// Installs the commands to the environment.
		/// </summary>
		public void InstallCommands()
		{
			foreach (AddInCommand command in this.Commands)
			{
				command.Install();
			}
		}

		/// <summary>
		/// Uninstalled from the environment.
		/// </summary>
		public void UninstallCommands()
		{
			foreach (AddInCommand command in this.Commands)
			{
				command.Uninstall();
			}
		}

		#region class AddInCommandCollection
		class AddInCommandCollection : System.Collections.CollectionBase
		{
			public int Add(AddInCommand command)
			{
				return base.InnerList.Add(command);
			}
			public AddInCommand this[int index]
			{
				get { return (AddInCommand)base.InnerList[index]; }
			}
			public AddInCommand this[string name]
			{
				get
				{
					foreach (AddInCommand cmd in this.InnerList)
					{
						if (0 == string.Compare(name, cmd.Name, false, CultureInfo.InvariantCulture))
							return cmd;
					}
					return null;
				}
			}
		}
		#endregion
	}
}