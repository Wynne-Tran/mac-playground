﻿using System;
using System.Windows.Forms;
using System.Threading;
using NUnit.Framework;

// Controls used in tests
using MailClient.UI.Controls;
using MailClient.UI.Controls.ControlSidebar;
using MailClient.Common.UI.Controls;
using MailClient.Common.UI.Controls.ControlToolStrip;
using MailClient.Common.UI.Controls.ControlTextBox;

using MailClient.Storage.Data;

namespace GUITest
{
	[TestFixture]
	class ContactsTestSuite
	{
		[Test]
		public void CreateNewContactTest()
		{
			Thread.Sleep(2000);

			UI.Mouse.Click(UI.TryGetControl("formMain.leftSpine1.controlSidebarBoxContacts"));
			UI.Mouse.Click(UI.TryGetControl("formMain.stripButton_New"));

			var formContact = UI.WaitForForm("formContact");
			Assert.IsNotNull(formContact);
			// Name, Surname
			UI.Mouse.Click(UI.TryGetControl("formContact.text_Overview_FullName"));
			UI.Type(string.Format("{0} {1}", Contact.TestContact.Name, Contact.TestContact.Surname));
			// Company
			UI.Mouse.Click(UI.TryGetControl("formContact.text_Overview_Company"));
			UI.Type(Contact.TestContact.Company);
			// Category combobox
			ComboBoxCategory combo = UI.TryGetControl<ComboBoxCategory>("formContact.combo_Overview_Category");
			UI.Mouse.Click(combo);
			var controlDataGrid = Utils.GetDropdownControlDataGrid(combo);
			UI.Mouse.Click(controlDataGrid, Utils.GetRowOfTheCategory(controlDataGrid, Contact.TestContact.Category));

			var contactInfoPanel = UI.TryGetControl("formContact.tableLayoutPanel_Overview_Left");
			// Add personal email
			UI.Mouse.Click(UI.TryGetControl("formContact.toolStripButton_AddEmail"));
			UI.Mouse.Click(UI.TryGetControl<ToolStripMenuItemEx>("formContact.menuItem_AddEmail_Email"));
			UI.Mouse.Click(UI.FindControl(contactInfoPanel, "text_Email_Email"));
			UI.Type(Contact.TestContact.Mails[0].Address);
			// Add work email
			UI.Mouse.Click(UI.TryGetControl("formContact.toolStripButton_AddEmail"));
			UI.Mouse.Click(UI.TryGetControl<ToolStripMenuItemEx>("formContact.menuItem_AddEmail_Work"));
			UI.Mouse.Click(UI.FindControls(contactInfoPanel, "text_Email_Email", 2)[1]);
			UI.Type(Contact.TestContact.Mails[1].Address);
			UI.Mouse.Click(UI.FindControls(contactInfoPanel, "toolStripButton_AddDisplayAs", 2)[1]);
			UI.Mouse.Click(UI.FindControls(contactInfoPanel, "text_Email_DisplayAs", 2)[1]);
			UI.Type(Contact.TestContact.Mails[1].DisplayAs);
			// Add work email
			UI.Mouse.Click(UI.TryGetControl("formContact.toolStripButton_AddEmail"));
			UI.Mouse.Click(UI.TryGetControl<ToolStripMenuItemEx>("formContact.menuItem_AddEmail_Home"));
			UI.Mouse.Click(UI.FindControls(contactInfoPanel, "text_Email_Email", 3)[2]);
			UI.Type(Contact.TestContact.Mails[2].Address);

			// Cancel
			UI.Mouse.Click(UI.TryGetControl("formContact.stripButton_Cancel"));

			var taskForm = UI.WaitForForm("taskForm");
			Assert.IsNull(taskForm);
			//MainTestSuite.ThrowIfNull(taskForm, "Confirmation dialog hasn't appeared.");
			//Thread.Sleep(2000);

			//var noButton = UI.FindControl(taskForm, "No");
			//MainTestSuite.ThrowIfNull(noButton, "'No' button not found.");
			//UI.Mouse.Click(noButton);
		}

		private void DbConnection()
		{
			//DataStore DataStore = new DataStore(MainClass.DbLocationPath, 0);
			//KeyValueStore Settings = new KeyValueStore(DataStore, "settings.dat", "GeneralSettings");
			//MailClient.Security.MasterPasswordManager MasterPasswordManager = new MailClient.Security.MasterPasswordManager(Settings);
			//MailClient.Protocols.InteractionControllerFactory ControllerFactory = new MailClient.Protocols.InteractionControllerFactory();
			//MailClient.Accounts.AccountManager AccountManager = new MailClient.Accounts.AccountManager(DataStore, Settings.GetValue("online", true), ControllerFactory, MasterPasswordManager);
		}
	}
}