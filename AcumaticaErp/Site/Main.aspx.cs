using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using PX.Common;
using PX.Data;
using PX.SM;
using PX.Web.Controls;
using PX.Web.UI;
using PX.Export.Authentication;

public partial class _Main : PX.Web.UI.PXPage, ITitleModuleController
{
	#region Event handlers

	//---------------------------------------------------------------------------
	/// <summary>
	/// The page Init event handler.
	/// </summary>
	protected override void OnInit(EventArgs e)
	{
		this.InitBusinessDate();
		this.InitUserName();
		this.InitializeTaskPanel();

		if (!this.IsCallback)
		{
			var renderer = JSManager.GetRenderer(this);
			var scriptFiles = new List<string>();
			PXContext.Session.PXSharedScriptFiles[JSManager.SharedScriptFilesKey] = scriptFiles;
			if (JSManager.GetBatchMode())
			{
				scriptFiles.AddRange(new string[] { 
					JS.BatchBase, JS.BatchEdit, JS.BatchGrid, AppJS.AppScriptBatch, AppJS.DashboardsBatch });
				JSManager.RegisterScriptBatch(renderer, JS.BaseKey, JS.BatchBase);
				JSManager.RegisterScriptBatch(renderer, JS.BaseKey, JS.BatchEdit);
				JSManager.RegisterScriptBatch(renderer, JS.BaseKey, JS.BatchGrid);
				JSManager.RegisterScriptBatch(renderer, AppJS.BaseKey, AppJS.AppScriptBatch);
				JSManager.RegisterScriptBatch(renderer, AppJS.BaseKey, AppJS.DashboardsBatch);
			}
			else
			{
				var list = new List<KeyValuePair<Type, string[]>>();
				list.Add(new KeyValuePair<Type, string[]>(JS.BaseKey, JS.BatchBaseFiles));
				list.Add(new KeyValuePair<Type, string[]>(JS.BaseKey, JS.BatchEditFiles));
				list.Add(new KeyValuePair<Type, string[]>(JS.BaseKey, JS.BatchGridFiles));
				list.Add(new KeyValuePair<Type, string[]>(AppJS.BaseKey, AppJS.BatchAppFiles));
				list.Add(new KeyValuePair<Type, string[]>(AppJS.BaseKey, AppJS.BatchDasboardFiles));

				foreach (KeyValuePair<Type, string[]> pair in list)
				{
					foreach (string script in pair.Value)
					{
						JSManager.RegisterModule(renderer, pair.Key, script);
						scriptFiles.Add(script);
					}
				}
			}
			this.SetCompanyLogo();
			JSManager.RegisterModule(renderer, typeof(AppJS), AppJS.MainFrame);

			if (!this.IsPostBack && this.Request.QueryString["ScreenID"] != null) PXContext.Session.SetString("LastUrl", null);
		}
		base.OnInit(e);
	}

	/// <summary>
	/// The page InitComplete event handler.
	/// </summary>
	protected override void OnInitComplete(EventArgs e)
	{
		this.InitTasksAndEvents();
		base.OnInitComplete(e);
	}

	//---------------------------------------------------------------------------
	/// <summary>
	/// The page load event handler.
	/// </summary>
	protected void Page_Load(object sender, EventArgs e)
	{
		Response.AddHeader("cache-control", "no-store, private");

		if (this.IsCallback)
		{
			this.EnableViewStateMac = false;
			this.activeSystem = this.Request.Form[_activeSystemKey];
			this.activeModule = this.Request.Form[_activeModuleKey];
			this.activeSubMod = this.Request.Form[_activeSubModKey];

			// load the active panel data
			this.activePanelIndex = this.Request.Form[_activePanelKey];
			string[] ar = this.activePanelIndex.Split('|');
			if (ar.Length == 2)
			{
				this.activePanelIndex = ar[0]; this.activePanelKey = ar[1];
			}
		}
		else
		{
			string suffix = PXSessionStateStore.GetSuffix(this.Context);
			int pos = suffix.IndexOf("W("), pos2 = suffix.IndexOf(")");
			if(pos >= 0) suffix = int.Parse(suffix.Substring(2, pos2 - 2)).ToString();
			HttpCookie cookie = Request.Cookies["favoritesActive" + suffix];
			if (cookie != null && !string.IsNullOrEmpty(cookie.Value)) this.favoritesActive = true;
		}
		
		if (PXSiteMap.IsPortal) this.moduleLink.Visible = false;

		this.InitSearchBox();
		if (!this.IsCallback || this.IsReloadTopPanel)
		{
			this.InitTopMenu();
			this.InitModulesMenu();
		}
		if (!this.IsCallback || this.IsReloadMenuPanel)
		{
			this.activePanelIndex = "0";

			string hideSubModules = System.Configuration.ConfigurationManager.AppSettings["HideSubModuleBar"];
			if (string.IsNullOrEmpty(hideSubModules) || hideSubModules.ToLower() != "true") 
				this.InitSubModulesMenu();
			else subModulesBar.Visible = false;
		}
		this.CreateActiveNavigationPanel();
		
		if (ControlHelper.IsRtlCulture())
			this.Page.Form.Style[HtmlTextWriterStyle.Direction] = "rtl";
	}

	/// <summary>
	/// The page PreRenderComplete event handler.
	/// </summary>
	protected override void OnPreRenderComplete(EventArgs e)
	{
		this.ClientScript.RegisterStartupScript(this.GetType(), "load", "\npx.loadFrameset();", true);
		this.ClientScript.RegisterStartupScript(this.GetType(), "hideScript", "\nvar hideScript = 1; ", true);
		this.ClientScript.RegisterStartupScript(this.GetType(),
			"time", string.Format("var timeShift ={0}; ", LocaleInfo.GetTimeZone().UtcOffset.TotalMilliseconds), true);
		if (PXSiteMap.IsPortal)
			this.ClientScript.RegisterStartupScript(this.GetType(), "portal", "var isPortal = 1;", true);
		
		var ms = WebConfigurationManager.GetSection(FormsAuthenticationSection._SECTION_PATH) as FormsAuthenticationSection;
		this.ClientScript.RegisterStartupScript(
			this.GetType(), "loginUrl", string.Format("var loginUrl = '{0}'; ", ms.LoginUrl), true);
		this.ClientScript.RegisterStartupScript(
			this.GetType(), "url", string.Format("\nvar lastUrl ='{0}';", this.GetLastUrl()), true);

		if (this.IsContextNavigation())
			this.ClientScript.RegisterStartupScript(this.GetType(), "contextNav", "var contextNav = 1;", true);
		
		this.ClientScript.RegisterHiddenField(_activeSystemKey, this.activeSystem);
		this.ClientScript.RegisterHiddenField(_activeModuleKey, this.activeModule);
		this.ClientScript.RegisterHiddenField(_activeSubModKey, this.activeSubMod);
		this.ClientScript.RegisterHiddenField(_activePanelKey, this.activePanelIndex);
		base.OnPreRenderComplete(e);
	}

