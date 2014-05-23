<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AR505000.aspx.cs" Inherits="Page_AR505000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.AR.ARCreateWriteOff" PrimaryView="Filter" PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Process" StartNewGroup="true" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="ProcessAll" CommitChanges="true" />
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="viewDocument" Visible="False" />
			<px:PXDSCallbackCommand Name="newCustomer" Visible="False" />
			<px:PXDSCallbackCommand Name="editCustomer" Visible="False" />
			<px:PXDSCallbackCommand Name="customerDocuments" Visible="False" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection" DefaultControlID="edWOType">
		<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
			<px:PXDropDown CommitChanges="True" ID="edWOType" runat="server" DataField="WOType" SelectedIndex="-1" />
			<px:PXDateTimeEdit CommitChanges="True" ID="edWODate" runat="server" DataField="WODate" />
			<px:PXSelector CommitChanges="True" ID="edWOFinPeriodID" runat="server" DataField="WOFinPeriodID" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
			<px:PXSegmentMask CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID" />
			<px:PXSegmentMask CommitChanges="True" ID="edCustomerID" runat="server" DataField="CustomerID" />
			<px:PXSelector CommitChanges="True" ID="PXSelector1" runat="server" DataField="ReasonCode" AutoRefresh="True" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
			<px:PXNumberEdit CommitChanges="True" ID="edWOLimit" runat="server" DataField="WOLimit" />
			<px:PXNumberEdit ID="edSelTotal" runat="server" DataField="SelTotal" Enabled="false" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="288px" Style="z-index: 100" Width="100%" Caption="Documents" AllowPaging="True" AdjustPageSize="Auto" SkinID="Inquire" TabIndex="3500">
		<Levels>
			<px:PXGridLevel DataMember="ARDocumentList">
				<RowTemplate>
					<px:PXSelector ID="RefNbr" runat="server" AllowEdit="True" DataField="RefNbr">
					</px:PXSelector>
					<px:PXSegmentMask ID="CustomerID" runat="server" AllowEdit="True" DataField="CustomerID">
					</px:PXSegmentMask>
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="Selected" Width="20px" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" AllowSort="False" AllowMove="False" AllowUpdate="False" AutoCallBack="True" />
					<px:PXGridColumn DataField="DocType" Type="DropDownList" Width="100px" />
					<px:PXGridColumn DataField="RefNbr" Width="100px" />
					<px:PXGridColumn DataField="CustomerID" Width="100px" AllowUpdate="False"/>
					<px:PXGridColumn DataField="CustomerID_BAccountR_acctName" Width="250px" />
					<px:PXGridColumn DataField="DocDate" Width="100px" />
					<px:PXGridColumn DataField="FinPeriodID" MaxLength="6" Width="81px" />
					<px:PXGridColumn AllowUpdate="False" DataField="DocBal" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="DocDesc" Width="250px" />
				</Columns>

<Layout FormViewHeight=""></Layout>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<ActionBar>
			<CustomItems>
				<px:PXToolBarButton Text="View Document" Tooltip="View Document" CommandName="viewDocument" CommandSourceID="ds" />
			</CustomItems>
		</ActionBar>
	</px:PXGrid>
</asp:Content>
