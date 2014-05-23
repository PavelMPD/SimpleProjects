using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Web.UI;
using PX.Common;
using PX.Data;
using PX.SM;
using PX.Web.UI;

namespace PX.Data.Wiki.Parser
{
	/// <summary>
	/// Defines settings for all wiki pages in Pure project.
	/// </summary>
	public class PXWikiSettings
	{
		public PXWikiSettings(Page page)
			:this(page, null)
		{			
		}		

		public PXWikiSettings(Page page, WikiReader reader)
		{
			this.page = page;
			this.reader = reader ?? PXGraph.CreateInstance<WikiReader>();
		}

		private readonly Page page;
		private readonly WikiReader reader;		

		public PXDBContext Relative
		{
			get
			{
				if (relative == null)
				{
					PXSettings settings = new PXSettings();
					settings.NamedLinks = false;

					uint num;
					settings.EditLinkText = PXMessages.Localize(Messages.Edit, out num);
					settings.CloseLinkText = PXMessages.Localize(Messages.Close, out num);
					settings.DefaultStylesPath = page.ResolveUrl/**/("~/App_Themes/Wiki.css");
					settings.GetCSSUrl = page.ResolveUrl/**/("~/App_Themes/GetCSS.aspx");
					settings.ArticleShowUrl = page.ResolveUrl/**/("~/Wiki/ShowWiki.aspx");
					settings.GetFileUrl = page.ResolveUrl/**/("~/Frames/GetFile.ashx");
					settings.FileEditUrl = page.ResolveUrl/**/("~/Pages/SM/SM202510.aspx");
					settings.GetRSSUrl = page.ResolveUrl/**/("~/Frames/GetRSS.ashx");

					settings.HintImageUrl = PXImages.ResolveImageUrl("~/App_Themes/Default/Images/Wiki/info.png");
					settings.WarnImageUrl = PXImages.ResolveImageUrl("~/App_Themes/Default/Images/Wiki/Warn.png");
					settings.MagnifyImageUrl = PXImages.ResolveImageUrl("~/App_Themes/Default/Images/Wiki/magnify.png");
					settings.DefaultExtensionImage = PXImages.ResolveImageUrl("~/App_Themes/Default/Images/Wiki/binary.gif");
					settings.RSSImageUrl = PXImages.ResolveImageUrl("~/App_Themes/Default/Images/Wiki/rss.gif");
					settings.FilesDirectAccess = true;

					UploadFileMaintenance filesAccessor = PXGraph.CreateInstance<UploadFileMaintenance>();
					foreach (UploadAllowedFileTypes ext in filesAccessor.GetAllowedFileTypes())
						if (!string.IsNullOrEmpty(ext.IconUrl))
							settings.ExtensionsImages.Add(ext.FileExt.ToLower(), page.ResolveUrl/**/(ext.IconUrl));
					relative = new PXDBContext(settings, reader);
				}
				return relative;
			}
		}
		private PXDBContext relative;

		public PXDBContext Absolute
		{
			get
			{
				return new PXDBContext(GetAbsoluteSettings(), reader);
			}
		}

		public static ISettings GetAbsoluteSettings()
		{
			return new PXAbsoluteSettings();
		}
	}

	public sealed class PXAbsoluteSettings : ISettings
	{
		private readonly string _editLinkText;
		private readonly string _closeLinkText;
		private string _rootPath;
		private string _GetCSSUrl;
		private IDictionary<string, string> _processedExtImages;

		public PXAbsoluteSettings()
		{
			uint num;
            _editLinkText = Messages.Edit; //PXMessages.Localize(Messages.Edit, out num);
            _closeLinkText = Messages.Close; //PXMessages.Localize(Messages.Close, out num);
		}

		public bool NamedLinks
		{
			get { return false; }
		}

		public string EditLinkText
		{
			get { return _editLinkText; }
		}

		public string CloseLinkText
		{
			get { return _closeLinkText; }
		}

		public string HintImageUrl
		{
			get { return RootPath + "/App_Themes/Default/Images/Wiki//info.png"; }
		}

		public string WarnImageUrl
		{
			get { return RootPath + "/App_Themes/Default/Images/Wiki//Warn.png"; }
		}

		public bool IsHtml { get; set; }

		public string DefaultStylesPath
		{
			get
			{
				string path = PXExecutionContext.Current.Request.ApplicationPath;
				if (!string.IsNullOrEmpty(path))
					path = path.TrimEnd('/') + "/";
				string css = ConfigurationManager.AppSettings["DefaultWikiCSS"];
				if (!string.IsNullOrEmpty(css))
					css = css.TrimStart('/');
				return path + css;
				
			}
		}

		public string GetCSSUrl
		{
			get
			{
				if (String.IsNullOrEmpty(_GetCSSUrl))
					return RootPath + "/App_Themes/GetCSS.aspx";
				return _GetCSSUrl;
			}
			set { _GetCSSUrl = value; }
		}

		public string ArticleShowUrl
		{
			get { return RootPath + "/Wiki/ShowWiki.aspx"; }
		}

		public string GetFileUrl
		{
			get { return RootPath + "/Frames/GetFile.ashx"; }
		}

		public string FileEditUrl
		{
			get { return RootPath + "/Pages/SM/SM202510.aspx"; }
		}

		public string MagnifyImageUrl
		{
			get { return string.Empty; }
		}

		public bool FilesDirectAccess
		{
			get { return false; }
		}

		private string _playFlashIconUrl;
		public string PlayFlashIconUrl
		{
			get { return _playFlashIconUrl; }
			set { _playFlashIconUrl = value; }
		}

		public string GetRSSUrl
		{
			get { return RootPath + "/Frames/GetRSS.ashx"; }
		}

		public string RSSImageUrl
		{
			get { return RootPath + "/App_Themes/Default/Images/Wiki/rss.gif"; }
		}

		public string RootUrl
		{
			get { return RootPath; }
		}

		public string ExternalRootUrl
		{
			get { return RootPath; }
		}

		public string SearchLink
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public string SearchUnknownLink
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public string DefaultExtensionImage
		{
			get { return RootPath + "/App_Themes/Default/Images/Wiki/binary.gif"; }
		}

		public IDictionary<string, string> ExtensionsImages
		{
			get
			{
				if (_processedExtImages == null)
				{
					_processedExtImages = new Dictionary<string, string>();
					string rootPath = RootPath;
					UploadFileMaintenance filesAccessor = PXGraph.CreateInstance<UploadFileMaintenance>();
					foreach (UploadAllowedFileTypes ext in filesAccessor.GetAllowedFileTypes())
						if (!string.IsNullOrEmpty(ext.IconUrl))
							_processedExtImages.Add(ext.FileExt.ToLower(), rootPath + ext.IconUrl.Replace("~", "/"));
				}
				return _processedExtImages;
			}
		}

		private string RootPath
		{
			get
			{
				if (_rootPath == null)
				{
					PXExecutionContext.RequestInfo context = PX.Common.PXExecutionContext.Current.Request;
					_rootPath = string.Format("{0}://{1}{2}", 
						context.Scheme,
						context.Authority,
						context.ApplicationPath != "/" ?
						context.ApplicationPath :
						string.Empty);
				}
				return _rootPath;
			}
		}
	}
}


