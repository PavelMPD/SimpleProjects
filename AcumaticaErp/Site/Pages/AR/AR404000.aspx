<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AR404000.aspx.cs" Inherits="Page_AR404000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.AR.ARStatementHistory" PrimaryView="Filter" PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="ViewDetails" Visible="false" />
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="PrintReport" Visible="false" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection" DefaultControlID="edStatementCycleId">
			<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			<px:PXSelector CommitChanges="True" ID="edStatementCycleId" runat="server" DataField="StatementCycleId" />
			<px:PXDateTimeEdit CommitChanges="True" ID="edStartDate" runat="server" DataField="StartDate" />
			<px:PXDateTimeEdit CommitChanges="True" ID="edEndDate" runat="server" DataField="EndDate" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Caption="Details" AllowSearch="True" AdjustPageSize="Auto" Height="150px" Style="z-index: 100" Width="100%" SkinID="Inquire" AllowPaging="True" RestrictFields="True">
		<Levels>
			<px:PXGridLevel DataMember="History">
				<RowTemplate>
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="StatementCycleId" Width="108px" SortDirection="Ascending" />
					<px:PXGridColumn DataField="Descr" Width="200px" />
					<px:PXGridColumn DataField="StatementDate" Width="108px" SortDirection="Descending" />
					<px:PXGridColumn DataField="NumberOfDocuments" TextAlign="Right" Width="120px" />
					<px:PXGridColumn DataField="ToPrintCount" TextAlign="Right" Width="60px" />
					<px:PXGridColumn DataField="PrintedCount" TextAlign="Right" Width="60px" />
					<px:PXGridColumn DataField="ToEmailCount" TextAlign="Right" Width="60px" />
					<px:PXGridColumn DataField="EmailedCount" TextAlign="Right" Width="60px" />
					<px:PXGridColumn DataField="NoActionCount" TextAlign="Right" Width="60px" />
					<px:PXGridColumn DataField="EmailCompletion" TextAlign="Right" Width="60px" />
					<px:PXGridColumn DataField="PrintCompletion" TextAlign="Right" Width="60px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<ActionBar DefaultAction="cmdViewDetails">
			<CustomItems>
				<px:PXToolBarButton Text="Statement Details" Key="cmdViewDetails">
				    <AutoCallBack Command="ViewDetails" Target="ds" />
				</px:PXToolBarButton>
				<px:PXToolBarButton Text="Print Statements" Key="cmdPrintReport">
				    <AutoCallBack Command="PrintReport" Target="ds" />
				</px:PXToolBarButton>
			</CustomItems>
		</ActionBar>
	</px:PXGrid>
</asp:Content>
