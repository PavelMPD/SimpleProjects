<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SP500000.aspx.cs" Inherits="Pages_NewComment"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="SP.Objects.SP.SPComment" PrimaryView="Comment">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="SaveClose" CommitChanges="True" Visible="True" ClosePopup="True"/>  
			<px:PXDSCallbackCommand Name="Close" Visible="True" ClosePopup="True"/>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="formview" runat="server" DataMember="Comment" FilesIndicator="True" NoteIndicator="False" Width="100%">
			<Template>
				<px:PXLayoutRule ID="PXLayoutRule5" runat="server" StartColumn="True" SuppressLabel="False" ControlSize="XXL"/>
				<px:PXTextEdit ID="edSubject" runat="server" DataField="Subject" Width="530"/>
				<px:PXTextEdit ID="edBody" runat="server" DataField="Body" TextMode="MultiLine" Width="650" SuppressLabel="True" style="resize: none">
				<AutoSize Enabled="True" Container="Window" />
				</px:PXTextEdit>
			</Template>
			<AutoSize Enabled="True" Container="Window" />
		</px:PXFormView> 
</asp:Content> 
