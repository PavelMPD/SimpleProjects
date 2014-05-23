using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using PX.Common;
using PX.Web.UI;
using System.Web;
using PX.Data;

public partial class Frames_Error : Page
{
    protected void Page_Init(object sender, EventArgs e)
    {
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        string exceptionID = Request.Params["exceptionID"];
		string errorcode = Request.Params["errorcode"];
		string typeID = Request.Params["typeID"];

	    if (typeID != null)
	    {
		    SetUserFriendlyInfoWarning(errorcode, exceptionID);
	    }
	    else
	    {
			if (exceptionID != null && PXContext.Session.Exception[exceptionID] != null)
			{
				PXException exception = (PXException)PXContext.Session.Exception[exceptionID];
				PXSetupNotEnteredException setPropException = exception as PXSetupNotEnteredException;

				if (setPropException != null)
				{
					string url = GetNextUrl(ref setPropException);
					if (url != null)
					{
						url = PXPageCache.FixPageUrl(ControlHelper.FixHideScriptUrl(url, null));
					}
					SetUserFriendlyInfo(url, setPropException.NavigateTo, setPropException.ExceptionNumber.ToString(), setPropException.MessageNoPrefix);
				}
				else
				{
					SetDefaultExceptionInfo(exception.ExceptionNumber.ToString(), exception.MessageNoPrefix);
				}
			}
			else
			{
				SetDefaultVisibility();
			}
	    }
        this.Server.ClearError();
    }

    //Gets url to navigate for entering required data
    private string GetNextUrl(ref PXSetupNotEnteredException exception)
    {
        Type graphType = null;
        PXGraph gettingCache = new PXGraph();
        bool createInstanceError = true;

        //Get graph that user must use at first
        while (createInstanceError)
        {
            createInstanceError = false;
            PXPrimaryGraphBaseAttribute attr = PXPrimaryGraphAttribute.FindPrimaryGraph(gettingCache.Caches[exception.DAC], out graphType);

            if (graphType != null)
            {
                try
                {
                    PXGraph tmpGraph = PXGraph.CreateInstance(graphType) as PXGraph;
                }
                catch (PXSetupNotEnteredException ctrException)
                {
                    createInstanceError = true;
                    exception = ctrException;
                }
            }
        }

		try
		{
			return graphType == null ? null : PXBaseDataSource.getMainForm(graphType);
		}
		//we cang get url if we don't have rights to the screen
		catch
		{
			return null;				
		}
    }

    //Fill controls with user friendly error info
    private void SetUserFriendlyInfo(string url, string navigateTo, string exceptionNumber, string exeptionMessage)
    {
        ((HyperLink)this.frmBottom.FindControl("lnkTrace")).Visible = false;
        ((Image)this.frmBottom.FindControl("imgMessage")).ImageUrl = "~/App_Themes/Default/Images/Wiki/Warn.png";
        ((PXLabel)this.frmBottom.FindControl("lblErrCode")).Text = string.Format(ErrorMessages.GetLocal(ErrorMessages.ErrorNumber), exceptionNumber);
		((PXLabel)this.frmBottom.FindControl("lblMessage")).Text = string.Format("{0}{1}{2}", ErrorMessages.GetLocal(ErrorMessages.SetupNotEnteredPrefix),
																							  exeptionMessage,
																							  ErrorMessages.GetLocal(ErrorMessages.SetupNotEnteredSuffix));

        HyperLink hlNavTo = this.frmBottom.FindControl("hlNavTo") as HyperLink;
        PXLabel lblNxStep = this.frmBottom.FindControl("lblNxStep") as PXLabel;
        PXLabel lblNavTo = this.frmBottom.FindControl("lblNavTo") as PXLabel;
        PXLabel lblNavToEnding = this.frmBottom.FindControl("lblNavToEnding") as PXLabel;

        if (url != null)
        {
            hlNavTo.Text = navigateTo;
            hlNavTo.NavigateUrl = url;

			lblNxStep.Text = ErrorMessages.GetLocal(ErrorMessages.NextStep);
			lblNavTo.Text = ErrorMessages.GetLocal(ErrorMessages.NavTo);
			lblNavToEnding.Text = ErrorMessages.GetLocal(ErrorMessages.NavToSuffix);
        }
        else
        {
            lblNxStep.Visible = false;
            lblNavTo.Visible = false;
            lblNavToEnding.Visible = false;
            hlNavTo.Visible = false;
        }
    }

    //Hide controls which used to show user friendly info
    private void SetDefaultVisibility()
    {
        ((PXLabel)this.frmBottom.FindControl("lblErrCode")).Visible = false;
        ((PXLabel)this.frmBottom.FindControl("lblNxStep")).Visible = false;
        ((PXLabel)this.frmBottom.FindControl("lblNavTo")).Visible = false;
        ((PXLabel)this.frmBottom.FindControl("lblNavToEnding")).Visible = false;
        ((HyperLink)this.frmBottom.FindControl("hlNavTo")).Visible = false;
    }

    //Fill controls with default exception info
    private void SetDefaultExceptionInfo(string exceptionNumber, string exeptionMessage)
    {
        ((PXLabel)this.frmBottom.FindControl("lblNxStep")).Visible = false;
        ((PXLabel)this.frmBottom.FindControl("lblNavTo")).Visible = false;
        ((PXLabel)this.frmBottom.FindControl("lblNavToEnding")).Visible = false;
        ((HyperLink)this.frmBottom.FindControl("hlNavTo")).Visible = false;

		((PXLabel)this.frmBottom.FindControl("lblErrCode")).Text = string.Format(ErrorMessages.GetLocal(ErrorMessages.ErrorNumber), exceptionNumber);
        ((PXLabel)this.frmBottom.FindControl("lblMessage")).Text = exeptionMessage;
    }

	private void SetUserFriendlyInfoWarning(string errorcode,string exeptionMessage)
	{
		
		((Image)this.frmBottom.FindControl("imgMessage")).ImageUrl = "~/App_Themes/Default/Images/Wiki/Warn.png";
		((HyperLink)this.frmBottom.FindControl("lnkTrace")).Visible = false;
		((PXLabel)this.frmBottom.FindControl("lblNxStep")).Visible = false;
		((PXLabel)this.frmBottom.FindControl("lblNavTo")).Visible = false;
		((PXLabel)this.frmBottom.FindControl("lblNavToEnding")).Visible = false;
		((HyperLink)this.frmBottom.FindControl("hlNavTo")).Visible = false;
		if (!String.IsNullOrEmpty(errorcode))
			((PXLabel) this.frmBottom.FindControl("lblErrCode")).Text = errorcode;
		else
			((PXLabel) this.frmBottom.FindControl("lblErrCode")).Visible = false;
		if (!String.IsNullOrEmpty(exeptionMessage))
			((PXLabel)this.frmBottom.FindControl("lblMessage")).Text = exeptionMessage;
		else
			((PXLabel)this.frmBottom.FindControl("lblMessage")).Visible = false;
	}
}