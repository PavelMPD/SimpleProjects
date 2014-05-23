using System;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using PX.SM;

public partial class Page_SM260000 : PX.Web.UI.PXPage
{
	protected void Page_Load(object sender, EventArgs e)
	{
	}

	protected void form_DataBound(object sender, EventArgs e)
	{
		TranslationMaint graph = this.ds.DataGraph as TranslationMaint;

		if (graph != null && graph.IsSiteMapAltered)
		{
			this.ds.CallbackResultArg = "RefreshSitemap";
		}
	}
}
