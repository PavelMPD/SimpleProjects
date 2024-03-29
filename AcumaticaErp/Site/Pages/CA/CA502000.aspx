<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CA502000.aspx.cs" Inherits="Page_CA502000"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" Visible="True" PrimaryView="CARegisterList" TypeName="PX.Objects.CA.CATrxRelease">
        <CallbackCommands>            
            <px:PXDSCallbackCommand CommitChanges="True" DependOnGrid="grid" Name="ViewCATrx" Visible="false" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100; left: 0px; top: 0px;" AllowPaging="True" ActionsPosition="Top" AllowSearch="true"
        DataSourceID="ds" Caption="Cash Transactions" SkinID="Inquire">
        <Levels>
            <px:PXGridLevel DataMember="CARegisterList">
                <Columns>
                    <px:PXGridColumn AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" Width="60px" AllowCheckAll="True" />
                    <px:PXGridColumn DataField="TranType" Width="120px" />
                    <px:PXGridColumn DataField="ReferenceNbr" />
                    <px:PXGridColumn DataField="CashAccountID" DisplayFormat="&gt;AAAAAAAAAA" Width="108px" />
                    <px:PXGridColumn DataField="CuryID" />
                    <px:PXGridColumn AllowNull="False" DataField="CuryTranAmt" TextAlign="Right" Width="81px" />
                    <px:PXGridColumn DataField="Description" Width="200px" />
                    <px:PXGridColumn DataField="DocDate" Width="90px" />
                    <px:PXGridColumn DataField="FinPeriodID" DisplayFormat="##-####" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
        <ActionBar DefaultAction="cmdViewCATrx">
            <CustomItems>
                <px:PXToolBarButton Text="View Transaction" Key="cmdViewCATrx">
                    <AutoCallBack Command="ViewCATrx" Target="ds" />
                </px:PXToolBarButton>
            </CustomItems>
        </ActionBar>
    </px:PXGrid>
</asp:Content>
