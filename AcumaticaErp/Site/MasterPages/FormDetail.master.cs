using System;
using PX.Web.UI;

public partial class MasterPages_FormDetail : PX.Web.UI.BaseMasterPage, IPXMasterPage
{
	protected void Page_Load(object sender, EventArgs e)
	{
		Response.AddHeader("cache-control", "no-store, private");
	}

	// We'll need this code in case we use ASP.NET standard localization
	protected void Page_Init(object sender, EventArgs e)
	{
		//if (PXDataSource.RedirectHelper.IsPopupPage(Page)) statusBar.Visible = false;
	}

	#region Public properties

	/// <summary>
	/// Gets or sets the screen title string.
	/// </summary>
	public string ScreenTitle
	{
		get { return this.usrCaption.ScreenTitle; }
		set { this.usrCaption.ScreenTitle = value; }
	}

	/// <summary>
	/// Gets or sets the screen ID text.
	/// </summary>
	public string ScreenID
	{
		get { return this.usrCaption.ScreenID; }
		set { this.usrCaption.ScreenID = value; }
	}

	/// <summary>
	/// Gets or sets the screen image url.
	/// </summary>
	public string ScreenImage
	{
		get { return this.usrCaption.ScreenImage; }
		set { this.usrCaption.ScreenImage = value; }
	}
	
	/// <summary>
	/// Gets or sets the customization availability.
	/// </summary>
	public bool CustomizationAvailable
	{
		get { return this.usrCaption.CustomizationAvailable; }
		set { this.usrCaption.CustomizationAvailable = value; }
	}

	/// <summary>
	/// Gets or sets the favorite maintenance availability
	/// </summary>
	public bool FavoriteAvailable
	{
		get { return this.usrCaption.FavoriteAvailable; }
		set { this.usrCaption.FavoriteAvailable = value; }
	}

	/// <summary>
	/// Gets or sets the files menu availability.
	/// </summary>
	public bool FilesMenuAvailable
	{
		get { return this.usrCaption.FilesMenuAvailable; }
		set { this.usrCaption.FilesMenuAvailable = value; }
	}

	/// <summary>
	/// Gets or sets branch visibility in title.
	/// </summary>
	public bool BranchAvailable
	{
		get { return true; }
		set { }
	}

	#endregion
}
