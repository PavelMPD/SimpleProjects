using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using PX.Data;
using PX.Data.Search;
using PX.Web.Controls.Wiki;
using Messages = PX.Data.Search.Messages;

public partial class Page_WikiNew : SearchPage
{
	private static readonly string _SEARCH_TIPS = "<br /><div class=\"wiki\"><b>" +
		PXMessages.LocalizeNoPrefix(Messages.SearchTips) + "</b><ul class=\"wikibulletlist\"><li>" +
		PXMessages.LocalizeNoPrefix(Messages.CheckSpelling) + "</li><li>" +
		PXMessages.LocalizeNoPrefix(Messages.SimplifyQuery) + "</li><li>" +
		PXMessages.LocalizeNoPrefix(Messages.TryFullTextSearch) + "</li><ul></div>";

	protected override string SearchTips
	{
		get { return _SEARCH_TIPS; }
	}

	protected override void InitHeader()
	{
		Search.WikiID = Request["wikiid"];
		base.InitHeader();
	}

	protected override TitlePanel Header
	{
		get
		{
			return usrCaption;
		}
	}

	protected override SearchPanel Search
	{
		get
		{
			return ctrlSearch;
		}
	}

	protected override string ArticleUrl
	{
		get
		{
			return "~/Wiki/ShowWiki.aspx";
		}
	}

	protected override string HeaderScreenID
	{
		get
		{
			return "SE.00.00.10";
		}
	}

	protected override string HeaderScreenTitle
	{
		get
		{
			return PXMessages.LocalizeNoPrefix(Messages.scrWikiSearch);
		}
	}

	protected override SearchType Type
	{
		get
		{
			return SearchType.Wiki;
		}
	}

	protected override bool PerformSearch(string query, bool isQuick)
	{
		DateTime start = DateTime.Now;
		query = query == string.Empty ? null : query;
		PXArticleSearch search = new PXArticleSearch(this.Request["wikiid"], this.Request["categoryID"], this.Request["productID"], this.Request["orderID"]);

		search.IsQuick = isQuick;
		search.LinkFormat = ArticleUrl + "?PageID={0}";
		search.IgnoreCache = Search.CurrentPage == 0 ? true : false;

		List<PXSearchResult> articles = search.Search(query, Search.CurrentPage * 20, 20);
		
		if (articles.Count == 0)
		{
			Search.DisplaySearchTips(query, SearchTips);
			return false;
		}

		int i = Search.CurrentPage * 20;
		foreach (PXArticleSearch.Result res in articles)
		{
			if (PXSiteMap.IsPortal)
			{
				Search.DisplayStar(res, res.Rating, res.Views, res.Publisheddate, res.Orderid, null);
			}
			else
			{
				Search.DisplayResult(res);
			}
			i++;
		}

		TimeSpan span = DateTime.Now.Subtract(start);
		Search.DisplayResult(PXMessages.LocalizeFormatNoPrefix(Messages.SearchResults, Search.CurrentPage * 20 + 1,
			i, search.TotalCount, HttpUtility.HtmlEncode(query), this.Search.FormatTimeResult(span)));
		Search.PagesCount = search.TotalCount % 20 == 0 ? search.TotalCount / 20 : search.TotalCount / 20 + 1;
		return true;
	}

	protected override void AcceptInput()
	{
		var url = "query=" + HttpUtility.UrlEncode(TrimLongString(Search.Text)) + "&wikiid=" + this.Search.WikiID + "&wikinumber=" + this.Search.WikiNumber + "&categoryID=" + this.Search.CategoryID + "&productID=" + this.Search.ProductID + "&orderID=" + this.Search.OrderID + "&adv=1";
		Response.Redirect(PX.Common.PXUrl.ToAbsoluteUrl(Request.AppRelativeCurrentExecutionFilePath) + '?' + url, false);
	}
}