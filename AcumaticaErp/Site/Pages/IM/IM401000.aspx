<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="IM401000.aspx.cs" Inherits="Pages_IM_IM401000" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="Content1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Invoices" TypeName="InvoiceAddon.Controllers.InvoiceController" PageLoadBehavior="GoLastRecord">
		<CallbackCommands>
		    <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="Invoices" Caption="Invoice"  Style="z-index: 100">
		<Template>
		    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
		    <px:PXTextEdit ID="edCode" runat="server" DataField="Code" />
            <px:PXTextEdit ID="edName" runat="server" DataField="Name" />
            <px:PXTextEdit ID="edStatus" runat="server" DataField="Status" />
        </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="phG" Runat="Server">
    <px:PXGrid ID="gridInvoices" runat="server" DataSourceID="ds" Width="100%" Height="150px" Caption="All Invoices"
		SkinID="Invoices" Style="z-index: 100" SyncPosition="true">
		<Levels>
			<px:PXGridLevel DataMember="Invoices">
				<Columns>
					<px:PXGridColumn DataField="InvoceID" Width="50px" />
                    <px:PXGridColumn DataField="Code" Width="100px" />
                    <px:PXGridColumn DataField="Name" Width="250px" />
                    <px:PXGridColumn DataField="Status" Width="50px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXGrid>
</asp:Content>

