using System;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using PX.Common;
using PX.Data;
using PX.Web.UI;

public partial class Page_GenericInquiry : PX.Web.UI.PXPage
{
	public string CustomInquiryName;

	protected void Page_Init(object sender, EventArgs e)
	{
		// application code can set preferable inquiry from outside
		if (!string.IsNullOrEmpty(this.CustomInquiryName))
			this.ds.InquiryName = this.CustomInquiryName;

		form.DataBinding += form_DataBinding;
		((IPXMasterPage)this.Master).CustomizationAvailable = false;
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		PXContext.SetSlot<Guid?>("__GEN_INQ_DESIGN_ID__", ((PXGenericInqGrph)ds.DataGraph).Design.DesignID);	
	}

	protected void form_DataBinding(object sender, EventArgs e)
	{
		PXGenericInqGrph graph = (PXGenericInqGrph)this.DefaultDataSource.DataGraph;
		IPXMasterPage master = Page.Master as IPXMasterPage;

		if (master != null && graph != null && graph.Design != null)
		{
			master.ScreenTitle = graph.Design.SitemapTitle ?? graph.Design.Name;
		}
	}
}