	/// <summary>
	/// Page unload event handler.
	/// </summary>
	protected override void OnUnload(EventArgs e)
	{
		PXSiteMap.Provider.SetCurrentNode(null);
		base.OnUnload(e);
	}

	//---------------------------------------------------------------------------
	/// <summary>
	/// The set business callback event handler.
	/// </summary>
	protected void onSetDate_CallBack(object sender, PXCallBackEventArgs e)
	{
		object date = ((PXDateTimeEdit)pnlDate.FindControl("edEffDate")).Value;
		if (date != null)
		{
			PXContext.SetBusinessDate((DateTime)date);
			PXDateTimeEdit.SetDefaultDate((DateTime)date);
		}
	}

	/// <summary>
	/// The set business callback event handler.
	/// </summary>
	protected void toolsBar_OnCallBack(object sender, PXCallBackEventArgs e)
	{
		if (e.Command.Name == "EventsCount")
		{
			PXTasksAndEventsNavPanel pnl = this.pnlTasksAndEvents;
			if (pnl != null)
			{
				e.Result = string.Format("{0}({1})|{2}|{3}", pnl.TodayTasksCount + pnl.TodayEventsCount,
					pnl.NewTasksCount + pnl.NewEventsCount, pnl.TasksLabelText, pnl.EventsLabelText);
			}
		}
	}

	/// <summary>
	/// The user menu items event handler.
	/// </summary>
	void UserNameMenu_ItemClick(object sender, PXMenuItemEventArgs e)
	{
		if (e.Item.Value == "LogOut")
		{
			PX.Data.PXLogin.LogoutUser(PXAccess.GetUserName(), Session.SessionID);
			PXContext.Session.SetString("UserLogin", string.Empty); // this session variable is used in global.asax

			string redirectUrl = PX.Export.Authentication.FormsAuthenticationModule.SignOut();
			if (String.IsNullOrEmpty(redirectUrl)) redirectUrl = ResolveUrl("~/Main.aspx");
			Session.Abandon();
			Response.Write("<script>window.open(\"" + redirectUrl + "\",\"_top\");</script>");
		}
		else if (e.Item.Value != null && e.Item.Value.StartsWith("Company"))
		{
			string navigateUrl, companyId = e.Item.Text;
			if (!PXLogin.SwitchCompany(companyId))
			{
				navigateUrl = string.Format("{0}?ReturnUrl={1}", 
					PX.Export.Authentication.FormsAuthenticationModule.LoginUrl,
					HttpUtility.UrlEncode(ResolveUrl("~/Main.aspx")));
				
				HttpCookie cookie = Response.Cookies["CompanyID"];
				if (cookie != null) cookie.Value = companyId;
				else Response.Cookies.Add(new HttpCookie("CompanyID", companyId));
			}
			else navigateUrl = ResolveUrl("~/Main.aspx");
			
			Response.Write("<script>window.open(\"" + navigateUrl + "\",\"_top\");</script>");
		}
	}
	#endregion

	#region Methods to initialize naviagtion bars

	/// <summary>
	/// Check if current callback perform the navigation panel reload.
	/// </summary>
	private bool IsReloadTopPanel
	{
		get
		{
			if (this.IsCallback) return this.Request.Params["__CALLBACKID"] == "panelT";
			return false;
		}
	}

	/// <summary>
	/// Check if current callback perform the navigation panel reload.
	/// </summary>
	private bool IsReloadMenuPanel
	{
		get
		{
			if (this.IsCallback) return this.Request.Params["__CALLBACKID"].EndsWith("menuPanel");
			return false;
		}
	}

	static private bool IsValidNode(SiteMapNode node)
	{
		PXSiteMapNode pxNode = node as PXSiteMapNode;
		return pxNode == null || pxNode.ScreenID != "HD000000";
	}

	/// <summary>
	/// Get the active sitemap node at specified level.
	/// </summary>
	private SiteMapNode GetActiveNode(int level)
	{
		string url = GetPureLastUrl(this.GetLastUrl());
		SiteMapNode an = string.IsNullOrEmpty(url) ? null : PXSiteMap.Provider.FindSiteMapNode(url);
		if (level < 0) return an;

		List<SiteMapNode> nodes = new List<SiteMapNode>();
		while (an != null && an != an.RootNode) { nodes.Insert(0, an); an = an.ParentNode; }
		return (level < nodes.Count) ? nodes[level] : null;
	}

	/// <summary>
	/// Check if current node in Favorites.
	/// </summary>
	private bool IsInFavorites(string nodeID)
	{
		using (PXDataRecord exist = PXDatabase.SelectSingle<Favorite>(
			new PXDataField("SiteMapID"), new PXDataFieldValue("SiteMapID", nodeID),
			new PXDataFieldValue("UserID", PXAccess.GetUserID())))
		{
			return exist != null;
		}
	}

	//---------------------------------------------------------------------------
	/// <summary>
	/// Create toolbar button with specified parameters.
	/// </summary>
	private static PXToolBarButton CreateToolsButton(
		PXToolBar tlb, string text, string key, string imageSet, string imageKey)
	{
		var btn = new PXToolBarButton() { Text = text, Key = key };
		btn.ImageSet = imageSet; btn.ImageKey = imageKey;
		if (tlb != null) tlb.Items.Add(btn);
		return btn;
	}

	/// <summary>
	/// Create toolbar button with specified parameters.
	/// </summary>
	private static PXToolBarButton CreateToolsButton(
		PXToolBar tlb, string tooltip, string key, string imageUrl)
	{
		var btn = new PXToolBarButton() { Tooltip = tooltip, Key = key };
		btn.Images.Normal = imageUrl;
		if (tlb != null) tlb.Items.Add(btn);
		return btn;
	}

	/// <summary>
	/// Create menu item with specified parameters.
	/// </summary>
	private static PXMenuItem CreateMenuItem(
		PXToolBarButton btn, string text, string key, string imageSet, string imageKey)
	{
		var item = new PXMenuItem() { Text = text, Value = key };
		item.ImageSet = imageSet; item.ImageKey = imageKey;
		if (btn != null) btn.MenuItems.Add(item);
		return item;
	}
	/// <summary>
	/// Create menu item with specified parameters.
	/// </summary>
	private static PXMenuItem CreateMenuItem(PXToolBarButton btn, string text, string key)
	{
		return CreateMenuItem(btn, text, key, string.Empty, string.Empty);
	}

