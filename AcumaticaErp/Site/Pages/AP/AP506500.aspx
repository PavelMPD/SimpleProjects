<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AP506500.aspx.cs" Inherits="Page_AP506500" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" Visible="True" PrimaryView="documents" TypeName="PX.Objects.AP.APLandedCostProcess">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Cancel" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Process" StartNewGroup="True" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="ProcessAll" />
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="viewDocument" Visible="False" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowPaging="True" ActionsPosition="Top" AllowSearch="True" DataSourceID="ds" Caption="Documents" FastFilterFields="DocType,RefNbr,VendorID,CuryID"
		BatchUpdate="True" SkinID="Inquire" TabIndex="7500">
		<ActionBar>
			<CustomItems>
				<px:PXToolBarButton Text="View Document">
					<AutoCallBack Command="viewDocument" Target="ds">
						<Behavior CommitChanges="True"></Behavior>
					</AutoCallBack>
				</px:PXToolBarButton>
			</CustomItems>
		</ActionBar>
		<Levels>
			<px:PXGridLevel DataMember="documents">
				<Columns>
					<px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="true" AllowMove="False" Width="30px" />
					<px:PXGridColumn DataField="DocType" Width="80px" Type="DropDownList" />
					<px:PXGridColumn DataField="RefNbr" Width="80px" LinkCommand="viewDocument" />
					<px:PXGridColumn DataField="DocDate" Width="90px" />
					<px:PXGridColumn DataField="CuryID" />
					<px:PXGridColumn DataField="VendorID" Width="100px" />
					<px:PXGridColumn DataField="VendorID_description" Width="200px" />
					<px:PXGridColumn DataField="VendorLocationID" Width="100px" />
					<px:PXGridColumn DataField="CuryDocAmount" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="CuryLCTranTotal" TextAlign="Right" Width="100px" />
				</Columns>
				<Layout FormViewHeight=""></Layout>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
	</px:PXGrid>
</asp:Content>
