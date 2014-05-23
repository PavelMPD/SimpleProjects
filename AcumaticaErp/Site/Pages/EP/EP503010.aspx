<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="EP503010.aspx.cs"
    Inherits="Page_EP503010" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.EP.EPApprovalProcess" PrimaryView="Filter">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Process" CommitChanges="true" StartNewGroup="True" />
            <px:PXDSCallbackCommand Visible="false" DependOnGrid="grid" Name="Details" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" 
        Caption="Selection">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXLayoutRule runat="server" Merge="True" />
            <px:PXSelector AutoRefresh="True" CommitChanges="True" ID="edOwnerID" runat="server" DataField="OwnerID" DataSourceID="ds" />
            <px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkMyOwner" runat="server" Checked="True" DataField="MyOwner" />
            <px:PXLayoutRule runat="server" />
            <px:PXLayoutRule runat="server" Merge="True" />
            <px:PXSelector AutoRefresh="True" CommitChanges="True" ID="edWorkGroupID" runat="server" DataField="WorkGroupID" DataSourceID="ds" />
            <px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkMyWorkGroup" runat="server" DataField="MyWorkGroup" />
            <px:PXLayoutRule runat="server" />
            <px:PXCheckBox CommitChanges="True" ID="chkMyEscalated" runat="server" DataField="MyEscalated" />
            </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" ActionsPosition="Top"
        Caption="Claims" OnRowDataBound="grid_RowDataBound" SkinID="Inquire">       
        <Levels>            
            <px:PXGridLevel DataMember="Records" >
                <RowTemplate>                    
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn AllowCheckAll="True" AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" Width="20px" />
                    <px:PXGridColumn DataField="DocType" Width = "100px" />
                    <px:PXGridColumn DataField="RefNoteID" Width = "100px" />
                    <px:PXGridColumn DataField="DocDate" Width="100px" />
                    <px:PXGridColumn DataField="BAccountID" Width="120px"/>
                    <px:PXGridColumn DataField="BAccountID_BAccount_acctName" Width="120px" />
                    <px:PXGridColumn DataField="Descr" Width="250px" />
                    <px:PXGridColumn DataField="Details" Width="250px" />
                    <px:PXGridColumn DataField="CreatedDateTime" Width="120px" DisplayFormat="g" />
                    <px:PXGridColumn DataField="CuryID" Width="65px"/>
                    <px:PXGridColumn DataField="CuryTotalAmount" TextAlign="Right" Width="81px" />
                    <px:PXGridColumn DataField="WorkgroupID" Width="120px" />
                    <px:PXGridColumn DataField="OwnerID" Width="100px"  DisplayMode="Text"/>
                </Columns>

<Layout FormViewHeight=""></Layout>
            </px:PXGridLevel>
        </Levels>
        <ActionBar DefaultAction="Details">
            <CustomItems>
                <px:PXToolBarButton Text="Details" Tooltip="View Document" Key="Details">
                    <AutoCallBack Command="Details" Target="ds">
                        <Behavior CommitChanges="True" />
                    </AutoCallBack>
                </px:PXToolBarButton>
            </CustomItems>
        </ActionBar>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <Mode AllowUpdate="False" />
    </px:PXGrid>
</asp:Content>
