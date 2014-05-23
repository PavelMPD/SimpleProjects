<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="CR306100.aspx.cs" Inherits="Page_CR306100" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.CR.ServiceCaseMaint"
		PrimaryView="ServiceCase">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Cancel" PopupVisible="true" />
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand Name="Save" PopupVisible="True" ClosePopup="True" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand Name="CurrencyView" Visible="False" />
			<px:PXDSCallbackCommand Name="Release" StartNewGroup="true" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="Action" StartNewGroup="true" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="Assign" Visible="False" RepaintControls="All" />
			<px:PXDSCallbackCommand Name="ViewInvoice" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="NewTask" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="NewEvent" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="NewMailActivity" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="NewActivity" Visible="False" CommitChanges="True" />
			<%--<px:PXDSCallbackCommand Name="Action@NewMailActivity" Visible="False" CommitChanges="True"
				PopupCommand="Cancel" PopupCommandTarget="ds" />
			<px:PXDSCallbackCommand Name="Action@NewEvent" Visible="False" CommitChanges="True"
				PopupCommand="Cancel" PopupCommandTarget="ds" />
			<px:PXDSCallbackCommand Name="Action@NewTask" Visible="False" CommitChanges="True"
				PopupCommand="Cancel" PopupCommandTarget="ds" />
			<px:PXDSCallbackCommand Name="Action@NewActivity" Visible="False" CommitChanges="True"
				PopupCommand="Cancel" PopupCommandTarget="ds" />--%>
			<px:PXDSCallbackCommand Name="ViewActivity" DependOnGrid="gridActivities" Visible="False"
				CommitChanges="true" />
			<px:PXDSCallbackCommand Name="OpenActivityOwner" Visible="False" CommitChanges="True"
				DependOnGrid="gridActivities" />
			<px:PXDSCallbackCommand CommitChanges="True" Visible="False" Name="LSServiceCaseItem_generateLotSerial" />
			<px:PXDSCallbackCommand CommitChanges="True" Visible="False" Name="LSServiceCaseItem_binLotSerial" DependOnGrid="grdDetails" />
		</CallbackCommands>
		<DataTrees>
			<px:PXTreeDataMember TreeView="_EPCompanyTree_Tree_" TreeKeys="WorkgroupID" />
		</DataTrees>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100"
		Width="100%"  DataMember="ServiceCase" Caption="Call Summary" 
		 NoteIndicator="True" FilesIndicator="True" 
		DefaultControlID="edServiceCaseCD" >
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True"  LabelsWidth="SM" ControlSize="SM" />
			<px:PXSelector ID="edServiceCaseCD" runat="server" DataField="ServiceCaseCD">
				<GridProperties>
				<PagerSettings Mode="NextPrevFirstLast" /></GridProperties>
			</px:PXSelector>
			<px:PXDateTimeEdit ID="edCreatedDateTime" runat="server" DataField="CreatedDateTime" DisplayFormat="g" Enabled="False"  />
			<px:PXLayoutRule ID="PXLayoutRule2" runat="server" Merge="True" />
			<px:PXDropDown CommitChanges="True" Size="s" ID="edStatus" runat="server" DataField="Status" SelectedIndex="-1"  />
			<px:PXCheckBox ID="edIsHode" runat="server" DataField="Hold" />
			<px:PXLayoutRule ID="PXLayoutRule3" runat="server" Merge="False" />
			<px:PXLabel runat="server" />
			<px:PXLayoutRule ID="PXLayoutRule4" runat="server" ColumnSpan="2" />
			<px:PXTextEdit ID="edSubject" runat="server" DataField="Subject"  />
			<px:PXLayoutRule ID="PXLayoutRule5" runat="server" StartColumn="True"  LabelsWidth="s" ControlSize="SM" />
			<px:PXSegmentMask CommitChanges="True" ID="edCustomerID" runat="server" DataField="CustomerID" AllowEdit="True" />
			<px:PXLayoutRule ID="PXLayoutRule6" runat="server" Merge="True" />
			<px:PXSegmentMask CommitChanges="True" ID="edLocationID" runat="server" DataField="LocationID" AutoRefresh="true" AllowEdit="True" />
			<px:PXLabel Size="xs" ID="lblLocationIDH" runat="server" />
			<px:PXLayoutRule ID="PXLayoutRule7" runat="server" Merge="False" />
			<px:PXSelector CommitChanges="True" ID="edContactID" runat="server" DataField="ContactID"
				 TextField="displayName" AutoRefresh="True" AllowEdit="True">
				<GridProperties FastFilterFields="DisplayName"></GridProperties>
			</px:PXSelector>
			<pxa:PXCurrencyRate  DataField="CuryID" ID="edCury" runat="server"
				DataSourceID="ds" RateTypeView="_CRServiceCase_CurrencyInfo_" DataMember="_Currency_">
			</pxa:PXCurrencyRate>
			<px:PXLayoutRule ID="PXLayoutRule8" runat="server" StartColumn="True"  LabelsWidth="S" ControlSize="SM" />
			<px:PXTreeSelector CommitChanges="True" ID="edWorkgroupID" runat="server" DataField="WorkgroupID" TreeDataMember="_EPCompanyTree_Tree_"
				TreeDataSourceID="ds"  PopulateOnDemand="True"
				InitialExpandLevel="0" ShowRootNode="false">
				<DataBindings>
					<px:PXTreeItemBinding TextField="Description" ValueField="Description" />
				</DataBindings>
			</px:PXTreeSelector>
			<px:PXSelector CommitChanges="True" ID="edOwnerID" runat="server" DataField="OwnerID" AutoRefresh="True" TextField="AcctName" AllowEdit="true" />
			<px:PXNumberEdit ID="edCostEstimated" runat="server" DataField="EstimatedCost" Enabled="False"  />
			<px:PXNumberEdit ID="edCostFinal" runat="server" DataField="FinalCost" Enabled="False"  />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Width="100%" Height="400px" DataSourceID="ds" 
		DataMember="ServiceCaseCurrent"  >
		<Items>
			<px:PXTabItem Text="Equipment Summary" >
				<Template>
					<px:PXPanel ID="PXPanel1" runat="server" >
					<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM" />								
					<px:PXSelector CommitChanges="True" ID="edWarrantyNbr" runat="server" DataField="ServiceItemID" ValueField="ServiceItemID" TextField="ServiceItemCD" AllowEdit="True" AutoRefresh="true"  />
					<px:PXSelector CommitChanges="True" ID="edInventoryID" runat="server" DataField="Equipment__InventoryID" ValueField="InventoryID" TextField="InventoryCD" AllowEdit="True" TextMode="Search"  />
					<px:PXSelector CommitChanges="True" ID="edLotSerialNbr" runat="server" DataField="Equipment__LotSerialNbr" ValueField="LotSerialNbr" AutoRefresh="true" TextMode="Search"  />
					<px:PXTextEdit ID="edWarranty" runat="server" DataField="Equipment__WarrantyNbr" />
					<px:PXTextEdit ID="edModel" runat="server" DataField="Equipment__Model" />
					<px:PXTextEdit ID="edManufacture" runat="server" DataField="Equipment__Manufacture" />
					<px:PXLayoutRule ID="PXLayoutRule26" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />								
					<px:PXDateTimeEdit ID="edExpireDate" runat="server" DataField="Equipment__ExpireDate" DisplayFormat="d" Enabled="False" />
					<px:PXDateTimeEdit ID="edLastServiceDate" runat="server" DataField="Equipment__LastServiceDate" DisplayFormat="d" Enabled="False" />
					<px:PXDateTimeEdit ID="edLastIncidentDate" runat="server" DataField="Equipment__LastIncidentDate" DisplayFormat="d" Enabled="False"/>
					<px:PXDateTimeEdit ID="edNextServiceDate" runat="server" DataField="Equipment__NextServiceDate" DisplayFormat="d" Enabled="False" />
					<px:PXTextEdit ID="edNextServiceCode" runat="server" DataField="Equipment__NextServiceCode" Enabled="False" Size="S"/>
					</px:PXPanel>
					<pxa:PXRichTextEdit ID="edDescription" runat="server" DataField="Description" Style="border-width: 0px;
						width: 100%; height: 100%;">
							<AutoSize Enabled="True" MinHeight="216" />
					</pxa:PXRichTextEdit>
				</Template> 
			</px:PXTabItem>
			<px:PXTabItem Text="Call Details">
				<Template>
					<px:PXGrid ID="grdDetails" runat="server" DataSourceID="ds" Style="z-index: 100; left: 0px;
						top: 0px; height: 332px;" Width="100%" 
						BorderWidth="0px" SkinID="Details" >
						<levels>
							<px:PXGridLevel DataMember="Details" DataKeyNames="ServiceCaseID,LineNbr">
								<Columns>
									<px:PXGridColumn DataField="InventoryID" Width="135px" 
										AutoCallBack="True" RenderEditorText="True" TextField="InventoryItem__InventoryCD" >
										<NavigateParams>
											<px:PXControlParam Name="InventoryID" ControlID="grdDetails" 
												PropertyName="DataValues[&quot;InventoryID&quot;]" />
										</NavigateParams>
									</px:PXGridColumn>
									<px:PXGridColumn DataField="SubItemID" Width="80px" AutoCallBack="True" />
									<px:PXGridColumn DataField="Description" Width="120px" />
									<px:PXGridColumn DataField="IsFree" Width="60px" AutoCallBack="True" AllowNull="False" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn DataField="SiteID" Width="80px" />
									<px:PXGridColumn DataField="LocationID" Width="81px" AllowShowHide="False" NullText="<SPLIT>" Required="true" />
									<px:PXGridColumn DataField="UOM" Width="63px" AutoCallBack="True" />
									<px:PXGridColumn DataField="EstimatedQuantity" Width="108px" AutoCallBack="True" TextAlign="Right" DataType="Decimal" />
									<px:PXGridColumn DataField="ActualQuantity" Width="108px" AutoCallBack="True" TextAlign="Right" DataType="Decimal" />
									<px:PXGridColumn DataField="CuryUnitPrice" Width="80px" Decimals="4" TextAlign="Right" DataType="Decimal" />
									<px:PXGridColumn DataField="Discount" Width="80px" AutoCallBack="True" Decimals="6" TextAlign="Right" DataType="Decimal" />
									<px:PXGridColumn DataField="EstimatedCuryDiscountAmount" Width="80px" Decimals="4" TextAlign="Right" DataType="Decimal" />
									<px:PXGridColumn DataField="ActualCuryDiscountAmount" AutoCallBack="True" Width="80px" Decimals="4" TextAlign="Right" DataType="Decimal" />
									<px:PXGridColumn DataField="EstimatedCuryAmount" Width="80px" Decimals="4" TextAlign="Right" DataType="Decimal" />
									<px:PXGridColumn DataField="ActualCuryAmount" AutoCallBack="True" Width="80px" Decimals="4" TextAlign="Right" DataType="Decimal" />
								</Columns>
								<RowTemplate>
									<px:PXSelector ID="edInventoryID_Dtl" runat="server" 
										DataField="InventoryID" DataSourceID="ds" DataKeyNames="InventoryCD"
										ValueField="InventoryID" TextField="InventoryCD" TextMode="Search" AllowEdit="true" >
										<GridProperties>
											<Columns>
												<px:PXGridColumn DataField="InventoryCD" MaxLength="30" Width="120px">
													<Header Text="Inventory">
													</Header>
												</px:PXGridColumn>
												<px:PXGridColumn DataField="Descr" MaxLength="60" Width="200px">
													<Header Text="Description">
													</Header>
												</px:PXGridColumn>
												<px:PXGridColumn DataField="ItemClassID" MaxLength="10">
													<Header Text="Inventory Class">
													</Header>
												</px:PXGridColumn>
												<px:PXGridColumn AllowNull="False" DataField="ItemStatus" MaxLength="2">
													<Header Text="Item Status">
													</Header>
												</px:PXGridColumn>
												<px:PXGridColumn AllowNull="False" DataField="ItemType" MaxLength="1">
													<Header Text="Type">
													</Header>
												</px:PXGridColumn>
											</Columns>
											<PagerSettings Mode="NextPrevFirstLast" />
										</GridProperties>
									</px:PXSelector>
									<px:PXSelector ID="edUOM_Dtl" runat="server" 
										DataField="UOM" DataSourceID="ds" DataKeyNames="fromUnit" 
										ValueField="fromUnit" AutoRefresh="true" >
										<Parameters>
											<px:PXControlParam ControlID="grdDetails" Name="CRServiceCaseItem.InventoryID" 
												PropertyName="DataValues[&quot;InventoryID&quot;]" />
										</Parameters>
									</px:PXSelector>
									<px:PXSegmentMask ID="edSiteID_Dtl" runat="server" DataField="SiteID" DataKeyNames="SiteCD"
										DataSourceID="ds" AllowEdit="True">
										<Items>
											<px:PXMaskItem EditMask="AlphaNumeric" Length="10" Separator="-" TextCase="Upper" />
										</Items>
										<GridProperties>
											<Columns>
												<px:PXGridColumn AutoGenerateOption="NotSet" DataField="SiteCD" MaxLength="30" Width="120px">
													<Header Text="Warehouse ID">
													</Header>
												</px:PXGridColumn>
												<px:PXGridColumn AllowNull="False" AutoGenerateOption="NotSet" DataField="INSiteStatus__QtyOnHand"
													DataType="Decimal" Width="100px">
													<Header Text="Qty. On Hand">
													</Header>
												</px:PXGridColumn>
												<px:PXGridColumn AutoGenerateOption="NotSet" DataField="INSiteStatus__Active" DataType="Boolean"
													Width="60px">
													<Header Text="Active">
													</Header>
												</px:PXGridColumn>
												<px:PXGridColumn AutoGenerateOption="NotSet" DataField="Descr" MaxLength="60" Width="200px">
													<Header Text="Description">
													</Header>
												</px:PXGridColumn>
											</Columns>
											<PagerSettings Mode="NextPrevFirstLast" />
										</GridProperties>
										<Parameters>
											<px:PXControlParam ControlID="grdDetails" Name="CRServiceCaseItem.InventoryID" 
												PropertyName="DataValues[&quot;InventoryID&quot;]" />
											<px:PXControlParam ControlID="grdDetails" Name="CRServiceCaseItem.SubItemID" 
												PropertyName="DataValues[&quot;SubItemID&quot;]" />
										</Parameters>
									</px:PXSegmentMask>
									<px:PXSegmentMask ID="edLocationID_Dtl" runat="server" DataField="LocationID" DataKeyNames="SiteID,LocationCD"
										DataSourceID="ds" HintField="descr" HintLabelID="lblLocationIDH" LabelID="lblLocationID"
										Style="z-index: 101; position: absolute; left: 122px; top: 93px; width: 96px;"
										TabIndex="230" AutoRefresh="True">
										<Items>
											<px:PXMaskItem EditMask="AlphaNumeric" Length="10" Separator="-" TextCase="Upper" />
										</Items>
										<GridProperties>
											<Columns>
												<px:PXGridColumn AutoGenerateOption="NotSet" DataField="LocationCD" MaxLength="30"
													Width="120px">
													<Header Text="Location ID">
													</Header>
												</px:PXGridColumn>
												<px:PXGridColumn AllowNull="False" AutoGenerateOption="NotSet" DataField="INLocationStatus__QtyOnHand"
													DataType="Decimal" Width="100px">
													<Header Text="Qty. On Hand">
													</Header>
												</px:PXGridColumn>
												<px:PXGridColumn AutoGenerateOption="NotSet" DataField="INLocationStatus__Active"
													DataType="Boolean" Width="60px">
													<Header Text="Active">
													</Header>
												</px:PXGridColumn>
												<px:PXGridColumn AutoGenerateOption="NotSet" DataField="PrimaryItemID">
													<Header Text="Primary Item">
													</Header>
												</px:PXGridColumn>
												<px:PXGridColumn AutoGenerateOption="NotSet" DataField="PrimaryItemClassID" MaxLength="10">
													<Header Text="Primary Item Class">
													</Header>
												</px:PXGridColumn>
												<px:PXGridColumn AllowNull="False" AutoGenerateOption="NotSet" DataField="ReceiptsValid"
													DataType="Boolean" Width="60px">
													<Header Text="Receipts Allowed">
													</Header>
												</px:PXGridColumn>
												<px:PXGridColumn AllowNull="False" AutoGenerateOption="NotSet" DataField="SalesValid"
													DataType="Boolean" Width="60px">
													<Header Text="Sales Allowed">
													</Header>
												</px:PXGridColumn>
												<px:PXGridColumn AllowNull="False" AutoGenerateOption="NotSet" DataField="TransfersValid"
													DataType="Boolean" Width="60px">
													<Header Text="Transfers Allowed">
													</Header>
												</px:PXGridColumn>
											</Columns>
											<PagerSettings Mode="NextPrevFirstLast" />
										</GridProperties>
										<Parameters>
											<px:PXControlParam ControlID="grdDetails" Name="CRServiceCaseItem.siteID" PropertyName="DataValues[&quot;SiteID&quot;]"
												Type="String" />
											<px:PXControlParam ControlID="grdDetails" Name="CRServiceCaseItem.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]"
												Type="String" />
											<px:PXControlParam ControlID="grdDetails" Name="CRServiceCaseItem.subItemID" PropertyName="DataValues[&quot;SubItemID&quot;]"
												Type="String" />
										</Parameters>
									</px:PXSegmentMask>
									<px:PXNumberEdit ID="edEstimatedQuantity_Dtl" runat="server" 
										DataField="EstimatedQuantity" AllowNull="False" Decimals="2" ValueType="Decimal" />
									<px:PXNumberEdit ID="edActualQuantity_Dtl" runat="server" 
										DataField="ActualQuantity" AllowNull="False" Decimals="2" ValueType="Decimal" />
									<px:PXNumberEdit ID="edCuryUnitPrice_Dtl" runat="server" 
										DataField="CuryUnitPrice" AllowNull="False" Decimals="2" ValueType="Decimal" />
