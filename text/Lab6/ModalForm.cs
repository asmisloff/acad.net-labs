using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;

using Autodesk.AutoCAD.Windows;
using Autodesk.AutoCAD.Runtime;

namespace Lab6
{
	/// <summary>
	/// Summary description for ModalForm.
	/// </summary>
	public class ModalForm : System.Windows.Forms.Form
	{
		internal System.Windows.Forms.Button Button2;
		internal System.Windows.Forms.Button SelectEmployeeButton;
		internal System.Windows.Forms.Label Label3;
		internal System.Windows.Forms.Label Label2;
		internal System.Windows.Forms.Label Label1;
		internal System.Windows.Forms.TextBox tb_Division;
		internal System.Windows.Forms.TextBox tb_Salary;
		internal System.Windows.Forms.TextBox tb_Name;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ModalForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.Button2 = new System.Windows.Forms.Button();
			this.SelectEmployeeButton = new System.Windows.Forms.Button();
			this.Label3 = new System.Windows.Forms.Label();
			this.Label2 = new System.Windows.Forms.Label();
			this.Label1 = new System.Windows.Forms.Label();
			this.tb_Division = new System.Windows.Forms.TextBox();
			this.tb_Salary = new System.Windows.Forms.TextBox();
			this.tb_Name = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// Button2
			// 
			this.Button2.Location = new System.Drawing.Point(240, 120);
			this.Button2.Name = "Button2";
			this.Button2.Size = new System.Drawing.Size(133, 39);
			this.Button2.TabIndex = 30;
			this.Button2.Text = "Close";
			this.Button2.Click += new System.EventHandler(this.Button2_Click);
			// 
			// SelectEmployeeButton
			// 
			this.SelectEmployeeButton.Location = new System.Drawing.Point(240, 56);
			this.SelectEmployeeButton.Name = "SelectEmployeeButton";
			this.SelectEmployeeButton.Size = new System.Drawing.Size(133, 40);
			this.SelectEmployeeButton.TabIndex = 29;
			this.SelectEmployeeButton.Text = "Select Employee";
			this.SelectEmployeeButton.Click += new System.EventHandler(this.SelectEmployeeButton_Click);
			// 
			// Label3
			// 
			this.Label3.Location = new System.Drawing.Point(32, 80);
			this.Label3.Name = "Label3";
			this.Label3.Size = new System.Drawing.Size(184, 20);
			this.Label3.TabIndex = 28;
			this.Label3.Text = "Division";
			// 
			// Label2
			// 
			this.Label2.Location = new System.Drawing.Point(32, 144);
			this.Label2.Name = "Label2";
			this.Label2.Size = new System.Drawing.Size(184, 20);
			this.Label2.TabIndex = 27;
			this.Label2.Text = "Salary";
			// 
			// Label1
			// 
			this.Label1.Location = new System.Drawing.Point(32, 24);
			this.Label1.Name = "Label1";
			this.Label1.Size = new System.Drawing.Size(184, 20);
			this.Label1.TabIndex = 26;
			this.Label1.Text = "Name";
			// 
			// tb_Division
			// 
			this.tb_Division.Location = new System.Drawing.Point(32, 104);
			this.tb_Division.Name = "tb_Division";
			this.tb_Division.Size = new System.Drawing.Size(184, 22);
			this.tb_Division.TabIndex = 25;
			this.tb_Division.Text = "";
			// 
			// tb_Salary
			// 
			this.tb_Salary.Location = new System.Drawing.Point(32, 160);
			this.tb_Salary.Name = "tb_Salary";
			this.tb_Salary.Size = new System.Drawing.Size(184, 22);
			this.tb_Salary.TabIndex = 24;
			this.tb_Salary.Text = "";
			// 
			// tb_Name
			// 
			this.tb_Name.Location = new System.Drawing.Point(32, 40);
			this.tb_Name.Name = "tb_Name";
			this.tb_Name.Size = new System.Drawing.Size(184, 22);
			this.tb_Name.TabIndex = 23;
			this.tb_Name.Text = "";
			// 
			// ModalForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
			this.ClientSize = new System.Drawing.Size(400, 207);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.Button2,
																		  this.SelectEmployeeButton,
																		  this.Label3,
																		  this.Label2,
																		  this.Label1,
																		  this.tb_Division,
																		  this.tb_Salary,
																		  this.tb_Name});
			this.Name = "ModalForm";
			this.Text = "ModalForm";
			this.ResumeLayout(false);

		}
		#endregion

		private void SelectEmployeeButton_Click(object sender, System.EventArgs e)
		{
			Database db = HostApplicationServices.WorkingDatabase;
			Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
			Transaction trans = db.TransactionManager.StartTransaction();
			this.Hide();
			try
			{
				PromptEntityOptions prEnt = new PromptEntityOptions("Select an Employee");
				PromptEntityResult prEntRes = ed.GetEntity(prEnt);
				if( prEntRes.Status != PromptStatus.OK)
				{
					this.Show();
					return;
				}
				
				ArrayList saEmployeeList = new ArrayList(); 

				AsdkClass1.ListEmployee(prEntRes.ObjectId, saEmployeeList);
				if (saEmployeeList.Count == 4)
				{
					tb_Name.Text = saEmployeeList[0].ToString();
					tb_Salary.Text = saEmployeeList[1].ToString();
					tb_Division.Text = saEmployeeList[2].ToString();
				}
			}
			finally
			{
				this.Show();
				trans.Dispose();
			}
		}

		private void Button2_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		[CommandMethod("MODALFORM")]
		public void ShowModalForm()
		{
			ModalForm modalForm = new ModalForm();
			Autodesk.AutoCAD.ApplicationServices.Application.ShowModalDialog(modalForm);
		}

	}
}
