using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using System.Threading;
using System.Globalization;
using PX.Common;
using PX.Data;
using PX.Data.Maintenance;
using PX.SM;

public partial class Frames_Login : System.Web.UI.Page
{
	#region Event handlers

	/// <summary>
	/// 
	/// </summary>
	protected void Page_Init(object sender, EventArgs e)
	{
		InitialiseRemindLink();

		// if we have troubles with this functions and it is not postback
		// then we should notify user about problems with database
		try
		{
			FillCompanyCombo();
			FillLocalesCombo();
		}
		catch
		{
			if (GetPostBackControl(this.Page) == null)
			{
				this.btnLogin.Visible = false;
				this.Master.Message = "Database could not be accessed";
			}
		}
	}

	/// <summary>
	/// 
	/// </summary>
	protected void Page_Load(object sender, EventArgs e)
	{
		if (PX.Data.Update.PXUpdateHelper.CheckUpdateLock()) throw new PXUnderMaintenanceException();

		var lockoutStatus = PXSiteLockout.GetStatus(true);
		if (lockoutStatus == PXSiteLockout.Status.Locked)
		{
			lblUnderMaintenance.Text = PXMessages.Localize(PX.Data.Update.Messages.SiteUnderMaintenance);
			lblUnderMaintenance.Visible = true;

			if (!string.IsNullOrWhiteSpace(PXSiteLockout.Message))
			{
				lblUnderMaintenanceReason.Text = string.Format(
					PXMessages.Localize(PX.Data.Update.Messages.LockoutReason), PXSiteLockout.Message);
				lblUnderMaintenanceReason.Visible = true;
			}
		}

		if (lockoutStatus == PXSiteLockout.Status.Pending)
		{
			string datetime = string.Format("{0} ({1} UTC)", PXSiteLockout.DateTime, PXSiteLockout.DateTimeUtc);
			lblUnderMaintenance.Text = string.Format(
				PXMessages.Localize(PX.Data.Update.Messages.PendingLockout),

				datetime, PXSiteLockout.Message);
			lblUnderMaintenance.Visible = true;
		}

		if (GetPostBackControl(this.Page) == cmbCompany) txtPass.Attributes.Add("value", txtPass.Text);

		if (GetPostBackControl(this.Page) == btnLogin && !String.IsNullOrEmpty(txtDummyCpny.Value))
			cmbCompany.SelectedValue = txtDummyCpny.Value;

		// if user already set password then we should disabling login and password
		if (!String.IsNullOrEmpty(txtVeryDummyPass.Value))
		{
			txtPass.Text = txtVeryDummyPass.Value;
			DisablingUserPassword();
		}

		// if (SecureCompanyID) then we should hide combobox before first login.
		// and also we should shrink companies list
		if (PXDatabase.SecureCompanyID && (Membership.Provider is IPasswordValidator))
		{
			this.cmbCompany.Visible = !String.IsNullOrEmpty(txtVeryDummyPass.Value);

			if (!String.IsNullOrEmpty(txtVeryDummyPass.Value))
			{
				List<String> companyFilter = new List<String>(PXAccess.GetCompanies(txtUser.Text, txtVeryDummyPass.Value));
				for (int i = cmbCompany.Items.Count - 1; i >= 0; i--)
				{
					ListItem item = cmbCompany.Items[i];
					if (!companyFilter.Contains(item.Value)) cmbCompany.Items.RemoveAt(i);
				}
			}
		}

		// Is user trying to recover his password using link from Email?
		if (Request.QueryString.AllKeys.Length > 0 && Request.QueryString.GetValues("gk") != null)
		{
			RemindUserPassword();
		}
		try
		{
			this.SetInfoText();
		}
		catch { /*SKIP ERROS*/ }
	}

