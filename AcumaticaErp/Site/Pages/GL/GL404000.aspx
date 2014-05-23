<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="GL404000.aspx.cs" Inherits="Page_GL404000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="Content1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.GL.AccountByPeriodEnq"
		PrimaryView="Filter" PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" StartNewGroup="true" Name="previousperiod" HideText="True"/>
			<px:PXDSCallbackCommand CommitChanges="True" Name="nextperiod" HideText="True"/>
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="ViewBatch" Visible="false" />
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="ViewAPDocument" Visible="false" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server"   Width="100%"
		Caption="Selection" DataMember="Filter" DefaultControlID="edFinPeriodID" DataSourceID="ds" TabIndex="100">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
			<px:PXSegmentMask CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID"/>
			<px:PXSelector CommitChanges="True" ID="edLedgerID" runat="server" DataField="LedgerID"/>
		    <px:PXSelector CommitChanges="True" ID="edStartPeriodID" runat="server" DataField="StartPeriodID"/>
			<px:PXSelector CommitChanges="True" ID="edEndPeriodID" runat="server" DataField="EndPeriodID" Autorefresh="True"/>
			<px:PXSegmentMask CommitChanges="True" ID="edAccountID" runat="server" DataField="AccountID"/>
			<px:PXSegmentMask CommitChanges="True" ID="edSubID" runat="server" DataField="SubID" SelectMode="Segment"  />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="S" />
			<px:PXDateTimeEdit CommitChanges="True" ID="edStartDate" runat="server" DataField="StartDate" />
			<px:PXDateTimeEdit ID="edPeriodStartDate" runat="server" DataField="PeriodStartDate" />
			<px:PXDateTimeEdit CommitChanges="True" ID="edEndDateUI" runat="server" DataField="EndDateUI" />
			<px:PXDateTimeEdit ID="edPeriodEndDateUI" runat="server" DataField="PeriodEndDateUI" />
			<px:PXLayoutRule runat="server" StartColumn="True" SuppressLabel="True" />
			<px:PXCheckBox CommitChanges="True" ID="chkShowSummary" runat="server" DataField="ShowSummary" />
			<px:PXCheckBox CommitChanges="True" ID="chkIncludeUnposted" runat="server" DataField="IncludeUnposted" />
			<px:PXCheckBox CommitChanges="True" ID="chkIncludeUnreleased" runat="server" DataField="IncludeUnreleased" />
			<px:PXCheckBox CommitChanges="True" ID="chkShowCuryDetail" runat="server" DataField="ShowCuryDetail" />
            
			<px:PXLayoutRule runat="server" StartColumn="True">
            </px:PXLayoutRule>
            <px:PXNumberEdit ID="edBegBal" runat="server" DataField="BegBal">
            </px:PXNumberEdit>
            <px:PXNumberEdit ID="edTurnOver" runat="server" DataField="TurnOver">
            </px:PXNumberEdit>
            <px:PXNumberEdit ID="edEndBal" runat="server" DataField="EndBal">
            </px:PXNumberEdit>
        </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server"  Height="150px" 
		Width="100%" AllowPaging="True" AdjustPageSize="Auto" Caption="Summary By Period"
		BatchUpdate="True" AllowSearch="True" SkinID="Inquire" RestrictFields="True" DataSourceID="ds" TabIndex="100">
		<AutoSize Container="Window" Enabled="True" />
		<Mode AllowAddNew="False" AllowDelete="False"  />
		<Levels>
			<px:PXGridLevel DataMember="GLTranEnq">
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
					<px:PXTextEdit ID="edModule" runat="server" DataField="Module" Enabled="False" />
					<px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True" />
					<px:PXSegmentMask ID="edReferenceID" runat="server" DataField="ReferenceID" Enabled="False" />
					<px:PXTextEdit ID="edBatchNbr" runat="server" DataField="BatchNbr" Enabled="False" />
					<px:PXNumberEdit ID="edLineNbr" runat="server" DataField="LineNbr" Enabled="False" />
					<px:PXDateTimeEdit ID="edTranDate" runat="server" DataField="TranDate" Enabled="False" />
					<px:PXSelector ID="edFinPeriodID" runat="server" DataField="FinPeriodID" Enabled="False"/>
					<px:PXTextEdit ID="edRefNbr" runat="server" DataField="RefNbr" Enabled="False" />
					<px:PXTextEdit ID="edBranchID" runat="server" DataField="BranchID" Enabled="False" />
					<px:PXSelector ID="edAccountID" runat="server" DataField="AccountID" Enabled="False"
						AutoGenerateColumns="True" />
					<px:PXSegmentMask ID="edSubID" runat="server" DataField="SubID" Enabled="False" SelectMode="Segment" />
					<px:PXTextEdit ID="edTranDesc" runat="server" DataField="TranDesc" Enabled="False" />
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
					<px:PXNumberEdit ID="edDebitAmt" runat="server" DataField="DebitAmt" Enabled="False" />
					<px:PXNumberEdit ID="edBegBalance" runat="server" DataField="BegBalance" Enabled="False" />
					<px:PXNumberEdit ID="edCuryBegBalance" runat="server" DataField="CuryBegBalance"
						Enabled="False" />
					<px:PXNumberEdit ID="edCreditAmt" runat="server" DataField="CreditAmt" Enabled="False" />
					<px:PXNumberEdit ID="edCuryDebitAmt" runat="server" DataField="CuryDebitAmt" Enabled="False" />
					<px:PXNumberEdit ID="edCuryCreditAmt" runat="server" DataField="CuryCreditAmt" Enabled="False" />
					<px:PXNumberEdit ID="edEndBalance" runat="server" DataField="EndBalance" Enabled="False" />
					<px:PXNumberEdit ID="edCuryEndBalance" runat="server" DataField="CuryEndBalance"
						Enabled="False" />
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="Module" Width="100px" />
					<px:PXGridColumn DataField="BatchNbr" Width="100px" />
					<px:PXGridColumn DataField="TranDate" Width="100px" />
					<px:PXGridColumn DataField="FinPeriodID" Width="100px" />
					<px:PXGridColumn DataField="TranDesc" Width="224px" />
					<px:PXGridColumn DataField="RefNbr" Width="100px"  />
					<px:PXGridColumn DataField="LineNbr" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="BranchID" Width="100px" />
					<px:PXGridColumn DataField="AccountID" Width="108px" />
					<px:PXGridColumn DataField="SubID" Width="198px" />
					<px:PXGridColumn DataField="SignBegBalance" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="DebitAmt" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="CreditAmt" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="SignEndBalance" TextAlign="Right" MatrixMode="True" Width="100px" />
					<px:PXGridColumn DataField="CuryID"  AllowShowHide="Server" />
					<px:PXGridColumn DataField="SignCuryBegBalance" TextAlign="Right" Width="100px" AllowShowHide="Server" />
					<px:PXGridColumn DataField="CuryDebitAmt" TextAlign="Right" Width="100px" AllowShowHide="Server" />
					<px:PXGridColumn DataField="CuryCreditAmt" TextAlign="Right" Width="100px" AllowShowHide="Server" />
					<px:PXGridColumn DataField="SignCuryEndBalance" TextAlign="Right" Width="100px" AllowShowHide="Server" />
					<px:PXGridColumn DataField="InventoryID" Width="120px" />
					<px:PXGridColumn  DataField="ReferenceID" Width="120px" />
				</Columns>
				<Layout FormViewHeight=""></Layout>
			</px:PXGridLevel>
		</Levels>
		<ActionBar DefaultAction="cmdViewBatch">
			<CustomItems>
				<px:PXToolBarButton Text="View Batch" Key="cmdViewBatch">
				    <AutoCallBack Command="ViewBatch" Target="ds" />
				</px:PXToolBarButton>
				<px:PXToolBarButton Text="View Source Document">
				    <AutoCallBack Command="ViewAPDocument" Target="ds" />
				</px:PXToolBarButton>
			</CustomItems>
		</ActionBar>
	</px:PXGrid>
</asp:Content>
