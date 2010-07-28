/*
Copyright (c) 2005-2010 Scott Willeke (scott@willeke.com)
Licensed under the MIT license: http://www.opensource.org/licenses/mit-license.php
*/
using System;
using System.ComponentModel;
using System.Windows.Forms;
using EnvDTE;
using PingPoet.SlashDocs.Commands;
using PingPoet.SlashDocs.Diagnostics;

namespace PingPoet.SlashDocs.UI
{
	/// <summary>
	/// Summary description for FileNamingDialog.
	/// </summary>
	internal class FileNamingDialog : Form
	{
		private TextBox txtMatchExpression;
		private Label lblMatchExpression;
		private Label lblReplaceExpression;
		private TextBox txtReplaceExpression;
		private GroupBox grpExpressions;
		private Label lblSavedWithProject;
		private Label lblSourceFile;
		private TextBox txtSlashdocFile;
		private Label lblDocFile;
		private Button btnOk;
		private Button btnCancel;
		private TextBox txtSourceCodeFilePath;
		private IContainer components;

		private ErrorProvider errorProvider;
		private Label lblIncludeStatement;
		private Label lblIncludeRelativePath;
		private ToolTip tip;
		private TextBox txtDocCommentIncludeFileBasePath;
		private GroupBox grpOutput;
		private GroupBox grpSource;
		private Label label1;
		private TextBox txtIncludeStatement;
		private TextBox txtCurrentMember;

		private OpenSlashDocFileCommand.GenDocFilePathFromCodeFilePathHandler _genFileNameHandler;
		private readonly CodeElement _codeElement;
		private bool _isInitializing = true;

		/// <summary>
		/// Initializes a new instance of the <see cref="FileNamingDialog"/>.
		/// </summary>
		/// <param name="sourceCodeFilePath"><see cref="SourceCodeFilePath"/></param>
		/// <param name="docCommentIncludeFileBasePath"><see cref="DocCommentIncludeFileBasePath"/></param>
		/// <param name="matchExpression"><see cref="MatchExpression"/></param>
		/// <param name="replaceExpression"><see cref="ReplaceExpression"/></param>
		/// <param name="genFileNameHandler"><see cref="OpenSlashDocFileCommand.GenDocFilePathFromCodeFilePathHandler"/></param>
		/// <param name="codeElement">The current code element being documented.</param>
		public FileNamingDialog(string sourceCodeFilePath, string docCommentIncludeFileBasePath, string matchExpression, string replaceExpression, CodeElement codeElement, OpenSlashDocFileCommand.GenDocFilePathFromCodeFilePathHandler genFileNameHandler)
		{
			InitializeComponent();
			_genFileNameHandler = genFileNameHandler;
			_codeElement = codeElement;

			this.txtSourceCodeFilePath.Text = sourceCodeFilePath;
			this.txtMatchExpression.Text = matchExpression;
			this.txtReplaceExpression.Text = replaceExpression;
			this.txtDocCommentIncludeFileBasePath.Text = docCommentIncludeFileBasePath;
			this.txtCurrentMember.Text = codeElement.FullName;

			_isInitializing = false;
			UpdateSlashdocFilePath();
		}

		/// <summary>
		/// Returns the replace expression displayed in the dialog.
		/// </summary>
		public string ReplaceExpression
		{
			get { return this.txtReplaceExpression.Text; }
		}

		/// <summary>
		/// Returns the match experssion displayed in the dialog.
		/// </summary>
		public string MatchExpression
		{
			get { return this.txtMatchExpression.Text; }
		}

		/// <summary>
		/// Returns the base path that the file path for the include element generated for the documentation comment in the source file is relative to.
		/// </summary>
		public string DocCommentIncludeFileBasePath
		{
			get { return this.txtDocCommentIncludeFileBasePath.Text; }
		}

		/// <summary>
		/// Returns the Slashdoc file path displayed in the dialog.
		/// </summary>
		public string SlashdocFilePath
		{
			get { return this.txtSlashdocFile.Text; }
		}

		/// <summary>
		/// Returns the Slashdoc file path displayed in the dialog.
		/// </summary>
		public string SourceCodeFilePath
		{
			get { return this.txtSourceCodeFilePath.Text; }
		}

