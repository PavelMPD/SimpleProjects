<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="DR401000.aspx.cs"
    Inherits="Page_DR401000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" BorderStyle="NotSet" PrimaryView="Filter"
        TypeName="PX.Objects.DR.SchedulesInq">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="ViewSchedule" PostData="Self" DependOnGrid="grid" Visible="false" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXDropDown CommitChanges="True" ID="edAccountType" runat="server" DataField="AccountType" Size="S" />
            <px:PXSegmentMask CommitChanges="True" ID="edAccountID" runat="server" DataField="AccountID" />
            <px:PXSegmentMask CommitChanges="True" ID="edSubID" runat="server" DataField="SubID" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXSelector CommitChanges="True" ID="edDeferredCode" runat="server" DataField="DeferredCode" />
            <px:PXSelector CommitChanges="True" ID="edBAccountID" runat="server" DataField="BAccountID" AutoRefresh="true" />
            <px:PXSegmentMask CommitChanges="True" ID="edComponentID" runat="server" DataField="ComponentID" AllowEdit="true" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="Inquire" Caption="Deferral Schedules"
        AllowPaging="True" RestrictFields="True">
        <Levels>
            <px:PXGridLevel DataMember="Records">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXNumberEdit ID="edScheduleID" runat="server" DataField="ScheduleID" Enabled="False" />
                    <px:PXDropDown ID="edStatus" runat="server" DataField="Status" Enabled="False" />
                    <px:PXTextEdit ID="edRefNbr" runat="server" DataField="RefNbr" />
                    <px:PXNumberEdit ID="edLineNbr" runat="server" DataField="LineNbr" />
                    <px:PXDateTimeEdit ID="edDocDate" runat="server" DataField="DocDate" />
                    <px:PXNumberEdit ID="edTotalAmt" runat="server" DataField="TotalAmt" />
                    <px:PXNumberEdit ID="edDefAmt" runat="server" DataField="DefAmt" />
                    <px:PXTextEdit ID="edDefCode" runat="server" DataField="DefCode" />
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXSegmentMask ID="edDefAcctID" runat="server" DataField="DefAcctID" />
                    <px:PXSegmentMask ID="edDefSubID" runat="server" DataField="DefSubID" />
                    <px:PXSegmentMask ID="edAccountID" runat="server" DataField="AccountID" />
                    <px:PXSegmentMask ID="edSubID" runat="server" DataField="SubID" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="ScheduleID" TextAlign="Right" Width="72px" />
                    <px:PXGridColumn DataField="DocumentType" Width="54px" />
                    <px:PXGridColumn DataField="RefNbr" Width="90px" />
                    <px:PXGridColumn DataField="ComponentCD" Width="144px" />
                    <px:PXGridColumn DataField="LineNbr" TextAlign="Right" Width="54px" />
                    <px:PXGridColumn DataField="DocDate" Width="90px" />
                    <px:PXGridColumn DataField="Status" RenderEditorText="True" Width="54px" />
                    <px:PXGridColumn DataField="TotalAmt" TextAlign="Right" Width="63px" />
                    <px:PXGridColumn DataField="DefAmt" TextAlign="Right" Width="63px" />
                    <px:PXGridColumn DataField="DefCode" Width="63px" />
                    <px:PXGridColumn DataField="DefAcctID" Width="81px" />
                    <px:PXGridColumn DataField="DefSubID" Width="108px" />
                    <px:PXGridColumn DataField="AccountID" Width="81px" />
                    <px:PXGridColumn DataField="SubID" Width="108px" />
                    <px:PXGridColumn DataField="BAccountID" Width="90px" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <Mode AllowAddNew="False" AllowDelete="False" />
        <ActionBar DefaultAction="cmdViewSchedule">
            <CustomItems>
                <px:PXToolBarButton Text="View Schedule" Key="cmdViewSchedule">
                    <AutoCallBack Command="ViewSchedule" Target="ds" />
                </px:PXToolBarButton>
            </CustomItems>
        </ActionBar>
    </px:PXGrid>
</asp:Content>
