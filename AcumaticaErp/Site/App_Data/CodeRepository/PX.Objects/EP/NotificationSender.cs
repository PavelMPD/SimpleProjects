using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Compilation;
using System.Xml;
using PX.Common;
using PX.Data;
using PX.Data.EP;
using PX.Data.Reports;
using PX.Data.Wiki.Parser;
using PX.Data.Wiki.Parser.Html;
using PX.Data.Wiki.Parser.Txt;
using PX.Objects.CR;
using PX.Objects.SM;
using PX.Reports;
using PX.Reports.Controls;
using PX.Reports.Data;
using PX.Reports.Mail;
using PX.SM;
using FileInfo = PX.SM.FileInfo;
using System.Net.Mail;
using Message = PX.Reports.Mail.Message;
using PX.Objects.CS;

namespace PX.Objects.EP
{
	#region NotificationProvider

	public class NotificationProvider : INotificationSender
	{
		public void Notify(int? accountId, string mailTo, string mailCc, string mailBcc, string subject, string body)
		{
			var sender = new NotificationGenerator
							{
								MailAccountId = accountId,
								To = mailTo,
								Cc = mailCc,
								Bcc = mailBcc,
								Subject = subject,
								Body = body
							};
			sender.Send();
		}
	}

	#endregion

	#region NotificationSender

	public class NotificationGenerator
	{
		#region Constants

		private static readonly Regex _HtmlRegex = new Regex(
			@"^.*?\<html(\s?[^\>]*?)\>.*\<body",
			RegexOptions.IgnoreCase | RegexOptions.Compiled);

		#endregion

		#region Fields

		private readonly List<Guid> _attachments = new List<Guid>();

		private readonly PXGraph _graph;
		private Guid? _owner;

		#endregion
		
		#region Ctors

		public NotificationGenerator()
			: this(new PXGraph())
		{
		}

		public NotificationGenerator(PXGraph graph)
		{
			_graph = graph;
		}

		#endregion

		#region Public Methods

		public string BodyFormat { get; set; }

		public string Body { get; set; }

		public string Subject { get; set; }

		public string From { get; set; }

		public string Bcc { get; set; }

		public string Cc { get; set; }

		public string To { get; set; }

		public int? MailAccountId { get; set; }

		public string Reply { get; set; }

		public long? RefNoteID { get; set; }

		public long? ParentRefNoteID { get; set; }

		public int? ParentTaskID { get; set; }

		public Guid? Owner
		{
			get { return _owner ?? EP.EmployeeMaint.GetCurrentEmployeeID(_graph); }
			set { _owner = value; }
		}

		public IEnumerable<NotificationRecipient> Watchers { get; set; }

		public long? AttachmentsID { get; set; }

		public bool IsSystem { get; set; }

