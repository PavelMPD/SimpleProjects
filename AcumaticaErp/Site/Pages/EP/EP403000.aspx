<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="EP403000.aspx.cs" Inherits="Pages_EP_EP403000"
    Title="Timecards" %>


<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.EP.TimecardInquiry" PrimaryView="Filter">
        <CallbackCommands>
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Selection" DataMember="Filter"
        DefaultControlID="edProjectID" NoteField="">
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
            <px:PXSelector CommitChanges="True" ID="edFromWeekID" runat="server" DataField="FromWeekID" TextMode="Search" DataSourceID="ds" ValueField="WeekID" DisplayMode="Text"/>
            <px:PXSelector CommitChanges="True" ID="edToWeekID" runat="server" DataField="ToWeekID" TextMode="Search" DataSourceID="ds" ValueField="WeekID" DisplayMode="Text"/>
            <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXSelector CommitChanges="True" ID="edEmployee" runat="server" DataField="EmployeeID" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="Inquire" AdjustPageSize="Auto"
        AllowPaging="True" Caption="Time Cards" FastFilterFields="TimecardCD,Description" RestrictFields="True">
        <Levels>
            <px:PXGridLevel DataMember="Items">
                <RowTemplate>
                    <px:PXSelector ID="edRefNbr" runat="server" DataField="TimecardCD" Enabled="False" AllowEdit="True" />
                    <px:PXSelector CommitChanges="True" ID="edEmployee" runat="server" DataField="EmployeeID" />
                    <px:PXDropDown ID="edStatus" runat="server" DataField="Status" />
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
                    <px:PXGridColumn DataField="EmployeeID" Label="Employee ID" Width="108px" />
                    <px:PXGridColumn DataField="EmployeeID_EPEmployee_acctName" Label="Employee Name" Width="108px" />
                    <px:PXGridColumn DataField="WeekID" Label="Week" Width="81px" DisplayMode="Text" />
                    <px:PXGridColumn DataField="Status" Label="Status" Width="81px" Type="DropDownList" />

                    <px:PXGridColumn DataField="TimeSpentCalc" Label="TimeSpentCalc" Width="81px" RenderEditorText="True" />
                    <px:PXGridColumn DataField="OvertimeSpentCalc" Label="OvertimeSpentCalc" Width="81px" RenderEditorText="True" />
                    <px:PXGridColumn DataField="TotalSpentCalc" Label="TotalSpentCalc" Width="81px" RenderEditorText="True" />
                    <px:PXGridColumn DataField="TimeBillableCalc" Label="TimeBillableCalc" Width="81px" RenderEditorText="True" />
                    <px:PXGridColumn DataField="OvertimeBillableCalc" Label="OvertimeBillableCalc" Width="81px" RenderEditorText="True" />
                    <px:PXGridColumn DataField="TotalBillableCalc" Label="TotalBillableCalc" Width="81px" RenderEditorText="True" />
                    <px:PXGridColumn DataField="BillingRateCalc" Label="BillingRateCalc" Width="81px" />
                    <px:PXGridColumn DataField="TimecardCD" Label="TimecardCD" Width="108px" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
	    <Mode AllowAddNew="False" AllowUpdate="False" AllowDelete="False"/>
    </px:PXGrid>
</asp:Content>
