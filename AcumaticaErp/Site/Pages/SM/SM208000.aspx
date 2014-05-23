<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM208000.aspx.cs" Inherits="Page_SM208000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<script type="text/javascript">
		function commandResult(ds, context)
		{
			if (context.command == "Save" || context.command == "Delete")
			{
				var ds = px_all[context.id];
				var isSitemapAltered = (ds.callbackResultArg == "RefreshSitemap");
				if (isSitemapAltered) __refreshMainMenu();
		}

			// grdFilterID and grdResultsID is registered by server
			var grid = null;
			var row = null;
			if (context.command == "moveDownFilter" || context.command == "moveUpFilter")
				grid = px_all[grdFilterID];
			else if (context.command == "moveDownResults" || context.command == "moveUpResults")
				grid = px_all[grdResultsID];
			else if (context.command == "moveDownSortings" || context.command == "moveUpSortings")
				grid = px_all[grdSortsID];
			else if (context.command == "moveDownCondition" || context.command == "moveUpCondition")
				grid = px_all[grdWheresID];
			else if (context.command == "moveDownRelations" || context.command == "moveUpRelations")
				grid = px_all[grdJoinsID];
			else if (context.command == "moveDownOn" || context.command == "moveUpOn")
				grid = px_all[grdOnsID];
			else if (context.command == "moveDownGroupBy" || context.command == "moveUpGroupBy")
				grid = px_all[grdGroupByID];


			if (context.command == "moveDownFilter" || context.command == "moveDownResults" || context.command == "moveDownSortings"
				|| context.command == "moveDownCondition" || context.command == "moveDownRelations" || context.command == "moveDownOn" || context.command == "moveDownGroupBy")
				row = grid.activeRow.nextRow();
			else if (context.command == "moveUpFilter" || context.command == "moveUpResults" || context.command == "moveUpSortings"
				|| context.command == "moveUpCondition" || context.command == "moveUpRelations" || context.command == "moveUpOn" || context.command == "moveUpGroupBy")
				row = grid.activeRow.prevRow();

			if (row)
				row.activate();
		}

		function onWhereCellUpdate(sender, e)
		{
		}
	</script>
	<px:PXDataSource ID="ds" runat="server" Visible="True" PrimaryView="Designs" TypeName="PX.Data.Maintenance.GI.GenericInquiryDesigner">
		<ClientEvents CommandPerformed="commandResult" />
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand Name="viewInquiry" StartNewGroup="True" />
			<px:PXDSCallbackCommand DependOnGrid="grdFilter" Name="moveUpFilter" Visible="False" />
			<px:PXDSCallbackCommand DependOnGrid="grdFilter" Name="moveDownFilter" Visible="False" />
			<px:PXDSCallbackCommand DependOnGrid="grdResults" Name="moveUpResults" Visible="False" />
			<px:PXDSCallbackCommand DependOnGrid="grdResults" Name="moveDownResults" Visible="False" />
			<px:PXDSCallbackCommand DependOnGrid="grdSorts" Name="moveUpSortings" Visible="False" />
			<px:PXDSCallbackCommand DependOnGrid="grdSorts" Name="moveDownSortings" Visible="False" />
			<px:PXDSCallbackCommand DependOnGrid="grdWheres" Name="moveUpCondition" Visible="False" />
			<px:PXDSCallbackCommand DependOnGrid="grdWheres" Name="moveDownCondition" Visible="False" />
			<px:PXDSCallbackCommand DependOnGrid="grdJoins" Name="moveUpRelations" Visible="False" />
			<px:PXDSCallbackCommand DependOnGrid="grdJoins" Name="moveDownRelations" Visible="False" />
			<px:PXDSCallbackCommand DependOnGrid="grdOns" Name="moveUpOn" Visible="False" />
			<px:PXDSCallbackCommand DependOnGrid="grdOns" Name="moveDownOn" Visible="False" />
			<px:PXDSCallbackCommand DependOnGrid="grdGroupBy" Name="moveUpGroupBy" Visible="False" />
			<px:PXDSCallbackCommand DependOnGrid="grdGroupBy" Name="moveDownGroupBy" Visible="False" />
			<px:PXDSCallbackCommand Name="showAvailableValues" Visible="False" DependOnGrid="grdFilter" />
		</CallbackCommands>
		<DataTrees>
			<px:PXTreeDataMember TreeView="SiteMapTree" TreeKeys="NodeID" />
		</DataTrees>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXSmartPanel ID="pnlCombos" runat="server" Style="z-index: 108;"
		Width="416px" Caption="Combo Box Values" CaptionVisible="true" LoadOnDemand="true"
		Key="ValuesLabels" AutoCallBack-Enabled="true" AutoCallBack-Target="gridCombos"
		AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True" 
		CallBackMode-PostData="Page">
		<div style="padding: 5px">
			<px:PXGrid ID="gridCombos" runat="server" DataSourceID="ds" Height="243px" Style="z-index: 100"
				Width="100%" AutoAdjustColumns="True">
				<Levels>
					<px:PXGridLevel  DataMember="ValuesLabels">
						<RowTemplate>
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
							<px:PXTextEdit ID="edValue" runat="server" DataField="Value" />
							<px:PXTextEdit ID="edLabel" runat="server" DataField="Label" /></RowTemplate>
						<Columns>
							<px:PXGridColumn DataField="Value" Label="Value" Width="108px" />
							<px:PXGridColumn DataField="Label" Label="Label" Width="108px" />
						</Columns>
					</px:PXGridLevel>
				</Levels>
				<CallbackCommands>
					<Save RepaintControls="None" RepaintControlsIDs="gridCombos" />
					<FetchRow RepaintControls="None" />
				</CallbackCommands>
			</px:PXGrid>
		</div>
		<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
			<px:PXButton ID="btnCombos" runat="server" DialogResult="OK" Text="OK" />
		</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%"
		Caption="Inquiry Summary" DataMember="Designs" OnDataBound="form_DataBound" AllowCollapse="True">
		<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
			<px:PXSelector ID="edName" runat="server" DataField="Name" DataSourceID="ds" />
			<px:PXLayoutRule runat="server" Merge="True" />
			<px:PXNumberEdit Size="S" runat="server" DataField="SelectTop" ID="edTop" />
			<px:PXLabel Size="xs" ID="lblRecords" runat="server">records</px:PXLabel>
			<px:PXLayoutRule runat="server" />
			<px:PXLayoutRule runat="server" Merge="True" />
			<px:PXNumberEdit Size="S" runat="server" DataField="FilterColCount" ID="edColCount" />
			<px:PXLabel Size="xs" ID="lblColumns" runat="server" Encode="True">columns</px:PXLabel>
			<px:PXLayoutRule runat="server" />
			<px:PXNumberEdit Size="S" runat="server" DataField="PageSize" ID="edPageSize" />
			<px:PXCheckBox Size="S" runat="server" DataField="PagerStyle" ID="edPagerStyle" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM" />
			<px:PXTreeSelector ID="edScreen" runat="server" DataField="SitemapParent" PopulateOnDemand="True" 
				ShowRootNode="False" TreeDataSourceID="ds" TreeDataMember="SiteMapTree" MinDropWidth="413" CommitChanges="true">
				<DataBindings>
					<px:PXTreeItemBinding DataMember="SiteMapTree" TextField="Title" ValueField="NodeID" ImageUrlField="Icon" />
				</DataBindings>
				<AutoCallBack Command="Save" Target="form">
				</AutoCallBack>
			</px:PXTreeSelector>
			<px:PXTextEdit runat="server" DataField="SitemapTitle" ID="edSitemapTitle" />
			<px:PXSelector ID="edCreatedByID" runat="server" DataField="CreatedByID" DataSourceID="ds" />
			<px:PXDateTimeEdit runat="server" DisplayFormat="g" DataField="CreatedDateTime" ID="edCreatedDateTime" Size="SM" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Width="100%" Height="150px" DataSourceID="ds" DataMember="CurrentDesign">
		<Items>
			<px:PXTabItem Text="Tables">
				<Template>
					<px:PXGrid ID="grdTables" runat="server" DataSourceID="ds" Height="150px" SkinID="DetailsInTab"
						Width="100%">
						<Mode AllowFormEdit="True" />
						<Levels>
							<px:PXGridLevel  DataMember="Tables">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" 
										ControlSize="M" />
									<px:PXSelector ID="edName" runat="server" DataField="Name">
										<GridProperties FastFilterFields="Name">
										</GridProperties>
									</px:PXSelector>
									<px:PXTextEdit ID="edAlias" runat="server" DataField="Alias" /></RowTemplate>
											<Columns>
									<px:PXGridColumn DataField="Name" Width="250px" />
									<px:PXGridColumn DataField="Alias" Width="250px" />
									<px:PXGridColumn DataField="Number" Width="60px" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Relations" Overflow="Hidden">
				<Template>
					<px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="300" SkinID="Horizontal" Panel1MinSize="150" Panel2MinSize="150">
						<AutoSize Enabled="true"  MinHeight="250"  />
						<Template1>
							<px:PXGrid ID="grdJoins" runat="server" DataSourceID="ds" Height="100%" Style="z-index: 100" Width="100%"
								AutoAdjustColumns="True" Caption="Table Relations" AllowSearch="True" SkinID="DetailsInTab" MatrixMode="True">
								<AutoCallBack Command="Refresh" Target="grdOns" ActiveBehavior="True">
									<Behavior RepaintControlsIDs="grdOns" BlockPage="True" CommitChanges="False" />
								</AutoCallBack>
								<AutoSize Enabled="true"  MinHeight="150"  />
								<Mode AllowFormEdit="True" InitNewRow="True" />
								<Levels>
									<px:PXGridLevel DataMember="Relations">
										<RowTemplate>
											<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
											<px:PXCheckBox SuppressLabel="True" ID="chkIsActive" runat="server" Checked="True" DataField="IsActive" />
											<px:PXDropDown ID="edParentTable" runat="server" DataField="ParentTable" AutoCallBack="True" />
											<px:PXDropDown ID="edJoinType" runat="server" AllowNull="False" DataField="JoinType" SelectedIndex="2" />
											<px:PXDropDown ID="edChildTable" runat="server" DataField="ChildTable" AutoCallBack="True" />
										</RowTemplate>
										<Columns>
											<px:PXGridColumn AllowNull="False" DataField="IsActive" TextAlign="Center" Type="CheckBox" Width="50px" />
											<px:PXGridColumn DataField="ParentTable" Width="150px" Type="DropDownList" RenderEditorText="true" />
											<px:PXGridColumn AllowNull="False" DataField="JoinType" RenderEditorText="True" Width="60px" />
											<px:PXGridColumn DataField="ChildTable" Width="150px" Type="DropDownList" RenderEditorText="true" />
											<px:PXGridColumn DataField="LineNbr" />
										</Columns>
										<Layout FormViewHeight="" />
									</px:PXGridLevel>
								</Levels>
								<ActionBar>
									<CustomItems>
										<px:PXToolBarButton CommandSourceID="ds" CommandName="moveUpRelations" Tooltip="Move Row Up">
											<Images Normal="main@ArrowUp" />
										</px:PXToolBarButton>
										<px:PXToolBarButton CommandSourceID="ds" CommandName="moveDownRelations" Tooltip="Move Row Down">
											<Images Normal="main@ArrowDown" />
										</px:PXToolBarButton>
									</CustomItems>
								</ActionBar>
							</px:PXGrid>
						</Template1>
						<Template2>
							<px:PXGrid ID="grdOns" runat="server" DataSourceID="ds" Height="200px" Width="100%" AutoAdjustColumns="True"
								Caption="Data Field Links For Active Relation" SkinID="DetailsInTab" MatrixMode="true">
								<AutoSize Enabled="true"  MinHeight="150"  />
								<Levels>
									<px:PXGridLevel DataMember="JoinConditions">
										<RowTemplate>
											<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
											<px:PXDropDown ID="edOpenBrackets" runat="server" DataField="OpenBrackets" />
											<pxa:PXFormulaCombo ID="edParentField" runat="server" DataField="ParentField" EditButton="True" FieldsAutoRefresh="True"
												FieldsRootAutoRefresh="true" LastNodeName="Fields" PanelAutoRefresh="True" IsInternalVisible="false"
												IsExternalVisible="false" OnRootFieldsNeeded="edOns_RootFieldsNeeded" />
											<px:PXDropDown ID="edCondition" runat="server" AllowNull="False" DataField="Condition" />
											<pxa:PXFormulaCombo ID="edChildField" runat="server" DataField="ChildField" EditButton="True" FieldsAutoRefresh="True"
												FieldsRootAutoRefresh="true" LastNodeName="Fields" PanelAutoRefresh="True" IsInternalVisible="false"
												IsExternalVisible="false" OnRootFieldsNeeded="edOns_RootFieldsNeeded" />
											<px:PXDropDown ID="edCloseBrackets" runat="server" DataField="CloseBrackets" />
											<px:PXDropDown ID="edOperation" runat="server" AllowNull="False" DataField="Operation" />
										</RowTemplate>
										<Columns>
											<px:PXGridColumn DataField="OpenBrackets" RenderEditorText="True" Width="72px" />
											<px:PXGridColumn DataField="ParentField" Width="108px" RenderEditorText="true" />
											<px:PXGridColumn AllowNull="False" DataField="Condition" RenderEditorText="True" Width="117px" />
											<px:PXGridColumn DataField="ChildField" RenderEditorText="True" Width="108px" AutoCallBack="true" />
											<px:PXGridColumn DataField="CloseBrackets" RenderEditorText="True" Width="72px" />
											<px:PXGridColumn AllowNull="False" DataField="Operation" RenderEditorText="True" Width="36px" AutoCallBack="true" />
										</Columns>
									</px:PXGridLevel>
								</Levels>
								<Mode InitNewRow="True" AutoInsert="True" />
								<AutoSize Enabled="True"  MinHeight="150"  />
								<ActionBar>
									<CustomItems>
										<px:PXToolBarButton CommandSourceID="ds" CommandName="moveUpOn" Text="Row Up" Tooltip="Move Row Up">
											<Images Normal="main@ArrowUp" />
										</px:PXToolBarButton>
										<px:PXToolBarButton CommandSourceID="ds" CommandName="moveDownOn" Text="Row Down" Tooltip="Move Row Down">
											<Images Normal="main@ArrowDown" />
										</px:PXToolBarButton>
									</CustomItems>
								</ActionBar>
								<Parameters>
									<px:PXSyncGridParam ControlID="grdJoins" Name="SyncGrid" />
								</Parameters>
							</px:PXGrid>
						</Template2>
					</px:PXSplitContainer>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Parameters">
				<Template>
					<px:PXGrid ID="grdFilter" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100;"
						Width="100%" AutoAdjustColumns="True" SkinID="DetailsInTab" MatrixMode="true">
						<Mode InitNewRow="True" />
						<ActionBar>
							<CustomItems>
								<px:PXToolBarButton CommandSourceID="ds" CommandName="moveUpFilter" Text="Row Up"
									Tooltip="Move Row Up">
									<Images Normal="main@ArrowUp" />
								</px:PXToolBarButton>
								<px:PXToolBarButton CommandSourceID="ds" CommandName="moveDownFilter" Text="Row Down"
									Tooltip="Move Row Down">
									<Images Normal="main@ArrowDown" />
								</px:PXToolBarButton>
								<px:PXToolBarButton CommandName="showAvailableValues" CommandSourceID="ds" Text="Combo Box Values"
									Tooltip="Values available in combo box.">
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
						<Levels>
							<px:PXGridLevel  DataMember="Parameters">
								<RowTemplate>
									<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXLayoutRule ID="PXLayoutRule2" runat="server" Merge="True" />
									<px:PXCheckBox ID="chkIsActive4" runat="server" Checked="True" DataField="IsActive" />
									<px:PXCheckBox ID="chkIsExpression" runat="server" DataField="IsExpression" />
									<px:PXLayoutRule ID="PXLayoutRule3" runat="server" Merge="False" />
									<px:PXTextEdit ID="edName4" runat="server" DataField="Name" CommitChanges="true" />
									<px:PXDropDown ID="edFieldName" runat="server" DataField="FieldName" />
									<px:PXTextEdit ID="edDisplayName" runat="server" DataField="DisplayName" />
									<px:PXTextEdit ID="edAvailableValues" runat="server" DataField="AvailableValues" />
									<px:PXTextEdit ID="edDefaultValue" runat="server" DataField="DefaultValue" />
									<px:PXNumberEdit ID="edColSpan" runat="server" AllowNull="False" DataField="ColSpan" />
									<px:PXCheckBox ID="chkRequired" runat="server" DataField="Required" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="IsActive" TextAlign="Center" Type="CheckBox" Width="60px" AllowNull="False" />
									<px:PXGridColumn DataField="Required" TextAlign="Center" Type="CheckBox" Width="60px" AllowNull="False"/>
									<px:PXGridColumn DataField="Hidden" TextAlign="Center" Type="CheckBox" Width="60px" />
									<px:PXGridColumn DataField="Name" Width="150px" AutoCallBack="True" />
									<px:PXGridColumn DataField="FieldName" Type="DropDownList" Width="150px" />
									<px:PXGridColumn DataField="DisplayName" Width="150px" />
									<px:PXGridColumn DataField="IsExpression" Label="Expression" TextAlign="Center" Type="CheckBox" Width="58px" AutoCallBack="True" />
									<px:PXGridColumn DataField="AvailableValues" Width="150px" Visible="false" AllowShowHide="False" />
									<px:PXGridColumn DataField="DefaultValue" Width="100px" />
									<px:PXGridColumn AllowNull="False" DataField="ColSpan" TextAlign="Right" Width="75px" />
									<px:PXGridColumn DataField="Size" Width="65px" />
									<px:PXGridColumn DataField="LabelSize" Width="65px" />
									
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="true" MinHeight="150"  />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Conditions">
				<Template>
					<px:PXGrid ID="grdWheres" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100;"
						Width="100%" AutoAdjustColumns="True" SkinID="DetailsInTab" MatrixMode="true">
						<ClientEvents AfterCellUpdate="onWhereCellUpdate" />
						<Levels>
							<px:PXGridLevel  DataMember="Wheres">
								<Mode InitNewRow="True" />
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXCheckBox ID="chkIsActive2" runat="server" Checked="True" DataField="IsActive" />
									<px:PXDropDown ID="edOpenBrackets2" runat="server" DataField="OpenBrackets" />
									<px:PXDropDown ID="edDataFieldName" runat="server" DataField="DataFieldName" />
									<px:PXDropDown ID="edCondition2" runat="server" AllowNull="False" DataField="Condition" />
									<pxa:PXFormulaCombo ID="edValue1" runat="server" DataField="Value1" EditButton="True"
										FieldsAutoRefresh="True" FieldsRootAutoRefresh="true" LastNodeName="Parameters" 
										IsInternalVisible="false" IsExternalVisible="false" OnRootFieldsNeeded="edValue_RootFieldsNeeded" />
									<pxa:PXFormulaCombo ID="edValue2" runat="server" DataField="Value2" EditButton="True"
										FieldsAutoRefresh="True" LastNodeName="Parameters" IsInternalVisible="false"
										IsExternalVisible="false" OnRootFieldsNeeded="edValue_RootFieldsNeeded" />
									<px:PXDropDown ID="edCloseBrackets2" runat="server" DataField="CloseBrackets" />
									<px:PXDropDown ID="edOperation2" runat="server" AllowNull="False" DataField="Operation" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn AllowNull="False" DataField="IsActive" AllowSort="False" TextAlign="Center"
										Type="CheckBox" Width="50px" />
									<px:PXGridColumn DataField="OpenBrackets" RenderEditorText="True" Width="60px" AllowSort="False" />
									<px:PXGridColumn DataField="DataFieldName" Width="150px" Type="DropDownList" AutoCallBack="true"
										AllowSort="False" />
									<px:PXGridColumn AllowNull="False" DataField="Condition" RenderEditorText="True"
										Width="120px" AllowSort="False" />
									<px:PXGridColumn AutoCallBack="True" DataField="IsExpression" AllowSort="False" Label="Expression"
										TextAlign="Center" Type="CheckBox" Width="58px" />
									<px:PXGridColumn DataField="Value1" Width="150px" Key="value1" AllowSort="False" />
									<px:PXGridColumn DataField="Value2" Visible="False" Width="150px" RenderEditorText="true"
										Key="value2" AllowSort="False" />
									<px:PXGridColumn DataField="CloseBrackets" RenderEditorText="True" Width="60px" AllowSort="False" />
									<px:PXGridColumn AllowNull="False" DataField="Operation" RenderEditorText="True"
										Width="40px" AllowSort="False" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
						<ActionBar>
							<CustomItems>
								<px:PXToolBarButton CommandSourceID="ds" CommandName="moveUpCondition" Text="Row Up"
									Tooltip="Move Row Up">
									<Images Normal="main@ArrowUp" />
								</px:PXToolBarButton>
								<px:PXToolBarButton CommandSourceID="ds" CommandName="moveDownCondition" Text="Row Down"
									Tooltip="Move Row Down">
									<Images Normal="main@ArrowDown" />
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Grouping" Visible ="True">
				<Template>
					<px:PXGrid ID="grdGroupBy" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100;"
						Width="100%" AutoAdjustColumns="False" SkinID="DetailsInTab" MatrixMode="true">
						<Mode InitNewRow="True" />
						<Levels>
							<px:PXGridLevel DataMember="GroupBy">
								<Columns>
									<px:PXGridColumn AllowNull="False" DataField="IsActive" TextAlign="Center" Type="CheckBox" Width="100px" />
									<px:PXGridColumn DataField="DataFieldName" Width="300px" Type="DropDownList" RenderEditorText="true" AutoCallBack="true" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="true" MinHeight="150" />
						<ActionBar>
							<CustomItems>
								<px:PXToolBarButton CommandSourceID="ds" CommandName="moveUpGroupBy" Text="Row Up"
									Tooltip="Move Row Up">
									<Images Normal="main@ArrowUp" />
								</px:PXToolBarButton>
								<px:PXToolBarButton CommandSourceID="ds" CommandName="moveDownGroupBy" Text="Row Down"
									Tooltip="Move Row Down">
									<Images Normal="main@ArrowDown" />
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Sort Order">
				<Template>
					<px:PXGrid ID="grdSorts" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100;"
						Width="100%" AutoAdjustColumns="False" SkinID="DetailsInTab" MatrixMode="true">
						<Mode InitNewRow="True" />
						<Levels>
							<px:PXGridLevel  DataMember="Sortings">
								<RowTemplate>
									<px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXCheckBox ID="chkIsActive3" runat="server" Checked="True" DataField="IsActive" />
									<px:PXDropDown ID="edDataFieldName3" runat="server" DataField="DataFieldName" />
									<px:PXDropDown ID="edSortOrder" runat="server" AllowNull="False" DataField="SortOrder" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn AllowNull="False" DataField="IsActive" AllowSort="False" TextAlign="Center" Type="CheckBox" Width="100px" />
									<px:PXGridColumn AllowSort="False" DataField="DataFieldName" Type="DropDownList" Width="300px" />
									<px:PXGridColumn AllowNull="False" AllowSort="False" DataField="SortOrder" RenderEditorText="True"	Width="150px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="true" MinHeight="150" />
						<ActionBar>
							<CustomItems>
								<px:PXToolBarButton CommandSourceID="ds" CommandName="moveUpSortings" Text="Row Up"
									Tooltip="Move Row Up">
									<Images Normal="main@ArrowUp" />
								</px:PXToolBarButton>
								<px:PXToolBarButton CommandSourceID="ds" CommandName="moveDownSortings" Text="Row Down"
									Tooltip="Move Row Down">
									<Images Normal="main@ArrowDown" />
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Results Grid">
				<Template>
					<px:PXGrid ID="grdResults" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100;"
						Width="100%" AutoAdjustColumns="True" SkinID="DetailsInTab" MatrixMode="true">
						<Mode InitNewRow="True" />
						<ActionBar>
							<CustomItems>
								<px:PXToolBarButton CommandSourceID="ds" CommandName="moveUpResults" Text="Row Up"
									Tooltip="Move Row Up">
									<Images Normal="main@ArrowUp" />
								</px:PXToolBarButton>
								<px:PXToolBarButton CommandSourceID="ds" CommandName="moveDownResults" Text="Row Down"
									Tooltip="Move Row Down">
									<Images Normal="main@ArrowDown" />
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
						<Levels>
							<px:PXGridLevel  DataMember="Results">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXLayoutRule runat="server" Merge="True" />
									<px:PXCheckBox ID="chkIsActive5" runat="server" Checked="True" DataField="IsActive" />
									<px:PXTextEdit Size="s" ID="edCaption" runat="server" DataField="Caption" />
									<px:PXLayoutRule runat="server" Merge="False" />
									<px:PXDropDown ID="edObjectName" runat="server" DataField="ObjectName" />
									<pxa:PXFormulaCombo ID="edField5" runat="server" DataField="Field" EditButton="True"
										FieldsAutoRefresh="True" FieldsRootAutoRefresh="true" LastNodeName="Fields" IsInternalVisible="false"
										IsExternalVisible="false" OnRootFieldsNeeded="edField_RootFieldsNeeded" CommitChanges="true" >                                        
                                    </pxa:PXFormulaCombo>
									<px:PXDropDown ID="edSchemaField" runat="server" DataField="SchemaField" />
									<px:PXDropDown ID="edAggregateFunction" runat="server" DataField="AggregateFunction" />
									<px:PXCheckBox ID="chkIsVisible" runat="server" Checked="True" DataField="IsVisible" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn AllowNull="False" DataField="IsActive" TextAlign="Center" Type="CheckBox" Width="60px" />
									<px:PXGridColumn DataField="ObjectName" Width="150px" Type="DropDownList" AutoCallBack="true" />
									<px:PXGridColumn DataField="Field" Width="150px" />
									<px:PXGridColumn DataField="SchemaField" Type="DropDownList" Width="150px" />
									<px:PXGridColumn DataField="Caption" Label="Caption" Visible="False" Width="108px" />
									<px:PXGridColumn AllowNull="False" DataField="AggregateFunction" Type="DropDownList" Width="100px" />
									<px:PXGridColumn DataField="Width" TextAlign="Right" Width="65px" />									
									<px:PXGridColumn AllowNull="False" DataField="IsVisible" TextAlign="Center" Type="CheckBox"	Width="60px" />									
									<px:PXGridColumn AllowNull="False" DataField="SuppressNav" Label="Suppress Navigation" TextAlign="Center" Type="CheckBox" Width="80px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="true" MinHeight="150" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="400" />
	</px:PXTab>
</asp:Content>