		public void AddAttachment(string name, byte[] content)
		{
			if (name == null) throw new ArgumentNullException("name");
			if (content == null) throw new ArgumentNullException("content");

			var upload = PXGraph.CreateInstance<UploadFileMaintenance>();
			var file = new FileInfo(Guid.NewGuid() + @"\" + name, null, content);
			upload.SaveFile(file);
			if (file.UID == null) throw new Exception(string.Format("Cannot save file '{0}'" + name));
			_attachments.Add(file.UID.Value);
		}

		public IEnumerable<EPActivity> Send()
		{
			IEnumerable<EPActivity> messages = CreateMessages();
			EPActivity[] activities = messages as EPActivity[] ?? messages.ToArray();
			PersistMessages(activities);
			return activities;
		}

		#endregion

		#region Protected Methods

		protected PXGraph Graph
		{
			get { return _graph; }
		}

		protected void PersistMessages(IEnumerable<EPActivity> messages)
		{
			using (PXTransactionScope ts = new PXTransactionScope())
			{
				foreach (EPActivity item in messages)
				{
					var activityCache = Graph.Caches[item.GetType()];
					PXDBDefaultAttribute.SetSourceType<EPActivity.refNoteID>(activityCache, item, null);
					activityCache.PersistInserted(item);
				}
				Graph.Caches<CRActivityStatistics>().Persist(PXDBOperation.Insert);
				Graph.Caches<CRActivityStatistics>().Persist(PXDBOperation.Update);

				ts.Complete();
			}
		}

		protected IEnumerable<EPActivity> CreateMessages()
		{
			var result = new List<EPActivity>();
			EPActivity main = CreateMessage();
			if (Watchers != null && Watchers.Any())
			{
				bool isMainAddedd = false;
				foreach (NotificationRecipient n in Watchers)
				{
					EPActivity m;
					var format = PXNotificationFormatAttribute.ValidBodyFormat(n.Format);
					if (format == main.Format)
						m = main;
					else
					{
						BodyFormat = format;
						m = CreateMessage();						
					}
					if (n.Hidden == true)
						m.MailBcc += (!string.IsNullOrEmpty(m.MailBcc) ? ";" : string.Empty) + n.Email;
					else
					{
						if (string.IsNullOrEmpty(m.MailTo)) m.MailTo = n.Email;
						else m.MailCc += (!string.IsNullOrEmpty(m.MailCc) ? ";" : string.Empty) + n.Email;
					}
					if (m.TaskID != main.TaskID || !isMainAddedd) result.Add(m);
					isMainAddedd |= m.TaskID == main.TaskID;
				}
			}
			else
				result.Add(main);

			if(result.Count == 0)
				throw new PXException(Messages.MailToUndefined);

			for (int i = result.Count - 1; i >= 0; i--)
			{
				var item = result[i];
				var mail = item;
				if (string.IsNullOrEmpty(mail.MailTo))
				{
					if (!string.IsNullOrEmpty(mail.MailCc))
					{
						string list = mail.MailCc;
						mail.MailTo = GetFirstMail(ref list);
						if (mail.MailTo != null) mail.MailCc = list;
					}
					if (!string.IsNullOrEmpty(mail.MailBcc))
					{
						string list = mail.MailBcc;
						mail.MailTo = GetFirstMail(ref list);
						if (mail.MailTo != null) mail.MailBcc = list;
					}

					if (string.IsNullOrEmpty(mail.MailTo))
					{
						Graph.Caches[typeof(EPActivity)].Delete(item);
						result.RemoveAt(i);
						PXTrace.WriteInformation(Messages.MailToUndefined);
					}
				}
			}
			return result;
		}

		#endregion

		#region Private Methods

		private static List<string> SplitAddresses(string source)
		{
			var destination = new List<string>();
			if (!string.IsNullOrEmpty(source))
			{
				destination.AddRange(source.Split(';'));
				if (destination.Count == 0) destination.Add(source);
			}
			return destination;
		}

		protected virtual EPActivity CreateMessage()
		{
			var activityCache = Graph.Caches[typeof(EPActivity)];
			var act = PXCache<EPActivity>.CreateCopy((EPActivity)activityCache.Insert());
			act.ClassID = CRActivityClass.Email;
			act.Type = null;
			var accountId = MailAccountId ?? MailAccountManager.DefaultMailAccountID;
			act.MailAccountID = accountId;
			act.MailFrom = accountId.
				With(_ => (EMailAccount)PXSelect<EMailAccount,
					Where<EMailAccount.emailAccountID, Equal<Required<EMailAccount.emailAccountID>>>>.
				Select(_graph, _.Value)).
				With(_ => _.Address);
			act.MailTo = To;
			act.MailCc = UpdateMailTo(act, Cc);
			act.MailBcc = UpdateMailTo(act, Bcc);
			act.MailReply = string.IsNullOrEmpty(Reply) ? act.MailFrom : Reply;
			act.Subject = Subject;
			act.IsIncome = false;
			act.MPStatus = MailStatusListAttribute.PreProcess;
			act.Format = BodyFormat ?? EmailFormatListAttribute.Html;
			act.RefNoteID = RefNoteID;
			act.ParentRefNoteID = ParentRefNoteID;
			act.ParentTaskID = ParentTaskID;
			act.Owner = Owner;
			act.IsBillable = false;

			act.StartDate = PXTimeZoneInfo.Now;
			act.Subject = Subject;
			act.Body = BodyFormat == null || BodyFormat == EmailFormatListAttribute.Html
						? CreateHtmlBody(Body)
						: CreateTextBody(Body);

			act.IsSystem = IsSystem;
			if (_attachments.Count > 0)
				PXNoteAttribute.SetFileNotes(activityCache, act, _attachments.ToArray());

			act = (EPActivity)activityCache.Update(act);

			return act;
		}

		private static bool IsHtml(string text)
		{
			/*using (TextReader r = new System.IO.StringReader(text))
			{
				var xml = FlexibleXmlReader.Create(r);
				while (xml.Read())
				{
					if (xml.NodeType == XmlNodeType.Element && 
						string.Equals("html", xml.Name, StringComparison.OrdinalIgnoreCase))
					{
						return true;
					}
				}
			}
			return false;*/

			return !string.IsNullOrEmpty(text) && _HtmlRegex.IsMatch(text);
		}

		private static string CreateHtmlBody(string text)
		{
			return IsHtml(text) ? text : Tools.ConvertSimpleTextToHtml(text);
		}

		private static string CreateTextBody(string text)
		{
			return IsHtml(text) ? Tools.ConvertHtmlToSimpleText(text) : text;
		}

		private static string GetFirstMail(ref string addressList)
		{
			int index = addressList.IndexOf(';');
			if (index < 0)
			{
				string result = addressList;
				addressList = null;
				return result;
			}
			else
			{
				string result = addressList.Substring(0, index);
				addressList = addressList.Substring(index + 1);
				return result;
			}
		}

		private static string UpdateMailTo(EPActivity result, string addressList)
		{
			if (string.IsNullOrEmpty(result.MailTo) && !string.IsNullOrEmpty(addressList))
			{
				var address = SplitAddresses(addressList);
				if (address.Count != 0)
				{
					result.MailTo = address[0];
					address.RemoveAt(0);
					addressList = string.Join(";", address.ToArray());
				}
			}
			return addressList;
		}

		#endregion
	}

