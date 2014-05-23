<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="GL509900.aspx.cs" Inherits="Page_GL509900" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" Visible="True" PrimaryView="LedgerList"
		TypeName="PX.Objects.GL.GLHistoryValidate">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="true" Name="Process" StartNewGroup="true" />
			<px:PXDSCallbackCommand CommitChanges="true" Name="ProcessAll" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" 
		AllowPaging="True" AllowSearch="true"  SkinID="Inquire" 
		Caption="Ledgers" FastFilterFields="LedgerCD,Descr">
		<Levels>
			<px:PXGridLevel DataMember="LedgerList" >
				<Columns>
					<px:PXGridColumn  DataField="Selected" TextAlign="Center" AllowCheckAll = "true" Type="CheckBox" Width="30px" />
					<px:PXGridColumn  DataField="LedgerCD"   Width="100px" />
					<px:PXGridColumn  DataField="Descr" Width="250px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
	</px:PXGrid>
</asp:Content>
