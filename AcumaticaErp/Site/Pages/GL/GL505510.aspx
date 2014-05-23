<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="GL505510.aspx.cs" Inherits="Page_GL505510"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" Visible="True" PrimaryView="BudgetArticles"
		TypeName="PX.Objects.GL.GLBudgetRelease" BorderStyle="NotSet" Height="42px" 
		SuspendUnloading="False">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Process" CommitChanges="true" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="ProcessAll" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="viewArticle" Visible="false" DependOnGrid="grid" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100;
		left: 0px; top: 0px;" AllowPaging="True" AllowSearch="true" BatchUpdate="true"
		SkinID="Inquire" Caption="Budget Articles" AdjustPageSize="Auto">
		<Levels>
			<px:PXGridLevel DataMember="BudgetArticles">
				<Columns>
					<px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" Width="40px"
						AllowCheckAll="True" />
					<px:PXGridColumn DataField="BranchID" Width="100px" />
					<px:PXGridColumn DataField="LedgerID" Width="100px" />
					<px:PXGridColumn DataField="FinYear" Width="70px" />
					<px:PXGridColumn DataField="AccountID" Width="100px" />
					<px:PXGridColumn DataField="SubID" Width="150px" />
					<px:PXGridColumn DataField="Description" Width="200px" />
					<px:PXGridColumn DataField="Amount" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="ReleasedAmount" TextAlign="Right" Width="100px" />
				</Columns>
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
					<px:PXCheckBox ID="chkSelected" runat="server" DataField="Selected" />
					<px:PXSelector Size="s" ID="edBranchID" runat="server" DataField="BranchID" Enabled="False" />
					<px:PXSelector Size="s" ID="edLedgerID" runat="server" DataField="LedgerID" Enabled="False" />
					<px:PXTextEdit ID="edFinYear" runat="server" DataField="FinYear" Enabled="False" />
					<px:PXSegmentMask ID="edAccountID" runat="server" DataField="AccountID" Enabled="False" />
					<px:PXSegmentMask ID="edSubID" runat="server" DataField="SubID" Enabled="False" />
					<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" Enabled="False" />
					<px:PXNumberEdit ID="edAmount" runat="server" DataField="Amount" Enabled="False" />
					<px:PXNumberEdit ID="edReleasedAmount" runat="server" DataField="ReleasedAmount" Enabled="False" />
				</RowTemplate>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
		<ActionBar>
			<CustomItems>
				<px:PXToolBarButton Text="View Article">
				    <AutoCallBack Target="ds" Command="viewArticle" />
				</px:PXToolBarButton>
			</CustomItems>
		</ActionBar>
		<Layout ShowRowStatus="False" />
	</px:PXGrid>
</asp:Content>