	#endregion

	#region TemplateNotificationSender

	public sealed class TemplateNotificationGenerator : NotificationGenerator
	{
		#region Fields

		private long? _refNoteId;
		private readonly object _entity;

		#endregion

		#region Ctors

		private TemplateNotificationGenerator(object row, string graphType = null)
			: base(InitializeGraph(graphType))
		{			
			_entity = row;
			if (row != null)
			{
				EntityHelper helper = new EntityHelper(this.Graph);
				Type rowType = row.GetType();
				if (this.Graph.PrimaryView != null)
				{
					Type primaryType = this.Graph.Views[this.Graph.PrimaryView].Cache.GetItemType();
					if (rowType.IsSubclassOf(primaryType))
						rowType = primaryType;
				}
				row = helper.GetEntityRow(rowType, helper.GetEntityKey(row.GetType(), row));
				this.Graph.Caches[rowType].Current = row;
			}
			CalculateRefNoteId();
		}

		private TemplateNotificationGenerator(object row, Notification t)
			: this(row, PXPageIndexingService.GetGraphTypeByScreenID(t.ScreenID))
		{
			InitializeTemplate(t);
		}

		public static TemplateNotificationGenerator Create(object row, int notificationId)
		{
			var maint = PXGraph.CreateInstance<SMNotificationMaint>();
			Notification notification = maint.PublishedNotification.
				Search<Notification.notificationID>(notificationId);
			if(notification == null)
				throw new PXException(Messages.NotificationTemplateNotFound);

			return new TemplateNotificationGenerator(row, notification);
		}

