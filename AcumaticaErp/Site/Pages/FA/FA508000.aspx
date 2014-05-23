<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FA508000.aspx.cs"
    Inherits="Page_FA508000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="Docs" TypeName="PX.Objects.FA.DeleteDocsProcess" Visible="True">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="ViewDocument" DependOnGrid="grid" Visible="False" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowPaging="True" AllowSearch="true"
        AdjustPageSize="Auto" DataSourceID="ds" SkinID="Inquire" Caption="Unreleased FA Documents" FastFilterFields="RefNbr">
        <Levels>
            <px:PXGridLevel DataKeyNames="RefNbr" DataMember="Docs">
                <Columns>
                    <px:PXGridColumn AllowCheckAll="True" AllowNull="False" DataField="Selected" Label="Selected" TextAlign="Center" Type="CheckBox"
                        Width="60px" />
                    <px:PXGridColumn DataField="RefNbr" Label="Reference Number" LinkCommand="ViewDocument" />
                    <px:PXGridColumn DataField="DocDate" Label="Document Date" Width="90px" />
                    <px:PXGridColumn AllowNull="False" DataField="Origin" Label="Origin" RenderEditorText="True" />
                    <px:PXGridColumn DataField="DocDesc" Label="Description" Width="200px" />
                    <px:PXGridColumn AllowNull="False" DataField="Hold" Label="On Hold" TextAlign="Center" Type="CheckBox" Width="60px" />
                    <px:PXGridColumn AllowNull="False" DataField="IsEmpty" Label="Empty" TextAlign="Center" Type="CheckBox" Width="60px" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
    </px:PXGrid>
</asp:Content>