		private void UpdateSlashdocFilePath()
		{
			if (_isInitializing)
				return;
			if (_genFileNameHandler != null)
			{
				bool foundMatch;
				this.txtSlashdocFile.Text = _genFileNameHandler(this.SourceCodeFilePath, this.MatchExpression, this.ReplaceExpression, out foundMatch);
				if (foundMatch)
					this.errorProvider.SetError(this.txtMatchExpression, "");
				else
					this.errorProvider.SetError(this.txtMatchExpression, "The match expression did not match the source file name.");
			}
			else
			{
				DiagnosticService.TraceError("GenFileNameHandler not initialized.");
			}
			string slashdocFilePath = this.SlashdocFilePath;
			string docCommentIncludeFileBasePath = this.DocCommentIncludeFileBasePath;
			CodeElement codeElement = this.CodeElement;
			string includeStatement;
			if (Util.IsNullOrEmpty(slashdocFilePath) || Util.IsNullOrEmpty(docCommentIncludeFileBasePath))
				includeStatement = "";
			else
				includeStatement = OpenSlashDocFileCommand.CreateDocCommentIncludeElement(slashdocFilePath, docCommentIncludeFileBasePath, codeElement);
			this.txtIncludeStatement.Text = includeStatement;
		}


		/// <summary>
		/// Handler for the TextChanged event of the expression textboxes.
		/// </summary>
		private void OnExpressionTextboxTextChanged(object sender, EventArgs e)
		{
			UpdateSlashdocFilePath();
		}

