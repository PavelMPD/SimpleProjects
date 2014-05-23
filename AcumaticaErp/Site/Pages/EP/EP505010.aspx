<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="EP505010.aspx.cs"
    Inherits="Page_EP505010" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.EP.TimeCardRelease" PrimaryView="FilteredItems">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Cancel" />
            <px:PXDSCallbackCommand Name="Process" CommitChanges="true" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="ProcessAll" CommitChanges="true" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" ActionsPosition="Top"
        Caption="Time Cards" SkinID="Inquire">
        <Levels>
            <px:PXGridLevel DataMember="FilteredItems">
                <RowTemplate>
                    <px:PXSelector ID="edRefNbr" runat="server" DataField="TimecardCD" Enabled="False" AllowEdit="True" />
                    <px:PXSelector CommitChanges="True" ID="edEmployee" runat="server" DataField="EmployeeID" />
                    <px:PXLayoutRule ID="PXLayoutRule1" runat="server" GroupCaption="Time" StartColumn="True" StartGroup="True" />
                    <px:PXTimeSpan runat="server" DataField="TimeSpentCalc" ID="RegularTime" Enabled="False" Size="XS" LabelWidth="55" InputMask="hh:mm" MaxHours="99" />
                    <px:PXTimeSpan ID="BillableTime" runat="server" DataField="TimeBillableCalc" Enabled="False" Size="XS" LabelWidth="55" InputMask="hh:mm" MaxHours="99" />
                    <px:PXLayoutRule ID="PXLayoutRule2" runat="server" GroupCaption="Overtime" StartColumn="True" StartGroup="True" />
                    <px:PXTimeSpan runat="server" DataField="OvertimeSpentCalc" ID="OvertimeSpentCalc" Enabled="False" SuppressLabel="True" Size="XS" InputMask="hh:mm" MaxHours="99" />
                    <px:PXTimeSpan ID="BillableOvertime" runat="server" DataField="OvertimeBillableCalc" Enabled="False" SuppressLabel="True" Size="XS" InputMask="hh:mm" MaxHours="99" />
                    <px:PXLayoutRule ID="PXLayoutRule3" runat="server" GroupCaption="Total" StartColumn="True" StartGroup="True" />
                    <px:PXTimeSpan ID="edTimeSpent" runat="server" DataField="TotalSpentCalc" Enabled="false" Size="XS" SuppressLabel="True" InputMask="hh:mm" MaxHours="99" />
                    <px:PXTimeSpan ID="PXMaskEdit1" runat="server" DataField="TotalBillableCalc" Enabled="false" SuppressLabel="True" Size="XS" InputMask="hh:mm" MaxHours="99" />

                </RowTemplate>
                <Columns>
                    <px:PXGridColumn AllowCheckAll="True" AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" Width="20px" />
                    <px:PXGridColumn DataField="TimeCardCD" Width="108px" />
                    <px:PXGridColumn DataField="EmployeeID" Label="Employee ID" Width="108px" />
                    <px:PXGridColumn DataField="EmployeeID_EPEmployee_acctName" Label="Employee Name" Width="108px" />
                    <px:PXGridColumn DataField="WeekID" Label="Week" Width="81px" DisplayMode="Text" />
                    <px:PXGridColumn DataField="TimeSpentCalc" Label="TimeSpentCalc" Width="81px" RenderEditorText="True" />
                    <px:PXGridColumn DataField="OvertimeSpentCalc" Label="OvertimeSpentCalc" Width="81px" RenderEditorText="True" />
                    <px:PXGridColumn DataField="TotalSpentCalc" Label="TotalSpentCalc" Width="81px" RenderEditorText="True" />
                    <px:PXGridColumn DataField="TimeBillableCalc" Label="TimeBillableCalc" Width="81px" RenderEditorText="True" />
                    <px:PXGridColumn DataField="OvertimeBillableCalc" Label="OvertimeBillableCalc" Width="81px" RenderEditorText="True" />
                    <px:PXGridColumn DataField="TotalBillableCalc" Label="TotalBillableCalc" Width="81px" RenderEditorText="True" />
                    <px:PXGridColumn DataField="ApprovedBy" Width="108px" />
                    <px:PXGridColumn DataField="ApproveDate" Width="90px" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXGrid>
</asp:Content>
