<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="IN202000.aspx.cs" Inherits="Page_IN202000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" AutoCallBack="True" Visible="True" Width="100%" TypeName="PX.Objects.IN.NonStockItemMaint" PrimaryView="Item">
    <CallbackCommands>
      <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
      <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
      <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
      <px:PXDSCallbackCommand Name="Last" PostData="Self" />
      <px:PXDSCallbackCommand StartNewGroup="True" Name="Action" CommitChanges="true"/>	
	  <px:PXDSCallbackCommand Name="UpdateCustPriceClass" Visible="false" CommitChanges="true" />
	  <px:PXDSCallbackCommand Name="UpdateCustomerPrice" Visible="false" CommitChanges="true" />
	  <px:PXDSCallbackCommand Name="UpdateAPVendorPrice" Visible="false" CommitChanges="true" />
	  <px:PXDSCallbackCommand Name="UpdateVendorPrice" Visible="false" CommitChanges="true" />
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
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Item" Caption="Non-Stock Item Summary" NoteIndicator="True" FilesIndicator="True" ActivityIndicator="True"
		ActivityField="NoteActivity" DefaultControlID="edInventoryCD">
    <Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			<px:PXSegmentMask ID="edInventoryCD" runat="server" DataField="InventoryCD" DataSourceID="ds" DisplayMode="Value" AutoRefresh="true"  />
            <px:PXDropDown ID="edItemStatus" runat="server" DataField="ItemStatus" Size="S" />
			<px:PXLayoutRule runat="server" ColumnSpan="2" />
			<px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			<px:PXSelector CommitChanges="True" ID="edProductWorkgroupID" runat="server" DataField="ProductWorkgroupID">
			</px:PXSelector>
			<px:PXSelector ID="edProductManagerID" runat="server" DataField="ProductManagerID" AutoRefresh="True" DataSourceID="ds" />
    </Template>
  </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Width="100%" Height="487px" DataSourceID="ds" DataMember="ItemSettings" MarkRequired="Dynamic">
    <Items>
      <px:PXTabItem Text="General Settings">
        <Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Item Defaults" />
					<px:PXSelector CommitChanges="True" ID="edItemClassID" runat="server" DataField="ItemClassID" AllowEdit="True" AutoRefresh="True" />
					<px:PXDropDown ID="edItemType" runat="server" DataField="ItemType" />
					<px:PXSelector ID="edPostClassID" runat="server" DataField="PostClassID" AllowEdit="True" AutoRefresh="True" CommitChanges="True" />
					<px:PXCheckBox ID="chkKitItem" runat="server" DataField="KitItem" />
					<px:PXSelector ID="edTaxCategoryID" runat="server" DataField="TaxCategoryID" AllowEdit="True" AutoRefresh="True"/>
					<px:PXSegmentMask ID="edDfltSiteID" runat="server" DataField="DfltSiteID" />
					<px:PXCheckBox ID="chkNonStockReceipt" runat="server" Checked="True" DataField="NonStockReceipt" CommitChanges="true"  />
					<px:PXCheckBox ID="chkNonStockShip" runat="server" Checked="True" DataField="NonStockShip" />
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Unit of Measure" />
					<px:PXSelector CommitChanges="True" ID="edBaseUnit" runat="server" DataField="BaseUnit" Size="S" />
					<px:PXSelector CommitChanges="True" ID="edSalesUnit" runat="server" DataField="SalesUnit" AutoRefresh="True" Size="S" />
					<px:PXSelector CommitChanges="True" ID="edPurchaseUnit" runat="server" DataField="PurchaseUnit" AutoRefresh="True" Size="S" />
					<px:PXLayoutRule runat="server" SuppressLabel="True" ControlSize="XM" LabelsWidth="SM" />
					<px:PXGrid ID="gridUnits" runat="server" DataSourceID="ds" Height="121px" Width="400px">
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
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Price/Cost Information">
				<Template>
					<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" GroupCaption="Base Price" />
					<px:PXSelector ID="edPriceClassID" runat="server" DataField="PriceClassID" AllowEdit="True" />
					<px:PXNumberEdit ID="edPendingBasePrice" runat="server" DataField="PendingBasePrice" CommitChanges="true" />
					<px:PXDateTimeEdit ID="edPendingBasePriceDate" runat="server" DataField="PendingBasePriceDate" CommitChanges="true"/>
					<px:PXNumberEdit ID="edBasePrice" runat="server" DataField="BasePrice" Enabled="False" />
					<px:PXDateTimeEdit ID="edBasePriceDate" runat="server" DataField="BasePriceDate" Enabled="False" />
					<px:PXNumberEdit ID="edLastBasePrice" runat="server" DataField="LastBasePrice" Enabled="False" />
					<px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartGroup="True" GroupCaption="Standard Cost" />
					<px:PXNumberEdit ID="edPendingStdCost" runat="server" DataField="PendingStdCost" />
					<px:PXDateTimeEdit ID="edPendingStdCostDate" runat="server" DataField="PendingStdCostDate" />
					<px:PXNumberEdit ID="edStdCost" runat="server" DataField="StdCost" Enabled="False" />
					<px:PXDateTimeEdit ID="edStdCostDate" runat="server" DataField="StdCostDate" Enabled="False" />
					<px:PXNumberEdit ID="edLastStdCost" runat="server" DataField="LastStdCost" Enabled="False" />
					<px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="true" LabelsWidth="SM" ControlSize="XM" GroupCaption="Price Management" />
				    <px:PXSelector CommitChanges="True" ID="edPriceWorkgroupID" runat="server" DataField="PriceWorkgroupID" />					
					<px:PXSelector ID="edPriceManagerID" runat="server" DataField="PriceManagerID" AutoRefresh="True" />
					<px:PXCheckBox ID="chkCommisionable" runat="server" DataField="Commisionable" />
					<px:PXNumberEdit ID="edMinGrossProfitPct" runat="server" DataField="MinGrossProfitPct" />
					<px:PXNumberEdit ID="edMarkupPct" runat="server" DataField="MarkupPct" />
					<px:PXNumberEdit ID="edRecPrice" runat="server" DataField="RecPrice" />
					<px:PXLayoutRule runat="server" ID="PXLayoutRuleC1" StartGroup="true" GroupCaption="RUT and RUT Settings" LabelsWidth="SM" ControlSize="XM" />
					<px:PXCheckBox runat="server" DataField="IsRUTROTDeductible" ID="chkIsRUTROTDeductible" AlignLeft="True" />
           
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
									<px:PXGridColumn DataField="CustPriceClassID" Width="90px"  CommitChanges="true"/>
									<px:PXGridColumn DataField="UOM" DisplayFormat="&gt;aaaaaa" Width="90px" CommitChanges="true" />
									<px:PXGridColumn DataField="IsPromotionalPrice" Type="CheckBox" Width="80px" TextAlign="Center" CommitChanges="true"/>
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
									<px:PXGridColumn DataField="CuryID" Width="90px" />
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
									<px:PXGridColumn DataField="IsPromotionalPrice" Type="CheckBox" Width="80px" TextAlign="Center" CommitChanges="true"/>
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
									<px:PXGridColumn DataField="CuryID" Width="90px" />
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
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="VendorID" Width="90px" CommitChanges="true" />
									<px:PXGridColumn DataField="Vendor__AcctName" Width="90px" />
									<px:PXGridColumn DataField="VendorLocationID"  Width="90px" CommitChanges="true" />
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
					<px:PXGrid ID="PXGridVendorItems" runat="server" DataSourceID="ds" Height="100%" Width="100%" BorderWidth="0px" SkinID="Details" SyncPosition="true">
						<Mode InitNewRow="True" />
						<Levels>
							<px:PXGridLevel  DataMember="VendorItems">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
									<px:PXCheckBox ID="vp_chkActive" runat="server" Checked="True" DataField="Active" />
									<px:PXCheckBox ID="IsDefault" runat="server" DataField="IsDefault" Text="Default"/>
									<px:PXSegmentMask ID="edVendorID" runat="server" DataField="VendorID" CommitChanges="True" AllowEdit="True" />
									<px:PXSegmentMask ID="edVendorLocationID" runat="server" DataField="VendorLocationID" AutoRefresh="True" />
									<px:PXMaskEdit ID="edVendorInventoryID" runat="server" DataField="VendorInventoryID" />
									<px:PXNumberEdit ID="edPendingPrice" runat="server" DataField="PendingPrice" />
									<px:PXDateTimeEdit ID="edPendingDate" runat="server" DataField="PendingDate" />
									<px:PXNumberEdit ID="edEffPrice" runat="server" DataField="EffPrice" Enabled="False" />
									<px:PXDateTimeEdit ID="edEffDate" runat="server" DataField="EffDate" Enabled="False" />
									<px:PXNumberEdit ID="edLastPrice" runat="server" DataField="LastPrice" Enabled="False" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox" Width="45px" />
									<px:PXGridColumn DataField="IsDefault" Width="60px" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn DataField="VendorID" Width="81px" AutoCallBack="True" />
									<px:PXGridColumn DataField="Vendor__AcctName" Width="210px" />
									<px:PXGridColumn DataField="VendorLocationID" Width="54px" AutoCallBack="True" />
									<px:PXGridColumn DataField="PurchaseUnit" Width="63px" />
									<px:PXGridColumn DataField="VendorInventoryID" Width="90px" AutoCallBack="True" />
									<px:PXGridColumn DataField="CuryID" Width="54px" />
                                    <px:PXGridColumn DataField="PendingPrice" TextAlign="Right" Width="99px" />
									<px:PXGridColumn DataField="PendingDate" Width="90px" />
									<px:PXGridColumn DataField="EffPrice" TextAlign="Right" Width="99px" />
									<px:PXGridColumn DataField="EffDate" Width="90px" />
								    <px:PXGridColumn DataField="LastPrice" TextAlign="Right" Width="99px" />
								</Columns>
								<Layout FormViewHeight="" />
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
			<px:PXTabItem Text="Packaging">
        <Template>
					<px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="S" />
					<px:PXLayoutRule ID="PXLayoutRule5" runat="server" StartGroup="True" GroupCaption="Dimensions" />
					<px:PXNumberEdit ID="edBaseItemWeight" runat="server" DataField="BaseItemWeight" />
					<px:PXSelector ID="edWeightUOM" runat="server" DataField="WeightUOM" />
					<px:PXNumberEdit ID="edBaseItemVolume" runat="server" DataField="BaseItemVolume" />
					<px:PXSelector ID="edVolumeUOM" runat="server" DataField="VolumeUOM" />
	    </Template>
      </px:PXTabItem>
      <px:PXTabItem Text="Deferred Revenue">
        <Template>
			<px:PXFormView ID="formDR" runat="server" Width="100%" DataMember="ItemSettings" DataSourceID="ds" Caption="Rules" CaptionVisible="False" SkinID="Transparent">
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
              <px:PXGridLevel DataMember="Components" >
                <RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXDropDown ID="edPriceOption" runat="server" DataField="AmtOption" />
									<px:PXSegmentMask ID="edComponentID" runat="server" DataField="ComponentID" AllowEdit="True" />
									<px:PXNumberEdit ID="edFixedAmt" runat="server" DataField="FixedAmt" />
									<px:PXSelector ID="edDeferredCode" runat="server" DataField="DeferredCode" AllowEdit="True" />
									<px:PXNumberEdit ID="edPercentage" runat="server" DataField="Percentage" />
									<px:PXSegmentMask ID="edSalesAcctID" runat="server" DataField="SalesAcctID" CommitChanges="true" />
									<px:PXSegmentMask ID="edSalesSubID" runat="server" DataField="SalesSubID" />
									<px:PXSelector ID="edUOM" runat="server" DataField="UOM" />
									<px:PXNumberEdit ID="edQty" runat="server" DataField="Qty" />
                </RowTemplate>
                <Columns>
									<px:PXGridColumn AutoCallBack="True" DataField="ComponentID" Width="99px" />
									<px:PXGridColumn DataField="SalesAcctID" Width="99px" />
									<px:PXGridColumn DataField="SalesSubID" Width="99px" />
									<px:PXGridColumn DataField="UOM" Width="99px" />
									<px:PXGridColumn DataField="Qty" TextAlign="Right" Width="99px" />
									<px:PXGridColumn DataField="DeferredCode" Width="99px" />
									<px:PXGridColumn DataField="AmtOption" Width="81px" />
									<px:PXGridColumn DataField="FixedAmt" TextAlign="Right" Width="81px" />
									<px:PXGridColumn DataField="Percentage" TextAlign="Right" Width="99px" />
                </Columns>
              </px:PXGridLevel>
            </Levels>
            <AutoSize Enabled="True"/>
          </px:PXGrid>
        </Template>
      </px:PXTabItem>
			<px:PXTabItem Text="GL Accounts">
				<Template>
					<px:PXLayoutRule ID="PXLayoutRule6" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
					<px:PXSegmentMask ID="edInvtAcctID" runat="server" DataField="InvtAcctID" CommitChanges="True" />
					<px:PXSegmentMask ID="edInvtSubID" runat="server" DataField="InvtSubID" AutoRefresh="True" />
					<px:PXSegmentMask CommitChanges="True" ID="edExpenseAccountID" runat="server" DataField="COGSAcctID" />
					<px:PXSegmentMask ID="edExpenseSubID" runat="server" DataField="COGSSubID" AutoRefresh="True" />
                    <px:PXSegmentMask ID="edPOAccrualAcctID" runat="server" DataField="POAccrualAcctID" CommitChanges="true" />
					<px:PXSegmentMask ID="edPOAccrualSubID" runat="server" DataField="POAccrualSubID" AutoRefresh="True" />
					<px:PXSegmentMask CommitChanges="True" ID="edSalesAcctID" runat="server" DataField="SalesAcctID" />
					<px:PXSegmentMask ID="edSalesSubID" runat="server" DataField="SalesSubID" AutoRefresh="True" />
				</Template>
			</px:PXTabItem>
            <px:PXTabItem Text="Attributes">
                <Template>
                    <px:PXGrid ID="PXGridAnswers" runat="server" DataSourceID="ds" Width="100%" Height="100%" SkinID="DetailsInTab" MatrixMode="True">
                        <Levels>
                            <px:PXGridLevel DataMember="Answers">
                                <Columns>
                                    <px:PXGridColumn DataField="AttributeID" TextAlign="Left" Width="220px" AllowShowHide="False" />
    								<px:PXGridColumn DataField="isRequired" TextAlign="Center" Type="CheckBox" Width="75px" />
                                    <px:PXGridColumn DataField="Value" Width="148px" RenderEditorText="True" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="true" />
                        <Mode AllowAddNew="False" AllowColMoving="False" AllowDelete="False" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>


    </Items>
    <AutoSize Container="Window" Enabled="True" MinHeight="150" />
  </px:PXTab>
	<px:PXSmartPanel ID="pnlUpdatePrice" runat="server" Key="VendorItems" CaptionVisible="True" Caption="Update Effective Vendor Prices" AllowResize="False">
		<px:PXFormView ID="formEffectiveDate" runat="server" DataSourceID="ds" CaptionVisible="false" DataMember="VendorInventory$UpdatePrice" SkinID="Transparent">
			<Activity Height="" HighlightColor="" SelectedColor="" Width="" />
			<Template>
				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
				<px:PXDateTimeEdit ID="edPendingDate" runat="server" DataField="PendingDate" />
			</Template>
		</px:PXFormView>
		<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton9" runat="server" DialogResult="OK" Text="Update" />
			<px:PXButton ID="PXButton10" runat="server" DialogResult="No" Text="Cancel" />
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
        <px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
            <px:PXButton ID="btnSave" runat="server" DialogResult="OK" Text="OK" >
            </px:PXButton>
            <px:PXButton ID="btnCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