	/// <summary>
	/// Fill the info about system,
	/// </summary>
	private void SetInfoText()
	{
		string copyR = PXVersionInfo.Copyright;
		txtDummyInstallationID.Value = PXLicenseHelper.InstallationID;

		bool hasError = false;
		if (!PX.Data.Update.PXUpdateHelper.ChectUpdateStatus())
		{
			this.updateError.Style["display"] = "";
			hasError = true;
		}

		if (Request.QueryString["licenseexceeded"] != null)
		{
			this.logOutReasone.Style["display"] = "";
			this.logOutReasoneMsg.InnerText = PXMessages.LocalizeFormatNoPrefix(
				PX.Data.ActionsMessages.LogoutReason, Request.QueryString["licenseexceeded"]);
			hasError = true;
		}
        else if (PXDatabase.Companies.Length > PXDatabase.AvailableCompanies.Length)
        {
            this.logOutReasone.Style["display"] = "";
            this.logOutReasoneMsg.InnerText = PXMessages.LocalizeNoPrefix(PX.Data.ActionsMessages.CompaniesOverlimit);
            hasError = true;
        }

		// sets the customization info text
		string status = Customization.CstWebsiteStorage.GetUpgradeStatus();
		if (!String.IsNullOrEmpty(status))
		{

			this.customizationError.Style["display"] = "";
			this.custErrorContent.InnerText = status;
			hasError = true;
		}
		login_info.Style[HtmlTextWriterStyle.Display] = hasError ? "" : "none";
	}

	/// <summary>
	/// The page Init event handler.
	/// </summary>
	protected override void OnInit(EventArgs e)
	{
		base.OnInit(e);
	}
	#endregion

	#region Login methods

	/// <summary>
	/// The login button event handler.
	/// </summary>
	protected void btnLogin_Click(object sender, EventArgs e)
	{
		try
		{
			string loginText = txtUser.Text;
			if (loginText != null && loginText.Contains(":"))
			{
				this.Master.Message = PXMessages.LocalizeNoPrefix(PX.AscxControlsMessages.LoginScreen.IncorrectLoginSymbols);
				return;
			}
			if (String.IsNullOrEmpty(loginText))
			{
				this.Master.Message = PXMessages.LocalizeNoPrefix(PX.AscxControlsMessages.LoginScreen.InvalidLogin);
				return;
			}

			if (String.IsNullOrEmpty(txtNewPassword.Text) && String.IsNullOrEmpty(txtConfirmPassword.Text))
			{
				string[] companies = null;
				if (PXDatabase.SecureCompanyID && (Membership.Provider is IPasswordValidator)
					&& String.IsNullOrEmpty(txtVeryDummyPass.Value)
					&& (companies = PXAccess.GetCompanies(loginText, txtPass.Text)).Length > 1)
				{
					SecureLogin(companies);
				}
				else
				{
					NormalLogin(companies);
				}
			}
			else //if user should change it password than we will login different way
			{
				ChangingPassord();
			}
		}
		catch (PXException ex)
		{
			this.Master.Message = ex.MessageNoPrefix;
		}
		catch (System.Reflection.TargetInvocationException ex)
		{
			this.Master.Message = PXException.ExtractInner(ex).Message;
		}
		catch (Exception ex)
		{
			this.Master.Message = ex.Message;
		}
	}

	//-----------------------------------------------------------------------------
	/// <summary>
	/// 
	/// </summary>
	private void NormalLogin(string[] companies)
	{
		if (companies != null && companies.Length == 1)
		{
			cmbCompany.Items.Clear();
			cmbCompany.Items.Add(companies[0]);
		}

		string loginText = txtUser.Text.Trim();
		string userName = PXDatabase.Companies.Length > 0 ? loginText + "@" +
			(cmbCompany.SelectedIndex != -1 ? cmbCompany.SelectedItem.Value : PXDatabase.Companies[0]) : loginText;

		if (!PXLogin.LoginUser(ref userName, txtPass.Text))
		{
			// we will change password during next round-trip
			PXContext.Session.SetString("ChangingPassword", txtPass.Text);

			DisablingUserPassword();
			EnablingChangingPassword();

			this.Master.Message = string.Empty;
		}
		else
		{
			PXLogin.InitUserEnvironment(userName, cmbLang.SelectedValue);
		}
	}

	/// <summary>
	/// 
	/// </summary>
	protected void SecureLogin(string[] companies)
	{
		this.cmbCompany.Items.Clear();
		for (int i = 0; i < companies.Length; i++) this.cmbCompany.Items.Add(companies[i]);

		HttpCookie cookie = Request.Cookies["CompanyID"];
		if (cookie != null && !string.IsNullOrEmpty(cookie.Value) &&
			this.cmbCompany.Items.FindByValue(cookie.Value) != null)
		{
			this.cmbCompany.SelectedValue = cookie.Value;
		}
		else if (this.cmbCompany.Items.Count > 0)
		{
			this.cmbCompany.SelectedValue = this.cmbCompany.Items[0].Value;
		}

		DisablingUserPassword();
		this.cmbCompany.Visible = true;
		//this.Master.Message = PX.Data.PXMessages.LocalizeNoPrefix(PX.AscxControlsMessages.LoginScreen.PleaseSelectCompany);
	}

