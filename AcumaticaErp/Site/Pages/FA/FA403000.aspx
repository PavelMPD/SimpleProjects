<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FA403000.aspx.cs"
    Inherits="Page_FA403000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.FA.FixedAssetCostEnq"
        PageLoadBehavior="PopulateSavedValues">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="ViewDetails" DependOnGrid="grid" Visible="False" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Selection" DataMember="Filter"
        NoteField="">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXSelector CommitChanges="True" ID="edAssetID" runat="server" DataField="AssetID" DataMember="_FixedAsset_" />
            <px:PXMaskEdit CommitChanges="True" ID="edPeriodID" runat="server" DataField="PeriodID" Size="S"/>
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="Inquire" Caption="Accounts/Subs" RestrictFields="True">
        <Levels>
            <px:PXGridLevel DataKeyNames="AccountID,SubID" DataMember="Amts">
                <Columns>
                    <px:PXGridColumn DataField="AccountID" Label="Account" />
                    <px:PXGridColumn DataField="AcctDescr" Label="Account Description" Width="200px" />
                    <px:PXGridColumn DataField="SubID" Label="Subaccount" Width="120px" />
                    <px:PXGridColumn DataField="SubDescr" Label="Subaccount Description" Width="200px" />
                    <px:PXGridColumn AllowNull="False" DataField="ItdAmt" Label="Incep to Date" TextAlign="Right" Width="100px" />
                    <px:PXGridColumn AllowNull="False" DataField="YtdAmt" Label="Year to Date" TextAlign="Right" Width="100px" />
                    <px:PXGridColumn AllowNull="False" DataField="PtdAmt" Label="Period to Date" TextAlign="Right" Width="100px" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <ActionBar>
            <CustomItems>
                <px:PXToolBarButton Text="View Details">
                    <AutoCallBack Command="ViewDetails" Target="ds" />
                </px:PXToolBarButton>
            </CustomItems>
        </ActionBar>
    </px:PXGrid>
</asp:Content>
