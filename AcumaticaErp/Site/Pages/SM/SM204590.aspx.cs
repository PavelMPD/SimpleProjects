using System.Web;
using PX.Data;
using PX.Web.UI;

[Customization.CstDesignMode(Disabled = true)]
public partial class Pages_SM_SM204590 : PXPage
{
	//protected void Page_Load(object sender, EventArgs e)
	//{

	//}

	public string GetScriptName(string rname)
	{
		string resource = "PX.Web.Customization.Controls.cseditor." + rname;
		string url = ClientScript.GetWebResourceUrl(typeof(Customization.WebsiteEntryPoints), resource);
		url = url.Replace(".axd?", ".axd?file=" + rname + "&");
		return HttpUtility.HtmlAttributeEncode(url);
		//			return VirtualPathUtility.GetFileName(url);

	}
}
