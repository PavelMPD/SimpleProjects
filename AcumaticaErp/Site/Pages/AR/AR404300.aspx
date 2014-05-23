<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AR404300.aspx.cs" Inherits="Page_AR404300" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.AR.ARStatementDetails" PrimaryView="Filter" PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="ViewDetails" Visible="False" />
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="PrintReport" Visible="False" />
			<px:PXDSCallbackCommand Name="prevStatementDate" StartNewGroup="true" HideText="True"/>
			<px:PXDSCallbackCommand Name="nextStatementDate" HideText="True"/>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection" DefaultControlID="edStatementCycleId">
		<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			<px:PXSelector CommitChanges="True" ID="edStatementCycleId" runat="server" DataField="StatementCycleId" />
			<px:PXDateTimeEdit CommitChanges="True" ID="edStatementDate" runat="server" DataField="StatementDate" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Caption="Details" AllowSearch="True" AllowPaging="True" AdjustPageSize="Auto" Height="150px" Style="z-index: 100" Width="100%" SkinID="Inquire" RestrictFields="True">
		<Levels>
			<px:PXGridLevel DataMember="Details">
				<RowTemplate>
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="CustomerID" Width="100px" AllowUpdate="False" SortDirection="Ascending" />
					<px:PXGridColumn DataField="CustomerID_BAccountR_acctName" Width="300px" />
					<px:PXGridColumn DataField="StatementBalance"  TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="OverdueBalance"  TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="CuryID" />
					<px:PXGridColumn DataField="CuryStatementBalance"  TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="CuryOverdueBalance"  TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="UseCurrency" TextAlign="Center" Type="CheckBox" Width="60px" />
					<px:PXGridColumn DataField="DontPrint" TextAlign="Center" Width="60px" Type="CheckBox" />
					<px:PXGridColumn DataField="Printed" TextAlign="Center" Width="60px" Type="CheckBox" />
					<px:PXGridColumn DataField="DontEmail" TextAlign="Center" Width="60px" Type="CheckBox" />
					<px:PXGridColumn DataField="Emailed" TextAlign="Center" Width="60px" Type="CheckBox" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<ActionBar DefaultAction="cmdViewDetails">
			<CustomItems>
				<px:PXToolBarButton Text="Customer Statement History" Key="cmdViewDetails">
				    <AutoCallBack Command="ViewDetails" Target="ds" />
				</px:PXToolBarButton>
				<px:PXToolBarButton Text="Print Statement" Key="cmdPrintReport">
				    <AutoCallBack Command="PrintReport" Target="ds" />
				</px:PXToolBarButton>
			</CustomItems>
		</ActionBar>
	</px:PXGrid>
</asp:Content>