<%--									<px:PXNumberEdit ID="edEstimatedCuryExtPrice_Dtl" runat="server" 
										DataField="EstimatedCuryExtPrice" AllowNull="False" Decimals="2" ValueType="Decimal" />
									<px:PXNumberEdit ID="edActualCuryExtPrice_Dtl" runat="server" 
										DataField="ActualCuryExtPrice" AllowNull="False" Decimals="2" ValueType="Decimal" />
--%>									<px:PXNumberEdit ID="edDiscount_Dtl" runat="server" 
										DataField="Discount" AllowNull="False" Decimals="6" ValueType="Decimal" />
									<px:PXNumberEdit ID="edEstimatedCuryDiscountAmount_Dtl" runat="server" 
										DataField="EstimatedCuryDiscountAmount" AllowNull="False" Decimals="2" ValueType="Decimal" />
									<px:PXNumberEdit ID="edActualCuryDiscountAmount_Dtl" runat="server" 
										DataField="ActualCuryDiscountAmount" AllowNull="False" Decimals="2" ValueType="Decimal" />
									<px:PXNumberEdit ID="edEstimatedCuryAmount_Dtl" runat="server" 
										DataField="EstimatedCuryAmount" AllowNull="False" Decimals="2" ValueType="Decimal" />
									<px:PXNumberEdit ID="edActualCuryAmount_Dtl" runat="server" 
										DataField="ActualCuryAmount" AllowNull="False" Decimals="2" ValueType="Decimal" />
								</RowTemplate>
							</px:PXGridLevel>
						</levels>
						<AutoSize enabled="True" minheight="150" />
						<ActionBar>
							<CustomItems>
								<px:PXToolBarButton Text="Bin/Lot/Serial" 
									CommandName="LSServiceCaseItem_binLotSerial" CommandSourceID="ds" 
                                    DependOnGrid="grdDetails" />
							</CustomItems>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Tax Details" LoadOnDemand="true">
				<Template>
					<px:PXGrid ID="grdTax" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
						Width="100%" SkinID="Details" ActionsPosition="Top" BorderWidth="0px">
						<AutoSize Enabled="True" MinHeight="150" />
						<ActionBar>
							<Actions>
								<Search Enabled="False" />
								<Save Enabled="False" />
								<EditRecord Enabled="False" />
							</Actions>
						</ActionBar>
						<Levels>
							<px:PXGridLevel  DataMember="Taxes">
								<RowTemplate>
									<px:PXLayoutRule ID="PXLayoutRule9" runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="XM" />

									<px:PXSelector SuppressLabel="True" ID="edTaxID" runat="server" DataField="TaxID" AllowEdit="True" /></RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="TaxID" Width="81px" AllowUpdate="False" />
									<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="TaxRate" TextAlign="Right" Width="81px" />
									<px:PXGridColumn AllowNull="False" DataField="CuryTaxableAmt" TextAlign="Right" Width="81px" />
									<px:PXGridColumn AllowNull="False" DataField="CuryTaxAmt" TextAlign="Right" Width="81px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Scheduling">
				<Template>
					<px:PXLayoutRule ID="PXLayoutRule10" runat="server" StartColumn="True"  LabelsWidth="s" ControlSize="XM" />
				    <px:PXTextEdit ID="edLocation" runat="server" DataField="Location" Size = "XXL" />
					<px:PXLayoutRule ID="PXLayoutRule11" runat="server" Merge="True" />
					<px:PXDateTimeEdit CommitChanges="True" Size="s" ID="edStartDate_Date" runat="server" DataField="StartDate_Date"  />
					<px:PXDateTimeEdit CommitChanges="True" SuppressLabel="True" Size="s" ID="edStartDate_Time" runat="server" DataField="StartDate_Time" TimeMode="true" />
					<px:PXLayoutRule ID="PXLayoutRule12" runat="server" />

					<px:PXLayoutRule ID="PXLayoutRule13" runat="server" Merge="True" />
					<px:PXMaskEdit CommitChanges="True" Size="s" ID="edDuration" runat="server" DataField="Duration" InputMask="### d\ays ## hrs ## mins" EmptyChar="0"  />
					<px:PXCheckBox CommitChanges="True" ID="chkAllDay" runat="server" Checked="True" DataField="AllDay" />
					<px:PXLayoutRule ID="PXLayoutRule14" runat="server" Merge="False" />

					<px:PXLayoutRule ID="PXLayoutRule15" runat="server" Merge="True" />	
					<px:PXDateTimeEdit CommitChanges="True" Size="s" ID="edEndDate_Date" runat="server" DataField="EndDate_Date"  />
					<px:PXDateTimeEdit CommitChanges="True" SuppressLabel="True" Size="s" ID="edEndDate_Time" runat="server" DataField="EndDate_Time" TimeMode="true" />
					<px:PXLayoutRule ID="PXLayoutRule16" runat="server" Merge="False" />
					<px:PXCheckBox CommitChanges="True" ID="chkIsReminderOn" runat="server" DataField="IsReminderOn" />
					<px:PXLayoutRule ID="PXLayoutRule17" runat="server" Merge="True" />
					<px:PXDateTimeEdit Size="s" ID="edReminderDate_Date" runat="server" DataField="ReminderDate_Date" DisplayFormat="g" EditFormat="g"  />
					<px:PXDateTimeEdit SuppressLabel="True" Size="s" ID="edReminderDate_Time" runat="server" DataField="ReminderDate_Time" DisplayFormat="g" EditFormat="g" TimeMode="True" />
					<px:PXLayoutRule ID="PXLayoutRule18" runat="server" Merge="False" />
