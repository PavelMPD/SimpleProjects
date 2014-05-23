<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FA502000.aspx.cs"
    Inherits="Page_FA502000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.FA.CalcDeprProcess" PrimaryView="Filter">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="calculate" StartNewGroup="True" CommitChanges="True" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="calculateAll" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="calcDepr" StartNewGroup="True" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="calcDeprAll" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Process" StartNewGroup="True" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="ProcessAll" />
            <px:PXDSCallbackCommand Name="ViewAsset" DependOnGrid="grid" Visible="False" />
            <px:PXDSCallbackCommand DependOnGrid="grid" Name="ViewBook" Visible="False" />
            <px:PXDSCallbackCommand DependOnGrid="grid" Name="ViewClass" Visible="False" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" DefaultControlID="edPeriodID"
        Caption="Parameters" NoteField="">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXMaskEdit CommitChanges="True" runat="server" InputMask="##-####" DataField="PeriodID" ID="edPeriodID" />
            <px:PXSegmentMask CommitChanges="True" Height="19px" ID="edBranchID" runat="server" DataField="BranchID" />
            <px:PXSelector CommitChanges="True" runat="server" DataField="ClassID" ID="edClassID" />
            <px:PXSelector CommitChanges="True" ID="edParentAssetID" runat="server" DataField="ParentAssetID" DataMember="_FixedAsset_" />
            <px:PXSelector CommitChanges="True" runat="server" DataField="BookID" ID="edBookID" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100; left: 0px; top: 0px;" Width="100%" Height="150px"
        SkinID="Inquire" Caption="Assets to Process" AdjustPageSize="Auto" AllowPaging="True" FastFilterFields="AssetID">
        <Levels>
            <px:PXGridLevel DataMember="Balances">
                <Columns>
                    <px:PXGridColumn DataField="Selected" Label="Selected" TextAlign="Center" Type="CheckBox" Width="40px" AllowCheckAll="True" />
                    <px:PXGridColumn DataField="FixedAsset__BranchID" Label="Fixed Asset-Branch" />
                    <px:PXGridColumn AllowUpdate="False" DataField="AssetID" Label="Asset ID" LinkCommand="ViewAsset" />
                    <px:PXGridColumn DataField="FixedAsset__Description" Label="Description" Width="200px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="ClassID" Label="Class ID" LinkCommand="ViewClass" />
                    <px:PXGridColumn DataField="FixedAsset__ParentAssetID" Label="Parent Asset" />
                    <px:PXGridColumn AllowUpdate="False" DataField="BookID" Label="Book" LinkCommand="ViewBook" />
                    <px:PXGridColumn AllowUpdate="False" DataField="CurrDeprPeriod" DisplayFormat="##-####" Label="Current Depr. Period" />
                    <px:PXGridColumn AllowNull="False" DataField="YtdDeprBase" Label="Basis" TextAlign="Right" Width="100px" />
                    <px:PXGridColumn DataField="FADetails__ReceiptDate" Label="Receipt Date" Width="90px" />
                    <px:PXGridColumn DataField="FixedAsset__UsefulLife" Label="Fixed Asset-Useful Life, Years" TextAlign="Right" Width="100px" />
                    <px:PXGridColumn DataField="FixedAsset__FAAccountID" Label="Fixed Assets Account" />
                    <px:PXGridColumn DataField="FixedAsset__FASubID" Label="Fixed Assets Sub." />
                    <px:PXGridColumn DataField="FADetails__TagNbr" Label="Tag Number" Width="80px" />
                    <px:PXGridColumn DataField="Account__AccountClassID" Label="Account Class" Width="80px" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <ActionBar PagerVisible="False"/>
    </px:PXGrid>
</asp:Content>
