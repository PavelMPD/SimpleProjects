<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FA404000.aspx.cs"
    Inherits="Page_FA404000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.FA.FACostDetailsInq"/>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Selection" DataMember="Filter"
        DefaultControlID="edAssetID" NoteField="">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXSelector CommitChanges="True" ID="edAssetID" runat="server" DataField="AssetID" DataMember="_FixedAsset_" />
            <px:PXSegmentMask CommitChanges="True" ID="edAccountID" runat="server" DataField="AccountID" />
            <px:PXSegmentMask CommitChanges="True" ID="edSubID" runat="server" DataField="SubID" />
            <px:PXMaskEdit CommitChanges="True" ID="edPeriodID" runat="server" DataField="PeriodID" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="Inquire" Caption="Transactions" RestrictFields="True">
        <Levels>
            <px:PXGridLevel DataKeyNames="RefNbr,LineNbr" DataMember="Transactions">
                <Columns>
                    <px:PXGridColumn DataField="RefNbr" Label="Reference Number" />
                    <px:PXGridColumn DataField="TranDate" Label="Tran. Date" Width="90px" />
                    <px:PXGridColumn DataField="FinPeriodID" Label="Tran. Period" />
                    <px:PXGridColumn DataField="TranType" Label="Transaction Type" RenderEditorText="True" />
                    <px:PXGridColumn DataField="TranDesc" Label="Transaction Description" Width="200px" />
                    <px:PXGridColumn AllowNull="False" DataField="DebitAmt" Label="Debit" TextAlign="Right" Width="100px" />
                    <px:PXGridColumn AllowNull="False" DataField="CreditAmt" Label="Credit" TextAlign="Right" Width="100px" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXGrid>
</asp:Content>