		public static TemplateNotificationGenerator Create(object row, string notificationCD = null)
		{
			Notification notification = null;
			if (notificationCD != null)
			{
				var maint = PXGraph.CreateInstance<SMNotificationMaint>();
				notification = maint.PublishedNotification.
					Search<Notification.name>(notificationCD);
				if (notification == null)
					throw new PXException(Messages.NotificationTemplateCDNotFound, notificationCD);
			}
			return notification == null ? 
				new TemplateNotificationGenerator(row):
				new TemplateNotificationGenerator(row, notification);
		}

		#endregion

		#region Public Methods

		public bool LinkToEntity { get; set; }

		#endregion

		#region Override Methods

		protected override EPActivity CreateMessage()
		{
			var message = base.CreateMessage();

			var updateRefNoteId = LinkToEntity && _refNoteId != null && (long)_refNoteId > 0;
			if (updateRefNoteId)
				message.RefNoteID = _refNoteId;

			if (_entity == null)
			{
				message.Body = Body;
				message.MailTo = To;
				message.MailCc = Cc;
				message.MailBcc = Bcc;
				message.Subject = Subject;
			}
			else 
			{
				var keys = GetKeys(_entity);				
				var entityType = EntityType;

				message.Body = PXTemplateContentParser.TemplateInstance.Process(Body, Graph, entityType, keys);
				message.MailTo = PXTemplateContentParser.Instance.Process(To, Graph, entityType, keys);
				message.MailCc = PXTemplateContentParser.Instance.Process(Cc, Graph, entityType, keys);
				message.MailBcc = PXTemplateContentParser.Instance.Process(Bcc, Graph, entityType, keys);
				message.Subject = PXTemplateContentParser.Instance.Process(Subject, Graph, entityType, keys);
			}

			return message;
		}

		private Type EntityType
		{
			get { return !string.IsNullOrEmpty(Graph.PrimaryView) ? Graph.Views[Graph.PrimaryView].Cache.GetItemType() : _entity.GetType(); }
		}

		#endregion

		#region Private Methods

		private object[] GetKeys(object row)
		{
			var keys = new List<object>();
			if (row != null)
			{
				var cache = Graph.Caches[row.GetType()];

				foreach (Type t in cache.BqlKeys)
					keys.Add(cache.GetValue(row, t.Name));
			}

			return keys.ToArray();
		}

		private void InitializeTemplate(Notification t)
		{
			MailAccountId = t.NFrom ?? MailAccountManager.DefaultMailAccountID;
			var mailFrom = (MailAccountId).
				With(_ => (EMailAccount)PXSelect<EMailAccount,
					Where<EMailAccount.emailAccountID, Equal<Required<EMailAccount.emailAccountID>>>>.
					Select(Graph, _.Value)).
				With(_ => _.Address);
			From = mailFrom;
			Reply = mailFrom;
			To = GenerateAddressTo(t, t.NTo);
			Cc = GenerateAddressTo(t, t.NCc);
			Bcc = GenerateAddressTo(t, t.NBcc);
			Subject = GenerateSubject(t, t.Subject);

			var content = GenerateContent(t);
			/*var style = GetStyle(t);
			Body = InjectStyle(content, style);*/
			Body = content;
			Subject = t.Subject;
		}

		private string GenerateContent(Notification notification)
		{
			if (_entity == null) return notification.Body;

			var keys = GetKeys(_entity, Graph.Caches[EntityType]);
			return PXTemplateContentParser.Instance.Process(notification.Body, Graph, EntityType, keys);
		}

		private string GenerateAddressTo(Notification notification, string field)
		{
			if (_entity == null) return notification.Body;
			var keys = GetKeys(_entity, Graph.Caches[EntityType]);
			return PXTemplateContentParser.Instance.Process(field, Graph, EntityType, keys);
		}

