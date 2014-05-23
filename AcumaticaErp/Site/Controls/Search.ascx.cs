using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using PX.Common;
using PX.SM;
using PX.Web.Controls.Wiki;
using PX.Web.UI;
using System.Collections.Generic;
using PX.Data;
using Messages = PX.Data.Search.Messages;
using PX.Data.Search;

public partial class Controls_SearchNew : SearchPanel
{
	Image imgMessage;
	Label lblMessage, lblResults;
	PXLabel lblFilter;
	PXTextEdit txtSearch;
	PXTextEdit txtReplace;
	PXCheckBox chkSearchReplace;
	HtmlTable mainContentTable;
	HtmlTableRow filterrow;
	PXPager pager;
	PXDropDown SearchCaption;
	PXDropDown GlobalSearchCaption;
	PXDropDown SearchCaptionCategory;
	PXDropDown SearchCaptionProduct;
	PXDropDown OrderCaption;
	PXButton Go;
	SearchType searchType;
	GridPagerMode pagerMode = GridPagerMode.Numeric;
	bool isUpdateAllowed;
	int maxDescriptionColumns;

	#region Event handlers
	protected void Page_Init(object sender, EventArgs e)
	{
		string query = Request.QueryString["query"];
		IsWiki = Request.QueryString["isWiki"];
		WikiID = Request.QueryString["wikiid"];
		WikiNumber = Request.QueryString["wikinumber"];
		CategoryID = Request.QueryString["categoryID"];
		ProductID = Request.QueryString["productID"];
		OrderID = Request.QueryString["orderID"];
		Globalsearchcaption = Request.QueryString["globalsearchcaption"];
		imgMessage = PXFormView1.FindControl("imgMessage") as Image;
		lblMessage = PXFormView1.FindControl("lblMessage") as Label;
		lblResults = PXFormView1.FindControl("lblResults") as Label;
		lblFilter = PXFormView1.FindControl("lblFilter") as PXLabel;
		txtSearch = PXFormView1.FindControl("txtSearch") as PXTextEdit;
		chkSearchReplace = PXFormView1.FindControl("chkSearchReplace") as PXCheckBox;
		txtReplace = PXFormView1.FindControl("txtReplace") as PXTextEdit;
		SearchCaption = PXFormView1.FindControl("SearchCaption") as PXDropDown;
		GlobalSearchCaption = PXFormView1.FindControl("GlobalSearchCaption") as PXDropDown;
		SearchCaptionCategory = PXFormView1.FindControl("SearchCaptionCategory") as PXDropDown;
		SearchCaptionProduct = PXFormView1.FindControl("SearchCaptionProduct") as PXDropDown;
		OrderCaption = PXFormView1.FindControl("OrderCaption") as PXDropDown;
		Go = PXFormView1.FindControl("btnSearch") as PXButton;
		filterrow = PXFormView1.FindControl("filterrow") as HtmlTableRow;

		mainContentTable = CreateMainTable();
		pager = CreatePager(query);
		PXFormView1.TemplateContainer.Controls.Add(MainContentTable);
		SetEditBoxAttrributes();

		if (this.searchType == SearchType.Wiki)
		{
			this.txtSearch.ToolTip = PXMessages.LocalizeNoPrefix(Messages.ttipHelpSearch);
		}
		else if (this.searchType == SearchType.Files)
		{
			this.txtSearch.ToolTip = PXMessages.LocalizeNoPrefix(Messages.ttipFileSearch);
		}
		else if (this.searchType == SearchType.Entity)
		{
			this.txtSearch.ToolTip = PXMessages.LocalizeNoPrefix(Messages.ttipEntitySearch);
		}
		else if (this.searchType == SearchType.Notes)
		{
			this.txtSearch.ToolTip = PXMessages.LocalizeNoPrefix(Messages.ttipNoteSearch);
		}

		FormatSearchCaption();
		RegisterThisId();
		imgMessage.ImageUrl = ResolveUrl("~/App_Themes/Default/Images/Message/information.gif");

		if (IsWiki == "0")
		{
			SearchCaption.Visible = false;
			SearchCaptionCategory.Visible = false;
			SearchCaptionProduct.Visible = false;
			OrderCaption.Visible = false;
			lblFilter.Visible = false;
			filterrow.Visible = false;

		}

		if (query == null || string.IsNullOrEmpty(query.Trim()))
		{
			imgMessage.Visible = true;
			lblMessage.Visible = true;
			lblResults.Visible = false;
			lblMessage.Text = PXMessages.LocalizeNoPrefix(Messages.SpecifySearchRequest);
			return;
		}

		imgMessage.Visible = false;
		lblMessage.Visible = false;
		lblResults.Visible = true;
	}
	#endregion

