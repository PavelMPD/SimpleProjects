<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="EP501000.aspx.cs"
    Inherits="Page_EP501000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" Visible="True" PrimaryView="EPDocumentList" TypeName="PX.Objects.EP.EPDocumentRelease">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Process" CommitChanges="true" StartNewGroup="True" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowPaging="True" ActionsPosition="Top"
        AutoAdjustColumns="True" AllowSearch="True" DataSourceID="ds" SkinID="Inquire" Caption="Expense Claims">
        <Levels>
            <px:PXGridLevel DataMember="EPDocumentList">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXCheckBox ID="chkSelected" runat="server" DataField="Selected" />
                    <px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" Enabled="False" AllowEdit="True" />
                    <px:PXDropDown ID="edStatus" runat="server" DataField="Status" Enabled="False" />
                    <px:PXDateTimeEdit ID="edDocDate" runat="server" DataField="DocDate" Enabled="False" />
                    <px:PXSelector ID="edCuryID" runat="server" DataField="CuryID" Enabled="False" />
                    <px:PXSegmentMask ID="edEmployeeID" runat="server" DataField="EmployeeID" Enabled="False" AllowEdit="True" />
                    <px:PXDateTimeEdit ID="edApproveDate" runat="server" DataField="ApproveDate" Enabled="False" />
                    <px:PXSelector ID="edApproverID" runat="server" DataField="ApproverID" Enabled="False" TextField="Username" />
                    <px:PXSelector ID="edDepartmentID" runat="server" DataField="DepartmentID" Enabled="False" />
                    <px:PXSegmentMask ID="edBranchID" runat="server" DataField="BranchID" Enabled="False" />
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXTextEdit ID="edDocDesc" runat="server" DataField="DocDesc" Enabled="False" />
                    <px:PXNumberEdit ID="edCuryOrigDocAmt" runat="server" DataField="CuryOrigDocAmt" Enabled="False" />
                    <px:PXTextEdit ID="edEPEmployee_acctName" runat="server" DataField="EmployeeID_EPEmployee_acctName" Enabled="False" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn AllowCheckAll="True" DataField="Selected" TextAlign="Center" Type="CheckBox" Width="20px" />
                    <px:PXGridColumn  DataField="RefNbr" Width="108px" />
                    <px:PXGridColumn  DataField="Status" Width="108px" RenderEditorText="True" />
                    <px:PXGridColumn  DataField="DocDate" Width="90px" />
                    <px:PXGridColumn  DataField="CuryID" Width="54px" />
                    <px:PXGridColumn  DataField="CuryOrigDocAmt" TextAlign="Right" Width="81px" />
                    <px:PXGridColumn  DataField="EmployeeID" Width="81px" RenderEditorText="True" />
                    <px:PXGridColumn  DataField="EmployeeID_EPEmployee_acctName" Width="160px" />
                    <px:PXGridColumn  DataField="DepartmentID" Width="81px" />
                    <px:PXGridColumn  DataField="BranchID" Width="81px" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
    </px:PXGrid>
</asp:Content>
