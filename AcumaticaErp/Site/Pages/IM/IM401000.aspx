<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="IM401000.aspx.cs" Inherits="Pages_IM_IM401000" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="Content1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" AutoCallBack="True" Visible="True" Width="100%"
		TypeName="InvoiceAddon.Controllers.InvoiceController" PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
		    <px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="phF" Runat="Server">
    <%--<px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="Filter" Caption="Selection" Style="z-index: 100" Width="100%">
		<Template>
        </Template>
	</px:PXFormView>--%>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="phG" Runat="Server">
    <px:PXGrid ID="gridInvoices" runat="server" DataSourceID="ds" Width="100%" ActionsPosition="Top" Caption="Invoices"
		AllowPaging="true" AllowSearch="true" BlankFilterHeader="All Invoices"
		MatrixMode="true" SkinID="Inquire" AdjustPageSize="Auto" FastFilterFields="Code" RestrictFields="True">
		<Levels>
			<px:PXGridLevel DataMember="Invoices">
				<Columns>
					<px:PXGridColumn DataField="InvoceID" Width="50px" />
                    <px:PXGridColumn DataField="Code" Width="100px" />
                    <px:PXGridColumn DataField="Name" Width="250px" AllowNull="False" />
                    <px:PXGridColumn DataField="Status" Width="50px" AllowNull="False" />
                    
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXGrid>
</asp:Content>

