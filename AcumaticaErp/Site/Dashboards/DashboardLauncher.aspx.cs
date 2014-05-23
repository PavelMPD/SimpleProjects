using System;
using PX.Data.Wiki.Parser;
using PX.Web.Controls;

public partial class Pages_DashboardLauncher : PX.Web.UI.PXPage
{
	//private PXDashboardContainerBase _dashContainer;

	protected override void OnPreInit(EventArgs e)
	{
		Master.ScreenID = null;
		Master.ScreenTitle = null;

		DashboardContainer.NodeId = Request.QueryString["ID"];
		DashboardContainer.InDashDesignMode = (string)PX.Common.PXContext.Session["DashDesign:" + Page.AppRelativeVirtualPath] == "True";

		PXDashboardContainer.WikiSettings = new PXWikiSettings(this).Relative;

		base.OnPreInit(e);
	}

	private PXDashboardContainerBase DashboardContainer
	{
		get
		{

			dashSet.Visible = false;
			return flowDashSet;
		}
	}
}
