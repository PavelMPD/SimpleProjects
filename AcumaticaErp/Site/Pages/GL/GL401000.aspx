<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="GL401000.aspx.cs" Inherits="Page_GL401000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.GL.AccountHistoryEnq"
		PrimaryView="Filter" PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="PreviousPeriod" StartNewGroup="True" HideText="True"/>
            <px:PXDSCallbackCommand Name="NextPeriod" HideText="True"/>
			<px:PXDSCallbackCommand Name="AccountDetails" Visible="False" DependOnGrid="grid" />
			<px:PXDSCallbackCommand Name="AccountBySub" Visible="False" DependOnGrid="grid" />
			<px:PXDSCallbackCommand Name="AccountByPeriod" Visible="False" DependOnGrid="grid" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" Width="100%" DataMember="Filter" Caption="Selection"
		DefaultControlID="edBranchID">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
			<px:PXSegmentMask CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID" />
			<px:PXSelector CommitChanges="True" ID="edLedgerID" runat="server" DataField="LedgerID" />
			<px:PXSelector CommitChanges="True" ID="edFinPeriodID" runat="server" DataField="FinPeriodID" />
			<px:PXSelector CommitChanges="True" ID="edAccountClassID" runat="server" DataField="AccountClassID" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
			<px:PXSegmentMask CommitChanges="True" ID="edSubCD" runat="server" DataField="SubCD"
				SelectMode="Segment" />
			<px:PXCheckBox CommitChanges="True" ID="chkShowCuryDetail" runat="server" DataField="ShowCuryDetail" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="150px" Width="100%" AllowSearch="True"
		BatchUpdate="True" AdjustPageSize="Auto" AllowPaging="True" Caption="Account Activity Summary"
		SkinID="Inquire"  FastFilterFields="AccountID,Description" RestrictFields="True">
		<Levels>
			<px:PXGridLevel DataMember="EnqResult">
				<Columns>
					<px:PXGridColumn DataField="BranchID" Width="100px" />
					<px:PXGridColumn DataField="AccountID" Width="100px" />
					<px:PXGridColumn DataField="Type" Type="DropDownList" Width="70px" />
					<px:PXGridColumn DataField="Description" Width="200px" />
					<px:PXGridColumn DataField="LastActivityPeriod" Width="100px" />
					<px:PXGridColumn DataField="SignBegBalance" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="PtdDebitTotal" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="PtdCreditTotal" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="SignEndBalance" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="CuryID" AllowShowHide="Server" />
					<px:PXGridColumn DataField="SignCuryBegBalance" TextAlign="Right" Width="100px" AllowShowHide="Server" />
					<px:PXGridColumn DataField="CuryPtdDebitTotal" TextAlign="Right" Width="100px" AllowShowHide="Server" />
					<px:PXGridColumn DataField="CuryPtdCreditTotal" TextAlign="Right" Width="100px" AllowShowHide="Server" />
					<px:PXGridColumn DataField="SignCuryEndBalance" TextAlign="Right" Width="100px" AllowShowHide="Server" />
					<px:PXGridColumn DataField="PtdSaldo" TextAlign="Right" Width="100px" AllowShowHide="Server" />
					<px:PXGridColumn DataField="CuryPtdSaldo" TextAlign="Right" Width="100px" AllowShowHide="Server" />
					<px:PXGridColumn DataField="ConsolAccountCD" Width="100px" />
					<px:PXGridColumn DataField="AccountClassID" Width="90px" />
				</Columns>
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
					<px:PXTextEdit ID="edBranchID" runat="server" DataField="BranchID" />
					<px:PXSelector ID="edAccountID" runat="server" DataField="AccountID" AutoGenerateColumns="True" />
					<px:PXDropDown ID="edType" runat="server" DataField="Type" />
					<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
					<px:PXLabel ID="lblSubIDH" runat="server"></px:PXLabel>
					<px:PXTextEdit ID="edLastActivityPeriod" runat="server" DataField="LastActivityPeriod" />
					<px:PXNumberEdit ID="edBegBalance" runat="server" DataField="BegBalance" />
					<px:PXNumberEdit ID="edPtdCreditTotal" runat="server" DataField="PtdCreditTotal" />
					<px:PXNumberEdit ID="edPtdDebitTotal" runat="server" DataField="PtdDebitTotal" />
					<px:PXNumberEdit ID="edEndBalance" runat="server" DataField="EndBalance" /></RowTemplate>
				<Layout FormViewHeight=""></Layout>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<Mode AllowAddNew="False" AllowDelete="False" />
		<ActionBar DefaultAction="cmdAcctDetails">
			<CustomItems>
				<px:PXToolBarButton Text="Account Details" Key="cmdAcctDetails">
					<AutoCallBack Target="ds" Command="AccountDetails" />
				</px:PXToolBarButton>
				<px:PXToolBarButton Text="Account by Subaccount">
					<AutoCallBack Target="ds" Command="AccountBySub" />
				</px:PXToolBarButton>
				<px:PXToolBarButton Text="Account By Period">
					<AutoCallBack Target="ds" Command="AccountByPeriod" />
				</px:PXToolBarButton>
			</CustomItems>
		</ActionBar>
	</px:PXGrid>
</asp:Content>