		private string GenerateSubject(Notification notification, string field)
		{
			if (_entity == null) return notification.Subject;

			var type = _entity.GetType();
			var keys = GetKeys(_entity, Graph.Caches[type]);
			return PXTemplateContentParser.Instance.Process(field, Graph, type, keys);
		}

		private static object[] GetKeys(object e, PXCache cache)
		{
			var keys = new List<object>();

			foreach (Type t in cache.BqlKeys)
				keys.Add(cache.GetValue(e, t.Name));

			return keys.ToArray();
		}

		private void CalculateRefNoteId()
		{
			_refNoteId = new EntityHelper(Graph).GetEntityNoteID(_entity);
		}

		private static PXGraph InitializeGraph(string graphType)
		{
			Type type = null;
			if (graphType != null)
			{
				type = BuildManager.GetType(graphType, false);
				if (type == null)
					throw new PXException(graphType + " type cannot be found.");
				if (!typeof(PXGraph).IsAssignableFrom(type))
					throw new PXException(graphType + " is not a graph subclass.");
			}

			return type == null ? new PXGraph() : (PXGraph)PXGraph.CreateInstance(type);
		}

		#endregion
	}

	#endregion

/*#if !AZURE
	[System.Security.Permissions.RegistryPermission(System.Security.Permissions.SecurityAction.Assert, Unrestricted = true)]
	[System.Security.Permissions.FileIOPermission(System.Security.Permissions.SecurityAction.Assert, Unrestricted = true)]
	[System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Assert, Unrestricted = true)]
#endif*/
	public class ReportNotificationGenerator
	{
		private readonly Report _report;
		private IDictionary<string, string> _parameters;

		public ReportNotificationGenerator(string reportId)
		{
			if (string.IsNullOrEmpty(reportId)) 
				throw new ArgumentNullException("reportId");

			_report = PXReportTools.LoadReport(reportId, null);

			if (_report == null) throw new ArgumentException(string.Format(Messages.ReportCannotBeFound, reportId), "reportId");
		}

		public int? MailAccountId { get; set; }

		public IDictionary<string, string> Parameters
		{
			get { return _parameters ?? (_parameters = new Dictionary<string, string>()); }
			set { _parameters = value; }
		}

		public IEnumerable<NotificationRecipient> AdditionalRecipents { get; set; }

		public string Format { get; set; }
		public Int32? NotificationID { get; set; }

		public IEnumerable<EPActivity> Send()
		{
			PXReportTools.InitReportParameters(_report, _parameters, SettingsProvider.Instance.Default);

			ReportNode reportNode = ReportProcessor.ProcessReport(_report);
			reportNode.SendMailMode = true;

			return SendMessages(MailAccountId, reportNode, Format, AdditionalRecipents, NotificationID);
		}

		public static IEnumerable<EPActivity> Send(string reportId, IDictionary<string, string> reportParams)
		{
			var sender = new ReportNotificationGenerator(reportId) { Parameters = reportParams };
			return sender.Send();
		}

		private static IEnumerable<EPActivity> SendMessages(int? accountId, ReportNode report, string format, IEnumerable<NotificationRecipient> additionalRecipents, Int32? TemplateID)
		{
			List<EPActivity> result = new List<EPActivity>();
			Exception ex = null;
			foreach (Message message in GetMessages(report))
			{
				try
				{
					result.AddRange(Send(accountId, message, format, additionalRecipents, TemplateID));
				}
				catch (Exception e)
				{
					PXTrace.WriteError(e);
					ex = e;
				}
			}
			if (ex != null) throw ex;
			return result;
		}

		private static IEnumerable<Message> GetMessages(ReportNode report)
		{
			{
				var mailSource = new Dictionary<GroupMessage, PX.Reports.Mail.Message>();
				foreach (GroupNode group in report.Groups)
				{
					GroupMessage message = group.MailSettings;

					if (group.MailSettings != null &&
						group.MailSettings.ShouldSerialize() &&
						!mailSource.ContainsKey(message))
					{
						mailSource.Add(message, new PX.Reports.Mail.Message(message, report, message));
					}
				}
				return mailSource.Values;
			}
		}

