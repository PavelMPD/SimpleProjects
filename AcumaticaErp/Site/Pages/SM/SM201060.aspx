<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM201060.aspx.cs" Inherits="Page_SM201060"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Prefs"
		TypeName="PX.SM.PreferencesSecurityMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Caption="General Settings"
		Style="z-index: 100" Width="100%" DataMember="Prefs" TemplateContainer="">
		<AutoSize Container="Window" Enabled="True" MinHeight="500" />
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="L" ControlSize="XL"
				ColumnSpan="2" />
			<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Password Policy" />
			<px:PXLayoutRule runat="server" Merge="True" />
			<px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkisPasswordDayAge"
				runat="server" DataField="IsPasswordDayAge" AlignLeft="True" Size="L" />
			<px:PXNumberEdit Size="XS" ID="edPasswordDayAge" runat="server" DataField="PasswordDayAge"
				SuppressLabel="True" />
			<px:PXLabel Size="xs" ID="lblPasswordDayAge" runat="server" Text="Days" Height="18px"></px:PXLabel>
			<px:PXLayoutRule runat="server" />
			<px:PXLayoutRule runat="server" Merge="True" />
			<px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkIsPasswordMinLength"
				runat="server" DataField="IsPasswordMinLength" AlignLeft="True" Size="L" />
			<px:PXNumberEdit Size="XS" ID="edPasswordMinLength" runat="server" DataField="PasswordMinLength"
				SuppressLabel="True" />
			<px:PXLabel Size="xs" ID="lblPasswordMinLength" runat="server" Text="Characters"
				Height="18px"></px:PXLabel>
			<px:PXLayoutRule runat="server" />
			<px:PXCheckBox SuppressLabel="True" ID="chkPasswordComplexity" runat="server" DataField="PasswordComplexity"
				AlignLeft="True" />
			<px:PXTextEdit ID="edPasswordRegexCheck" runat="server" DataField="PasswordRegexCheck" />
			<px:PXTextEdit ID="edPasswordRegexCheckMessage" runat="server" DataField="PasswordRegexCheckMessage"
				TextMode="MultiLine"   />
			<px:PXLabel ID="PXHole" runat="server"></px:PXLabel>
			<px:PXLayoutRule runat="server" StartColumn="True" StartRow="True" 
				ControlSize="SM" LabelsWidth="M" />
			<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Account Lockout Policy" />
			<px:PXLayoutRule runat="server" Merge="True" />
			<px:PXNumberEdit Size="XS" ID="edAccountLockoutThreshold" runat="server" 
				DataField="AccountLockoutThreshold" />
			<px:PXLabel Size="m" ID="lblUnsuccess" runat="server" Text="Unsuccessful Login Attempts"></px:PXLabel>
			<px:PXLayoutRule runat="server" />
			<px:PXLayoutRule runat="server" Merge="True" />
			<px:PXNumberEdit Size="XS" ID="edAccountLockoutDuration" runat="server" 
				DataField="AccountLockoutDuration" />
			<px:PXLabel Size="xs" ID="lblLockMinutes" runat="server" Text="Minutes"></px:PXLabel>
			<px:PXLayoutRule runat="server" />
			<px:PXLayoutRule runat="server" Merge="True" />
			<px:PXNumberEdit Size="XS" ID="edAccountLockoutReset" runat="server" 
				DataField="AccountLockoutReset" />
			<px:PXLabel Size="xs" ID="lblResetMinutes" runat="server" Text="Minutes"></px:PXLabel>
			<px:PXLayoutRule runat="server" />
			<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Password Encryption" />
			<px:PXSelector ID="edDBCertificateName" runat="server" DataField="DBCertificateName"
				DataSourceID="ds" />
			<px:PXSelector ID="edPdfCertificateName" runat="server" DataField="PdfCertificateName"
				DataSourceID="ds" />
			<px:PXDropDown CommitChanges="True" ID="edPasswordSecurityType" runat="server" AllowNull="False"
				DataField="PasswordSecurityType" Size="SM" />
			<px:PXTextEdit ID="edPasswordSecurityForAdmin" runat="server" DataField="PasswordSecurityForAdmin" />
			<px:PXTextEdit ID="edPasswordSecurityForAll" runat="server" DataField="PasswordSecurityForAll" />
			<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" 
				LabelsWidth="M">
			</px:PXLayoutRule>
			<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Audit" />
			<px:PXLayoutRule runat="server" Merge="True" />
			<px:PXNumberEdit Size="XS" ID="edTraceMonthsKeep" runat="server" 
				DataField="TraceMonthsKeep" />
			<px:PXLabel Size="xxs" ID="lblMonth" runat="server" Text="Months"></px:PXLabel>
			<px:PXLayoutRule runat="server" />
			<px:PXPanel ID="PXPanel1" runat="server" RenderSimple="True" 
				RenderStyle="Simple">
				<px:PXLayoutRule runat="server" StartColumn="True" SuppressLabel="True">
				</px:PXLayoutRule>
				<px:PXCheckBox ID="chkTraceOperationLogin" runat="server" AlignLeft="True" 
					DataField="TraceOperationLogin" SuppressLabel="True">
				</px:PXCheckBox>
				<px:PXCheckBox ID="chkTraceOperationLoginFailed" runat="server" 
					AlignLeft="True" DataField="TraceOperationLoginFailed" SuppressLabel="True">
				</px:PXCheckBox>
				<px:PXCheckBox ID="chkTraceOperationLogout" runat="server" AlignLeft="True" 
					DataField="TraceOperationLogout" SuppressLabel="True">
				</px:PXCheckBox>
				<px:PXLayoutRule runat="server" StartColumn="True" SuppressLabel="True">
				</px:PXLayoutRule>
				<px:PXCheckBox ID="chkTraceOperationAccessScreen" runat="server" 
					AlignLeft="True" DataField="TraceOperationAccessScreen" SuppressLabel="True">
				</px:PXCheckBox>
				<px:PXCheckBox ID="chkTraceOperationSessionExpired" runat="server" 
					AlignLeft="True" DataField="TraceOperationSessionExpired" SuppressLabel="True">
				</px:PXCheckBox>
				<px:PXLayoutRule runat="server" StartColumn="True" SuppressLabel="True">
				</px:PXLayoutRule>
				<px:PXCheckBox ID="chkTraceSendMail" runat="server" AlignLeft="True" 
					DataField="TraceOperationSendMail" SuppressLabel="True">
				</px:PXCheckBox>
				<px:PXCheckBox ID="chkTraceOperationSendMailFailed" runat="server" 
					AlignLeft="True" DataField="TraceOperationSendMailFailed" SuppressLabel="True">
				</px:PXCheckBox>
			</px:PXPanel>
		</Template>
	</px:PXFormView>
</asp:Content>