	//---------------------------------------------------------------------------
	/// <summary>
	/// Initialize the top menu of the page.
	/// </summary>
	private void InitTopMenu()
	{
		SiteMapNode an = null;
		if (!this.Page.IsCallback || this.IsReloadTopPanel) an = GetActiveNode(0);

		var nodes = this.GetTopMenuDataSource();
		PXToolBarButton activeButton = null;
		for (int i = 0; i < nodes.Count; i++)
		{
			SiteMapNode node = nodes[i];
			string descr = node.Description, image = null;
			bool active = false;
			if (!string.IsNullOrEmpty(descr))
			{
				string[] im = descr.Split('|');
				image = PXImages.ResolveImageUrl(im[0], this);
			}

			// sets the active module keys
			if (!this.Page.IsCallback && ((an == null && i == 0) || (an != null && an.Key == node.Key)))
			{
				this.activeSystem = node.Key; active = true; this.activeNode = node; 
			}
			
			var btn = CreateToolsButton(systemsBar, node.Title, node.Key, null, null);
			//btn.Images.Normal = image;
			btn.NavigateUrl = node.Url;
			btn.ToggleGroup = "1";
			btn.ToggleMode = true;
			btn.Pushed = active;
			if (active) activeButton = btn;
		}
		
		if (string.IsNullOrEmpty(this.activeSystem) && nodes.Count > 0)
		{
			this.activeSystem = nodes[0].Key;
			activeButton = ((PXToolBarButton)systemsBar.Items[0]);
			activeButton.Pushed = true;
		}
		if (activeButton != null)
		{
			this.moduleLink.InnerText = activeButton.Text;
			this.moduleLink.HRef = this.ResolveUrl(activeButton.NavigateUrl);
			//activeButton.Attributes["target"] = "main";
			//activeButton.Attributes["href"] = this.moduleLink.HRef;
		}
	}

	/// <summary>
	/// Gets the root nodes data source.
	/// </summary>
	private List<SiteMapNode> GetTopMenuDataSource()
	{
		List<SiteMapNode> source = new List<SiteMapNode>();

		foreach (SiteMapNode node in System.Web.SiteMap.RootNode.ChildNodes)
			if (IsValidNode(node)) source.Add(node);
		return source;
	}

	//---------------------------------------------------------------------------
	/// <summary>
	/// Initialize the modules toolbar.
	/// </summary>
	private void InitModulesMenu()
	{
		SiteMapNode an = null, firstNode = null;
		bool reloadTopPanel = this.IsReloadTopPanel;
		if (!this.Page.IsCallback || reloadTopPanel)
		{
			an = GetActiveNode(1);
			if (an != null && this.favoritesActive && IsInFavorites(GetActiveNode(-1).Key)) an = null;
		}

		PXToolBarButton firstButton = null, activeButton = null, fav = null;
		// create Favorites button
		if (!PXSiteMap.IsPortal)
		{
			fav = CreateToolsButton(modulesBar, null, _favorites, null, null);
			fav.NavigateUrl = _favoritesUrl;
			fav.Images.Normal = Sprite.Main.GetFullUrl(Sprite.Main.StarGray);
			fav.PushedImages.Normal = Sprite.Main.GetFullUrl(Sprite.Main.StarWhite);
			fav.ToggleMode = true;
			fav.Tooltip = "Favorites";
			if (!this.Page.IsCallback || reloadTopPanel)
			{
				Boolean exist = PXSiteMap.FavoritesProvider.FavoritesExists();
				if (exist && this.activeModule == _favorites && reloadTopPanel)
					an = null;
				if (exist && an == null)
				{
					this.activeModule = _favorites;
					activeButton = fav;
					fav.Pushed = true;
				}
				fav.Visible = exist;
				modulesBar.Items.Add(new PXToolBarSeperator() {Visible = fav.Visible});
			}
		}

		var nodes = this.GetModulesDataSource();
		string parentKey = null;
		PXToolBarButton btn = null, visibleBtn = null;
		int groupIndex = 0, visibleCount = 0;
		for (int i = 0; i < nodes.Count; i++)
		{
			SiteMapNode node = nodes[i];
			string descr = node.Description, image = null;
			if (!string.IsNullOrEmpty(descr))
			{
				string[] im = descr.Split('|');
				image = PXImages.ResolveImageUrl(im[0], this);
			}

			if (parentKey != node.ParentNode.Key) { parentKey = node.ParentNode.Key; groupIndex++; }
			bool visible = true, active = false;
			if (!string.IsNullOrEmpty(this.activeSystem) && parentKey != this.activeSystem)
				visible = false;
			else
				visibleCount++;

			if ((an != null && an.Key == node.Key) || 
				(an == null && this.activeSystem == parentKey && string.IsNullOrEmpty(this.activeModule)))
			{
				this.activeModule = node.Key; active = true; this.activeNode = node; 
			}

			btn = CreateToolsButton(modulesBar, node.Title, parentKey + "|" + node.Key, null, null);
			btn.Visible = visible;
			btn.NavigateUrl = node.Url;
			btn.ToggleGroup = groupIndex.ToString();
			btn.ToggleMode = true;
			btn.Pushed = active;
			if (visible) visibleBtn = btn;
			
			if (active) activeButton = btn;
			if (visible && firstNode == null) 
			{ 
				firstNode = node; firstButton = btn;
				if (fav != null) fav.ToggleGroup = btn.ToggleGroup;
			}
			modulesBar.Items.Add(new PXToolBarSeperator() { Visible = visible });
		}
		
		if (visibleCount == 1)
		{
			int index = modulesBar.Items.IndexOf(visibleBtn);
			visibleBtn.Visible = false;
			if (index < (modulesBar.Items.Count - 1)) modulesBar.Items[index + 1].Visible = false;
			visibleCount--;
		}
		
		// hide favorites if modules bar has no items
		if (visibleCount == 0)
		{
			this.activeModule = string.Empty;	activeButton = null;
			if (fav != null && fav.Visible)
			{
				modulesBar.Items[fav.Index + 1].Visible = fav.Visible = false;
				fav.Attributes["data-hidden"] = "1";
			}
			modulesBar.CssClass += " hidden"; 
			panelT.CssClass += " modulesHidden";
		}

		if (string.IsNullOrEmpty(this.activeModule) && firstNode != null)
		{
			this.activeModule = firstNode.Key;
			firstButton.Pushed = true; activeButton = firstButton;
		}
		if (activeButton != null)
		{
			this.moduleLink.InnerText = 
				string.IsNullOrEmpty(activeButton.Text) ? activeButton.Tooltip : activeButton.Text;
			this.moduleLink.HRef = this.ResolveUrl(activeButton.NavigateUrl);
		}
	}

	/// <summary>
	/// Gets the modules data source.
	/// </summary>
	private List<SiteMapNode> GetModulesDataSource()
	{
		List<SiteMapNode> source = new List<SiteMapNode>();
		foreach (SiteMapNode node in System.Web.SiteMap.RootNode.ChildNodes)
		{
			if (!IsValidNode(node)) continue; 
			foreach (SiteMapNode node2 in node.ChildNodes) source.Add(node2);
		}
		return source;
	}

