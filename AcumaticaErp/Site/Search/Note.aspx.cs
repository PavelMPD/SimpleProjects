using System;
using System.Web;
using PX.Common;
using PX.Data;
using PX.Data.Search;
using System.Collections.Generic;
using PX.Web.Controls.Wiki;
using PX.Web.UI;

public partial class Search_Note : SearchPage
{
	private static readonly string _SEARCH_TIPS = "<br /><div class=\"wiki\"><b>" +
		PXMessages.LocalizeNoPrefix(Messages.SearchTips) + "</b><ul class=\"wikibulletlist\"><li>" +
		PXMessages.LocalizeNoPrefix(Messages.CheckSpelling) + "</li><li>" +
		PXMessages.LocalizeNoPrefix(Messages.SimplifyQuery) + "</li><ul></div>";

	protected override string SearchTips
	{
		get { return _SEARCH_TIPS; }
	}

	protected override TitlePanel Header
	{
		get { return usrCaption; }
	}

	protected override SearchPanel Search
	{
		get { return ctrlSearch; }
	}

	protected override string ArticleUrl
	{
		get { return "~/Wiki/ShowWiki.aspx"; }
	}

	protected override string HeaderScreenID
	{
		get
		{
			return "SE.00.00.50";
		}
	}

	protected override string HeaderScreenTitle
	{
		get
		{
			return PXMessages.LocalizeNoPrefix(Messages.scrNoteSearch);
		}
	}

	protected override SearchType Type
	{
		get
		{
			return SearchType.Notes;
		}
	}

	protected override PX.Web.UI.GridPagerMode PagerMode
	{
		get
		{
			return GridPagerMode.NextPrevFirstLast;
		}
	}

	protected override bool PerformSearch(string query, bool isQuick)
	{
		DateTime start = DateTime.Now;
		PXNoteSearch search = new PXNoteSearch();

		List<PXSearchResult> searchResults = new List<PXSearchResult>();
		Search.NavigationHandler = delegate(string args) { return NavHandler(args, searchResults); };
		search.IgnoreCache = Search.CurrentPage == 0 ? true : false;
		searchResults.AddRange(search.Search(query, Search.CurrentPage * 20, 20));
		if (searchResults.Count == 0)
		{
			Search.DisplaySearchTips(query, SearchTips);
			return false;
		}

		int i = Search.CurrentPage * 20;
		Search.MaxDescriptionColumns = search.MaxDescriptionColumns;
		foreach (PXNoteSearch.Result res in searchResults)
		{
			Search.DisplayResult(res);
			i++;
		}

		TimeSpan span = DateTime.Now.Subtract(start);
		Search.DisplayResult(PXMessages.LocalizeFormatNoPrefix(Messages.SearchResultsShort, HttpUtility.HtmlEncode(query), this.ctrlSearch.FormatTimeResult(span)));
		Search.PagesCount = search.TotalCount % 20 == 0 ? search.TotalCount / 20 : search.TotalCount / 20 + 1;
		Search.PagerHasPrev = search.HasPrevPage;
		Search.PagerHasNext = search.HasNextPage;
		return true;
	}

	protected override void AcceptInput()
	{
		var url = "query=" + HttpUtility.UrlEncode(TrimLongString(Search.Text)) + "&wikiid=" + this.Search.WikiID + "&wikinumber=" + this.Search.WikiNumber + "&categoryID=" + this.Search.CategoryID +
			"&productID=" + this.Search.ProductID + "&orderID=" + this.Search.OrderID + "&isWiki=" + this.Search.IsWiki + "&globalsearchcaption=" + this.Search.Globalsearchcaption + "&adv=1";
		Response.Redirect(PX.Common.PXUrl.ToAbsoluteUrl(Request.AppRelativeCurrentExecutionFilePath) + '?' + url, false);
	}

	private bool NavHandler(string args, IList<PXSearchResult> searchResults)
	{
		int num = Convert.ToInt32(args.Split(':')[1]);
		try
		{
			searchResults[num].Description[0].CaptionButtons[0].DoClickHandling(searchResults[num]);
		}
		catch (PXRedirectRequiredException ex)
		{
			string url = PX.Web.UI.PXDataSource.getMainForm(ex.Graph.GetType());
			if (url != null)
			{
				ex.Graph.Unload();
				PXContext.Session.RedirectGraphType[VirtualPathUtility.ToAbsolute(url)] = ex.Graph.GetType();
				throw new PXRedirectRequiredException(url, ex.Graph, "Redirect0:" + this.ResolveUrl/**/(url));
			}
		}
		return false;
	}
}
