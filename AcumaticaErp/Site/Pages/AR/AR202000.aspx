<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AR202000.aspx.cs"
    Inherits="Page_AR202000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.AR.ARSalesPriceMaint">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="Preload" Visible="False" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Process" Visible="False" DependOnGrid="grid" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="ProcessAll" Visible="False" DependOnGrid="grid" />
			<px:PXDSCallbackCommand CommitChanges="true" Visible="false" Name="WCopyNext" />
			<px:PXDSCallbackCommand CommitChanges="true" Visible="false" Name="WCopySave" />
			<px:PXDSCallbackCommand CommitChanges="true" Visible="false" Name="WCalcNext" />
			<px:PXDSCallbackCommand CommitChanges="true" Visible="false" Name="WCalcSave" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Selection" DataMember="Filter"
        DefaultControlID="edCustPriceClassID">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXSelector CommitChanges="True" ID="edCustPriceClassID" runat="server" DataField="CustPriceClassID" AllowEdit="True" />
            <px:PXSelector CommitChanges="True" ID="edCuryID" runat="server" DataField="CuryID" AllowEdit="True" />
			<px:PXCheckBox runat="server" ID="chkPromotionalPrice" DataField="PromotionalPrice" CommitChanges="true" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXLayoutRule runat="server" Merge="True" />
            <px:PXSelector CommitChanges="True" ID="edPriceManagerID" runat="server" DataField="OwnerID" />
            <px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkMyUser" runat="server" Checked="True" DataField="MyOwner" />
            <px:PXLayoutRule runat="server" Merge="False" />
            <px:PXLayoutRule runat="server" Merge="True" />
            <px:PXSelector CommitChanges="True" ID="edWorkGroupID" runat="server" DataField="WorkGroupID" />
            <px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkMyWorkGroup" runat="server" DataField="MyWorkGroup" />
            <px:PXLayoutRule runat="server" Merge="False" />
            <px:PXSelector CommitChanges="True" ID="edInventoryPriceClassID" runat="server" DataField="InventoryPriceClassID" AllowEdit="True" />
            <px:PXSelector CommitChanges="True" ID="edItemClassID" runat="server" DataField="ItemClassID" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <%--<px:PXSmartPanel ID="PanelMassCopy" runat="server" CommandSourceID="ds" Caption="Copy Prices" CaptionVisible="True" ShowAfterLoad="true" LoadOnDemand="true"
        DesignView="Content" Key="MassCopySettings" AutoCallBack-Enabled="true" AutoCallBack-Target="massCopyForm" AutoCallBack-Command="Refresh">
        <div style="padding: 5px">
            <px:PXFormView ID="massCopyForm" runat="server" Width="100%" DataSourceID="ds" SkinID="Transparent" DataMember="MassCopySettings">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXTextEdit ID="edLabel" runat="server" BorderStyle="None" DataField="Label" NullText="WARNING: " SkinID="Label" />
                    <px:PXSelector CommitChanges="True" ID="edCustPriceClassID" runat="server" DataField="CustPriceClassID" AllowEdit="True" />
                    <px:PXDateTimeEdit ID="edEffectiveDate" runat="server" DataField="EffectiveDate" />
                    <px:PXNumberEdit ID="edCorrectionPercent" runat="server" DataField="CorrectionPercent" />
                    <px:PXNumberEdit ID="edRounding" runat="server" DataField="Rounding" />
                    <px:PXCheckBox ID="chkUsePendingPrice" runat="server" DataField="UsePendingPrice" />
                    <px:PXCheckBox ID="chkOverrideExisting" runat="server" DataField="OverrideExisting" />
                    <px:PXSelector CommitChanges="True" ID="edCuryID" runat="server" AutoRefresh="True" DataField="CuryID" AllowEdit="True" />
                    <px:PXSelector ID="edRateTypeID" runat="server" DataField="RateTypeID" AllowEdit="True" />
                    <px:PXDateTimeEdit ID="edCurrencyDate" runat="server" DataField="CurrencyDate" />
                </Template>
            </px:PXFormView>
        </div>
        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
            <px:PXButton ID="btnSave" runat="server" DialogResult="OK" Text="Copy">
                <AutoCallBack Command="Save" Target="massCopyForm"/>
            </px:PXButton>
            <px:PXButton ID="btnCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>--%>
    <%--<px:PXSmartPanel ID="PanelMassUpdate" runat="server" CommandSourceID="ds" Caption="Calc. Pending Prices" CaptionVisible="True" ShowAfterLoad="true" LoadOnDemand="true"
        DesignView="Content" Key="MassUpdateSettings" AutoCallBack-Enabled="true" AutoCallBack-Target="massUpdateForm" AutoCallBack-Command="Refresh">
        <div style="padding: 5px">
            <px:PXFormView ID="massUpdateForm" runat="server" Width="100%" DataSourceID="ds" SkinID="Transparent" DataMember="MassUpdateSettings">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXTextEdit ID="edLabel" runat="server" BorderStyle="None" DataField="Label" NullText="WARNING: " SkinID="Label" />
                    <px:PXDateTimeEdit ID="edEffectiveDate" runat="server" DataField="EffectiveDate" />
                    <px:PXNumberEdit ID="edCorrectionPercent" runat="server" DataField="CorrectionPercent" />
                    <px:PXNumberEdit ID="edRounding" runat="server" DataField="Rounding" />
                    <px:PXCheckBox SuppressLabel="True" ID="chkSkip" runat="server" Checked="True" DataField="UpdateOnZero" />
                    <px:PXLayoutRule runat="server" StartColumn="True" SuppressLabel="True" />
                    <px:PXGroupBox RenderStyle="Fieldset" ID="MassUpdateGroupBox" runat="server" DataField="PriceBasis" Caption="Price Basis">
                        <Template>
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                            <px:PXRadioButton ID="rdbLastCost" runat="server" Text="Last Cost + Markup %" Value="L" />
                            <px:PXRadioButton ID="rdbStdCost" runat="server" Text="Avg/Std. Cost + Markup %" Value="S" />
                            <px:PXRadioButton ID="rdbCurrentPrice" runat="server" Text="Current Price" Value="P" Checked="True" />
                            <px:PXRadioButton ID="rdbRecPrice" runat="server" Text="Recommended Price" Value="R" />
                            <px:PXRadioButton ID="PXRadioButton1" runat="server" Text="Pending Price" Value="N" />
                        </Template>
                    </px:PXGroupBox>
                </Template>
            </px:PXFormView>
        </div>
        <px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
            <px:PXButton ID="btnSave2" runat="server" DialogResult="OK" Text="Update">
                <AutoCallBack Command="Save" Target="massUpdateForm"/>
            </px:PXButton>
            <px:PXButton ID="btnCancel2" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>--%>
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="144px" Style="z-index: 100" Width="100%" Caption="Sales Prices"
        SkinID="Details" FilterShortCuts="True" AdjustPageSize="Auto" AllowPaging="True">
        <Levels>
            <px:PXGridLevel DataMember="Records">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                    <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True">
                        <GridProperties>
                            <PagerSettings Mode="NextPrevFirstLast" />
                        </GridProperties>
                    </px:PXSegmentMask>
                    <px:PXDateTimeEdit ID="edEffectiveDate" runat="server" DataField="EffectiveDate" />
					<px:PXNumberEdit ID="edPendingBreakQty" runat="server" DataField="PendingBreakQty" />
                    <px:PXNumberEdit ID="edPendingPrice" runat="server" DataField="PendingPrice" />
                    <px:PXNumberEdit ID="edBreakQty" runat="server" DataField="BreakQty" />
                    <px:PXSelector ID="edPendingTaxID" runat="server" DataField="PendingTaxID" />
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                    <px:PXSelector ID="edUOM" runat="server" DataField="UOM" AutoRefresh="true">
                        <Parameters>
                            <px:PXSyncGridParam ControlID="grid" />
                        </Parameters>
                    </px:PXSelector>
                    <px:PXDateTimeEdit ID="edLastDate" runat="server" DataField="LastDate" />
                    <px:PXNumberEdit ID="edLastPrice" runat="server" DataField="LastPrice" />
                    <px:PXSelector ID="edLastTaxID" runat="server" DataField="LastTaxID" />
                    <px:PXLayoutRule ID="PXLayoutRule13" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                    <px:PXNumberEdit ID="edSalesPrice" runat="server" DataField="SalesPrice" />
                    <px:PXSelector ID="edTaxID" runat="server" DataField="TaxID" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" Width="60px" AllowCheckAll="True" />
                    <px:PXGridColumn DataField="InventoryID" Width="108px" AutoCallBack="True" />
                    <px:PXGridColumn AllowUpdate="False" DataField="InventoryItem__Descr" Width="130px" />
                    <px:PXGridColumn AllowUpdate="False" DataField="InventoryItem__PriceClassID" DisplayFormat="&gt;aaaaaaaaaa" Width="81px" />
                    <px:PXGridColumn DataField="UOM" DisplayFormat="&gt;aaaaaa" Width="90px" CommitChanges="true" />
                    <px:PXGridColumn AutoCallBack="true" DataField="EffectiveDate" Width="90px" />
					<px:PXGridColumn DataField="PendingBreakQty" Width="90px" TextAlign="Right" CommitChanges="true"/>
                    <px:PXGridColumn DataField="PendingPrice" TextAlign="Right" Width="81px" />
                    <px:PXGridColumn DataField="PendingTaxID" DisplayFormat="&gt;aaaaaaaaaa" Width="81px" />
					<px:PXGridColumn DataField="BreakQty" Width="90px" TextAlign="Right"/>
                    <px:PXGridColumn DataField="SalesPrice" TextAlign="Right" Width="81px" />
                    <px:PXGridColumn DataField="TaxID" DisplayFormat="&gt;aaaaaaaaaa" Width="81px" />
                    <px:PXGridColumn DataField="LastDate" Width="90px" />
					<px:PXGridColumn DataField="ExpirationDate" Width="90px" />
					<px:PXGridColumn DataField="LastBreakQty" Width="90px" TextAlign="Right"/>
                    <px:PXGridColumn DataField="LastPrice" TextAlign="Right" Width="81px" />
                    <px:PXGridColumn DataField="LastTaxID" DisplayFormat="&gt;aaaaaaaaaa" Width="81px" />
                    <px:PXGridColumn DataField="CuryID" Width="90px" />
                    <%--<px:PXGridColumn AllowUpdate="False" DataField="LastCost" TextAlign="Right" Width="90px" />--%>
                    <%--<px:PXGridColumn AllowUpdate="False" DataField="AvgCost" TextAlign="Right" Width="90px" />--%>
                    <%--<px:PXGridColumn AllowUpdate="False" DataField="MinGPPrice" TextAlign="Right" Width="90px" />--%>
                    <%--<px:PXGridColumn AllowUpdate="False" DataField="InventoryItem__MarkupPct" Width="81px" />--%>
                    <%--<px:PXGridColumn AllowUpdate="False" DataField="RecPrice" Width="81px" />--%>
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <ActionBar PagerVisible="False">
            <PagerSettings Mode="NextPrevFirstLast" />
            <CustomItems>
                <px:PXToolBarButton Text="Preload from Inventory" Tooltip="Appends missing records from Inventory" Key="cmdPreload">
                    <AutoCallBack Command="Preload" Target="ds">
                        <Behavior PostData="Page" />
                    </AutoCallBack>
                </px:PXToolBarButton>
                <px:PXToolBarSeperator />
                <pxa:PXGridProcessing ListItems="Copy Prices,Calc. Pending Prices, Update Prices" ParameterName="action" DataMember="Operations" DataField="Action" />
            </CustomItems>
        </ActionBar>
        <CallbackCommands>
            <Save PostData="Page" />
        </CallbackCommands>
        <LevelStyles>
            <RowForm Height="160px" Width="755px"/>
        </LevelStyles>
        <Mode AllowFormEdit="True" AllowUpload="True" />
    </px:PXGrid>
	<%--<px:PXSmartPanel ID="spCopyPriceDlg" runat="server" CommandName="Process" CommandSourceID="ds"
		Width="435" Caption="Copy Prices Wizard" Key="MassCopySettings" CaptionVisible="True"
		Style="display: none;" AutoCallBack-Enabled="true" AutoCallBack-Target="PXWizard1" AutoCallBack-Command="WizCancel">--%>
	<px:PXSmartPanel ID="spCopyPriceDlg" runat="server"
        Width="400px" Caption="Copy Prices Wizard" CaptionVisible="True" Key="MassCopySettings" LoadOnDemand="True" ShowAfterLoad="true"
        AutoCallBack-Enabled="true" AutoCallBack-Target="form" AutoCallBack-Command="Refresh"
        CallBackMode-CommitChanges="True" CallBackMode-PostData="Page">
        <px:PXWizard ID="PXWizard1" runat="server" Width="530" Height="260" DataMember="MassCopySettings"
			SkinID="Flat" >
			<NextCommand Target="ds" Command="wCopyNext" />
			<SaveCommand Target="ds" Command="wCopySave" />
			<Pages>
				<px:PXWizardPage>
					<Template>
						<px:PXFormView ID="formPanel" runat="server" SkinID="Transparent" DataMember="MassCopySettings" DefaultControlID="prCustPriceClassID"  MarkRequired="Dynamic" >
							<Template>
								<px:PXLayoutRule ID="PXLayoutRule1" runat="server" LabelsWidth="SM" ControlSize="XM"
									StartColumn="True" ColumnSpan="2" />
								<px:PXLayoutRule ID="PXLayoutRule7" runat="server" ControlSize="S" Merge="true" />
								<px:PXLabel runat="server" ID="lblStep1" Width="500px" Style="font-weight: bold;
									text-align: right">Step 1 of 3</px:PXLabel>
								<px:PXLayoutRule ID="PXLayoutRule5" runat="server" ColumnSpan="1" />
								<px:PXLayoutRule ID="PXLayoutRule11" runat="server" StartColumn="True" ControlSize="M"
									LabelsWidth="S" />
								<px:PXLabel runat="server" ID="lblDetails1" Width="500px" Height="35px" Style="font-style: italic;
									overflow: visible; padding-left: 10px">This wizard allows you to copy prices of the selected items to a different price list or to define promotional prices.</px:PXLabel>
								<px:PXTextEdit ID="edLabel" DataField="Label" runat="server" Style="font-style: italic;
									overflow: visible; padding-left: 10px" SkinID="Label" Width="500px" Height="20px" SuppressLabel="True" Enabled="False" />
								<px:PXLabel runat="server" ID="PXLabel3" Width="500px" Height="20px" Style="font-style: italic;
									overflow: visible; padding-left: 10px">Select how the prices should be copied.</px:PXLabel>
								<px:PXPanel ID="pnl2" runat="server" RenderStyle="Simple" Style="padding-left: 10px">
									<px:PXLayoutRule ID="PXLayoutRule4" runat="server" LabelsWidth="SM" ControlSize="M"
										StartColumn="True" ColumnSpan="2" />
									<px:PXGroupBox DataField="PriceOption" CommitChanges="true" runat="server" RenderStyle="Simple" ID="Radio1">
										<Template>
											<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
											<px:PXRadioButton ID="rdPriceClass" Text="Price Class" runat="server" Value="P" Checked="true" CommitChanges="true"  />
											<px:PXPanel ID="PXPanel1" runat="server" RenderStyle="Simple" Style="padding-left: 50px">
												<px:PXLayoutRule ID="PXLayoutRule15" runat="server" StartColumn="True" LabelsWidth="SM"
													ControlSize="XM" />
												<px:PXSelector ID="prCustPriceClassID" runat="server" CommitChanges="True" DataField="CustPriceClassID" />
											</px:PXPanel>
											<px:PXRadioButton ID="rdCustomer" Text="Customer" runat="server" Value="C" CommitChanges="true"  />
											<px:PXPanel ID="PXPanel3" runat="server" RenderStyle="Simple" Style="padding-left: 50px">
												<px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="SM"
													ControlSize="XM" />
												<px:PXSelector ID="edCustomerID" runat="server" DataField="CustomerID" />
											</px:PXPanel>
											<px:PXCheckBox ID="chkOverrideExisting" runat="server" DataField="OverrideExisting" SuppressLabel="True" AlignLeft="true" Style="padding-left: 4px" />
										</Template>
									</px:PXGroupBox>
								</px:PXPanel>
							</Template>
						</px:PXFormView>
					</Template>
				</px:PXWizardPage>
				<px:PXWizardPage>
					<Template>
						<px:PXFormView ID="formPanel" runat="server" SkinID="Transparent" DataMember="MassCopySettings" MarkRequired="Dynamic">
							<Template>
								<px:PXLayoutRule ID="PXLayoutRule1" runat="server" LabelsWidth="SM" ControlSize="XM"
									StartColumn="True" ColumnSpan="2" />
								<px:PXLayoutRule ID="PXLayoutRule7" runat="server" ControlSize="S" Merge="true" />
								<px:PXLabel runat="server" ID="lblStep2" Width="500px" Style="font-weight: bold;
									text-align: right">Step 2 of 3</px:PXLabel>
								<px:PXLayoutRule ID="PXLayoutRule5" runat="server" ColumnSpan="1" />
								<px:PXLayoutRule ID="PXLayoutRule11" runat="server" StartColumn="True" ControlSize="M"
									LabelsWidth="S" />
								<px:PXLabel runat="server" ID="lblDetails2" Width="500px" Height="35px" Style="font-style: italic;
									overflow: visible; padding-left: 10px">Specify pending prices as a percentage of original prices.</px:PXLabel>
								<px:PXPanel ID="PXPanel1" runat="server" RenderStyle="Simple" Style="padding-left: 10px">
									<px:PXLayoutRule ID="PXLayoutRule15" runat="server" StartColumn="True" LabelsWidth="SM"
										ControlSize="M" />
									<px:PXLayoutRule runat="server" Merge="true" />
									<px:PXNumberEdit ID="edCorrectionPercent" DataField="CorrectionPercent" runat="server" />
									<px:PXCheckBox ID="chkUsePendingPrice" DataField="UsePendingPrice" runat="server" SuppressLabel="true" />
									<px:PXLayoutRule runat="server" Merge="false" />
									<px:PXNumberEdit runat="server" ID="edRounding" DataField="Rounding" />
								</px:PXPanel>
								<%--<px:PXLabel runat="server" ID="PXLabel2" Width="500px" Height="35px" Style="font-style: italic;
									overflow: visible; padding-left: 10px"></px:PXLabel>--%>
								<px:PXTextEdit runat="server" ID="edLabelCury" DataField="LabelCury" TextMode="MultiLine" Style="font-style: italic;
									overflow: visible; padding-left: 10px; background-color:#E5E9EE; resize: none; color:black" SuppressLabel="true" Enabled="false" AllowEdit="false" SkinID="none" Width="500px">
									<Padding Bottom="0px" />
									<Border> 
										<Bottom Width="0px" />
										<Top Width="0px" />
										<Left Width="0px" />
										<Right Width="0px" />
									</Border>
								</px:PXTextEdit>
								<px:PXPanel ID="PXPanel2" runat="server" RenderStyle="Simple" Style="padding-left: 10px">
									<px:PXLayoutRule ID="PXLayoutRule10" runat="server" StartColumn="True" LabelsWidth="SM"
										ControlSize="M" />
									<px:PXSelector ID="edCuryID" runat="server" DataField="CuryID" LabelsWidth="SM"
										ControlSize="M" CommitChanges="true" />
									<px:PXSelector ID="edRateTypeID" runat="server" DataField="RateTypeID" LabelsWidth="SM"
										ControlSize="M" CommitChanges="true" />
                                    <px:PXDateTimeEdit CommitChanges="True" ID="edCurrencyDate" runat="server" DataField="CurrencyDate" LabelsWidth="SM" ControlSize="M"/>
									<px:PXNumberEdit ID="edCustomRate" DataField="CustomRate" runat="server"/>
								</px:PXPanel>
							</Template>
						</px:PXFormView>
					</Template>
				</px:PXWizardPage>
				<px:PXWizardPage>
					<Template>
						<px:PXFormView ID="formPanel" runat="server" SkinID="Transparent" DataMember="MassCopySettings">
							<Template>
								<px:PXLayoutRule ID="PXLayoutRule1" runat="server" LabelsWidth="SM" ControlSize="XM"
									StartColumn="True" ColumnSpan="2" />
								<px:PXLayoutRule ID="PXLayoutRule7" runat="server" ControlSize="S" Merge="true" />
								<px:PXLabel runat="server" ID="lblStep3" Width="500" Style="font-weight: bold;
									text-align: right">Step 3 of 3</px:PXLabel>
								<px:PXLayoutRule ID="PXLayoutRule5" runat="server" ColumnSpan="1" />
								<px:PXLayoutRule ID="PXLayoutRule11" runat="server" StartColumn="True" ControlSize="M"
									LabelsWidth="S" />
								<px:PXLabel runat="server" ID="lblDetails3" Width="500" Height="35px" Style="font-style: italic;
									overflow: visible; padding-left: 10px">Enter the date when the pending prices should become effective. If you are defining promotional prices, enter the expiration date.
								</px:PXLabel>
								<px:PXPanel ID="pnl2" runat="server" Caption="Test" RenderStyle="Simple" Style="padding-left: 10px">
									<px:PXLayoutRule ID="PXLayoutRule15" runat="server" StartColumn="True" LabelsWidth="M"
										ControlSize="M" />
									<px:PXDateTimeEdit runat="server" ID="edEffectiveDate" DataField="EffectiveDate" />
									<px:PXDateTimeEdit runat="server" ID="edExpirationDate" DataField="ExpirationDate" />
								</px:PXPanel>
							</Template>
						</px:PXFormView>
					</Template>
				</px:PXWizardPage>
			</Pages>
		</px:PXWizard>
    </px:PXSmartPanel>
	<%--<px:PXSmartPanel ID="PXSmartPanel2" runat="server" CommandSourceID="ds"
		Width="435" Caption="Calculate Pending Prices Wizard" LoadOnDemand="True" ShowAfterLoad="true"
        AutoCallBack-Enabled="true" AutoCallBack-Target="form" AutoCallBack-Command="Refresh"
        CallBackMode-CommitChanges="True" CallBackMode-PostData="Page">--%>
	<px:PXSmartPanel ID="PXSmartPanel2" runat="server" Width="435px" Caption="Calculate Pending Prices Wizard" 
		CaptionVisible="True" Key="MassCalcSettings" LoadOnDemand="True" ShowAfterLoad="true" AutoCallBack-Enabled="true" 
		AutoCallBack-Target="form" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True" CallBackMode-PostData="Page">
        <px:PXWizard ID="PXWizard2" runat="server" Width="530" Height="240" DataMember="MassCalcSettings"
			SkinID="Flat" >
			<NextCommand Target="ds" Command="wCalcNext" />
			<SaveCommand Target="ds" Command="wCalcSave" />
			<Pages>
				<px:PXWizardPage>
					<Template>
						<px:PXFormView ID="formPanel" runat="server" SkinID="Transparent" DataMember="MassCalcSettings">
							<Template>
								<px:PXLayoutRule ID="PXLayoutRule1" runat="server" LabelsWidth="SM" ControlSize="XM"
									StartColumn="True" ColumnSpan="2" />
								<px:PXLayoutRule ID="PXLayoutRule7" runat="server" ControlSize="S" Merge="true" />
								<px:PXLabel runat="server" ID="lblStep1" Width="500px" Style="font-weight: bold;
									text-align: right">Step 1 of 3</px:PXLabel>
								<px:PXLayoutRule ID="PXLayoutRule5" runat="server" ColumnSpan="1" />
								<px:PXLayoutRule ID="PXLayoutRule11" runat="server" StartColumn="True" ControlSize="M"
									LabelsWidth="S" />
								<px:PXLabel runat="server" ID="lblDetails1" Width="500px" Height="20px" Style="font-style: italic;
									overflow: visible; padding-left: 10px">This wizard allows you to calculate pending prices for the selected items.</px:PXLabel>
								<px:PXTextEdit ID="edLabel" DataField="Label" runat="server" Style="font-style: italic;
									overflow: visible; padding-left: 10px" SkinID="Label" Width="500px" Height="20px" SuppressLabel="True" Enabled="False" />
								<px:PXLabel runat="server" ID="PXLabel3" Width="500px" Height="20px" Style="font-style: italic;
									overflow: visible; padding-left: 10px">Select the basis for calculating the new pending prices:</px:PXLabel>
								<px:PXPanel ID="pnl2" runat="server" RenderStyle="Simple" Style="padding-left: 10px">
									<px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="True" SuppressLabel="True"  />
									<px:PXGroupBox RenderStyle="Simple" ID="MassUpdateGroupBox" runat="server" DataField="PriceBasis" Caption="Price Basis">
										<Template>
											<px:PXLayoutRule ID="PXLayoutRule9" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
											<px:PXRadioButton ID="rdbLastCost" runat="server" Text="Last Cost + Markup %" Value="L" />
											<px:PXRadioButton ID="rdbStdCost" runat="server" Text="Avg/Std. Cost + Markup %" Value="S" />
											<px:PXRadioButton ID="rdbCurrentPrice" runat="server" Text="Current Price" Value="P" Checked="True" />
											<px:PXRadioButton ID="rdbRecPrice" runat="server" Text="Recommended Price" Value="R" />
											<px:PXRadioButton ID="PXRadioButton1" runat="server" Text="Pending Price" Value="N" />
										</Template>
									</px:PXGroupBox>
								</px:PXPanel>
							</Template>
						</px:PXFormView>
					</Template>
				</px:PXWizardPage>
				<px:PXWizardPage>
					<Template>
						<px:PXFormView ID="formPanel" runat="server" SkinID="Transparent" DataMember="MassCalcSettings">
							<Template>
								<px:PXLayoutRule ID="PXLayoutRule1" runat="server" LabelsWidth="SM" ControlSize="XM"
									StartColumn="True" ColumnSpan="2" />
								<px:PXLayoutRule ID="PXLayoutRule7" runat="server" ControlSize="S" Merge="true" />
								<px:PXLabel runat="server" ID="lblStep2" Width="500px" Style="font-weight: bold;
									text-align: right">Step 2 of 3</px:PXLabel>
								<px:PXLayoutRule ID="PXLayoutRule5" runat="server" ColumnSpan="1" />
								<px:PXLayoutRule ID="PXLayoutRule11" runat="server" StartColumn="True" ControlSize="M"
									LabelsWidth="S" />
								<px:PXLabel runat="server" ID="lblDetails2" Width="500px" Height="35px" Style="font-style: italic;
									overflow: visible; padding-left: 10px">Specify new prices as a percentage of the basis.</px:PXLabel>
								<px:PXPanel ID="PXPanel1" runat="server" RenderStyle="Simple" Style="padding-left: 10px">
									<px:PXLayoutRule ID="PXLayoutRule15" runat="server" StartColumn="True" LabelsWidth="SM"
										ControlSize="M" />
									<px:PXNumberEdit ID="edCorrectionPercent" DataField="CorrectionPercent" runat="server" />
									<px:PXNumberEdit runat="server" ID="edRounding" DataField="Rounding" />
									<px:PXCheckBox runat="server" ID="edupdateOnZero" DataField="updateOnZero" />
								</px:PXPanel>
							</Template>
						</px:PXFormView>
					</Template>
				</px:PXWizardPage>
				<px:PXWizardPage>
					<Template>
						<px:PXFormView ID="formPanel" runat="server" SkinID="Transparent" DataMember="MassCalcSettings">
							<Template>
								<px:PXLayoutRule ID="PXLayoutRule1" runat="server" LabelsWidth="SM" ControlSize="XM"
									StartColumn="True" ColumnSpan="2" />
								<px:PXLayoutRule ID="PXLayoutRule7" runat="server" ControlSize="S" Merge="true" />
								<px:PXLabel runat="server" ID="lblStep3" Width="500" Style="font-weight: bold;
									text-align: right">Step 3 of 3</px:PXLabel>
								<px:PXLayoutRule ID="PXLayoutRule5" runat="server" ColumnSpan="1" />
								<px:PXLayoutRule ID="PXLayoutRule11" runat="server" StartColumn="True" ControlSize="M"
									LabelsWidth="S" />
								<px:PXLabel runat="server" ID="lblDetails3" Width="500" Height="35px" Style="font-style: italic;
									overflow: visible; padding-left: 10px">Enter the date when the pending prices become effective.
								</px:PXLabel>
								<px:PXPanel ID="pnl2" runat="server" Caption="Test" RenderStyle="Simple" Style="padding-left: 10px">
									<px:PXLayoutRule ID="PXLayoutRule15" runat="server" StartColumn="True" LabelsWidth="SM"
										ControlSize="M" />
									<px:PXDateTimeEdit runat="server" ID="edEffectiveDate" DataField="EffectiveDate" />
									<px:PXDateTimeEdit runat="server" ID="edExpirationDate" DataField="ExpirationDate" />
								</px:PXPanel>
							</Template>
						</px:PXFormView>
					</Template>
				</px:PXWizardPage>
			</Pages>
		</px:PXWizard>
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
