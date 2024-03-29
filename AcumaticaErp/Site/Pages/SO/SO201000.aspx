<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SO201000.aspx.cs"
    Inherits="Page_SO201000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.SO.SOOrderTypeMaint" PrimaryView="soordertype">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="soordertype" Caption="Order Type Settings"
        DefaultControlID="edOrderType" NoteIndicator="True" FilesIndicator="True" ActivityIndicator="true" ActivityField="NoteActivity">
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXLayoutRule runat="server" Merge="True" />
            <px:PXSelector Size="xs" ID="edOrderType" runat="server" DataField="OrderType" />
            <px:PXCheckBox CommitChanges="True" ID="chkActive" runat="server" Checked="True" DataField="Active" />
            <px:PXLayoutRule runat="server" Merge="False" />
            <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
            <px:PXSelector CommitChanges="True" ID="edTemplate" runat="server" DataField="Template" NullText="<NEW>" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" Height="606px" DataSourceID="ds" DataMember="currentordertype" LinkPage="">
        <AutoSize Enabled="True" Container="Window" MinHeight="150" />
        <Items>
            <px:PXTabItem Text="General Settings">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Order Settings" />
                    <px:PXSelector ID="edOrderNumberingID" runat="server" DataField="OrderNumberingID" AllowEdit="True" />
                    <px:PXNumberEdit ID="edDaysToKeep" runat="server" DataField="DaysToKeep" />
                    <px:PXCheckBox SuppressLabel="True" ID="chkHoldEntry" runat="server" DataField="HoldEntry" />
                    <px:PXCheckBox SuppressLabel="True" ID="chkCreditHoldEntry" runat="server" DataField="CreditHoldEntry" />
                    <px:PXCheckBox SuppressLabel="True" ID="chkRequireControlTotal" runat="server" DataField="RequireControlTotal" />
                    <px:PXCheckBox SuppressLabel="True" ID="chkBillSeparately" runat="server" DataField="BillSeparately" />
                    <px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkShipSeparately" runat="server" DataField="ShipSeparately" />
                    <px:PXCheckBox SuppressLabel="True" ID="chkCalculateFreight" runat="server" DataField="CalculateFreight" />
                    <px:PXCheckBox SuppressLabel="True" ID="chkRecalculateDiscOnPartialShipment" runat="server" DataField="RecalculateDiscOnPartialShipment" />
                    <px:PXCheckBox SuppressLabel="True" ID="chkCopyNotes" runat="server" DataField="CopyNotes" />
                    <px:PXCheckBox SuppressLabel="True" ID="chkCopyFiles" runat="server" DataField="CopyFiles" />
                    <px:PXCheckBox SuppressLabel="True" ID="chkCopyLineNotesToShipment" runat="server" DataField="CopyLineNotesToShipment" />
                    <px:PXCheckBox SuppressLabel="True" ID="chkCopyLineFilesToShipment" runat="server" DataField="CopyLineFilesToShipment" />
                    <px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkCopyLineNotesToInvoice" runat="server" DataField="CopyLineNotesToInvoice" />
                    <px:PXCheckBox ID="chkCopyLineNotesToInvoiceOnlyNS" runat="server" DataField="CopyLineNotesToInvoiceOnlyNS" Style="margin-left: 25px"/>
                    <px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkCopyLineFilesToInvoice" runat="server" DataField="CopyLineFilesToInvoice" />
                    <px:PXCheckBox ID="chkCopyLineFilesToInvoiceOnlyNS" runat="server" DataField="CopyLineFilesToInvoiceOnlyNS" Style="margin-left: 25px" />
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Accounts Receivable Settings" />
                    <px:PXSelector ID="edInvoiceNumberingID" runat="server" AllowNull="False" DataField="InvoiceNumberingID" Text="ARINVOICE"
                        AllowEdit="True" />
                    <px:PXCheckBox ID="chkMarkInvoicePrinted" runat="server" DataField="MarkInvoicePrinted" />
                    <px:PXCheckBox ID="edMarkInvoiceEmailed" runat="server" DataField="MarkInvoiceEmailed" />
                    <px:PXCheckBox ID="edInvoiceHoldEntry" runat="server" DataField="InvoiceHoldEntry" />
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Posting Settings" />
                    <px:PXDropDown ID="edSalesAcctDefault" runat="server" AllowNull="False" DataField="SalesAcctDefault" />
                    <px:PXSegmentMask ID="edSalesSubMask" runat="server" DataField="SalesSubMask" />
                    <px:PXDropDown ID="edMiscAcctDefault" runat="server" AllowNull="False" DataField="MiscAcctDefault" />
                    <px:PXSegmentMask ID="edMiscSubMask" runat="server" DataField="MiscSubMask" />
                    <px:PXSegmentMask CommitChanges="True" ID="edFreightAcctID" runat="server" DataField="FreightAcctID" />
                    <px:PXDropDown ID="edFreightAcctDefault" runat="server" AllowNull="False" DataField="FreightAcctDefault" />
                    <px:PXSegmentMask ID="edFreightSubID" runat="server" DataField="FreightSubID" />
                    <px:PXSegmentMask ID="edFreightSubMask" runat="server" DataField="FreightSubMask" />
                    <px:PXSegmentMask CommitChanges="True" ID="edDiscountAcctID" runat="server" DataField="DiscountAcctID" AutoRefresh="True" />
                    <px:PXDropDown ID="edDiscAcctDefault" runat="server" AllowNull="False" DataField="DiscAcctDefault" />
                    <px:PXSegmentMask ID="edDiscountSubID" runat="server" DataField="DiscountSubID" AutoRefresh="True" />
                    <px:PXSegmentMask ID="edDiscSubMask" runat="server" DataField="DiscSubMask" />
                    <px:PXCheckBox CommitChanges="True" ID="chkPostLineDiscSeparately" runat="server" DataField="PostLineDiscSeparately" />
                    <px:PXCheckBox CommitChanges="True" ID="chkUseDiscountSubFromSalesSub" runat="server" DataField="UseDiscountSubFromSalesSub" />
                    <px:PXDropDown ID="edCOGSAcctDefault" runat="server" AllowNull="False" DataField="COGSAcctDefault" />
                    <px:PXSegmentMask ID="edCOGSSubMask" runat="server" DataField="COGSSubMask" />
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Template Settings" >
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXDropDown CommitChanges="True" ID="edBehavior" runat="server" DataField="Behavior" />
                    <px:PXDropDown ID="edDefaultOperation" runat="server" DataField="DefaultOperation" CommitChanges="true" />
                    <px:PXDropDown CommitChanges="True" ID="edARDocType" runat="server" DataField="ARDocType" />
                    <px:PXCheckBox CommitChanges="True" ID="chkRequireShipping" runat="server" DataField="RequireShipping" />
                    <px:PXCheckBox CommitChanges="True" ID="chkRequireLocation" runat="server" DataField="RequireLocation" />
                    <px:PXCheckBox CommitChanges="True" ID="chkRequireAllocation" runat="server" DataField="RequireAllocation" />
                    <px:PXGrid runat="server" ID="grid" SkinID="ShortList" Caption="Operations" DataSourceID="ds" Width="1000px" Height="180px">
                        <AutoSize Enabled="True" MinHeight="150" />
                        <Levels>
                            <px:PXGridLevel DataMember="operations" DataKeyNames="OrderType,Operation">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                                    <px:PXSelector ID="edOrderPlanType" runat="server" DataField="OrderPlanType" TextField="Descr" />
                                    <px:PXSelector ID="edShipmentPlanType" runat="server" DataField="ShipmentPlanType" TextField="Descr" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="Operation" Label="Operation" RenderEditorText="True" Width="80px"/>
                                    <px:PXGridColumn AllowNull="False" DataField="Active" Label="Active" TextAlign="Center" Type="CheckBox" Width="60px" CommitChanges="true" />
                                    <px:PXGridColumn DataField="INDocType" Label="Inventory Transaction Type" RenderEditorText="True" Width="100px" 
                                        AutoCallBack="True" />
                                    <px:PXGridColumn DataField="OrderPlanType" TextField="OrderPlanType_INPlanType_Descr" Label="Order Plan Type" Width="100px"
                                        AutoCallBack="True" />
                                    <px:PXGridColumn DataField="ShipmentPlanType" TextField="ShipmentPlanType_INPlanType_Descr" Label="Shipment Plan Type" Width="100px"
                                        AutoCallBack="True" />
                                    <px:PXGridColumn DataField="OrderType" Label="Order Type" Visible="False" />
                                    <px:PXGridColumn AllowNull="False" DataField="AutoCreateIssueLine" Label="Auto Create Issue Line" TextAlign="Center" Type="CheckBox"
                                        Width="100px" />
                                    <px:PXGridColumn AllowNull="False" DataField="RequireReasonCode" Label="Require Reason Code" TextAlign="Center" Type="CheckBox"
                                        Width="60px" />
                                </Columns>
                                <Layout FormViewHeight="" />
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
    </px:PXTab>
</asp:Content>