	//---------------------------------------------------------------------------
	/// <summary>
	/// Initialize the Sub-module menu control.
	/// </summary>
	private void InitSubModulesMenu()
	{
		bool reloadMenuPanel = this.IsReloadMenuPanel;
		SiteMapNode an = null;
		if (!this.Page.IsCallback || reloadMenuPanel)
		{
			an = GetActiveNode(2);
			if (an != null && this.favoritesActive && IsInFavorites(GetActiveNode(-1).Key)) an = null;
		}

		List<SiteMapNode> nodes = this.GetSubModulesDataSource();
		PXToolBarButton btn = null, visibleBtn = null;
		string parentKey = null;
		int groupIndex = 0, visibleCount = 0;
		PXWikiProvider wp = PXSiteMap.WikiProvider;
		string module = string.IsNullOrEmpty(this.activeModule) ? this.activeSystem : this.activeModule;

		for (int i = 0; i < nodes.Count; i++)
		{
			SiteMapNode node = nodes[i];
			if (PXSiteMap.Provider.GetChildNodesSimple(node).Count == 0)
			{
				PXSiteMapNode pxNode = node as PXSiteMapNode;
				if (pxNode == null || wp.FindSiteMapNodeFromKey(pxNode.NodeID) == null)
					continue;
			}

			string descr = node.Description, url = null;
			if (!string.IsNullOrEmpty(descr))
			{
				string[] im = descr.Split('|');
				url = PXImages.ResolveImageUrl(im[0], this);
			}

			bool startGroup = (parentKey != node.ParentNode.Key);
			if (startGroup) { parentKey = node.ParentNode.Key; groupIndex++; }
			bool visible = true, active = false;
			if (!string.IsNullOrEmpty(module) && parentKey != module)
				visible = false;
			else
				visibleCount++;

			if ((an != null && an.Key == node.Key) ||
				(an == null && string.IsNullOrEmpty(this.activeSubMod) && (module == parentKey || module == node.Key)))
			{
				this.activeSubMod = node.Key; active = true; this.activeNode = node; 
			}
			if (startGroup && btn != null) btn.Attributes.Add("endGroup", "1");

			btn = CreateToolsButton(subModulesBar, node.Title, parentKey + "|" + node.Key, url);
			btn.Visible = visible;
			btn.ToggleGroup = groupIndex.ToString();
			btn.ToggleMode = true;
			btn.Pushed = active;
			if (startGroup) btn.Attributes.Add("startGroup", "1");
			if (visible) visibleBtn = btn;
		}
		if (visibleCount == 1) visibleBtn.Visible = false;
	}

	/// <summary>
	/// Gets the modules data source.
	/// </summary>
	private List<SiteMapNode> GetSubModulesDataSource()
	{
		List<SiteMapNode> source = new List<SiteMapNode>();

		foreach (SiteMapNode node in System.Web.SiteMap.RootNode.ChildNodes)
		{
			if (!node.HasChildNodes || !IsValidNode(node)) continue;
			foreach (SiteMapNode node2 in node.ChildNodes)
			{
				if (!node2.HasChildNodes || !IsValidNode(node2)) continue;
				foreach (SiteMapNode node3 in node2.ChildNodes) source.Add(node3);
			}
		}
		return source;
	}

	//---------------------------------------------------------------------------
	/// <summary>
	/// Create the panel for active navigation tree.
	/// </summary>
	private PXSmartPanel CreateActiveNavigationPanel()
	{
		List<string> reqParts = null;
		if (this.Page.IsCallback)
		{
			string req = this.Request.Params["__CALLBACKID"];
			reqParts = new List<string>(req.Split('$'));
		}

		// create favorites panel
		string module = string.IsNullOrEmpty(this.activeModule) ? this.activeSystem : this.activeModule;
		if (module == _favorites || (reqParts != null && reqParts.Contains("spFav")))
		{
			var panel = CreateNavigationPanel(null, true, "Fav");
			((IParserAccessor)this.menuPanel).AddParsedSubObject(panel);
			return panel;
		}

		bool createPanel = (!this.IsCallback || this.IsReloadMenuPanel);
		if (!createPanel && this.IsCallback)
		{
			string panelID = "sp" + this.activePanelIndex, trId = "tree" + this.activePanelIndex;
			if (reqParts.Contains(panelID) || reqParts.Contains(trId)) createPanel = true;
		}

		if (createPanel)
		{
			if (!string.IsNullOrEmpty(this.activePanelKey))
				this.activeNode = PXSiteMap.Provider.FindSiteMapNodeFromKey(this.activePanelKey);

			if (this.activeNode == null)
			{
				if (!string.IsNullOrEmpty(this.activeSubMod)) module = this.activeSubMod;
				this.activeNode = PXSiteMap.Provider.FindSiteMapNodeFromKey(module);
			}

			var panel = CreateNavigationPanel(this.activeNode, true, this.activePanelIndex);
			((IParserAccessor)this.menuPanel).AddParsedSubObject(panel);
			return panel;
		}
		return null;
	}

	/// <summary>
	/// Create SmartPanel to render navigation tree.
	/// </summary>
	private PXSmartPanel CreateNavigationPanel(SiteMapNode node, bool active, object index)
	{
		string panelID = "sp" + index.ToString();
		bool favorites = (node == null);
		string trId = favorites ? _favTreeID : ("tree" + index.ToString());

		PXSmartPanel panel = new PXSmartPanel();
		panel.ID = panelID;
		panel.Key = favorites ? _favorites : node.Key;
		panel.LoadOnDemand = !active;
		panel.AllowResize = panel.AllowMove = false;
		panel.RenderVisible = active;
		panel.AutoSize.Enabled = true;
		panel.Position = PanelPosition.Original;

		if ((this.IsCallback && !this.IsReloadMenuPanel) || active)
		{
			SiteMapDataSource ds = new SiteMapDataSource();
			ds.ID = "ds" + index.ToString();
			ds.ShowStartingNode = false;
			panel.Controls.Add(ds);

			PXSiteMapNode pxNode = node as PXSiteMapNode;
			Control content = null;
			PXWikiProvider wp = PXSiteMap.WikiProvider;
			
			if (favorites)
			{
				ds.Provider = PXSiteMap.FavoritesProvider;
				ds.StartingNodeUrl = System.Web.SiteMap.RootNode.Url;
				content = CreateTree(ds, trId);
			}
			else if (wp.FindSiteMapNodeFromKey(pxNode.NodeID) != null)
			{
				if (wp.GetWikiID(pxNode.Title) != Guid.Empty || wp.GetWikiIDFromUrl(pxNode.Url) != Guid.Empty)
					content = CreateWikiTree(pxNode, trId);
				if (PXSiteMap.IsPortal) 
					this.moduleLink.Visible = true;
			}
			else
			{
				INavigateUIData dataItem = node as INavigateUIData;
				if (string.IsNullOrEmpty(dataItem.NavigateUrl))
				{
					PXSiteMap.Provider.SetCurrentNode(PXSiteMap.Provider.FindSiteMapNodeFromKey(node.Key));
					ds.StartFromCurrentNode = true;
				}
				else ds.StartingNodeUrl = dataItem.NavigateUrl;
				content = CreateTree(ds, trId);
			}
			if (content != null) panel.Controls.Add(content);
		}
		return panel;
	}
	#endregion

	#region Methods to work with sitemap tree

