using System;
using PX.Web.UI;
using PX.Data;

public partial class Pages_SM_SM201510 : PXPage
{
    protected void Page_Load(object sender, EventArgs e)
    {
		PXLabel label = frmInstall.FindControl("lblDisclamer") as PXLabel;
		label.Text = PX.Data.PXMessages.LocalizeNoPrefix(PX.Data.ActionsMessages.LicenseDisclamer).Replace(Environment.NewLine, "<BR>");

		PXLabel warning = frmWarning.FindControl("edWarningText") as PXLabel;
		warning.Text = PX.Data.PXMessages.LocalizeNoPrefix(PX.SM.Messages.LSWarningMessage).Replace(Environment.NewLine, "<BR>");
    }
}
