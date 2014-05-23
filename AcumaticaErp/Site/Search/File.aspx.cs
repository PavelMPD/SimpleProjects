using System;
using System.Web;
using System.Web.UI;
using PX.Data;
using PX.Data.Search;
using System.Collections.Generic;
using PX.Web.Controls.Wiki;

public partial class Wiki_SearchFile : SearchPage
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
		get { return "~/Pages/SM/SM202510.aspx"; }
	}

	protected override string HeaderScreenID
	{
		get
		{
			return "SE.00.00.20";
		}
	}

	protected override string HeaderScreenTitle
	{
		get
		{
			return PXMessages.LocalizeNoPrefix(Messages.scrFileSearch);
		}
	}

	protected override SearchType Type
	{
		get
		{
			return SearchType.Files;
		}
	}

	protected override bool PerformSearch(string query, bool isQuick)
	{
		DateTime start = DateTime.Now;
		PXFileSearch search = new PXFileSearch();

		string url = ArticleUrl;
		search.LinkFormat = string.Concat(url, url.Contains("?") ? string.Empty : "?", "fileID={0}");
		search.IgnoreCache = Search.CurrentPage == 0 ? true : false;
		List<PXSearchResult> files = search.Search(query, Search.CurrentPage * 20, 20);

		if (files.Count == 0)
		{
			Search.DisplaySearchTips(query, SearchTips);
			return false;
		}

		int i = Search.CurrentPage * 20;
		foreach (PXFileSearch.Result res in files)
		{
			Search.DisplayResult(res);
			//this.Search.DisplayResult(res.GetLink(), res.LinkText, new LiteralControl(this.FormatCategories(res.Categories)), new LiteralControl(res.Description));
			i++;
		}

		TimeSpan span = DateTime.Now.Subtract(start);
		Search.DisplayResult(PXMessages.LocalizeFormatNoPrefix(Messages.SearchResults, Search.CurrentPage * 20 + 1, i, search.TotalCount, HttpUtility.HtmlEncode(query), this.Search.FormatTimeResult(span)));
		Search.PagesCount = search.TotalCount % 20 == 0 ? search.TotalCount / 20 : search.TotalCount / 20 + 1;
		return true;
	}
	
	protected override void AcceptInput()
	{
		var url = "query=" + HttpUtility.UrlEncode(TrimLongString(Search.Text)) + "&wikiid=" + this.Search.WikiID + "&wikinumber=" + this.Search.WikiNumber + "&categoryID=" + this.Search.CategoryID +
			"&productID=" + this.Search.ProductID + "&orderID=" + this.Search.OrderID + "&isWiki=" + this.Search.IsWiki + "&globalsearchcaption=" + this.Search.Globalsearchcaption + "&adv=1";
		Response.Redirect(PX.Common.PXUrl.ToAbsoluteUrl(Request.AppRelativeCurrentExecutionFilePath) + '?' + url, false);
	}
}
