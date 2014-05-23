<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AR301000.aspx.cs" Inherits="Page_AR301000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.AR.ARInvoiceEntry" PrimaryView="Document">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand StartNewGroup="True" Name="Release" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="Action" CommitChanges="true" StartNewGroup="true" />
			<px:PXDSCallbackCommand Name="Report" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="Inquiry" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="False" Name="ReverseInvoice" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="False" Name="WriteOff" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="False" Name="PayInvoice" />
			<px:PXDSCallbackCommand Visible="False" Name="CreateSchedule" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="False" Name="ViewSchedule" CommitChanges="true" DependOnGrid="grid" />
			<px:PXDSCallbackCommand Visible="False" Name="ViewBatch" />
			<px:PXDSCallbackCommand Visible="False" Name="NewCustomer" />
			<px:PXDSCallbackCommand Visible="False" Name="SendARInvoiceMemo" />
			<px:PXDSCallbackCommand Visible="False" Name="EditCustomer" />
			<px:PXDSCallbackCommand Visible="False" Name="CustomerDocuments" />
			<px:PXDSCallbackCommand Visible="false" Name="AutoApply" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="false" Name="ViewItem" DependOnGrid="grid" />
			<px:PXDSCallbackCommand Visible="false" Name="ViewPayment" DependOnGrid="detgrid" CommitChanges="true" />
			<px:PXDSCallbackCommand Visible="False" Name="CurrencyView" />
			<px:PXDSCallbackCommand Visible="False" Name="SOInvoice" />
			<px:PXDSCallbackCommand Name="NewTask" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="NewEvent" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="NewActivity" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="NewMailActivity" Visible="False" CommitChanges="True" PopupCommand="Cancel" PopupCommandTarget="ds" />
			<px:PXDSCallbackCommand StartNewGroup="True" Name="ValidateAddresses" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="RecalculateDiscountsAction" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="RecalcOk" PopupCommand="" PopupCommandTarget="" PopupPanel="" Text="" Visible="False" />
		</CallbackCommands>
		<DataTrees>
			<px:PXTreeDataMember TreeView="_EPCompanyTree_Tree_" TreeKeys="WorkgroupID" />
		</DataTrees>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" Width="100%" DataMember="Document" Caption="Invoice Summary" NoteIndicator="True" FilesIndicator="True" ActivityIndicator="True" ActivityField="NoteActivity" LinkIndicator="True"
		NotifyIndicator="True" EmailingGraph="PX.Objects.CR.CREmailActivityMaint,PX.Objects" DefaultControlID="edDocType" TabIndex="100">
		<CallbackCommands>
			<Save PostData="Self" />
		</CallbackCommands>
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
			<px:PXDropDown ID="edDocType" runat="server" DataField="DocType" SelectedIndex="-1" />
			<px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" AutoRefresh="True">
				<GridProperties FastFilterFields="InvoiceNbr"/>
			</px:PXSelector>
			<px:PXDropDown ID="edStatus" runat="server" DataField="Status" Enabled="False" />
			<px:PXCheckBox CommitChanges="True" ID="chkHold" runat="server" DataField="Hold" />
			<px:PXDateTimeEdit CommitChanges="True" ID="edDocDate" runat="server" DataField="DocDate" />
			<px:PXSelector CommitChanges="True" ID="edFinPeriodID" runat="server" DataField="FinPeriodID" />
			<px:PXTextEdit ID="edInvoiceNbr" runat="server" DataField="InvoiceNbr" />
			<px:PXLayoutRule runat="server" ColumnSpan="2" />
			<px:PXTextEdit ID="edDocDesc" runat="server" DataField="DocDesc" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
			<px:PXSegmentMask CommitChanges="True" ID="edCustomerID" runat="server" DataField="CustomerID" AllowAddNew="True" AllowEdit="True" AutoRefresh="True" />
			<px:PXSegmentMask CommitChanges="True" ID="edCustomerLocationID" runat="server" AutoRefresh="True" DataField="CustomerLocationID" />
			<pxa:PXCurrencyRate DataField="CuryID" ID="edCury" runat="server" RateTypeView="_ARInvoice_CurrencyInfo_" DataMember="_Currency_"></pxa:PXCurrencyRate>
			<px:PXSelector CommitChanges="True" ID="edTermsID" runat="server" DataField="TermsID" />
			<px:PXDateTimeEdit ID="edDueDate" runat="server" DataField="DueDate" />
			<px:PXDateTimeEdit ID="edDiscDate" runat="server" DataField="DiscDate" />
			<px:PXSegmentMask CommitChanges="True" ID="edProjectID" runat="server" DataField="ProjectID" AutoRefresh="True" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
			<px:PXNumberEdit ID="edCuryLineTotal" runat="server" DataField="CuryLineTotal" />
            <px:PXNumberEdit CommitChanges="True" ID="edCuryDiscTot" runat="server" Enabled="True" DataField="CuryDiscTot" />
			<px:PXNumberEdit ID="edCuryVatTaxableTotal" runat="server" DataField="CuryVatTaxableTotal" Enabled="False" />
			<px:PXNumberEdit ID="edCuryVatExemptTotal" runat="server" DataField="CuryVatExemptTotal" Enabled="False" />
			<px:PXNumberEdit ID="edCuryTaxTotal" runat="server" DataField="CuryTaxTotal" Enabled="False" />
			<px:PXNumberEdit ID="edCuryDocBal" runat="server" DataField="CuryDocBal" Enabled="False" />
            <px:PXNumberEdit CommitChanges="True" ID="edCuryRoundDiff" runat="server" DataField="CuryRoundDiff" Enabled="False"/>
			<px:PXNumberEdit CommitChanges="True" ID="edCuryOrigDocAmt" runat="server" DataField="CuryOrigDocAmt" />
			<px:PXNumberEdit CommitChanges="True" ID="edCuryOrigDiscAmt" runat="server" DataField="CuryOrigDiscAmt" />
			<px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" ControlSize="SM" />
			<px:PXCheckBox runat="server" DataField="IsRUTROTDeductible" CommitChanges="True" ID="chkRUTROT" AlignLeft="True" />
			<px:PXCheckBox runat="server" DataField="RUTROTAutoDistribution" CommitChanges="True" ID="chkRRAutoDistribution" AlignLeft="True" />
			<px:PXGroupBox runat="server" DataField="RUTROTType" CommitChanges="True" RenderStyle="Simple" ID="gbRRType">
				<ContentLayout Layout="Stack" Orientation="Horizontal" />
				<Template>
					<px:PXRadioButton runat="server" Value="O" ID="gbRRType_opO" GroupName="gbRRType" Text="ROT" />
					<px:PXRadioButton runat="server" Value="U" ID="gbRRType_opU" GroupName="gbRRType" Text="RUT" /></Template></px:PXGroupBox>
			<px:PXTextEdit runat="server" DataField="ROTEstateAppartment" ID="edRREstateAppartment" />
			<px:PXTextEdit runat="server" DataField="ROTOrganizationNbr" ID="edRROrganizationNbr" />
			<px:PXNumberEdit runat="server" DataField="RUTROTDeductionPct" CommitChanges="True" ID="edRRDeduction" />
			<px:PXNumberEdit runat="server" DataField="CuryRUTROTTotalAmt" ID="edRRTotalAmt" Enabled="false" />
			<px:PXNumberEdit runat="server" DataField="CuryRUTROTDistributedAmt" ID="edRRAvailAmt" Enabled="false" />
			<px:PXNumberEdit runat="server" DataField="CuryRUTROTUndistributedAmt" ID="edRRUndsitributedAmt" Enabled="false" />
			<px:PXGrid runat="server" DataSourceID="ds" Width="270px" AllowFilter="false" AllowSearch="false" Height="60px" SkinID="ShortList" ID="gridDistribution">
				<Levels>
					<px:PXGridLevel DataMember="RUTROTDistribution">
						<RowTemplate>
							<px:PXTextEdit runat="server" DataField="PersonalID" ID="edPersonalID" />
							<px:PXNumberEdit runat="server" DataField="CuryAmount" ID="edAmount" />
						</RowTemplate>
						<Columns>
							<px:PXGridColumn DataField="PersonalID" Width="130px" />
							<px:PXGridColumn DataField="CuryAmount" Width="95px" TextAlign="Right" />
						</Columns>
					</px:PXGridLevel>
				</Levels>
			</px:PXGrid>

		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Height="300px" Style="z-index: 100;" Width="100%" TabIndex="200" DataMember="CurrentDocument">
		<Items>
			<px:PXTabItem Text="Document Details">
				<Template>
					<px:PXGrid ID="grid" runat="server" NoteIndicator="True" FilesIndicator="True" Width="100%" SyncPosition="True" SkinID="DetailsInTab" TabIndex="300" Height="300px" FilesField="NoteFiles">
						<Levels>
							<px:PXGridLevel DataMember="Transactions">
								<Columns>
									<px:PXGridColumn DataField="BranchID" AutoCallBack="True" AllowShowHide="Server" />
									<px:PXGridColumn DataField="Date"/>
									<px:PXGridColumn DataField="InventoryID" Width="100px" AutoCallBack="True" />
									<px:PXGridColumn DataField="SOLine__SubItemID" AutoCallBack="True" />
									<px:PXGridColumn DataField="TranDesc" Width="200px" />
									<px:PXGridColumn DataField="Qty" TextAlign="Right" AutoCallBack="True" />
									<px:PXGridColumn DataField="UOM" Width="50px" AutoCallBack="True" />
									<px:PXGridColumn DataField="CuryUnitPrice" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryExtPrice" TextAlign="Right" />
									<px:PXGridColumn DataField="DiscPct" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryDiscAmt" TextAlign="Right" />
									<px:PXGridColumn DataField="CuryTranAmt" TextAlign="Right" Width="100px" />
									<px:PXGridColumn DataField="ManualDisc" TextAlign="Center" Type="CheckBox" CommitChanges="true" />
                                    <px:PXGridColumn DataField="DiscountID" RenderEditorText="True" TextAlign="Left" AllowShowHide="Server" Width="90px" AutoCallBack="True" />
									<px:PXGridColumn DataField="AccountID" Width="100px" AutoCallBack="True" />
									<px:PXGridColumn DataField="AccountID_Account_description" Width="120px" SyncVisibility="false" />
									<px:PXGridColumn DataField="SubID" Width="150px" />
									<px:PXGridColumn DataField="TaskID" Width="100px" />
									<px:PXGridColumn DataField="SalesPersonID" Width="100px" />
									<px:PXGridColumn DataField="DefScheduleID" TextAlign="Right" Width="100px" />
									<px:PXGridColumn DataField="DeferredCode" />
									<px:PXGridColumn DataField="TaxCategoryID" />
                                    <px:PXGridColumn DataField="Commissionable" TextAlign="Center" Type="CheckBox" AutoCallBack="True" />
									<px:PXGridColumn DataField="PMDeltaOption" Label="Complete Line" Width="100px" Type="DropDownList" />
                                    <px:PXGridColumn DataField="OrigInvoiceDate" />
                                    <px:PXGridColumn DataField="IsRUTROTDeductible" Width="100px" Type="Checkbox" AutoCallBack="True" />
									<px:PXGridColumn DataField="CuryRUTROTAvailableAmt" Width="100px" />
                                </Columns>
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
									<px:PXSegmentMask CommitChanges="True" ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True" AllowAddNew="True" AutoRefresh="True" />
									<px:PXSegmentMask ID="edSOLine__SubItemID" runat="server" DataField="SOLine__SubItemID" CommitChanges="True" />
									<px:PXSelector CommitChanges="True" ID="edUOM" runat="server" DataField="UOM" />
									<px:PXNumberEdit CommitChanges="True" ID="edQty" runat="server" DataField="Qty" />
									<px:PXNumberEdit ID="edCuryUnitPrice" runat="server" DataField="CuryUnitPrice" />
									<px:PXNumberEdit ID="edCuryExtPrice" runat="server" DataField="CuryExtPrice" />
									<px:PXNumberEdit ID="edDiscPct" runat="server" DataField="DiscPct" />
									<px:PXNumberEdit ID="edCuryDiscAmt" runat="server" DataField="CuryDiscAmt" />
									<px:PXCheckBox ID="chkManualDisc" runat="server" DataField="ManualDisc" CommitChanges="true" />
									<px:PXNumberEdit ID="edCuryTranAmt" runat="server" DataField="CuryTranAmt" Enabled="False" />
									<px:PXLayoutRule runat="server" ColumnSpan="2" />
									<px:PXTextEdit ID="edTranDesc" runat="server" DataField="TranDesc" />
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
									<px:PXSegmentMask CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID" />
									<px:PXSegmentMask CommitChanges="True" ID="edAccountID" runat="server" DataField="AccountID" AutoRefresh="True" />
									<px:PXSegmentMask ID="edSubID" runat="server" DataField="SubID" AutoRefresh="True" />
									<px:PXSegmentMask ID="edSalesPersonID" runat="server" DataField="SalesPersonID" />
									<px:PXCheckBox CommitChanges="True" ID="chkCommissionable" runat="server" DataField="Commissionable" />
									<px:PXSelector ID="edDefScheduleID" runat="server" DataField="DefScheduleID" />
									<px:PXSelector ID="edDeferredCode" runat="server" DataField="DeferredCode" />
									<px:PXSelector ID="edTaxCategoryID" runat="server" DataField="TaxCategoryID" AutoRefresh="True"/>
                                    <px:PXSegmentMask ID="edTaskID" runat="server" DataField="TaskID" AutoRefresh="True" />
									<px:PXDropDown ID="edPMDeltaOption" runat="server" DataField="PMDeltaOption" />
									<px:PXCheckBox runat="server" DataField="IsRUTROTDeductible" CommitChanges="True" ID="chkRRDeductibleTran" />
									<px:PXNumberEdit runat="server" DataField="CuryRUTROTAvailableAmt" ID="edRRAvailable" />
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
								<px:PXToolBarButton Text="View Item" Key="ViewItem">
									<AutoCallBack Command="ViewItem" Target="ds" />
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Financial Details">
				<Template>
					<px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="SM" StartColumn="True" GroupCaption="Link to GL" />
					<px:PXSelector ID="edBatchNbr" runat="server" DataField="BatchNbr" Enabled="False" AllowEdit="True" />
					<px:PXSegmentMask CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID" />
					<px:PXSegmentMask ID="edARAccountID" runat="server" DataField="ARAccountID" CommitChanges="True" />
					<px:PXSegmentMask ID="edARSubID" runat="server" DataField="ARSubID" AutoRefresh="True" />
                    <px:PXLabel runat="server" ID="space1"></px:PXLabel>
					<px:PXLayoutRule runat="server" GroupCaption="Default Payment Info" />
					<px:PXSelector CommitChanges="True" ID="edPaymentMethodID" runat="server" DataField="PaymentMethodID" />
					<px:PXSelector CommitChanges="True" ID="edPMInstanceID" runat="server" DataField="PMInstanceID" TextField="Descr" AutoRefresh="True" AllowAddNew="True" AllowEdit="True" />
					<px:PXSegmentMask CommitChanges="True" ID="edCashAccountID" runat="server" DataField="CashAccountID" />
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" GroupCaption="Tax and Commission" StartGroup="True" />
					<px:PXSelector CommitChanges="True" ID="edTaxZoneID" runat="server" DataField="TaxZoneID" />
                    <px:PXDropDown ID="edAvalaraCustomerUsageTypeID" runat="server" CommitChanges="True" DataField="AvalaraCustomerUsageType" />
					<px:PXSegmentMask CommitChanges="True" ID="edSalesPersonID" runat="server" DataField="SalesPersonID" />
					<px:PXNumberEdit ID="edCuryCommnblAmt" runat="server" DataField="CuryCommnblAmt" Enabled="False" />
					<px:PXNumberEdit ID="edCuryCommnAmt" runat="server" DataField="CuryCommnAmt" Enabled="False" />
					<px:PXLayoutRule runat="server" GroupCaption="Assigned To" StartGroup="True" />
					<px:PXTreeSelector CommitChanges="True" ID="edWorkgroupID" runat="server" DataField="WorkgroupID" TreeDataMember="_EPCompanyTree_Tree_" TreeDataSourceID="ds" PopulateOnDemand="True" InitialExpandLevel="0"
						ShowRootNode="False">
						<DataBindings>
							<px:PXTreeItemBinding TextField="Description" ValueField="Description" />
						</DataBindings>
					</px:PXTreeSelector>
					<px:PXSelector CommitChanges="True" ID="edOwnerID" runat="server" AutoRefresh="True" DataField="OwnerID" />
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Billing Address">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
					<px:PXFormView ID="Billing_Contact" runat="server" Caption="Contact Information" DataMember="Billing_Contact" RenderStyle="Fieldset">
						<Template>
							<px:PXLayoutRule ID="PXLayoutRule1" runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
							<px:PXCheckBox CommitChanges="True" ID="chkOverrideContact" runat="server" DataField="OverrideContact" />
							<px:PXTextEdit ID="edFullName" runat="server" DataField="FullName" />
							<px:PXTextEdit ID="edSalutation" runat="server" DataField="Salutation" />
							<px:PXMaskEdit ID="edPhone1" runat="server" DataField="Phone1" />
							<px:PXMailEdit ID="edEmail" runat="server" DataField="Email" CommandSourceID="ds" />
						</Template>
					</px:PXFormView>
					<px:PXFormView ID="Billing_Address" runat="server" Caption="Address" DataMember="Billing_Address" RenderStyle="Fieldset">
						<Template>
							<px:PXLayoutRule ID="PXLayoutRule1" runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
							<px:PXCheckBox CommitChanges="True" ID="chkOverrideAddress" runat="server" DataField="OverrideAddress" Height="18px" />
							<px:PXCheckBox ID="edIsValidated" runat="server" DataField="IsValidated" Enabled="False" />
							<px:PXTextEdit ID="edAddressLine1" runat="server" DataField="AddressLine1" />
							<px:PXTextEdit ID="edAddressLine2" runat="server" DataField="AddressLine2" />
							<px:PXTextEdit ID="edCity" runat="server" DataField="City" />
							<px:PXSelector ID="edCountryID" runat="server" DataField="CountryID" AutoRefresh="True" CommitChanges="true" />
							<px:PXSelector ID="edState" runat="server" DataField="State" AutoRefresh="True" />
							<px:PXMaskEdit CommitChanges="True" ID="edPostalCode" runat="server" DataField="PostalCode" />
						</Template>
					</px:PXFormView>
					<px:PXLayoutRule runat="server" GroupCaption="Print and Email Options" StartColumn="True" StartGroup="True" />
					<px:PXLayoutRule runat="server" Merge="True" />
					<px:PXCheckBox ID="chkPrinted" runat="server" DataField="Printed" Size="SM" AlignLeft="true" />
					<px:PXCheckBox ID="chkDontPrint" runat="server" DataField="DontPrint" Size="SM" AlignLeft="true" />
					<px:PXLayoutRule runat="server" Merge="True" />
					<px:PXCheckBox ID="chkEmailed" runat="server" DataField="Emailed" Enabled="False" Size="SM" AlignLeft="true" />
					<px:PXCheckBox ID="chkDontEmail" runat="server" DataField="DontEmail" Size="SM" AlignLeft="true" />
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Tax Details">
				<Template>
					<px:PXGrid ID="grid1" runat="server" Width="100%" SkinID="DetailsInTab" Height="300px" TabIndex="500">
						<AutoSize Enabled="True" MinHeight="150" />
						<Levels>
							<px:PXGridLevel DataMember="Taxes">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
									<px:PXSelector CommitChanges="True" ID="edTaxID" runat="server" DataField="TaxID" />
									<px:PXNumberEdit ID="edTaxRate" runat="server" DataField="TaxRate" Enabled="False" />
									<px:PXNumberEdit ID="edTaxableAmt" runat="server" DataField="TaxableAmt" />
									<px:PXNumberEdit ID="edTaxAmt" runat="server" DataField="TaxAmt" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="TaxID" Width="100px" />
									<px:PXGridColumn DataField="TaxRate" TextAlign="Right" Width="100px" />
									<px:PXGridColumn DataField="CuryTaxableAmt" TextAlign="Right" Width="100px" />
									<px:PXGridColumn DataField="CuryTaxAmt" TextAlign="Right" Width="100px" />
									<px:PXGridColumn DataField="Tax__TaxType" Width="60px" />
									<px:PXGridColumn DataField="Tax__PendingTax" Type="CheckBox" TextAlign="Center" Width="60px" />
									<px:PXGridColumn DataField="Tax__ReverseTax" Type="CheckBox" TextAlign="Center" Width="60px" />
									<px:PXGridColumn DataField="Tax__ExemptTax" Type="CheckBox" TextAlign="Center" Width="60px" />
									<px:PXGridColumn DataField="Tax__StatisticalTax" Type="CheckBox" TextAlign="Center" Width="60px" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Salesperson Commission">
				<Template>
					<px:PXGrid ID="gridSalesPerTran" runat="server" Width="100%" SkinID="DetailsInTab" TabIndex="600">
						<Levels>
							<px:PXGridLevel DataMember="SalesPerTrans" DataKeyNames="DocType,RefNbr,SalespersonID,AdjNbr,AdjdDocType,AdjdRefNbr">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
									<px:PXSegmentMask CommitChanges="True" ID="edSalespersonID" runat="server" DataField="SalespersonID" Enabled="False" />
									<px:PXNumberEdit ID="edCommnPct" runat="server" DataField="CommnPct" />
									<px:PXNumberEdit ID="edCuryCommnAmt" runat="server" DataField="CuryCommnAmt" Enabled="False" />
									<px:PXNumberEdit ID="edCuryCommnblAmt" runat="server" DataField="CuryCommnblAmt" Enabled="False" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="SalespersonID" Width="100px" AutoCallBack="True" />
									<px:PXGridColumn DataField="CommnPct" TextAlign="Right" Width="100px" />
									<px:PXGridColumn DataField="CuryCommnAmt" TextAlign="Right" Width="100px" />
									<px:PXGridColumn DataField="CuryCommnblAmt" TextAlign="Right" Width="100px" />
									<px:PXGridColumn AllowShowHide="False" DataField="AdjdDocType" Visible="False" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
						<Mode AllowAddNew="False" AllowDelete="False" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
            <px:PXTabItem Text="Discount Details">
                <Template>
                    <px:PXGrid ID="formDiscountDetail" runat="server" DataSourceID="ds" Width="100%" SkinID="Details" BorderStyle="None" SyncPosition="true">
                        <Levels>
                            <px:PXGridLevel DataMember="ARDiscountDetails" DataKeyNames="DiscountID,DiscountSequenceID,Type">
                                <RowTemplate>
                                    <px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXSelector ID="edDiscountID" runat="server" DataField="DiscountID" 
                                        AllowEdit="True" edit="1" />
                                    <px:PXDropDown ID="edType" runat="server" DataField="Type" Enabled="False" />
                                    <px:PXCheckBox ID="chkIsManual" runat="server" DataField="IsManual" />
                                    <px:PXSelector ID="edDiscountSequenceID" runat="server" DataField="DiscountSequenceID" />
                                    <px:PXNumberEdit ID="edCuryDiscountableAmt" runat="server" DataField="CuryDiscountableAmt" />
                                    <px:PXNumberEdit ID="edDiscountableQty" runat="server" DataField="DiscountableQty" />
                                    <px:PXNumberEdit ID="edCuryDiscountAmt" runat="server" DataField="CuryDiscountAmt" />
                                    <px:PXNumberEdit ID="edDiscountPct" runat="server" DataField="DiscountPct" />
                                    <px:PXSegmentMask ID="edFreeItemID" runat="server" DataField="FreeItemID" AllowEdit="True" />
                                    <px:PXNumberEdit ID="edFreeItemQty" runat="server" DataField="FreeItemQty" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="DiscountID" Width="90px" CommitChanges ="true" />
                                    <px:PXGridColumn DataField="DiscountSequenceID" Width="90px" CommitChanges="true" />
                                    <px:PXGridColumn DataField="Type" RenderEditorText="True" Width="90px" />
                                    <px:PXGridColumn DataField="IsManual" Width="75px" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="CuryDiscountableAmt" TextAlign="Right" Width="90px" />
                                    <px:PXGridColumn DataField="DiscountableQty" TextAlign="Right" Width="90px" />
                                    <px:PXGridColumn DataField="CuryDiscountAmt" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="DiscountPct" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="FreeItemID" DisplayFormat="&gt;CCCCC-CCCCCCCCCCCCCCC" Width="144px" />
                                    <px:PXGridColumn DataField="FreeItemQty" TextAlign="Right" Width="81px" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
			<px:PXTabItem Text="Applications">
				<Template>
					<px:PXGrid ID="detgrid" runat="server" Width="100%" SkinID="DetailsInTab" Height="300px" TabIndex="700">
						<Levels>
							<px:PXGridLevel DataMember="Adjustments">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
									<px:PXDropDown ID="edAdjgDocType" runat="server" DataField="AdjgDocType" Enabled="False" />
									<px:PXSelector CommitChanges="True" ID="edAdjgRefNbr" runat="server" DataField="AdjgRefNbr" Enabled="False">
										<Parameters>
											<px:PXControlParam ControlID="form" Name="ARInvoice.customerID" PropertyName="DataControls[&quot;edCustomerID&quot;].Value" />
											<px:PXControlParam ControlID="detgrid" Name="ARAdjust.adjgDocType" PropertyName="DataValues[&quot;AdjgDocType&quot;]" />
										</Parameters>
									</px:PXSelector>
									<px:PXNumberEdit CommitChanges="True" ID="edCuryAdjdAmt" runat="server" DataField="CuryAdjdAmt" />
									<px:PXDateTimeEdit ID="edARPayment__DocDate" runat="server" DataField="ARPayment__DocDate" Enabled="False" />
									<px:PXNumberEdit ID="edCuryDocBal" runat="server" DataField="CuryDocBal" Enabled="False" />
									<px:PXSelector ID="edARPayment__CuryID" runat="server" DataField="ARPayment__CuryID" />
									<px:PXSelector ID="edARPayment__FinPeriodID" runat="server" DataField="ARPayment__FinPeriodID" Enabled="False" />
									<px:PXTextEdit ID="edARPayment__ExtRefNbr" runat="server" DataField="ARPayment__ExtRefNbr" />
									<px:PXDropDown ID="edARPayment__Status" runat="server" DataField="ARPayment__Status" Enabled="False" />
									<px:PXTextEdit ID="edARPayment__DocDesc" runat="server" DataField="ARPayment__DocDesc" /></RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="AdjgDocType" Width="100px" RenderEditorText="True" />
									<px:PXGridColumn DataField="AdjgRefNbr" Width="100px" AutoCallBack="True" />
									<px:PXGridColumn DataField="CuryAdjdAmt" AutoCallBack="True" TextAlign="Right" Width="100px" />
									<px:PXGridColumn DataField="ARPayment__DocDate" Width="100px" />
									<px:PXGridColumn DataField="CuryDocBal" TextAlign="Right" Width="100px" />
									<px:PXGridColumn DataField="ARPayment__DocDesc" Width="250px" />
									<px:PXGridColumn DataField="ARPayment__CuryID" Width="50px" />
									<px:PXGridColumn DataField="ARPayment__FinPeriodID" />
									<px:PXGridColumn DataField="ARPayment__ExtRefNbr" Width="100px" />
									<px:PXGridColumn DataField="CustomerID" TextAlign="Right" Width="100px" />
									<px:PXGridColumn DataField="AdjdDocType" Width="50px" />
									<px:PXGridColumn DataField="AdjdRefNbr" Width="100px" />
									<px:PXGridColumn DataField="AdjNbr" TextAlign="Right" Width="100px" />
									<px:PXGridColumn DataField="ARPayment__Status" Label="Status" Type="DropDownList" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
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
						<Mode AllowFormEdit="True" />
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
			<px:PXButton ID="PXButton9" runat="server" DialogResult="OK" Text="OK" CommandSourceID="ds" />
            <px:PXButton ID="PXButton1" runat="server" DialogResult="Cancel" Text="Cancel" CommandSourceID="ds" />
		</px:PXPanel>
	</px:PXSmartPanel>

    <%-- Recalculate Prices and Discounts --%>
    <px:PXSmartPanel ID="PanelRecalcDiscounts" runat="server" Caption="Recalculate Prices and Discounts" CaptionVisible="true" LoadOnDemand="true" Key="recalcdiscountsfilter"
        AutoCallBack-Enabled="true" AutoCallBack-Target="formRecalcDiscounts" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True"
        CallBackMode-PostData="Page">
        <div style="padding: 5px">
            <px:PXFormView ID="formRecalcDiscounts" runat="server" DataSourceID="ds" CaptionVisible="False" DataMember="recalcdiscountsfilter">
                <Activity Height="" HighlightColor="" SelectedColor="" Width="" />
                <ContentStyle BackColor="Transparent" BorderStyle="None" />
                <Template>
                    <px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM" />
                    <px:PXDropDown ID="edRecalcTerget" runat="server" DataField="RecalcTarget" CommitChanges ="true" />
                    <px:PXCheckBox CommitChanges="True" ID="chkRecalcUnitPrices" runat="server" DataField="RecalcUnitPrices" />
                    <px:PXCheckBox CommitChanges="True" ID="chkOverrideManualPrices" runat="server" DataField="OverrideManualPrices" />
                    <px:PXCheckBox CommitChanges="True" ID="chkRecalcDiscounts" runat="server" DataField="RecalcDiscounts" />
                    <px:PXCheckBox CommitChanges="True" ID="chkOverrideManualDiscounts" runat="server" DataField="OverrideManualDiscounts" />
                </Template>
            </px:PXFormView>
        </div>
        <px:PXPanel ID="PXPanel5" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButton10" runat="server" DialogResult="OK" Text="OK" CommandName="RecalcOk" CommandSourceID="ds" />
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
