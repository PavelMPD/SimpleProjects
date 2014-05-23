<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CA304000.aspx.cs" Inherits="Page_CA304000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="CAAdjRecords" TypeName="PX.Objects.CA.CATranEntry">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand StartNewGroup="True" CommitChanges="true" Name="Release" />
			<px:PXDSCallbackCommand Visible="False" Name="CurrencyView" />
			<px:PXDSCallbackCommand Visible="False" Name="ViewBatch" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Hold" />
			<px:PXDSCallbackCommand Name="Flow" Visible="false" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="NewTask" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="NewEvent" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="NewActivity" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="NewMailActivity" Visible="False" CommitChanges="True" PopupCommand="Cancel" PopupCommandTarget="ds" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" Width="100%" DataMember="CAAdjRecords" Caption="Transaction Summary" NoteIndicator="True" FilesIndicator="True" ActivityIndicator="True" ActivityField="NoteActivity"
		LinkIndicator="True" NotifyIndicator="True" DefaultControlID="edAdjRefNbr">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="s" ControlSize="s" />
			<px:PXDropDown ID="edAdjTranType" runat="server" DataField="AdjTranType" Enabled="False" />
			<px:PXSelector ID="edAdjRefNbr" runat="server" DataField="AdjRefNbr" />
			<px:PXDropDown ID="edStatus" runat="server" DataField="Status" Enabled="False" />
			<px:PXCheckBox ID="chkHold" runat="server" DataField="Hold">
				<AutoCallBack Command="Hold" Target="ds" />
			</px:PXCheckBox>
			<px:PXCheckBox ID="chkApproved" runat="server" DataField="Approved" Enabled="False" />
			<px:PXDateTimeEdit CommitChanges="True" ID="edTranDate" runat="server" DataField="TranDate" />
			<px:PXSelector CommitChanges="True" ID="edFinPeriodID" runat="server" DataField="FinPeriodID" />
			<px:PXLayoutRule runat="server" ColumnSpan="2" />
			<px:PXTextEdit ID="edTranDesc" runat="server" DataField="TranDesc" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
			<px:PXSegmentMask CommitChanges="True" ID="edCashAccountID" runat="server" DataField="CashAccountID" />
			<pxa:PXCurrencyRate DataField="CuryID" ID="edCury" runat="server" RateTypeView="_CAAdj_CurrencyInfo_" DataMember="_Currency_" />
			<px:PXSelector CommitChanges="True" ID="edEntryTypeID" runat="server" DataField="EntryTypeID" AutoRefresh="True" />
			<px:PXDropDown ID="edDrCr" runat="server" DataField="DrCr" Enabled="False" />
			<px:PXTextEdit ID="edExtRefNbr" runat="server" DataField="ExtRefNbr" />
			<px:PXSegmentMask ID="edEmployeeID" runat="server" DataField="EmployeeID" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="s" ControlSize="s" />
			<px:PXNumberEdit ID="edCuryTranAmt" runat="server" DataField="CuryTranAmt" Enabled="False" />
			<px:PXNumberEdit ID="edCuryVatTaxableTotal" runat="server" DataField="CuryVatTaxableTotal" Enabled="False" />
			<px:PXNumberEdit ID="edCuryVatExemptTotal" runat="server" DataField="CuryVatExemptTotal" Enabled="False" />
			<px:PXNumberEdit ID="edCuryTaxTotal" runat="server" DataField="CuryTaxTotal" Enabled="False" />
			<px:PXNumberEdit CommitChanges="True" ID="edCuryControlAmt" runat="server" DataField="CuryControlAmt" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Height="180px" Width="100%" DataMember="CurrentDocument" DataSourceID="ds">
		<Items>
			<px:PXTabItem Text="Transaction Details">
				<Template>
					<px:PXGrid ID="grid" runat="server" Style="z-index: 100;" Width="100%" SkinID="DetailsInTab" DataSourceID="ds" TabIndex="-14436" SyncPosition="True">
						<Levels>
							<px:PXGridLevel DataMember="CASplitRecords" DataKeyNames="AdjRefNbr,AdjTranType,LineNbr">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
									<px:PXSegmentMask CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID" />
									<px:PXSegmentMask CommitChanges="True" ID="edProjectID" runat="server" DataField="ProjectID" />
									<px:PXSegmentMask ID="edTaskID" runat="server" DataField="TaskID" AutoRefresh="True" />
