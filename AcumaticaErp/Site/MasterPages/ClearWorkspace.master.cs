using System;

public partial class Master_ClearWorkspace : PX.Web.UI.BaseMasterPage
{
	protected void Page_Load(object sender, EventArgs e)
	{
		Response.AddHeader("cache-control", "no-store, private");
	}

	#region Public properties
	/// <summary>
	/// Gets or sets the screen title string.
	/// </summary>
	public string ScreenTitle
	{
		get { return Page.Title; }
		set { Page.Title = value; }
	}
	#endregion
}
