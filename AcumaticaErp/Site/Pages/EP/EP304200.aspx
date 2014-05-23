<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="EP304200.aspx.cs"
    Inherits="Page_EP304200" Title="Changeset Maintenance" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Changeset" TypeName="PX.Objects.EP.ChangesetMaint">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Save" PopupVisible="true" ClosePopup="true" />
            <px:PXDSCallbackCommand Name="Cancel" PopupVisible="true" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="frm" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Changeset" Caption="Changeset"
        NoteIndicator="True" FilesIndicator="True" TabIndex="19500">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXTextEdit ID="edTaskID" runat="server" DataField="TaskID" />
            <px:PXDateTimeEdit ID="edStartDate" runat="server" DataField="StartDate" />
            <px:PXSelector ID="edCreatedByID" runat="server" DataField="CreatedByID" Enabled="False" TextField="Username" 
                DataSourceID="ds" />
            <pxa:PXRefNoteSelector ID="edRefNoteID" runat="server" DataField="Source" NoteIDDataField="RefNoteID" MaxValue="0"
                MinValue="0" >
                <LookupButton CommandName="" CommandSourceID="">
                </LookupButton>
                <LookupPanel DataMember="" DataSourceID="" IDDataField="" TypeDataField="" />
                <EditButton CommandName="" CommandSourceID="">
                </EditButton>
            </pxa:PXRefNoteSelector>
            <px:PXTextEdit ID="edSourceDescription" runat="server" DataField="Source_Description" SkinID="Label" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="server">
    <px:PXGrid ID="grdDetails" runat="server" DataSourceID="ds" Height="323px" Caption="Fields" Width="100%" AllowSearch="True"
        ActionsPosition="Top" AllowPaging="true" AdjustPageSize="Auto" SkinID="Details" MatrixMode="true">
        <ActionBar>
            <Actions>
                <AddNew Enabled="False" />
                <Delete Enabled="false" />
                <EditRecord Enabled="false" />
            </Actions>
        </ActionBar>
        <Levels>
            <px:PXGridLevel DataMember="Details">
                <Columns>
                    <px:PXGridColumn DataField="name" Width="60px" />
                    <px:PXGridColumn DataField="oldValueString" Width="270px" />
                    <px:PXGridColumn DataField="newValueString" Width="270px" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Enabled="True" MinHeight="100" Container="Window" />
        <Mode AllowAddNew="false" AllowDelete="false" AllowUpdate="false" />
    </px:PXGrid>
</asp:Content>
