<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AR506500.aspx.cs" Inherits="Page_AR506500" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PageLoadBehavior="GoLastRecord" PrimaryView="Filter" TypeName="PX.Objects.AR.ARSPCommissionReview">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand Name="VoidCommissions" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="ClosePeriod" PostData="Self" />
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="ViewDetails" Visible="False" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Caption="Selection" Width="100%" DataMember="Filter" DefaultControlID="edCommnPeriodID">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
			<px:PXSelector ID="edCommnPeriodID" runat="server" DataField="CommnPeriodID" />
			<px:PXDropDown ID="edStatus" runat="server" DataField="Status" Enabled="False" SelectedIndex="1" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
			<px:PXDateTimeEdit ID="edStartDate" runat="server" DataField="StartDate" Enabled="False" />
			<px:PXDateTimeEdit ID="edEndDate" runat="server" DataField="EndDateUI" Enabled="False" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Caption="Salespersons' Commissions" Width="100%" SkinID="Inquire">
		<Levels>
			<px:PXGridLevel DataMember="ToProcess">
				<RowTemplate>
				</RowTemplate>
				<Columns>
					<px:PXGridColumn  DataField="SalesPersonID"  Width="100px" />
					<px:PXGridColumn  DataField="SalesPersonID_SalesPerson_descr" Width="250px" />
					<px:PXGridColumn   DataField="CommnblAmt" TextAlign="Right" Width="100px" />
					<px:PXGridColumn   DataField="CommnAmt" TextAlign="Right" Width="100px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<ActionBar DefaultAction="cmdViewDetails">
			<CustomItems>
				<px:PXToolBarButton Text="View Details" Key="cmdViewDetails">
				    <AutoCallBack Command="ViewDetails" Target="ds" />
				</px:PXToolBarButton>
			</CustomItems>
		</ActionBar>
	</px:PXGrid>
</asp:Content>