		private static IEnumerable<EPActivity> Send(int? accountId, Message message, string format, IEnumerable<NotificationRecipient> additionalRecipients, Int32? TemplateID)
		{
			var graph = new PXGraph();

			var activitySource = message.Relationship.ActivitySource;
			long? refNoteID = GetEntityNoteID(graph, activitySource);

			var parentSource = message.Relationship.ParentSource;
			long? parentRefNoteID = GetEntityNoteID(graph, parentSource);

			NotificationGenerator sender = null;
			if (TemplateID != null)
			{
				sender = TemplateNotificationGenerator.Create(activitySource, (int)TemplateID);
			}
			else if (!string.IsNullOrEmpty(message.TemplateID))
			{
				sender = TemplateNotificationGenerator.Create(activitySource, message.TemplateID);
			}
			if (sender == null)
				sender = new NotificationGenerator(graph);
			
			sender.Body = string.IsNullOrEmpty(sender.Body) ? message.Content.Body : sender.Body;
			sender.Subject = string.IsNullOrEmpty(sender.Subject) ? message.Content.Subject : sender.Subject;
			sender.MailAccountId = accountId;
			sender.Reply = accountId.
				With(_ => (EMailAccount)PXSelect<EMailAccount,
					Where<EMailAccount.emailAccountID, Equal<Required<EMailAccount.emailAccountID>>>>.
				Select(graph, _.Value)).
				With(_ => _.Address);
			sender.To = message.Addressee.To;
			sender.Cc = message.Addressee.Cc;
			sender.Bcc = message.Addressee.Bcc;
			sender.RefNoteID = refNoteID;
			sender.ParentRefNoteID = parentRefNoteID;
			sender.BodyFormat = PXNotificationFormatAttribute.ValidBodyFormat(format);

		    List<NotificationRecipient> watchers = new List<NotificationRecipient>();
            if (sender.Watchers != null)
                watchers.AddRange(sender.Watchers);
            if (additionalRecipients != null)
                watchers.AddRange(additionalRecipients);
            sender.Watchers = watchers;
			
			foreach (ReportStream attachment in message.Attachments)
				sender.AddAttachment(attachment.Name, attachment.GetBytes());

			return sender.Send();
		}

		private static long? GetEntityNoteID(PXGraph graph, object row)
		{
			var helper = new EntityHelper(graph);
			long? refNoteId = null;
			if (row != null)
			{
				var cacheType = row.GetType();
				var graphType = helper.GetPrimaryGraphType(cacheType, row, false);
				if (graphType != null)
				{
					var primaryGraph = graph.GetType() != graphType
						? (PXGraph)PXGraph.CreateInstance(graphType)
						: graph;

					var primaryCache = primaryGraph.Caches[cacheType];
					refNoteId = PXNoteAttribute.GetNoteID(primaryCache,
						row, EntityHelper.GetNoteField(cacheType));
				}
			}
			return refNoteId;
		}

