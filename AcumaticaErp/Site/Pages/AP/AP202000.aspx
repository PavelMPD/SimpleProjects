<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" 
	CodeFile="AP202000.aspx.cs" Inherits="Pages_AP_AP202000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.AP.APVendorSalesPriceMaint">
		<CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand CommitChanges="true" Name="Update" Visible="false" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Selection" DataMember="Filter"
        DefaultControlID="edCustPriceClassID">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXSelector CommitChanges="True" ID="edVendorID" runat="server" DataField="VendorID" AllowEdit="True" />
            <px:PXSelector CommitChanges="True" ID="edLocationID" runat="server" DataField="LocationID" AllowEdit="True" />
			<px:PXSelector CommitChanges="True" ID="edCuryID" runat="server" DataField="CuryID" AllowEdit="true"/>
			<px:PXCheckBox runat="server" ID="chkPromotionalPrice" DataField="PromotionalPrice" CommitChanges="true" />
			<px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXLayoutRule ID="PXLayoutRule3" runat="server" Merge="True" />
            <px:PXSelector CommitChanges="True" ID="edPriceManagerID" runat="server" DataField="OwnerID" />
            <px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkMyUser" runat="server" Checked="True" DataField="MyOwner" />
            <px:PXLayoutRule ID="PXLayoutRule4" runat="server" Merge="False" />
            <px:PXLayoutRule ID="PXLayoutRule5" runat="server" Merge="True" />
            <px:PXSelector CommitChanges="True" ID="edWorkGroupID" runat="server" DataField="WorkGroupID" />
            <px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkMyWorkGroup" runat="server" DataField="MyWorkGroup" />
            <px:PXLayoutRule ID="PXLayoutRule6" runat="server" Merge="False" />
            <px:PXSelector CommitChanges="True" ID="edItemClassID" runat="server" DataField="ItemClassID" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="144px" Style="z-index: 100" Width="100%" Caption="Sales Prices"
        SkinID="Details" FilterShortCuts="True" AdjustPageSize="Auto" AllowPaging="True" SyncPosition="true">
        <Levels>
            <px:PXGridLevel DataMember="Records">
				<RowTemplate>
					<px:PXNumberEdit runat="server" ID="edPendingBreakQty" DataField="PendingBreakQty" />
					<px:PXNumberEdit runat="server" ID="edPendingPrice" DataField="PendingPrice" />
					<px:PXSegmentMask ID="edSubItemID2" runat="server" DataField="SubItemID" AutoRefresh="True" />
				</RowTemplate>
                <Columns>
					<px:PXGridColumn Type="CheckBox" DataField="AllLocations" Width="80px" TextAlign="Center" />
					<px:PXGridColumn DataField="InventoryID" Width="108px" AutoCallBack="True"/>
					<px:PXGridColumn AllowUpdate="False" DataField="InventoryItem__Descr" Width="130px" />
					<px:PXGridColumn DataField="SubItemID" Width="85px" />
					<px:PXGridColumn DataField="UOM" DisplayFormat="&gt;aaaaaa" Width="90px" CommitChanges="true" />
					<px:PXGridColumn DataField="VendorInventoryID" Width="90px"  />
					<px:PXGridColumn AutoCallBack="true" DataField="EffectiveDate" Width="90px" />
					<px:PXGridColumn DataField="PendingBreakQty" Width="90px" TextAlign="Right" CommitChanges="true"/>
                    <px:PXGridColumn DataField="PendingPrice" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="BreakQty" Width="90px" TextAlign="Right"/>
                    <px:PXGridColumn DataField="SalesPrice" TextAlign="Right" Width="81px" />
                    <px:PXGridColumn DataField="LastDate" Width="90px" />
                    <px:PXGridColumn DataField="ExpirationDate" Width="90px" />
					<px:PXGridColumn DataField="LastBreakQty" Width="90px" TextAlign="Right"/>
                    <px:PXGridColumn DataField="LastPrice" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="CuryID" Width="81px" />
                </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <ActionBar PagerVisible="False">
            <PagerSettings Mode="NextPrevFirstLast" />
            <CustomItems>
                <px:PXToolBarButton Text="Update Sales Price" Key="cmdUpdate">
                    <AutoCallBack Command="Update" Target="ds">
                        <Behavior PostData="Page" />
                    </AutoCallBack>
                </px:PXToolBarButton>
            </CustomItems>
        </ActionBar>
		<Mode AllowUpload="true" />
	</px:PXGrid>
	<px:PXSmartPanel ID="PanelMassUpdate" runat="server" CommandSourceID="ds" Caption="Update Prices" CaptionVisible="True" ShowAfterLoad="true" LoadOnDemand="true"
        DesignView="Content" Key="MassUpdateSettings" AutoCallBack-Enabled="true" AutoCallBack-Target="massUpdateForm" AutoCallBack-Command="Refresh">
            <px:PXFormView ID="massUpdateForm" runat="server" Width="100%" DataSourceID="ds" SkinID="Transparent" DataMember="MassUpdateSettings">
                <Template>
                    <px:PXLayoutRule ID="PXLayoutRule6" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXDateTimeEdit ID="edEffectiveDate" runat="server" DataField="EffectiveDate" />
                </Template>
            </px:PXFormView>
        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
            <px:PXButton ID="btnSave" runat="server" DialogResult="OK" Text="OK" >
                <AutoCallBack Command="Save" Target="massUpdateForm"/>
            </px:PXButton>
            <px:PXButton ID="btnCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>