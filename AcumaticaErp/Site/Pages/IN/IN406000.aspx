<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="IN406000.aspx.cs"
    Inherits="Page_IN406000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PageLoadBehavior="PopulateSavedValues" PrimaryView="Filter"
        TypeName="PX.Objects.IN.InventoryTranSumEnq">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Cancel" PostData="Self" />
            <px:PXDSCallbackCommand Name="previousperiod" StartNewGroup="True" HideText="True" />
            <px:PXDSCallbackCommand Name="nextperiod" StartNewGroup="True" HideText="True" />
            <px:PXDSCallbackCommand Name="ViewInventoryTranDet" Visible="False" DependOnGrid="grid" />
            <px:PXDSCallbackCommand Name="ViewItem" DependOnGrid="grid" Visible="false" />
            <px:PXDSCallbackCommand Name="ViewSummary" DependOnGrid="grid" Visible="false" />
            <px:PXDSCallbackCommand Name="ViewAllocDet" DependOnGrid="grid" Visible="false" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Selection" CaptionAlign="Justify"
        DataMember="Filter" DefaultControlID="edPeriodID">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXSelector CommitChanges="True" ID="edPeriodID" runat="server" DataField="FinPeriodID" />
            <px:PXCheckBox CommitChanges="True" ID="chkByFinancialPeriod" runat="server" DataField="ByFinancialPeriod" />
            <px:PXSegmentMask CommitChanges="True" ID="edSiteID" runat="server" DataField="SiteID" />
            <px:PXSegmentMask CommitChanges="True" ID="edLocationID" runat="server" AutoRefresh="True" DataField="LocationID" />
            <px:PXSelector CommitChanges="True" ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="true">
                <GridProperties>
                    <PagerSettings Mode="NextPrevFirstLast" />
                </GridProperties>
            </px:PXSelector>
            <px:PXSegmentMask CommitChanges="True" ID="edSubItemCD" runat="server" DataField="SubItemCD" AutoRefresh="true" />
            <px:PXCheckBox CommitChanges="True" ID="chkShowItemsWithoutMovement" runat="server" DataField="ShowItemsWithoutMovement" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Detail Level" />
            <px:PXCheckBox CommitChanges="True" AlignLeft="True" ID="chkSubItemDetails" runat="server" DataField="SubItemDetails" />
            <px:PXCheckBox CommitChanges="True" AlignLeft="True" ID="chkSiteDetails" runat="server" DataField="SiteDetails" />
            <px:PXCheckBox CommitChanges="True" AlignLeft="True" ID="chkLocationDetails" runat="server" DataField="LocationDetails" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="144px" Style="z-index: 100; left: 0px; top: 0px;" Width="100%"
        AdjustPageSize="Auto" AllowPaging="True" AllowSearch="True" BatchUpdate="True" Caption="Transaction Details" SkinID="Inquire" RestrictFields="True">
        <Levels>
            <px:PXGridLevel DataMember="ResultRecords">
                <Columns>
                    <px:PXGridColumn DataField="FinPeriodID" DisplayFormat="##-####" Visible="False" />
                    <px:PXGridColumn DataField="InventoryID" DisplayFormat="&gt;CCCCC-CCCCCCCCCCCCCCC" />
                    <px:PXGridColumn DataField="SubItemID" DisplayFormat="&gt;AA-A" />
                    <px:PXGridColumn DataField="SiteID" DisplayFormat="&gt;AAAAAAAAAA" />
                    <px:PXGridColumn DataField="LocationID" DisplayFormat="&gt;AAAAAAAAAA" />
                    <px:PXGridColumn AllowNull="False" DataField="TranBegQty" TextAlign="Right" />
                    <px:PXGridColumn DataField="TranPtdQtyIssued" TextAlign="Right" />
                    <px:PXGridColumn DataField="TranPtdQtyReceived" TextAlign="Right" />
                    <px:PXGridColumn DataField="TranPtdQtySales" TextAlign="Right" />
                    <px:PXGridColumn DataField="TranPtdQtyCreditMemos" TextAlign="Right" />
                    <px:PXGridColumn DataField="TranPtdQtyDropShipSales" TextAlign="Right" Width="80px" />
                    <px:PXGridColumn DataField="TranPtdQtyTransferIn" TextAlign="Right" />
                    <px:PXGridColumn DataField="TranPtdQtyTransferOut" TextAlign="Right" />
                    <px:PXGridColumn DataField="TranPtdQtyAssemblyIn" TextAlign="Right" Width="90px" />
                    <px:PXGridColumn DataField="TranPtdQtyAssemblyOut" TextAlign="Right" Width="90px" />
                    <px:PXGridColumn DataField="TranPtdQtyAdjusted" TextAlign="Right" />
                    <px:PXGridColumn AllowNull="False" DataField="TranYtdQty" TextAlign="Right" />
                    <px:PXGridColumn DataField="InventoryID_InventoryItem_Descr" Visible="false" Width="120px" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <ActionBar DefaultAction="tranDet">
            <CustomItems>
                <px:PXToolBarButton Key="item" Text="View Inventory Item">
                    <AutoCallBack Target="ds" Command="ViewItem" />
                </px:PXToolBarButton>
                <px:PXToolBarButton Key="summary" Text="View Summary">
                    <AutoCallBack Target="ds" Command="ViewSummary" />
                </px:PXToolBarButton>
                <px:PXToolBarButton Key="allocDet" Text="View Allocation Details">
                    <AutoCallBack Target="ds" Command="ViewAllocDet" />
                </px:PXToolBarButton>
                <px:PXToolBarButton Text="View Transaction Details" Key="tranDet">
                    <AutoCallBack Command="ViewInventoryTranDet" Target="ds" />
                </px:PXToolBarButton>
            </CustomItems>
        </ActionBar>
        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
    </px:PXGrid>
</asp:Content>
