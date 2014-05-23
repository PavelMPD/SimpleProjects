using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using PX.Web.UI;
using PX.Data;
using PX.Objects.IN;

public partial class Page_IN402000 : PX.Web.UI.PXPage
{
	protected void Page_Init(object sender, EventArgs e)
	{
		this.Master.PopupWidth = 960;
		this.Master.PopupHeight = 600;
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		if (!this.IsCallback)
		{
			PXLabel lbl1 = this.form.FindControl("lblNote1") as PXLabel;
			PXLabel lbl2 = this.form.FindControl("lblNote2") as PXLabel;
			if (lbl1 != null)
				lbl1.Text = PXMessages.LocalizeNoPrefix(Messages.ExceptLocationNotAvailable);
			if (lbl2 != null)
				lbl2.Text = PXMessages.LocalizeNoPrefix(Messages.ExceptExpiredNotAvailable);
		}
	}
}
