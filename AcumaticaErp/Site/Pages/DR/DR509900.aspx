<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="DR509900.aspx.cs"
    Inherits="Page_DR509900" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" Visible="true" PrimaryView="Items" TypeName="PX.Objects.DR.DRBalanceValidation">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="true" Name="Process" StartNewGroup="true" />
            <px:PXDSCallbackCommand CommitChanges="true" Name="ProcessAll" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowSearch="true" AdjustPageSize="Auto"
        DataSourceID="ds" SkinID="Inquire" Caption="Deferral Types">
        <Levels>
            <px:PXGridLevel DataMember="Items">
                <Columns>
                    <px:PXGridColumn AllowCheckAll="True" AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" Width="60px" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="AccountType" RenderEditorText="True" Width="90px" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
    </px:PXGrid>
</asp:Content>
