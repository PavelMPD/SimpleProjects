<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AR402000.aspx.cs" Inherits="Page_AR402000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="true" TypeName="PX.Objects.AR.ARDocumentEnq" PrimaryView="Filter" 
        PageLoadBehavior="PopulateSavedValues" style="float:left">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="PreviousPeriod" StartNewGroup="True" HideText="True"/>
            <px:PXDSCallbackCommand Name="NextPeriod" HideText="True"/>
            <px:PXDSCallbackCommand DependOnGrid="grid" Name="ViewDocument" Visible="false" />
            <px:PXDSCallbackCommand DependOnGrid="grid" Name="PayDocument" Visible="false" />
            <px:PXDSCallbackCommand Name="CreateInvoice" Visible="false" />
            <px:PXDSCallbackCommand Name="CreatePayment" Visible="false" />
            <px:PXDSCallbackCommand Name="ARBalanceByCustomerReport" Visible="False"/>
            <px:PXDSCallbackCommand Name="CustomerHistoryReport" Visible="False"/>
            <px:PXDSCallbackCommand Name="ARAgedPastDueReport" Visible="False"/>
            <px:PXDSCallbackCommand Name="ARAgedOutstandingReport" Visible="False"/>
            <px:PXDSCallbackCommand Name="ARRegisterReport" Visible="False"/>
        </CallbackCommands>
    </px:PXDataSource>
    <px:PXToolBar ID="toolbar1" runat="server" SkinID="Navigation" BackColor="Transparent" CommandSourceID="ds">
        <Items>
            <px:PXToolBarSeperator />
            <px:PXToolBarButton Text="Actions" Tooltip="Actions">
                <MenuItems>
                    <px:PXMenuItem CommandName="CreateInvoice" Text="New Invoice/Memo" CommandSourceID="ds" />
                    <px:PXMenuItem CommandName="CreatePayment" Text="New Payment" CommandSourceID="ds" />
                </MenuItems>
            </px:PXToolBarButton>
            <px:PXToolBarButton Text="Reports" Tooltip="Reports">
                <MenuItems>
                    <px:PXMenuItem Text="AR Balance by Customer" CommandSourceID="ds" CommandName="ARBalanceByCustomerReport"/>
                    <px:PXMenuItem Text="Customer History" CommandSourceID="ds" CommandName="CustomerHistoryReport"/>
                    <px:PXMenuItem Text="AR Aged Past Due" CommandSourceID="ds" CommandName="ARAgedPastDueReport"/>
                    <px:PXMenuItem Text="AR Aged Outstanding" CommandSourceID="ds" CommandName="ARAgedOutstandingReport"/>
                    <px:PXMenuItem Text="AR Register" CommandSourceID="ds" CommandName="ARRegisterReport"/>
                </MenuItems>
            </px:PXToolBarButton>
        </Items>
        <Layout ItemsAlign="Left" />
    </px:PXToolBar>
    <div style="clear: left" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection" DefaultControlID="edCustomerID" TabIndex="1100">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			<px:PXSegmentMask CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID" DataSourceID="ds" />
			<px:PXSegmentMask CommitChanges="True" ID="edCustomerID" runat="server" DataField="CustomerID" DataSourceID="ds" />
			<px:PXSelector CommitChanges="True" ID="edPeriod" runat="server" DataField="Period" DataSourceID="ds" />
			<px:PXSelector CommitChanges="True" ID="edCuryID" runat="server" DataField="CuryID" DataSourceID="ds" />
			<px:PXSegmentMask CommitChanges="True" ID="edARAcctID" runat="server" DataField="ARAcctID" DataSourceID="ds" />
			<px:PXSegmentMask CommitChanges="True" ID="edSubCD" runat="server" DataField="SubCD" SelectMode="Segment" DataSourceID="ds" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			<px:PXCheckBox CommitChanges="True" ID="chkShowOpenDocsOnly" runat="server" DataField="ShowAllDocs" AlignLeft="True" TextAlign="Left" LabelWidth="m" />
            <px:PXCheckBox CommitChanges="True" ID="chkIncludeUnreleased" runat="server" DataField="IncludeUnreleased" AlignLeft="True" TextAlign="Left" LabelWidth="m" />
			<px:PXCheckBox CommitChanges="True" ID="chkByFinancialPeriod" runat="server" DataField="ByFinancialPeriod" AlignLeft="True" TextAlign="Left" LabelWidth="m" />
			<px:PXPanel runat="server" ID="pnlBalances" RenderStyle="Simple">			
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="S" />			
			<px:PXNumberEdit ID="edBalanceSummary" runat="server" DataField="BalanceSummary" Enabled="False" />
			<px:PXNumberEdit ID="edCustomerBalance" runat="server" DataField="CustomerBalance" Enabled="False" />
			<px:PXNumberEdit ID="edCustomerDepositsBalance" runat="server" DataField="CustomerDepositsBalance" Enabled="False" />
			<px:PXNumberEdit ID="edDifference" runat="server" DataField="Difference" Enabled="False" />						
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="S" />			
			<px:PXNumberEdit ID="edCuryBalanceSummary" runat="server" DataField="CuryBalanceSummary" Enabled="False" />
			<px:PXNumberEdit ID="edCuryCustomerBalance" runat="server" DataField="CuryCustomerBalance" Enabled="False" />
			<px:PXNumberEdit ID="edCuryCustomerDepositsBalance" runat="server" DataField="CuryCustomerDepositsBalance" Enabled="False" />
			<px:PXNumberEdit ID="edCuryDifference" runat="server" DataField="CuryDifference" Enabled="False" />
			</px:PXPanel>
		</Template>
		<Activity Width="" Height=""></Activity>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="153px" Style="z-index: 100" Width="100%" Caption="Documents" AllowSearch="True" AdjustPageSize="Auto" SkinID="Inquire" AllowPaging="True" 
		TabIndex="1300" RestrictFields="True">
		<Levels>
			<px:PXGridLevel DataMember="Documents">
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
					</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="DisplayDocType" Type="DropDownList" Width="81px" />
					<px:PXGridColumn DataField="RefNbr" Width="108px" />
					<px:PXGridColumn DataField="FinPeriodID" />
					<px:PXGridColumn DataField="DocDate" Width="90px" />
					<px:PXGridColumn DataField="DueDate" Width="90px" />
					<px:PXGridColumn DataField="Status" Type="DropDownList" Width="72px" />
					<px:PXGridColumn DataField="CuryID" />
					<px:PXGridColumn DataField="CuryOrigDocAmt" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="CuryBegBalance" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="CuryDocBal" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="CuryDiscActTaken" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="OrigDocAmt" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="BegBalance" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="DocBal" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="DiscActTaken" TextAlign="Right" Width="100px" />					
					<px:PXGridColumn DataField="RGOLAmt" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="PaymentMethodID" />
					<px:PXGridColumn DataField="ExtRefNbr" Width="120px" />
					<px:PXGridColumn DataField="DocDesc" Width="180px" />					
				</Columns>

<Layout FormViewHeight=""></Layout>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<ActionBar DefaultAction="cmdViewDoc">
			<CustomItems>
				<px:PXToolBarButton Text="View Document" Key="cmdViewDoc">
				    <AutoCallBack Command="ViewDocument" Target="ds" />
				</px:PXToolBarButton>
				<px:PXToolBarButton Text="Pay Invoice">
				    <AutoCallBack Command="PayDocument" Target="ds" />
				</px:PXToolBarButton>
			</CustomItems>
		</ActionBar>
	</px:PXGrid>
</asp:Content>
