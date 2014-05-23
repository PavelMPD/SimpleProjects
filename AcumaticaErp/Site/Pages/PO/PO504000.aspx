<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PO504000.aspx.cs"
    Inherits="Page_PO504000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="VendorCatalogueFilter" TypeName="PX.Objects.PO.POVendorCatalogueEnq"
        BorderStyle="NotSet" PageLoadBehavior="PopulateSavedValues">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="Process" StartNewGroup="true" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="ProcessAll" />
            <px:PXDSCallbackCommand DependOnGrid="grid" Name="viewInventoryItem" Visible="False" />
            <px:PXDSCallbackCommand DependOnGrid="grid" Name="viewVendorCatalogue" Visible="False" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Selection" DataMember="VendorCatalogueFilter"
        DefaultControlID="chkMyOwner" TabIndex="100">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
            <px:PXLayoutRule runat="server" Merge="True" />
            <px:PXSelector CommitChanges="True" ID="edOwnerID" runat="server" DataField="OwnerID" DataSourceID="ds" />
            <px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkMyOwner" runat="server" Checked="True" DataField="MyOwner" />
            <px:PXLayoutRule runat="server" />
            <px:PXLayoutRule runat="server" Merge="True" />
            <px:PXSelector CommitChanges="True" ID="edWorkGroupID" runat="server" DataField="WorkGroupID" DataSourceID="ds" />
            <px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkMyWorkGroup" runat="server" DataField="MyWorkGroup" />
            <px:PXLayoutRule runat="server" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edPendingBasePriceDate" runat="server" DataField="PendingBasePriceDate" />
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXTextEdit CommitChanges="True" ID="edDescr" runat="server" DataField="Descr" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXSegmentMask CommitChanges="True" ID="edVendorID" runat="server" DataField="VendorID" DataSourceID="ds" />
            <px:PXSegmentMask CommitChanges="True" ID="edInventoryID" runat="server" DataField="InventoryID" DataSourceID="ds" />
            <px:PXSegmentMask CommitChanges="True" ID="edSubItemCD" runat="server" DataField="SubItemCD" DataSourceID="ds" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" AllowPaging="true" Style="z-index: 100; left: 0px; top: 0px;"
        Width="100%" SkinID="Inquire" Caption="Vendor Prices">
        <Levels>
            <px:PXGridLevel DataMember="VendorCatalogue">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                    <px:PXSegmentMask ID="edVendorID" runat="server" DataField="VendorID" />
                    <px:PXTextEdit ID="edVendorInventoryID" runat="server" DataField="VendorInventoryID" />
                    <px:PXNumberEdit ID="edPendingPrice" runat="server" DataField="PendingPrice" />
                    <px:PXSegmentMask ID="edVendorLocationID" runat="server" DataField="VendorLocationID" />
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                    <px:PXCheckBox ID="chkActive" runat="server" Checked="True" DataField="Active" />
                    <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="true" />
                    <px:PXSegmentMask ID="edSubItemID" runat="server" DataField="SubItemID" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" Width="60px" AllowCheckAll="true" />
                    <px:PXGridColumn DataField="VendorID" DisplayFormat="&gt;AAAAAAAAAA" Width="81px" AllowUpdate="False" />
                    <px:PXGridColumn DataField="AcctName" Width="150px" AllowUpdate="False" />
                    <px:PXGridColumn DataField="VendorLocationID" Width="54px" AllowUpdate="False" DisplayFormat="&gt;AAAA" />
                    <px:PXGridColumn DataField="InventoryID" Width="104px" AllowUpdate="False" DisplayFormat="&gt;AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" />
                    <px:PXGridColumn DataField="Descr" Width="150px" AllowUpdate="False" />
                    <px:PXGridColumn DataField="SubItemID" Width="45px" AllowUpdate="False" DisplayFormat="&gt;A" />
                    <px:PXGridColumn DataField="PurchaseUnit" AllowUpdate="False" DisplayFormat="&gt;aaaaaa" />
                    <px:PXGridColumn DataField="CuryID" AllowUpdate="False" DisplayFormat="&gt;LLLLL" />
                    <px:PXGridColumn AllowNull="False" DataField="PendingPrice" TextAlign="Right" Width="81px" AllowUpdate="False" />
                    <px:PXGridColumn AllowUpdate="False" DataField="PendingDate" Width="90px" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="EffPrice" TextAlign="Right" Width="100px" />
                    <px:PXGridColumn DataField="EffDate" Width="90px" AllowUpdate="False" />
                    <px:PXGridColumn DataField="LastPrice" Width="100px" AllowNull="False" AllowUpdate="False" TextAlign="Right" />
                    <px:PXGridColumn DataField="ItemClassID" AllowUpdate="False" DisplayFormat="&gt;aaaaaaaaaa" />
                    <px:PXGridColumn DataField="VendorClassID" DisplayFormat="&gt;aaaaaaaaaa" AllowUpdate="False" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <ActionBar>
            <Actions>
                <NoteShow Enabled="False" />
            </Actions>
            <CustomItems>
                <px:PXToolBarButton Text="View Inventory Item">
                    <AutoCallBack Command="viewInventoryItem" Target="ds">
                        <Behavior CommitChanges="True" />
                    </AutoCallBack>
                </px:PXToolBarButton>
                <px:PXToolBarButton Text="View Vendor Catalogue">
                    <AutoCallBack Command="viewVendorCatalogue" Target="ds">
                        <Behavior CommitChanges="True" />
                    </AutoCallBack>
                </px:PXToolBarButton>
            </CustomItems>
        </ActionBar>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXGrid>
</asp:Content>
