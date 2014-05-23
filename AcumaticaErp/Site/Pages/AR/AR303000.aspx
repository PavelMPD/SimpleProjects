<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AR303000.aspx.cs" Inherits="Page_AR303000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.AR.CustomerMaint" PrimaryView="BAccount">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand StartNewGroup="True" Name="First" />
			<px:PXDSCallbackCommand DependOnGrid="grdContacts" Name="ViewContact" Visible="False" />
			<px:PXDSCallbackCommand Name="NewContact" Visible="False" CommitChanges="true" />
			<px:PXDSCallbackCommand DependOnGrid="grdLocations" Name="ViewLocation" Visible="False" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="NewLocation" Visible="False" CommitChanges="true" />
			<px:PXDSCallbackCommand DependOnGrid="grdLocations" Name="SetDefault" Visible="False" /> 
			<px:PXDSCallbackCommand Name="ViewVendor" Visible="False" />

			<px:PXDSCallbackCommand Name="ViewBusnessAccount" Visible="False" />
			<px:PXDSCallbackCommand Name="ViewMainOnMap" Visible="false" />
			<px:PXDSCallbackCommand Name="ViewDefLocationOnMap" Visible="false" />
			<px:PXDSCallbackCommand Name="ViewRestrictionGroups" Visible="False" />
			<px:PXDSCallbackCommand Name="ExtendToVendor" Visible="false" />
			<px:PXDSCallbackCommand Name="CustomerDocuments" Visible="False" />
			<px:PXDSCallbackCommand Name="StatementForCustomer" Visible="False" />
			<px:PXDSCallbackCommand DependOnGrid="grdPaymentMethods" Name="ViewPaymentMethod" Visible="false" StartNewGroup="True" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="AddPaymentMethod" Visible="False" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="NewInvoiceMemo" Visible="False" />
			<px:PXDSCallbackCommand Name="NewSalesOrder" Visible="False" />
			<px:PXDSCallbackCommand Name="NewPayment" Visible="False" />
			<px:PXDSCallbackCommand Name="WriteOffBalance" Visible="False" />
			<px:PXDSCallbackCommand Name="ViewBillAddressOnMap" Visible="false" />
			<px:PXDSCallbackCommand Name="Action" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="Inquiry" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="Report" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="ARBalanceByCustomer" Visible="False" />
			<px:PXDSCallbackCommand Name="CustomerHistory" Visible="False" />
			<px:PXDSCallbackCommand Name="ARAgedPastDue" Visible="False" />
			<px:PXDSCallbackCommand Name="ARAgedOutstanding" Visible="False" />
			<px:PXDSCallbackCommand Name="ARRegister" Visible="False" />
			<px:PXDSCallbackCommand Name="CustomerDetails" Visible="False" />
			<px:PXDSCallbackCommand Name="CustomerStatement" Visible="False" />
			<px:PXDSCallbackCommand Name="CustomerStatementMC" Visible="False" />
			<px:PXDSCallbackCommand StartNewGroup="True" Name="ValidateAddresses" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand StartNewGroup="True" Name="RegenerateLastStatement" Visible="False" CommitChanges="True" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXSmartPanel ID="smpCPMInstance" runat="server" Key="PMInstanceEditor" InnerPageUrl="~/Pages/AR/AR303010.aspx?PopupPanel=On" CaptionVisible="true" Caption="Card Definition" RenderIFrame="true" Visible="False" DesignView="Content">
	</px:PXSmartPanel>
    <px:PXSmartPanel ID="pnlChangeID" runat="server"  Caption="Specify New ID"
        CaptionVisible="true" DesignView="Hidden" LoadOnDemand="true" Key="ChangeIDDialog" CreateOnDemand="false" AutoCallBack-Enabled="true"
        AutoCallBack-Target="formChangeID" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True" CallBackMode-PostData="Page"
        AcceptButtonID="btnOK">
            <px:PXFormView ID="formChangeID" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" CaptionVisible="False"
                DataMember="ChangeIDDialog">
                <ContentStyle BackColor="Transparent" BorderStyle="None" />
                <Template>
                    <px:PXLayoutRule ID="rlAcctCD" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                    <px:PXSegmentMask ID="edAcctCD" runat="server" DataField="CD" />
                </Template>
            </px:PXFormView>
            <px:PXPanel ID="pnlChangeIDButton" runat="server" SkinID="Buttons">
                <px:PXButton ID="btnOK" runat="server" DialogResult="OK" Text="OK" >
                    <AutoCallBack Target="formChangeID" Command="Save" />
                </px:PXButton>
            </px:PXPanel>
    </px:PXSmartPanel>
	<px:PXFormView ID="BAccount" runat="server" Width="100%" Caption="Customer Summary" DataMember="BAccount" NoteIndicator="True" FilesIndicator="True" ActivityIndicator="true" ActivityField="NoteActivity" LinkIndicator="true" NotifyIndicator="true" DefaultControlID="edAcctCD">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
			<px:PXSegmentMask ID="edAcctCD" runat="server" DataField="AcctCD" FilterByAllFields="True" />
			<px:PXLayoutRule runat="server" ColumnSpan="2" />
			<px:PXTextEdit CommitChanges="True" ID="edAcctName" runat="server" DataField="AcctName" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="XS" ControlSize="S" />
			<px:PXDropDown ID="edStatus" runat="server" DataField="Status" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
			<px:PXFormView ID="CustomerBalance" runat="server" DataMember="CustomerBalance" RenderStyle="Simple">
				<Template>
					<px:PXLayoutRule runat="server" ControlSize="SM" LabelsWidth="SM" StartColumn="True"/>
                    <px:PXNumberEdit ID="edBalance" runat="server" DataField="Balance" Enabled="False"/>
                    <px:PXNumberEdit ID="edSignedDepositsBalance" runat="server" DataField="SignedDepositsBalance" Enabled="False" />
                </Template>
			</px:PXFormView>
			<px:PXLayoutRule runat="server"/>
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Height="580px" Style="z-index: 100" Width="100%" DataMember="CurrentCustomer" DataSourceID="ds">
		<AutoSize Enabled="True" Container="Window" MinWidth="300" MinHeight="250"></AutoSize>
		<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
		<Items>
			<px:PXTabItem Text="General Info">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
					<px:PXFormView ID="DefContact" runat="server" Caption="Main Contact" DataMember="DefContact" RenderStyle="Fieldset" DataSourceID="ds">
						<Template>
							<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
							<px:PXTextEdit ID="edFullName" runat="server" DataField="FullName" />
							<px:PXTextEdit ID="edSalutation" runat="server" DataField="Salutation" />
							<px:PXMailEdit ID="edEMail" runat="server" DataField="EMail" CommitChanges="True"/>
							<px:PXLinkEdit ID="edWebSite" runat="server" DataField="WebSite" CommitChanges="True"/>
							<px:PXMaskEdit ID="edPhone1" runat="server" DataField="Phone1" />
							<px:PXMaskEdit ID="edPhone2" runat="server" DataField="Phone2" />
							<px:PXMaskEdit ID="edFax" runat="server" DataField="Fax" />
						</Template>
					</px:PXFormView>
					<px:PXTextEdit ID="edAcctReferenceNbr" runat="server" DataField="AcctReferenceNbr" />
					<px:PXSegmentMask ID="edParentBAccountID" runat="server" DataField="ParentBAccountID" AllowEdit="True" />
					<px:PXFormView ID="DefAddress" runat="server" Caption="Main Address" DataMember="DefAddress" SyncPosition="true" RenderStyle="Fieldset" DataSourceID="ds">
						<Template>
							<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
							<px:PXCheckBox ID="edIsValidated" runat="server" DataField="IsValidated" Enabled="False"/>
							<px:PXTextEdit ID="edAddressLine1" runat="server" DataField="AddressLine1" />
							<px:PXTextEdit ID="edAddressLine2" runat="server" DataField="AddressLine2" />
							<px:PXTextEdit ID="edCity" runat="server" DataField="City" />
							<px:PXSelector ID="edCountryID" runat="server" AllowEdit="True" CommitChanges="True" DataField="CountryID" />
							<px:PXSelector ID="edState" runat="server" AllowEdit="True" AutoRefresh="True" DataField="State" />
							<px:PXLayoutRule runat="server" Merge="True" />
							<px:PXMaskEdit ID="edPostalCode" runat="server" DataField="PostalCode" Size="s" />
							<px:PXButton ID="btnViewMainOnMap" runat="server" CommandName="ViewMainOnMap" CommandSourceID="ds" Text="View on Map" />
							<px:PXLayoutRule runat="server" />
						</Template>
					</px:PXFormView>
					<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Financial Settings" />
					<px:PXSelector CommitChanges="True" ID="edCustomerClassID" runat="server" DataField="CustomerClassID" AllowEdit="True" />
					<px:PXSelector ID="edTermsID" runat="server" DataField="TermsID" AllowEdit="True" />
					<px:PXSelector ID="edStatementCycleId" runat="server" DataField="StatementCycleId" AllowEdit="True" />
					<px:PXCheckBox SuppressLabel="True" ID="chkAutoApplyPayments" runat="server" DataField="AutoApplyPayments" />
					<px:PXCheckBox CommitChanges="True" ID="chkFinChargeApply" runat="server" DataField="FinChargeApply" />
					<px:PXCheckBox CommitChanges="True" ID="chkSmallBalanceAllow" runat="server" DataField="SmallBalanceAllow" />
					<px:PXNumberEdit ID="edSmallBalanceLimit" runat="server" DataField="SmallBalanceLimit" />
					<px:PXLayoutRule runat="server" Merge="True" />
					<px:PXSelector ID="edCuryID" runat="server" DataField="CuryID" AllowEdit="True" Size="S" />
					<px:PXCheckBox ID="chkAllowOverrideCury" runat="server" DataField="AllowOverrideCury" />
					<px:PXLayoutRule runat="server" />
					<px:PXLayoutRule runat="server" Merge="True" />
					<px:PXSelector ID="edCuryRateTypeID" runat="server" DataField="CuryRateTypeID" AllowEdit="True" Size="S" />
					<px:PXCheckBox ID="chkAllowOverrideRate" runat="server" DataField="AllowOverrideRate" />
					<px:PXLayoutRule runat="server" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Credit Verification Rules" />
					<px:PXDropDown CommitChanges="True" ID="edCreditRule" runat="server" DataField="CreditRule" />
					<px:PXNumberEdit ID="edCreditLimit" runat="server" DataField="CreditLimit" CommitChanges="True"/>
                    <px:PXNumberEdit ID="edCreditDaysPastDue" runat="server" DataField="CreditDaysPastDue" />
                    <px:PXFormView ID="CustomerBalance" runat="server" DataMember="CustomerBalance" RenderStyle="Simple">
                        <Template>
                            <px:PXLayoutRule runat="server" ControlSize="SM" LabelsWidth="SM" />
                            <px:PXNumberEdit ID="edUnreleasedBalance" runat="server" DataField="UnreleasedBalance" Enabled="False" />
                            <px:PXNumberEdit ID="edOpenOrdersBalance" runat="server" DataField="OpenOrdersBalance" Enabled="False" />
                            <px:PXNumberEdit ID="edRemainingCreditLimit" runat="server" DataField="RemainingCreditLimit" Enabled="False" />
                            <px:PXDateTimeEdit ID="edOldInvoiceDate" runat="server" DataField="OldInvoiceDate" Enabled="False"/>
                        </Template>
                    </px:PXFormView>
                </Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Billing Settings" LoadOnDemand="True">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Bill-To Contact" />
					<px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkIsBillContSameAsMain" runat="server" DataField="IsBillContSameAsMain" />
					<px:PXFormView ID="BillContact" runat="server" DataMember="BillContact" RenderStyle="Simple" DataSourceID="ds">
						<Template>
							<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
							<px:PXTextEdit ID="edFullName" runat="server" DataField="FullName" />
							<px:PXTextEdit ID="edSalutation" runat="server" DataField="Salutation" />
							<px:PXMaskEdit ID="edPhone1" runat="server" DataField="Phone1" />
							<px:PXMaskEdit ID="edPhone2" runat="server" DataField="Phone2" />
							<px:PXMaskEdit ID="edFax" runat="server" DataField="Fax" />
							<px:PXMailEdit ID="edEMail" runat="server" DataField="EMail" CommitChanges="True"/>
							<px:PXLinkEdit ID="edWebSite" runat="server" DataField="WebSite" CommitChanges="True"/>
						</Template>
					</px:PXFormView>
					<px:PXLayoutRule runat="server" GroupCaption="Bill-To Address" StartGroup="True" />
					<px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkIsBillSameAsMain" runat="server" DataField="IsBillSameAsMain" />
					<px:PXLayoutRule runat="server"/>
					<px:PXFormView ID="BillAddress" runat="server" DataMember="BillAddress" RenderStyle="Simple" SyncPosition="true" DataSourceID="ds">
						<Template>
							<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
							<px:PXCheckBox ID="edIsValidated" runat="server" DataField="IsValidated" Enabled="False"/>
							<px:PXTextEdit ID="edAddressLine1" runat="server" DataField="AddressLine1" />
							<px:PXTextEdit ID="edAddressLine2" runat="server" DataField="AddressLine2" />
							<px:PXTextEdit ID="edCity" runat="server" DataField="City" />
							<px:PXSelector ID="edCountryID" runat="server" AllowEdit="True" AutoRefresh="True" CommitChanges="True" DataField="CountryID" />
							<px:PXSelector ID="edState" runat="server" AllowEdit="True" AutoRefresh="True" DataField="State" />
							<px:PXLayoutRule runat="server" Merge="True" />
							<px:PXMaskEdit ID="edPostalCode" runat="server" DataField="PostalCode" Size="s" />
							<px:PXButton ID="btnViewBillAddressOnMap" runat="server" CommandName="ViewBillAddressOnMap" CommandSourceID="ds" Text="View on Map" />
							<px:PXLayoutRule runat="server" />
						</Template>
					</px:PXFormView>
					<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Print and Email Settings" />
					<px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkPrintInvoices" runat="server" DataField="PrintInvoices" />
					<px:PXCheckBox SuppressLabel="True" ID="chkMailInvoices" runat="server" DataField="MailInvoices" />
					<px:PXCheckBox SuppressLabel="True" ID="chkPrintStatements" runat="server" DataField="PrintStatements" />
					<px:PXCheckBox SuppressLabel="True" ID="chkSendStatementByEmails" runat="server" DataField="SendStatementByEmail" />
					<px:PXCheckBox SuppressLabel="True" ID="chkPrintCuryStatements" runat="server" DataField="PrintCuryStatements" />
					<px:PXDropDown CommitChanges="True" ID="edStatementType" runat="server" DataField="StatementType" />
					<px:PXLayoutRule runat="server" GroupCaption="Default Payment Method" StartGroup="True" />
					<px:PXSelector ID="edDefPaymentMethodID" runat="server" DataField="DefPaymentMethodID" AllowEdit="True" />
					<px:PXFormView ID="DefPaymentMethodInstance" runat="server" DataMember="DefPaymentMethodInstance" RenderStyle="Simple">
						<Template>
							<px:PXLayoutRule runat="server" StartGroup="True" ControlSize="XM" LabelsWidth="SM" />
							<px:PXSegmentMask CommitChanges="True" ID="edCashAccountID" runat="server" DataField="CashAccountID" AllowEdit="True" />
							<px:PXTextEdit ID="edDescr" runat="server" DataField="Descr"/>
							<px:PXGrid ID="grdPMInstanceDetails" runat="server" MatrixMode="True" Caption="Payment Method Details" SkinID="Attributes" Height="160px" Width="400px" DataSourceID="ds">
								<Levels>
									<px:PXGridLevel DataMember="DefPaymentMethodInstanceDetails" DataKeyNames="PMInstanceID,PaymentMethodID,DetailID">
										<Columns>
											<px:PXGridColumn DataField="DetailID_PaymentMethodDetail_descr" Width="150px" />
											<px:PXGridColumn DataField="Value" Width="200px" />
										</Columns>
										<Layout FormViewHeight="" />
									</px:PXGridLevel>
								</Levels>
							</px:PXGrid>
						</Template>
					</px:PXFormView>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Delivery Settings" LoadOnDemand="True">
				<Template>
					<px:PXFormView ID="DefLocation" runat="server" DataMember="DefLocation" SkinID="Transparent" Width="100%" >
						<Template>
							<px:PXLayoutRule runat="server" StartColumn="True" StartGroup="True" GroupCaption="Shipping Contact" LabelsWidth="SM" ControlSize="XM" />
							<px:PXCheckBox CommitChanges="True" ID="chkIsContactSameAsMain" runat="server" DataField="IsContactSameAsMain" />
							<px:PXFormView ID="DefLocationContact" runat="server" DataMember="DefLocationContact" RenderStyle="Simple">
								<Template>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
									<px:PXTextEdit ID="edFullName" runat="server" DataField="FullName" />
									<px:PXTextEdit ID="edSalutation" runat="server" DataField="Salutation" />
									<px:PXMailEdit ID="edEMail" runat="server" DataField="EMail" CommitChanges="True"/>
									<px:PXLinkEdit ID="edWebSite" runat="server" DataField="WebSite" CommitChanges="True"/>
									<px:PXMaskEdit ID="edPhone1" runat="server" DataField="Phone1" />
									<px:PXMaskEdit ID="edPhone2" runat="server" DataField="Phone2" />
									<px:PXMaskEdit ID="edFax" runat="server" DataField="Fax" />
								</Template>
							</px:PXFormView>
							<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Shipping Address" />
							<px:PXCheckBox CommitChanges="True" ID="chkIsMain" runat="server" DataField="IsAddressSameAsMain" />
							<px:PXCheckBox ID="edIsValidated" runat="server" DataField="IsValidated" Enabled="False"/>
							<px:PXTextEdit ID="edAddressLine1" runat="server" DataField="AddressLine1" />
							<px:PXTextEdit ID="edAddressLine2" runat="server" DataField="AddressLine2" />
							<px:PXTextEdit ID="edCity" runat="server" DataField="City" />
							<px:PXSelector CommitChanges="True" ID="edCountryID" runat="server" DataField="CountryID" AllowEdit="True" />
							<px:PXSelector ID="edState" runat="server" AutoRefresh="True" DataField="State" AllowEdit="True" />
							<px:PXLayoutRule runat="server" Merge="True" />
							<px:PXMaskEdit Size="s" ID="edPostalCode" runat="server" DataField="PostalCode" />
							<px:PXButton ID="btnViewDefLoactionOnMap" runat="server" CommandName="ViewDefLocationOnMap" CommandSourceID="ds" Text="View on Map" />
							<px:PXLayoutRule runat="server" />
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
							<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Default Location Settings" />
							<%--<px:PXSegmentMask ID="edLocationCD" runat="server" DataField="LocationCD" AllowEdit="True" />--%>
							<px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
							<px:PXTextEdit ID="edTaxRegistrationID" runat="server" DataField="TaxRegistrationID" />
							<px:PXSelector ID="edCTaxZoneID" runat="server" DataField="CTaxZoneID" AllowEdit="True" />
							<px:PXSelector ID="edCBranchID" runat="server" DataField="CBranchID" AllowEdit="True" />
							<px:PXSelector ID="edCPriceClassID" runat="server" DataField="CPriceClassID" AllowEdit="True" />
							<px:PXLayoutRule runat="server" GroupCaption="Shipping Instructions" />
							<px:PXSegmentMask ID="edCSiteID" runat="server" DataField="CSiteID" AllowEdit="True"  CommitChanges="True" AutoRefresh="True"/>
							<px:PXSelector CommitChanges="True" ID="edCarrierID" runat="server" DataField="CCarrierID" AllowEdit="True" />
							<px:PXSelector ID="edShipTermsID" runat="server" DataField="CShipTermsID" AllowEdit="True" />
							<px:PXSelector ID="edShipZoneID" runat="server" DataField="CShipZoneID" AllowEdit="True" />
							<px:PXSelector ID="edFOBPointID" runat="server" DataField="CFOBPointID" AllowEdit="True" />
                            <px:PXCheckBox ID="chkResedential" runat="server" DataField="CResedential" />
                            <px:PXCheckBox ID="chkSaturdayDelivery" runat="server" DataField="CSaturdayDelivery" />
							<px:PXCheckBox ID="PXCheckBox1" runat="server" DataField="CInsurance" />
							<px:PXDropDown ID="edCShipComplete" runat="server" DataField="CShipComplete" />
							<px:PXNumberEdit ID="edCOrderPriority" runat="server" DataField="COrderPriority" />
							<px:PXNumberEdit ID="edLeadTime" runat="server" DataField="CLeadTime" />
							
							<px:PXGrid ID="PXGridAccounts" runat="server" DataSourceID="ds" AllowFilter="False" Height="80px" Width="400px" SkinID="ShortList" FastFilterFields="CustomerID,CustomerID_description,CarrierAccount"
								CaptionVisible="True" Caption="Carrier Accounts">
								<Levels>
									<px:PXGridLevel DataMember="Carriers">
										<Columns>
											<px:PXGridColumn DataField="IsActive" Label="Active" TextAlign="Center" Type="CheckBox" Width="40px" />
											<px:PXGridColumn AutoCallBack="True" DataField="CarrierPluginID" Label="Carrier" Width="100px" />
											<px:PXGridColumn DataField="CarrierAccount" Width="100px" />
											<px:PXGridColumn DataField="PostalCode" Width="100px">
											</px:PXGridColumn>
										</Columns>
										<Layout FormViewHeight="" />
									</px:PXGridLevel>
								</Levels>
							</px:PXGrid>
						</Template>
					</px:PXFormView>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Locations" LoadOnDemand="True">
				<Template>
					<px:PXGrid ID="grdLocations" runat="server" Height="300px" Width="100%" AllowSearch="True" SkinID="DetailsInTab" DataSourceID="ds">
						<Mode AllowAddNew="False" AllowColMoving="False" AllowDelete="False" />
						<ActionBar>
							<Actions>
								<Save Enabled="False" />
								<AddNew Enabled="False" />
								<Delete Enabled="False" />
								<EditRecord Enabled="False" />
							</Actions>
							<CustomItems>
								<px:PXToolBarButton Text="New Location">
									<AutoCallBack Command="NewLocation" Target="ds" />
								    <PopupCommand Command="Refresh" Target="grdLocations" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Key="cmdViewLocation" Text="Location Details">
									<AutoCallBack Command="ViewLocation" Target="ds" />
								    <PopupCommand Command="Refresh" Target="grdLocations" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Text="Set as Default">
									<AutoCallBack Command="SetDefault" Target="ds" />
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
						<AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
						<Levels>
							<px:PXGridLevel DataMember="Locations" DataKeyNames="LocationBAccountID,LocationCD">
								<RowTemplate>
									<px:PXSelector ID="edLocationCD" runat="server" DataField="LocationCD" AllowEdit="True" />
									<px:PXSelector ID="edCPriceClassID" runat="server" DataField="CPriceClassID" AllowEdit="True" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="LocationBAccountID" TextAlign="Right" Visible="False" AllowShowHide="False" />
									<px:PXGridColumn DataField="LocationID" TextAlign="Right" Visible="False" />
									<px:PXGridColumn DataField="IsActive" TextAlign="Center" Type="CheckBox" Width="60px" />
									<px:PXGridColumn DataField="IsDefault" TextAlign="Center" Type="CheckBox" Width="60px" />
									<px:PXGridColumn DataField="LocationCD" LinkCommand="ViewLocation" />
									<px:PXGridColumn DataField="Descr" Width="200px" />
									<px:PXGridColumn DataField="City" Width="200px" />
									<px:PXGridColumn DataField="CountryID" AutoCallBack="True" />
									<px:PXGridColumn DataField="State" Width="100px" />
									<px:PXGridColumn DataField="CTaxZoneID" Width="80px" />
									<px:PXGridColumn DataField="CPriceClassID" Width="81px" />
									<px:PXGridColumn DataField="CSalesAcctID" Width="108px" AutoCallBack="True" />
									<px:PXGridColumn DataField="CSalesSubID" Width="100px" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<Mode AllowUpdate="False" AllowAddNew="False" AllowDelete="False"></Mode>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Payment Methods" LoadOnDemand="True">
				<Template>
					<px:PXGrid ID="grdPaymentMethods" runat="server" Height="300px" Width="100%" SkinID="DetailsInTab" DataSourceID="ds">
						<Levels>
							<px:PXGridLevel DataMember="PaymentMethods">
								<Columns>
									<px:PXGridColumn DataField="IsDefault" TextAlign="Center" Type="CheckBox" Width="60px" />
									<px:PXGridColumn DataField="PaymentMethodID" />
									<px:PXGridColumn DataField="Descr" Width="200px" />
									<px:PXGridColumn DataField="CashAccountID" />
									<px:PXGridColumn DataField="IsActive" TextAlign="Center" Type="CheckBox" Width="60px" />
									<px:PXGridColumn DataField="IsCustomerPaymentMethod" TextAlign="Center" Type="CheckBox" Width="70px" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
						<ActionBar PagerVisible="False" DefaultAction="cmdViewPaymentMethod" PagerActionsText="True">
							<Actions>
								<Save Enabled="False" />
								<AddNew Enabled="False" />
								<Delete Enabled="False" />
								<EditRecord Enabled="False" />
							</Actions>
							<CustomItems>
								<px:PXToolBarButton Text="Add Payment Method">
								    <AutoCallBack Command="AddPaymentMethod" Target="ds" />
								    <PopupCommand Command="Cancel" Target="ds" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Key="cmdViewPaymentMethod" Text="View Payment Method">
								    <AutoCallBack Command="ViewPaymentMethod" Target="ds" />
								    <PopupCommand Command="Cancel" Target="ds" />
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Contacts" LoadOnDemand="True">
				<Template>
					<px:PXGrid ID="grdContacts" runat="server" Height="300px" Width="100%" ActionsPosition="Top" AllowSearch="True" SkinID="DetailsInTab" DataSourceID="ds" >
						<Levels>
							<px:PXGridLevel DataMember="ExtContacts">
								<Columns>
									<px:PXGridColumn DataField="ContactBAccountID" TextAlign="Right" Visible="False" AllowShowHide="False" />
									<px:PXGridColumn DataField="ContactID" TextAlign="Right" />
									<px:PXGridColumn DataField="IsActive" TextAlign="Center" Type="CheckBox" Width="60px" />
									<px:PXGridColumn DataField="Salutation" Width="160px" />
									<px:PXGridColumn DataField="ContactDisplayName" Width="280px" LinkCommand="ViewContact" />
									<px:PXGridColumn DataField="City" Width="180px" />
									<px:PXGridColumn DataField="EMail" Width="200px" />
									<px:PXGridColumn DataField="Phone1" Width="140px" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
						<LevelStyles>
							<RowForm Height="500px" Width="920px">
							</RowForm>
						</LevelStyles>
						<Mode AllowColMoving="False" AllowAddNew="False" AllowDelete="False" AllowUpdate ="False"/>
						<ActionBar DefaultAction="cmdViewContact">
							<Actions>
								<Save Enabled="False" />
								<AddNew Enabled="False" />
								<Delete Enabled="False" />
								<EditRecord Enabled="False" />
							</Actions>
							<CustomItems>
								<px:PXToolBarButton Text="New Contact">
								    <AutoCallBack Command="NewContact" Target="ds" />
								    <PopupCommand Command="Refresh" Target="grdContacts" />
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Salespersons" LoadOnDemand="True">
				<Template>
					<px:PXGrid ID="PXGrid1" runat="server" Height="300px" Width="100%" SkinID="DetailsInTab" DataSourceID="ds">
						<Levels>
							<px:PXGridLevel DataMember="SalesPersons" DataKeyNames="SalesPersonID,LocationID">
								<Columns>
									<px:PXGridColumn DataField="SalesPersonID" Width="108px" />
									<px:PXGridColumn DataField="SalesPersonID_SalesPerson_descr" />
									<px:PXGridColumn DataField="LocationID" Width="54px" />
									<px:PXGridColumn DataField="LocationID_description" Width="108px" />
									<px:PXGridColumn DataField="CommisionPct" TextAlign="Right" Width="108px" />
									<px:PXGridColumn DataField="IsDefault" Width="60px" Type="CheckBox" TextAlign="Center" />
								</Columns>
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
									<px:PXSegmentMask ID="edSalesPersonID" runat="server" DataField="SalesPersonID" AutoRefresh="True" AllowEdit="True" />
									<px:PXSegmentMask ID="edLocationID" runat="server" DataField="LocationID" AutoRefresh="True" AllowEdit="True" />
									<px:PXTextEdit ID="edLocation_descr" runat="server" DataField="LocationID_description" Enabled="False" />
									<px:PXNumberEdit ID="edCommisionPct" runat="server" DataField="CommisionPct" />
								</RowTemplate>
								<Mode InitNewRow="False" />
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
						<ActionBar>
							<Actions>
								<Save Enabled="False" />
							</Actions>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="GL Accounts">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
					<px:PXFormView ID="DefLocationAccount" runat="server" DataMember="DefLocation" RenderStyle="Simple">
						<Template>
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
							<px:PXSegmentMask CommitChanges="True" ID="edCARAccountID" runat="server" DataField="CARAccountID" />
							<px:PXSegmentMask ID="edCARSubID" runat="server" DataField="CARSubID" AutoRefresh="True" AllowEdit="True" />
							<px:PXSegmentMask CommitChanges="True" ID="edCSalesAcctID" runat="server" DataField="CSalesAcctID" />
							<px:PXSegmentMask ID="edCSalesSubID" runat="server" DataField="CSalesSubID" AutoRefresh="True" AllowEdit="True" />
							<px:PXSegmentMask CommitChanges="True" ID="edCDiscountAcctID" runat="server" DataField="CDiscountAcctID" />
							<px:PXSegmentMask ID="edCDiscountSubID" runat="server" DataField="CDiscountSubID" AutoRefresh="True" AllowEdit="True" />
							<px:PXSegmentMask CommitChanges="True" ID="edCFreightAcctID" runat="server" DataField="CFreightAcctID" />
							<px:PXSegmentMask ID="edCFreightSubID" runat="server" DataField="CFreightSubID" AutoRefresh="True" AllowEdit="True" />
						</Template>
					</px:PXFormView>
					<px:PXSegmentMask CommitChanges="True" ID="edDiscTakenAcctID" runat="server" DataField="DiscTakenAcctID" />
					<px:PXSegmentMask ID="edDiscTakenSubID" runat="server" DataField="DiscTakenSubID" AutoRefresh="True" AllowEdit="True" />
					<px:PXSegmentMask CommitChanges="True" ID="edPrepaymentAcctID" runat="server" DataField="PrepaymentAcctID" />
					<px:PXSegmentMask ID="edPrepaymentSubID" runat="server" DataField="PrepaymentSubID" AutoRefresh="True" AllowEdit="True" />
					<px:PXSegmentMask CommitChanges="True" ID="edCOGSAcctID" runat="server" DataField="COGSAcctID" AllowEdit="True" />
					<px:PXSegmentMask ID="edCOGSSubID" runat="server" DataField="COGSSubID" AutoRefresh="True" AllowEdit="True" />
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Mailing Settings" Overflow="Hidden" LoadOnDemand="True">
				<Template>
					<px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="300" SkinID="Horizontal" Height="500px">
						<AutoSize Enabled="true" />
						<Template1>
							<px:PXGrid ID="gridNS" runat="server" SkinID="DetailsInTab" Width="100%" Height="150px" Caption="Mailings"
								AdjustPageSize="Auto" AllowPaging="True" DataSourceID="ds">
								<AutoSize Enabled="True" />
								<AutoCallBack Target="gridNR" Command="Refresh" />
								<Levels>
									<px:PXGridLevel DataMember="NotificationSources" DataKeyNames="SourceID,SetupID">
										<RowTemplate>
											<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
											<px:PXDropDown ID="edFormat" runat="server" DataField="Format" />
											<px:PXSegmentMask ID="edNBranchID" runat="server" DataField="NBranchID" />
											<px:PXCheckBox ID="chkActive" runat="server" Checked="True" DataField="Active" />
											<px:PXSelector ID="edSetupID" runat="server" DataField="SetupID" />
											<px:PXSelector ID="edReportID" runat="server" DataField="ReportID" ValueField="ScreenID" />
											<px:PXSelector ID="edNotificationID" runat="server" DataField="NotificationID" ValueField="Name" />
											<px:PXSelector ID="edEMailAccountID" runat="server" DataField="EMailAccountID" DisplayMode="Text" />
										</RowTemplate>
										<Columns>
											<px:PXGridColumn DataField="SetupID" Width="108px" AutoCallBack="True" />
											<px:PXGridColumn DataField="NBranchID" AutoCallBack="True" Label="Branch" />
											<px:PXGridColumn DataField="EMailAccountID" Width="200px" DisplayMode="Text" />
											<px:PXGridColumn DataField="ReportID" Width="150px" AutoCallBack="True" />
											<px:PXGridColumn DataField="NotificationID" Width="150px" AutoCallBack="True" />
											<px:PXGridColumn DataField="Format" Width="54px" RenderEditorText="True" AutoCallBack="True" />
											<px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox" />
											<px:PXGridColumn DataField="OverrideSource" TextAlign="Center" Type="CheckBox" AutoCallBack="True" />
										</Columns>
										<Layout FormViewHeight="" />
									</px:PXGridLevel>
								</Levels>
							</px:PXGrid>
						</Template1>
						<Template2>
							<px:PXGrid ID="gridNR" runat="server" SkinID="DetailsInTab" Width="100%" Caption="Recipients" AdjustPageSize="Auto"
								AllowPaging="True" DataSourceID="ds">
								<AutoSize Enabled="True" />
								<Mode InitNewRow="True"></Mode>
								<Parameters>
									<px:PXSyncGridParam ControlID="gridNS" />
								</Parameters>
								<CallbackCommands>
									<Save RepaintControls="None" RepaintControlsIDs="ds" />
									<FetchRow RepaintControls="None" />
								</CallbackCommands>
								<Levels>
									<px:PXGridLevel DataMember="NotificationRecipients" DataKeyNames="NotificationID">
										<Mode InitNewRow="True"></Mode>
										<Columns>
											<px:PXGridColumn DataField="ContactType" RenderEditorText="True" Width="100px" AutoCallBack="True" />
											<px:PXGridColumn DataField="OriginalContactID" Visible="False" AllowShowHide="False" />
											<px:PXGridColumn DataField="ContactID" Width="200px">
												<NavigateParams>
													<px:PXControlParam Name="ContactID" ControlID="gridNR" PropertyName="DataValues[&quot;OriginalContactID&quot;]" />
												</NavigateParams>
											</px:PXGridColumn>
											<px:PXGridColumn DataField="Email" Width="200px" />
											<px:PXGridColumn DataField="Format" RenderEditorText="True" Width="60px" AutoCallBack="True" />
											<px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox" Width="60px" />
											<px:PXGridColumn DataField="Hidden" TextAlign="Center" Type="CheckBox" Width="60px" />
										</Columns>
										<RowTemplate>
											<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
											<px:PXDropDown ID="edContactType" runat="server" DataField="ContactType" />
											<px:PXSelector ID="edContactID" runat="server" DataField="ContactID" AutoRefresh="True" ValueField="DisplayName"
												AllowEdit="True" />
										</RowTemplate>
										<Layout FormViewHeight="" />
									</px:PXGridLevel>
								</Levels>
							</px:PXGrid>
						</Template2>
					</px:PXSplitContainer>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="300" MinWidth="300" />
	</px:PXTab>
</asp:Content>
