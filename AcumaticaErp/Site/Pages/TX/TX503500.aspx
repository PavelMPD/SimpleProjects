<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="TX503500.aspx.cs" Inherits="Page_TX503000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" Visible="true" runat="server" TypeName="PX.Objects.TX.ProcessPendingVAT" PrimaryView="VendorSVAT">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Cancel" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="Process" CommitChanges="true" StartNewGroup="True" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" AllowPaging="True" AllowSearch="true" AdjustPageSize="Auto" DataSourceID="ds" ActionsPosition="Top" BatchUpdate="true" SkinID="Inquire" Caption="Transactions">
		<Levels>
			<px:PXGridLevel DataMember="VendorSVAT">
				<Columns>
					<px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" Width="60px" />
					<px:PXGridColumn DataField="TranType" />
					<px:PXGridColumn DataField="RefNbr" />
					<px:PXGridColumn DataField="TaxID" />
					<px:PXGridColumn DataField="TaxRate" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="CuryTaxableAmt" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="CuryTaxAmt" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="TaxInvoiceDate" Width="90px" />
					<px:PXGridColumn DataField="TaxInvoiceNbr" />
				</Columns>
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" />
					<px:PXDateTimeEdit SuppressLabel="True" ID="edTaxInvoiceDate" runat="server" DataField="TaxInvoiceDate" />
				</RowTemplate>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
		<ActionBar>
		</ActionBar>
	</px:PXGrid>
</asp:Content>
