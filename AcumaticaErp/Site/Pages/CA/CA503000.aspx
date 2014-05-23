<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CA503000.aspx.cs" Inherits="Page_CA503000"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" Visible="True" PrimaryView="CABalValidateList" TypeName="PX.Objects.CA.CABalValidate">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="true" Name="Process" StartNewGroup="true" />
            <px:PXDSCallbackCommand CommitChanges="true" Name="ProcessAll" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" AllowPaging="True" ActionsPosition="Top" AllowSearch="true" DataSourceID="ds" SkinID="Inquire" Caption="Cash Accounts"
        FastFilterFields="AccountCD">
        <Levels>
            <px:PXGridLevel DataMember="CABalValidateList">
                <Columns>
                    <px:PXGridColumn AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" Width="60px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="CashAccountCD" DisplayFormat="&gt;AAAA" />
                    <px:PXGridColumn AllowUpdate="False" DataField="Descr" Width="200px" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
        <ActionBar>
            <Actions>
                <Save Enabled="False" />
                <EditRecord Enabled="False" />
                <Delete Enabled="False" />
                <Search Text="Find" />
            </Actions>
        </ActionBar>
    </px:PXGrid>
</asp:Content>
