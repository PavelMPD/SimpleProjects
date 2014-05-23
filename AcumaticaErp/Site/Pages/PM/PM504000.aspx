<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="PM504000.aspx.cs" Inherits="Page_PM504000"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" TypeName="PX.Objects.PM.ProjectBalanceValidation"
        PrimaryView="Items" Visible="true">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="true" Name="Process" StartNewGroup="true" />
            <px:PXDSCallbackCommand CommitChanges="true" Name="ProcessAll" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100"
        AllowPaging="True" AllowSearch="true" AdjustPageSize="Auto" DataSourceID="ds"
        SkinID="Inquire" Caption="Projects">
        <Levels>
            <px:PXGridLevel DataKeyNames="ContractCD" DataMember="Items">
                <Columns>
                    <px:PXGridColumn AllowCheckAll="true" DataField="Selected" Label="Selected" TextAlign="Center" Type="CheckBox" Width="60px" />
                    <px:PXGridColumn DataField="ContractCD" Width="108px"/>
                    <px:PXGridColumn DataField="Description" Label="Description" Width="400px" />
                    <px:PXGridColumn DataField="CustomerID" Label="Customer" Width="108px"/>
                    <px:PXGridColumn DataField="Status" Label="Status" RenderEditorText="True" Width="108px"/>
                    <px:PXGridColumn DataField="StartDate" Label="Start Date" Width="90px" />
                    <px:PXGridColumn DataField="ExpireDate" Label="End Date" Width="90px" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
    </px:PXGrid>
</asp:Content>
