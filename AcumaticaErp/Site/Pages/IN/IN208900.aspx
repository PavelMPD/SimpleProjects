<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false"
	CodeFile="IN208900.aspx.cs" Inherits="Page_IN208900" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Width="100%" TypeName="PX.Objects.IN.INPIClassMaint" Visible="True"
		TabIndex="1" PrimaryView="Classes">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand Name="addLocation" CommitChanges="True" Visible="false" />
			<px:PXDSCallbackCommand Name="addItem" CommitChanges="True" Visible="false" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" DataMember="Classes"
		Caption="Physical Inventory Type Summary" DefaultControlID="edPIClassID">
		<Template>
			<px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="M" />
			<px:PXSelector ID="edPIClassID" runat="server" DataField="PIClassID">
				<AutoCallBack Enabled="true" Command="Cancel" Target="ds" />
			</px:PXSelector>
			<px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
			<px:PXDropDown ID="edMethod" runat="server" DataField="Method">
				<AutoCallBack Enabled="true" Command="Save" Target="form" />
			</px:PXDropDown>
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" DataSourceID="ds" Height="261px" Width="100%" DataMember="CurrentClass">
		<Items>
			<px:PXTabItem Text="Inventory Item Selection" VisibleExp='DataItem.Method==I'>
				<Template>
					<px:PXPanel runat="server" ID="pnlInvItemSel">
						<px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="M" StartColumn="True" />
						<px:PXDropDown ID="edSelectedMethod" runat="server" AllowNull="False" DataField="SelectedMethod">
							<AutoCallBack Command="Save" Target="tab" />
						</px:PXDropDown>
						<px:PXNumberEdit ID="edRandomItemsLimit" runat="server" DataField="RandomItemsLimit" ValueType="Int16" />
						<px:PXNumberEdit ID="edLastCountPeriod" runat="server" DataField="LastCountPeriod" ValueType="Int16" />
					</px:PXPanel>
					<px:PXGrid ID="gridItems" runat="server" DataSourceID="ds" Height="150px" Width="100%" Caption="Items"
						SkinID="DetailsInTab" AllowPaging="True">
						<ActionBar>
							<CustomItems>
								<px:PXToolBarButton Text="Add Item">
									<AutoCallBack Command="addItem" Target="ds" />
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
						<Levels>
							<px:PXGridLevel DataMember="Items" DataKeyNames="PIClassID,InventoryID">
								<RowTemplate>
									<px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="M" StartColumn="True" />
									<px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" HintField="descr" Width="225px" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="PIClassID" Visible="False" Width="120px" />
									<px:PXGridColumn DataField="InventoryID" />
									<px:PXGridColumn DataField="InventoryItem__Descr" Label="InventoryItem-Description" Width="200px" />
									<px:PXGridColumn AllowNull="False" DataField="InventoryItem__ItemStatus" DefValueText="AC" RenderEditorText="True" />
									<px:PXGridColumn DataField="InventoryItem__ItemClassID" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="ABC Code Selection" VisibleExp='DataItem.Method==A'>
				<Template>
					<px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="M" StartColumn="True" />
					<px:PXSelector ID="edABCCodeID" runat="server" DataField="ABCCodeID" HintField="descr" />
					<px:PXCheckBox ID="chkByFrequency" runat="server" DataField="ByABCFrequency" Text="By Frequency">
						<AutoCallBack Command="Save" Target="tab" />
					</px:PXCheckBox>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Movement Class Selection" VisibleExp='DataItem.Method==M'>
				<Template>
					<px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="M" StartColumn="True" />
					<px:PXSelector ID="edMovementClassID" runat="server" DataField="MovementClassID" />
					<px:PXCheckBox ID="chkByMovementClassFrequency" runat="server" DataField="ByMovementClassFrequency" Text="By Frequency">
						<AutoCallBack Command="Save" Target="tab" />
					</px:PXCheckBox>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="PI Cycle Selection" VisibleExp='DataItem.Method==Y'>
				<Template>
					<px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="M" StartColumn="True" />
					<px:PXSelector ID="edCycleID" runat="server" DataField="CycleID" />
					<px:PXCheckBox ID="chkByCycleFrequency" runat="server" DataField="ByCycleFrequency">
						<AutoCallBack Command="Save" Target="tab" />
					</px:PXCheckBox>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Item Class Selection" VisibleExp='DataItem.Method==C'>
				<Template>
					<px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="M" StartColumn="True" />
					<px:PXSelector ID="edItemClassID" runat="server" DataField="ItemClassID" />
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Warehouse/Location Selection">
				<Template>
					<px:PXPanel runat="server" ID="pnlWarehouseSel">
						<px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="M" StartColumn="True" />
						<px:PXSegmentMask ID="edSiteID" runat="server" DataField="SiteID">
							<AutoCallBack Command="Save" Target="tab" />
							<GridProperties FastFilterFields="Descr">
								<Layout ColumnsMenu="False" />
							</GridProperties>
						</px:PXSegmentMask>
					</px:PXPanel>
					<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="200px" Width="100%" Caption="Locations"
						SkinID="DetailsInTab">
						<ActionBar>
							<CustomItems>
								<px:PXToolBarButton Text="Add Location">
									<AutoCallBack Command="addLocation" Target="ds" />
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
						<Levels>
							<px:PXGridLevel DataMember="Locations" DataKeyNames="PIClassID,LocationID">
								<RowTemplate>
									<px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="M" StartColumn="True" />
									<px:PXSegmentMask ID="edLocationID" runat="server" DataField="LocationID" AutoRefresh="True" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="LocationID" />
									<px:PXGridColumn DataField="INLocation__Descr" Width="200px" />
									<px:PXGridColumn AllowNull="False" DataField="INLocation__PickPriority" DataType="Int16" DefValueText="1"
										TextAlign="Right" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Assignment Order">
				<Template>
					<px:PXPanel ID="pnlNumberAssignmentOrder" runat="server" Caption="Line and Tag Number Assignment Order"
						RenderStyle="Fieldset">
						<px:PXLayoutRule runat="server" LabelsWidth="S" ControlSize="S" StartColumn="True" />
						<px:PXLabel ID="PXLabel1" runat="server" Width="75px"></px:PXLabel>
						<px:PXLayoutRule runat="server" LabelsWidth="XXS" ControlSize="M" StartColumn="True" />
						<px:PXDropDown ID="edNAO1" runat="server" DataField="NAO1">
							<AutoCallBack Command="Save" Target="form" />
						</px:PXDropDown>
						<px:PXDropDown ID="edNAO2" runat="server" DataField="NAO2">
							<AutoCallBack Command="Save" Target="form" />
						</px:PXDropDown>
						<px:PXDropDown ID="edNAO3" runat="server" DataField="NAO3">
							<AutoCallBack Command="Save" Target="form" />
						</px:PXDropDown>
						<px:PXDropDown ID="edNAO4" runat="server" DataField="NAO4">
							<AutoCallBack Command="Save" Target="form" />
						</px:PXDropDown>
					</px:PXPanel>
					<px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="M" />
					<px:PXNumberEdit ID="edBlankLines" runat="server" DataField="BlankLines">
					</px:PXNumberEdit>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Enabled="true" Container="Window" />
	</px:PXTab>
	<px:PXSmartPanel ID="PanelLF" runat="server" DesignView="Content" Width="300px" Caption="Add by Filter"
		Key="LocationFilter" AutoCallBack-Enabled="True" AutoCallBack-Target="optform" AutoCallBack-Command="Refresh">
		<px:PXFormView ID="optform" runat="server" Height="92px" Width="100%" CaptionVisible="False" DataMember="LocationFilter"
			DataSourceID="ds" DefaultControlID="" SkinID="Transparent">
			<Activity Height="" HighlightColor="" SelectedColor="" Width="" />
			<Template>
				<px:PXLabel ID="lblStartLocationID" runat="server" EnableClientScript="False" Encode="True" Style="z-index: 100;
					position: absolute; left: 12px; top: 11px;">Start Location:</px:PXLabel>
				<px:PXSegmentMask ID="edStartLocationID" runat="server" DataField="StartLocationID" HintField="descr"
					HintLabelID="lblStartLocationIDH" LabelID="lblStartLocationID" Style="z-index: 101; position: absolute;
					left: 93px; top: 11px;" TabIndex="10" Width="81px" AutoRefresh="true">
					<Items>
						<px:PXMaskItem EditMask="AlphaNumeric" Length="10" Separator="-" TextCase="Upper" />
					</Items>
				</px:PXSegmentMask>
				<px:PXLabel ID="lblStartLocationIDH" runat="server" EnableClientScript="False" Encode="True" Style="z-index: 102;
					position: absolute; left: 183px; top: 11px;"></px:PXLabel>
				<px:PXLabel ID="lblEndLocationID" runat="server" EnableClientScript="False" Encode="True" Style="z-index: 103;
					position: absolute; left: 12px; top: 38px;">End Location:</px:PXLabel>
				<px:PXSegmentMask ID="edEndLocationID" runat="server" DataField="EndLocationID" HintField="descr" HintLabelID="lblEndLocationIDH"
					LabelID="lblEndLocationID" Style="z-index: 104; position: absolute; left: 93px; top: 38px;" TabIndex="11"
					Width="81px" AutoRefresh="true">
					<Items>
						<px:PXMaskItem EditMask="AlphaNumeric" Length="10" Separator="-" TextCase="Upper" />
					</Items>
				</px:PXSegmentMask>
				<px:PXLabel ID="lblEndLocationIDH" runat="server" EnableClientScript="False" Encode="True" Style="z-index: 105;
					position: absolute; left: 183px; top: 38px;"></px:PXLabel>
			</Template>
		</px:PXFormView>
		<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton7" runat="server" Text="Add" DialogResult="OK" />
			<px:PXButton ID="PXButton8" runat="server" DialogResult="Cancel" Text="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXSmartPanel ID="PanelIF" runat="server" DesignView="Content" Width="400px" Caption="Add by Filter"
		Key="InventoryFilter" AutoCallBack-Enabled="True" AutoCallBack-Target="ifilter" AutoCallBack-Command="Refresh">
		<px:PXFormView ID="ifilter" runat="server" Height="92px" Width="100%" CaptionVisible="False" DataMember="InventoryFilter"
			DataSourceID="ds" DefaultControlID="" SkinID="Transparent">
			<Activity Height="" HighlightColor="" SelectedColor="" Width="" />
			<Template>
				<px:PXLabel ID="lblStartInventoryIDH" runat="server" EnableClientScript="False" Encode="True" Style="z-index: 100;
					position: absolute; left: 228px; top: 11px;"></px:PXLabel>
				<px:PXSegmentMask ID="edStartInventoryID" runat="server" DataField="StartInventoryID" HintField="descr"
					HintLabelID="lblStartInventoryIDH" LabelID="lblStartInventoryID" Style="z-index: 101; position: absolute;
					left: 111px; top: 11px; width: 108px;" TabIndex="10">
					<Items>
						<px:PXMaskItem EditMask="AlphaNumeric" Length="35" Separator="-" TextCase="Upper" />
					</Items>
				</px:PXSegmentMask>
				<px:PXLabel ID="lblStartInventoryID" runat="server" EnableClientScript="False" Encode="True" Style="z-index: 102;
					position: absolute; left: 12px; top: 11px;">Start Inventory ID:</px:PXLabel>
				<px:PXLabel ID="lblEndInventoryID" runat="server" EnableClientScript="False" Encode="True" Style="z-index: 103;
					position: absolute; left: 12px; top: 38px;">End Inventory ID:</px:PXLabel>
				<px:PXSegmentMask ID="edEndInventoryID" runat="server" DataField="EndInventoryID" HintField="descr" HintLabelID="lblEndInventoryIDH"
					LabelID="lblEndInventoryID" Style="z-index: 104; position: absolute; left: 111px; top: 38px; width: 108px;"
					TabIndex="11">
					<Items>
						<px:PXMaskItem EditMask="AlphaNumeric" Length="35" Separator="-" TextCase="Upper" />
					</Items>
				</px:PXSegmentMask>
				<px:PXLabel ID="lblEndInventoryIDH" runat="server" EnableClientScript="False" Encode="True" Style="z-index: 105;
					position: absolute; left: 228px; top: 38px;"></px:PXLabel>
			</Template>
		</px:PXFormView>
		<px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton1" runat="server" Text="Add" DialogResult="OK" />
			<px:PXButton ID="PXButton2" runat="server" DialogResult="Cancel" Text="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>
</asp:Content>
