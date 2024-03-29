<%@ Page Language="C#" MasterPageFile="~/MasterPages/TabDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="TX205000.aspx.cs" Inherits="Page_TX205000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/TabDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Tax" TypeName="PX.Objects.TX.SalesTaxMaint" BorderStyle="NotSet">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXTab ID="tab" runat="server" DataSourceID="ds" Width="100%" DataMember="Tax" >
		<Items>
			<px:PXTabItem Text="Tax Settings">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
					<px:PXSelector ID="edTaxID" runat="server" DataField="TaxID" 
                        DataSourceID="ds" />
					<px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
					<px:PXDropDown CommitChanges="True" ID="edTaxType" runat="server" DataField="TaxType" />
                    <px:PXCheckBox CommitChanges="True" ID="chkDeductible" runat="server" DataField="DeductibleVAT" />
					<px:PXLayoutRule runat="server" Merge="True" />
					<px:PXCheckBox CommitChanges="True" ID="chkReverseTax" runat="server" DataField="ReverseTax" Size="SM" />
					<px:PXCheckBox CommitChanges="True" ID="chkPendingTax" runat="server" DataField="PendingTax" />
					<px:PXLayoutRule runat="server" />
					<px:PXLayoutRule runat="server" Merge="True" />
					<px:PXCheckBox CommitChanges="True" ID="chkExemptTax" runat="server" DataField="ExemptTax" Size="SM" />
					<px:PXCheckBox CommitChanges="True" ID="chkStatisticalTax" runat="server" DataField="StatisticalTax" />
					<px:PXLayoutRule runat="server" />
					<px:PXCheckBox CommitChanges="True" ID="chkDirectTax" runat="server" DataField="DirectTax" />
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
					<px:PXDropDown CommitChanges="True" ID="edTaxCalcRule" runat="server" DataField="TaxCalcRule" />
					<px:PXDropDown ID="edTaxApplyTermsDisc" runat="server" DataField="TaxApplyTermsDisc" />
					<px:PXCheckBox CommitChanges="True" ID="chkTaxCalcLevel2Exclude" runat="server" DataField="TaxCalcLevel2Exclude" />
					<px:PXSegmentMask CommitChanges="True" ID="edTaxVendorID" runat="server" 
                        DataField="TaxVendorID" AllowEdit="True" DataSourceID="ds" />
				    <px:PXDateTimeEdit ID="OutDate" runat="server" CommitChanges="True" 
                        DataField="OutDate">
                    </px:PXDateTimeEdit>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="GL Accounts">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" />
					<px:PXSegmentMask CommitChanges="True" ID="edSalesTaxAcctID" runat="server" DataField="SalesTaxAcctID" OnValueChanged="edSalesTaxAcctID_ValueChanged" />
					<px:PXSegmentMask CommitChanges="True" ID="edSalesTaxSubID" runat="server" DataField="SalesTaxSubID" />
					<px:PXSegmentMask CommitChanges="True" ID="edPurchTaxAcctID" runat="server" DataField="PurchTaxAcctID" />
					<px:PXSegmentMask CommitChanges="True" ID="edPurchTaxSubID" runat="server" DataField="PurchTaxSubID" />
					<px:PXSegmentMask CommitChanges="True" ID="edExpenseAccountID" runat="server" DataField="ExpenseAccountID" />
					<px:PXSegmentMask CommitChanges="True" ID="edExpenseSubID" runat="server" DataField="ExpenseSubID" />
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" />
					<px:PXSegmentMask ID="edPendingSalesTaxAcctID" runat="server" DataField="PendingSalesTaxAcctID" />
					<px:PXSegmentMask ID="edPendingSalesTaxSubID" runat="server" DataField="PendingSalesTaxSubID" />
					<px:PXSegmentMask ID="edPendingPurchTaxAcctID" runat="server" DataField="PendingPurchTaxAcctID" />
					<px:PXSegmentMask ID="edPendingPurchTaxSubID" runat="server" DataField="PendingPurchTaxSubID" /></Template>
			</px:PXTabItem>
		</Items>
	</px:PXTab>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="PXTab1" runat="server" Height="210px" Width="100%">
		<Items>
			<px:PXTabItem Text="Tax Schedule">
				<Template>
					<px:PXGrid ID="grid1" runat="server" DataSourceID="ds" Height="150px" Width="100%" ActionsPosition="top" AllowSearch="true" SkinID="Details" BorderWidth="0">
						<Levels>
							<px:PXGridLevel DataMember="TaxRevisions">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" />
									<px:PXDateTimeEdit ID="edStartDate" runat="server" DataField="StartDate" />
									<px:PXDropDown ID="edTaxBucketID" runat="server" DataField="TaxBucketID" />
									<px:PXNumberEdit ID="edTaxRate" runat="server" DataField="TaxRate" />
                                    <px:PXNumberEdit ID="edNonDeductibleTaxRate" runat="server" DataField="NonDeductibleTaxRate" />
									<px:PXNumberEdit ID="edTaxableMin" runat="server" DataField="TaxableMin" />
									<px:PXNumberEdit ID="edTaxableMax" runat="server" DataField="TaxableMax" />
									<px:PXCheckBox ID="chkOutdated" runat="server" DataField="Outdated" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="TaxID" />
									<px:PXGridColumn DataField="RevisionID" TextAlign="Right" />
									<px:PXGridColumn DataField="StartDate" Width="90px" />
									<px:PXGridColumn DataField="TaxRate" TextAlign="Right" Width="100px" />
                                    <px:PXGridColumn DataField="NonDeductibleTaxRate" TextAlign="Right" Width="100px" />
									<px:PXGridColumn DataField="TaxableMin" TextAlign="Right" Width="100px" />
									<px:PXGridColumn DataField="TaxableMax" TextAlign="Right" Width="100px" />
									<px:PXGridColumn DataField="TaxBucketID" Width="180px" RenderEditorText="true" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
						<ActionBar>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Categories">
				<Template>
					<px:PXGrid ID="grid2" runat="server" DataSourceID="ds" Height="150px" Width="100%" ActionsPosition="top" AllowSearch="true" BorderWidth="0px" SkinID="Details">
						<Levels>
							<px:PXGridLevel DataMember="Categories">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" />
									<px:PXCheckBox ID="chkTaxCategory__TaxCatFlag" runat="server" DataField="TaxCategory__TaxCatFlag" Enabled="False" />
									<px:PXTextEdit ID="edTaxCategory__Descr" runat="server" DataField="TaxCategory__Descr" Enabled="False" />
									<px:PXSelector ID="edTaxCategoryID" runat="server" DataField="TaxCategoryID" AllowEdit="True" AutoRefresh="True"/>
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="TaxID" Width="81px" Visible="False"  />
									<px:PXGridColumn DataField="TaxCategoryID" Width="81px" AllowShowHide="False" />
									<px:PXGridColumn AllowUpdate="False" DataField="TaxCategory__TaxCatFlag" TextAlign="Center" Type="CheckBox" Width="81px" />
									<px:PXGridColumn AllowUpdate="False" DataField="TaxCategory__Descr" Width="180px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
						<ActionBar>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Zones">
				<Template>
					<px:PXGrid ID="grid3" runat="server" DataSourceID="ds" Height="150px" Width="100%" ActionsPosition="top" AllowSearch="true" BorderWidth="0px" SkinID="Details">
						<Levels>
							<px:PXGridLevel DataMember="Zones">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" />
									<px:PXSelector ID="edTaxZone__DfltTaxCategoryID" runat="server" DataField="TaxZone__DfltTaxCategoryID" Enabled="False" AllowEdit="True" />
									<px:PXTextEdit ID="edTaxZone__Descr" runat="server" DataField="TaxZone__Descr" Enabled="False" />
									<px:PXSelector ID="edTaxZoneID" runat="server" DataField="TaxZoneID" AllowEdit="True" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="TaxID" Width="81px" Visible="False" />
									<px:PXGridColumn DataField="TaxZoneID" Width="81px" AllowShowHide="False" />
									<px:PXGridColumn AllowUpdate="False" DataField="TaxZone__DfltTaxCategoryID" Width="81px" />
									<px:PXGridColumn AllowUpdate="False" DataField="TaxZone__Descr" Width="180px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
						<ActionBar>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="210" />
	</px:PXTab>
</asp:Content>
