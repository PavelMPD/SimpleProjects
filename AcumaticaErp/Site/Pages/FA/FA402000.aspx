<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FA402000.aspx.cs"
    Inherits="Page_FA402000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.FA.AssetSummary">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Cancel" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="dispose" StartNewGroup="True" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Selection" DataMember="Filter"
        TabIndex="1100">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="S" />
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXSelector CommitChanges="True" runat="server" DataField="ClassID" ID="edClassID" DataSourceID="ds" />
            <px:PXDropDown CommitChanges="True" runat="server" DataField="AssetType" ID="edAssetType" />
            <px:PXDropDown CommitChanges="True" runat="server" DataField="PropertyType" ID="edPropertyType" />
            <px:PXDropDown CommitChanges="True" runat="server" DataField="Condition" ID="edCondition" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edAcqDateFrom" runat="server" DataField="AcqDateFrom" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edAcqDateTo" runat="server" DataField="AcqDateTo" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM" />
            <px:PXSelector ID="edPONumber" runat="server" DataField="PONumber" DataSourceID="ds" />
            <px:PXSelector CommitChanges="True" runat="server" DataField="ReceiptNbr" ID="edReceiptNbr" DataSourceID="ds" />
            <px:PXTextEdit CommitChanges="True" runat="server" DataField="BillNumber" ID="edBillNumber" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXSegmentMask CommitChanges="True" runat="server" DataField="BranchID" ID="edLocationID" DataSourceID="ds" />
            <px:PXSelector CommitChanges="True" runat="server" DataField="BuildingID" ID="edBuildingID" DataSourceID="ds" />
            <px:PXTextEdit CommitChanges="True" runat="server" DataField="Floor" ID="edFloor" />
            <px:PXTextEdit CommitChanges="True" runat="server" DataField="Room" ID="edRoom" />
            <px:PXSelector CommitChanges="True" runat="server" DataField="Custodian" ID="edCustodian" DataSourceID="ds" />
            <px:PXSelector CommitChanges="True" runat="server" DataField="Department" ID="edDepartment" DataSourceID="ds" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="Inquire" Caption="Fixed Assets Summary"
        AdjustPageSize="Auto" AllowPaging="True" AllowSearch="True" FastFilterFields="AssetCD" RestrictFields="True">
        <Levels>
            <px:PXGridLevel DataMember="assets">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                    <px:PXSelector ID="edAssetCD" runat="server" AllowEdit="True" DataField="AssetCD" />
                    <px:PXSelector ID="edParentAssetID" runat="server" AllowEdit="True" DataField="ParentAssetID" />
                    <px:PXSelector ID="edClassID" runat="server" AllowEdit="True" DataField="ClassID" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn AllowCheckAll="True" DataField="Selected" Label="Selected" TextAlign="Center" Type="CheckBox" Width="60px" />
                    <px:PXGridColumn DataField="AssetCD" DisplayFormat="&gt;CCCCCCCCCCCCCCC" Label="Asset ID" />
                    <px:PXGridColumn DataField="Description" Label="Description" Width="200px" />
                    <px:PXGridColumn DataField="ClassID" Label="Asset Class" />
                    <px:PXGridColumn DataField="ParentAssetID" Label="Parent Asset" />
                    <px:PXGridColumn DataField="AssetType" Label="Asset Type" RenderEditorText="True" />
                    <px:PXGridColumn DataField="UsefulLife" Label="Useful Life, Years" TextAlign="Right" Width="100px" />
                    <px:PXGridColumn DataField="FADetails__DepreciateFromDate" Label="Depreciate From Date" Width="90px" />
                    <px:PXGridColumn AllowNull="False" DataField="FADetails__AcquisitionCost" Label="Acquisition Cost" TextAlign="Right" Width="100px" />
                    <px:PXGridColumn AllowNull="False" DataField="FADetails__PropertyType" Label="Property Type" RenderEditorText="True" />
                    <px:PXGridColumn AllowNull="False" DataField="FADetails__Condition" Label="Condition" RenderEditorText="True" />
                    <px:PXGridColumn DataField="FADetails__ReceiptNbr" Label="Receipt Nbr." DisplayFormat="&gt;CCCCCCCCCCCCCCC" />
                    <px:PXGridColumn DataField="FADetails__PONumber" Label="PO Number" />
                    <px:PXGridColumn DataField="FADetails__BillNumber" Label="Bill Number" />
                    <px:PXGridColumn DataField="FALocationHistory__LocationID" Label="Location" DisplayFormat="&gt;AAAAAA" />
                    <px:PXGridColumn DataField="FALocationHistory__BuildingID" Label="FALocationHistory-Building" />
                    <px:PXGridColumn DataField="FALocationHistory__Floor" Label="FALocationHistory-Floor" />
                    <px:PXGridColumn DataField="FALocationHistory__Room" Label="FALocationHistory-Room" />
                    <px:PXGridColumn DataField="FALocationHistory__Custodian" Label="FALocationHistory-Custodian" />
                    <px:PXGridColumn DataField="FALocationHistory__Department" DisplayFormat="&gt;aaaaaaaaaa" Label="FALocationHistory-Department" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <ActionBar PagerVisible="False"/>
    </px:PXGrid>
    <px:PXSmartPanel ID="spDisposeParamDlg" runat="server" DesignView="Content" Key="DispParams" LoadOnDemand="True" AcceptButtonID="cbOk" CancelButtonID="cbCancel"
        Caption="Disposal Parameters" CaptionVisible="True" HideAfterAction="False">
        <px:PXFormView ID="DisposePrm" runat="server" DataSourceID="ds" Style="z-index: 108" Width="100%" DataMember="DispParams"
            Caption="Dispose Parameters" SkinID="Transparent" TabIndex="2300">
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                <px:PXDateTimeEdit CommitChanges="True" ID="edDisposalDate" runat="server" DataField="DisposalDate" />
                <px:PXSelector CommitChanges="True" ID="edDisposalPeriodID" runat="server" DataField="DisposalPeriodID" />
                <px:PXNumberEdit CommitChanges="True" ID="edDisposalAmt" runat="server" AllowNull="True" DataField="DisposalAmt" />
                <px:PXSelector CommitChanges="True" ID="edDisposalMethodID" runat="server" DataField="DisposalMethodID" />
                <px:PXSegmentMask CommitChanges="True" ID="edDisposalAccountID" runat="server" DataField="DisposalAccountID" />
                <px:PXSegmentMask CommitChanges="True" ID="edDisposalSubID" runat="server" DataField="DisposalSubID" AutoRefresh="True" />
                <px:PXCheckBox CommitChanges="True" ID="chkDeprBeforeDisposal" runat="server" DataField="DeprBeforeDisposal" />
                <px:PXTextEdit ID="edReason" runat="server" DataField="Reason" />
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
            <px:PXButton ID="cbOk" runat="server" Text="OK" DialogResult="OK" />
            <px:PXButton ID="cbCancel" runat="server" Text="Cancel" DialogResult="Cancel"/>
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
