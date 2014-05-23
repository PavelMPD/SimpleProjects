<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="IN202500.aspx.cs" Inherits="Page_IN202500" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.IN.InventoryItemMaint" PrimaryView="Item">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand StartNewGroup="True" Name="Action" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="Inquiry" />
			<px:PXDSCallbackCommand Name="UpdateVendorPrice" Visible="false" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="AddWarehouseDetail" Visible="false" CommitChanges="true" />						
			<px:PXDSCallbackCommand Name="UpdateReplenishment" Visible="false" CommitChanges="true" DependOnGrid="repGrid" />						
			<px:PXDSCallbackCommand Name="GenerateSubitems" Visible="false" CommitChanges="true" DependOnGrid="repGrid" />						
			<px:PXDSCallbackCommand Name="ViewGroupDetails" Visible="False" DependOnGrid="grid3" />
			<px:PXDSCallbackCommand Name="UpdateCustPriceClass" Visible="false" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="UpdateCustomerPrice" Visible="false" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="UpdateAPVendorPrice" Visible="false" CommitChanges="true" />
		</CallbackCommands>
		<DataTrees>
			<px:PXTreeDataMember TreeView="_EPCompanyTree_Tree_" TreeKeys="WorkgroupID" />
		</DataTrees>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
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
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Item" Caption="Stock Item Summary" NoteIndicator="True" FilesIndicator="True" ActivityIndicator="True"
		ActivityField="NoteActivity" DefaultControlID="edInventoryCD">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			<px:PXSegmentMask ID="edInventoryCD" runat="server" DataField="InventoryCD" DataSourceID="ds" AutoRefresh="true"/>
			<px:PXDropDown ID="edItemStatus" runat="server" DataField="ItemStatus" Size="S" />
			<px:PXLayoutRule runat="server" ColumnSpan="2" />
			<px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
		    <px:PXSelector CommitChanges="True" ID="edProductWorkgroupID" runat="server" DataField="ProductWorkgroupID"/>			
			<px:PXSelector ID="edProductManagerID" runat="server" DataField="ProductManagerID" AutoRefresh="True" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Width="100%" Height="606px" DataSourceID="ds" DataMember="ItemSettings" FilesIndicator="False" NoteIndicator="False">
		<AutoSize Enabled="True" Container="Window" MinHeight="150" />
		<Items>
			<px:PXTabItem Text="General Settings">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Item Defaults" />
					<px:PXSelector CommitChanges="True" ID="edItemClassID" runat="server" DataField="ItemClassID" AllowEdit="True" />
					<px:PXDropDown ID="edItemType" runat="server" DataField="ItemType" />
					<px:PXCheckBox SuppressLabel="True" ID="chkKitItem" runat="server" DataField="KitItem" />
					<px:PXDropDown CommitChanges="True" ID="edValMethod" runat="server" DataField="ValMethod" />
					<px:PXSelector ID="edTaxCategoryID" runat="server" DataField="TaxCategoryID" AllowEdit="True" CommitChanges="True" AutoRefresh="True"/>
					<px:PXSelector CommitChanges="True" ID="edPostClassID" runat="server" DataField="PostClassID" AllowEdit="True" />
					<px:PXSelector CommitChanges="True" ID="edLotSerClassID" runat="server" DataField="LotSerClassID" AllowEdit="True" />
					<px:PXMaskEdit ID="edLotSerNumVal" runat="server" DataField="LotSerNumVal" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Warehouse Defaults" />
					<px:PXSegmentMask CommitChanges="True" ID="edDfltSiteID" runat="server" DataField="DfltSiteID" AllowEdit="True" />
					<px:PXSegmentMask CommitChanges="True" ID="edDfltShipLocationID" runat="server" DataField="DfltShipLocationID" AutoRefresh="True" AllowEdit="True" />
					<px:PXSegmentMask CommitChanges="True" ID="edDfltReceiptLocationID" runat="server" DataField="DfltReceiptLocationID" AutoRefresh="True" AllowEdit="True" />
					<px:PXLayoutRule runat="server" Merge="True" />
					<px:PXSegmentMask Size="s" ID="edDefaultSubItemID" runat="server" DataField="DefaultSubItemID" AutoRefresh="True" />
					<px:PXCheckBox ID="chkDefaultSubItemOnEntry" runat="server" DataField="DefaultSubItemOnEntry" />
					<px:PXLayoutRule runat="server" />
					<px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
					<px:PXLayoutRule runat="server" GroupCaption="Unit of Measure" StartGroup="True" />
					<px:PXSelector ID="edBaseUnit" Size="s" runat="server" AllowEdit="True" CommitChanges="True" DataField="BaseUnit" />
					<px:PXSelector ID="edSalesUnit" Size="s" runat="server" AllowEdit="True" AutoRefresh="True" CommitChanges="True" DataField="SalesUnit" />
					<px:PXSelector ID="edPurchaseUnit" Size="s" runat="server" AllowEdit="True" AutoRefresh="True" CommitChanges="True" DataField="PurchaseUnit" />
					<px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="S" SuppressLabel="True" />
					<px:PXGrid ID="gridUnits" runat="server" DataSourceID="ds" SkinID="ShortList" Width="400px" Height="114px">
							<Mode InitNewRow="True" />
							<Levels>
								<px:PXGridLevel  DataMember="itemunits">
									<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXTextEdit ID="edItemClassID2" runat="server" DataField="ItemClassID" />
									<px:PXNumberEdit ID="edInventoryID" runat="server" DataField="InventoryID" />
									<px:PXMaskEdit ID="edFromUnit" runat="server" DataField="FromUnit" />
									<px:PXMaskEdit ID="edSampleToUnit" runat="server" DataField="SampleToUnit" />
									<px:PXNumberEdit ID="edUnitRate" runat="server" DataField="UnitRate" />
									</RowTemplate>
									<Columns>
									<px:PXGridColumn DataField="UnitType" Type="DropDownList" Width="99px" Visible="False" />
									<px:PXGridColumn DataField="ItemClassID" Width="36px" Visible="False" />
									<px:PXGridColumn DataField="InventoryID" Visible="False" TextAlign="Right" Width="54px" />
									<px:PXGridColumn DataField="FromUnit" Width="72px" />
									<px:PXGridColumn DataField="UnitMultDiv" Type="DropDownList" Width="90px" />
									<px:PXGridColumn DataField="UnitRate" TextAlign="Right" Width="108px" />
									<px:PXGridColumn DataField="SampleToUnit" Width="72px" />
									</Columns>
								</px:PXGridLevel>
							</Levels>
							<Layout ColumnsMenu="False" />
						</px:PXGrid>
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Physical Inventory" />
					<px:PXSelector CommitChanges="True" ID="edPICycleID" runat="server" DataField="CycleID" AllowEdit="True" />
					<px:PXSelector CommitChanges="True" ID="edABCCodeID" runat="server" DataField="ABCCodeID" AllowEdit="True" />
					<px:PXCheckBox SuppressLabel="True" ID="chkABCCodeIsFixed" runat="server" DataField="ABCCodeIsFixed" />
					<px:PXSelector CommitChanges="True" ID="edMovementClassID" runat="server" DataField="MovementClassID" AllowEdit="True" />
					<px:PXCheckBox SuppressLabel="True" ID="chkMovementClassIsFixed" runat="server" DataField="MovementClassIsFixed" />
				</Template>
			</px:PXTabItem>
            <px:PXTabItem Text="Subitems" Key="Subitems">
                <Template>
					<px:PXGrid ID="gridSegmentValues" runat="server" DataSourceID="ds" Height="100%" Width="100%" SkinID="DetailsInTab">
						<Mode InitNewRow="true" />
						<Levels>
							<px:PXGridLevel DataMember="SegmentValues">							
								<Columns>
									<px:PXGridColumn DataField="SegmentID" Width="90px" CommitChanges="true"/>
									<px:PXGridColumn DataField="Value" DisplayFormat="&gt;aaaaaa" Width="90px" CommitChanges="true" />									
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />						
					</px:PXGrid>
				</Template>
            </px:PXTabItem>
			<px:PXTabItem Text="Price/Cost Info">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
					<px:PXLayoutRule runat="server" GroupCaption="Base Price" StartGroup="True" />
					<px:PXSelector ID="edPriceClassID" runat="server" DataField="PriceClassID" AllowEdit="True" />
					<px:PXNumberEdit ID="edPendingBasePrice" runat="server" DataField="PendingBasePrice" CommitChanges="True" />
					<px:PXDateTimeEdit ID="edPendingBasePriceDate" runat="server" DataField="PendingBasePriceDate" CommitChanges="true"/>
					<px:PXNumberEdit ID="edBasePrice" runat="server" DataField="BasePrice" Enabled="False" />
					<px:PXDateTimeEdit ID="edBasePriceDate" runat="server" DataField="BasePriceDate" Enabled="False" />
					<px:PXNumberEdit ID="edLastBasePrice" runat="server" DataField="LastBasePrice" Enabled="False" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Standard Cost" />
					<px:PXNumberEdit ID="edPendingStdCost" runat="server" DataField="PendingStdCost" CommitChanges="True" />
                    <px:PXDateTimeEdit ID="edPendingStdCostDate" runat="server" DataField="PendingStdCostDate" />
					<px:PXNumberEdit ID="edStdCost" runat="server" DataField="StdCost" Enabled="False" />
					<px:PXDateTimeEdit ID="edStdCostDate" runat="server" DataField="StdCostDate" Enabled="False" />					
					<px:PXNumberEdit ID="edLastStdCost" runat="server" DataField="LastStdCost" Enabled="False" />
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />f
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Price Management" />
				    <px:PXSelector CommitChanges="True" ID="edPriceWorkgroupID" runat="server" DataField="PriceWorkgroupID" ShowRootNode="False"/>													
					<px:PXSelector ID="edPriceManagerID" runat="server" DataField="PriceManagerID" AutoRefresh="True" />
					<px:PXCheckBox SuppressLabel="True" ID="chkCommisionable" runat="server" DataField="Commisionable" />
					<px:PXNumberEdit ID="edMinGrossProfitPct" runat="server" DataField="MinGrossProfitPct" />
					<px:PXNumberEdit ID="edMarkupPct" runat="server" DataField="MarkupPct" />
					<px:PXNumberEdit ID="edRecPrice" runat="server" DataField="RecPrice" />
					<px:PXLayoutRule runat="server" GroupCaption="Cost Statistics" ControlSize="SM" LabelsWidth="SM" StartGroup="True" />
					<px:PXFormView ID="formPanelStat" runat="server" SkinID="Transparent" DataSourceID="ds" DataMember="itemcosts" RenderStyle="Simple">
						<Template>
							<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="SM" LabelsWidth="SM" />
							<px:PXNumberEdit ID="edLastCost" runat="server" DataField="LastCost" />
							<px:PXNumberEdit ID="edAvgCost" runat="server" DataField="AvgCost" Enabled="False" />
							<px:PXNumberEdit ID="edMinCost" runat="server" DataField="MinCost" Enabled="False" />
							<px:PXNumberEdit ID="edMaxCost" runat="server" DataField="MaxCost" Enabled="False" />
						</Template>
					</px:PXFormView>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Warehouse Details">
				<Template>
					<px:PXGrid ID="grid2" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" ActionsPosition="Top" EditPageUrl="~/Pages/IN/IN204500.aspx" BorderWidth="0px" SkinID="Details">
						<EditPageParams>
							<px:PXControlParam ControlID="grid2" Direction="Output" Name="InventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
							<px:PXControlParam ControlID="grid2" Direction="Output" Name="SiteID" PropertyName="DataValues[&quot;SiteID&quot;]" Type="String" />
						</EditPageParams>
					    <ActionBar>
						    <CustomItems>
							    <px:PXToolBarButton>
					            <AutoCallBack Command="AddWarehouseDetail" Target="ds" />
							    </px:PXToolBarButton>								            
						    </CustomItems>
					    </ActionBar>
					    <Levels>
							<px:PXGridLevel DataMember="itemsiterecords" DataKeyNames="InventoryID,SiteID">
											<RowTemplate>
									<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXSegmentMask ID="edPreferredVendorID" runat="server" DataField="PreferredVendorID" Enabled="False" AllowEdit="True" />
									<px:PXSegmentMask SuppressLabel="True" Size="s" ID="edSiteID2" runat="server" DataField="SiteID" AllowEdit="True" TextField="INSite__SiteCD" />
									<px:PXSegmentMask SuppressLabel="True" Size="s" ID="edInvtAcctID2" runat="server" DataField="InvtAcctID" />
									<px:PXSegmentMask SuppressLabel="True" Size="s" ID="edDfltShipLocationID2" runat="server" DataField="DfltShipLocationID" AutoRefresh="True" />
									<px:PXSegmentMask SuppressLabel="True" Size="xm" ID="edInvtSubID2" runat="server" DataField="InvtSubID" />
									<px:PXSegmentMask SuppressLabel="True" ID="edDfltReceiptLocationID2" runat="server" DataField="DfltReceiptLocationID" AutoRefresh="True" />
											</RowTemplate>
											<Columns>
									<px:PXGridColumn DataField="IsDefault" TextAlign="Center" Type="CheckBox" Width="60px" />
									<px:PXGridColumn DataField="SiteID">
										<NavigateParams>
											<px:PXControlParam ControlID="grid2" Direction="Output" Name="SiteID" PropertyName="DataValues[&quot;SiteID&quot;]" Type="String" />
											<px:PXControlParam ControlID="grid2" Direction="Output" Name="InventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
										</NavigateParams>
                                                </px:PXGridColumn>
									<px:PXGridColumn DataField="DfltReceiptLocationID" />
									<px:PXGridColumn DataField="DfltShipLocationID" />
									<px:PXGridColumn DataField="SiteStatus" Type="DropDownList" />
									<px:PXGridColumn DataField="InvtAcctID" />
									<px:PXGridColumn DataField="InvtSubID" />
									<px:PXGridColumn DataField="ProductWorkgroupID" />
									<px:PXGridColumn DataField="ProductManagerID" />
									<px:PXGridColumn DataField="StdCostOverride" TextAlign="Center" Type="CheckBox" Width="60px" />
									<px:PXGridColumn DataField="BasePriceOverride" TextAlign="Center" Type="CheckBox" Width="60px" />
									<px:PXGridColumn DataField="INSiteStatusSummary__QtyOnHand" TextAlign="Right" Width="100px" />
									<px:PXGridColumn DataField="PreferredVendorOverride" Label="Preferred Vendor Override" TextAlign="Center" Type="CheckBox" Width="60px" />
									<px:PXGridColumn DataField="PreferredVendorID" Width="81px" />
									<px:PXGridColumn DataField="ReplenishmentPolicyOverride" Label="Replenishment Policy Override" TextAlign="Center" Type="CheckBox" Width="90px" />
									<px:PXGridColumn DataField="ReplenishmentPolicyID" Label="Seasonality" Width="90px" />
									<px:PXGridColumn DataField="ReplenishmentSource" Label="Replenishment Source" RenderEditorText="True" Width="90px" />
									<px:PXGridColumn DataField="ReplenishmentSourceSiteID" Label="Replenishment Warehouse" Width="90px" />
									<px:PXGridColumn DataField="ServiceLevelOverride" Label="Service Level Override" TextAlign="Center" Type="CheckBox" Width="60px" />									
									<px:PXGridColumn DataField="ServiceLevelPct" Label="Service Level" Width="90px" />									
									<px:PXGridColumn DataField="LastForecastDate"  Label="LastForecastDate" Width="140px" />
									<px:PXGridColumn DataField="DemandPerDayAverage"  Label="Demand Per Day Forecast" Width="60px" />
									<px:PXGridColumn DataField="DemandPerDaySTDEV"  Label="Demand Per Day Error (STDEV)" Width="80px" />									
											</Columns>
										</px:PXGridLevel>
									</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
								</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Sales Prices" LoadOnDemand="true">
				<Template>
					<px:PXGrid ID="PXGridSalesPrices" runat="server" DataSourceID="ds" Height="100%" Width="100%" SkinID="DetailsInTab">
						<Mode InitNewRow="true" />
						<Levels>
							<px:PXGridLevel DataMember="ARSalesPrices" DataKeyNames="RecordID">
								<RowTemplate>
									<px:PXNumberEdit runat="server" DataField="PendingBreakQty" ID="edPendingBreakQty1"/>
									<px:PXNumberEdit runat="server" DataField="PendingPrice" ID="edPendingPrice1"/>
									<px:PXNumberEdit runat="server" DataField="BreakQty" ID="edBreakQty1"/>
									<px:PXNumberEdit runat="server" DataField="SalesPrice" ID="edSalesPrice1"/>
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="CustPriceClassID" Width="90px" CommitChanges="true"/>
									<px:PXGridColumn DataField="UOM" DisplayFormat="&gt;aaaaaa" Width="90px" CommitChanges="true" />
									<px:PXGridColumn DataField="IsPromotionalPrice" Type="CheckBox" Width="80px" TextAlign="Center" CommitChanges="true" />
									<px:PXGridColumn AutoCallBack="true" DataField="EffectiveDate" Width="90px" />
									<px:PXGridColumn DataField="PendingBreakQty" Width="90px" TextAlign="Right" CommitChanges="true"/>
								    <px:PXGridColumn DataField="PendingPrice" TextAlign="Right" Width="90px" />
									<px:PXGridColumn DataField="PendingTaxID" TextAlign="Right" Width="90px" />
									<px:PXGridColumn DataField="BreakQty" Width="90px" TextAlign="Right"/>
								    <px:PXGridColumn DataField="SalesPrice" TextAlign="Right" Width="90px" />
									<px:PXGridColumn DataField="TaxID" TextAlign="Right" Width="90px" />
									<px:PXGridColumn DataField="LastDate" Width="90px" />
									<px:PXGridColumn DataField="ExpirationDate" Width="90px" />
									<px:PXGridColumn DataField="LastBreakQty" Width="90px" TextAlign="Right"/>
									<px:PXGridColumn DataField="LastPrice" TextAlign="Right" Width="90px" />
									<px:PXGridColumn DataField="LastTaxID" TextAlign="Right" Width="90px" />
									<px:PXGridColumn DataField="CuryID" Width="81px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
						<ActionBar PagerVisible="False">
							<PagerSettings Mode="NextPrevFirstLast" />
							<CustomItems>
								<px:PXToolBarButton Text="Update Prices" Key="cmdUpdateCustPriceClass">
									<AutoCallBack Command="UpdateCustPriceClass" Target="ds">
										<Behavior PostData="Page" />
									</AutoCallBack>
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Customer Prices" LoadOnDemand="true">
				<Template>
					<px:PXGrid ID="PXGridCustomerPrices" runat="server" DataSourceID="ds" Height="100%" Width="100%" SkinID="DetailsInTab">
						<Mode InitNewRow="true" />
						<Levels>
							<px:PXGridLevel DataMember="CustomerSalesPrice" DataKeyNames="RecordID">
								<RowTemplate>
									<px:PXNumberEdit runat="server" DataField="PendingBreakQty" ID="edPendingBreakQty2"/>
									<px:PXNumberEdit runat="server" DataField="PendingPrice" ID="edPendingPrice2"/>
									<px:PXNumberEdit runat="server" DataField="BreakQty" ID="edBreakQty2"/>
									<px:PXNumberEdit runat="server" DataField="SalesPrice" ID="edSalesPrice2"/>
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="CustomerID" Width="90px" CommitChanges="true"/>
									<px:PXGridColumn DataField="Customer__AcctName" Width="90px" />
									<px:PXGridColumn DataField="UOM" DisplayFormat="&gt;aaaaaa" Width="90px" CommitChanges="true" />
									<px:PXGridColumn DataField="IsPromotionalPrice" Type="CheckBox" Width="80px" TextAlign="Center" CommitChanges="true" />
									<px:PXGridColumn AutoCallBack="true" DataField="EffectiveDate" Width="90px" CommitChanges="true"/>
									<px:PXGridColumn DataField="PendingBreakQty" Width="90px" TextAlign="Right" CommitChanges="true"/>
								    <px:PXGridColumn DataField="PendingPrice" TextAlign="Right" Width="90px" />
									<px:PXGridColumn DataField="PendingTaxID" TextAlign="Right" Width="90px" />
									<px:PXGridColumn DataField="BreakQty" Width="90px" TextAlign="Right"/>
								    <px:PXGridColumn DataField="SalesPrice" TextAlign="Right" Width="90px" />
									<px:PXGridColumn DataField="TaxID" TextAlign="Right" Width="90px" />
									<px:PXGridColumn DataField="LastDate" Width="90px" />
									<px:PXGridColumn DataField="ExpirationDate" Width="90px" />
									<px:PXGridColumn DataField="LastBreakQty" Width="90px" TextAlign="Right"/>
									<px:PXGridColumn DataField="LastPrice" TextAlign="Right" Width="90px" />
									<px:PXGridColumn DataField="LastTaxID" TextAlign="Right" Width="90px" />
									<px:PXGridColumn DataField="CuryID" Width="81px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
						<ActionBar PagerVisible="False">
							<PagerSettings Mode="NextPrevFirstLast" />
							<CustomItems>
								<px:PXToolBarButton Text="Update Prices" Key="cmdUpdateCustomerPrice">
									<AutoCallBack Command="UpdateCustomerPrice" Target="ds">
										<Behavior PostData="Page" />
									</AutoCallBack>
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Vendor Prices" LoadOnDemand="true">
				<Template>
					<px:PXGrid ID="PXGridCustomerPrices" runat="server" DataSourceID="ds" Height="100%" Width="100%" SkinID="DetailsInTab" SyncPosition="true">
						<Mode InitNewRow="true" />
						<Levels>
							<px:PXGridLevel DataMember="VendorSalesPrice" DataKeyNames="RecordID">
								<RowTemplate>
									<px:PXSegmentMask ID="edVendorLocationID1" runat="server" DataField="VendorLocationID" AutoRefresh="True"/>
									<px:PXNumberEdit runat="server" DataField="PendingBreakQty" ID="edPendingBreakQty3"/>
									<px:PXNumberEdit runat="server" DataField="PendingPrice" ID="edPendingPrice3"/>
									<px:PXNumberEdit runat="server" DataField="BreakQty" ID="edBreakQty3"/>
									<px:PXNumberEdit runat="server" DataField="SalesPrice" ID="edSalesPrice3"/>
									<px:PXSegmentMask ID="edSubItemID2" runat="server" DataField="SubItemID" AutoRefresh="True" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="VendorID" Width="90px" CommitChanges="true" />
									<px:PXGridColumn DataField="Vendor__AcctName" Width="90px" />
									<px:PXGridColumn DataField="VendorLocationID"  Width="90px" CommitChanges="true"/>
									<px:PXGridColumn DataField="SubItemID" AutoCallBack="True" />
									<px:PXGridColumn DataField="UOM" DisplayFormat="&gt;aaaaaa" Width="90px" CommitChanges="true" />
									<px:PXGridColumn DataField="IsPromotionalPrice" Type="CheckBox" Width="80px" TextAlign="Center" CommitChanges="true"/>
									<px:PXGridColumn AutoCallBack="true" DataField="EffectiveDate" Width="90px" />
									<px:PXGridColumn DataField="PendingBreakQty" Width="90px" TextAlign="Right" CommitChanges="true"/>
								    <px:PXGridColumn DataField="PendingPrice" TextAlign="Right" Width="90px" />
									<px:PXGridColumn DataField="BreakQty" Width="90px" TextAlign="Right"/>
								    <px:PXGridColumn DataField="SalesPrice" TextAlign="Right" Width="90px" />
									<px:PXGridColumn DataField="LastDate" Width="90px" />
									<px:PXGridColumn DataField="ExpirationDate" Width="90px" />
									<px:PXGridColumn DataField="LastBreakQty" Width="90px" TextAlign="Right"/>
									<px:PXGridColumn DataField="LastPrice" TextAlign="Right" Width="90px" />
									<px:PXGridColumn DataField="CuryID" Width="90px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
						<ActionBar PagerVisible="False">
							<PagerSettings Mode="NextPrevFirstLast" />
							<CustomItems>
								<px:PXToolBarButton Text="Update Prices" Key="cmdUpdateAPVendorPrice">
									<AutoCallBack Command="UpdateAPVendorPrice" Target="ds">
										<Behavior PostData="Page" />
									</AutoCallBack>
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Vendor Details" LoadOnDemand="true">
				<Template>
					<px:PXGrid ID="PXGridVendorItems" runat="server" DataSourceID="ds" Height="100%" Width="100%" SkinID="DetailsInTab" SyncPosition="true">
						<Mode InitNewRow="True" />
						<Levels>
							<px:PXGridLevel DataMember="VendorItems" DataKeyNames="RecordID">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXSegmentMask ID="edVendorID" runat="server" DataField="VendorID" AllowEdit="True" />
									<px:PXSelector ID="edLocation__VBranchID" runat="server" DataField="Location__VBranchID" />
									<px:PXSegmentMask Size="xxs" ID="vp_edSubItemID" runat="server" DataField="SubItemID" AutoRefresh="True" />
									<px:PXSegmentMask ID="edLocation__VSiteID" runat="server" DataField="Location__VSiteID" />
									<px:PXSegmentMask ID="edVendorLocationID" runat="server" DataField="VendorLocationID" AutoRefresh="True" AllowEdit="True" />
									<px:PXMaskEdit ID="edVendorInventoryID" runat="server" DataField="VendorInventoryID" />
									<px:PXNumberEdit ID="edAddLeadTimeDays" runat="server" DataField="AddLeadTimeDays" />
									<px:PXCheckBox ID="vp_chkActive" runat="server" Checked="True" DataField="Active" />
									<px:PXNumberEdit ID="edMinOrdFreq" runat="server" DataField="MinOrdFreq" />
									<px:PXNumberEdit ID="edMinOrdQty" runat="server" DataField="MinOrdQty" />
									<px:PXNumberEdit ID="edMaxOrdQty" runat="server" DataField="MaxOrdQty" />
									<px:PXNumberEdit ID="edLotSize" runat="server" DataField="LotSize" />
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXNumberEdit ID="edERQ" runat="server" DataField="ERQ" />
									<px:PXSelector ID="edCuryID" runat="server" DataField="CuryID" />
									<px:PXNumberEdit ID="edPendingPrice" runat="server" DataField="PendingPrice" />
									<px:PXDateTimeEdit ID="edPendingDate" runat="server" DataField="PendingDate" />
									<px:PXNumberEdit ID="edEffPrice" runat="server" DataField="EffPrice" Enabled="False" />
									<px:PXDateTimeEdit ID="edEffDate" runat="server" DataField="EffDate" Enabled="False" />
									<px:PXNumberEdit ID="edLastPrice" runat="server" DataField="LastPrice" Enabled="False" />
									<px:PXCheckBox ID="chkIsDefault" runat="server" DataField="IsDefault" />
									<px:PXTextEdit ID="edVendor__AcctName" runat="server" DataField="Vendor__AcctName" />
									<px:PXNumberEdit ID="edLocation__VLeadTime" runat="server" DataField="Location__VLeadTime" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox" Width="45px" />
									<px:PXGridColumn DataField="IsDefault" TextAlign="Center" Type="CheckBox" Width="45px" />
									<px:PXGridColumn DataField="VendorID" Width="81px" AutoCallBack="True" />
									<px:PXGridColumn DataField="Vendor__AcctName" Width="210px" />
									<px:PXGridColumn DataField="VendorLocationID" Width="54px" AutoCallBack="True" />
									<px:PXGridColumn DataField="Location__VSiteID" Width="81px" />
									<px:PXGridColumn DataField="SubItemID" AutoCallBack="True" />
									<px:PXGridColumn DataField="PurchaseUnit" Width="63px" />
									<px:PXGridColumn DataField="VendorInventoryID" Width="90px" AutoCallBack="True" />
									<px:PXGridColumn DataField="Location__VLeadTime" Width="90px" TextAlign="Right" />
									<px:PXGridColumn DataField="OverrideSettings" TextAlign="Center" Type="CheckBox" Width="60px" AutoCallBack="True" />
									<px:PXGridColumn DataField="AddLeadTimeDays" TextAlign="Right" Width="90px" />
									<px:PXGridColumn DataField="MinOrdFreq" TextAlign="Right" Width="84px" />
									<px:PXGridColumn DataField="MinOrdQty" TextAlign="Right" Width="81px" />
									<px:PXGridColumn DataField="MaxOrdQty" TextAlign="Right" Width="81px" />
									<px:PXGridColumn DataField="LotSize" TextAlign="Right" Width="81px" />
									<px:PXGridColumn DataField="ERQ" TextAlign="Right" Width="81px" />
									<px:PXGridColumn DataField="CuryID" Width="54px" />
									<px:PXGridColumn DataField="PendingPrice" TextAlign="Right" Width="99px" />
									<px:PXGridColumn DataField="PendingDate" Width="90px" />
									<px:PXGridColumn DataField="EffPrice" TextAlign="Right" Width="99px" />
									<px:PXGridColumn DataField="EffDate" Width="90px" />
									<px:PXGridColumn DataField="LastPrice" TextAlign="Right" Width="99px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<ActionBar>
							<CustomItems>
								<px:PXToolBarButton Text="Update Effective Vendor Prices" Key="cmdVendorCostUpdate">
								    <AutoCallBack Command="UpdateVendorPrice" Target="ds">
										<Behavior PostData="Page" />
									</AutoCallBack>
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
						<AutoSize Enabled="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Attributes">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
					<px:PXGrid ID="PXGridAnswers" runat="server" Caption="Attributes" DataSourceID="ds" Height="150px" MatrixMode="True" Width="420px" SkinID="Attributes">
						<Levels>
							<px:PXGridLevel DataKeyNames="AttributeID,EntityType,EntityID" DataMember="Answers">
								<RowTemplate>
									<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="M" StartColumn="True" />
									<px:PXTextEdit ID="edParameterID" runat="server" DataField="AttributeID" Enabled="False" />
									<px:PXTextEdit ID="edAnswerValue" runat="server" DataField="Value" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn AllowShowHide="False" DataField="AttributeID" TextField="AttributeID_description" TextAlign="Left" Width="135px" />
    							<px:PXGridColumn DataField="isRequired" TextAlign="Center" Type="CheckBox" Width="80px" />
									<px:PXGridColumn DataField="Value" Width="185px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
					</px:PXGrid>
					<px:PXGrid ID="PXGridCategory" runat="server" Caption="Sales Categories" DataSourceID="ds" Height="150px" Width="420px" SkinID="ShortList">
						<Levels>
							<px:PXGridLevel DataMember="Categories">
								<Columns>
									<px:PXGridColumn DataField="CategoryID" Width="150px" />
									<px:PXGridColumn DataField="INCategory__Description" Width="250px" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
					</px:PXGrid>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
					<px:PXImageUploader Height="150px" Width="420px" ID="imgUploader" runat="server" DataField="ImageUrl" />
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Packaging">
				<Template>                
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Dimensions" />
					<px:PXNumberEdit ID="edBaseItemWeight" runat="server" DataField="BaseItemWeight"  />
					<px:PXSelector ID="edWeightUOM" runat="server" DataField="WeightUOM" Size="S" AutoRefresh="true" />
					<px:PXNumberEdit ID="edBaseItemVolume" runat="server" DataField="BaseItemVolume" />
					<px:PXSelector ID="edVolumeUOM" runat="server" DataField="VolumeUOM" Size="S" AutoRefresh="true" />
					<px:PXLayoutRule runat="server" StartColumn="True"/>
					<px:PXLayoutRule ID="PXLayoutRule5" runat="server" StartGroup="True" GroupCaption="Automatic Packaging"/>
					<px:PXLayoutRule ID="PXLayoutRule6" runat="server" Merge="True" />
					<px:PXDropDown ID="edPackageOption" runat="server" DataField="PackageOption" CommitChanges="true" AllowNull="False"/>
				    <px:PXCheckBox ID="edPackSeparately" DataField="PackSeparately" runat="server"/>
					<px:PXLayoutRule ID="PXLayoutRule7" runat="server" Merge="False" />
					<px:PXGrid ID="PXGridBoxes" runat="server" Caption="Boxes" DataSourceID="ds" Height="130px" Width="420px" SkinID="ShortList">
						<Levels>
							<px:PXGridLevel DataMember="Boxes">
								<RowTemplate>
									<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
									<px:PXSelector ID="edBoxID" runat="server" DataField="BoxID" />
									<px:PXSelector ID="edUOM_box" runat="server" DataField="UOM" />
									<px:PXNumberEdit ID="edQty_box" runat="server" DataField="Qty" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="BoxID" Width="91px" CommitChanges="True" />
									<px:PXGridColumn DataField="Description" Width="91px" />
									<px:PXGridColumn DataField="UOM" Width="54px" CommitChanges="True"/>
									<px:PXGridColumn DataField="Qty" TextAlign="Right" Width="54px" />
									<px:PXGridColumn DataField="MaxWeight" Width="54px" />
									<px:PXGridColumn DataField="MaxVolume" Width="54px" />
									<px:PXGridColumn DataField="MaxQty" TextAlign="Right" Width="54px" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Cross-Reference">
				<Template>
					<px:PXGrid ID="grid1" runat="server" DataSourceID="ds" Height="150px" Width="100%" ActionsPosition="Top" SkinID="DetailsInTab" SyncPosition="true">
						<Mode InitNewRow="True" />
						<Levels>
							<px:PXGridLevel DataMember="itemxrefrecords" DataKeyNames="InventoryID,SubItemID,AlternateType,BAccountID,AlternateID">
								<Columns>
									<px:PXGridColumn DataField="SubItemID" Width="135px" />
									<px:PXGridColumn DataField="AlternateType" Type="DropDownList" Width="135px" AutoCallBack="true" />
									<px:PXGridColumn DataField="BAccountID" Width="135px" AutoCallBack="true" />
									<px:PXGridColumn DataField="AlternateID" Width="180px" />
									<px:PXGridColumn DataField="Descr" Width="351px" />
								</Columns>
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXSelector ID="edBAccountID" runat="server" DataField="BAccountID" AutoRefresh="True" AllowEdit="True" CommitChanges="true" />
									<px:PXSegmentMask SuppressLabel="True" ID="edSubItemID" runat="server" DataField="SubItemID" AutoRefresh="True" />
									<px:PXTextEdit ID="edAlternateID" runat="server" DataField="AlternateID" />
									<px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
                                </RowTemplate>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
					</px:PXGrid>
				</Template>				
			</px:PXTabItem>
            <px:PXTabItem Text="Replenishment Info">
                <Template>
                    <px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="300" SkinID="Horizontal" Height="500px">
                        <AutoSize Enabled="true" />
                        <Template1>
                            <px:PXGrid ID="repGrid" runat="server" Height="250px" Width="100%" DataSourceID="ds" SkinID="DetailsInTab" Caption="Replenishment Parameters"
                                TabIndex="100">
                                <AutoCallBack Command="Refresh" Target="repSubGrid" />
                                <Mode InitNewRow="True" />
                                <Levels>
                                    <px:PXGridLevel DataMember="replenishment" DataKeyNames="InventoryID,ReplenishmentClassID">
                                        <RowTemplate>
                                            <px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                                            <px:PXSelector ID="edReplenishmentClassID" runat="server" DataField="ReplenishmentClassID" AllowEdit="True" CommitChanges="true" />
                                            <px:PXSelector ID="edReplenishmentPolicyID" runat="server" DataField="ReplenishmentPolicyID" AllowEdit="True" />
                                            <px:PXDropDown ID="edReplenishmentMethod" runat="server" DataField="ReplenishmentMethod" />
                                            <px:PXDropDown ID="edReplenishmentSource" runat="server" DataField="ReplenishmentSource" />
                                            <px:PXSegmentMask ID="edReplenishmentSourceSiteID" runat="server" DataField="ReplenishmentSourceSiteID" CommitChanges="true" />
                                            <px:PXNumberEdit ID="edMaxShelfLife" runat="server" DataField="MaxShelfLife" />
                                            <px:PXDateTimeEdit ID="edLaunchDate" runat="server" DataField="LaunchDate" />
                                            <px:PXDateTimeEdit ID="edTerminationDate" runat="server" DataField="TerminationDate" />
											<px:PXNumberEdit ID="edServiceLevelPct" runat="server" DataField="ServiceLevelPct" />
                                            <px:PXNumberEdit ID="edSafetyStock" runat="server" DataField="SafetyStock" />
                                            <px:PXNumberEdit ID="edMinQty" runat="server" DataField="MinQty" />
                                            <px:PXNumberEdit ID="edMaxQty" runat="server" DataField="MaxQty" />
											<px:PXNumberEdit ID="edTransferERQ" runat="server" DataField="TransferERQ" />
                                            <px:PXNumberEdit ID="edHistoryDepth" runat="server" AllowNull="true" Size="xxs" DataField="HistoryDepth" />
                                        </RowTemplate>
                                        <Columns>
                                            <px:PXGridColumn DataField="ReplenishmentClassID" AutoCallBack="True" Width="90px" />
                                            <px:PXGridColumn DataField="ReplenishmentPolicyID" Width="100px" />
                                            <px:PXGridColumn DataField="ReplenishmentMethod" Type="DropDownList" />
                                            <px:PXGridColumn DataField="ReplenishmentSource" Type="DropDownList" AutoCallBack="True" />
                                            <px:PXGridColumn DataField="ReplenishmentSourceSiteID" Width="140px" />
                                            <px:PXGridColumn DataField="MaxShelfLife" TextAlign="Right" />
                                            <px:PXGridColumn DataField="LaunchDate" Width="90px" />
                                            <px:PXGridColumn DataField="TerminationDate" Width="90px" />
											<px:PXGridColumn DataField="ServiceLevelPct" Width="90px" />
                                            <px:PXGridColumn DataField="SafetyStock" TextAlign="Right" Width="80px" />
                                            <px:PXGridColumn DataField="MinQty" TextAlign="Right" Width="80px" />
                                            <px:PXGridColumn DataField="MaxQty" TextAlign="Right" Width="80px" />
											<px:PXGridColumn DataField="TransferERQ" TextAlign="Right" Width="80px" />
                                            <px:PXGridColumn DataField="ForecastModelType" Label="Forecast Model Type" Width="140px" />
                                            <px:PXGridColumn DataField="ForecastPeriodType" Label="Forecast Period Type" Width="50px" />
                                            <px:PXGridColumn DataField="HistoryDepth" Label="History Scan Depth" Width="50px" />
                                        </Columns>
                                        <Layout FormViewHeight="" />
                                    </px:PXGridLevel>
                                </Levels>
                                <AutoSize Enabled="True" />
                            </px:PXGrid>
                        </Template1>
                        <Template2>
                            <px:PXGrid ID="repSubGrid" runat="server" Height="200px" Width="100%" DataSourceID="ds" SkinID="DetailsInTab" Caption="Subitem Replenishment Parameters"
                                Style="left: 0px; top: 0px" TabIndex="150">
                                <Mode InitNewRow="True" />
                                <CallbackCommands>
                                    <Save CommitChangesIDs="repSubGrid" RepaintControls="None" />
                                    <FetchRow RepaintControls="None" />
                                </CallbackCommands>
                                <Parameters>
                                    <px:PXSyncGridParam ControlID="repGrid" />
                                </Parameters>
                                <ActionBar>
                                    <CustomItems>
                                        <px:PXToolBarButton>
                                            <AutoCallBack Command="GenerateSubitems" Target="ds" />
                                        </px:PXToolBarButton>
                                        <px:PXToolBarButton>
                                            <AutoCallBack Command="UpdateReplenishment" Target="ds" />
                                        </px:PXToolBarButton>
                                    </CustomItems>
                                </ActionBar>
                                <Levels>
                                    <px:PXGridLevel DataMember="subreplenishment" DataKeyNames="InventoryID,ReplenishmentClassID,SubItemID">
                                        <RowTemplate>
                                            <px:PXSegmentMask ID="edRPSubItemID" runat="server" DataField="SubItemID" Width="27px" AutoRefresh="True" />
                                        </RowTemplate>
                                        <Columns>
                                            <px:PXGridColumn DataField="InventoryID" />
                                            <px:PXGridColumn DataField="ReplenishmentClassID" />
                                            <px:PXGridColumn DataField="SubItemID" />
                                            <px:PXGridColumn DataField="SafetyStock" TextAlign="Right" Width="100px" />
                                            <px:PXGridColumn DataField="MinQty" TextAlign="Right" Width="100px" />
                                            <px:PXGridColumn DataField="MaxQty" TextAlign="Right" Width="100px" />
											<px:PXGridColumn DataField="TransferERQ" TextAlign="Right" Width="80px" />
                                            <px:PXGridColumn DataField="ItemStatus" Type="DropDownList" />
                                        </Columns>
                                        <Layout FormViewHeight="" />
                                    </px:PXGridLevel>
                                </Levels>
                                <AutoSize Enabled="True" MinHeight="150" />
                            </px:PXGrid>
                        </Template2>
                    </px:PXSplitContainer>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Deferred Revenue">
				<Template>
					<px:PXFormView ID="formDR" runat="server" Width="100%" DataMember="ItemSettings" DataSourceID="ds" Caption="Rules" SkinID="Transparent">
						<Activity HighlightColor="" SelectedColor="" />
						<Template>
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
							<px:PXSelector CommitChanges="True" ID="edDeferredCode1" runat="server" DataField="DeferredCode" AllowEdit="True" DataSourceID="ds" />
							<px:PXCheckBox CommitChanges="True" ID="chkIsSplitted" runat="server" DataField="IsSplitted" />
							<px:PXCheckBox ID="chkUseParentSubID" runat="server" DataField="UseParentSubID" />
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
							<px:PXNumberEdit ID="edTotalPercentage" runat="server" DataField="TotalPercentage" Enabled="false" />
						</Template>
					</px:PXFormView>
					<px:PXGrid ID="PXGridComponents" runat="server" DataSourceID="ds" AllowFilter="False" Height="200px" Width="100%" Caption="Revenue Components" SkinID="DetailsWithFilter">
						<Mode InitNewRow="True" />
						<Levels>
							<px:PXGridLevel DataMember="Components" DataKeyNames="InventoryID,ComponentID">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
									<px:PXDropDown ID="edPriceOption" runat="server" DataField="AmtOption" />
									<px:PXSegmentMask ID="edComponentID" runat="server" DataField="ComponentID" AllowEdit="True" />
									<px:PXNumberEdit Size="xs" ID="edFixedAmt" runat="server" DataField="FixedAmt" />
									<px:PXSelector ID="edDeferredCode" runat="server" DataField="DeferredCode" AllowEdit="True" />
									<px:PXNumberEdit ID="edPercentage" runat="server" DataField="Percentage" />
									<px:PXSegmentMask ID="edSalesAcctID" runat="server" DataField="SalesAcctID" AllowEdit="True" />
									<px:PXSegmentMask ID="edSalesSubID" runat="server" DataField="SalesSubID" />
									<px:PXSelector ID="edUOM" runat="server" DataField="UOM" AllowEdit="True" />
									<px:PXNumberEdit ID="edQty" runat="server" DataField="Qty" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn AutoCallBack="True" DataField="ComponentID" Width="99px" />
									<px:PXGridColumn DataField="SalesAcctID" Width="99px" />
									<px:PXGridColumn DataField="SalesSubID" Width="99px" />
									<px:PXGridColumn DataField="UOM" Width="99px" />
									<px:PXGridColumn DataField="Qty" TextAlign="Right" Width="99px" />
									<px:PXGridColumn DataField="DeferredCode" Width="99px" />
									<px:PXGridColumn DataField="AmtOption" RenderEditorText="True" Width="81px" />
									<px:PXGridColumn DataField="FixedAmt" TextAlign="Right" Width="81px" />
									<px:PXGridColumn DataField="Percentage" TextAlign="Right" Width="99px" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="GL Accounts">
				<Template>
					<px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
					<px:PXSegmentMask ID="edInvtAcctID" runat="server" DataField="InvtAcctID" CommitChanges="true" />
					<px:PXSegmentMask ID="edInvtSubID" runat="server" DataField="InvtSubID" AutoRefresh="True" CommitChanges="True"/>
					<px:PXSegmentMask ID="edReasonCodeSubID" runat="server" DataField="ReasonCodeSubID" AutoRefresh="True" />
					<px:PXSegmentMask ID="edSalesAcctID" runat="server" DataField="SalesAcctID" CommitChanges="true" />
					<px:PXSegmentMask ID="edSalesSubID" runat="server" DataField="SalesSubID" AutoRefresh="True" />
					<px:PXSegmentMask ID="edCOGSAcctID" runat="server" DataField="COGSAcctID" CommitChanges="true" />
					<px:PXSegmentMask ID="edCOGSSubID" runat="server" DataField="COGSSubID" AutoRefresh="True" />
					<px:PXSegmentMask ID="edStdCstVarAcctID" runat="server" DataField="StdCstVarAcctID" CommitChanges="true" />
					<px:PXSegmentMask ID="edStdCstVarSubID" runat="server" DataField="StdCstVarSubID" AutoRefresh="True" />
					<px:PXSegmentMask ID="edStdCstRevAcctID" runat="server" DataField="StdCstRevAcctID" AutoRefresh="True" />
					<px:PXSegmentMask ID="edStdCstRevSubID" runat="server" DataField="StdCstRevSubID" AutoRefresh="True" />
					<px:PXSegmentMask ID="edPOAccrualAcctID" runat="server" DataField="POAccrualAcctID" CommitChanges="true" />
					<px:PXSegmentMask ID="edPOAccrualSubID" runat="server" DataField="POAccrualSubID" AutoRefresh="True" />
					<px:PXSegmentMask ID="edPPVAcctID" runat="server" DataField="PPVAcctID" CommitChanges="true" />
					<px:PXSegmentMask ID="edPPVSubID" runat="server" DataField="PPVSubID" AutoRefresh="True" />
					<px:PXSegmentMask ID="edLCVarianceAcctID" runat="server" DataField="LCVarianceAcctID" CommitChanges="true" />
					<px:PXSegmentMask ID="edLCVarianceSubID" runat="server" DataField="LCVarianceSubID" AutoRefresh="True" />
					<px:PXSegmentMask ID="edDiscAcctID" runat="server" DataField="DiscAcctID" CommitChanges="true" />
					<px:PXSegmentMask ID="edDiscSubID" runat="server" DataField="DiscSubID" AutoRefresh="True" />
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Restriction Groups">
				<Template>
					<px:PXGrid ID="grid3" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" AdjustPageSize="Auto" AllowSearch="True" SkinID="Details" BorderWidth="0px">
						<ActionBar>
							<Actions>
								<NoteShow Enabled="False" />
							</Actions>
							<CustomItems>
							    <px:PXToolBarButton Text="Group Details" CommandSourceID="ds" CommandName="ViewGroupDetails"/>
							</CustomItems>
						</ActionBar>
						<Levels>
							<px:PXGridLevel DataMember="Groups" DataKeyNames="GroupName">
								<Mode AllowAddNew="False" AllowDelete="False" />
								<Columns>
									<px:PXGridColumn DataField="Included" TextAlign="Center" Type="CheckBox" Width="40px" AllowCheckAll="True" />
									<px:PXGridColumn DataField="GroupName" Width="150px" />
									<px:PXGridColumn DataField="SpecificType" Width="150px" Type="DropDownList" />
									<px:PXGridColumn DataField="Description" Width="200px" />
									<px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox" Width="60px" />
									<px:PXGridColumn DataField="GroupType" Label="Visible To Entities" Width="171px" Type="DropDownList" />
								</Columns>
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXCheckBox SuppressLabel="True" ID="chkSelected" runat="server" DataField="Included" />
									<px:PXSelector ID="edGroupName" runat="server" DataField="GroupName" />
									<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
									<px:PXCheckBox SuppressLabel="True" ID="chkActive" runat="server" Checked="True" DataField="Active" />
									<px:PXDropDown ID="edGroupType" runat="server" DataField="GroupType" Enabled="False" />
								</RowTemplate>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Enabled="True" MinHeight="150" />
	</px:PXTab>
	<px:PXSmartPanel ID="pnlUpdatePrice" runat="server" Key="VendorItems" CaptionVisible="true" DesignView="Content" Caption="Update Effective Vendor Prices" AllowResize="false">
		<px:PXFormView ID="formEffectiveDate" runat="server" DataSourceID="ds" CaptionVisible="false" DataMember="VendorInventory$UpdatePrice" Width="280px" Height="50px" SkinID="Transparent">
			<Template>
				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
				<px:PXDateTimeEdit ID="edPendingDate" runat="server" DataField="PendingDate" />
			</Template>
		</px:PXFormView>
		<px:PXPanel ID="PXPanelBtn" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton3" runat="server" DialogResult="OK" Text="Update" />
			<px:PXButton ID="PXButton4" runat="server" DialogResult="No" Text="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXSmartPanel ID="PanelMassUpdate" runat="server" CommandSourceID="ds" Caption="Update Prices" CaptionVisible="True" ShowAfterLoad="true" LoadOnDemand="true"
        DesignView="Content" Key="MassUpdateSettings" AutoCallBack-Enabled="true" AutoCallBack-Target="massUpdateForm" AutoCallBack-Command="Refresh">
        <div style="padding: 5px">
            <px:PXFormView ID="massUpdateForm" runat="server" Width="100%" DataSourceID="ds" SkinID="Transparent" DataMember="MassUpdateSettings">
                <Template>
                    <px:PXLayoutRule ID="PXLayoutRule6" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXDateTimeEdit ID="edEffectiveDate" runat="server" DataField="EffectiveDate" />
                </Template>
            </px:PXFormView>
        </div>
        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
            <px:PXButton ID="btnSave" runat="server" DialogResult="OK" Text="OK" >
                <AutoCallBack Command="Save" Target="massUpdateForm"/>
            </px:PXButton>
            <px:PXButton ID="btnCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
