<%@ Application Language="C#" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.Runtime.InteropServices" %>
<%@ Import Namespace="System.Threading" %>
<%@ Import Namespace="System.Web.Configuration" %>
<%@ Import Namespace="PX.Common" %>
<%@ Import Namespace="PX.Data" %>
<%@ Import Namespace="PX.Data.Maintenance" %>
<%@ Import Namespace="PX.SM" %>
<%@ Import Namespace="PX.Web.UI" %>

<script RunAt="server">
	protected void Application_PostAuthorizeRequest()
	{		
		// turning on session state for webapi requests
		if (HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath.StartsWith("~/" + PX.Api.ApiConfiguration.RestfulApiUriBase))
		{
			HttpContext.Current.SetSessionStateBehavior(SessionStateBehavior.Required);
		}
	}

	void Application_Start(object sender, EventArgs e)
	{
        AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(App_SubCode_Caches);
        PX.Data.PXFirstChanceExceptionLogger.Initialise();
        PX.Api.ApiConfiguration.Configure();
        PX.Common.AzureConfigurationManager.PatchWebConfig();
        
		if (System.IO.File.Exists(System.Web.Hosting.HostingEnvironment.MapPath("~/Bin/App_Subcode_Caches.dll")))
			throw new ApplicationException("Remove App_Subcode_Caches.dll from Website/Bin directory.");

        PX.SM.PXPerformanceMonitor.Init();
		Customization.WebsiteEntryPoints.InitCustomizationResourceProvider();
		PX.Web.UI.AssemblyResourceProvider.MergeAssemblyResourcesIntoWebsite<PX.Web.Controls.PXResPanelEditor>();
		PX.Web.UI.AssemblyResourceProvider.MergeAssemblyResourcesIntoWebsite<PX.Web.UI.PXResPanelBase>();

		//Updating Database. If update started prevent system tasks to start
		if (PX.Data.Update.PXUpdateHelper.CheckAndUpdate()) return;

		Initialization.ProcessApplication();

		PX.Data.ScheduleProcessor.Initialize();
		PXPage.CompileAllPagesAsync();
	}

    System.Reflection.Assembly App_SubCode_Caches(object sender, ResolveEventArgs args)
    {
        if (args.Name != null && args.Name.StartsWith("App_SubCode_Caches."))
        {
            return AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(_ => _.FullName != null && (_.FullName.StartsWith("App_SubCode_Caches,") || _.FullName.StartsWith("App_SubCode_Caches.")));
        }
        return null;
    }

	void Session_End(object sender, EventArgs e)
	{
		string login = PXContext.Session["UserLogin"] as string;
		if (string.IsNullOrEmpty(login))
			return;
		try
		{
			PX.Data.PXLogin.SessionExpired(login, PXContext.Session["IPAddress"] as string, Session.SessionID);
		}
		catch
		{
		}
	}

	protected void Application_PostAuthenticateRequest(object sender, EventArgs e)
	{
		//after authentiaction we need to reaply user personal culture format settings
		//LocaleInfo.ReaplyAllCulture();
	
		if (Request.IsAuthenticated && HttpContext.Current != null)
		{
			
			String cpny = HttpContext.Current.Request.QueryString["cpid"];
			// executes request on behalf of a different company
			Int32 cpid;
			if (!String.IsNullOrEmpty(cpny) && Int32.TryParse(cpny, out cpid))
				PX.Data.PXDatabase.InitializeRequest(cpid);
		}
		if (!Request.IsAuthenticated && Request.HttpMethod == "POST" && Request.Form["__CALLBACKID"] != null)
		{
			throw new PX.Data.PXNotLoggedInException();
		}

		if (Request.RequestType == "POST" && Request.Form["FileUniqueKey"] != null)
		{
			if (Request.ContentLength > PX.Data.Process.RequestSizeValidator.MaxUploadSize * 1024)
			{
				PX.Data.Process.RequestSizeValidator.SetIsSizeValid(Request.Form["FileUniqueKey"], false);
				System.Threading.Thread.Sleep(2000);
			}
			else
				PX.Data.Process.RequestSizeValidator.SetIsSizeValid(Request.Form["FileUniqueKey"], true);
		}
	}

	void Application_PreRequestHandlerExecute(object sender, EventArgs e)
	{
		
		PXSessionContextFactory.BeginRequest();
		PX.Data.PXSessionStateStore.InitExtensions();

		LocaleInfo.SetAllCulture();
		Initialization.ProcessHandler();

		PX.Reports.Web.WebReport.ClearSessionReport();

		String url = HttpContext.Current.Request.Url.ToString();
		if (!url.Contains("Frames/Maintenance.aspx") && PXContext.PXIdentity.Authenticated &&
			!string.IsNullOrEmpty(PXContext.PXIdentity.IdentityName) &&
			!"anonymous".Equals(PXContext.PXIdentity.IdentityName, StringComparison.OrdinalIgnoreCase)
			&& !PXContext.PXIdentity.User.IsInRole(PXAccess.GetAdministratorRole())
			&& PX.Data.Maintenance.PXSiteLockout.GetStatus() == PXSiteLockout.Status.Locked)
		{
			Redirector.RedirectPage(HttpContext.Current, "~/Frames/Maintenance.aspx");
		}

		HttpApplication app = sender as HttpApplication;
		if (app == null) return;
		Page p = app.Context.Handler as Page;
		if (p == null) return;
		if (p.AppRelativeVirtualPath == PX.Data.PXSiteMap.DefaultFrame) return;
		
		if (app.Context.User != null &&
			!string.IsNullOrEmpty(PXContext.PXIdentity.IdentityName) &&
			!"anonymous".Equals(PXContext.PXIdentity.IdentityName, StringComparison.OrdinalIgnoreCase))
		{
			Customization.WebsiteEntryPoints.InitPageDesignMode(p);
		}
	}
	void Application_PostRequestHandlerExecute(object sender, EventArgs e)
	{
		if (this.Context.CurrentHandler is PXPage) PXPageCache.SetCookie(this.Context);
		PXSessionContextFactory.EndRequest();
		PX.Data.PXConnectionList.CheckConnections();
	}

	private static readonly int _requestLatency = RequestLatency;
	static int RequestLatency
	{
		get
		{
			string config = WebConfigurationManager.AppSettings["RequestLatency"];
			int result;
			int.TryParse(config, out result);
			return result;

		}
	}
    void Application_ReleaseRequestState(object sender, EventArgs e)
    {
        PXLongOperation.OnReleaseRequestState();
    }
	void Application_BeginRequest(object sender, EventArgs e)
	{
		if (_requestLatency > 0)
			Thread.Sleep(_requestLatency);

		if (!Request.IsSecureConnection
		&& PX.Common.AzureConfigurationManager.IsAzure
		&& Request.Headers["X-Host"] == null)
		{
			string url = String.Format("https://{0}{1}", Request.Headers["Host"], VirtualPathUtility.ToAbsolute("~/main.aspx"));
			Response.Redirect(url, true);
			return;
		}

		Initialization.ProcessRequest();
		Customization.WebsiteEntryPoints.ApplicationBeginRequest();
	}

	void Application_EndRequest(object sender, EventArgs e)
	{
		if (PXContext.PXIdentity.Authenticated)
		{
			PXTimeZoneInfo timeZone = LocaleInfo.GetTimeZone();
			HttpCookie localeCookie = Response.Cookies["Locale"];
			if (timeZone != null) localeCookie["TimeZone"] = timeZone.Id;
			if (string.IsNullOrEmpty(localeCookie["Culture"]))
				localeCookie["Culture"] = LocaleInfo.GetCulture().Name;

			int? branchId = PXContext.GetBranchID();
			HttpCookie branchCooky = Response.Cookies["UserBranch"];
			string branchObj = branchId == null ? null : branchId.ToString();
			if (branchCooky == null)
			{
				Response.Cookies.Add(new HttpCookie("UserBranch", branchObj));
			}
			else
			{
				branchCooky.Value = branchObj;
			}
		}
	}

	void Application_PreSendRequestHeaders(object sender, EventArgs e)
	{
		PX.Web.UI.PXImages.WriteCacheHeaders(this.Context);
		if (Request.RawUrl.Contains("QR.axd")) AssemblyResourceLoader.WriteResponseHeaders(Context);
	}

	void Application_Error(Object sender, EventArgs e)
	{
		//if we have special header or query string parameter than we should skip this handler. (Debugging purpose)
		if (Request.Headers.Get("ShowError") != null || Request.QueryString["ShowError"] != null) return;

		HttpContext ctx = HttpContext.Current;
		if (string.Compare(ctx.Request.HttpMethod, "GET", true) != 0 &&
			string.Compare(ctx.Request.HttpMethod, "POST", true) != 0)
		{
			return;
		}
		if (Request.IsAuthenticated && PX.Data.PXDatabase.RequiresLogOut)
		{
			ctx.ClearError();
			PX.Export.Authentication.FormsAuthenticationModule.SignOut();
			Session.Abandon();
			Response.Clear();

			if (Request.Form["__CALLBACKID"] != null)
			{
				if (Request.QueryString["PopupPanel"] != null) Response.Write("eClosePopup");
				else PX.Data.Redirector.Refresh(ctx);
				Response.StatusCode = (int)PX.Export.HttpStatusCodes.Forbidden;
				Response.End();
			}
			else
			{
				string retUrl = HttpUtility.UrlEncode(PX.Common.PXUrl.ToAbsoluteUrl("~/Main.aspx"));
				string logUrl = PX.Export.Authentication.FormsAuthenticationModule.LoginUrl;
				string NavigateUrl = string.Format("{0}?ReturnUrl={1}", logUrl, retUrl);
				PX.Data.Redirector.RedirectPage(ctx, NavigateUrl);
			}
			return;
		}

		Exception exception = ctx.Server.GetLastError();
		HttpException serverError = exception as HttpException;
		if (serverError != null && serverError.GetHttpCode() == 404)
		{
			ctx.ClearError();
			if (string.Compare(ctx.Request.HttpMethod, "GET", true) == 0)
				Response.Redirect("~/Frames/Default.aspx");
		}

		// if database locked for update
		if (FindException(typeof(PX.Data.PXUnderMaintenanceException), exception))
		{
			ctx.ClearError();
			PX.Data.Redirector.RedirectPage(ctx, "~/Frames/Maintenance.aspx");
			return;
        }
        // if loged out due to license
        if (FindException(typeof(PX.Data.PXLicenseExceededException), exception))
        {
            ctx.ClearError();
            return;
        }

		PX.Data.PXSetupNotEnteredException setPropException = exception.InnerException as PX.Data.PXSetupNotEnteredException;
		if (setPropException == null &&
				exception is HttpUnhandledException &&
				exception.InnerException is System.Reflection.TargetInvocationException &&
				exception.InnerException.InnerException is PX.Data.PXSetupNotEnteredException)
		{
			setPropException = exception.InnerException.InnerException as PX.Data.PXSetupNotEnteredException;
		}

		if ((Request.IsLocal && !PX.Common.AzureConfigurationManager.IsAzure && (setPropException == null))
			|| (Request.RawUrl != null && Request.RawUrl.EndsWith("/Frames/Error.aspx")))
			return;

		Guid? exceptionId = null;
		try
		{
			if (exception is HttpUnhandledException)
			{
				exception = exception.InnerException;
				if (exception is System.Reflection.TargetInvocationException)
				{
					exception = exception.InnerException;
				}
			}
			PX.Data.PXTrace.WriteError(exception);
			PX.Data.PXException pe = exception as PX.Data.PXException;
			if (setPropException != null)
			{
				exceptionId = Guid.NewGuid();
				PXContext.Session.Exception[exceptionId.ToString()] = setPropException;
			}
			else if (pe != null)
			{
				exceptionId = Guid.NewGuid();
				PXContext.Session.Exception[exceptionId.ToString()] = pe;
			}
		}
		catch
		{
		}

		if (Request.HttpMethod != "POST" || Request.Form["__CALLBACKID"] == null)
		{
			try
			{
				PX.Data.PXEventLog.WriteGeneralError(exception);
			}
			catch
			{
			}

			//we need to clear error on IntegratedPipeline. overwise redirect will not work correctly.
			if (System.Web.HttpRuntime.UsingIntegratedPipeline) ctx.ClearError();

			if (exceptionId != null)
			{
				PX.Data.Redirector.Redirect(ctx, string.Format("~/Frames/Error.aspx?exceptionID={0}", exceptionId.ToString()));
			}
			else
			{
				PX.Data.Redirector.Redirect(ctx, "~/Frames/Error.aspx");
			}
		}
	}

	bool FindException(Type exType, Exception ex)
	{
		if (ex != null)
		{
			if (ex.GetType() == exType) return true;
			if (ex.InnerException != null) return FindException(exType, ex.InnerException);
		}
		return false;
	}
</script>