	//---------------------------------------------------------------------------
	/// <summary>
	/// Create default menu tree control with specified name and data source.
	/// </summary>
	private PXTreeView CreateTree(SiteMapDataSource ds, string controlName)
	{
		PXTreeView tree = new PXTreeView();
		tree.DataSourceID = ds.ID;
		tree.ID = controlName;
		tree.ShowRootNode = false;
		tree.FastExpand = true;
		tree.Target = "main";
		tree.ApplyStyleSheetSkin(this);
		tree.NodeDataBound += new PXTreeNodeEventHandler(tree_NodeDataBound);
		tree.DataBound += tree_DataBound;
		tree.ShowDefaultImages = tree.ShowLines = false;
		tree.Synchronize += new PXTreeSyncEventHandler(tree_Synchronize);
		tree.CssClass += " menuTree";
		tree.ExclusiveExpand = this.IsContextNavigation();
		if(PXSiteMap.IsPortal)
			tree.SearchUrl = this.ResolveUrl("~/Search/WikiSP.aspx") + "?globalsearchcaption=0adv=1&query=";
		else
			tree.SearchUrl = this.ResolveUrl("~/Search/Entity.aspx") + "?globalsearchcaption=1&isWiki=0" + "&query=";
		return tree;
	}

	/// <summary>
	/// Create default Wiki tree control with specified name and data source.
	/// </summary>
	private PXWikiTree CreateWikiTree(PXSiteMapNode node, string treeId)
	{
		PXWikiTree tree = new PXWikiTree();
		string url = node.Url;
		Guid wikiId = PXSiteMap.WikiProvider.GetWikiIDFromUrl(node.Url);
		WikiReader reader = PXGraph.CreateInstance<WikiReader>();
		WikiDescriptor wiki = reader.wikis.SelectWindowed(0, 1, wikiId);
		tree.Provider = PX.Data.PXSiteMap.WikiProvider;
		url = Wiki.Url(wikiId);

		tree.TreeSkin = "Help";
		tree.ID = treeId;
		tree.WikiID = wikiId;
		tree.ShowDefaultImages = tree.ShowRootNode = false;
		tree.Target = "main";
		tree.StartingNodeUrl = this.ResolveUrl(url);
		tree.SearchUrl = this.ResolveUrl("~/Search/Wiki.aspx") + "?globalsearchcaption=0&isWiki=1" + "&query=";
		if (PXSiteMap.IsPortal)
			tree.SearchUrl = this.ResolveUrl("~/Search/WikiSP.aspx") + "?adv=1&query=";
		tree.NewArticleUrl = (wiki == null ||
			string.IsNullOrEmpty(wiki.UrlEdit)) ? "" : this.ResolveUrl(wiki.UrlEdit) + "?wiki=" + wikiId;
		tree.CssClass = "menuTreeHlp";

		tree.ClientEvents.NodeClick = "MainFrame.treeClick";
		tree.Synchronize += new PXTreeSyncEventHandler(wikiTree_Synchronize);
		return tree;
	}

	/// <summary>
	/// Hide left panel for empty tree control.
	/// </summary>
	void tree_DataBound(object sender, EventArgs e)
	{
		PXTreeView tree = sender as PXTreeView;
		if (!this.Page.IsCallback && tree != null && tree.Nodes.Count == 0 && this.IsContextNavigation())
		{
			frameT.Attributes["class"] = "menuHidden"; sp1.Enabled = false;
			hideFrameBox.Style[HtmlTextWriterStyle.Display] = "none";
			frameT.Rows[0].Cells[0].Style[HtmlTextWriterStyle.Display] = "none";
			frameT.Rows[0].Cells[1].Style[HtmlTextWriterStyle.Display] = "none";
		}
	}

	void tree_Synchronize(object sender, PXTreeSyncEventArgs e)
	{
		this.PrepareSynchronizationPath(e, System.Web.SiteMap.Provider);
	}

	void wikiTree_Synchronize(object sender, PXTreeSyncEventArgs e)
	{
		this.PrepareSynchronizationPath(e, PXSiteMap.WikiProvider);
	}

	//---------------------------------------------------------------------------
	/// <summary>
	/// Calculate the Node path for tree sinchronization.
	/// </summary>
	private void PrepareSynchronizationPath(PXTreeSyncEventArgs e, SiteMapProvider prov)
	{
		List<string> path = new List<string>();
		System.Text.StringBuilder result = new System.Text.StringBuilder();
		SiteMapNode node = prov.FindSiteMapNodeFromKey(e.SyncNodeKey);

		while (node != null && node.ParentNode != prov.RootNode)
		{
			path.Add(node.Key);
			node = node.ParentNode;
		}
		for (int i = path.Count - 1; i >= 0; i--)
		{
			result.Append(path[i]);
			result.Append('|');
		}
		if (result.Length != 0) result = result.Remove(result.Length - 1, 1);
		e.NodePath = result.ToString();
	}

	/// <summary>
	/// The tree node bound event handler.
	/// </summary>
	private void tree_NodeDataBound(object sender, PXTreeNodeEventArgs e)
	{
		INavigateUIData dataItem = e.Node.DataItem as INavigateUIData;
		PX.Data.PXSiteMapNode node = e.Node.DataItem as PX.Data.PXSiteMapNode;
		PXTreeView tree = (PXTreeView)sender;
		
		string descr = dataItem.Description;
		// sets the node images
		if (!string.IsNullOrEmpty(descr))
		{
			string[] im = descr.Split('|');
			e.Node.Images.Normal = PXStyle.ResolveImageUrl(this, im[0]);
			if (im.Length > 1) e.Node.Images.Selected = PXStyle.ResolveImageUrl(this, im[1]);
		}
		else
		{
			if (node != null && node.IsFolder)
			{
				e.Node.Images.Normal = Sprite.Tree.GetFullUrl(Sprite.Tree.Folder);
				e.Node.Images.Selected = Sprite.Tree.GetFullUrl(Sprite.Tree.FolderS);
			}
		}

		// sets the node tooltip
		//e.Node.ToolTip = dataItem.Name;
		//if (!string.IsNullOrEmpty(node.ScreenID))
		//	e.Node.ToolTip += string.Format(" ({0})", node.ScreenID);

		if (node != null)
		{
			if (node.ChildNodes.Count == 0 && String.IsNullOrEmpty(node.Url))
			{
				var nodes = e.Node.Parent.ChildNodes;
				if (nodes.Contains(e.Node)) nodes.Remove(e.Node);
			}
			else if (node.Expanded == true && !this.IsContextNavigation()) 
				e.Node.Expanded = true;
		}
		//e.Node.NavigateUrl = PXPageCache.FixPageUrl(e.Node.NavigateUrl);

		SiteMapNode an = this.GetActiveNode(-1);
		string url = e.Node.NavigateUrl;
		if (an != null && url == an.Url) tree.SelectedNode = e.Node;

		// for favorites tree store the actual sitemap data path
		bool isFav = (tree.ID == _favTreeID);
		e.Node.Value = e.Node.DataPath;
		if (isFav)
		{
			var n = PXSiteMap.Provider.FindSiteMapNode(url);
			if (n != null) e.Node.Value = n.Key;
		}

		if (!string.IsNullOrEmpty(url))
		{
			url += url.Contains("?") ? "&" : "?";
			e.Node.NavigateUrl = string.Format("{0}{1}={2}", url, PXUrl.HideScriptParameter, "On");
		}
	}
	#endregion

	#region Methods to work with tools Bar

