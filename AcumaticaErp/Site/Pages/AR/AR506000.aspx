<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AR506000.aspx.cs" Inherits="Page_AR506000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.AR.ARAutoApplyPayments">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Cancel" />
			<px:PXDSCallbackCommand StartNewGroup="True" CommitChanges="True" Name="Process" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="ProcessAll" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Parameters" DataMember="Filter" DefaultControlID="edApplicationDate">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
			<px:PXDateTimeEdit CommitChanges="True" ID="edApplicationDate" runat="server" DataField="ApplicationDate" />
			<px:PXSelector CommitChanges="True" ID="edFinPeriodID" runat="server" DataField="FinPeriodID" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" SuppressLabel="true" />
			<px:PXCheckBox ID="chkApplyCreditMemos" runat="server" DataField="ApplyCreditMemos" />
			<px:PXCheckBox ID="chkReleaseBatchWhenFinished" runat="server" DataField="ReleaseBatchWhenFinished" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" AllowPaging="True" Caption="Statement Cycles" SkinID="Inquire">
		<Levels>
			<px:PXGridLevel DataMember="ARStatementCycleList">
				<RowTemplate>
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" AllowSort="False" AllowMove="False" Width="30px" />
					<px:PXGridColumn DataField="StatementCycleId" Width="100px" />
					<px:PXGridColumn DataField="LastStmtDate" Width="90px" />
					<px:PXGridColumn DataField="Descr" Width="300px" />
					<px:PXGridColumn DataField="NextStmtDate" Width="90px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXGrid>
</asp:Content>
