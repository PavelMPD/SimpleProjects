using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using PX.Web.UI;

public partial class Page_GL102000 : PX.Web.UI.PXPage
{
	protected void Page_Load(object sender, EventArgs e)
	{
		PXGroupBox gb = (PXGroupBox)this.form.FindControl("gbCOAOrder");
		((PXRadioButton)gb.FindControl("gbCOAOrder_op0")).Text = PX.Data.PXMessages.LocalizeNoPrefix(PX.Objects.GL.Messages.COAOrderOp0);
		((PXRadioButton)gb.FindControl("gbCOAOrder_op1")).Text = PX.Data.PXMessages.LocalizeNoPrefix(PX.Objects.GL.Messages.COAOrderOp1);
		((PXRadioButton)gb.FindControl("gbCOAOrder_op2")).Text = PX.Data.PXMessages.LocalizeNoPrefix(PX.Objects.GL.Messages.COAOrderOp2);
		((PXRadioButton)gb.FindControl("gbCOAOrder_op3")).Text = PX.Data.PXMessages.LocalizeNoPrefix(PX.Objects.GL.Messages.COAOrderOp3);
		((PXRadioButton)gb.FindControl("gb_COAOrder_op128")).Text = PX.Data.PXMessages.LocalizeNoPrefix(PX.Objects.GL.Messages.COAOrderOp128);

		((Label)this.form.FindControl("lblPeriods")).Text = PX.Data.PXMessages.LocalizeNoPrefix(PX.Objects.GL.Messages.Periods);
	}
}
