using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using PX.Common;
using PX.Common.Mail;
using PX.Data;
using PX.Data.EP;
using PX.Data.Wiki.Parser;
using PX.Objects.CR;
using PX.SM;

namespace PX.Objects.EP
{
	public sealed class CommonMailSendProvider : IMailSendProvider
	{
		#region AttachmentCollection

		private class AttachmentCollection : IEnumerable<Attachment>
		{
			#region File

			private sealed class File
			{
				private readonly Guid _id;
				private readonly string _name;
				private readonly byte[] _data;

				private string _ext;

				public File(Guid id, string name, byte[] data)
				{
					_id = id;
					_name = name;
					_data = data;
				}

				public Guid Id
				{
					get { return _id; }
				}

				public string Name
				{
					get { return _name; }
				}

				public byte[] Data
				{
					get { return _data; }
				}

				public string Extension
				{
					get
					{
						return _ext ?? (_ext = MimeTypes.GetMimeType(Path.GetExtension(_name)));
					}
				}
			}

			#endregion

			#region Fields

			private readonly List<File> _items = new List<File>();
			private readonly PXGraph _graph;

			#endregion

			#region Ctors

			public AttachmentCollection(PXGraph graph)
			{
				_graph = graph;
			}

			#endregion

			public void Add(Guid id, string name, byte[] data)
			{
				if (_items.Any(e => e.Id == id))
				{
					return;
				}

				_items.Add(new File(id, name, data));
			}

			public void Add(Guid id)
			{
				if (_items.Any(e => e.Id == id))
				{
					return;
				}

				var f = ReadFile(id);
				if (f == null) return;

				var filename = f.Name.Split('\\');
				var name = filename[filename.Length - 1].Replace('/', '_').Replace('\\', '_');
				var a = new File(id, name, f.Data);

				_items.Add(a);
			}

			public static string CreateLink(Guid id)
			{
				return "cid:" + id;
			}

			public bool ResizeImage(Guid id, int width, int height)
			{
				var res = false;
				foreach (File att in _items)
					if (att.Id == id)
					{
						var result = Drawing.ScaleImageFromBytes(att.Data, width, height);
						if (result != null)
						{
							var replacer = new File(att.Id, att.Name, result);
							_items.Remove(att);
							_items.Add(replacer);
							res = true;
						}
						break;
					}
				return res;
			}

			private UploadFile ReadFile(Guid? id)
			{
				var result = (PXResult<UploadFile, UploadFileRevision>)
					PXSelectJoin<UploadFile,
					InnerJoin<UploadFileRevision,
						On<UploadFile.fileID, Equal<UploadFileRevision.fileID>,
							And<UploadFile.lastRevisionID, Equal<UploadFileRevision.fileRevisionID>>>>,
					Where<UploadFile.fileID, Equal<Required<UploadFile.fileID>>>>.
					Select(_graph, id);
				if (result == null) return null;
				var file = (UploadFile)result[typeof(UploadFile)];
				var revision = (UploadFileRevision)result[typeof(UploadFileRevision)];
				if (file != null && revision != null)
					file.Data = revision.Data;
				return file;
			}