		public static IEnumerable<GroupMessage> GetWatchers(GroupMessage source, string defaultFormat, RecipientList watchers)
		{
			if (watchers != null)
			{
				GroupMessage msg = null;
				bool sourceAdded = false;

				if (defaultFormat != null)
				{
					var format = ConvertFormat(defaultFormat);

					msg = new GroupMessage(source.From, source.UID, source.Addressee,
						source.Content, source.TemplateID,
						format, source.Relationship, source.Report);
					sourceAdded = true;
				}

				//Redefine message format;
				foreach (NotificationRecipient n in watchers)
				{
					if (string.Compare(n.Email, source.Addressee.To, true) == 0)
					{
						var format = ConvertFormat(n.Format);
						msg = new GroupMessage(source.From, source.UID, source.Addressee,
							source.Content, source.TemplateID,
							format, source.Relationship, source.Report);
						sourceAdded = true;
						break;
					}
				}

				foreach (NotificationRecipient n in watchers)
				{
					string format = ConvertFormat(n.Format);
					if (msg != null && msg.Format != format)
					{
						yield return msg;
						msg = null;
					}

					if (msg == null)
					{
						if (format == source.Format)
						{
							msg = new GroupMessage(source);
							sourceAdded = true;
						}
						else
						{
							msg = new GroupMessage(source.From, source.UID, PX.Common.Mail.MailSender.MessageAddressee.Empty,
								source.Content, source.TemplateID,
								format, source.Relationship, source.Report);
						}
					}

					if (n.Hidden == true)
					{
						if (!string.IsNullOrEmpty(msg.Addressee.Bcc) && msg.Addressee.Bcc.Contains(n.Email)) continue;
						var bcc = (msg.Addressee.Bcc != null ? msg.Addressee.Bcc + ';' : string.Empty) + n.Email;
						var addresse = new PX.Common.Mail.MailSender.MessageAddressee(
							msg.Addressee.To, msg.Addressee.Reply, msg.Addressee.Cc, bcc);
						msg = new GroupMessage(msg.From, msg.UID, addresse,
							msg.Content, msg.TemplateID,
							msg.Format, msg.Relationship, msg.Report);
					}
					else
					{
						if (string.IsNullOrEmpty(msg.Addressee.To))
						{
							var addresse = new PX.Common.Mail.MailSender.MessageAddressee(
								n.Email, msg.Addressee.Reply, msg.Addressee.Cc, msg.Addressee.Bcc);
							msg = new GroupMessage(msg.From, msg.UID, addresse,
								msg.Content, msg.TemplateID,
								msg.Format, msg.Relationship, msg.Report);
						}
						else
						{
							if (msg.Addressee.To == n.Email) continue;
							if (!string.IsNullOrEmpty(msg.Addressee.Cc) && msg.Addressee.Cc.Contains(n.Email)) continue;

							var cc = (msg.Addressee.Cc != null ? msg.Addressee.Cc + ';' : string.Empty) + n.Email;

							var addresse = new PX.Common.Mail.MailSender.MessageAddressee(
								msg.Addressee.To, msg.Addressee.Reply, cc, msg.Addressee.Bcc);
							msg = new GroupMessage(msg.From, msg.UID, addresse,
								msg.Content, msg.TemplateID,
								msg.Format, msg.Relationship, msg.Report);
						}
					}
				}
				if (msg != null) yield return msg;

				if (!sourceAdded) yield return source;

				yield break;
			}
			yield return source;
		}

		private static string ConvertFormat(string notificationFormat)
		{
			switch (notificationFormat)
			{
				case NotificationFormat.PDF:
					return ReportProcessor.FilterPdf;
				case NotificationFormat.Excel:
					return ReportProcessor.FilterExcel;
				default:
					return ReportProcessor.FilterHtml;
			}
		}
	}

	public class RecipientList : IEnumerable<NotificationRecipient>
	{
		public RecipientList()
		{
			items = new SortedList<string, NotificationRecipient>();
		}

		public void Add(NotificationRecipient item)
		{
			string key = item.Format + '.' + items.Count;
			items.Add(key, item);
		}

		private readonly SortedList<string, NotificationRecipient> items;

		#region IEnumerable<NotificationRecipient> Members
		public IEnumerator<NotificationRecipient> GetEnumerator()
		{
			foreach (KeyValuePair<string, NotificationRecipient> item in items)
				yield return item.Value;
		}
		#endregion

		#region IEnumerable Members
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			foreach (KeyValuePair<string, NotificationRecipient> item in items)
				yield return item.Value;
		}
		#endregion
	}
}
