using System;
using System.Drawing;
using System.Web.UI.WebControls;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.EP;

public partial class Pages_CR_CO000000 : PX.Web.UI.PXPage
{
	protected void Page_Load(object sender, EventArgs e)
	{
		RegisterStyle("BaseBold", null, null, true);
	}

	private void RegisterStyle(string name, string backColor, string foreColor, bool bold)
	{
		Style style = new Style();
		if (!string.IsNullOrEmpty(backColor)) style.BackColor = Color.FromName(backColor);
		if (!string.IsNullOrEmpty(foreColor)) style.ForeColor = Color.FromName(foreColor);
		if (bold) style.Font.Bold = true;
		this.Page.Header.StyleSheet.CreateStyleRule(style, this, "." + name);
	}

	protected void grid_RowDataBound(object sender, PX.Web.UI.PXGridRowEventArgs e)
	{
		PXResult record = e.Row.DataItem as PXResult;
		if (record == null) return;
		EPView viewInfo = (EPView)record[typeof(EPView)];
		bool isBold = viewInfo != null && (viewInfo.Status == null || viewInfo.Status == EPViewStatusAttribute.NOTVIEWED);
		EPActivity item = (EPActivity)record[typeof(EPActivity)];
		if (item.Subject.Length > 35)
			item.Subject = item.Subject.Substring(0, 35) + "...";
		if (isBold) e.Row.Style.CssClass = "BaseBold";
	}

	protected void grid_RowDataBoundApprovals(object sender, PX.Web.UI.PXGridRowEventArgs e)
	{
		/*PXResult record = e.Row.DataItem as PXResult;
		if (record == null) return;
		EPView viewInfo = (EPView)record[typeof(EPView)];
		bool isBold = viewInfo != null && (viewInfo.Status == null || viewInfo.Status == EPViewStatusAttribute.NOTVIEWED);
		EPExpenseClaim item = (EPExpenseClaim)record[typeof(EPExpenseClaim)];
		if (item..Length > 35)
			item.Subject = item.Subject.Substring(0, 35) + "...";
		if (isBold) e.Row.Style.CssClass = "BaseBold";*/
	}

	/*protected void grid_RowDataBoundAnn(object sender, PX.Web.UI.PXGridRowEventArgs e)
	{
		PXResult record = e.Row.DataItem as PXResult;
		if (record == null) return;
		EPView viewInfo = (EPView)record[typeof(EPView)];
		bool isBold = viewInfo != null && (viewInfo.Status == null || viewInfo.Status == EPViewStatusAttribute.NOTVIEWED);
		
		CRAnnouncement item = (CRAnnouncement)record[typeof(CRAnnouncement)];
		item
		if (item.Subject.Length > 35)
			item.Subject = item.Subject.Substring(0, 35) + "...";
		if (isBold) e.Row.Style.CssClass = "BaseBold";
	}*/
}