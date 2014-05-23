<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AP509900.aspx.cs" Inherits="Page_AP509900" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.AP.APIntegrityCheck" PrimaryView="Filter">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Process" CommitChanges="True" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="ProcessAll" CommitChanges="True" />
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="ViewVendor" Visible="False" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection" DefaultControlID="edVendorClassID">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
			<px:PXSelector ID="edFinPeriodID" runat="server" DataField="FinPeriodID" />
			<px:PXSelector CommitChanges="True" ID="edVendorClassID" runat="server" DataField="VendorClassID" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" Caption="Vendors" AllowPaging="true" AdjustPageSize="Auto" SkinID="Inquire" AllowSearch="True" FastFilterFields="AcctCD,AcctName">
		<Levels>
			<px:PXGridLevel DataMember="APVendorList">
				<RowTemplate>
				</RowTemplate>
				<Columns>
					<px:PXGridColumn AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" AllowSort="False" AllowMove="False" Width="20px" />
					<px:PXGridColumn DataField="AcctCD" Width="81px"/>
					<px:PXGridColumn DataField="VendorClassID" Width="81px" />
					<px:PXGridColumn DataField="AcctName" Width="297px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<ActionBar>
			<CustomItems>
				<px:PXToolBarButton Text="View Vendor" Tooltip="View Vendor" CommandName="ViewVendor" CommandSourceID="ds" />
			</CustomItems>
		</ActionBar>
	</px:PXGrid>
</asp:Content>