<px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True" />
									<px:PXNumberEdit ID="edQty" runat="server" DataField="Qty" />
									<px:PXNumberEdit ID="edCuryUnitPrice" runat="server" DataField="CuryUnitPrice" />
									<px:PXNumberEdit ID="edCuryTranAmt" runat="server" DataField="CuryTranAmt" />
                                    <px:PXSegmentMask ID="edCashAccountID" runat="server" DataField="CashAccountID" CommitChanges="True" />
									<px:PXSegmentMask ID="edAccountID" runat="server" DataField="AccountID" CommitChanges="True" />
									<px:PXSegmentMask ID="edSubID" runat="server" DataField="SubID" />
									<px:PXSelector ID="edTaxCategoryID" runat="server" DataField="TaxCategoryID" AutoRefresh="True"/>
									<px:PXTextEdit ID="edTranDesc" runat="server" DataField="TranDesc" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="LineNbr" TextAlign="Right" Width="54px" Visible="False" />
									<px:PXGridColumn DataField="BranchID" Width="81px" />
									<px:PXGridColumn DataField="InventoryID" Width="100px" AutoCallBack="True" />
									<px:PXGridColumn DataField="TranDesc" Width="200px" />
									<px:PXGridColumn DataField="Qty" TextAlign="Right" Width="81px" />
									<px:PXGridColumn DataField="CuryUnitPrice" TextAlign="Right" Width="81px" />
									<px:PXGridColumn DataField="CuryTranAmt" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="CashAccountID" Width="100px" AutoCallBack="True" />
									<px:PXGridColumn DataField="AccountID" Width="100px" AutoCallBack="True" />
									<px:PXGridColumn AllowUpdate="False" DataField="AccountID_description" Width="200px" />
									<px:PXGridColumn DataField="SubID" Width="150px" />
									<px:PXGridColumn AutoCallBack="True" DataField="ProjectID" Width="100px" />
									<px:PXGridColumn DataField="TaskID" Width="100px" />
                                    <px:PXGridColumn DataField="NonBillable" Label="Non Billable" Type="CheckBox"/>
									<px:PXGridColumn DataField="TaxCategoryID" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<Mode InitNewRow="True" AllowFormEdit="True" AllowUpload="True"/>
						<AutoSize Enabled="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Tax Details">
				<Template>
					<px:PXGrid ID="grid1" runat="server" Height="150px" Width="100%" SkinID="DetailsInTab" DataSourceID="ds">
						<AutoSize Enabled="True" />
						<Levels>
							<px:PXGridLevel DataMember="Taxes">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="L" />
									<px:PXSelector ID="edTaxID" runat="server" DataField="TaxID" />
									<px:PXNumberEdit ID="edTaxRate" runat="server" DataField="TaxRate" Enabled="False" />
									<px:PXNumberEdit ID="edCuryTaxableAmt" runat="server" DataField="CuryTaxableAmt" />
									<px:PXNumberEdit ID="edCuryTaxAmt" runat="server" DataField="CuryTaxAmt" /></RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="TaxID" Width="100px" AllowUpdate="False" />
									<px:PXGridColumn AllowUpdate="False" DataField="TaxRate" TextAlign="Right" Width="81px" />
									<px:PXGridColumn DataField="CuryTaxableAmt" TextAlign="Right" Width="81px" />
									<px:PXGridColumn DataField="CuryTaxAmt" TextAlign="Right" Width="81px" />
									<px:PXGridColumn DataField="Tax__TaxType" Width="60px" />
									<px:PXGridColumn DataField="Tax__PendingTax" Type="CheckBox" TextAlign="Center" Width="60px" />
									<px:PXGridColumn DataField="Tax__ReverseTax" Type="CheckBox" TextAlign="Center" Width="60px" />
									<px:PXGridColumn DataField="Tax__ExemptTax" Type="CheckBox" TextAlign="Center" Width="60px" />
									<px:PXGridColumn DataField="Tax__StatisticalTax" Type="CheckBox" TextAlign="Center" Width="60px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Financial Details">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" GroupCaption="Link to GL" StartGroup="True" />
					<px:PXSelector ID="edBatchNbr" runat="server" DataField="TranID_CATran_batchNbr" Enabled="False" AllowEdit="True" />
					<px:PXSegmentMask CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID" />
					<px:PXCheckBox CommitChanges="True" ID="chkCleared" runat="server" DataField="Cleared" />
					<px:PXDateTimeEdit CommitChanges="True" ID="edClearDate" runat="server" DataField="ClearDate" />
					<px:PXLayoutRule runat="server" ControlSize="M" GroupCaption="Tax Settings" LabelsWidth="S" StartColumn="True" StartGroup="True" />
					<px:PXSelector CommitChanges="True" ID="edTaxZoneID" runat="server" DataField="TaxZoneID" />
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Approval Details">
				<Template>
					<px:PXGrid ID="gridApproval" runat="server" Width="100%" SkinID="DetailsInTab" NoteIndicator="True" DataSourceID="ds">
						<AutoSize Enabled="True" />
						<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
					    <Levels>
							<px:PXGridLevel DataMember="Approval" DataKeyNames="ApprovalID,AssignmentMapID">
								<Columns>
                                    <px:PXGridColumn DataField="ApproverEmployee__AcctCD" Width="160px" />
                                    <px:PXGridColumn DataField="ApproverEmployee__AcctName" Width="160px" />
                                    <px:PXGridColumn DataField="ApprovedByEmployee__AcctCD" Width="100px" />
                                    <px:PXGridColumn DataField="ApprovedByEmployee__AcctName" Width="160px" />
                                    <px:PXGridColumn DataField="ApproveDate" Width="90px" />
                                    <px:PXGridColumn DataField="Status" AllowNull="False" AllowUpdate="False" RenderEditorText="True" />
                                </Columns>
							</px:PXGridLevel>
						</Levels>
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
</asp:Content>
