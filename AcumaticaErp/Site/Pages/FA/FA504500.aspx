<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FA504500.aspx.cs"
    Inherits="Page_FA504500" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.FA.AssetGLTransactions">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Process" StartNewGroup="True" CommitChanges="true" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Options" DataMember="Filter">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXSegmentMask CommitChanges="True" runat="server" DataField="AccountID" ID="edAccountID" />
            <px:PXSegmentMask CommitChanges="True" Height="19" runat="server" DataField="SubID" ID="edSubID" />
            <px:PXCheckBox ID="chkShowReconciled" runat="server" DataField="ShowReconciled" CommitChanges="True"/>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXSegmentMask CommitChanges="True" Height="19px" ID="edBranchID" runat="server" DataField="BranchID" />
            <px:PXSelector CommitChanges="True" runat="server" DataField="Custodian" ID="edCustodian" />
            <px:PXSelector CommitChanges="True" runat="server" DataField="Department" ID="edDepartment" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
  <px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="300" SkinID="Horizontal" Height="300px">
      <AutoSize Enabled="true" Container="Window"/>
      <Template1>
                <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100; left: 0px; top: 0px; height: 188px;" Width="100%"
                    SkinID="Inquire" Caption="GL Transactions" AdjustPageSize="Auto" AllowPaging="True" SyncPosition="True">
                    <Levels>
                        <px:PXGridLevel DataMember="GLTransactions">
                            <RowTemplate>
                                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                <px:PXSelector ID="edClassID" runat="server" DataField="ClassID" />
                                <px:PXLabel ID="lblLocationIDH" runat="server"></px:PXLabel>
                                <px:PXSelector ID="edCustodian" runat="server" DataField="Custodian" />
                                <px:PXSelector ID="edDepartment" runat="server" DataField="Department" />
                            </RowTemplate>
                            <Columns>
                                <px:PXGridColumn AllowCheckAll="True" DataField="Selected" Label="Selected" TextAlign="Center" Type="CheckBox" Width="60px"
                                    AutoCallBack="True" />
                                <px:PXGridColumn DataField="ClassID" Label="Asset Class" AutoCallBack="True" />
                                <px:PXGridColumn DataField="Reconciled" TextAlign="Center" Type="CheckBox" Width="60px" AutoCallBack="True"/>
                                <px:PXGridColumn DataField="BranchID" DisplayFormat="&gt;AAAAAA" Label="Location" AutoCallBack="True" />
                                <px:PXGridColumn DataField="Custodian" Label="Custodian" AutoCallBack="True" />
                                <px:PXGridColumn DataField="Department" Label="Department" AutoCallBack="True" />
                                <px:PXGridColumn AllowUpdate="False" DataField="GLTranBranchID" DisplayFormat="&gt;AAAAAAAAAA" Label="Transaction Branch" />
                                <px:PXGridColumn AllowUpdate="False" DataField="GLTranInventoryID" DisplayFormat="&gt;AAAAAAAAAAAA" Label="Inventory ID" />
                                <px:PXGridColumn DataField="GLTranUOM" DisplayFormat="&gt;aaaaaa" Label="UOM" />
                                <px:PXGridColumn AllowUpdate="False" DataField="SelectedQty" Label="Selected Quantity" TextAlign="Right" Width="100px" />
                                <px:PXGridColumn AllowUpdate="False" DataField="SelectedAmt" Label="Selected Amount" TextAlign="Right" Width="100px" />
                                <px:PXGridColumn AllowUpdate="False" DataField="OpenQty" Label="Open Quantity" TextAlign="Right" Width="100px" />
                                <px:PXGridColumn AllowUpdate="False" DataField="OpenAmt" Label="Open Amount" TextAlign="Right" Width="100px" />
                                <px:PXGridColumn AllowUpdate="False" DataField="GLTranQty" Label="Quantity" TextAlign="Right" Width="100px" />
                                <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="UnitCost" Label="Unit Cost" TextAlign="Right" Width="100px" />
                                <px:PXGridColumn AllowUpdate="False" DataField="GLTranAmt" Label="Amount" TextAlign="Right" Width="100px" />
                                <px:PXGridColumn AllowUpdate="False" DataField="GLTranDate" Label="Transaction Date" Width="90px" />
                                <px:PXGridColumn DataField="GLTranModule" Label="Module" />
                                <px:PXGridColumn DataField="GLTranBatchNbr" Label="Batch Number" />
                                <px:PXGridColumn AllowNull="False" DataField="GLTranRefNbr" Label="Ref. Number" />
                                <px:PXGridColumn DataField="GLTranReferenceID" Label="Customer/Vendor" />
                                <px:PXGridColumn DataField="GLTranDesc" Label="Transaction Description" Width="200px" />
                            </Columns>
                        </px:PXGridLevel>
                    </Levels>
                    <AutoSize Enabled="True" />
                    <AutoCallBack Command="Refresh" Target="gridSplit"/>
                    <Mode AllowAddNew="False" AllowDelete="False" />
                    <ActionBar PagerVisible="False">
                    </ActionBar>
                </px:PXGrid>
            </Template1>
            <Template2>
                <px:PXGrid ID="gridSplit" runat="server" DataSourceID="ds" Style="z-index: 100; left: 0px; top: 0px; height: 280px;" Width="100%"
                    SkinID="Details" Caption="Transaction Split Details" AdjustPageSize="Auto" AllowPaging="True">
                    <Levels>
                        <px:PXGridLevel DataMember="FATransactions">
                            <RowTemplate>
                                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                <px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" Enabled="False" />
                                <px:PXNumberEdit ID="edLineNbr" runat="server" DataField="LineNbr" />
                                <px:PXSelector ID="edBookID" runat="server" DataField="BookID" />
                                <px:PXSelector CommitChanges="True" ID="edClassID1" runat="server" DataField="ClassID" />
                                <px:PXSelector CommitChanges="True" ID="edTargetAssetID" runat="server" DataField="TargetAssetID" />
                                <px:PXLabel ID="lblLocationIDH1" runat="server"></px:PXLabel>
                                <px:PXSelector ID="edCustodian1" runat="server" DataField="Custodian" />
                                <px:PXSelector CommitChanges="True" ID="edDepartment1" runat="server" DataField="Department" />
                            </RowTemplate>
                            <Columns>
                                <px:PXGridColumn DataField="NewAsset" Label="New Asset" TextAlign="Center" Type="CheckBox" Width="65px" AutoCallBack="True" />
                                <px:PXGridColumn DataField="Component" Label="Component" TextAlign="Center" Type="CheckBox" Width="60px" AutoCallBack="True" />
                                <px:PXGridColumn DataField="ClassID" Label="Asset Class" AutoCallBack="True" Width="80px" />
                                <px:PXGridColumn AutoCallBack="True" DataField="AssetCD" Label="Asset ID" Width="80px" />
                                <px:PXGridColumn AllowNull="False" DataField="Qty" Label="Quantity" TextAlign="Right" Width="60px" />
                                <px:PXGridColumn DataField="TargetAssetID" Label="Target Asset" AutoCallBack="True" />
                                <px:PXGridColumn DataField="BranchID" DisplayFormat="&gt;AAAAAA" Label="Location" />
                                <px:PXGridColumn DataField="Custodian" Label="Custodian" />
                                <px:PXGridColumn DataField="Department" Label="Department" Width="90px"/>
                                <px:PXGridColumn DataField="TranType" Label="Transaction Type" RenderEditorText="True" Width="100px" />
                                <px:PXGridColumn AutoCallBack="True" DataField="ReceiptDate" Width="90px" />
                                <px:PXGridColumn DataField="DeprFromDate" Width="90px" />
                                <px:PXGridColumn DataField="TranDate" Label="Tran. Date" Width="90px" AutoCallBack="True" />
                                <px:PXGridColumn DataField="FinPeriodID" DisplayFormat="##-####" Label="Tran. Period" Width="85px" />
                                <px:PXGridColumn AllowNull="False" DataField="TranAmt" Label="Transaction Amount" TextAlign="Right" Width="110px" AutoCallBack="True" />
                                <px:PXGridColumn DataField="TranDesc" Label="Description" Width="150px" />
                            </Columns>
                        </px:PXGridLevel>
                    </Levels>
                    <AutoSize Enabled="True" />
                    <Mode InitNewRow="True" AutoInsert="True" />
                    <ActionBar PagerVisible="False"/>
                    <CallbackCommands>
                        <Refresh CommitChanges="True" />
                    </CallbackCommands>
                    <Parameters>
                        <px:PXSyncGridParam ControlID="grid" />
                    </Parameters>
                </px:PXGrid>
             </Template2>
        </px:PXSplitContainer>
</asp:Content>
