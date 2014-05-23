<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM204002.aspx.cs" Inherits="Page_SM204002" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="EMailAccounts"
		TypeName="PX.SM.EMailAccountMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Save" PopupVisible="True" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="Cancel" />
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand Name="checkEMailAccount" PopupVisible="True" StartNewGroup="True"
				CommitChanges="true" />
			<px:PXDSCallbackCommand Name="Action" PopupVisible="True" />
			<px:PXDSCallbackCommand Name="sendAll" Visible="False" />
			<px:PXDSCallbackCommand Name="receiveAll" Visible="False" />
			<px:PXDSCallbackCommand Name="sendReceiveAll" Visible="False" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%"
		DataMember="EMailAccounts" NoteIndicator="True" FilesIndicator="True" LinkIndicator="True"
		NotifyIndicator="True" Caption="Email Account Summary" DefaultControlID="edDescription">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" /> 
			<px:PXSelector ID="edDescription" runat="server" DataField="EmailAccountID" NullText="<NEW>"
				TextMode="Search" DisplayMode="Text" AutoRefresh="True" FilterByAllFields="True" />		 
			<px:PXTextEdit ID="edDescriptionOld" runat="server" DataField="Description" />
			<px:PXTextEdit ID="edAddress" runat="server" DataField="Address" />
			<px:PXTextEdit ID="edReplyAddress" runat="server" DataField="ReplyAddress" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" DataSourceID="ds" Height="387px" Style="z-index: 100"
		Width="100%" DataMember="CurrentEMailAccounts" DefaultControlID="edDescription">
		<Items>
			<px:PXTabItem Text="Servers">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Server Information" />
					<px:PXDropDown CommitChanges="True" ID="edIncomingHostProtocol" runat="server" DataField="IncomingHostProtocol"
						AllowNull="False" />
					<px:PXTextEdit ID="edImapRootFolder" runat="server" DataField="ImapRootFolder" />
					<px:PXTextEdit ID="edIncomingHostName" runat="server" DataField="IncomingHostName" />
					<px:PXTextEdit ID="edOutcomingHostName" runat="server" DataField="OutcomingHostName" />
				    <px:PXTextEdit ID="edGroupMail" runat="server" DataField="SendGroupMails" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Logon Information" />
					<px:PXTextEdit ID="edLoginName" runat="server" DataField="LoginName" />
					<px:PXTextEdit ID="edPassword" runat="server" DataField="Password" /></Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Advanced Settings">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" StartGroup="True" GroupCaption="Security" SuppressLabel="True"/>
					<px:PXCheckBox CommitChanges="True" ID="chkOutcomingAuthenticationRequest"
						runat="server" DataField="OutcomingAuthenticationRequest" />
					<px:PXCheckBox CommitChanges="True" ID="chkOutcomingAuthenticationDifferent"
						runat="server" DataField="OutcomingAuthenticationDifferent" />
					<px:PXTextEdit ID="edOutcomingLoginName" runat="server" DataField="OutcomingLoginName" SuppressLabel="False"/>
					<px:PXTextEdit ID="edOutcomingPassword" runat="server" DataField="OutcomingPassword" SuppressLabel="False"/>
					<px:PXCheckBox CommitChanges="True" ID="chkValidateFrom" runat="server" DataField="ValidateFrom" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Server Port Numbers" ControlSize="S" LabelsWidth="XM" SuppressLabel="True"/>
					<px:PXNumberEdit ID="edIncomingPort" runat="server" AllowNull="False" DataField="IncomingPort" SuppressLabel="False"/>
					<px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkIncomingSSLRequest"	runat="server" DataField="IncomingSSLRequest"/>
					<px:PXNumberEdit ID="edOutcomingPort" runat="server" AllowNull="False" DataField="OutcomingPort" SuppressLabel="False"/>
					<px:PXDropDown ID="chkOutcomingSSLRequest" runat="server" DataField="OutcomingSSLRequest"  Text="Outcoming SSL Request" NullText="None" SuppressLabel="False"/>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Incoming Mail Processing">
				<Template>
					<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" SuppressLabel="True" />
				    <px:PXCheckBox ID="chkIncomingProcessing" runat="server" DataField="IncomingProcessing">
						<AutoCallBack Enabled="true" Command="Save" Target="tab" />
					</px:PXCheckBox>  
					<px:PXLayoutRule ID="PXLayoutRule5" runat="server" StartGroup="True" GroupCaption="Initial Processing" ControlSize="M" LabelsWidth="SM" SuppressLabel="True"/>
					<px:PXCheckBox ID="chkConfirmReceipt" runat="server" DataField="ConfirmReceipt">
						<AutoCallBack Enabled="true" Command="Save" Target="tab" />
					</px:PXCheckBox>
					<px:PXSelector ID="edConfirmReceiptTemplate" runat="server" PopulateOnDemand="True"
						DataField="ConfirmReceiptNotificationID" SuppressLabel="False" Size="L" DisplayMode="Text">
						<AutoCallBack Enabled="True" Command="Save" Target="tab" />
					</px:PXSelector> 
					<px:PXLayoutRule ID="PXLayoutRule6" runat="server" StartGroup="True" GroupCaption="Main Processing" ControlSize="M" LabelsWidth="SM" SuppressLabel="True"/>
					<px:PXCheckBox ID="PXCheckBox1" runat="server" DataField="CreateCase" />
					<px:PXCheckBox ID="PXCheckBox2" runat="server" DataField="CreateActivity" />
					<px:PXCheckBox ID="PXCheckBox3" runat="server" DataField="CreateLead" />
					<px:PXCheckBox ID="chkProcessUnassigned" runat="server" DataField="ProcessUnassigned">
						<AutoCallBack Enabled="true" Command="Save" Target="tab" />
					</px:PXCheckBox>
					<px:PXSelector ID="edRespnseTemplate" runat="server" PopulateOnDemand="True"
						DataField="ResponseNotificationID" SuppressLabel="False" Size="L" DisplayMode="Text">
						<AutoCallBack Enabled="True" Command="Save" Target="tab" />
					</px:PXSelector>
					<px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartGroup="True" GroupCaption="Final Processing" ControlSize="M" LabelsWidth="SM" SuppressLabel="True" Merge="True"/>
					<px:PXCheckBox ID="chkDeleteUnProcessed" runat="server" DataField="DeleteUnProcessed"
						Width="135px">
						<AutoCallBack Enabled="True" Command="Save" Target="tab" />
					</px:PXCheckBox>
					<px:PXDropDown ID="edTypeDelete" runat="server" DataField="TypeDelete" AllowNull="false"
						Size="XS" />
					<px:PXLabel ID="PXLabel3" runat="server" Text="processing" />
					<px:PXLayoutRule ID="PXLayoutRule3" runat="server" />
					<px:PXLayoutRule ID="PXLayoutRule4" runat="server" LabelsWidth="SM" ControlSize="M"
						SuppressLabel="True" />
					<px:PXCheckBox ID="chkAddUpInformation" runat="server" DataField="AddUpInformation" />
				</Template>
			</px:PXTabItem>	
			
			<px:PXTabItem Text="Content">
				<Template>
					<px:PXLayoutRule ID="PXLayoutRule5" runat="server" StartGroup="True" GroupCaption="Content" ControlSize="S" LabelsWidth="SM" SuppressLabel="True"/>
					<px:PXLayoutRule runat="server" Merge="True" />
					<px:PXNumberEdit ID="edMaxIncomingSize" runat="server" AllowNull="False" DataField="MaxIncomingSize"
						Text="1024" SuppressLabel="False"/>
					<px:PXLabel runat="server">KB</px:PXLabel>
					<px:PXLayoutRule runat="server"  ControlSize="S" LabelsWidth="M" SuppressLabel="True" />
					<px:PXCheckBox ID="chkIncomingDelSuccess" runat="server" DataField="IncomingDelSuccess" />
					<px:PXLabel ID="lblCommentAttach" runat="server" Text="Specify the extensions for allowed attachment types separating them with comma: .cvs, .jpg, .png, .xls"
						SuppressLabel="False"/>	 
					<px:PXTextEdit ID="edIncomingAttachmentType" runat="server" DataField="IncomingAttachmentType" MaxLength="100" Size="XXL" SuppressLabel="True" />
				</Template>
			</px:PXTabItem>	

		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="250" MinWidth="300" />
	</px:PXTab>
</asp:Content>
