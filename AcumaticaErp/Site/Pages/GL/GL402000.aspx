<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="GL402000.aspx.cs" Inherits="Page_GL402000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.GL.AccountHistoryByYearEnq"
		PrimaryView="Filter" PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="previousperiod" StartNewGroup="true" HideText="True"/>
			<px:PXDSCallbackCommand CommitChanges="True" Name="nextperiod" HideText="True"/>
			<px:PXDSCallbackCommand Name="AccountDetails" Visible="False" DependOnGrid="grid" />
			<px:PXDSCallbackCommand Name="AccountBySub" Visible="False" DependOnGrid="grid" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server"  Width="100%" DataMember="Filter"
		Caption="Selection" DefaultControlID="edBranchID">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
			<px:PXSegmentMask CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID" />
			<px:PXSelector CommitChanges="True" ID="edLedgerID" runat="server" DataField="LedgerID" />
			<px:PXSelector CommitChanges="True" ID="edFinYear" runat="server" DataField="FinYear" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
			<px:PXSegmentMask CommitChanges="True" ID="edAccountID" runat="server" DataField="AccountID" />
			<px:PXSegmentMask CommitChanges="True" ID="edSubCD" runat="server" DataField="SubCD"
				SelectMode="Segment" />
			<px:PXCheckBox CommitChanges="True" ID="chkShowCuryDetail" runat="server" DataField="ShowCuryDetail" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="153px"  Width="100%"
		Caption="Account Summary by Financial Periods" AllowSearch="True" AllowPaging="True"
		SkinID="Inquire" RestrictFields="True">
		<Levels>
			<px:PXGridLevel DataMember="EnqResult">
				<Columns>
					<px:PXGridColumn DataField="LastActivityPeriod" Width="100px" />
					<px:PXGridColumn DataField="SignBegBalance" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="PtdDebitTotal" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="PtdCreditTotal" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="SignEndBalance" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="CuryID" Width="100px" AllowShowHide="Server" />
					<px:PXGridColumn DataField="SignCuryBegBalance" TextAlign="Right" Width="100px" AllowShowHide="Server" />
					<px:PXGridColumn DataField="CuryPtdDebitTotal" TextAlign="Right" Width="100px" AllowShowHide="Server" />
					<px:PXGridColumn DataField="CuryPtdCreditTotal" TextAlign="Right" Width="100px" AllowShowHide="Server" />
					<px:PXGridColumn DataField="SignCuryEndBalance" TextAlign="Right" Width="100px" AllowShowHide="Server" />
					<px:PXGridColumn DataField="PtdSaldo" TextAlign="Right" Width="100px" AllowShowHide="Server" />
					<px:PXGridColumn DataField="CuryPtdSaldo" TextAlign="Right" Width="100px" AllowShowHide="Server" />
				</Columns>
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
					<px:PXSelector ID="edAccountID" runat="server" DataField="AccountID" Enabled="False"
						AutoGenerateColumns="True" />
					<px:PXNumberEdit ID="edPtdSaldo" runat="server" DataField="PtdSaldo" />
					<px:PXNumberEdit ID="edBegBalance" runat="server" DataField="BegBalance" Enabled="False" />
					<px:PXNumberEdit ID="edCuryPtdSaldo" runat="server" DataField="CuryPtdSaldo" />
					<px:PXNumberEdit ID="edPtdDebitTotal" runat="server" DataField="PtdDebitTotal" Enabled="False" />
					<px:PXNumberEdit ID="edPtdCreditTotal" runat="server" DataField="PtdCreditTotal"
						Enabled="False" />
					<px:PXNumberEdit ID="edEndBalance" runat="server" DataField="EndBalance" Enabled="False" />
					<px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="SM" StartColumn="True" />
					<px:PXTextEdit ID="edLastActivityPeriod" runat="server" DataField="LastActivityPeriod"
						Enabled="False" />
					<px:PXTextEdit ID="edCuryID" runat="server" DataField="CuryID" Enabled="False" />
					<px:PXNumberEdit ID="edCuryBegBalance" runat="server" DataField="CuryBegBalance"
						Enabled="False" />
					<px:PXNumberEdit ID="edCuryPtdDebitTotal" runat="server" DataField="CuryPtdDebitTotal"
						Enabled="False" />
					<px:PXNumberEdit ID="edCuryPtdCreditTotal" runat="server" DataField="CuryPtdCreditTotal"
						Enabled="False" />
					<px:PXNumberEdit ID="edCuryEndBalance" runat="server" DataField="CuryEndBalance"
						Enabled="False" />
                        </RowTemplate>
				<Layout FormViewHeight=""></Layout>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<ActionBar DefaultAction="cmdAcctDetails">
			<CustomItems>
				<px:PXToolBarButton Text="Account Details" Key="cmdAcctDetails">
					<AutoCallBack Target="ds" Command="AccountDetails" />
				</px:PXToolBarButton>
				<px:PXToolBarButton Text="Account by Subaccount">
					<AutoCallBack Target="ds" Command="AccountBySub" />
				</px:PXToolBarButton>
			</CustomItems>
		</ActionBar>
		<Mode AllowAddNew="False" AllowDelete="False" />
	</px:PXGrid>
</asp:Content>