</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Labor">
				<Template>
					<px:PXGrid ID="grdLabor" runat="server" ActionsPosition="Top" DataSourceID="ds" 
						Height="100%" Style="z-index: 100; left: 0px; top: 0px"
						Width="100%" BorderWidth="0px" SkinID="Details">
						<Mode InitNewRow="True" />
						<AutoSize Enabled="True" MinHeight="160" />
						<Levels>
							<px:PXGridLevel DataMember="Labor" >
								<Columns>
									<px:PXGridColumn DataField="EmployeeID" Width="81px" AutoCallBack="True" TextField="EPEmployee__AcctCD"  />
									<px:PXGridColumn DataField="EPEmployee__AcctName" Width="130px" AutoCallBack="True"  />
									<px:PXGridColumn DataField="StartDate" Width="120px" DisplayFormat="g" AutoCallBack="True" AllowNull="False" />
									<px:PXGridColumn DataField="TimeSpent" Width="80px" AutoCallBack="True" AllowNull="False"  RenderEditorText="True" />
									<px:PXGridColumn DataField="OverTimeSpent" Width="80px" AutoCallBack="True" AllowNull="False" />
									<px:PXGridColumn DataField="TimeBillable" Width="80px" AutoCallBack="True" AllowNull="False" />
									<px:PXGridColumn DataField="OverTimeBillable" Width="80px" AutoCallBack="True" AllowNull="False" />
									<px:PXGridColumn DataField="Description" Width="220px" />
								</Columns>
								<RowTemplate>
									<px:PXSelector ID="edEmployeeID_Labor" runat="server" DataField="EmployeeID" 
										 ValueField="BAccountID" TextField="AcctCD" >
										<GridProperties FastFilterFields="AcctName">
										</GridProperties>
									</px:PXSelector>
									<px:PXTimeSpan ID="edTimeSpent_Labor" runat="server" DataField="TimeSpent" TimeMode="true" InputMask="hh:mm" />
									<px:PXDateTimeEdit ID="edOverTimeSpent_Labor" runat="server" DataField="OverTimeSpent" TimeMode="true" />
									<px:PXDateTimeEdit ID="edBillableTime_Labor" runat="server" DataField="TimeBillable" TimeMode="true" />
									<px:PXDateTimeEdit ID="edBillableOverTime_Labor" runat="server" DataField="OverTimeBillable" TimeMode="true" />
								</RowTemplate>
							</px:PXGridLevel>
						</Levels>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Address" RepaintOnDemand="false" >
				<Template>
					<px:PXLayoutRule ID="PXLayoutRule19" runat="server" StartColumn="True" SuppressLabel="True" />
                    <px:PXPanel RenderStyle="Fieldset" ID="pnlDestAddress" runat="server" Caption="Destination Address">
						<px:PXFormView ID="frmDestAddress" DataMember="DestinationAddress"  runat="server"
							DataSourceID="ds" CaptionVisible="false">
							<Template>
								<px:PXLayoutRule ID="PXLayoutRule20" runat="server" StartColumn="True"  LabelsWidth="s" ControlSize="L" />

								<px:PXCheckBox CommitChanges="True" ID="chkOverrideAddress" runat="server" DataField="OverrideAddress"/>
								<px:PXTextEdit ID="edAddressLine1" runat="server" DataField="AddressLine1"  />
								<px:PXTextEdit ID="edAddressLine2" runat="server" DataField="AddressLine2"  />
								<px:PXTextEdit ID="edCity" runat="server" DataField="City"  />
								<px:PXSelector ID="edCountryID" runat="server" DataField="CountryID" AutoRefresh="True" Size="M"/>
								<px:PXSelector ID="edState" runat="server" DataField="State"   AutoRefresh="true" Size="M">
									<CallBackMode PostData="Container" />
									<Parameters>
										<px:PXControlParam ControlID="frmDestAddress" Name="CRServiceCaseDestinationAddress.countryID" PropertyName="DataControls[&quot;edCountryID&quot;].Value"
											Type="String" />
									</Parameters>
								</px:PXSelector>
								<px:PXMaskEdit CommitChanges="True" ID="edPostalCode" runat="server" DataField="PostalCode" Size="M"/></Template>
							<ContentStyle BackColor="Transparent" BorderStyle="None">
							</ContentStyle>
						</px:PXFormView>
					</px:PXPanel>
					<px:PXLayoutRule ID="PXLayoutRule21" runat="server" StartColumn="True" SuppressLabel="True" />
                    <px:PXPanel RenderStyle="Fieldset" ID="pnlBillAddress" runat="server" Caption="Billing Address">
						<px:PXFormView ID="frmBillAddress" DataMember="BillingAddress"  runat="server"
							DataSourceID="ds" CaptionVisible="false">
						<Template>
								<px:PXLayoutRule ID="PXLayoutRule22" runat="server" StartColumn="True"  LabelsWidth="s" ControlSize="L" />

								<px:PXCheckBox CommitChanges="True" ID="chkOverrideAddress" runat="server" DataField="OverrideAddress" />
							<px:PXTextEdit ID="edAddressLine1" runat="server" DataField="AddressLine1"  />
							<px:PXTextEdit ID="edAddressLine2" runat="server" DataField="AddressLine2"  />
							<px:PXTextEdit ID="edCity" runat="server" DataField="City"  />
							<px:PXSelector ID="edCountryID" runat="server" DataField="CountryID" AutoRefresh="True" Size="M"/>
							<px:PXSelector ID="edState" runat="server" DataField="State" AutoRefresh="true" Size="M">
								<CallBackMode PostData="Container" />
								<Parameters>
										<px:PXControlParam ControlID="frmBillAddress" Name="CRServiceCaseBillingAddress.countryID" PropertyName="DataControls[&quot;edCountryID&quot;].Value"
										Type="String" />
								</Parameters>
							</px:PXSelector>
							<px:PXMaskEdit CommitChanges="True" ID="edPostalCode" runat="server" DataField="PostalCode" Size="M"/></Template>
						<ContentStyle BackColor="Transparent" BorderStyle="None">
						</ContentStyle>
					</px:PXFormView>
					</px:PXPanel>
                </Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Activity History" LoadOnDemand="true">
				<Template>
				<pxa:PXGridWithPreview ID="gridActivities" runat="server" DataSourceID="ds" 
							Width="100%" AllowSearch="True" DataMember="Activities"
							AllowPaging="true" NoteField="NoteText" FilesField="NoteFiles" 
							BorderWidth="0px" GridSkinID="Details"
							SplitterStyle="z-index: 100; border-top: solid 1px Gray;  border-bottom: solid 1px Gray"
							PreviewPanelStyle="z-index: 100; background-color: Window"
							PreviewPanelSkinID="Preview" BlankFilterHeader="All Activities" MatrixMode="true" PrimaryViewControlID="form" >
						<ActionBar DefaultAction="cmdViewActivity" PagerVisible="False">
							<Actions>
								<AddNew Enabled="False" />
							</Actions>
							<CustomItems>
								<px:PXToolBarButton Text="Add Task" Key="cmdAddTask">
								    <AutoCallBack Command="NewTask" Target="ds" />
								    <PopupCommand Command="Cancel" Target="ds" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Text="Add Event" Key="cmdAddEvent">
								    <AutoCallBack Command="NewEvent" Target="ds" />
								    <PopupCommand Command="Cancel" Target="ds" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Text="Add Email" Key="cmdAddEmail">
								    <AutoCallBack Command="NewMailActivity" Target="ds" />
								    <PopupCommand Command="Cancel" Target="ds" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Text="Add Activity" Key="cmdAddActivity">
								    <AutoCallBack Command="NewActivity" Target="ds" />
								    <PopupCommand Command="Cancel" Target="ds" />
									<ActionBar />
								</px:PXToolBarButton>
								<px:PXToolBarButton Key="cmdViewActivity" Visible="false" >
									<ActionBar MenuVisible="false" />
									<AutoCallBack Command="ViewActivity" Target="ds" />
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
						<Levels>
							<px:PXGridLevel DataMember="Activities" >
								<Columns>
									<px:PXGridColumn DataField="IsCompleteIcon" Width="21px" AllowShowHide="False" ForceExport="True" />
									<px:PXGridColumn DataField="PriorityIcon" Width="21px" AllowShowHide="False" AllowResize="False" ForceExport="True" />
									<px:PXGridColumn DataField="ReminderIcon" Width="21px" AllowShowHide="False" AllowResize="False" ForceExport="True" />
									<px:PXGridColumn DataField="ClassIcon" Width="21px" AllowShowHide="False" ForceExport="True" />
									<px:PXGridColumn DataField="ClassInfo" />
									<px:PXGridColumn DataField="RefNoteID" Visible="false" AllowShowHide="False" />
									<px:PXGridColumn DataField="Subject" LinkCommand="ViewActivity" Width="297px" />
									<px:PXGridColumn AllowNull="False" DataField="Status" />
									<px:PXGridColumn DataField="StartDate" DisplayFormat="g" Width="108px" />
									<px:PXGridColumn DataField="CategoryID" />
									<px:PXGridColumn AllowNull="False" DataField="IsBillable" TextAlign="Center" Type="CheckBox" Width="60px" />
									<px:PXGridColumn DataField="TimeSpent" Width="100px" />
									<px:PXGridColumn DataField="OvertimeSpent" Width="100px" />
									<px:PXGridColumn AllowUpdate="False" DataField="TimeBillable" Width="100px" />
									<px:PXGridColumn AllowUpdate="False" DataField="OvertimeBillable" Width="100px" />
									<px:PXGridColumn AllowUpdate="False" DataField="CreatedByID_Creator_Username" Visible="false" SyncVisible="False" SyncVisibility="False" Width="108px" />
									<px:PXGridColumn DataField="GroupID" Width="90px" />
									<px:PXGridColumn DataField="Owner" LinkCommand="OpenActivityOwner" Width="150px" DisplayMode="Text"/>
								</Columns>
								<RowTemplate>
									<px:PXSelector ID="edActivityAssignedTo" runat="server" DataField="ActivityOwner__fullname" AllowEdit="true"  />
									<px:PXSelector ID="edActivityOwnerID" runat="server" DataField="CreatedByID" AllowEdit="true"  />
									<px:PXSelector ID="edActivitySubject" runat="server" DataField="Subject" AllowEdit="true" />
								</RowTemplate>
							</px:PXGridLevel>
						</Levels>
						<CallbackCommands>
							<Refresh CommitChanges="True" PostData="Page" />
						</CallbackCommands>
					<%--<AutoSize Enabled="True" MinHeight="100" MinWidth="100" />--%>
					<GridMode AllowAddNew="False" AllowUpdate="False" />
						<PreviewPanelTemplate>
							<pxa:PXHtmlView ID="edBody" runat="server" DataField="previewHtml" TextMode="MultiLine" MaxLength="50" Width="100%" Height="100%" SkinID="Label" /> 
						</PreviewPanelTemplate>
				</pxa:PXGridWithPreview>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="100" MinWidth="300" />
	</px:PXTab>
	<px:PXSmartPanel ID="PanelLS" runat="server" Style="z-index: 108;
		left: 252px; position: absolute; top: 531px; height: 360px;" Width="764px" Caption="Bin/Lot/Serial Numbers" 
			DesignView="Content" CaptionVisible="True" 
			Key="BinLotSerialSelect" AutoCallBack-Command="Refresh" 
			AutoCallBack-Enabled="True" AutoCallBack-Target="optform">
		<px:PXFormView ID="optform" runat="server" Width="100%" 
			CaptionVisible="False" DataMember="LSServiceCaseItem_lotseropts" DataSourceID="ds" SkinID="Transparent">
				<Parameters>
					<px:PXSyncGridParam ControlID="grdDetails" />
				</Parameters>
				<Template>
					<px:PXLayoutRule ID="PXLayoutRule23" runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="XM" />

					<px:PXNumberEdit ID="edUnassignedQty" runat="server" DataField="UnassignedQty" Enabled="False"  />
					<px:PXNumberEdit ID="edQty" runat="server" DataField="Qty">
						<AutoCallBack>
							<Behavior CommitChanges="True" />
						</AutoCallBack>
					</px:PXNumberEdit>
					<px:PXLayoutRule ID="PXLayoutRule24" runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="XM" />

					<px:PXMaskEdit ID="edStartNumVal" runat="server" DataField="StartNumVal"  />
					<px:PXButton ID="btnGenerate" runat="server" Text="Generate" Height="20px"  
						CommandName="LSServiceCaseItem_generateLotSerial" CommandSourceID="ds" />
				</Template>
		</px:PXFormView>
		<px:PXGrid ID="grid2" runat="server" Width="100%" AutoAdjustColumns="True" DataSourceID="ds"
			Style="border-width: 1px 0px; left: 0px; top: 0px; height: 192px;">
			<AutoSize Enabled="true" />
			<Mode InitNewRow="True" />
			<Parameters>
					<px:PXSyncGridParam ControlID="grdDetails" />
			</Parameters>
			<Levels>
				<px:PXGridLevel DataMember="Splits" >
					<Columns>
						<px:PXGridColumn DataField="InventoryID" Width="108px" />
						<px:PXGridColumn DataField="SubItemID" Width="108px" />
						<px:PXGridColumn DataField="LocationID" AllowShowHide="Server" Width="108px" />
						<px:PXGridColumn DataField="LotSerialNbr" AllowShowHide="Server" Width="108px" />
						<px:PXGridColumn DataField="Qty" Width="108px" TextAlign="Right" />
						<px:PXGridColumn DataField="UOM" Width="108px" />
						<px:PXGridColumn DataField="ExpireDate" AllowShowHide="Server" Width="90px" />
						<px:PXGridColumn AllowUpdate="False" DataField="InventoryID_InventoryItem_descr" Width="108px" />							
					</Columns>
					<RowTemplate>
						<px:PXLayoutRule ID="PXLayoutRule25" runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="XM" />

						<px:PXSegmentMask ID="edSubItemID2" runat="server" DataField="SubItemID" AutoRefresh="true" />
						<px:PXSegmentMask ID="edLocationID2" runat="server" DataField="LocationID"  AutoRefresh="true">
							<Parameters>
								<px:PXControlParam ControlID="grid2" Name="CRServiceCaseItemSplit.siteID" 
									PropertyName="DataValues[&quot;SiteID&quot;]" Type="String" />
								<px:PXControlParam ControlID="grid2" Name="CRServiceCaseItemSplit.inventoryID" 
									PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
								<px:PXControlParam ControlID="grid2" Name="CRServiceCaseItemSplit.subItemID" 
									PropertyName="DataValues[&quot;SubItemID&quot;]" Type="String" />
							</Parameters>
						</px:PXSegmentMask>
						<px:PXNumberEdit ID="edQty2" runat="server" DataField="Qty"  />
						<px:PXSelector ID="edUOM2" runat="server" DataField="UOM"   AutoRefresh="true">
							<Parameters>
								<px:PXControlParam ControlID="grdDetails" Name="CRServiceCaseItem.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]"
									Type="String" />
							</Parameters>
						</px:PXSelector>
						<px:PXSelector ID="edLotSerialNbr2" runat="server" DataField="LotSerialNbr"   AutoRefresh="true">
							<Parameters>
								<px:PXControlParam ControlID="grid2" Name="CRServiceCaseItemSplit.inventoryID" 
									PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
								<px:PXControlParam ControlID="grid2" Name="CRServiceCaseItemSplit.subItemID" 
									PropertyName="DataValues[&quot;SubItemID&quot;]" Type="String" />
								<px:PXControlParam ControlID="grid2" Name="CRServiceCaseItemSplit.locationID" 
									PropertyName="DataValues[&quot;LocationID&quot;]" Type="String" />
							</Parameters>
						</px:PXSelector>
						<px:PXDateTimeEdit ID="edExpireDate2" runat="server" DataField="ExpireDate"  />
                    </RowTemplate>
				</px:PXGridLevel>
			</Levels>
		</px:PXGrid>
		<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
			<px:PXButton ID="btnSave" runat="server" DialogResult="OK" Text="OK" />
		</px:PXPanel>
	</px:PXSmartPanel>
</asp:Content>