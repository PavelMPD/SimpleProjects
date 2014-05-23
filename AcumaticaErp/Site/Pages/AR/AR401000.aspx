<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AR401000.aspx.cs" Inherits="Page_AR401000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="true" TypeName="PX.Objects.AR.ARCustomerBalanceEnq" 
        PrimaryView="Filter" style="float:left">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="PreviousPeriod" StartNewGroup="True" HideText="True"/>
            <px:PXDSCallbackCommand Name="NextPeriod" HideText="True"/>
            <px:PXDSCallbackCommand DependOnGrid="grid" Name="ViewDetails" Visible="false" />
            <px:PXDSCallbackCommand DependOnGrid="grid" Name="ARBalanceByCustomerReport" Visible="false" />
            <px:PXDSCallbackCommand DependOnGrid="grid" Name="CustomerHistoryReport" Visible="false" />
            <px:PXDSCallbackCommand DependOnGrid="grid" Name="ARAgedPastDueReport" Visible="false" />
            <px:PXDSCallbackCommand DependOnGrid="grid" Name="ARAgedOutstandingReport" Visible="false" />
        </CallbackCommands>
    </px:PXDataSource>
    <px:PXToolBar ID="toolbar1" runat="server" SkinID="Navigation" BackColor="Transparent">
        <Items>
            <px:PXToolBarSeperator />
            <px:PXToolBarButton Text="Reports" Tooltip="Reports">
                <MenuItems>
                    <px:PXMenuItem Text="AR Balance by Customer" CommandSourceID="ds" CommandName="ARBalanceByCustomerReport"/>
                    <px:PXMenuItem Text="Customer History" CommandSourceID="ds" CommandName="CustomerHistoryReport"/>
                    <px:PXMenuItem Text="AR Aged Past Due" CommandSourceID="ds" CommandName="ARAgedPastDueReport"/>
                    <px:PXMenuItem Text="AR Aged Outstanding" CommandSourceID="ds" CommandName="ARAgedOutstandingReport"/>
                </MenuItems>
            </px:PXToolBarButton>
        </Items>
        <Layout ItemsAlign="Left" />
    </px:PXToolBar>
    <div style="clear: left" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection" DefaultControlID="edPeriod">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			<px:PXSegmentMask CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID" />
			<px:PXSelector CommitChanges="True" ID="edPeriod" runat="server" DataField="Period" />			
			<px:PXSelector CommitChanges="True" ID="edCustomerClassID" runat="server" DataField="CustomerClassID" />			
			<px:PXSegmentMask CommitChanges="True" ID="edARAcctID" runat="server" DataField="ARAcctID" />
			<px:PXSegmentMask CommitChanges="True" ID="edSubCD" runat="server" DataField="SubCD" SelectMode="Segment" />
			<px:PXSegmentMask CommitChanges="True" ID="edARSubID" runat="server" DataField="ARSubID" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
			<px:PXSelector CommitChanges="True" ID="edCuryID" runat="server" DataField="CuryID" />
			<px:PXCheckBox CommitChanges="True" ID="chkSplitByCurrency" runat="server" DataField="SplitByCurrency" AlignLeft="True" />
			<px:PXCheckBox CommitChanges="True" ID="chkShowWithBalanceOnly" runat="server" DataField="ShowWithBalanceOnly" AlignLeft="True" />
			<px:PXCheckBox CommitChanges="True" ID="chkByFinancialPeriod" runat="server" DataField="ByFinancialPeriod" AlignLeft="True"/>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
			<px:PXNumberEdit ID="edBalanceSummary" runat="server" DataField="BalanceSummary" Enabled="False" />
			<px:PXNumberEdit ID="edDepositsSummary" runat="server" AllowNull="False" DataField="DepositsSummary" Enabled="False" />
			<px:PXNumberEdit ID="edRevaluedSummary" runat="server" DataField="RevaluedSummary" Enabled="False" />			
			<px:PXNumberEdit ID="edCuryBalanceSummary" runat="server" DataField="CuryBalanceSummary" Enabled="False" />			
			<px:PXNumberEdit ID="edCuryDepositsSummary" runat="server" AllowNull="False" DataField="CuryDepositsSummary" Enabled="False" />
		</Template>
		<CallbackCommands>
			<Save PostData="Page" />
		</CallbackCommands>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="153px" Style="z-index: 100;" Width="100%" Caption="Customers" AllowSearch="True" AdjustPageSize="Auto" SkinID="Inquire"
		AllowPaging="True" RestrictFields="True">
		<Levels>
			<px:PXGridLevel DataMember="History">
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
					<px:PXSegmentMask ID="edAcctCD" runat="server" DataField="AcctCD" FilterByAllFields="True" />
					<px:PXTextEdit ID="edAcctName" runat="server" DataField="AcctName" />
					<px:PXNumberEdit ID="edBegBalance" runat="server" DataField="BegBalance" />
					<px:PXNumberEdit ID="edEndBalance" runat="server" DataField="EndBalance" />
					<px:PXNumberEdit ID="edBalance" runat="server" DataField="Balance" />
					<px:PXNumberEdit ID="edPayments" runat="server" DataField="Payments" />
					<px:PXNumberEdit ID="edDiscount" runat="server" DataField="Discount" />
					<px:PXNumberEdit ID="edRGOL" runat="server" DataField="RGOL" />
					<px:PXNumberEdit ID="edCrAdjustments" runat="server" DataField="CrAdjustments" />
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
					<px:PXNumberEdit ID="edDrAdjustments" runat="server" DataField="DrAdjustments" /></RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="AcctCD" Width="81px" />
					<px:PXGridColumn DataField="AcctName" Width="297px" />
					<px:PXGridColumn DataField="FinPeriodID" AllowShowHide="False" />
					<px:PXGridColumn DataField="CuryID" />
					<px:PXGridColumn DataField="CuryBegBalance" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="CuryEndBalance" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="CuryBalance" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="CuryDepositsBalance" Label="Prepayments Balance" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="CurySales" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="CuryPayments" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="CuryDiscount" TextAlign="Right" Width="81px" />					
					<px:PXGridColumn DataField="CuryCrAdjustments" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="CuryDrAdjustments" TextAlign="Right" Width="81px" />					
					<px:PXGridColumn DataField="CuryDeposits" Label="PTD Prepayments" TextAlign="Right" Width="100px" />

					<px:PXGridColumn DataField="BegBalance" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="EndBalance" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="Balance" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="DepositsBalance" Label="Prepayments Balance" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="Sales" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="Payments" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="Discount" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="RGOL" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="CrAdjustments" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="DrAdjustments" TextAlign="Right" Width="81px" />
					<px:PXGridColumn AllowNull="False" DataField="FinPtdRevaluated" Label="PTD Revaluated" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="Deposits" Label="PTD Prepayments" TextAlign="Right" Width="100px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<ActionBar DefaultAction="cmdViewDetails">
			<CustomItems>
				<px:PXToolBarButton Text="Customer Details" Key="cmdViewDetails">
				    <AutoCallBack Command="ViewDetails" Target="ds" />
				</px:PXToolBarButton>
			</CustomItems>
		</ActionBar>
	</px:PXGrid>
</asp:Content>