	//-----------------------------------------------------------------------------
	/// <summary>
	/// Perform the user password changing.
	/// </summary>
	protected void ChangingPassord()
	{
		string loginText = txtUser.Text;
		if (txtRecoveryAnswer.Visible && !PXLogin.ValidateAnswer(PXDatabase.Companies.Length > 0 ?
			loginText + "@" + cmbCompany.SelectedItem.Value : loginText, txtRecoveryAnswer.Text))
		{
			this.Master.Message = PX.Data.PXMessages.LocalizeNoPrefix(PX.AscxControlsMessages.LoginScreen.InvalidRecoveryAnswer);
		}
		if (txtNewPassword.Text != txtConfirmPassword.Text)
		{
			this.Master.Message = PX.Data.PXMessages.LocalizeNoPrefix(PX.AscxControlsMessages.LoginScreen.PasswordNotConfirmed);
		}
		if ((string)PXContext.Session["ChangingPassword"] == txtNewPassword.Text)
		{
			this.Master.Message = PX.Data.PXMessages.LocalizeNoPrefix(PX.AscxControlsMessages.LoginScreen.NewPasswordMustDiffer);
		}
		if (string.IsNullOrEmpty(txtNewPassword.Text))
		{
			this.Master.Message = PX.Data.PXMessages.LocalizeNoPrefix(PX.AscxControlsMessages.LoginScreen.PasswordBlank);
		}

		if (!String.IsNullOrEmpty(this.Master.Message))
		{
			txtVeryDummyPass.Value = (string)PXContext.Session["ChangingPassword"];
			DisablingUserPassword();
			EnablingChangingPassword();
			return;
		}

		string userName = PXDatabase.Companies.Length > 0
			? loginText + "@" + (cmbCompany.SelectedIndex != -1 ? cmbCompany.SelectedItem.Value : PXDatabase.Companies[0])
			: loginText;

		PXLogin.LoginUser(
			ref userName,
			Request.QueryString.Get("gk") ?? (string)PXContext.Session["ChangingPassword"],
			txtNewPassword.Text);

		PXLogin.InitUserEnvironment(userName, cmbLang.SelectedValue);
	}

	#endregion

	#region Private methods

	//-----------------------------------------------------------------------------
	/// <summary>
	/// Fill the system locales drop-down.
	/// </summary>
	private void FillLocalesCombo()
	{
		try
		{
			if (cmbLang.Items.Count != 0) return;

			Boolean found = false;
			PXLocale[] locales = PXLocalesProvider.GetLocales(
				(!String.IsNullOrEmpty(txtUser.Text) ? txtUser.Text : "temp") + (PXDatabase.Companies.Length > 0 ? "@" +
				(cmbCompany.SelectedIndex != -1 ? cmbCompany.SelectedItem.Value : PXDatabase.Companies[0]) : ""));

			foreach (PXLocale loc in locales)
			{
				ListItem item = new ListItem(loc.DisplayName, loc.Name);
				cmbLang.Items.Add(item);
				if (!found && Request.Cookies["Locale"] != null && Request.Cookies["Locale"]["Culture"] != null &&
					string.Compare(Request.Cookies["Locale"]["Culture"], item.Value, true) == 0)
				{
					cmbLang.SelectedValue = item.Value;
					found = true;
				}
			}

			String value = this.Request.Form[cmbLang.ClientID.Replace('_', '$')];
			if (!String.IsNullOrEmpty(value) && locales.Any(l => l.Name == value))
			{
				cmbLang.SelectedValue = value;
				found = true;
			}
			if (cmbLang.Items.Count == 1) cmbLang.Style[HtmlTextWriterStyle.Display] = "none";
		}
		catch
		{
			cmbLang.Visible = false;
			this.btnLogin.Visible = false;
			this.Master.Message = "Database could not be accessed";
		}
	}

	//-----------------------------------------------------------------------------
	/// <summary>
	/// Fill the allowed companies drop-down.
	/// </summary>
	private void FillCompanyCombo()
	{
		string[] companies = PXDatabase.AvailableCompanies;
		if (companies.Length == 0)
		{
			this.cmbCompany.Visible = false;
		}
		else
		{
			this.cmbCompany.Items.Clear();
			for (int i = 0; i < companies.Length; i++) this.cmbCompany.Items.Add(companies[i]);

			if (companies.Length == 1)
			{
				this.cmbCompany.Visible = false;
				this.cmbCompany.SelectedValue = this.cmbCompany.Items[0].Value;
			}
			else
			{
				HttpCookie cookie = this.Request.Cookies["CompanyID"];
				if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
					this.cmbCompany.SelectedValue = cookie.Value;
			}
		}
	}

