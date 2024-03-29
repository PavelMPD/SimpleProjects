<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PM502500.aspx.cs" Inherits="Page_PM502500"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" Visible="true" PrimaryView="Filter" TypeName="PX.Objects.PM.AllocationProcessByProject">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="true" Name="Process" StartNewGroup="true" />
            <px:PXDSCallbackCommand CommitChanges="true" Name="ProcessAll" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection">
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXSelector CommitChanges="True" ID="edAllocationID" runat="server" DataField="AllocationID" DataSourceID="ds" />
            <px:PXSegmentMask CommitChanges="True" ID="edProjectID" runat="server" DataField="ProjectID" DataSourceID="ds" />
            <px:PXSelector CommitChanges="True" ID="edCustomerClassID" runat="server" DataField="CustomerClassID" DataSourceID="ds" />
            <px:PXSegmentMask CommitChanges="True" ID="edCustomerID" runat="server" DataField="CustomerID" DataSourceID="ds" />
            <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edDate" runat="server" DataField="Date" />
            <px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartGroup="True" GroupCaption="Date Range Restrictions" />
            <px:PXDateTimeEdit CommitChanges="True" ID="PXDateTimeEdit1" runat="server" DataField="DateFrom" />
            <px:PXDateTimeEdit CommitChanges="True" ID="PXDateTimeEdit2" runat="server" DataField="DateTo" />
            </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" Caption="Projects"
        SkinID="Inquire" AdjustPageSize="Auto" AllowPaging="True">
        <Levels>
            <px:PXGridLevel DataMember="Items">
                <RowTemplate>
                    <px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                    <px:PXCheckBox ID="chkSelected" runat="server" DataField="Selected" />
                    <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" Enabled="False" />
                    <px:PXSelector ID="edAllocationID" runat="server" DataField="AllocationID" Enabled="False" />
                    <px:PXSegmentMask ID="edCustomerID" runat="server" DataField="CustomerID" Enabled="False" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn AllowCheckAll="True" DataField="Selected" Label="Selected" TextAlign="Center" Type="CheckBox" Width="20px" />
                    <px:PXGridColumn DataField="ContractCD" Label="Project ID" Width="108px" />
                    <px:PXGridColumn DataField="Description" Label="Description" Width="200px" />
                    <px:PXGridColumn DataField="AllocationID" Label="Allocation Rule" Width="117px" />
                    <px:PXGridColumn DataField="CustomerID" Label="Customer" Width="108px" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXGrid>
</asp:Content>