	//---------------------------------------------------------------------------
	/// <summary>
	/// Initialize the application business date.
	/// </summary>
	private void InitBusinessDate()
	{
		var date = PXContext.GetBusinessDate();
		if (PXSiteMap.IsPortal)
		{
			((PXToolBarButton)toolsBar.Items["businessDate"]).Enabled = false;
			((PXToolBarButton) toolsBar.Items["businessDate"]).Tooltip = "";
		}

		if (date != null)
		{
			this.SetBusinessDateText(date);
			PXDateTimeEdit.SetDefaultDate((DateTime)date);
			if (!Page.IsPostBack || Page.IsCallback)
			{
				PXDateTimeEdit dateCtrl = pnlDate.FindControl("edEffDate") as PXDateTimeEdit;
				if (dateCtrl != null) dateCtrl.Value = date;
			}
		}
		else this.SetBusinessDateText(PXTimeZoneInfo.Now);
	}

	/// <summary>
	/// Sets the business date button text.
	/// </summary>
	private void SetBusinessDateText(object date)
	{
		PXToolBarButton btn = (PXToolBarButton)toolsBar.Items["businessDate"];
		btn.Text = ((DateTime)date).ToShortDateString();
	}

	/// <summary>
	/// Initialize the tasks and events menu.
	/// </summary>
	private void InitTasksAndEvents()
	{
		PXToolBarButton btn = (PXToolBarButton)toolsBar.Items["events"];
		PXTasksAndEventsNavPanel pnl = this.pnlTasksAndEvents;
		if (pnl != null)
		{
			btn.Text = string.Format("{0}({1})", pnl.TodayTasksCount + pnl.TodayEventsCount, pnl.NewTasksCount + pnl.NewEventsCount);
			btn.RenderMenuButton = false;

			Func<PXMenuItem, PXMenuItem> addItem = delegate(PXMenuItem item) { btn.MenuItems.Add(item); return item; };
			addItem(new PXMenuItem(pnl.TasksLabelText)
			{
				Target = "main",
				NavigateUrl = _TASKS_URL,
				ImageSet = Sprite.AliasMain,
				ImageKey = Sprite.Main.Task
			});
			addItem(new PXMenuItem(pnl.EventsLabelText)
			{
				Target = "main",
				NavigateUrl = _EVENTS_URL,
				ImageSet = Sprite.AliasMain,
				ImageKey = Sprite.Main.Event
			});

			var rem = new PX.Web.Controls.TitleModules.ReminderTitleModule();
			rem.PassiveMode = true; rem.Initialize(this);
		}
		else btn.Visible = false;
	}

	//---------------------------------------------------------------------------
	/// <summary>
	/// Initialize the user name button.
	/// </summary>
	private void InitUserName()
	{
		PXToolBarButton btn = (PXToolBarButton)toolsBar.Items["userName"];
		btn.Text = this.GetUserName(false);
		btn.RenderMenuButton = false;


		Func<string, string, string, string> func = delegate(string txt, string val, string labelID)
		{
			return string.Format(
				"<div class='size-s inline-block'>{0}</div> <span id='{1}'>{2}</span>", txt, labelID, val);
		};
		Func<PXMenuItem, PXMenuItem> addItem = delegate(PXMenuItem item) { btn.MenuItems.Add(item); return item; };

		var company = PXLogin.ExtractCompany(this.GetUserName(false));
		if (!String.IsNullOrEmpty(company))
		{
			string[] companies = PXAccess.GetCompanies();
			int lastI = companies.Length - 1;
			if (lastI >= 0) for (int i = 0; i < companies.Length; i++)
				{
					bool active = (companies[i] == company);
					addItem(new PXMenuItem(companies[i])
					{
						AutoPostBack = !active,
						Value = "Company@" + companies[i],
						ShowSeparator = (i == lastI),
						ShowCheckBox = active,
						Checked = active,
						Enabled = !active
					});
				}
		}

		//addItem(new PXMenuItem(func(
		//  PXMessages.LocalizeNoPrefix(PX.AscxControlsMessages.PageTitle.ScreenID), this.GetScreenID(), "screenID")));
		//addItem(new PXMenuItem(func("Version", this.GetVersion(true), "sysVersion")) { ShowSeparator = true });

		//addItem(new PXMenuItem(func(PXMessages.LocalizeNoPrefix(PX.AscxControlsMessages.PageTitle.TimeZone),
		//  this.GetTimeZone(), "timeZone")) { ShowSeparator = true });

		if (!PXSiteMap.IsPortal)
		{
			addItem(new PXMenuItem("Organize Favorites...")
			{
				Target = "main",
				NavigateUrl = _favoritesUrl
			});

			addItem(new PXMenuItem("User Profile...")
			{
				Target = "main",
				ShowSeparator = true,
				NavigateUrl = "~/pages/sm/sm203010.aspx"
			});
		}

		//addItem(new PXMenuItem("Trace...") { NavigateUrl = "~/Frames/Trace.aspx", Target = "_blank"});
		//addItem(new PXMenuItem("About...") { PopupPanel = "pnlAbout", ShowSeparator = true });

		addItem(new PXMenuItem("Log out")
		{
			AutoPostBack = true,
			ImageSet = Sprite.AliasMain,
			ImageKey = Sprite.Main.Logout,
			Value = "LogOut"
		});
		((IPXMenu)btn.DropDownMenu).MenuItemClick += UserNameMenu_ItemClick;
	}
	#endregion

	#region Helper methods

	//---------------------------------------------------------------------------
	/// <summary>
	/// Calculate the start page url.
	/// </summary>
	private string GetLastUrl()
	{
		string lastUrl = (string)PXContext.Session["LastUrl"];
		if (lastUrl == null) lastUrl = this.GetDefaultUrl();

		string screen = Page.Request.QueryString["ScreenId"];		
		if (string.IsNullOrEmpty(lastUrl) || !string.IsNullOrEmpty(screen))
		{
			string url = lastUrl;
			if (!string.IsNullOrEmpty(url))
			{
				int i = url.IndexOf('?'); if (i > 0) url = url.Substring(0, i);
			}
			if (string.IsNullOrEmpty(url) || !url.Replace(".aspx", "").ToUpper().EndsWith(screen.ToUpper()))
			{
				PXSiteMapNode node = (screen != null) ? PXSiteMap.Provider.FindSiteMapNodeByScreenID(screen) : null;
				
				string query = Page.Request.Url.Query.Replace("ScreenId=" + screen,
					String.Empty).Replace("ScreenID=" + screen, String.Empty).Replace("?", String.Empty);

				if (node == null && !string.IsNullOrEmpty(screen) && screen.ToLower() == "showwiki")
					lastUrl = this.ResolveUrl("~/Wiki/Show.aspx");
				else
				{
					if (node != null && !string.IsNullOrEmpty(node.Url)) lastUrl = this.ResolveUrl(node.Url);
					else lastUrl = this.ResolveUrl("Frames/Default.aspx");
				}

				lastUrl = PX.Common.PXUrl.CombineParameters(lastUrl, query);
			}
		}
		return lastUrl;
	}

