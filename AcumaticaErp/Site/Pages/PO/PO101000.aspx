<%@ Page Language="C#" MasterPageFile="~/MasterPages/TabView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="PO101000.aspx.cs" Inherits="Page_PO101000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/TabView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Setup"
		TypeName="PX.Objects.PO.POSetupMaint" BorderStyle="NotSet">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXTab ID="tab" runat="server" DataSourceID="ds" Height="487px" Style="z-index: 100"
		Width="100%" DataMember="Setup" Caption="General Settings" 
        DefaultControlID="edStandardPONumberingID">
		<autosize container="Window" enabled="True" minheight="200" />
<Activity Width="" Height=""></Activity>
		<Items>
			<px:PXTabItem Text="General Settings">				
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="XM" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Purchase Order Numbering Settings" />

					<px:PXSelector ID="edStandardPONumberingID" runat="server" AllowNull="False" DataField="StandardPONumberingID" Text="POORDERSTD" AllowEdit="True" />
					<px:PXSelector ID="edRegularPONumberingID" runat="server" AllowNull="False" DataField="RegularPONumberingID" Text="POORDERREG" AllowEdit="True" />
					<px:PXSelector ID="edReceiptNumberingID" runat="server" AllowNull="False" DataField="ReceiptNumberingID" Text="PORECEIPT" AllowEdit="True" />
					
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Validate  Total  on Entry" />
					<px:PXCheckBox SuppressLabel="True" ID="chkRequireReceiptControlTotal" runat="server" DataField="RequireReceiptControlTotal" />
					<px:PXCheckBox SuppressLabel="True" ID="chkRequireOrderControlTotal" runat="server" DataField="RequireOrderControlTotal" />
					<px:PXCheckBox SuppressLabel="True" ID="chkRequireBlanketControlTotal" runat="server" DataField="RequireBlanketControlTotal" />
					<px:PXCheckBox SuppressLabel="True" ID="chkRequireDropShipControlTotal" runat="server" DataField="RequireDropShipControlTotal" />
					
                    <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="XM" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Other" />
					<px:PXCheckBox SuppressLabel="True" ID="chkAutoCreateInvoiceOnReceipt" runat="server" DataField="AutoCreateInvoiceOnReceipt" />
					<px:PXSegmentMask CommitChanges="True" ID="edFreightExpenseAcctID" runat="server" DataField="FreightExpenseAcctID" />
					<px:PXSegmentMask ID="edFreightExpenseSubID" runat="server" DataField="FreightExpenseSubID" AutoRefresh="True" />
					<px:PXSelector ID="edRCReturnReasonCodeID" runat="server" DataField="RCReturnReasonCodeID" AllowEdit="True" />
					<px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkAutoReleaseIN" runat="server" DataField="AutoReleaseIN" />
					<px:PXCheckBox SuppressLabel="True" ID="chkAutoReleaseLCIN" runat="server" DataField="AutoReleaseLCIN" />
					<px:PXCheckBox SuppressLabel="True" ID="chkAutoReleaseAP" runat="server" DataField="AutoReleaseAP" />
					<px:PXCheckBox SuppressLabel="True" ID="chkHoldReceipts" runat="server" Checked="True" DataField="HoldReceipts" />
					<px:PXCheckBox SuppressLabel="True" ID="chkUpdateSubOnOwnerChange" runat="server" DataField="UpdateSubOnOwnerChange" />
					<px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkCopyLineDescrSO" runat="server" DataField="CopyLineDescrSO" />
					<px:PXCheckBox ID="chkCopyLineNoteSO" runat="server" DataField="CopyLineNoteSO" />
				    <px:PXCheckBox ID="chkAutoAddLineReceiptBarcode" runat="server" DataField="AutoAddLineReceiptBarcode" />
                    <px:PXCheckBox ID="chkReceiptByOneBarcodeReceiptBarcode" runat="server" DataField="ReceiptByOneBarcodeReceiptBarcode" />
					<px:PXSelector ID="edDefaultReceiptAssignmentMapID" runat="server" AllowEdit="True" DataField="DefaultReceiptAssignmentMapID" TextField="Name" />
                    <px:PXDropDown ID="edVendorPriceUpdate" runat="server" AllowNull="False" DataField="VendorPriceUpdate"  />
                    <px:PXDropDown ID="edShipDestType" runat="server" AllowNull="False" DataField="ShipDestType"  />
                    <px:PXDropDown ID="edDefaultReceiptQty" runat="server" AllowNull="False" DataField="DefaultReceiptQty"  />
				    </Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Approval">				
				<Template>
				    <px:PXPanel runat="server">
				        <px:PXLayoutRule runat="server" LabelsWidth="S" ControlSize="XM" />
				        <px:PXCheckBox ID="chkOrderRequestApproval" runat="server" AlignLeft="true" Checked="True" DataField="OrderRequestApproval" />				        
                    </px:PXPanel>
                    <px:PXGrid ID="gridApproval" runat="server" DataSourceID="ds" SkinID="Details" Width="100%" >
                        <AutoSize Enabled="true" Container="Parent" MinHeight="200"/>
					    <Levels>
						    <px:PXGridLevel DataMember="SetupApproval" DataKeyNames="ApprovalID">
							    <RowTemplate>
								    <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="XM" />

								    <px:PXDropDown ID="edOrderType" runat="server" DataField="OrderType"  />
								    <px:PXSelector ID="edAssignmentMapID" runat="server" DataField="AssignmentMapID" TextField="Name" AllowEdit="True" /></RowTemplate>
							    <Columns>
								    <px:PXGridColumn DataField="OrderType" RenderEditorText="True" Width="100px" />
								    <px:PXGridColumn DataField="AssignmentMapID" Width="250px" 
                                        RenderEditorText="True" TextField="AssignmentMapID_EPAssignmentMap_Name" />
							    </Columns>
							    <Layout FormViewHeight="" />
						    </px:PXGridLevel>
					    </Levels>                        
					</px:PXGrid>
                </Template>
			</px:PXTabItem>
            <px:PXTabItem Text="Mailing Settings">
                <Template>
                    <px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="300" SkinID="Horizontal" Height="500px">
                        <AutoSize Enabled="true" />
                        <Template1>
                            <px:PXGrid ID="gridNS" runat="server" SkinID="DetailsInTab" Width="100%" DataSourceID="ds" Height="150px" Caption="Default Sources"
                                AdjustPageSize="Auto" AllowPaging="True">
                                <AutoCallBack Target="gridNR" Command="Refresh" />
                                <Levels>
                                    <px:PXGridLevel DataMember="Notifications" DataKeyNames="Module,SourceCD,NotificationCD">
                                        <RowTemplate>
                                            <px:PXMaskEdit ID="edNotificationCD" runat="server" DataField="NotificationCD" />
                                            <px:PXSelector ID="edNotificationID" runat="server" DataField="NotificationID" ValueField="Name" />
                                            <px:PXDropDown ID="edFormat" runat="server" AllowNull="False" DataField="Format" SelectedIndex="3" />
                                            <px:PXCheckBox ID="chkActive" runat="server" DataField="Active" />
                                            <px:PXSelector ID="edReportID" runat="server" DataField="ReportID" ValueField="ScreenID" />
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
                                        <Layout FormViewHeight="" />
                                    </px:PXGridLevel>
                                </Levels>
                                <AutoSize Enabled="True" />
                            </px:PXGrid>
                        </Template1>
                        <Template2>
                            <px:PXGrid ID="gridNR" runat="server" SkinID="DetailsInTab" DataSourceID="ds" Width="100%" Caption="Default Recipients" AdjustPageSize="Auto"
                                AllowPaging="True" Style="left: 0px; top: 0px">
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
	</px:PXTab>
</asp:Content>
