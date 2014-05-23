<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" 
	CodeFile="AP502020.aspx.cs" Inherits="Pages_AP_AP502020" Title="Untitled Page"%>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.AP.APUpdateVendorPrice">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Process" StartNewGroup="True" CommitChanges="true" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Selection" DataMember="Filter"
        DefaultControlID="edPendingBasePriceDate">
        <Template>
			<px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXLayoutRule ID="PXLayoutRule3" runat="server" Merge="True" />
            <px:PXSelector CommitChanges="True" ID="edPriceManagerID" runat="server" DataField="OwnerID" />
            <px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkMyUser" runat="server" Checked="True" DataField="MyOwner" />
            <px:PXLayoutRule ID="PXLayoutRule4" runat="server" Merge="False" />
            <px:PXLayoutRule ID="PXLayoutRule5" runat="server" Merge="True" />
            <px:PXSelector CommitChanges="True" ID="edWorkGroupID" runat="server" DataField="WorkGroupID" />
            <px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkMyWorkGroup" runat="server" DataField="MyWorkGroup" />
            <px:PXLayoutRule ID="PXLayoutRule6" runat="server" Merge="False" />
			<px:PXDateTimeEdit CommitChanges="True" ID="edPendingBasePriceDate" runat="server" DataField="PendingBasePriceDate" />
			<px:PXLayoutRule ID="PXLayoutRule8" runat="server" ColumnSpan="2" />
			<%--<px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />--%>
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
            <px:PXSelector CommitChanges="True" ID="edVendorID" runat="server" DataField="VendorID" />
            <px:PXSelector CommitChanges="True" ID="edInventoryID" runat="server" DataField="InventoryID" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" SkinID="Inquire" Caption="Prices"
        AllowPaging="True" AdjustPageSize="Auto">
        <Levels>
            <px:PXGridLevel DataMember="Items">
                <RowTemplate>
                    <px:PXLayoutRule ID="PXLayoutRule7" runat="server" StartColumn="True" LabelsWidth="XM" ControlSize="XM" />
                    <px:PXCheckBox ID="chkSelected" runat="server" DataField="Selected" />
                    <px:PXSelector ID="edCuryID" runat="server" DataField="CuryID" Enabled="False" />
                    <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" Enabled="False" AllowEdit="True" />
                    <px:PXSelector ID="edUOM" runat="server" DataField="UOM" Enabled="False" />
                    <px:PXDateTimeEdit ID="edEffectiveDate" runat="server" DataField="EffectiveDate" Enabled="False" />
                    <px:PXDateTimeEdit ID="edLastDate" runat="server" DataField="LastDate" Enabled="False" />
                    <px:PXNumberEdit ID="edSalesPrice" runat="server" DataField="SalesPrice" Enabled="False" />
                    <px:PXNumberEdit ID="edLastPrice" runat="server" DataField="LastPrice" Enabled="False" />
                    <px:PXNumberEdit ID="edPendingPrice" runat="server" DataField="PendingPrice" Enabled="False" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="Selected" AllowCheckAll="true" TextAlign="Center" Type="CheckBox" Width="30px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="VendorID" DisplayFormat="&gt;aaaaaaaaaa" Width="90px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="InventoryID" DisplayFormat="&gt;CCCCC-CCCCCCCCCCCCCCC" Width="144px" />
                    <px:PXGridColumn DataField="InventoryItem__Descr" Label="Inventory Item-Description" Width="200px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="UOM" DisplayFormat="&gt;aaaaaa" Width="90px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="CuryID" Width="80px" />
					<px:PXGridColumn DataField="BreakQty" Width="90px" TextAlign="Right"/>
                    <px:PXGridColumn AllowUpdate="False" DataField="SalesPrice" TextAlign="Right" Width="81px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="LastDate" Width="90px" />
					<px:PXGridColumn DataField="LastBreakQty" Width="90px" TextAlign="Right"/>
                    <px:PXGridColumn AllowUpdate="False" DataField="LastPrice" TextAlign="Right" Width="81px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="EffectiveDate" Width="90px" />
					<px:PXGridColumn DataField="PendingBreakQty" Width="90px" TextAlign="Right"/>
                    <px:PXGridColumn AllowUpdate="False" DataField="PendingPrice" TextAlign="Right" Width="81px" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <ActionBar PagerVisible="False">
            <PagerSettings Mode="NextPrevFirstLast" />
        </ActionBar>
    </px:PXGrid>
</asp:Content>
