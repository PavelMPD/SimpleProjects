<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="IN403000.aspx.cs"
    Inherits="Page_IN403000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PageLoadBehavior="PopulateSavedValues" PrimaryView="Filter"
        TypeName="PX.Objects.IN.InventoryTranByAcctEnq">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Cancel" PostData="Self" />
            <px:PXDSCallbackCommand Name="previousperiod" StartNewGroup="True" HideText="True" />
            <px:PXDSCallbackCommand Name="nextperiod" StartNewGroup="True" HideText="True" />
            <px:PXDSCallbackCommand Name="ViewItem" DependOnGrid="grid" Visible="false" />
            <px:PXDSCallbackCommand Name="ViewSummary" DependOnGrid="grid" Visible="false" />
            <px:PXDSCallbackCommand Name="ViewAllocDet" DependOnGrid="grid" Visible="false" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Selection" CaptionAlign="Justify"
        DataMember="Filter" DefaultControlID="edAccountID" TabIndex="5500">
        <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXSegmentMask CommitChanges="True" ID="edAccountID" runat="server" DataField="AccountID" DataSourceID="ds" />
            <px:PXSegmentMask CommitChanges="True" ID="edSubCD" runat="server" DataField="SubCD" SelectMode="Segment" 
                DataSourceID="ds" />
            <px:PXSelector CommitChanges="True" Size="xs" ID="edPeriodID" runat="server" DataField="FinPeriodID" 
                DataSourceID="ds" />
            <px:PXCheckBox CommitChanges="True" ID="chkByFinancialPeriod" runat="server" DataField="ByFinancialPeriod" />
            <px:PXSegmentMask CommitChanges="True" ID="edInventoryID" runat="server" DataField="InventoryID" DataSourceID="ds" />
            <px:PXSegmentMask CommitChanges="True" ID="edSiteID" runat="server" DataField="SiteID" DataSourceID="ds" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" StartGroup="True" GroupCaption="Date Range"/>
            <px:PXPanel ID="PXPanel1" runat="server" RenderSimple="True" RenderStyle="Simple">
                <px:PXLayoutRule runat="server" StartColumn="True">
                </px:PXLayoutRule>
                <px:PXDateTimeEdit ID="edStartDate" runat="server" CommitChanges="True" DataField="StartDate" DisplayFormat="d">
                </px:PXDateTimeEdit>
                <px:PXDateTimeEdit ID="edEndDate" runat="server" CommitChanges="True" DataField="EndDate" DisplayFormat="d">
                </px:PXDateTimeEdit>
                <px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="S" StartColumn="True">
                </px:PXLayoutRule>
                <px:PXDateTimeEdit ID="edPeriodStartDate" runat="server" DataField="PeriodStartDate">
                </px:PXDateTimeEdit>
                <px:PXDateTimeEdit ID="edPeriodEndDate" runat="server" DataField="PeriodEndDateInclusive">
                </px:PXDateTimeEdit>
            </px:PXPanel>
            <px:PXCheckBox CommitChanges="True" ID="chkSummaryByDay" runat="server" DataField="SummaryByDay" AlignLeft="True" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="144px" Style="z-index: 100" Width="100%" AdjustPageSize="Auto"
        AllowPaging="True" AllowSearch="True" BatchUpdate="True" Caption="Transaction Details" SkinID="Inquire" RestrictFields="True">
        <Levels>
            <px:PXGridLevel DataMember="ResultRecords">
                <RowTemplate>
                    <px:PXLayoutRule ID="PXLayoutRule8" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                    <px:PXLayoutRule ID="PXLayoutRule9" runat="server" Merge="True" />
                    <px:PXSelector Size="s" ID="edDocRefNbr" runat="server" DataField="DocRefNbr" AllowEdit="true" />
                    <px:PXSelector Size="xxs" ID="edINTran__SOOrderType" runat="server" DataField="INTran__SOOrderType" />
                    <px:PXLayoutRule ID="PXLayoutRule10" runat="server" Merge="False" />
                    <px:PXLayoutRule ID="PXLayoutRule11" runat="server" Merge="True" />
                    <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="true" />
                    <px:PXSelector Size="s" ID="edINTran__SOOrderNbr" runat="server" DataField="INTran__SOOrderNbr" AllowEdit="true" />
                    <px:PXLayoutRule ID="PXLayoutRule12" runat="server" Merge="False" />
                    <px:PXSelector ID="edINTran__POReceiptNbr" runat="server" DataField="INTran__POReceiptNbr" AllowEdit="true" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="GridLineNbr" TextAlign="Right" />
                    <px:PXGridColumn DataField="AccountID" DisplayFormat="&gt;AAAAAAAAAA" />
                    <px:PXGridColumn DataField="SubID" DisplayFormat="&gt;AAAA-AA-AA-AAAA" />
                    <px:PXGridColumn DataField="TranType" />
                    <px:PXGridColumn DataField="DocRefNbr" />
                    <px:PXGridColumn DataField="InventoryID" DisplayFormat="&gt;CCCCC-CCCCCCCCCCCCCCC" />
                    <px:PXGridColumn DataField="SubItemCD" DisplayFormat="&gt;AA-A" />
                    <px:PXGridColumn DataField="SiteID" DisplayFormat="&gt;AAAAAAAAAA" />
                    <px:PXGridColumn DataField="LocationID" DisplayFormat="&gt;AAAAAAAAAA" />
                    <px:PXGridColumn DataField="CostAdj" Type="CheckBox" />
                    <px:PXGridColumn DataField="TranDate" />
                    <px:PXGridColumn AllowNull="False" DataField="BegBalance" TextAlign="Right" Width="100px" />
                    <px:PXGridColumn AllowNull="False" DataField="Debit" TextAlign="Right" />
                    <px:PXGridColumn AllowNull="False" DataField="Credit" TextAlign="Right" />
                    <px:PXGridColumn AllowNull="False" DataField="EndBalance" TextAlign="Right" Width="100px" />
                    <px:PXGridColumn DataField="FinPerNbr" DisplayFormat="##-####" />
                    <px:PXGridColumn DataField="TranPerNbr" DisplayFormat="##-####" />
                    <px:PXGridColumn AllowNull="False" DataField="Qty" TextAlign="Right" />
                    <px:PXGridColumn AllowNull="False" DataField="UnitCost" TextAlign="Right" />
                    <px:PXGridColumn DataField="InventoryID_InventoryItem_Descr" Visible="false" Width="120px" />
                    <px:PXGridColumn DataField="INTran__SOOrderType" DisplayFormat="&gt;aa" Label="SO Order Type" />
                    <px:PXGridColumn DataField="INTran__SOOrderNbr" DisplayFormat="&gt;CCCCCCCCCCCCCCC" Label="SO Order Nbr.">
                        <NavigateParams>
                            <px:PXControlParam Name="OrderType" ControlID="grid" PropertyName="DataValues[&quot;INTran__SOOrderType&quot;]" />
                            <px:PXControlParam Name="OrderNbr" ControlID="grid" PropertyName="DataValues[&quot;INTran__SOOrderNbr&quot;]" />
                        </NavigateParams>
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="INTran__POReceiptNbr" DisplayFormat="&gt;CCCCCCCCCCCCCCC" Label="PO Receipt Nbr." />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <ActionBar DefaultAction="allocDet">
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
            </CustomItems>
        </ActionBar>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXGrid>
</asp:Content>
