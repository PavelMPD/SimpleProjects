<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AP401000.aspx.cs" Inherits="Page_AP401000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="true" TypeName="PX.Objects.AP.APVendorBalanceEnq" 
        PrimaryView="Filter" style="float:left">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="PreviousPeriod" StartNewGroup="True" HideText="True"/>
            <px:PXDSCallbackCommand Name="NextPeriod" HideText="True"/>
            <px:PXDSCallbackCommand DependOnGrid="grid" Name="ViewDetails" Visible="false" />
            <px:PXDSCallbackCommand Name="APBalanceByVendorReport" Visible="False" DependOnGrid="grid"/>
            <px:PXDSCallbackCommand Name="VendorHistoryReport" Visible="False" DependOnGrid="grid"/>
            <px:PXDSCallbackCommand Name="APAgedPastDueReport" Visible="False" DependOnGrid="grid"/>
            <px:PXDSCallbackCommand Name="APAgedOutstandingReport" Visible="False" DependOnGrid="grid"/>
        </CallbackCommands>
    </px:PXDataSource>
    <px:PXToolBar ID="toolbar1" runat="server" SkinID="Navigation" BackColor="Transparent">
        <Items>
            <px:PXToolBarButton Text="Reports" Tooltip="Reports">
                <MenuItems>
                    <px:PXMenuItem Text="AP Balance by Vendor" CommandSourceID="ds" CommandName="APBalanceByVendorReport"/>
                    <px:PXMenuItem Text="Vendor History" CommandSourceID="ds" CommandName="VendorHistoryReport"/>
                    <px:PXMenuItem Text="AP Aged Past Due" CommandSourceID="ds" CommandName="APAgedPastDueReport"/>
                    <px:PXMenuItem Text="AP Aged Outstanding" CommandSourceID="ds" CommandName="APAgedOutstandingReport"/>
                </MenuItems>
            </px:PXToolBarButton>
        </Items>
        <Layout ItemsAlign="Left" />
    </px:PXToolBar>
    <div style="clear: left" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection" DefaultControlID="edFinPeriodID">
		<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			<px:PXSegmentMask CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID" />
			<px:PXSelector CommitChanges="True" ID="edVendorClassID" runat="server" DataField="VendorClassID" />
			<px:PXSelector CommitChanges="True" ID="edFinPeriodID" runat="server" DataField="FinPeriodID" />
			<px:PXSegmentMask CommitChanges="True" ID="edAccountID" runat="server" DataField="AccountID" />
			<px:PXSegmentMask CommitChanges="True" ID="edSubID" runat="server" DataField="SubID" SelectMode="Segment" />
			<px:PXSelector CommitChanges="True" ID="edCuryID" runat="server" DataField="CuryID" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
			<px:PXCheckBox CommitChanges="True" ID="chkSplitByCurrency" runat="server" DataField="SplitByCurrency" AlignLeft="True"  />
			<px:PXCheckBox CommitChanges="True" ID="chkShowWithBalanceOnly" runat="server" DataField="ShowWithBalanceOnly" AlignLeft="True"  />
			<px:PXCheckBox CommitChanges="True" ID="chkByFinancialPeriod" runat="server" DataField="ByFinancialPeriod" AlignLeft="True"  />			
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="S" />
			<px:PXNumberEdit ID="edBalanceSummary" runat="server" DataField="BalanceSummary" Enabled="False" />
			<px:PXNumberEdit ID="edDepositsSummary" runat="server" DataField="DepositsSummary" Enabled="False" />
			<px:PXNumberEdit ID="edCuryBalanceSummary" runat="server" DataField="CuryBalanceSummary" Enabled="False" />
			<px:PXNumberEdit ID="edCuryDepositsSummary" runat="server" DataField="CuryDepositsSummary" Enabled="False" />			
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="153px" Style="z-index: 100" Width="100%" Caption="Vendors" AllowSearch="True" AllowPaging="True" AdjustPageSize="Auto" SkinID="Inquire" MatrixMode="True"
		FastFilterFields="VendorID,AcctName" TabIndex="7900" RestrictFields="True">
		<Levels>
			<px:PXGridLevel DataMember="History">
				<RowTemplate>
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="AcctCD" Width="100px" LinkCommand="ViewDetails" />
					<px:PXGridColumn DataField="AcctName" Width="250px" />
					<px:PXGridColumn DataField="FinPeriodID" />
					<px:PXGridColumn DataField="CuryID" />
					<px:PXGridColumn DataField="CuryBegBalance" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="CuryEndBalance" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="CuryDepositsBalance" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="CuryPurchases" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="CuryPayments" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="CuryDiscount" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="CuryWhTax" TextAlign="Right" Width="81px" />					
					<px:PXGridColumn DataField="CuryCrAdjustments" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="CuryDrAdjustments" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="CuryDeposits" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="BegBalance" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="EndBalance" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="DepositsBalance" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="Purchases" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="Payments" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="Discount" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="WhTax" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="RGOL" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="CrAdjustments" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="DrAdjustments" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="Deposits" TextAlign="Right" Width="81px" />
				</Columns>
				<Layout FormViewHeight=""></Layout>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<ActionBar DefaultAction="cmdViewDetails">
			<CustomItems>
				<px:PXToolBarButton Text="Vendor Details" Key="cmdViewDetails">
				    <AutoCallBack Command="ViewDetails" Target="ds" />
				</px:PXToolBarButton>
			</CustomItems>
		</ActionBar>
	</px:PXGrid>
</asp:Content>
