using System;
using System.Drawing;
using System.Web.Compilation;
using PX.Data;
using PX.SM;
using PX.Web.UI;
using PX.Web.Controls;
using PX.Data.Wiki.Parser;

public partial class Page_SM201020 : PX.Web.UI.PXPage
{
	private const string _CALENDAR_SYNC_HANDLER_TYPE = "PX.Objects.EP.PXCalendarSyncHandler";
	private static readonly System.Reflection.MethodInfo _getSyncUrlMethod;

	static Page_SM201020()
	{
		Type syncHandlerType = BuildManager.GetType(_CALENDAR_SYNC_HANDLER_TYPE, false);
		if (syncHandlerType != null)
			_getSyncUrlMethod = syncHandlerType.GetMethod("GetSyncUrl", new Type[] { typeof(string), typeof(string), typeof(string), typeof(string) });
	}

	protected void Page_Init(object sender, EventArgs e)
	{
		if (tab != null && _getSyncUrlMethod == null)
		{
			PXTabItem item = tab.Items["calendar"];
			if (item != null) item.Visible = false;
		}
	}

	protected void tab_Init(object sender, EventArgs e)
	{
		bool existMyProfileMaint = System.String.Compare(ds.TypeName, "PX.SM.MyProfileMaint", StringComparison.OrdinalIgnoreCase) == 0;
		tab.Items.Remove(tab.Items[existMyProfileMaint ? "searchSettingsSimple" : "searchSettings"]);
	}

	protected void tab_DataBound(object sender, EventArgs e)
	{
		PXButton button = (PXButton)this.tab.FindControl("btnChangePassword");
		if (button != null)
		{
			PXSmartPanel panel = (PXSmartPanel)this.tab.FindControl("pnlChangePassword");
			PXTextEdit edit = (PXTextEdit)panel.FindControl("edNewPassword");
			if (edit != null)
			{
				button.Enabled = edit.Enabled;
				button.Hidden = edit.Hidden;
			}
		}

		int index;
		if (int.TryParse(Request["tab"], out index) && index < this.tab.Items.Count)
			this.tab.SelectedIndex = index;
	}

	protected void cmdCheckMailSettings_CallBack(object sender, PXCallBackEventArgs e)
	{
		try
		{
			((SMAccessPersonalMaint)ds.DataGraph).getCalendarSyncUrl(
				new PXAdapter(ds.DataGraph.Views[ds.PrimaryView]));
		}
		catch (PXDialogRequiredException ex)
		{
			if (_getSyncUrlMethod != null)
			{
				ex.SetMessage((string)_getSyncUrlMethod.Invoke(null,
						new object[] { Page.Request.Url.Scheme, Page.Request.Url.Host, Page.Request.ApplicationPath, ex.Message }));
				ex.DataSourceID = ds.ID;
			}
			throw ex;
		}
	}
}
