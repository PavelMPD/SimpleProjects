<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FA503000.aspx.cs"
    Inherits="Page_FA503000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="Filter" TypeName="PX.Objects.FA.AssetTranRelease" Visible="True">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Process" CommitChanges="true" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Schedule" StartNewGroup="True">
            </px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="viewDocument" DependOnGrid="grid" Visible="False" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" Caption="Options" DataMember="Filter" DefaultControlID="edOrigin">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="XS" ControlSize="SM" />
            <px:PXDropDown CommitChanges="True" runat="server" DataField="Origin" ID="edOrigin" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowPaging="True" AllowSearch="true"
        AdjustPageSize="Auto" DataSourceID="ds" SkinID="Inquire" Caption="Documents" FastFilterFields="RefNbr" SyncPosition="True">
        <Levels>
            <px:PXGridLevel DataMember="FADocumentList">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                    <px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" Enabled="False" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn AllowCheckAll="True" AllowNull="False" DataField="Selected" Label="Selected" TextAlign="Center" Type="CheckBox"
                        Width="60px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="RefNbr" DisplayFormat="&gt;CCCCCCCCCCCCCCC" Label="Register Number" LinkCommand="viewDocument"/>
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="Origin" Label="Origin" RenderEditorText="True" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="Status" Label="Status" RenderEditorText="True" />
                    <px:PXGridColumn AllowUpdate="False" DataField="DocDate" Label="Document Date" Width="90px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="FinPeriodID" DisplayFormat="##-####" Label="Period ID" />
                    <px:PXGridColumn AllowUpdate="False" DataField="DocDesc" Label="Description" Width="200px" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
        <ActionBar PagerVisible="False">
            <CustomItems>
                <px:PXToolBarButton Text="View Document">
                    <AutoCallBack Command="viewDocument" Target="ds" />
                </px:PXToolBarButton>
            </CustomItems>
        </ActionBar>
    </px:PXGrid>
</asp:Content>
