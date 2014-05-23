<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AP502500.aspx.cs"
    Inherits="Page_AP502500" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.AP.APUpdateDiscounts"
        BorderStyle="NotSet">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Process" CommitChanges="true" StartNewGroup="True" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Selection" DataMember="Filter"
        DefaultControlID="edPendingDiscountDate">
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
            <px:PXSelector ID="edVendorID" runat="server" DataField="VendorID" CommitChanges="True"></px:PXSelector>
            <px:PXDateTimeEdit CommitChanges="True" ID="edPendingDiscountDate" runat="server" DataField="PendingDiscountDate" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" AllowPaging="True"
        Caption="Discount Sequences" SkinID="Inquire">
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <Levels>
            <px:PXGridLevel DataMember="Items">
                <RowTemplate>
                    <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                    <px:PXCheckBox ID="chkSelected" runat="server" DataField="Selected" />
                    <px:PXTextEdit ID="edDiscountID" runat="server" DataField="DiscountID" Enabled="False" />
                    <px:PXTextEdit ID="edDiscountSequenceID" runat="server" DataField="DiscountSequenceID" Enabled="False" />
                    <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" Enabled="False" />
                    <px:PXDropDown ID="edDiscountedFor" runat="server" AllowNull="False" DataField="DiscountedFor" Enabled="False" />
                    <px:PXDropDown ID="edBreakBy" runat="server" AllowNull="False" DataField="BreakBy" Enabled="False" SelectedIndex="1" />
                    <px:PXCheckBox ID="chkIsPromotion" runat="server" DataField="IsPromotion" Enabled="False" />
                    <px:PXCheckBox ID="chkIsActive" runat="server" Checked="True" DataField="IsActive" Enabled="False" />
                    <px:PXDateTimeEdit ID="edStartDate" runat="server" DataField="StartDate" Enabled="False" />
                    <px:PXDateTimeEdit ID="edEndDate" runat="server" DataField="EndDate" Enabled="False" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="Selected" AllowCheckAll="true" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn AllowUpdate="False" DataField="DiscountID" Width="63px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="DiscountSequenceID" Width="63px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="Description" Width="500px" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="DiscountedFor" Width="90px" RenderEditorText="true" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="BreakBy" Width="63px" RenderEditorText="true" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="IsActive" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn AllowUpdate="False" DataField="StartDate" Width="90px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="EndDate" Width="120px" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
    </px:PXGrid>
</asp:Content>