	/// <summary>
	/// Sets the password reminder url.
	/// </summary>
	private void InitialiseRemindLink()
	{
		string target = HttpUtility.UrlEncode(Request.Url.AbsolutePath);
		string path = PX.Common.PXUrl.SiteUrlWithPath();

		path += path.EndsWith("/") ? "" : "/";
		if (Request.QueryString.Keys.Count != 0)
			lnkForgotPswd.NavigateUrl = path + "Frames/PasswordRemind.aspx" + Request.Url.Query + "&Target=" + target;
		else
			lnkForgotPswd.NavigateUrl = path + "Frames/PasswordRemind.aspx?" + "Target=" + target;
	}

	//-----------------------------------------------------------------------------
	/// <summary>
	/// Disable the password field.
	/// </summary>
	private void DisablingUserPassword()
	{
		txtPass.ReadOnly = txtUser.ReadOnly = true;
		txtPass.BackColor = txtUser.BackColor = System.Drawing.Color.LightGray;

		if (!String.IsNullOrEmpty(txtPass.Text))
		{
			txtVeryDummyPass.Value = txtPass.Text;
			txtPass.Attributes.Add("value", txtPass.Text);
		}
	}

	/// <summary>
	/// Activate the password change mode.
	/// </summary>
	private void EnablingChangingPassword()
	{
		if (cmbCompany.SelectedIndex != -1) txtDummyCpny.Value = cmbCompany.SelectedItem.Text;
		cmbCompany.Enabled = cmbLang.Enabled = false;
		txtNewPassword.Visible = txtConfirmPassword.Visible = true;
		lnkForgotPswd.Visible = false;
	}

	/// <summary>
	/// 
	/// </summary>
	private static Control GetPostBackControl(Page page)
	{
		Control control = null;
		string ctrlname = page.Request.Params.Get("__EVENTTARGET");
		if (ctrlname != null && ctrlname != string.Empty)
		{
			control = page.FindControl(ctrlname);
		}
		else
		{
			foreach (string ctl in page.Request.Form)
			{
				Control c = page.FindControl(ctl);
				if (c is System.Web.UI.WebControls.Button) { control = c; break; }
			}
		}
		return control;
	}

	//-----------------------------------------------------------------------------
	/// <summary>
	/// 
	/// </summary>
	private void RemindUserPassword()
	{
		string login = "";
		string cid = null;
		if (PXDatabase.Companies.Length > 0 && Request.QueryString.GetValues("cid") != null)
		{
			cid = Request.QueryString.Get("cid");
			login = "temp@" + cid;
		}
		string username = PXLogin.FindUserByHash(Request.QueryString.Get("gk"), login);
		if (username != null)
		{
			txtUser.Text = username;
			txtPass.Text = Request.QueryString.Get("gk");
			txtUser.ReadOnly = true;
			txtUser.BackColor = System.Drawing.Color.LightGray;

			lnkForgotPswd.Visible = false;
			txtPass.Visible = false;
			txtPass.TextMode = TextBoxMode.SingleLine;
			txtDummyPass.Text = txtPass.Text;
			txtDummyPass.Visible = true;
			txtNewPassword.Visible = txtConfirmPassword.Visible = true;

			txtRecoveryQuestion.Text = PXLogin.FindQuestionByUsername(username, login);
			if (!string.IsNullOrEmpty(txtRecoveryQuestion.Text))
			{
				txtRecoveryQuestion.ReadOnly = true;
				txtRecoveryQuestion.BackColor = System.Drawing.Color.LightGray;
				txtRecoveryQuestion.Visible = true;
				txtRecoveryAnswer.Visible = true;
			}

			//this.Master.Message = PX.Data.PXMessages.LocalizeNoPrefix(PX.AscxControlsMessages.LoginScreen.PleaseChangePassword);
			if (cid != null)
			{
				this.cmbCompany.SelectedValue = cid;
				this.cmbCompany.Enabled = false;
				this.txtDummyCpny.Value = this.cmbCompany.SelectedValue;
			}
		}
	}
	#endregion
}