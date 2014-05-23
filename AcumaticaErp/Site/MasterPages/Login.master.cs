using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PX.Data;

public partial class MasterPages_Login : System.Web.UI.MasterPage
{
	/// <summary>
	/// Gets or sets the message text.
	/// </summary>
	public string Message
	{
		get { return this.lblMsg.Text; }
		set { this.lblMsg.Text = value; }
	}

	/// <summary>
	/// 
	/// </summary>
	protected void Page_Init(object sender, EventArgs e)
	{
		this.Response.AddHeader("cache-control", "no-store, private");
	}

	/// <summary>
	/// 
	/// </summary>
	protected void Page_Load(object sender, EventArgs e)
	{
		string copyR = PXVersionInfo.Copyright;
		String version = PXMessages.LocalizeFormatNoPrefix(PX.AscxControlsMessages.PageTitle.Version, PXVersionInfo.Version);

		if (!PX.Data.Update.PXUpdateHelper.VersionsEquals()) version = "<b style=\"color:red\">" + version + "</b>";
		if (!PX.Data.Update.PXUpdateHelper.ChectUpdateStatus())
		{
			version = "<b style=\"color:red\">" + version + "</b>";
		}
		lblCopy.Text = copyR + " " + version;

		string cstProjects = Customization.CstWebsiteStorage.PublishedProjectList;
		if (!string.IsNullOrEmpty(cstProjects))
		{
			this.lblCstProjects.Visible = true;
			this.lblCstProjects.Text = string.Format("Customized: {0}", cstProjects.Replace(",", ", "));
		}
		this.CorrectCssUrl();
	}

	/// <summary>
	/// The page PreRender event handler.
	/// </summary>
	protected override void OnPreRender(EventArgs e)
	{
		int index = -1;
		if (!this.IsPostBack || string.IsNullOrEmpty(txtLoginBgIndex.Value))
		{
			var r = new Random(); index = Math.Min(r.Next(1, 8), 7);
			txtLoginBgIndex.Value = index.ToString();
		}
		else index = int.Parse(txtLoginBgIndex.Value);

		string url = this.ResolveUrl(string.Format("../Icons/login_bg{0}.jpg", index));
		this.Page.ClientScript.RegisterClientScriptBlock(
			this.GetType(), "LoginImage", string.Format("var __loginBg = '{0}';", url), true);
		
		base.OnPreRender(e);
	}

	/// <summary>
	/// Append timestamp to CSS file url.
	/// </summary>
	private void CorrectCssUrl()
	{
		foreach (Control ctrl in this.Page.Header.Controls)
		{
			var link = ctrl as System.Web.UI.HtmlControls.HtmlLink;
			if (link != null && !string.IsNullOrEmpty(link.Href) && !link.Href.Contains("timestamp"))
			{
				string filePath = link.Href.Replace("~/", this.Request.PhysicalApplicationPath).Replace('/', '\\');
				bool exists = File.Exists(filePath);
				if (exists) link.Href += string.Format("?timestamp={0}", File.GetLastWriteTimeUtc(filePath).Ticks.ToString());
			}
		}
	}
}
