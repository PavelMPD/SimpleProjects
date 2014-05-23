using System;
using System.Collections.Generic;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.Design;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using PX.Data;
using PX.Data.Wiki.Parser;
using PX.Objects.CR;
using PX.Web.UI;
using PX.SM;
using PX.TM;
using PX.Web.UI.Design;
using PX.Web.Controls;

public partial class Page_EP206000 : PX.Web.UI.PXPage
{
	protected void Page_Init(object sender, EventArgs e)
	{
		this.Master.PopupWidth = 980;
		this.Master.PopupHeight = 650;
		Master.ScreenID = "EP.20.60.00";
	}

	protected void Page_Load(object sender, EventArgs e)
	{
	}
}
