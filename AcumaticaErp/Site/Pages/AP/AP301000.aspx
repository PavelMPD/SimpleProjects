<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AP301000.aspx.cs" Inherits="Page_AP301000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.AP.APInvoiceEntry" PrimaryView="Document">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand StartNewGroup="True" Name="Release" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="Prebook" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="VoidInvoice" CommitChanges="true" />
			<px:PXDSCallbackCommand StartNewGroup="True" Name="Action" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="Inquiry" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="Report" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="false" Name="ReverseInvoice" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="false" Name="VendorRefund" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="false" Name="VoidDocument" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="false" Name="PayInvoice" />
			<px:PXDSCallbackCommand Visible="False" Name="ViewSchedule" CommitChanges="true" DependOnGrid="grid" />
			<px:PXDSCallbackCommand Visible="false" Name="CreateSchedule" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="false" Name="ViewBatch" />
			<px:PXDSCallbackCommand Visible="false" Name="NewVendor" />
			<px:PXDSCallbackCommand Visible="false" Name="EditVendor" />
			<px:PXDSCallbackCommand Visible="false" Name="VendorDocuments" />
			<px:PXDSCallbackCommand Visible="false" Name="AddPOReceipt2" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="false" Name="AddReceiptLine2" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="false" Name="AddPOOrder2" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="false" Name="AddPOReceipt" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="false" Name="AddReceiptLine" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="false" Name="AddPOOrder" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="false" Name="ViewPODocument" DependOnGrid="grid" />
			<px:PXDSCallbackCommand Visible="false" Name="ViewLCPOReceipt" DependOnGrid="gridLCTran" />
			<px:PXDSCallbackCommand Visible="false" Name="ViewLCINDocument" DependOnGrid="gridLCTran" />
			<px:PXDSCallbackCommand Visible="false" Name="AutoApply" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="false" Name="ViewPayment" DependOnGrid="detgrid" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="False" Name="CurrencyView" />
			<px:PXDSCallbackCommand Visible="false" Name="AddPostLandedCostTran" CommitChanges="true" />
			<px:PXDSCallbackCommand CommitChanges="True" Visible="False" Name="LsLCSplits" DependOnGrid="gridLCTran" />
            <px:PXDSCallbackCommand Name="RecalculateDiscountsAction" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="RecalcOk" PopupCommand="" PopupCommandTarget="" PopupPanel="" Text="" Visible="False" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" Style="z-index: 100" Width="100%" 
        DataMember="Document" Caption="Document Summary" NoteIndicator="True" 
        FilesIndicator="True" ActivityIndicator="True" ActivityField="NoteActivity"
		LinkIndicator="True" NotifyIndicator="True" DefaultControlID="edDocType" 
        TabIndex="100" DataSourceID="ds">
		<CallbackCommands>
			<Save PostData="Self" />
		</CallbackCommands>
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
			<px:PXDropDown ID="edDocType" runat="server" DataField="DocType" />
			<px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" 
                AutoRefresh="True" DataSourceID="ds">
                <GridProperties FastFilterFields="InvoiceNbr" />
             </px:PXSelector> 
			<px:PXDropDown ID="edStatus" runat="server" DataField="Status" Enabled="False" />
			<px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkHold" runat="server" DataField="Hold" />
			<px:PXDateTimeEdit CommitChanges="True" ID="edDocDate" runat="server" DataField="DocDate" />
			<px:PXSelector CommitChanges="True" ID="edFinPeriodID" runat="server" 
                DataField="FinPeriodID" DataSourceID="ds" />
			<px:PXTextEdit CommitChanges="True" ID="edInvoiceNbr" runat="server" DataField="InvoiceNbr" />
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
			<px:PXTextEdit ID="edDocDesc" runat="server" DataField="DocDesc" />
			<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="S" StartColumn="True" />
			<px:PXSegmentMask CommitChanges="True" ID="edVendorID" runat="server" 
                DataField="VendorID" AllowAddNew="True" AllowEdit="True" DataSourceID="ds" AutoRefresh="True" />
			<px:PXSegmentMask CommitChanges="True" ID="edVendorLocationID" runat="server" 
                AutoRefresh="True" DataField="VendorLocationID" DataSourceID="ds" />
			<pxa:PXCurrencyRate ID="edCury" DataField="CuryID" runat="server" 
                RateTypeView="_APInvoice_CurrencyInfo_" DataMember="_Currency_" 
                DataSourceID="ds"></pxa:PXCurrencyRate>
			<px:PXSelector CommitChanges="True" ID="edTermsID" runat="server" 
                DataField="TermsID" DataSourceID="ds" />
			<px:PXDateTimeEdit CommitChanges="True" ID="edDueDate" runat="server" DataField="DueDate" />
			<px:PXDateTimeEdit CommitChanges="True" ID="edDiscDate" runat="server" DataField="DiscDate" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
			<px:PXPanel ID="PXPanel1" runat="server" RenderSimple="True" RenderStyle="Simple">
                <px:PXLayoutRule runat="server" StartColumn="True" />
			    <px:PXNumberEdit ID="edCuryLineTotal" runat="server" DataField="CuryLineTotal" Enabled="False" />
                <px:PXNumberEdit CommitChanges="True" ID="edCuryDiscTot" runat="server" Enabled="True" DataField="CuryDiscTot" />
			    <px:PXNumberEdit ID="CuryVatTaxableTotal" runat="server" DataField="CuryVatTaxableTotal" Enabled="False" />
			    <px:PXNumberEdit ID="CuryVatExemptTotal" runat="server" DataField="CuryVatExemptTotal" Enabled="False" />
			    <px:PXNumberEdit ID="edCuryTaxTotal" runat="server" DataField="CuryTaxTotal" Enabled="False" Size="s" />
			    <px:PXNumberEdit ID="edCuryOrigWhTaxAmt" runat="server" DataField="CuryOrigWhTaxAmt" Enabled="False" />
			    <px:PXNumberEdit ID="edCuryDocBal" runat="server" DataField="CuryDocBal" Enabled="False" />
			    <px:PXNumberEdit ID="edCuryRoundDiff" runat="server" CommitChanges="True" DataField="CuryRoundDiff" Enabled="False" />
			    <px:PXNumberEdit ID="edCuryOrigDocAmt" runat="server" CommitChanges="True" DataField="CuryOrigDocAmt" />
			    <px:PXNumberEdit ID="edCuryOrigDiscAmt" runat="server" CommitChanges="True" DataField="CuryOrigDiscAmt" />
			    <px:PXCheckBox ID="chkLCEnabled" runat="server" DataField="LCEnabled" />
			</px:PXPanel>
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Height="300px" Style="z-index: 100;" Width="100%">
		<Items>
			<px:PXTabItem Text="Document Details">
				<Template>
					<px:PXGrid ID="grid" runat="server" Style="z-index: 100;" Width="100%" Height="300px" SyncPosition="True" SkinID="DetailsInTab" DataSourceID="ds" TabIndex="18300">
						<Levels>
							<px:PXGridLevel DataMember="Transactions">
								<Columns>
									<px:PXGridColumn DataField="BranchID" Width="100px" AutoCallBack="True" AllowShowHide="Server" />
                                    <px:PXGridColumn DataField="Date"/>
									<px:PXGridColumn DataField="LineNbr" TextAlign="Right" Width="50px" />
									<px:PXGridColumn DataField="InventoryID" Width="100px" AutoCallBack="True" />
									<px:PXGridColumn DataField="POReceiptLine__SubItemID" Label="Subitem" />
									<px:PXGridColumn DataField="TranDesc" Width="200px" />
									<px:PXGridColumn DataField="Qty" TextAlign="Right" Width="81px" AutoCallback = "True"  />
									<px:PXGridColumn DataField="UOM" Width="50px" AutoCallBack="True" />
									<px:PXGridColumn DataField="CuryUnitCost" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="CuryLineAmt" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="DiscPct" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryDiscAmt" TextAlign="Right" />
                                    <px:PXGridColumn DataField="CuryDiscCost" TextAlign="Right" />
									<px:PXGridColumn DataField="ManualDisc" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="DiscountID" RenderEditorText="True" TextAlign="Left" AllowShowHide="Server" Width="90px" AutoCallBack="True" />
									<px:PXGridColumn DataField="CuryTranAmt" TextAlign="Right" Width="81px" />
									<px:PXGridColumn DataField="AccountID" Width="100px" AutoCallBack="True" />
									<px:PXGridColumn DataField="AccountID_Account_description" Width="100px" SyncVisibility="false" />
									<px:PXGridColumn DataField="SubID" Width="200px" />
									<px:PXGridColumn AutoCallBack="True" DataField="ProjectID" Label="Project" Width="100px" />
									<px:PXGridColumn DataField="TaskID" Label="Task" Width="100px" />
                                    <px:PXGridColumn DataField="NonBillable" Label="Non Billable" Type="CheckBox"/>
									<px:PXGridColumn DataField="Box1099" Width="200px" AllowShowHide="Server" />
									<px:PXGridColumn DataField="DeferredCode" Width="50px" />
									<px:PXGridColumn DataField="DefScheduleID" TextAlign="Right" Width="100px" />
									<px:PXGridColumn DataField="TaxCategoryID" Width="50px" AutoCallBack="True"/>
									<px:PXGridColumn DataField="POOrderType" />
									<px:PXGridColumn DataField="PONbr" />
									<px:PXGridColumn DataField="POLineNbr" TextAlign="Right" />
									<px:PXGridColumn DataField="ReceiptNbr" />
									<px:PXGridColumn DataField="ReceiptLineNbr" TextAlign="Right" />
								</Columns>
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
									<px:PXSegmentMask CommitChanges="True" ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True" AllowAddNew="True" AutoRefresh="True" />
									<px:PXSegmentMask ID="edPOReceiptLine__SubItemID" runat="server" DataField="POReceiptLine__SubItemID" Enabled="False" />
									<px:PXSelector CommitChanges="True" ID="edUOM" runat="server" DataField="UOM" />
									<px:PXNumberEdit ID="edQty" runat="server" DataField="Qty" />
									<px:PXNumberEdit ID="edCuryUnitCost" runat="server" DataField="CuryUnitCost" />
                                    <px:PXNumberEdit ID="edCuryLineAmt" runat="server" DataField="CuryLineAmt" />
									<px:PXNumberEdit ID="edCuryTranAmt" runat="server" DataField="CuryTranAmt" />
									<px:PXSegmentMask CommitChanges="True" ID="edAccountID" runat="server" DataField="AccountID" AutoRefresh="True" />
									<px:PXSegmentMask ID="edSubID" runat="server" DataField="SubID" AutoRefresh="True" />
									<px:PXDropDown ID="edBox1099" runat="server" DataField="Box1099" />
									<px:PXLayoutRule runat="server" ColumnSpan="2" />
									<px:PXTextEdit ID="edTranDesc" runat="server" DataField="TranDesc" />
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
									<px:PXSegmentMask CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID" />
									<px:PXSelector ID="edDeferredCode" runat="server" DataField="DeferredCode" />
									<px:PXSelector ID="edDefScheduleID" runat="server" DataField="DefScheduleID" />
									<px:PXSelector ID="edTaxCategoryID" runat="server" DataField="TaxCategoryID" CommitChanges="True" AutoRefresh="True"/>
									<px:PXSegmentMask CommitChanges="True" ID="edProjectID" runat="server" DataField="ProjectID" />
									<px:PXSegmentMask ID="edTaskID" runat="server" DataField="TaskID" AutoRefresh="True" />
									<px:PXDropDown ID="edPOOrderType" runat="server" DataField="POOrderType" Enabled="False" />
									<px:PXSelector ID="edPONbr" runat="server" DataField="PONbr" Enabled="False" AllowEdit="True" />
									<px:PXSelector ID="edReceiptNbr" runat="server" DataField="ReceiptNbr" Enabled="False" AllowEdit="True" />
								</RowTemplate>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
						<Mode InitNewRow="True" AllowFormEdit="True" AllowUpload="True" />
						<ActionBar>
							<CustomItems>
								<px:PXToolBarButton Text="View Schedule" Key="cmdViewSchedule">
								    <AutoCallBack Command="ViewSchedule" Target="ds" />
								    <PopupCommand Command="Cancel" Target="ds" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Text="Add PO Receipt" Key="cmdRT">
									<AutoCallBack Command="AddPOReceipt" Target="ds" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Text="Add PO Receipt Line" Key="cmdRTL">
									<AutoCallBack Command="AddReceiptLine" Target="ds" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Text="Add PO Order" Key="cmdPO">
									<AutoCallBack Command="AddPOOrder" Target="ds" />
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Financial Details" LoadOnDemand="false">
				<Template>
					<px:PXFormView ID="form2" runat="server" Style="z-index: 100" Width="100%" DataMember="CurrentDocument" CaptionVisible="False" SkinID="Transparent" DataSourceID="ds" TabIndex="18500">
						<Template>
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" GroupCaption="Link to GL" StartGroup="True" />
							<px:PXSelector ID="edBatchNbr" runat="server" DataField="BatchNbr" Enabled="False" AllowEdit="True" DataSourceID="ds" />
							<px:PXSelector ID="edPrebookBatchNbr" runat="server" DataField="PrebookBatchNbr" Enabled="False" AllowEdit="true" DataSourceID="ds" />
							<px:PXSelector ID="edVoidBatchNbr" runat="server" DataField="VoidBatchNbr" Enabled="False" AllowEdit="true" DataSourceID="ds" />
							<px:PXSegmentMask CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID" DataSourceID="ds" />
							<px:PXSegmentMask ID="edAPAccountID" runat="server" DataField="APAccountID" CommitChanges="True" DataSourceID="ds" />
							<px:PXSegmentMask ID="edAPSubID" runat="server" DataField="APSubID" AutoRefresh="True" DataSourceID="ds" />
							<px:PXSegmentMask ID="edPrebookAcctID" runat="server" DataField="PrebookAcctID" CommitChanges="True" DataSourceID="ds" />
							<px:PXSegmentMask ID="edPrebookSubID" runat="server" DataField="PrebookSubID" AutoRefresh="True" DataSourceID="ds" />
							<px:PXLayoutRule runat="server" GroupCaption="Default Payment Info" StartGroup="True" />
							<px:PXCheckBox ID="chkSeparateCheck" runat="server" DataField="SeparateCheck" />
							<px:PXCheckBox CommitChanges="True" ID="chkPaySel" runat="server" DataField="PaySel" />
							<px:PXDateTimeEdit ID="edPayDate" runat="server" DataField="PayDate" />
							<px:PXSegmentMask CommitChanges="True" ID="edPayLocationID" runat="server" AutoRefresh="True" DataField="PayLocationID" DataSourceID="ds" />
							<px:PXSelector CommitChanges="True" ID="edPayTypeID" runat="server" DataField="PayTypeID" DataSourceID="ds" />
							<px:PXSegmentMask CommitChanges="True" ID="edPayAccountID" runat="server" DataField="PayAccountID" DataSourceID="ds" />
							<px:PXLayoutRule runat="server" ControlSize="XM" GroupCaption="Tax" LabelsWidth="SM" StartColumn="True" StartGroup="True" />
							<px:PXSelector CommitChanges="True" ID="edTaxZoneID" runat="server" DataField="TaxZoneID" DataSourceID="ds" />
						</Template>
					</px:PXFormView>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Tax Details">
				<Template>
					<px:PXGrid ID="grid1" runat="server" Style="z-index: 100;" Height="300px" 
                        Width="100%" ActionsPosition="Top" SkinID="DetailsInTab" DataSourceID="ds" 
                        TabIndex="3900">
						<AutoSize Enabled="True" MinHeight="150" />
						<LevelStyles>
							<RowForm Width="300px">
							</RowForm>
						</LevelStyles>
						<ActionBar>
							<Actions>
								<Search Enabled="False" />
								<Save Enabled="False" />
							</Actions>
						</ActionBar>
						<Levels>
							<px:PXGridLevel DataMember="Taxes">
								<%--<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" 
                                        ControlSize="XM" />
									<px:PXSelector CommitChanges="True" Size="s" ID="edTaxID" runat="server" DataField="TaxID" />
									<px:PXNumberEdit ID="edTaxRate" runat="server" DataField="TaxRate" Enabled="False" />
									<px:PXNumberEdit Size="s" ID="edTaxableAmt" runat="server" DataField="TaxableAmt" />
									<px:PXNumberEdit ID="edTaxAmt" runat="server" DataField="TaxAmt" />
                                    <px:PXNumberEdit ID="edNonDeductibleTaxRate" runat="server" DataField="NonDeductibleTaxRate" Enabled="False" />
                                    <px:PXNumberEdit ID="edExpenseAmt" runat="server" DataField="CuryExpenseAmt" />
                                    <px:PXDropDown ID="Tax__TaxType" runat="server" DataField="Tax__TaxType">
                                    </px:PXDropDown>
                                    <px:PXCheckBox ID="Tax__PendingTax" runat="server" DataField="Tax__PendingTax" 
                                        Text="Pending VAT">
                                    </px:PXCheckBox>
                                    <px:PXCheckBox ID="Tax__ReverseTax" runat="server" DataField="Tax__ReverseTax" 
                                        Text="Reverse VAT">
                                    </px:PXCheckBox>
                                    <px:PXCheckBox ID="Tax__ExemptTax" runat="server" DataField="Tax__ExemptTax" 
                                        Text="Exempt From VAT">
                                    </px:PXCheckBox>
                                    <px:PXCheckBox ID="Tax__StatisticalTax" runat="server" 
                                        DataField="Tax__StatisticalTax" Text="Statistical VAT">
                                    </px:PXCheckBox>
                                </RowTemplate>--%>
								<Columns>
									<px:PXGridColumn DataField="TaxID" Width="100px" />
									<px:PXGridColumn DataField="TaxRate" TextAlign="Right" Width="100px" />
									<px:PXGridColumn DataField="CuryTaxableAmt" TextAlign="Right" Width="100px" />
									<px:PXGridColumn DataField="CuryTaxAmt" TextAlign="Right" Width="100px" />
                                    <px:PXGridColumn DataField="NonDeductibleTaxRate" TextAlign="Right" Width="100px" />
                                    <px:PXGridColumn DataField="CuryExpenseAmt" TextAlign="Right" Width="100px" />
								    <px:PXGridColumn DataField="Tax__TaxType">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Tax__PendingTax" TextAlign="Center" Type="CheckBox" 
                                        Width="60px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Tax__ReverseTax" TextAlign="Center" Type="CheckBox" 
                                        Width="60px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Tax__ExemptTax" TextAlign="Center" Type="CheckBox" 
                                        Width="60px">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Tax__StatisticalTax" TextAlign="Center" 
                                        Type="CheckBox" Width="60px">
                                    </px:PXGridColumn>
								</Columns>
							    <Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<%--<Mode AllowFormEdit="True" />--%>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Applications">
				<Template>
					<px:PXGrid ID="detgrid" runat="server" Style="z-index: 100;" Width="100%" Height="300px" SkinID="DetailsInTab">
						<ActionBar>
							<CustomItems>
								<px:PXToolBarButton Text="View Payment" Tooltip="View Payment">
									<AutoCallBack Command="ViewPayment" Target="ds">
										<Behavior CommitChanges="True" />
									</AutoCallBack>
								</px:PXToolBarButton>
								<px:PXToolBarButton Text="Auto Apply" Tooltip="Auto Apply">
									<AutoCallBack Command="AutoApply" Target="ds">
										<Behavior CommitChanges="True" />
									</AutoCallBack>
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
						<Levels>
							<px:PXGridLevel DataMember="Adjustments">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
									<px:PXNumberEdit CommitChanges="True" ID="edCuryAdjdAmt" runat="server" DataField="CuryAdjdAmt" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="AdjgDocType" Width="100px" Type="DropDownList" />
									<px:PXGridColumn DataField="AdjgRefNbr" Width="100px" AutoCallBack="True" />
									<px:PXGridColumn DataField="CuryAdjdAmt" AutoCallBack="True" TextAlign="Right" Width="100px" />
									<px:PXGridColumn DataField="APPayment__DocDate" Width="90px" />
									<px:PXGridColumn DataField="CuryDocBal" TextAlign="Right" Width="100px" />
									<px:PXGridColumn DataField="APPayment__DocDesc" Width="250px" />
									<px:PXGridColumn DataField="APPayment__CuryID" Width="50px" />
									<px:PXGridColumn DataField="APPayment__FinPeriodID" />
									<px:PXGridColumn DataField="APPayment__ExtRefNbr" Width="90px" />
									<px:PXGridColumn DataField="AdjdDocType" Width="18px" />
									<px:PXGridColumn DataField="AdjdRefNbr" Width="90px" />
									<px:PXGridColumn DataField="AdjNbr" TextAlign="Right" Width="54px" />
									<px:PXGridColumn DataField="APPayment__Status" Label="Status" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Landed Costs" BindingContext="form" VisibleExp="DataControls[&quot;chkLCEnabled&quot;].Value = 1">
				<Template>
					<px:PXGrid ID="gridLCTran" runat="server" Style="z-index: 100; left: 0px; top: 0px; height: 269px;" Width="100%" ActionsPosition="Top" BorderWidth="0px" SkinID="Details" DataSourceID="ds" Height="269px"
						TabIndex="18100">
						<Levels>
							<px:PXGridLevel DataMember="landedCostTrans" SortOrder="GroupTranID">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXSelector CommitChanges="True" ID="edLandedCostCodeID" runat="server" AutoRefresh="True" DataField="LandedCostCodeID" />
									<px:PXNumberEdit ID="edCuryLCAmount" runat="server" DataField="CuryLCAmount" />
									<px:PXDropDown CommitChanges="True" ID="edPOReceiptType" runat="server" DataField="POReceiptType" />
									<px:PXSelector CommitChanges="True" ID="edPOReceiptNbr" runat="server" DataField="POReceiptNbr" />
									<px:PXSegmentMask ID="edInventoryID1" runat="server" DataField="InventoryID" AutoRefresh="True" />
									<px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
									<px:PXSelector ID="edTaxCategoryID1" runat="server" DataField="TaxCategoryID" AutoRefresh="True"/>
									<px:PXSegmentMask CommitChanges="True" ID="edSiteID" runat="server" DataField="SiteID" />
									<px:PXSegmentMask Size="xs" ID="edVendorID" runat="server" DataField="VendorID" />
									<px:PXSegmentMask ID="edLocationID" runat="server" DataField="LocationID" AutoRefresh="True" />
									<px:PXSegmentMask ID="edVendorLocationID" runat="server" DataField="VendorLocationID" />
									<px:PXNumberEdit CommitChanges="True" ID="edLCTranID" runat="server" DataField="LCTranID" Enabled="False" /></RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="LCTranID" TextAlign="Right" Visible="False" />
									<px:PXGridColumn DataField="LandedCostCodeID" Width="117px" AutoCallBack="True" />
									<px:PXGridColumn DataField="Descr" Width="351px" />
									<px:PXGridColumn DataField="VendorID" Width="81px" AutoCallBack="True" />
									<px:PXGridColumn DataField="VendorLocationID" Width="54px" />
									<px:PXGridColumn DataField="CuryLCAPEffAmount" TextAlign="Right" Width="81px" />
									<px:PXGridColumn DataField="TaxCategoryID" Label="Tax Category" Width="81px" />
									<px:PXGridColumn DataField="POReceiptType" Width="110px" AutoCallBack="true" />
									<px:PXGridColumn DataField="POReceiptNbr" AutoCallBack="true" Width="115px" NullText="&lt;SPLIT&gt;" />
									<px:PXGridColumn DataField="InventoryID" Width="115px" />
									<px:PXGridColumn DataField="SiteID" Width="81px" AutoCallBack="True" />
									<px:PXGridColumn DataField="LocationID" Width="81px" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
						<Mode InitNewRow="True" AllowFormEdit="True" />
						<ActionBar>
							<CustomItems>
								<px:PXToolBarButton Text="View PO Receipt">
								    <AutoCallBack Command="ViewLCPOReceipt" Target="ds" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Text="View LC IN Adjustment" Key="cmdViewLCTran">
								    <AutoCallBack Command="ViewLCINDocument" Target="ds" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Text="Add Posted Landed Cost" Key="cmdAddPLCT">
									<AutoCallBack Command="AddPostLandedCostTran" Target="ds" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Text="LC Splits" Key="cmdLS" CommandName="LsLCSplits" CommandSourceID="ds" DependOnGrid="gridLCTran" />
							</CustomItems>
						</ActionBar>
						<AutoSize Enabled="True" MinHeight="150" />
						<Mode InitNewRow="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
            <px:PXTabItem Text="Discount Details">
                <Template>
                    <px:PXGrid ID="formDiscountDetail" runat="server" DataSourceID="ds" Width="100%" SkinID="Details" BorderStyle="None">
                        <Levels>
                            <px:PXGridLevel DataMember="DiscountDetails" DataKeyNames="DocType,RefNbr,DiscountID,DiscountSequenceID,Type">
                                <RowTemplate>
                                    <px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXSelector ID="edDiscountID" runat="server" DataField="DiscountID" 
                                        AllowEdit="True" edit="1" />
                                    <px:PXDropDown ID="edType" runat="server" DataField="Type" Enabled="False" />
                                    <px:PXCheckBox ID="chkIsManual" runat="server" DataField="IsManual" />
                                    <px:PXTextEdit ID="edDiscountSequenceID" runat="server" DataField="DiscountSequenceID" />
                                    <px:PXNumberEdit ID="edCuryDiscountableAmt" runat="server" DataField="CuryDiscountableAmt" />
                                    <px:PXNumberEdit ID="edDiscountableQty" runat="server" DataField="DiscountableQty" />
                                    <px:PXNumberEdit ID="edCuryDiscountAmt" runat="server" DataField="CuryDiscountAmt" />
                                    <px:PXNumberEdit ID="edDiscountPct" runat="server" DataField="DiscountPct" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="DiscountID" Width="90px" CommitChanges="true" />
                                    <px:PXGridColumn DataField="DiscountSequenceID" Width="90px" />
                                    <px:PXGridColumn DataField="Type" RenderEditorText="True" Width="90px" />
                                    <px:PXGridColumn DataField="IsManual" Width="75px" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="CuryDiscountableAmt" TextAlign="Right" Width="90px" />
                                    <px:PXGridColumn DataField="DiscountableQty" TextAlign="Right" Width="90px" />
                                    <px:PXGridColumn DataField="CuryDiscountAmt" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="DiscountPct" TextAlign="Right" Width="81px" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                        <Mode AllowUpdate="False" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
		</Items>
		<CallbackCommands>
			<Search CommitChanges="True" PostData="Page" />
			<Refresh CommitChanges="True" PostData="Page" />
		</CallbackCommands>
		<AutoSize Container="Window" Enabled="True" MinHeight="180" />
	</px:PXTab>
	<px:PXSmartPanel ID="PanelAddPOReceipt" runat="server" Style="z-index: 108;" Width="800px" Height="400px" 
		Key="poreceiptslist" AutoCallBack-Command="Refresh" AutoCallBack-Target="grid4" CommandSourceID="ds"
		Caption="Add PO Receipt" CaptionVisible="True" LoadOnDemand="True" ContentLayout-OuterSpacing="None">
		<px:PXFormView ID="frmPOrderFilter" runat="server" DataMember="pOrderFilter" Style="z-index: 100;" Width="100%" CaptionVisible="false" SkinID="Transparent">
			<Template>
				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="MS" ControlSize="M" />
				<px:PXSelector CommitChanges="True" ID="edOrderNbr" runat="server" DataField="OrderNbr" AutoRefresh="True" />
			</Template>
		</px:PXFormView>
        <px:PXGrid ID="grid4" runat="server" Width="100%" Height="200px" BatchUpdate="True" SkinID="Inquire"
			DataSourceID="ds" FastFilterFields="ReceiptNbr" TabIndex="16900">
			<Levels>
				<px:PXGridLevel DataMember="poreceiptslist" DataKeyNames="ReceiptType,ReceiptNbr">
					<Columns>
						<px:PXGridColumn AllowCheckAll="True" AllowMove="False" AllowSort="False" DataField="Selected" TextAlign="Center" Type="CheckBox" Width="20px" />
						<px:PXGridColumn DataField="ReceiptNbr" Width="108px" />
						<px:PXGridColumn DataField="ReceiptType" />
						<px:PXGridColumn DataField="VendorID" Width="81px" />
						<px:PXGridColumn DataField="VendorLocationID" Width="63px" />
						<px:PXGridColumn DataField="ReceiptDate" Width="90px" />
						<px:PXGridColumn DataField="OrderQty" TextAlign="Right" Width="81px" />
						<px:PXGridColumn DataField="CuryOrderTotal" TextAlign="Right" Width="100px" />
						<px:PXGridColumn DataField="UnbilledQty" TextAlign="Right" Width="100px" />
						<px:PXGridColumn DataField="CuryUnbilledTotal" TextAlign="Right" Width="100px" />
					</Columns>
					<Layout FormViewHeight="" />
				</px:PXGridLevel>
			</Levels>
			<AutoSize Enabled="True" />
		</px:PXGrid>
		<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton1" runat="server" DialogResult="OK" Text="Add and Close" />
			<px:PXButton ID="PXButton2" runat="server" DialogResult="No" Text="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXSmartPanel ID="PanelAddPOReceiptLine" runat="server" Caption="Add Receipt Line" CaptionVisible="True" 
		Height="400px" Width="800px" Key="poReceiptLinesSelection" LoadOnDemand="True" ShowAfterLoad="True">
		<px:PXFormView ID="frmPOFilter" runat="server" DataMember="filter" Style="z-index: 100;" Width="100%" CaptionVisible="false" SkinID="Transparent">
			<Template>
				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="MS" ControlSize="M" />
				<px:PXSelector CommitChanges="True" ID="edReceiptNbr" runat="server" DataField="ReceiptNbr" AutoRefresh="True" />
			</Template>
		</px:PXFormView>
		<px:PXGrid ID="gridOL" runat="server" Height="200px" Width="100%" SkinID="Inquire" DataSourceID="ds" 
			FastFilterFields="InvenotryID,TranDesc" TabIndex="17300">
			<Levels>
				<px:PXGridLevel DataMember="poReceiptLinesSelection">
					<Columns>
						<px:PXGridColumn AllowCheckAll="True" AutoCallBack="True" DataField="Selected" Type="CheckBox" TextAlign="Center" Width="20px" />
						<px:PXGridColumn DataField="LineType" />
						<px:PXGridColumn DataField="InventoryID" Width="100px" />
						<px:PXGridColumn DataField="SubItemID" Width="100px" />
						<px:PXGridColumn DataField="UOM" />
                        <px:PXGridColumn DataField="PONbr" />
                        <px:PXGridColumn DataField="POReceipt__InvoiceNbr" />
						<px:PXGridColumn DataField="ReceiptQty" TextAlign="Right" Width="100px" />
						<px:PXGridColumn DataField="CuryExtCost" TextAlign="Right" Width="100px" />
						<px:PXGridColumn DataField="UnbilledQty" TextAlign="Right" Width="100px" />
						<px:PXGridColumn DataField="CuryUnbilledAmt" TextAlign="Right" Width="100px" />
						<px:PXGridColumn DataField="TranDesc" Width="200px" />
					</Columns>
				</px:PXGridLevel>
			</Levels>
			<AutoSize Enabled="true" />
		</px:PXGrid>
		<px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton3" runat="server" Text="Add">
				<AutoCallBack Command="AddReceiptLine2" Target="ds" />
			</px:PXButton>
			<px:PXButton ID="PXButton4" runat="server" DialogResult="OK" Text="Add and Close" />
			<px:PXButton ID="PXButton5" runat="server" DialogResult="No" Text="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXSmartPanel ID="PanelAddPOOrder" runat="server" Width="800px" Height="400px" Key="poorderslist" CommandSourceID="ds" 
		Caption="Add PO Order" CaptionVisible="True" LoadOnDemand="True" AutoCallBack-Command="Refresh" AutoCallBack-Target="PXGrid1">
		<px:PXGrid ID="PXGrid1" runat="server" Height="200px" Width="100%" BatchUpdate="True" SkinID="Inquire" 
			DataSourceID="ds" FastFilterFields="OrderNbr" TabIndex="17500">
			<AutoSize Enabled="true" />
			<Levels>
				<px:PXGridLevel DataMember="poorderslist">
					<Columns>
						<px:PXGridColumn DataField="Selected" Type="CheckBox" AllowCheckAll="True" AllowSort="False" AllowMove="False" TextAlign="Center" Width="20px" />
						<px:PXGridColumn DataField="OrderNbr" Width="100px" />
						<px:PXGridColumn DataField="OrderType" />
						<px:PXGridColumn DataField="VendorID" Width="100px" />
						<px:PXGridColumn DataField="VendorLocationID" Width="80px" />
						<px:PXGridColumn DataField="OrderDate" Width="90px" />
						<px:PXGridColumn DataField="CuryOrderTotal" TextAlign="Right" Width="81px" />
						<px:PXGridColumn DataField="LeftToBillQty" TextAlign="Right" Width="81px" />
						<px:PXGridColumn DataField="CuryLeftToBillCost" TextAlign="Right" Width="81px" />
					</Columns>
					<Layout FormViewHeight="" />
				</px:PXGridLevel>
			</Levels>
		</px:PXGrid>
		<px:PXPanel ID="PXPanel3" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton6" runat="server" Text="Add">
				<AutoCallBack Command="AddPOOrder2" Target="ds" />
			</px:PXButton>
			<px:PXButton ID="PXButton7" runat="server" DialogResult="OK" Text="Add and Close" />
			<px:PXButton ID="PXButton8" runat="server" DialogResult="No" Text="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXSmartPanel ID="PanelAddPostLandedCostTran" runat="server" Width="800px" Height="400px" Style="z-index: 108;" Key="landedCostTranSelection" AutoCallBack-Command="Refresh" AutoCallBack-Target="gridLCSelection"
		CommandSourceID="ds" Caption="Add Postponed Landed Cost" CaptionVisible="True" LoadOnDemand="True">
		<px:PXGrid ID="gridLCSelection" runat="server" Height="200px" Width="100%" BatchUpdate="True" SkinID="Inquire" 
			DataSourceID="ds" FastFilterFields="POReceiptNbr,VendorID" TabIndex="17700">
			<AutoSize Enabled="true" />
			<Levels>
				<px:PXGridLevel DataMember="landedCostTranSelection">
					<Columns>
						<px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" AllowSort="False" AllowMove="False" Width="20px" />
						<px:PXGridColumn DataField="LCTranID" Width="100px" />
						<px:PXGridColumn DataField="LandedCostCodeID" />
						<px:PXGridColumn DataField="VendorID" Width="100px" />
						<px:PXGridColumn DataField="VendorLocationID" Width="80px" />
						<px:PXGridColumn DataField="POReceiptNbr" Width="100px" />
						<px:PXGridColumn DataField="CuryLCAmount" TextAlign="Right" Width="100px" />
					</Columns>
					<Layout FormViewHeight="" />
				</px:PXGridLevel>
			</Levels>
		</px:PXGrid>
		<px:PXPanel ID="PXPanel4" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton9" runat="server" DialogResult="OK" Text="Add and Close" />
			<px:PXButton ID="PXButton10" runat="server" DialogResult="Cancel" Text="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXSmartPanel ID="PanelLCS" runat="server" Style="z-index: 108;" Width="800px" Height="400px" Caption="Landed Cost Splits" 
		CaptionVisible="True" Key="landedCostTrans" AutoCallBack-Command="Refresh" AutoCallBack-Enabled="True" 
		AutoCallBack-Target="grdLCSplit">
		<px:PXGrid ID="grdLCSplit" runat="server" Width="100%" Height="240px" SkinID="Details" DataSourceID="ds" TabIndex="17900">
			<Mode InitNewRow="True" />
			<Parameters>
				<px:PXSyncGridParam ControlID="gridLCTran" />
			</Parameters>
			<Levels>
				<px:PXGridLevel DataMember="LCTranSplit">
					<RowTemplate>
						<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="MS" ControlSize="M" />
						<px:PXSelector CommitChanges="True" ID="edPOReceiptNbr2" runat="server" DataField="POReceiptNbr" AutoRefresh="True" />
						<px:PXSegmentMask ID="edInventoryID2" runat="server" DataField="InventoryID" AutoRefresh="True" />
					</RowTemplate>
					<Columns>
						<px:PXGridColumn DataField="POReceiptNbr" Label="PO Receipt Nbr." AutoCallBack="True" Width="100px" />
						<px:PXGridColumn DataField="InventoryID" Label="Inventory ID" Width="100px" />
						<px:PXGridColumn DataField="Descr" Width="250px">
						</px:PXGridColumn>
					</Columns>
					<Layout FormViewHeight="" />
				</px:PXGridLevel>
			</Levels>
			<AutoSize Enabled="true" />
		</px:PXGrid>
		<px:PXPanel ID="PXPanel5" SkinID="Buttons">
			<px:PXButton ID="PXButton11" runat="server" DialogResult="OK" Text="Ok" />
			<px:PXButton ID="PXButton12" runat="server" DialogResult="Cancel" Text="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>
    <px:PXSmartPanel ID="panelDuplicate" runat="server" Caption="Duplicate Reference Nbr." CaptionVisible="true" LoadOnDemand="true" Key="duplicatefilter"
		AutoCallBack-Enabled="true" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True"
		CallBackMode-PostData="Page">
		<div style="padding: 5px">
			<px:PXFormView ID="formCopyTo" runat="server" DataSourceID="ds" CaptionVisible="False" DataMember="duplicatefilter">
				<ContentStyle BackColor="Transparent" BorderStyle="None" />
				<Template>
					<px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM" />
                    <px:PXLabel Size="xl" ID="lblMessage" runat="server">Record already exists. Please enter new Reference Nbr.</px:PXLabel>
                    <px:PXMaskEdit CommitChanges="True" ID="edRefNbr" runat="server" DataField="RefNbr" InputMask="&gt;CCCCCCCCCCCCCCC" />
				</Template>
			</px:PXFormView>
		</div>
		<px:PXPanel ID="PXPanel7" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton15" runat="server" DialogResult="OK" Text="OK" CommandSourceID="ds" />
            <px:PXButton ID="PXButton16" runat="server" DialogResult="Cancel" Text="Cancel" CommandSourceID="ds" />
		</px:PXPanel>
	</px:PXSmartPanel>
    <%-- Recalculate Prices and Discounts --%>
    <px:PXSmartPanel ID="PanelRecalcDiscounts" runat="server" Caption="Recalculate Prices" CaptionVisible="true" LoadOnDemand="true" Key="recalcdiscountsfilter"
        AutoCallBack-Enabled="true" AutoCallBack-Target="formRecalcDiscounts" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True"
        CallBackMode-PostData="Page">
        <div style="padding: 5px">
            <px:PXFormView ID="formRecalcDiscounts" runat="server" DataSourceID="ds" CaptionVisible="False" DataMember="recalcdiscountsfilter">
                <Activity Height="" HighlightColor="" SelectedColor="" Width="" />
                <ContentStyle BackColor="Transparent" BorderStyle="None" />
                <Template>
                    <px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM" />
                    <px:PXDropDown ID="edRecalcTerget" runat="server" DataField="RecalcTarget" CommitChanges ="true" />
                    <%--<px:PXCheckBox CommitChanges="True" ID="chkRecalcUnitPrices" runat="server" DataField="RecalcUnitPrices" />
                    <px:PXCheckBox CommitChanges="True" ID="chkOverrideManualPrices" runat="server" DataField="OverrideManualPrices" />
                    <px:PXCheckBox CommitChanges="True" ID="chkRecalcDiscounts" runat="server" DataField="RecalcDiscounts" />
                    <px:PXCheckBox CommitChanges="True" ID="chkOverrideManualDiscounts" runat="server" DataField="OverrideManualDiscounts" />--%>
                </Template>
            </px:PXFormView>
        </div>
        <px:PXPanel ID="PXPanel6" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton13" runat="server" DialogResult="OK" Text="OK" CommandName="RecalcOk" CommandSourceID="ds" />
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
