<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="EP207000.aspx.cs" Inherits="Page_EP207000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="CompensationCodes"
		TypeName="PX.Objects.EP.EPCompCodeMaint" Visible="True">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100"
		AllowPaging="True" AllowSearch="true" AdjustPageSize="Auto" DataSourceID="ds"
		SkinID="Primary">
		<Levels>
			<px:PXGridLevel  DataMember="CompensationCodes">
				<Columns>
					<px:PXGridColumn DataField="CompensationCode" Label="Compensation Code" Width="140" />
					<px:PXGridColumn DataField="Description" Label="Description" Width="200px" />
					<px:PXGridColumn AllowNull="False" DataField="Type" Label="Type" RenderEditorText="True" Width="140px" />
					<px:PXGridColumn DataField="Rate" Label="Rate" TextAlign="Right" Width="100px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
	</px:PXGrid>
</asp:Content>
