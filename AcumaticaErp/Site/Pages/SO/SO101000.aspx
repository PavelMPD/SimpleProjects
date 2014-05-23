<%@ Page Language="C#" MasterPageFile="~/MasterPages/TabView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SO101000.aspx.cs"
    Inherits="Page_SO101000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/TabView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.SO.SOSetupMaint" PrimaryView="sosetup">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXTab ID="tab" runat="server" DataSourceID="ds" Height="500px" Style="z-index: 100" Width="100%" DataMember="sosetup"
        DefaultControlID="edDefaultOrderType">
        <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
        <Items>
            <px:PXTabItem Text="General Settings">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="XM" ControlSize="XM" />
                    <px:PXPanel ID="pnlDataEntrySettings" runat="server" Caption="Data Entry Settings" RenderStyle="Fieldset">
                        <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="XM" ControlSize="XM" />
                        <px:PXSelector ID="edDefaultOrderType" runat="server"
                            DataField="DefaultOrderType" DataSourceID="ds" />
                        <px:PXSelector ID="edTransferOrderType" runat="server"
                            DataField="TransferOrderType" DataSourceID="ds" />
                        <px:PXSelector ID="edShipmentNumberingID" runat="server" AllowNull="False"
                            DataField="ShipmentNumberingID" Text="SOSHIPMENT"
                            AllowEdit="True" DataSourceID="ds" edit="1" />
                        <px:PXCheckBox ID="chkAdvancedAvailCheck" runat="server" DataField="AdvancedAvailCheck" />
                    </px:PXPanel>
                    <px:PXPanel ID="pnlPriceSettings" runat="server" Caption="Price Validation Settings" RenderStyle="Fieldset">
                        <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="XM" ControlSize="XM" />
                        <px:PXDropDown ID="edMinGrossProfitValidation" runat="server" AllowNull="False" DataField="MinGrossProfitValidation" SelectedIndex="1" />
                        <px:PXDropDown ID="edSalesPriceUpdateUnit" runat="server" AllowNull="False" DataField="SalesPriceUpdateUnit" />
                    </px:PXPanel>
                    <px:PXPanel ID="pnlShipmentSettings" runat="server" Caption="Shipment Settings" RenderStyle="Fieldset">
                        <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="XM" ControlSize="XM" />
                        <px:PXDropDown ID="edFreeItemShipping" runat="server" AllowNull="False" DataField="FreeItemShipping" SelectedIndex="-1" />
                        <px:PXCheckBox ID="chkHoldShipments" runat="server" Checked="True" DataField="HoldShipments" />
                        <px:PXCheckBox ID="chkRequireShipmentTotal" runat="server" Checked="True" DataField="RequireShipmentTotal" />
                        <px:PXCheckBox ID="chkAddAllToShipment" runat="server" DataField="AddAllToShipment" CommitChanges="true" />
                        <px:PXCheckBox ID="chkCreateZeroShipments" runat="server" DataField="CreateZeroShipments" CommitChanges="true" />
                    </px:PXPanel>
                    <px:PXPanel ID="pnlInvoiceSettings" runat="server" Caption="Invoice Settings" RenderStyle="Fieldset">
                        <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="XM" ControlSize="XM" />
                        <px:PXCheckBox ID="chkCreditCheckError" runat="server" DataField="CreditCheckError" />
                        <px:PXCheckBox ID="chkUseShipDateForInvoiceDate" runat="server" DataField="UseShipDateForInvoiceDate" />
                    </px:PXPanel>
                    <px:PXPanel ID="pnlPostingSettings" runat="server" Caption="Posting Settings" RenderStyle="Fieldset">
                        <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="XM" ControlSize="XM" />
                        <px:PXCheckBox ID="chkConsolidateIN" runat="server" DataField="ConsolidateIN" />
                        <px:PXCheckBox ID="chkAutoReleaseIN" runat="server" DataField="AutoReleaseIN" />
                    </px:PXPanel>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Mailing Settings">
                <Template>
                    <px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="300" 
                        SkinID="Horizontal" Height="500px" SavePosition="True">
                        <AutoSize Enabled="True" />
                        <Template1>
                            <px:PXGrid ID="gridNS" runat="server" SkinID="DetailsInTab" Width="100%" DataSourceID="ds" Height="150px" Caption="Default Sources"
                                AdjustPageSize="Auto" AllowPaging="True">
                                <AutoCallBack Target="gridNR" Command="Refresh" />
                                <Levels>
                                    <px:PXGridLevel DataMember="Notifications" DataKeyNames="Module,NotificationCD">
                                        <RowTemplate>
                                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                            <px:PXMaskEdit ID="edNotificationCD" runat="server" DataField="NotificationCD" />
                                            <px:PXSelector ID="edNotificationID" runat="server" DataField="NotificationID" ValueField="Name" />
                                            <px:PXDropDown ID="edFormat" runat="server" AllowNull="False" DataField="Format" SelectedIndex="3" />
                                            <px:PXCheckBox ID="chkActive" runat="server" DataField="Active" />
                                            <px:PXSelector ID="edReportID" runat="server" DataField="ReportID" ValueField="ScreenID" />
                                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                            <px:PXSelector ID="edEMailAccountID" runat="server" DataField="EMailAccountID" DisplayMode="Text" />
                                        </RowTemplate>
                                        <Columns>
                                            <px:PXGridColumn DataField="NotificationCD" Width="120px" />
                                            <px:PXGridColumn DataField="EMailAccountID" Width="200px" DisplayMode="Text" />
                                            <px:PXGridColumn DataField="ReportID" DisplayFormat="CC.CC.CC.CC" Width="150px" AutoCallBack="True" />
                                            <px:PXGridColumn DataField="NotificationID" Width="150px" AutoCallBack="True" />
                                            <px:PXGridColumn AllowNull="False" DataField="Format" RenderEditorText="True" Width="60px" AutoCallBack="True" />
                                            <px:PXGridColumn AllowNull="False" DataField="Active" TextAlign="Center" Type="CheckBox" Width="60px" />
                                        </Columns>
                                    </px:PXGridLevel>
                                </Levels>
                                <AutoSize Enabled="True" />
                            </px:PXGrid>
                        </Template1>
                        <Template2>
                            <px:PXGrid ID="gridNR" runat="server" SkinID="Details" DataSourceID="ds" Width="100%" Caption="Default Recipients">
                                <Parameters>
                                    <px:PXSyncGridParam ControlID="gridNS" />
                                </Parameters>
                                <CallbackCommands>
                                    <Save CommitChangesIDs="gridNR" RepaintControls="None" />
                                    <FetchRow RepaintControls="None" />
                                </CallbackCommands>
                                <Levels>
                                    <px:PXGridLevel DataMember="Recipients" DataKeyNames="RecipientID">
                                        <Columns>
                                            <px:PXGridColumn DataField="ContactType" RenderEditorText="True" Width="100px" AutoCallBack="True" />
                                            <px:PXGridColumn DataField="OriginalContactID" Visible="False" AllowShowHide="False" />
                                            <px:PXGridColumn DataField="ContactID" Width="120px">
                                                <NavigateParams>
                                                    <px:PXControlParam Name="ContactID" ControlID="gridNR" PropertyName="DataValues[&quot;OriginalContactID&quot;]" />
                                                </NavigateParams>
                                            </px:PXGridColumn>
                                            <px:PXGridColumn DataField="Format" RenderEditorText="True" Width="60px" AutoCallBack="True" />
                                            <px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox" Width="60px" />
                                            <px:PXGridColumn AllowNull="False" DataField="Hidden" TextAlign="Center" Type="CheckBox" Width="60px" />
                                        </Columns>
                                        <RowTemplate>
                                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                            <px:PXSelector ID="edContactID" runat="server" DataField="ContactID" AutoRefresh="True" ValueField="DisplayName" AllowEdit="True">
                                                <Parameters>
                                                    <px:PXSyncGridParam ControlID="gridNR" />
                                                </Parameters>
                                            </px:PXSelector>
                                        </RowTemplate>
                                        <Layout FormViewHeight="" />
                                    </px:PXGridLevel>
                                </Levels>
                                <AutoSize Enabled="True" MinHeight="150" />
                            </px:PXGrid>
                        </Template2>
                    </px:PXSplitContainer>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize MinHeight="480" Container="Window" Enabled="True" />
    </px:PXTab>
</asp:Content>