			public IEnumerator<Attachment> GetEnumerator()
			{
				foreach (File item in _items)
					yield return
						new System.Net.Mail.Attachment(new MemoryStream(item.Data),
									   item.Name, item.Extension) { ContentId = item.Id.ToString() };
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		#endregion

		#region Message

		private sealed class Message
		{
			private readonly string _from;
			private readonly string _to;
			private readonly string _subject;

			public Message(string @from, string to, string subject)
			{
				if (string.IsNullOrEmpty(from)) throw new ArgumentNullException("from");
				if (string.IsNullOrEmpty(to)) throw new ArgumentNullException("to");

				_from = from;
				_to = to;
				_subject = subject;
			}

			public string From
			{
				get { return _from; }
			}

			public string To
			{
				get { return _to; }
			}

			public string Reply { get; set; }

			public string Cc { get; set; }

			public string Bcc { get; set; }

			public string Subject
			{
				get { return _subject; }
			}

			public string Body { get; set; }

			public IEnumerable<Attachment> Files { get; set; }

			public bool Html { get; set; }

			public string UID { get; set; }
		}

		#endregion

		#region ImageExtractor

		public sealed class ImageExtractor
		{
			private static readonly Regex _exp = new Regex("([\"'])((.*?[ \t\n\f\r]+?data:)|(data:)).*?(?<encoding>;base64)?,(?<data>[^\\1]*?)\\1",
				RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

			public bool Extract(string content, out string newContent, out ICollection<ImageInfo> images)
			{
				if (content == null) throw new ArgumentNullException("content");

				images = new List<ImageInfo>();
				var sb = new StringBuilder();
				var currentIndex = 0;
				foreach (Match match in _exp.Matches(content))
				{
					byte[] data;
					if (match.Groups["encoding"].Value == ";base64")
						try
						{
							data = Convert.FromBase64String(match.Groups["data"].Value);
						}
						catch
						{
							break;
						}

					else
						try
						{
							data = Encoding.ASCII.GetBytes(match.Groups["data"].Value);
						}
						catch
						{
							break;
						}

					var img = new ImageInfo(Guid.NewGuid(), data);
					images.Add(img);
					sb.Append(content, currentIndex, match.Index - currentIndex);
					currentIndex = match.Index + match.Length;
					sb.Append("\"cid:");
					sb.Append(img.CID);
					sb.Append("\"");
				}
				if (currentIndex < content.Length - 1)
					sb.Append(content, currentIndex, content.Length - currentIndex);
				newContent = sb.ToString();
				return images.Count > 0;
			}
		}

		#endregion

		#region ImageInfo

		public sealed class ImageInfo
		{
			private readonly Guid _id;
			private readonly string _cid;
			private readonly byte[] _bytes;

			public ImageInfo(Guid id, byte[] bytes)
			{
				_id = id;
				_cid = id.ToString();
				_bytes = bytes;
			}

			public Guid ID
			{
				get { return _id; }
			}

			public string CID
			{
				get { return _cid; }
			}

			public byte[] Bytes
			{
				get { return _bytes; }
			}
		}

		#endregion

		#region MessageProcessor

		private class MessageProcessor
		{
			#region Constants

			private const string _FILEID_REGEX_GROUP = "fileid";
			private const string _SRC_REGEX_GROUP = "src";
			private const string _SRC_ATT_PREFIX = "src=\"";
			private const string _SRC_ATT_POSTFIX = "\"";

			private static readonly Regex _imagesRegex =
				new Regex("<img [^<>]*" + _SRC_ATT_PREFIX + "(?<" + _SRC_REGEX_GROUP + ">[^<>\"]*?)/getfile.ashx" +
						"\\?([^<>\"]*&)*fileid=(?<" + _FILEID_REGEX_GROUP + ">[^\\.<>\"&]*?)(\\.[^<>&\"]{1,}){0,1}(&[^<>\"]*)*" + _SRC_ATT_POSTFIX + "[^<>]*>",
						RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);

			#endregion

			private readonly PXGraph _graph;			
			private EMailAccount _account;
			private Common.Mail.MailSender _mailer;
			
			public MessageProcessor(int? accountID)
			{
				_graph = new PXGraph();
				_graph.SelectTimeStamp();
				ReadAccount(accountID);
				PXSelect<PreferencesEmail>.Clear(_graph);				
			}

			public void ProcessAll()
			{
				foreach (EPActivity message in
				PXSelect<EPActivity,
					Where<EPActivity.mailAccountID, Equal<Required<EPActivity.mailAccountID>>,
						And2<
							Where<EPActivity.classID, Equal<CRActivityClass.email>,
								Or<EPActivity.classID, Equal<CRActivityClass.emailRouting>>>,
						And<EPActivity.isIncome, NotEqual<True>,
						And<EPActivity.mpstatus, Equal<MailStatusListAttribute.preProcess>>>>>>.
				SelectWindowed(_graph, 0, _account.SendGroupMails.GetValueOrDefault(), _account.EmailAccountID))
				{
					Process(message);
				}
			}

			public void Process(EPActivity message)
			{				
				using (PXTransactionScope sc = new PXTransactionScope())
				{
					try
					{
						PreProcessMessage(message);
					}
					catch (Exception)
					{
						//Unable to change status - leave message alone.
						return;
					}
					try
					{
						ProcessMessage(message);
						PostProcessMessage(message);
					}
					catch (Exception ex)
					{
						this._graph.Clear();
						if (message == null || message.TaskID < 0) return;

						message = PXSelect<EPActivity, Where<EPActivity.taskID, Equal<Required<EPActivity.taskID>>>>.SelectWindowed(_graph, 0, 1, message.TaskID);
						if (message != null)
						{
							message = (EPActivity)_graph.Caches[message.GetType()].CreateCopy(message);
							message.Exception = ex.Message;
							message.MPStatus = MailStatusListAttribute.Failed;
							UpdateMessage(message);							
						}
					}
					sc.Complete();
				}				
			}

			private void ReadAccount(int? accountID)
			{
				int? smtpAccount = accountID;
				EMailAccount account = null;
				if (smtpAccount != null)
					account = ReadAccountSettings(smtpAccount);
				if (account == null)
					account = ReadDefaultAccountSettings();
				if (account == null)
					throw new PXException(ErrorMessages.EmailNotConfigured);
				if (string.IsNullOrEmpty(account.Address.With(_ => _.Trim())))
					throw new PXException(Messages.EmptyEmailAccountAddress);

				var mailer = MailAccountManager.GetSender(account);
				if (mailer == null)
					throw new PXException(ErrorMessages.EmailNotConfigured);

				_account = account;
				_mailer = mailer;
			}

			private void PreProcessMessage(EPActivity message)
			{
				message = (EPActivity)_graph.Caches[message.GetType()].CreateCopy(message);

				message.MPStatus = MailStatusListAttribute.InProcess;
				message.Exception = null;

				UpdateMessage(message);
			}

			private void PostProcessMessage(EPActivity message)
			{
				if (message.Exception == null)
				{
					message.MPStatus = MailStatusListAttribute.Processed;
					message.UIStatus = ActivityStatusAttribute.Completed;
				}
				else if (message.Exception.StartsWith("4") && message.RetryCount < 3)
					{
						message.MPStatus = MailStatusListAttribute.PreProcess;
						message.UIStatus = ActivityStatusAttribute.Open;
						message.RetryCount += 1;
					}
				else
				{
					message.MPStatus = MailStatusListAttribute.Failed;
					message.UIStatus = ActivityStatusAttribute.Canceled;
				}
				
				UpdateMessage(message);
			}

			private void ProcessMessage(EPActivity message)
			{
				try
				{					
					SendMail(CreateMail(message));
					message.Exception = null;
				}
				catch (OutOfMemoryException) { throw; }
				catch (StackOverflowException) { throw; }
				catch (Exception e)
				{
					message.Exception = e.Message;
				}
			}

			private Message CreateMail(EPActivity message)
			{
				var correctFrom = GenerateFrom(message);
				var correctReply = GenerateReply(message);
				var subject = GenerateSubject(message);
				var fs = ReadAttachments(message);
				var content = ExtractInlineImages(message,fs);

				Message mail = new Message(correctFrom, message.MailTo, subject)
								{
									Reply = correctReply,
									Cc = message.MailCc,
									Bcc = message.MailBcc,
									Body = GenerateBody(content, fs),
									Html = message.Format == null || message.Format == EmailFormatListAttribute.Html,
									UID = message.MessageId,
									Files = fs
								};

				message.MailFrom = correctFrom;
				message.MailReply = correctReply;
				return mail;
			}

			private void UpdateMessage(EPActivity message)
			{
				var emailType = message.GetType();
				var cache = _graph.Caches[emailType];
				message = (EPActivity)cache.Update(message);
				_graph.EnshureCachePersistance(emailType);				
				var cached = _graph.Caches[message.GetType()].Locate(message);
				_graph.Persist();
				_graph.SelectTimeStamp();
				message = (EPActivity)cache.CreateCopy(cached);
			}

			private string ExtractInlineImages(EPActivity message, AttachmentCollection fs)
			{
				string res;
				ICollection<ImageInfo> files;
				if (message.Body != null && new ImageExtractor().Extract(message.Body, out res, out files))
				{
					foreach (ImageInfo imageInfo in files)
						fs.Add(imageInfo.ID, imageInfo.ID.ToString(), imageInfo.Bytes);
					return res;
				}
				return message.Body;
			}

			private void SendMail(Message mail)
			{
				var mailerReply = _account.With(_ => _.ReplyAddress).With(_ => _.Trim());
				var correctReply = string.IsNullOrEmpty(mailerReply) ? mail.Reply : mailerReply;

				var from = mail.From.With(_ => _.TrimEnd(';'));
				var reply = correctReply;
				var to = mail.To;
				var addressee = new Common.Mail.MailSender.MessageAddressee(to, reply, mail.Cc, mail.Bcc);
				var content = new Common.Mail.MailSender.MessageContent(mail.Subject, mail.Html, mail.Body);
				var msg = new Common.Mail.MailSender.MailMessageT(from, mail.UID, addressee, content);
				_mailer.Send(msg, mail.Files.ToArray());
			}

			private EMailAccount ReadDefaultAccountSettings()
			{
				PXSelect<PreferencesEmail>.Clear(_graph);
				var defAddress = ((PreferencesEmail)PXSelect<PreferencesEmail>.SelectWindowed(_graph, 0, 1)).With(_ => _.DefaultEMailAccountID);
				return ReadAccountSettings(defAddress);
			}

			private EMailAccount ReadAccountSettings(int? accountId)
			{
				if (accountId == null) return null;

				PXSelect<EMailAccount,
					Where<EMailAccount.emailAccountID, Equal<Required<EMailAccount.emailAccountID>>>>.
					Clear(_graph);

				var account = (EMailAccount)PXSelect<EMailAccount,
					Where<EMailAccount.emailAccountID, Equal<Required<EMailAccount.emailAccountID>>>>.
					Select(_graph, accountId);

				return account;
			}

			private string GenerateReply(EPActivity message)
			{
				var defaultAddress = _account.ReplyAddress.With(_ => _.Trim());
				if (string.IsNullOrEmpty(defaultAddress))
					defaultAddress = _account.Address.With(_ => _.Trim());
				return GenerateBackAddress(message, null, defaultAddress, true);
			}

			private string GenerateFrom(EPActivity message)
			{
				var defaultAddress = _account.Address.With(_ => _.Trim());
				var defaultDisplayName = _account.Description.With(_ => _.Trim());

				return GenerateBackAddress(message, defaultDisplayName, defaultAddress, true);
			}

			private string GenerateBackAddress(EPActivity message, string defaultDisplayName, string defaultAddress, bool suspendCustomAddress)
			{
				if (message.Owner == null)
				{
					return string.IsNullOrEmpty(defaultDisplayName)
					? defaultAddress
					: Mailbox.Create(defaultDisplayName, defaultAddress);
				}

				var records = PXSelectJoin<Users, 
					LeftJoin<EPEmployee, On<EPEmployee.userID, Equal<Users.pKID>>, 
					LeftJoin<Contact, On<Contact.contactID, Equal<EPEmployee.defContactID>>>>, 
					Where<Users.pKID, Equal<Required<Users.pKID>>>>.
					SelectWindowed(_graph, 0, 1, message.Owner);
				if (records == null || records.Count == 0) return defaultAddress;
				
				var row = records[0];
				var employee = (Contact)row[typeof(Contact)];
				var user = (Users)row[typeof(Users)];
				string displayName = null;
				string address = defaultAddress;
				if (user != null && user.PKID != null)
				{
					var userDisplayName = user.FullName.With(_ => _.Trim());
					if (!string.IsNullOrEmpty(userDisplayName))
						displayName = userDisplayName;
					var userAddress = user.Email.With(_ => _.Trim());
					if (!suspendCustomAddress && !string.IsNullOrEmpty(userAddress))
						address = userAddress;
				}
				if (employee != null && employee.BAccountID != null)
				{
					var employeeDisplayName = employee.DisplayName.With(_ => _.Trim());
					if (!string.IsNullOrEmpty(employeeDisplayName))
						displayName = employeeDisplayName;
					var employeeAddress = employee.EMail.With(_ => _.Trim());
					if (!suspendCustomAddress && !string.IsNullOrEmpty(employeeAddress))
						address = employeeAddress;
				}
				return string.IsNullOrEmpty(displayName)
					? address
					: Mailbox.Create(displayName, address).ToString();
			}

			private string GenerateBody(string content, AttachmentCollection fs)
			{
				var body = content ?? string.Empty;
				var images = _imagesRegex.Matches(body);

			    if (images.Count > 0)
				{
					var sb = new StringBuilder();
					var currentIndex = 0;
					foreach (Match match in images)
					{
						var fileid = match.Groups[_FILEID_REGEX_GROUP];
						var src = match.Groups[_SRC_REGEX_GROUP];
						Guid imgId;
						if (GUID.TryParse(fileid.Value, out imgId))
						{
							fs.Add(imgId);
							sb.Append(body.Substring(currentIndex, src.Index - currentIndex));
							sb.Append(AttachmentCollection.CreateLink(imgId));
							sb.Append(_SRC_ATT_POSTFIX);
							currentIndex = body.IndexOf(_SRC_ATT_POSTFIX, fileid.Index + fileid.Length) + _SRC_ATT_POSTFIX.Length;
						}
						else
						{
							var newIndex = src.Index + src.Length + _SRC_ATT_POSTFIX.Length;
							sb.Append(body.Substring(currentIndex, newIndex - currentIndex));
							currentIndex = newIndex;
						}
					}
					if (currentIndex < body.Length - 1)
						sb.Append(body.Substring(currentIndex));
					body = sb.ToString();
				}
				return body;
			}

			private string GenerateSubject(EPActivity message)
			{
				var subject = message.Subject;
                if (message.Ticket == null) message.Ticket = message.TaskID;
			    if (message.Ticket != null)
			    {
			        var ticket = EncodeTicket((int) message.TaskID);
			        subject = message.Subject.StartsWith("FW: ") || message.Subject.StartsWith("RE: ")
			                      ? message.Subject.Substring(0, 4) + message.Subject.Substring(4)
			                      : message.Subject;
			        subject += " " + ticket;
			    }
			    return subject;
			}

			private string EncodeTicket(int taskId)
			{
			    string emailTagPrefix = "[";
			    string emailTagSuffix = "]";
                foreach (PreferencesEmail curacc in PXSelect<PreferencesEmail>.Select(_graph))
                {
                    if (curacc.DefaultEMailAccountID != null)
                    {
                        emailTagPrefix = curacc.EmailTagPrefix;
                        emailTagSuffix = curacc.EmailTagSuffix;
                    }
                }
				return emailTagPrefix + taskId + emailTagSuffix;
			}

			private AttachmentCollection ReadAttachments(EPActivity message)
			{
				var fs = new AttachmentCollection(_graph);

				foreach (NoteDoc notes in
					PXSelect<NoteDoc,
						Where<NoteDoc.fileID, IsNotNull,
							And<NoteDoc.noteID, Equal<Required<NoteDoc.noteID>>>>>.
						Select(_graph, message.NoteID))
				{
					fs.Add((Guid)notes.FileID);
				}

				var addFiles = PXSelect<DynamicAttachment,
					Where<DynamicAttachment.refNoteID, Equal<Required<DynamicAttachment.refNoteID>>>>.
					Select(_graph, message.NoteID);
				foreach (Guid fileId in DynamicAttachmentManager.Process(_graph, addFiles.Extract()))
				{
					fs.Add(fileId);
				}

				return fs;
			}
		}

		#endregion

		public void Send(int accountId)
		{
			var graph = new PXGraph();
			graph.SelectTimeStamp();

			if (MailAccountManager.IsMailProcessingOff) throw new PXException(Messages.MailProcessingIsTurnedOff);

			new MessageProcessor(accountId).ProcessAll();			
		}

		public void SendMessage(object message)
		{
			if (message == null) throw new ArgumentNullException("message");

			if (!(message is EPActivity))
			{
				var error = string.Format("Cann't process message '{0}'. Expected type is '{1}'",
					message.GetType().Name, typeof(EPActivity).Name);
				throw new ArgumentException(error, "message");
			}

			EPActivity activity = message as EPActivity;
			new MessageProcessor(activity.MailAccountID).Process(activity);
		}
	}
}
