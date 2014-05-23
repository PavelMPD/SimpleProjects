<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="DR501000.aspx.cs"
    Inherits="Page_DR501000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.DR.DRRecognition" PrimaryView="Filter"
        BorderStyle="NotSet">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Process" StartNewGroup="True" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="ViewSchedule" DependOnGrid="grid" Visible="false" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXDateTimeEdit CommitChanges="True" runat="server" DataField="RecDate" ID="edRecDate" />
            <px:PXSelector CommitChanges="True" runat="server" DataField="DeferredCode" ID="edDeferredCode" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="Inquire" Caption="Deferred Transactions"
        AllowPaging="True" AdjustPageSize="Auto">
        <Levels>
            <px:PXGridLevel DataMember="Items">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXCheckBox ID="chkSelected" runat="server" DataField="Selected" />
                    <px:PXNumberEdit ID="edScheduleID" runat="server" DataField="ScheduleID" Enabled="False" />
                    <px:PXNumberEdit ID="edLineNbr" runat="server" DataField="LineNbr" Enabled="False" />
                    <px:PXDateTimeEdit ID="edRecDate" runat="server" DataField="RecDate" Enabled="False" />
                    <px:PXNumberEdit ID="edAmount" runat="server" DataField="Amount" Enabled="False" />
                    <px:PXSegmentMask ID="edAccountID" runat="server" DataField="AccountID" Enabled="False" />
                    <px:PXMaskEdit ID="edFinPeriodID" runat="server" DataField="FinPeriodID" Enabled="False" />
                    <px:PXTextEdit ID="edDefCode" runat="server" AllowNull="False" DataField="DefCode" Enabled="False" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn AllowCheckAll="true" AllowNull="False" DataField="Selected" Width="37px" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn AllowUpdate="False" DataField="DocType" Width="63px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="ScheduleID" TextAlign="Right" Width="72px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="ComponentCD" TextAlign="Left" Width="90px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="LineNbr" TextAlign="Right" Width="54px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="RecDate" Width="90px" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="Amount" TextAlign="Right" Width="81px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="AccountID" Width="81px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="FinPeriodID" Width="63px" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="DefCode" Width="90px" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="BranchID" Width="90px" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <ActionBar DefaultAction="cmdViewSchedule">
            <CustomItems>
                <px:PXToolBarButton Text="View Schedule" Key="cmdViewSchedule">
                    <AutoCallBack Command="ViewSchedule" Target="ds"/>
                </px:PXToolBarButton>
            </CustomItems>
        </ActionBar>
    </px:PXGrid>
</asp:Content>