	/// <summary>
	/// Calculate the start page url.
	/// </summary>
	private string GetDefaultUrl()
	{
		string url = null;
		PXGraph graph = new PXGraph();
		UserPreferences userPref = PXSelectReadonly<UserPreferences,
				Where<UserPreferences.userID, Equal<Required<UserPreferences.userID>>>>.Select(graph, PXAccess.GetUserID());
		if (userPref != null) url = ResolveLastUrl(userPref.HomePage);

		if (url == null)
		{
			PreferencesGeneral sitePref = PXSelectReadonly<PreferencesGeneral>.Select(graph, PXAccess.GetUserID());
			if (sitePref != null) url = ResolveLastUrl(sitePref.HomePage);
		}

		// start page for portal
		if (PXSiteMap.IsPortal)
		{
			for (int i = 0; i < 9; i++)
			{
				string screenname = "SP_00_00_0" + i.ToString();
				WikiPage page = PXSelect<WikiPage,
					Where<WikiPage.name, Equal<Required<WikiPage.name>>>>.SelectWindowed(graph, 0, 1, screenname);
				if (page != null)
					if (PXSiteMap.WikiProvider.GetAccessRights(page.PageID.Value) >= PXWikiRights.Select)
					{
						PXWikiMapNode node =
							(PXWikiMapNode) PXSiteMap.WikiProvider.FindSiteMapNodeFromKey(page.PageID.GetValueOrDefault());
						if (node != null)
						{
							return ResolveUrl(node.Url);
						}
					}
			}
		}
		return url;
	}

	/// <summary>
	/// Gets the page Url without session identifier.
	/// </summary>
	private static string GetPureLastUrl(string url)
	{
		int i1 = url.IndexOf("/(W("), i2 = url.IndexOf("/", i1 + 1);
		if (i1 > 0) url = url.Substring(0, i1) + url.Substring(i2);
		return url;
	}

	/// <summary>
	/// Gets the full page Url by specified page identifier.
	/// </summary>
	private static string ResolveLastUrl(object homePage)
	{
		if (homePage == null) return null;
		PXSiteMapNode lastUrlnode = PXSiteMap.Provider.FindSiteMapNodeFromKey((Guid)homePage);
		
		while (lastUrlnode != null && string.IsNullOrEmpty(lastUrlnode.Url))
			lastUrlnode = (PXSiteMapNode)PXSiteMap.Provider.GetParentNode(lastUrlnode);
		
		return lastUrlnode != null && 
			!string.IsNullOrEmpty(lastUrlnode.Url) ? lastUrlnode.Url.Remove(0, 2) : string.Empty;
	}

	/// <summary>
	/// Check access right for specified Url.
	/// </summary>
	private bool VerifyRightsOnScreen(string screenUrl)
	{
		PXSiteMapNode node = PXSiteMap.Provider.FindSiteMapNode(screenUrl) as PXSiteMapNode;
		return node == null ? false : PXAccess.VerifyRights(node.ScreenID);
	}

	//---------------------------------------------------------------------------
	/// <summary>
	/// Initialize the search control.
	/// </summary>
	private void InitSearchBox()
	{
		PXSearchBox box = this.searchBox;
		if (PXSiteMap.IsPortal)
		{
			box.SearchNavigateUrl = this.ResolveUrl/**/("~/Search/WikiSP.aspx") + "?globalsearchcaption=0adv=1&query=";
		}
		else
		{
			SiteMapNode currenttopnode = PXSiteMap.Provider.FindSiteMapNodeFromKey(this.activeModule);
			if (currenttopnode != null && currenttopnode.ParentNode != null && currenttopnode.ParentNode.Title == "Help")
				box.SearchNavigateUrl = this.ResolveUrl("~/Search/Wiki.aspx") + "?globalsearchcaption=0&isWiki=1&adv=1" + "&query=";
			else
				box.SearchNavigateUrl = this.ResolveUrl("~/Search/Entity.aspx") + "?globalsearchcaption=1&isWiki=0" + "&query=";
		}

		box.Text = PXMessages.LocalizeNoPrefix(PX.AscxControlsMessages.SearchBox.TypeYourQueryHere);
		box.ToolTip = PXMessages.LocalizeNoPrefix(PX.AscxControlsMessages.SearchBox.SearchSystem);
		box.AddNewVisible = false;
	}

	/// <summary>
	/// Gets the root nodes data source.
	/// </summary>
	private List<SiteMapNode> GetNavPanelDataSource()
	{
		List<SiteMapNode> source = new List<SiteMapNode>();
		SiteMapNode favorite = null;
		string module = this.activeModule, system = this.activeSystem;
		SiteMapNodeCollection nodes = null, nodes2 = null;

		if (!string.IsNullOrEmpty(system))
		{
			SiteMapNode node2 = null;
			foreach (SiteMapNode node in System.Web.SiteMap.RootNode.ChildNodes)
				if (node.Key == system) { nodes = node.ChildNodes; node2 = node; break; }
			if (node2 != null && nodes.Count == 0) source.Add(node2);
		}

		foreach (SiteMapNode node in nodes)
		{
			PXSiteMapNode pxNode = node as PXSiteMapNode;
			bool flag = true;
			if (pxNode != null)
			{
				if (pxNode.ScreenID == "FV000000") { favorite = node; flag = false; }
				else if (pxNode.ScreenID == "HD000000") flag = false;
			}

			if (!string.IsNullOrEmpty(module))
			{
				if (module == node.Key)
				{ 
					nodes2 = node.ChildNodes;
					if (nodes2.Count == 0) source.Add(node);
					break;
				}
			}
			else if (flag) source.Add(node);
		}

		if (nodes2 != null) foreach (SiteMapNode node in nodes2)
			{
				PXSiteMapNode pxNode = node as PXSiteMapNode;
				if (pxNode == null || (pxNode.ScreenID != "HD000000")) source.Add(node);
			}
		
		bool? fvExists = PXContext.Session.FavoritesExists["FavoritesExists"];
		if (fvExists == null)
		{
			fvExists = PX.Data.PXSiteMap.FavoritesProvider.FavoritesExists();
			PXContext.Session.FavoritesExists["FavoritesExists"] = fvExists;
		}

		//if ((bool)fvExists && favorite != null) source.Insert(0, favorite);
		return source;
	}

