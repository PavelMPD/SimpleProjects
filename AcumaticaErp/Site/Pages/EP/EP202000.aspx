<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="EP202000.aspx.cs"
    Inherits="Page_EP202000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="EmployeeClass" TypeName="PX.Objects.EP.EmployeeClassMaint">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Employee Class" DataMember="EmployeeClass"
        NoteIndicator="True" FilesIndicator="True" ActivityIndicator="True" ActivityField="NoteActivity" DefaultControlID="edVendorClassID" TabIndex="500">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXSelector ID="edVendorClassID" runat="server" DataField="VendorClassID" DataSourceID="ds" />
            <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
            <px:PXSelector ID="edTermsID" runat="server" DataField="TermsID" AllowEdit="True" DataSourceID="ds" />
            <px:PXSelector CommitChanges="True" ID="edPaymentMethodID" runat="server" DataField="PaymentMethodID" AllowEdit="True" DataSourceID="ds" />
            <px:PXSegmentMask CommitChanges="True" ID="edCashAcctID" runat="server" DataField="CashAcctID" AutoRefresh="True"
                DataSourceID="ds" />
            <px:PXSegmentMask CommitChanges="True" ID="edAPAcctID" runat="server" DataField="APAcctID" DataSourceID="ds" />
            <px:PXSegmentMask ID="edAPSubID" runat="server" DataField="APSubID" DataSourceID="ds" />
            <px:PXSegmentMask ID="edDiscTakenAcctID" runat="server" DataField="DiscTakenAcctID" DataSourceID="ds" />
            <px:PXSegmentMask ID="edDiscTakenSubID" runat="server" DataField="DiscTakenSubID" DataSourceID="ds" />
            <px:PXSegmentMask CommitChanges="True" ID="edPrepaymentAcctID" runat="server" DataField="PrepaymentAcctID"
                              DataSourceID="ds" />
            <px:PXSegmentMask ID="edPrepaymentSubID" runat="server" DataField="PrepaymentSubID" DataSourceID="ds" />
            <px:PXSegmentMask CommitChanges="True" ID="edExpenseAcctID" runat="server" DataField="ExpenseAcctID" DataSourceID="ds" />
            <px:PXSegmentMask ID="edExpenseSubID" runat="server" DataField="ExpenseSubID" AllowEdit="True" DataSourceID="ds" />
            <px:PXSegmentMask CommitChanges="True" ID="edSalesAcctID" runat="server" DataField="SalesAcctID" DataSourceID="ds" />
            <px:PXSegmentMask ID="edSalesSubID" runat="server" DataField="SalesSubID" DataSourceID="ds" />
            <px:PXLayoutRule runat="server" Merge="True" />
            <px:PXSelector Size="S" ID="edCuryID" runat="server" DataField="CuryID" AllowEdit="True" DataSourceID="ds" />
            <px:PXCheckBox ID="chkAllowOverrideCury" runat="server" DataField="AllowOverrideCury" />
            <px:PXLayoutRule runat="server" />
            <px:PXLayoutRule runat="server" Merge="True" />
            <px:PXSelector Size="S" ID="edCuryRateTypeID" runat="server" DataField="CuryRateTypeID" AllowEdit="True" DataSourceID="ds" />
            <px:PXCheckBox ID="chkAllowOverrideRate" runat="server" DataField="AllowOverrideRate" />
            <px:PXLayoutRule runat="server" />
            <px:PXSelector ID="edCalendarID" runat="server" DataField="CalendarID" AllowEdit="True" DataSourceID="ds" />
            <px:PXSelector ID="edTaxZoneID" runat="server" DataField="TaxZoneID" DataSourceID="ds" />
            <px:PXDropDown ID="edHoursValidation" runat="server" AllowNull="False" DataField="HoursValidation"/>
        </Template>
        <AutoSize Container="Window" Enabled="True" MinHeight="430" />
    </px:PXFormView>
</asp:Content>
