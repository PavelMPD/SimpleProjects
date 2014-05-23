<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CA506500.aspx.cs" Inherits="Page_CA506500"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="filter" PageLoadBehavior="PopulateSavedValues" TypeName="PX.Objects.CA.PaymentReclassifyProcess">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Cancel" RepaintControls="All" />
            <px:PXDSCallbackCommand Name="Process" StartNewGroup="true" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="ProcessAll" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="ViewResultDocument" DependOnGrid="grid" Visible="false" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" Caption="Selection" DataMember="filter">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="m" ControlSize="XXL" />
            <px:PXLayoutRule runat="server" Merge="True" />
            <px:PXSelector CommitChanges="True" runat="server" DataField="EntryTypeID" ID="edEntryTypeID" Size="m" />
            <px:PXCheckBox CommitChanges="True" runat="server" SuppressLabel="true" DataField="ShowReclassified" ID="chkShowReclassified" />
            <px:PXLayoutRule runat="server" Merge="False" />
            <px:PXSegmentMask CommitChanges="True" runat="server" DataField="AccountID" ID="edAccountID" AutoRefresh="True" Size="m" />
            <px:PXCheckBox CommitChanges="True" runat="server" Checked="True" DataField="IncludeUnreleased" ID="chkIncludeUnreleased" />
            <px:PXSelector CommitChanges="True" runat="server" DataField="CuryID" Enabled="False" ID="edCuryID" Size="xxs" />
            <px:PXDateTimeEdit CommitChanges="True" runat="server" DataField="StartDate" ID="edStartDate" Size="s" />
            <px:PXDateTimeEdit CommitChanges="True" runat="server" DataField="EndDate" ID="edEndDate" Size="s" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" ActionsPosition="Top" Caption="Transactions" Style="z-index: 100; left: 0px; top: 0px; height: 236px;" Width="100%"
        SkinID="Details">
        <Levels>
            <px:PXGridLevel DataMember="Adjustments">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="L" ControlSize="L" />
                    <px:PXLayoutRule runat="server" Merge="True" />
                    <px:PXCheckBox ID="chkSelected" runat="server" DataField="Selected" />
                    <px:PXSelector ID="edPMInstanceID" runat="server" AutoRefresh="True" DataField="PMInstanceID" TextField="Descr">
                        <Parameters>
                            <px:PXSyncGridParam ControlID="grid" />
                        </Parameters>
                    </px:PXSelector>
                    <px:PXLayoutRule runat="server" Merge="False" />
                    <px:PXDropDown ID="edAdjTranType" runat="server" DataField="AdjTranType" Enabled="False" />
                    <px:PXSelector ID="edAdjRefNbr" runat="server" DataField="AdjRefNbr" AllowEdit="true" Enabled="False" />
                    <px:PXDropDown ID="edDrCr" runat="server" AllowNull="False" DataField="DrCr" Enabled="False" />
                    <px:PXTextEdit ID="edExtRefNbr" runat="server" DataField="ExtRefNbr" Enabled="False" />
                    <px:PXSegmentMask ID="edCashAccountID" runat="server" DataField="CashAccountID" Enabled="False" />
                    <px:PXDateTimeEdit ID="edTranDate" runat="server" DataField="TranDate" Enabled="False" />
                    <px:PXSelector ID="edCuryID" runat="server" DataField="CuryID" Enabled="False" />
                    <px:PXSelector ID="edFinPeriodID" runat="server" DataField="FinPeriodID" Enabled="False" />
                    <px:PXSegmentMask ID="edAccountID" runat="server" DataField="AccountID" Enabled="False"/>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="L" ControlSize="L" />
                    <px:PXSegmentMask ID="edSubID" runat="server" DataField="SubID" Enabled="False" />
                    <px:PXTextEdit ID="edTranDesc" runat="server" DataField="TranDesc" Enabled="False" />
                    <px:PXNumberEdit ID="edCuryTranAmt" runat="server" AllowNull="False" DataField="CuryTranAmt" Enabled="False" />
                    <px:PXNumberEdit ID="edTranAmt" runat="server" AllowNull="False" DataField="TranAmt" Enabled="False" />
                    <px:PXCheckBox ID="chkCleared" runat="server" DataField="Cleared" Enabled="False" />
                    <px:PXDropDown ID="edOrigModule" runat="server" AllowNull="False" DataField="OrigModule" />
                    <px:PXSelector ID="edReferenceID" runat="server" DataField="ReferenceID" AutoRefresh="true">
                        <Parameters>
                            <px:PXSyncGridParam ControlID="grid" />
                        </Parameters>
                    </px:PXSelector>
                    <px:PXSegmentMask ID="edLocationID" runat="server" DataField="LocationID" AutoRefresh="true">
                        <Parameters>
                            <px:PXSyncGridParam ControlID="grid" />
                        </Parameters>
                    </px:PXSegmentMask>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="L" ControlSize="L" />
                    <px:PXSelector ID="edPaymentMethodID" runat="server" DataField="PaymentMethodID" AutoRefresh="true">
                        <Parameters>
                            <px:PXSyncGridParam ControlID="grid" />
                        </Parameters>
                    </px:PXSelector>
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="Selected" Label="Selected" TextAlign="Center" Type="CheckBox" AutoCallBack="true" AllowCheckAll="true" Width="40px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="CashAccountID" DisplayFormat="&gt;######" Label="Cash Account" Width="92px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="ExtRefNbr" Label="Document Ref." Width="90px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="TranDate" Label="Tran. Date" Width="72px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="AdjRefNbr" Label="Ref. Nbr" Width="90px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="CuryID" DisplayFormat="&gt;LLLLL" Label="Currency" Width="54px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="DrCr" Label="Disb. / Receipt" RenderEditorText="True" Width="90px" AllowNull="False" Visible="False" />
                    <px:PXGridColumn AllowUpdate="False" DataField="FinPeriodID" Label="Fin. Period" Visible="False" DisplayFormat="##-####" Width="63px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="ReclassCashAccountID" Label="Offset Account" Width="92px" Visible="true"/>                    
                    <px:PXGridColumn AllowUpdate="False" DataField="AccountID" DisplayFormat="&gt;######" Label="Offset Account" Width="92px" Visible="False">
                        <Header Text="Offset Account"></Header>
                    </px:PXGridColumn>
                    <px:PXGridColumn AllowUpdate="False" DataField="SubID" DisplayFormat="&gt;AA-AA-AA-AA-AAA" Label="Offset Subaccount" Width="108px" Visible="False">
                        <Header Text="Offset Subaccount"></Header>
                    </px:PXGridColumn>
                    <px:PXGridColumn AllowUpdate="False" DataField="CuryTranAmt" Label="Amount" Width="81px" AllowNull="False" TextAlign="Right">
                        <Header Text="Amount"></Header>
                    </px:PXGridColumn>
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="TranAmt" Label="Tran. Amount" TextAlign="Right" Width="81px" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="Cleared" Label="Cleared" TextAlign="Center" Type="CheckBox" Visible="False" />
                    <px:PXGridColumn AllowNull="False" DataField="OrigModule" Label="Module" AutoCallBack="True" RenderEditorText="True" Width="54px" />
                    <px:PXGridColumn AllowShowHide="Server" AllowUpdate="False" DataField="ChildOrigTranType" Label="Tran. Type" RenderEditorText="True" />
                    <px:PXGridColumn AllowShowHide="Server" AllowUpdate="False" DataField="ChildOrigRefNbr" Label="ChildOrig. Doc. Number" />
                    <px:PXGridColumn AutoCallBack="True" DataField="ReferenceID" Label="Business Account" Width="140px" />
                    <px:PXGridColumn DataField="ReferenceID_BAccountR_AcctName" Width="140px" />
                    <px:PXGridColumn AutoCallBack="True" DataField="LocationID" Label="Location ID" Width="76px" DisplayFormat="&gt;AAAAAA" />
                    <px:PXGridColumn DataField="PaymentMethodID" DisplayFormat="&gt;aaaaaaaaaa" Label="Payment Method ID" Width="81px" AutoCallBack="true" />
                    <px:PXGridColumn DataField="PMInstanceID" Label="Payment Method Instance" Width="108px" AutoCallBack="True" TextAlign="Right" TextField="PMInstanceID_CustomerPaymentMethod_Descr" />
                    <px:PXGridColumn AllowUpdate="False" DataField="TranDesc" Label="Description" Width="151px" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <ActionBar>
            <CustomItems>
                <px:PXToolBarButton Text="View Result Document">
                    <AutoCallBack Command="ViewResultDocument" Target="ds" />
                </px:PXToolBarButton>
            </CustomItems>
        </ActionBar>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXGrid>
</asp:Content>
