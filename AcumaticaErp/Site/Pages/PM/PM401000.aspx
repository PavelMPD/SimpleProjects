<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PM401000.aspx.cs"
    Inherits="Page_PM401000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.PM.TransactionInquiry" PrimaryView="Filter">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="ViewDocument" Visible="False" DependOnGrid="grid" />
            <px:PXDSCallbackCommand Name="ViewInventory" DependOnGrid="grid" Visible="False" />
			<px:PXDSCallbackCommand Name="ViewInvoice" DependOnGrid="grid" Visible="False" />
			<px:PXDSCallbackCommand Name="ViewAllocation" DependOnGrid="grid" Visible="False" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Selection" DataMember="Filter"
        DefaultControlID="edProjectID" NoteField="">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXSegmentMask CommitChanges="True" ID="edProjectID" runat="server" DataField="ProjectID" />
            <px:PXSegmentMask CommitChanges="True" ID="edAccountGroupID" runat="server" DataField="AccountGroupID" />
            <px:PXSegmentMask CommitChanges="True" ID="edProjectTaskID" runat="server" DataField="ProjectTaskID" />
            <px:PXSelector CommitChanges="True" ID="edInventoryID" runat="server" AutoRefresh="True" DataField="InventoryID" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edDateFrom" runat="server" DataField="DateFrom" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edDateTo" runat="server" DataField="DateTo" />
            <px:PXSegmentMask CommitChanges="True" ID="edResourceID" runat="server" DataField="ResourceID" DataMember="_EPEmployee_" />
            <px:PXCheckBox CommitChanges="True" ID="chkOnlyAllocation" runat="server" DataField="OnlyAllocation" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="Inquire" AdjustPageSize="Auto"
        AllowPaging="True" Caption="Transactions" FastFilterFields="RefNbr,Description" RestrictFields="True">
        <Levels>
            <px:PXGridLevel DataMember="Transactions">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXSegmentMask ID="edResourceID" runat="server" DataField="ResourceID" />
                    <px:PXSegmentMask ID="edAccountGroupID" runat="server" DataField="AccountGroupID" />
                    <px:PXSegmentMask ID="edTaskID" runat="server" DataField="TaskID" AllowEdit="True" />
                    <px:PXSegmentMask ID="edBranchID" runat="server" DataField="BranchID" DataMember="_Branch_" />
                    <px:PXSegmentMask ID="edResourceLocID" runat="server" DataField="ResourceLocID" />
                    <px:PXSelector ID="edBAccountID" runat="server" DataField="BAccountID" />
                    <px:PXSegmentMask ID="edLocationID" runat="server" DataField="LocationID" />
                    <px:PXSelector ID="edInventoryID" runat="server" DataField="InventoryID" />
                    <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
                    <px:PXSelector ID="edUOM" runat="server" DataField="UOM" />
                    <px:PXNumberEdit ID="edQty" runat="server" DataField="Qty" />
                    <px:PXCheckBox ID="chkBillable" runat="server" Checked="True" DataField="Billable" />
                    <px:PXNumberEdit ID="edBillableQty" runat="server" DataField="BillableQty" />
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXNumberEdit ID="edUnitRate" runat="server" DataField="UnitRate" />
                    <px:PXNumberEdit ID="edAmount" runat="server" DataField="Amount" />
                    <px:PXSegmentMask ID="edAccountID" runat="server" DataField="AccountID" />
                    <px:PXSegmentMask ID="edSubID" runat="server" DataField="SubID" />
                    <px:PXSegmentMask ID="edOffsetAccountID" runat="server" DataField="OffsetAccountID" />
                    <px:PXSelector Size="s" ID="edBatchNbr" runat="server" DataField="BatchNbr" DataMember="_Batch_PMTran.tranType_" AllowEdit="True" />
                    <px:PXSegmentMask ID="edOffsetSubID" runat="server" DataField="OffsetSubID" />
				</RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="BranchID" Label="Branch" Width="63px" />
                    <px:PXGridColumn DataField="PMRegister__Module" Label="Module" RenderEditorText="True" Width="63px" />
                    <px:PXGridColumn DataField="PMRegister__RefNbr" Label="Ref Number" LinkCommand="ViewDocument" Width="81px" />
                    <px:PXGridColumn DataField="TaskID" Label="Task" Width="81px" />
                    <px:PXGridColumn DataField="Date" DataType="DateTime" Width="90px"/>
                    <px:PXGridColumn DataField="Description" Label="Description" Width="200px" />
                    <px:PXGridColumn DataField="InventoryID" Label="InventoryID" Width="108px" LinkCommand="ViewInventory" />
                    <px:PXGridColumn DataField="UOM" Label="UOM" Width="54px" />
                    <px:PXGridColumn DataField="Qty" Label="Qty" TextAlign="Right" Width="54px" />
                    <px:PXGridColumn DataField="Billable" Label="Billable" TextAlign="Center" Type="CheckBox" Width="54px" />
                    <px:PXGridColumn DataField="BillableQty" Label="Billable Qty" TextAlign="Right" Width="54px" />
                    <px:PXGridColumn DataField="UnitRate" Label="Unit Rate" TextAlign="Right" Width="54px" />
                    <px:PXGridColumn DataField="Amount" Label="Amount" TextAlign="Right" Width="81px" />
                    <px:PXGridColumn DataField="BAccountID" Label="Customer/Vendor" Width="108px" />
                    <px:PXGridColumn DataField="ResourceID" Label="Resource" Width="81px" />
                    <px:PXGridColumn DataField="AccountGroupID" Label="Account Group" Width="81px" />
                    <px:PXGridColumn DataField="AccountID" Label="Account" Width="54px" />
                    <px:PXGridColumn DataField="SubID" Label="Subaccount" Width="108px" />
                    <px:PXGridColumn DataField="Account__AccountGroupID" Label="GL Account-Account Group" Width="81px" />
                    <px:PXGridColumn DataField="OffsetAccountID" Label="Offset Account" Width="54px" />
                    <px:PXGridColumn DataField="OffsetSubID" Label="Offset SubAccount" Width="108px" />
                    <px:PXGridColumn DataField="BatchNbr" Width="108px" />
                    <px:PXGridColumn DataField="EarningType" Width="100px" />
                    <px:PXGridColumn DataField="OvertimeMultiplier" Width="70px" />
                    <px:PXGridColumn DataField="Released" Label="Released" TextAlign="Center" Type="CheckBox" Width="54px" />
                    <px:PXGridColumn DataField="Allocated" Label="Allocated" TextAlign="Center" Type="CheckBox" Width="54px" />
					<px:PXGridColumn DataField="PMTranEx__RefNbr" Width="90px"  LinkCommand="ViewAllocation"/>
                    <px:PXGridColumn DataField="Billed" Label="Billed" TextAlign="Center" Type="CheckBox" Width="54px" />
                    <px:PXGridColumn DataField="PMRegister__OrigDocType" Type="DropDownList" Width="90px" />
                    <px:PXGridColumn DataField="PMRegister__OrigDocNbr" Width="90px" />
					<px:PXGridColumn DataField="ARTran__RefNbr" Width="90px" LinkCommand="ViewInvoice" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <ActionBar>
            <CustomItems>
                <px:PXToolBarButton CommandName="ViewDocument" CommandSourceID="ds" Text="View Document" />
            </CustomItems>
        </ActionBar>
    </px:PXGrid>
</asp:Content>
