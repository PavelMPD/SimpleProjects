<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PO505000.aspx.cs"
    Inherits="Page_PO505000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.PO.POCreate" PrimaryView="Filter">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Process" StartNewGroup="True" CommitChanges="true" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" DataMember="Filter" Width="100%" Caption="Selection"
        DefaultControlID="edPurchDate" EmailingGraph="">
        <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXDateTimeEdit CommitChanges="True" runat="server" DataField="PurchDate" ID="edPurchDate" />
            <px:PXLayoutRule runat="server" Merge="True" />
            <px:PXSelector CommitChanges="True" ID="edOwnerID" runat="server" DataField="OwnerID" />
            <px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkMyOwner" runat="server" Checked="True" DataField="MyOwner" />
            <px:PXLayoutRule runat="server" Merge="False" />
            <px:PXLayoutRule runat="server" Merge="True" />
            <px:PXSelector CommitChanges="True" ID="edWorkGroupID" runat="server" DataField="WorkGroupID" />
            <px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkMyWorkGroup" runat="server" DataField="MyWorkGroup" />
            <px:PXLayoutRule runat="server" Merge="False" />
            <px:PXSelector CommitChanges="True" ID="edItemClassID" runat="server" DataField="ItemClassID" />
            <px:PXSegmentMask CommitChanges="True" ID="edInventoryID" runat="server" DataField="InventoryID">
                <GridProperties>
                    <PagerSettings Mode="NextPrevFirstLast" />
                </GridProperties>
            </px:PXSegmentMask>
            <px:PXSegmentMask CommitChanges="True" runat="server" DataField="SiteID" ID="edSiteID" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXSegmentMask CommitChanges="True" runat="server" DataField="VendorID" ID="edVendorID" />
            <px:PXSegmentMask CommitChanges="True" ID="edReplenishmentSourceSiteID" runat="server" DataField="ReplenishmentSourceSiteID" />
            <px:PXSegmentMask CommitChanges="True" ID="edCustomerID" runat="server" DataField="CustomerID" />
            <px:PXSelector CommitChanges="True" ID="edOrderType" runat="server" DataField="OrderType" />
            <px:PXSelector CommitChanges="True" ID="edOrderNbr" runat="server" DataField="OrderNbr" AutoRefresh="true" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXNumberEdit ID="edOrderTotal" runat="server" DataField="OrderTotal" Enabled="False" />
            <px:PXNumberEdit ID="edOrderWeight" runat="server" DataField="OrderWeight" Enabled="False" />
            <px:PXNumberEdit ID="edOrderVolume" runat="server" DataField="OrderVolume" Enabled="False" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="Details" Caption="Details">
        <Levels>
            <px:PXGridLevel DataMember="FixedDemand">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                    <px:PXSegmentMask ID="edReplenishmentSourceSiteID" runat="server" DataField="ReplenishmentSourceSiteID" Enabled="False" />
                    <px:PXSegmentMask ID="edVendorLocationID" runat="server" AutoRefresh="True" DataField="VendorLocationID">
                        <Parameters>
                            <px:PXSyncGridParam ControlID="grid" />
                        </Parameters>
                    </px:PXSegmentMask>
                    <px:PXDateTimeEdit ID="edPlanDate" runat="server" DataField="PlanDate" Enabled="False" />
                    <px:PXSegmentMask CommitChanges="True" ID="edVendorID" runat="server" DataField="VendorID" Enabled="False" AutoRefresh="true">
                        <Parameters>
                            <px:PXSyncGridParam ControlID="grid" />
                        </Parameters>
                    </px:PXSegmentMask>
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="true" />
                    <px:PXGridColumn AllowUpdate="False" DataField="PlanType_INPlanType_descr" Width="126px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="InventoryID" DisplayFormat="&gt;AAA-&gt;CCC-&gt;AA" Width="81px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="InventoryID_InventoryItem_descr" Width="108px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="SubItemID" DisplayFormat="&gt;AA-A" Width="45px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="SiteID" DisplayFormat="&gt;AAAAAAAAAA" Width="81px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="SiteID_INSite_descr" Width="108px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="UOM" Width="81px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="OrderQty" TextAlign="Right" Width="81px" AutoCallBack="true" />
                    <px:PXGridColumn AllowUpdate="False" DataField="PlanDate" Width="90px" />
                    <px:PXGridColumn AllowUpdate="False" Width="80px" AllowNull="false" DataField="FixedSource" Label="Method" RenderEditorText="True"
                        AutoCallBack="true" />
                    <px:PXGridColumn AllowUpdate="False" DataField="ReplenishmentSourceSiteID" DisplayFormat="&gt;AAAAAAAAAA" Label="Source Warehouse" />
                    <px:PXGridColumn AllowUpdate="False" DataField="VendorID" DisplayFormat="&gt;AAAAAAAAAA" Width="81px" AutoCallBack="true" />
                    <px:PXGridColumn AllowUpdate="False" DataField="VendorID_Vendor_acctName" Width="108px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="VendorLocationID" DisplayFormat="&gt;AAAAAAAAAA" Width="81px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="Location__vLeadTime" Width="81px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="AddLeadTimeDays" Width="81px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="Vendor__TermsID" Width="81px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="Location__vCarrierID" Width="81px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="effPrice" Width="81px" TextAlign="Right" />
                    <px:PXGridColumn AllowUpdate="False" DataField="ExtCost" TextAlign="Right" Width="100px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="Vendor__CuryID" Width="71px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="SOOrder__CustomerID" DisplayFormat="&gt;AAAAAAAAAA" Width="81px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="SOOrder__CustomerID_BAccountR_acctName" Width="108px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="SOOrder__CustomerLocationID" DisplayFormat="&gt;AAAAAAAAAA" Width="81px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="SOLine__UnitPrice" Width="81px" TextAlign="Right" />
                    <px:PXGridColumn AllowUpdate="False" DataField="SOLine__UOM" Width="81px" />
                    <px:PXGridColumn DataField="SOOrder__OrderNbr" DisplayFormat="&gt;CCCCCCCCCCCCCCC" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="ExtWeight" Label="Weight" TextAlign="Right" Width="100px" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="ExtVolume" Label="Volume" TextAlign="Right" Width="100px" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <ActionBar>
        </ActionBar>
    </px:PXGrid>
</asp:Content>