	private void RegisterThisId()
	{
		this.Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "varid", "var thisId = '" + this.ClientID + "';", true);
	}

	private HtmlTable CreateMainTable()
	{
		HtmlTable table = new HtmlTable();
		table.CellSpacing = 0;
		table.CellPadding = 10;
		table.Width = "100%";
		return table;
	}

	private void SetEditBoxAttrributes()
	{
		if (!Page.IsPostBack)
			txtSearch.Text = Request.QueryString["query"];
	}

	private PXPager CreatePager(string query)
	{
		PXPager pager = new PXPager();
		pager.NavigateUrl = Request.RawUrl;
		pager.Width = Unit.Percentage(100);
		pager.Style[HtmlTextWriterStyle.TextAlign] = "center";
		pager.Style[HtmlTextWriterStyle.MarginBottom] = "5px";
		pager.Mode = this.pagerMode;
		if (Page.IsPostBack)
		{
			pager.NavigateUrl += "&adv=1"; // paging for full-text search
		}
		if (!string.IsNullOrEmpty(Request.QueryString[pager.ParameterName]))
		{
			try
			{
				pager.CurrentPage = Convert.ToInt32(Request.QueryString[pager.ParameterName]);
			}
			catch
			{
				pager.CurrentPage = 0;
			}
		}
		return pager;
	}

	private void FormatSearchCaption()
	{
		PXGraph graph = new PXGraph();
		this.CreateGlobalSearchCaption(graph, GlobalSearchCaption);
		this.CreateWikiMenu(graph, SearchCaption);
		this.CreateCategoryMenu(graph, SearchCaptionCategory);
		this.CreateProductMenu(graph, SearchCaptionProduct);
		this.CreateOrderMenu(graph, OrderCaption);
	}

	private void CreateGlobalSearchCaption(PXGraph graph, PXDropDown dd)
	{
		PXListItem liWikis = new PXListItem("Help");
		dd.Items.Add(liWikis);

		PXListItem liEntities = new PXListItem("Entities");
		dd.Items.Add(liEntities);

		PXListItem liFiles = new PXListItem("Files");
		dd.Items.Add(liFiles);

		PXListItem liNotes = new PXListItem("Notes");
		dd.Items.Add(liNotes);

		PXListItem liScreens = new PXListItem("Form Titles");
		dd.Items.Add(liScreens);

		Int32 select;
		Int32.TryParse(Globalsearchcaption, out select);
		dd.SelectedIndex = select;
			
		string path = PXUrl.SiteUrlWithPath();
		var url = "";
		switch (dd.SelectedIndex)
		{
			case 0:
				path = PXUrl.SiteUrlWithPath();
				path += path.EndsWith("/") ? "" : "/";
				url = string.Format("{0}Search/{1}?query={2}&adv=1",
					path, this.ResolveClientUrl("~/Search/Wiki.aspx"), txtSearch.Value);
				url = url + "&wikiid=" + WikiID + "&wikinumber=" + WikiNumber 
					+ "&categoryID=" + CategoryID + "&productID=" + ProductID + "&orderID=" + OrderID + "&isWiki=1" + "&globalsearchcaption=" + dd.SelectedIndex.ToString();
				break;

			case 1:
				path = PXUrl.SiteUrlWithPath();
				path += path.EndsWith("/") ? "" : "/";
				url = string.Format("{0}Search/{1}?query={2}&adv=1",
					path, this.ResolveClientUrl("~/Search/Entity.aspx"), txtSearch.Value);
				url = url + "&wikiid=" + WikiID + "&wikinumber=" + WikiNumber
					+ "&categoryID=" + CategoryID + "&productID=" + ProductID + "&orderID=" + OrderID + "&isWiki=0" + "&globalsearchcaption=" + dd.SelectedIndex.ToString();
				break;

			case 2:
				path = PXUrl.SiteUrlWithPath();
				path += path.EndsWith("/") ? "" : "/";
				url = string.Format("{0}Search/{1}?query={2}&adv=1",
					path, this.ResolveClientUrl("~/Search/File.aspx"), txtSearch.Value);
				url = url + "&wikiid=" + WikiID + "&wikinumber=" + WikiNumber
					+ "&categoryID=" + CategoryID + "&productID=" + ProductID + "&orderID=" + OrderID + "&isWiki=0" + "&globalsearchcaption=" + dd.SelectedIndex.ToString();
				break;

			case 3:
				path = PXUrl.SiteUrlWithPath();
				path += path.EndsWith("/") ? "" : "/";
				url = string.Format("{0}Search/{1}?query={2}&adv=1",
					path, this.ResolveClientUrl("~/Search/Note.aspx"), txtSearch.Value);
				url = url + "&wikiid=" + WikiID + "&wikinumber=" + WikiNumber +
					"&categoryID=" + CategoryID + "&productID=" + ProductID + "&orderID=" + OrderID + "&isWiki=0" + "&globalsearchcaption=" + dd.SelectedIndex.ToString();
				break;

			case 4:
				path = PXUrl.SiteUrlWithPath();
				path += path.EndsWith("/") ? "" : "/";
				url = string.Format("{0}Search/{1}?query={2}&adv=1",
					path, this.ResolveClientUrl("~/Search/FormsTitle.aspx"), txtSearch.Value);
				url = url + "&wikiid=" + WikiID + "&wikinumber=" + WikiNumber +
					"&categoryID=" + CategoryID + "&productID=" + ProductID + "&orderID=" + OrderID + "&isWiki=0" + "&globalsearchcaption=" + dd.SelectedIndex.ToString();
				break;
		}
	}

	protected void GlobalSearchCaption_Change(object sender, PXCallBackEventArgs e)
	{
		if (!string.IsNullOrEmpty(Request.RawUrl))
		{
			string path = PXUrl.SiteUrlWithPath();
			var url = "";
			switch (GlobalSearchCaption.SelectedIndex)
			{
				case 0:
					path = PXUrl.SiteUrlWithPath();
					path += path.EndsWith("/") ? "" : "/";
					url = string.Format("{0}Search/{1}?query={2}&adv=1",
						path, this.ResolveClientUrl("~/Search/Wiki.aspx"), txtSearch.Value);
					url = url + "&wikiid=" + WikiID + "&wikinumber=" + WikiNumber
						+ "&categoryID=" + CategoryID + "&productID=" + ProductID + "&orderID=" + OrderID + "&isWiki=1" + "&globalsearchcaption=" + GlobalSearchCaption.SelectedIndex.ToString();
					throw new Exception("Redirect0:" + url);

				case 1:
					path = PXUrl.SiteUrlWithPath();
					path += path.EndsWith("/") ? "" : "/";
					url = string.Format("{0}Search/{1}?query={2}&adv=1",
						path, this.ResolveClientUrl("~/Search/Entity.aspx"), txtSearch.Value);
					url = url + "&wikiid=" + WikiID + "&wikinumber=" + WikiNumber
						+ "&categoryID=" + CategoryID + "&productID=" + ProductID + "&orderID=" + OrderID + "&isWiki=0" + "&globalsearchcaption=" + GlobalSearchCaption.SelectedIndex.ToString();
					throw new Exception("Redirect0:" + url);

				case 2:
					path = PXUrl.SiteUrlWithPath();
					path += path.EndsWith("/") ? "" : "/";
					url = string.Format("{0}Search/{1}?query={2}&adv=1",
						path, this.ResolveClientUrl("~/Search/File.aspx"), txtSearch.Value);
					url = url + "&wikiid=" + WikiID + "&wikinumber=" + WikiNumber
						+ "&categoryID=" + CategoryID + "&productID=" + ProductID + "&orderID=" + OrderID + "&isWiki=0" + "&globalsearchcaption=" + GlobalSearchCaption.SelectedIndex.ToString();
					throw new Exception("Redirect0:" + url);

				case 3:
					path = PXUrl.SiteUrlWithPath();
					path += path.EndsWith("/") ? "" : "/";
					url = string.Format("{0}Search/{1}?query={2}&adv=1",
						path, this.ResolveClientUrl("~/Search/Note.aspx"), txtSearch.Value);
					url = url + "&wikiid=" + WikiID + "&wikinumber=" + WikiNumber +
						"&categoryID=" + CategoryID + "&productID=" + ProductID + "&orderID=" + OrderID + "&isWiki=0" + "&globalsearchcaption=" + GlobalSearchCaption.SelectedIndex.ToString();
					throw new Exception("Redirect0:" + url);

				case 4:
					path = PXUrl.SiteUrlWithPath();
					path += path.EndsWith("/") ? "" : "/";
					url = string.Format("{0}Search/{1}?query={2}&adv=1",
						path, this.ResolveClientUrl("~/Search/FormsTitle.aspx"), txtSearch.Value);
					url = url + "&wikiid=" + WikiID + "&wikinumber=" + WikiNumber +
						"&categoryID=" + CategoryID + "&productID=" + ProductID + "&orderID=" + OrderID + "&isWiki=0" + "&globalsearchcaption=" + GlobalSearchCaption.SelectedIndex.ToString();
					throw new Exception("Redirect0:" + url);
			}
		}
	}


	private void CreateWikiMenu(PXGraph graph, PXDropDown dd)
	{
		PXListItem liall = new PXListItem("Entire Help");
		dd.Items.Add(liall);

		foreach (PXResult result in PXSelect<WikiDescriptor>.Select(graph))
		{
			WikiDescriptor wiki = result[typeof(WikiDescriptor)] as WikiDescriptor;
			if (wiki != null && wiki.PageID != null)
			{
				var node = PXSiteMap.Provider.FindSiteMapNodeFromKey((Guid)wiki.PageID);
				if (node != null)
				{
					string title = wiki.WikiTitle ?? node.Title;
					PXListItem li = new PXListItem(title, wiki.PageID.ToString());
					dd.Items.Add(li);
				}
			}
		}
		for (int i = 0; i < dd.Items.Count; i++)
		{
			if (WikiID == dd.Items[i].Value)
			{
				dd.SelectedIndex = i;
			}
		}
		string path = PXUrl.SiteUrlWithPath();
		var url = "";
		path = PXUrl.SiteUrlWithPath();
		path += path.EndsWith("/") ? "" : "/";
		url = string.Format("{0}Search/{1}?query={2}&adv=1",
			path, this.ResolveClientUrl("~/Search/Wiki.aspx"), txtSearch.Value);
		url = url + "&wikiid=" + SearchCaption.Value + "&wikinumber=" + SearchCaption.SelectedIndex.ToString() + 
			"&categoryID=" + CategoryID + "&productID=" + ProductID + "&orderID=" + OrderID + "&isWiki=1" + "&globalsearchcaption=" + Globalsearchcaption;
	}

	protected void SearchCaption_WikiChange(object sender, PXCallBackEventArgs e)
	{
		if (!string.IsNullOrEmpty(Request.RawUrl))
		{
			string path = PXUrl.SiteUrlWithPath();
			var url = "";
			path = PXUrl.SiteUrlWithPath();
			path += path.EndsWith("/") ? "" : "/";
			url = string.Format("{0}Search/{1}?query={2}&adv=1",
			                    path, this.ResolveClientUrl("~/Search/Wiki.aspx"), txtSearch.Value);
			url = url + "&wikiid=" + SearchCaption.Value + "&wikinumber=" + SearchCaption.SelectedIndex.ToString() +
				  "&categoryID=" + CategoryID + "&productID=" + ProductID + "&orderID=" + OrderID + "&isWiki=1" + "&globalsearchcaption=" + Globalsearchcaption;
			throw new Exception("Redirect0:" + url);
		}
	}

	private void CreateCategoryMenu(PXGraph graph, PXDropDown dd)
	{
		PXListItem liall = new PXListItem(PXMessages.LocalizeNoPrefix(PX.SM.Messages.SearchCategory));
		dd.Items.Add(liall);
		foreach (PXResult result in PXSelect<SPWikiCategory>.Select(graph))
		{
			SPWikiCategory wc = result[typeof(SPWikiCategory)] as SPWikiCategory;
			PXListItem li = new PXListItem(wc.Description, wc.CategoryID);
			dd.Items.Add(li);
		}

		for (int i = 0; i < dd.Items.Count; i++)
		{
			if (CategoryID == dd.Items[i].Value)
			{
				dd.SelectedIndex = i;
			}
		}

		string path = PXUrl.SiteUrlWithPath();
		path += path.EndsWith("/") ? "" : "/";
		var url = string.Format("{0}Search/{1}?query={2}&adv=1",
			path, this.ResolveClientUrl("~/Search/Wiki.aspx"), txtSearch.Value);
		url = url + "&wikiid=" + WikiID + "&wikinumber=" + WikiNumber +
		      "&categoryID=" + SearchCaptionCategory.Value + "&productID=" + ProductID + "&orderID=" +
			  OrderID + "&isWiki=" + IsWiki + "&globalsearchcaption=" + Globalsearchcaption;
	}

	protected void SearchCaption_CategoryChange(object sender, PXCallBackEventArgs e)
	{
		if (!string.IsNullOrEmpty(Request.RawUrl))
		{
			string path = PXUrl.SiteUrlWithPath();
			path += path.EndsWith("/") ? "" : "/";
			var url = string.Format("{0}Search/{1}?query={2}&adv=1",
				path, this.ResolveClientUrl("~/Search/Wiki.aspx"), txtSearch.Value);
			url = url + "&wikiid=" + WikiID + "&wikinumber=" + WikiNumber +
				"&categoryID=" + SearchCaptionCategory.Value + "&productID=" + ProductID + "&orderID=" + 
				OrderID + "&isWiki=" + IsWiki + "&globalsearchcaption=" + Globalsearchcaption;
			throw new Exception("Redirect0:" + url);
		}
	}

	private void CreateProductMenu(PXGraph graph, PXDropDown dd)
	{
		PXListItem liall = new PXListItem(PXMessages.LocalizeNoPrefix(PX.SM.Messages.SearchProduct));
		dd.Items.Add(liall);
		foreach (PXResult result in PXSelect<SPWikiProduct>.Select(graph))
		{
			SPWikiProduct wc = result[typeof(SPWikiProduct)] as SPWikiProduct;
			PXListItem li = new PXListItem(wc.Description, wc.ProductID);
			dd.Items.Add(li);
		}

		for (int i = 0; i < dd.Items.Count; i++)
		{
			if (ProductID == dd.Items[i].Value)
			{
				dd.SelectedIndex = i;
			}
		}

		string path = PXUrl.SiteUrlWithPath();
		path += path.EndsWith("/") ? "" : "/";
		var url = string.Format("{0}Search/{1}?query={2}&adv=1",
			path, this.ResolveClientUrl("~/Search/Wiki.aspx"), txtSearch.Value);
		url = url + "&wikiid=" + WikiID + "&wikinumber=" + WikiNumber + "&categoryID=" + 
			CategoryID + "&productID=" + SearchCaptionProduct.Value + "&orderID=" + 
			OrderID + "&isWiki=" + IsWiki + "&globalsearchcaption=" + Globalsearchcaption;
	}

	protected void SearchCaption_ProductChange(object sender, PXCallBackEventArgs e)
	{
		if (!string.IsNullOrEmpty(Request.RawUrl))
		{
			string path = PXUrl.SiteUrlWithPath();
			path += path.EndsWith("/") ? "" : "/";
			var url = string.Format("{0}Search/{1}?query={2}&adv=1",
				path, this.ResolveClientUrl("~/Search/WikiSP.aspx"), txtSearch.Value);
			url = url + "&wikiid=" + WikiID + "&wikinumber=" + WikiNumber + 
				"&categoryID=" + CategoryID + "&productID=" + SearchCaptionProduct.Value + "&orderID=" + 
				OrderID + "&isWiki=" + IsWiki + "&globalsearchcaption=" + Globalsearchcaption;
			throw new Exception("Redirect0:" + url);
		}
	}

	private void CreateOrderMenu(PXGraph graph, PXDropDown dd)
	{
		//public const string SearchCategory = "All category";
		PXListItem li1 = new PXListItem("Order by Most Recent", "0");
		dd.Items.Add(li1);
		PXListItem li2 = new PXListItem("Order by Views", "1");
		dd.Items.Add(li2);
		PXListItem li3 = new PXListItem("Order by Rating", "2");
		dd.Items.Add(li3);

		for (int i = 0; i < dd.Items.Count; i++)
		{
			if (OrderID == dd.Items[i].Value)
			{
				dd.SelectedIndex = i;
			}
		}

		string path = PXUrl.SiteUrlWithPath();
		path += path.EndsWith("/") ? "" : "/";
		var url = string.Format("{0}Search/{1}?query={2}&adv=1",
			path, this.ResolveClientUrl("~/Search/Wiki.aspx"), txtSearch.Value);
		url = url + "&wikiid=" + WikiID + "&wikinumber=" + WikiNumber + "&categoryID=" + CategoryID + "&productID=" + 
			ProductID + "&orderID=" + OrderCaption.Value + "&isWiki=" + IsWiki + "&globalsearchcaption=" + Globalsearchcaption;
	}

	protected void OrderCaption_OrderChange(object sender, PXCallBackEventArgs e)
	{
		if (!string.IsNullOrEmpty(Request.RawUrl))
		{
			string path = PXUrl.SiteUrlWithPath();
			path += path.EndsWith("/") ? "" : "/";
			var url = string.Format("{0}Search/{1}?query={2}&adv=1",
				path, this.ResolveClientUrl("~/Search/Wiki.aspx"), txtSearch.Value);
			url = url + "&wikiid=" + WikiID + "&wikinumber=" + WikiNumber + "&categoryID=" + CategoryID + "&productID=" +
				ProductID + "&orderID=" + OrderCaption.Value + "&isWiki=" + IsWiki + "&globalsearchcaption=" + Globalsearchcaption;
			throw new Exception("Redirect0:" + url);
		}
	}

	#region Protected Methods
	protected override HtmlTable MainContentTable
	{
		get
		{
			return this.mainContentTable;
		}
	}

	protected override Image ImgMessage
	{
		get
		{
			return this.imgMessage;
		}
	}

	protected override Label LblMessage
	{
		get
		{
			return this.lblMessage;
		}
	}

	protected override Label LblResults
	{
		get
		{
			return this.lblResults;
		}
	}

	#endregion

	#region Public properties
	public override int CurrentPage
	{
		get
		{
			return pager.CurrentPage;
		}
		set
		{
			pager.CurrentPage = value;
		}
	}

	public override GridPagerMode PagerMode
	{
		get { return this.pagerMode; }
		set { this.pagerMode = value; }
	}

	public override string Text
	{
		get { return txtSearch.Text; }
		set { txtSearch.Text = value; }
	}

	public override int PagesCount
	{
		get
		{
			return this.pager.PagesNumber;
		}
		set
		{
			this.pager.PagesNumber = value;
			if (value > 1 && !this.PXFormView1.TemplateContainer.Controls.Contains(this.pager))
			{
				this.PXFormView1.TemplateContainer.Controls.Add(this.pager);
				this.PXFormView1.TemplateContainer.Controls.Add(new LiteralControl("<br />"));
			}
		}
	}

	public override bool PagerHasNext
	{
		get
		{
			return this.pager.HasNext;
		}
		set
		{
			this.pager.HasNext = value;
		}
	}

	public override bool PagerHasPrev
	{
		get
		{
			return this.pager.HasPrev;
		}
		set
		{
			this.pager.HasPrev = value;
		}
	}

	public override bool IsUpdateAllowed
	{
		get
		{
			return this.isUpdateAllowed;
		}
		set
		{
			this.isUpdateAllowed = value;
		}
	}

	public override SearchType TypeOfSearch
	{
		get
		{
			return this.searchType;
		}
		set
		{
			this.searchType = value;
		}
	}

	public override int MaxDescriptionColumns
	{
		get { return maxDescriptionColumns; }
		set { maxDescriptionColumns = value; }
	}
	#endregion

	public override bool FullText
	{
		get
		{
			return true;
		}
		set
		{
			value = true;
		}
	}
}