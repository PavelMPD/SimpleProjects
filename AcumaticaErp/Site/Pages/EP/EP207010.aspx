<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="EP207010.aspx.cs"
    Inherits="Page_EP207010" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="PayGroups" TypeName="PX.Objects.EP.EPPayGroupMaint" Visible="True">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowPaging="True" AllowSearch="true"
        AdjustPageSize="Auto" DataSourceID="ds" SkinID="Primary">
        <Levels>
            <px:PXGridLevel DataMember="PayGroups">
                <Columns>
                    <px:PXGridColumn DataField="PayGroupID" Label="Pay Group ID" Width="100px" />
                    <px:PXGridColumn DataField="Description" Label="Description" Width="200px" />
                    <px:PXGridColumn AllowNull="False" DataField="PayPeriod" Label="Pay Frequency" RenderEditorText="True" Width="100px" />
                    <px:PXGridColumn DataField="HoursPerPayPeriod" Label="Standard Units per Pay Period" TextAlign="Right" Width="160px" />
                    <px:PXGridColumn DataField="WorkHoursPerDay" Width="110px" TextAlign="Right" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
    </px:PXGrid>
</asp:Content>