	//---------------------------------------------------------------------------
	/// <summary>
	/// Initialize task and events naviagtion panel.
	/// </summary>
	private void InitializeTaskPanel()
	{
		bool hasRightsOnTasks;
		bool hasRightsOnEvents;
		if (System.Web.Compilation.BuildManager.GetType(_TASKS_AND_EVENTS_REMINDER_GRAPH, false) != null &&
			((hasRightsOnTasks = VerifyRightsOnScreen(_TASKS_URL)) | (hasRightsOnEvents = VerifyRightsOnScreen(_EVENTS_URL))))
		{
			this.pnlTasksAndEvents = new PXTasksAndEventsNavPanel();
			pnlTasksAndEvents.ID = "pnlTasksAndEvents";
			pnlTasksAndEvents.Width = Unit.Percentage(100.0D);
			pnlTasksAndEvents.ImgCss = "treeImage";
			pnlTasksAndEvents.GraphType = _TASKS_AND_EVENTS_REMINDER_GRAPH;
			pnlTasksAndEvents.GetOpenTasksCountMethod = "GetOpenTasksCount";
			pnlTasksAndEvents.GetTodayTasksCountMethod = "GetTodayTasksCount";
			pnlTasksAndEvents.GetNewTasksCountMethod = "GetNewTasksCount";
			pnlTasksAndEvents.GetTasksFilterIDMethod = "GetTasksDefaultFilterID";
			pnlTasksAndEvents.GetOpenEventsCountMethod = "GetOpenEventsCount";
			pnlTasksAndEvents.GetTodayEventsCountMethod = "GetTodayEventsCount";
			pnlTasksAndEvents.GetNewEventsCountMethod = "GetNewEventsCount";
			pnlTasksAndEvents.GetEventsFilterIDMethod = "GetEventsDefaultFilterID";
			pnlTasksAndEvents.Target = "main";
			pnlTasksAndEvents.TaskImage = Sprite.Main.GetFullUrl(Sprite.Main.DataEntry);
			pnlTasksAndEvents.TasksLabel = PX.Data.PXMessages.LocalizeNoPrefix(PX.AscxControlsMessages.TasksAndEventsPanel.Tasks);
			pnlTasksAndEvents.TasksUrl = _TASKS_URL;
			pnlTasksAndEvents.TasksFilterMember = "Tasks";
			pnlTasksAndEvents.TasksFilterName = "TS_OPEN";
			pnlTasksAndEvents.ShowTasks = hasRightsOnTasks;
			pnlTasksAndEvents.EventImage = Sprite.Main.GetFullUrl(Sprite.Main.Calendar);
			pnlTasksAndEvents.EventsLabel = PX.Data.PXMessages.LocalizeNoPrefix(PX.AscxControlsMessages.TasksAndEventsPanel.Events);
			pnlTasksAndEvents.EventsUrl = _EVENTS_URL;
			pnlTasksAndEvents.EventsFilterMember = "Events";
			pnlTasksAndEvents.EventsFilterName = "EV_TODAY";
			pnlTasksAndEvents.ShowEvents = hasRightsOnEvents;
			divNavPanel.Controls.Add(pnlTasksAndEvents);
		}
		else
		{
			FindControl("divNavPanel").Visible = false;
		}
	}

	/// <summary>
	/// Sets the logo image for active company.
	/// </summary>
	private void SetCompanyLogo()
	{
		// set company logo
		PXResult<Branch, UploadFile> res = (PXResult<Branch, UploadFile>)PXSelectJoin<Branch,
				InnerJoin<UploadFile, On<Branch.logoName, Equal<UploadFile.name>>>,
				Where<Branch.branchCD, Equal<Required<Branch.branchCD>>>>.Select(new PXGraph(), PXAccess.GetBranchCD());
		if (res != null)
		{
			UploadFile file = (UploadFile)res;
			if (file != null)
				this.logoImg.ImageUrl = ControlHelper.GetAttachedFileUrl(this, file.FileID.ToString());
		}

		string url = GetDefaultUrl();
		if (string.IsNullOrEmpty(url)) url = this.ResolveUrl("~/Frames/Default.aspx");
		url = ControlHelper.FixHideScriptUrl(url, true);
		this.logoCell.HRef = url;
	}

	//---------------------------------------------------------------------------
	/// <summary>
	/// Gets the active user name.
	/// </summary>
	private string GetUserName(bool addBranch)
	{
		if (PXContext.PXIdentity.Authenticated)
		{
			userName = PXContext.PXIdentity.IdentityName;
			if (addBranch)
			{
				string branch = PXAccess.GetBranchCD();
				if (!string.IsNullOrEmpty(branch)) userName += ":" + branch;
			}
		}
		return this.userName;
	}
	
	/// <summary>
	/// Gets the active time zone.
	/// </summary>
	private string GetTimeZone()
	{
		if (timeZone == null) timeZone = LocaleInfo.GetTimeZone().ShortName;
		return timeZone;
	}

	/// <summary>
	/// Gets the current version of the system.
	/// </summary>
	private string GetVersion(bool raw)
	{
		string ver = PXVersionInfo.Version;
		if (!raw) ver = PXMessages.LocalizeFormatNoPrefix(PX.AscxControlsMessages.PageTitle.Version, ver);

		string cstProjects = Customization.CstWebsiteStorage.PublishedProjectList;
		if (!string.IsNullOrEmpty(cstProjects))
			ver += " " + PXMessages.LocalizeNoPrefix(PX.AscxControlsMessages.PageTitle.Customization) + cstProjects;
		return ver;
	}

	/// <summary>
	/// Gets the active screen number.
	/// </summary>
	private string GetScreenID()
	{
		if (this.screenID == null) this.screenID = ControlHelper.GetScreenID();
		return this.screenID;
	}

	/// <summary>
	/// Returns true if site support context navigation.
	/// </summary>
	private bool IsContextNavigation()
	{
		var cn = System.Configuration.ConfigurationManager.AppSettings["ContextNavigation"];
		return !string.IsNullOrEmpty(cn) && cn.ToLower() == "true";
	}
	#endregion

	#region ITitleModuleController

	void ITitleModuleController.AppendControl(Control control)
	{
		Page.Form.Controls.Add(control);
	}

	void ITitleModuleController.AppendToolbarItem(PXToolBarItem item)
	{
		var cont = item as PXToolBarContainer;
		if (cont != null && cont.TemplateContainer.Controls.Count > 0)
		{
			var but = cont.TemplateContainer.Controls[0] as PXNewsCheckerButton;
			if (but != null)
			{
				but.NormalCss = toolsBar.Styles.Normal.CssClass;
				but.HoverCss = toolsBar.Styles.Normal.CssClass;
				but.PushedCss = toolsBar.Styles.Pushed.CssClass;
				toolsBar.Items.Insert(1, item);
			}
		}
	}

	Page ITitleModuleController.Page
	{
		get { return this; }
	}
	#endregion

	#region Fields
	private const string _TASKS_AND_EVENTS_REMINDER_GRAPH = "PX.Objects.EP.TasksAndEventsReminder";
	private const string _TASKS_URL = "~/Pages/EP/EP404000.aspx";
	private const string _EVENTS_URL = "~/Pages/EP/EP404100.aspx";
	private const string _favoritesUrl = "~/pages/sm/sm203020.aspx";
	private const string _favorites = "favorites";
	private const string _favTreeID = "favTree";
	private const string _activeSystemKey = "__activeSystem";
	private const string _activeModuleKey = "__activeModule";
	private const string _activeSubModKey = "__activeSubMod";
	private const string _activePanelKey = "__activePanel";

	//private bool bindComplete = false;
	private string activeSystem = "", activeModule = "", activeSubMod = "";
	private bool favoritesActive = false;
	private SiteMapNode activeNode;
	private string activePanelIndex = "0", activePanelKey;
	private string screenID, userName, timeZone;
	private PXTasksAndEventsNavPanel pnlTasksAndEvents;
	#endregion
}