		#region Disposable

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(FileNamingDialog));
			this.txtMatchExpression = new System.Windows.Forms.TextBox();
			this.lblMatchExpression = new System.Windows.Forms.Label();
			this.lblReplaceExpression = new System.Windows.Forms.Label();
			this.txtReplaceExpression = new System.Windows.Forms.TextBox();
			this.grpExpressions = new System.Windows.Forms.GroupBox();
			this.txtDocCommentIncludeFileBasePath = new System.Windows.Forms.TextBox();
			this.lblIncludeRelativePath = new System.Windows.Forms.Label();
			this.lblSavedWithProject = new System.Windows.Forms.Label();
			this.lblSourceFile = new System.Windows.Forms.Label();
			this.txtSourceCodeFilePath = new System.Windows.Forms.TextBox();
			this.txtSlashdocFile = new System.Windows.Forms.TextBox();
			this.lblDocFile = new System.Windows.Forms.Label();
			this.btnOk = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.errorProvider = new System.Windows.Forms.ErrorProvider();
			this.txtIncludeStatement = new System.Windows.Forms.TextBox();
			this.lblIncludeStatement = new System.Windows.Forms.Label();
			this.tip = new System.Windows.Forms.ToolTip(this.components);
			this.grpOutput = new System.Windows.Forms.GroupBox();
			this.grpSource = new System.Windows.Forms.GroupBox();
			this.txtCurrentMember = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.grpExpressions.SuspendLayout();
			this.grpOutput.SuspendLayout();
			this.grpSource.SuspendLayout();
			this.SuspendLayout();
			// 
			// txtMatchExpression
			// 
			this.txtMatchExpression.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.txtMatchExpression.Location = new System.Drawing.Point(8, 32);
			this.txtMatchExpression.Name = "txtMatchExpression";
			this.txtMatchExpression.Size = new System.Drawing.Size(304, 20);
			this.txtMatchExpression.TabIndex = 1;
			this.txtMatchExpression.Text = "";
			this.tip.SetToolTip(this.txtMatchExpression, "This expression is used to match the portion of the source code file\'s path & nam" +
				"e used when generating the slashdoc file.");
			this.txtMatchExpression.TextChanged += new System.EventHandler(this.OnExpressionTextboxTextChanged);
			// 
			// lblMatchExpression
			// 
			this.lblMatchExpression.AutoSize = true;
			this.lblMatchExpression.Location = new System.Drawing.Point(8, 16);
			this.lblMatchExpression.Name = "lblMatchExpression";
			this.lblMatchExpression.Size = new System.Drawing.Size(158, 16);
			this.lblMatchExpression.TabIndex = 0;
			this.lblMatchExpression.Text = "Source File Match Expression:";
			// 
			// lblReplaceExpression
			// 
			this.lblReplaceExpression.AutoSize = true;
			this.lblReplaceExpression.Location = new System.Drawing.Point(8, 56);
			this.lblReplaceExpression.Name = "lblReplaceExpression";
			this.lblReplaceExpression.Size = new System.Drawing.Size(135, 16);
			this.lblReplaceExpression.TabIndex = 2;
			this.lblReplaceExpression.Text = "Slashdoc File Expression:";
			// 
			// txtReplaceExpression
			// 
			this.txtReplaceExpression.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.txtReplaceExpression.Location = new System.Drawing.Point(8, 72);
			this.txtReplaceExpression.Name = "txtReplaceExpression";
			this.txtReplaceExpression.Size = new System.Drawing.Size(304, 20);
			this.txtReplaceExpression.TabIndex = 3;
			this.txtReplaceExpression.Text = "";
			this.tip.SetToolTip(this.txtReplaceExpression, "This expression is used to generate the slashdoc filename with the captures gathe" +
				"red in the match expression.");
			this.txtReplaceExpression.TextChanged += new System.EventHandler(this.OnExpressionTextboxTextChanged);
			// 
			// grpExpressions
			// 
			this.grpExpressions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.grpExpressions.Controls.Add(this.txtDocCommentIncludeFileBasePath);
			this.grpExpressions.Controls.Add(this.lblIncludeRelativePath);
			this.grpExpressions.Controls.Add(this.lblSavedWithProject);
			this.grpExpressions.Controls.Add(this.lblMatchExpression);
			this.grpExpressions.Controls.Add(this.lblReplaceExpression);
			this.grpExpressions.Controls.Add(this.txtReplaceExpression);
			this.grpExpressions.Controls.Add(this.txtMatchExpression);
			this.grpExpressions.Location = new System.Drawing.Point(8, 112);
			this.grpExpressions.Name = "grpExpressions";
			this.grpExpressions.Size = new System.Drawing.Size(328, 160);
			this.grpExpressions.TabIndex = 2;
			this.grpExpressions.TabStop = false;
			this.grpExpressions.Text = "Project Settings";
			// 
			// txtDocCommentIncludeFileBasePath
			// 
			this.txtDocCommentIncludeFileBasePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.txtDocCommentIncludeFileBasePath.Location = new System.Drawing.Point(8, 112);
			this.txtDocCommentIncludeFileBasePath.Name = "txtDocCommentIncludeFileBasePath";
			this.txtDocCommentIncludeFileBasePath.Size = new System.Drawing.Size(304, 20);
			this.txtDocCommentIncludeFileBasePath.TabIndex = 5;
			this.txtDocCommentIncludeFileBasePath.Text = "";
			this.tip.SetToolTip(this.txtDocCommentIncludeFileBasePath, "This is the path that the <include> statements\' file attribute value is relative " +
				"to. Typically this is the directory that you compile the code from (the solution" +
				" or project directory).");
			this.txtDocCommentIncludeFileBasePath.TextChanged += new System.EventHandler(this.OnExpressionTextboxTextChanged);
			// 
			// lblIncludeRelativePath
			// 
			this.lblIncludeRelativePath.AutoSize = true;
			this.lblIncludeRelativePath.Location = new System.Drawing.Point(8, 96);
			this.lblIncludeRelativePath.Name = "lblIncludeRelativePath";
			this.lblIncludeRelativePath.Size = new System.Drawing.Size(120, 16);
			this.lblIncludeRelativePath.TabIndex = 4;
			this.lblIncludeRelativePath.Text = "File Include Base Path:";
			// 
			// lblSavedWithProject
			// 
			this.lblSavedWithProject.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.lblSavedWithProject.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lblSavedWithProject.ForeColor = System.Drawing.SystemColors.GrayText;
			this.lblSavedWithProject.Location = new System.Drawing.Point(8, 136);
			this.lblSavedWithProject.Name = "lblSavedWithProject";
			this.lblSavedWithProject.Size = new System.Drawing.Size(312, 16);
			this.lblSavedWithProject.TabIndex = 6;
			this.lblSavedWithProject.Text = "These are saved with the project.";
			this.lblSavedWithProject.TextAlign = System.Drawing.ContentAlignment.BottomRight;
			this.lblSavedWithProject.UseMnemonic = false;
			// 
			// lblSourceFile
			// 
			this.lblSourceFile.AutoSize = true;
			this.lblSourceFile.Location = new System.Drawing.Point(8, 16);
			this.lblSourceFile.Name = "lblSourceFile";
			this.lblSourceFile.Size = new System.Drawing.Size(65, 16);
			this.lblSourceFile.TabIndex = 0;
			this.lblSourceFile.Text = "Source File:";
			// 
			// txtSourceCodeFilePath
			// 
			this.txtSourceCodeFilePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.txtSourceCodeFilePath.AutoSize = false;
			this.txtSourceCodeFilePath.Location = new System.Drawing.Point(8, 32);
			this.txtSourceCodeFilePath.Name = "txtSourceCodeFilePath";
			this.txtSourceCodeFilePath.ReadOnly = true;
			this.txtSourceCodeFilePath.Size = new System.Drawing.Size(312, 20);
			this.txtSourceCodeFilePath.TabIndex = 1;
			this.txtSourceCodeFilePath.Text = "";
			// 
			// txtSlashdocFile
			// 
			this.txtSlashdocFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.txtSlashdocFile.AutoSize = false;
			this.txtSlashdocFile.Location = new System.Drawing.Point(8, 32);
			this.txtSlashdocFile.Multiline = true;
			this.txtSlashdocFile.Name = "txtSlashdocFile";
			this.txtSlashdocFile.Size = new System.Drawing.Size(312, 32);
			this.txtSlashdocFile.TabIndex = 1;
			this.txtSlashdocFile.Text = "";
			// 
			// lblDocFile
			// 
			this.lblDocFile.AutoSize = true;
			this.lblDocFile.Location = new System.Drawing.Point(8, 16);
			this.lblDocFile.Name = "lblDocFile";
			this.lblDocFile.Size = new System.Drawing.Size(75, 16);
			this.lblDocFile.TabIndex = 0;
			this.lblDocFile.Text = "Slashdoc File:";
			// 
			// btnOk
			// 
			this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOk.Location = new System.Drawing.Point(184, 408);
			this.btnOk.Name = "btnOk";
			this.btnOk.TabIndex = 4;
			this.btnOk.Text = "&OK";
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(264, 408);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.TabIndex = 5;
			this.btnCancel.Text = "&Cancel";
			// 
			// errorProvider
			// 
			this.errorProvider.ContainerControl = this;
			// 
			// txtIncludeStatement
			// 
			this.txtIncludeStatement.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.txtIncludeStatement.Location = new System.Drawing.Point(8, 88);
			this.txtIncludeStatement.Multiline = true;
			this.txtIncludeStatement.Name = "txtIncludeStatement";
			this.txtIncludeStatement.Size = new System.Drawing.Size(312, 32);
			this.txtIncludeStatement.TabIndex = 3;
			this.txtIncludeStatement.Text = "<include file=\"../blah/blah.xml\" path=\"/library/types/type/member/etc...\"/>";
			// 
			// lblIncludeStatement
			// 
			this.lblIncludeStatement.AutoSize = true;
			this.lblIncludeStatement.Location = new System.Drawing.Point(8, 72);
			this.lblIncludeStatement.Name = "lblIncludeStatement";
			this.lblIncludeStatement.Size = new System.Drawing.Size(99, 16);
			this.lblIncludeStatement.TabIndex = 2;
			this.lblIncludeStatement.Text = "Include Statement:";
			// 
			// grpOutput
			// 
			this.grpOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.grpOutput.Controls.Add(this.txtSlashdocFile);
			this.grpOutput.Controls.Add(this.lblDocFile);
			this.grpOutput.Controls.Add(this.txtIncludeStatement);
			this.grpOutput.Controls.Add(this.lblIncludeStatement);
			this.grpOutput.Location = new System.Drawing.Point(8, 272);
			this.grpOutput.Name = "grpOutput";
			this.grpOutput.Size = new System.Drawing.Size(328, 128);
			this.grpOutput.TabIndex = 3;
			this.grpOutput.TabStop = false;
			this.grpOutput.Text = "Output";
			// 
			// grpSource
			// 
			this.grpSource.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.grpSource.Controls.Add(this.txtCurrentMember);
			this.grpSource.Controls.Add(this.label1);
			this.grpSource.Controls.Add(this.txtSourceCodeFilePath);
			this.grpSource.Controls.Add(this.lblSourceFile);
			this.grpSource.Location = new System.Drawing.Point(8, 8);
			this.grpSource.Name = "grpSource";
			this.grpSource.Size = new System.Drawing.Size(328, 104);
			this.grpSource.TabIndex = 6;
			this.grpSource.TabStop = false;
			this.grpSource.Text = "Source";
			// 
			// txtCurrentMember
			// 
			this.txtCurrentMember.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.txtCurrentMember.AutoSize = false;
			this.txtCurrentMember.Location = new System.Drawing.Point(8, 72);
			this.txtCurrentMember.Name = "txtCurrentMember";
			this.txtCurrentMember.ReadOnly = true;
			this.txtCurrentMember.Size = new System.Drawing.Size(312, 20);
			this.txtCurrentMember.TabIndex = 3;
			this.txtCurrentMember.Text = "MyNamespace.MyClass.MyMember";
			this.txtCurrentMember.WordWrap = false;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(8, 56);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(90, 16);
			this.label1.TabIndex = 2;
			this.label1.Text = "Current Member:";
			// 
			// FileNamingDialog
			// 
			this.AcceptButton = this.btnOk;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(344, 446);
			this.Controls.Add(this.grpSource);
			this.Controls.Add(this.grpOutput);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOk);
			this.Controls.Add(this.grpExpressions);
			this.HelpButton = true;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(216, 416);
			this.Name = "FileNamingDialog";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "New Slashdoc...";
			this.TransparencyKey = System.Drawing.Color.FromArgb(((System.Byte)(0)), ((System.Byte)(254)), ((System.Byte)(0)));
			this.grpExpressions.ResumeLayout(false);
			this.grpOutput.ResumeLayout(false);
			this.grpSource.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private void btnOk_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		public CodeElement CodeElement
		{
			get { return _codeElement; }
		}
	}
}