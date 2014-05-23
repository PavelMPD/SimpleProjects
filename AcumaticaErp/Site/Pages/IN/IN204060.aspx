<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" ValidateRequest="false"
	CodeFile="IN204060.aspx.cs" Inherits="Page_IN204060" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" Visible="True" TypeName="PX.Objects.IN.INCategoryMaint" PrimaryView="Items">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="Cancel"/>
			<px:PXDSCallbackCommand Name="Left" Visible="false" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="Right" Visible="false" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="Down" Visible="false" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="Up" Visible="false" CommitChanges="True" />
		</CallbackCommands>
		<DataTrees>
			<px:PXTreeDataMember TreeView="Folders" TreeKeys="CategoryID" />
		</DataTrees>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="300">
		<AutoSize Enabled="true" Container="Window" />
		<Template1>
			<px:PXTreeView ID="tree" runat="server" DataSourceID="ds" Height="180px" PopulateOnDemand="True"
				ShowRootNode="False" ExpandDepth="1" AllowCollapse="true" Caption="Categories" AutoRepaint="True">
				<ToolBarItems>
					<px:PXToolBarButton Tooltip="Move to external node">
						<Images Normal="main@ArrowLeft" />
						<AutoCallBack Command="Left" Enabled="True" Target="ds" />
					</px:PXToolBarButton>
					<px:PXToolBarButton Tooltip="Move to internal node">
						<Images Normal="main@ArrowRight" />
						<AutoCallBack Command="Right" Enabled="True" Target="ds" />
					</px:PXToolBarButton>
				</ToolBarItems>
				<AutoCallBack Target="grid" Command="Refresh" Enabled="True" />
				<DataBindings>
					<px:PXTreeItemBinding DataMember="Folders" TextField="Description" ValueField="CategoryID" />
				</DataBindings>
				<AutoSize Enabled="True" />
			</px:PXTreeView>
		</Template1>
		<Template2>
			<px:PXSplitContainer runat="server" ID="sp2" SplitterPosition="300" SkinID="Horizontal" Height="500px">
				<AutoSize Enabled="true" />
				<Template1>
					<px:PXGrid ID="grid" runat="server" Height="200px" Width="100%" Style="z-index: 100; position: relative;"
						DataSourceID="ds" AllowSearch="True" ActionsPosition="Top" SyncPosition="true" Caption="List of Sub Categories"
						SkinID="Details" KeepPosition="true" CaptionVisible="True">
						<Levels>
							<px:PXGridLevel DataMember="Items">
								<Columns>
									<px:PXGridColumn DataField="CategoryID" DataType="Int32" TextAlign="Right" Visible="False" AllowShowHide="False">
										<Header Text="CategoryID" />
									</px:PXGridColumn>
									<px:PXGridColumn DataField="CategoryCD" DataType="string" Width="100px">
										<Header Text="CategoryCD" />
									</px:PXGridColumn>
									<px:PXGridColumn DataField="Description" Width="250px">
										<Header Text="Description" />
									</px:PXGridColumn>
								</Columns>
								<RowTemplate>
									<px:PXLabel ID="lblDescription" runat="server" Style="z-index: 100; left: 9px; position: absolute; top: 9px">Description :</px:PXLabel>
									<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" LabelID="lblDescription" Style="z-index: 101; left: 126px; position: absolute; top: 9px"
										TabIndex="10" Width="297px" />
								</RowTemplate>
							</px:PXGridLevel>
						</Levels>
						<Parameters>
							<px:PXControlParam ControlID="tree" Name="CategoryID" PropertyName="SelectedValue" />
						</Parameters>
						<AutoCallBack Target="gridMembers" Command="Refresh" />
						<OnChangeCommand Target="tree" Command="Refresh" />
						<AutoSize Enabled="True" />
						<ActionBar>
							<CustomItems>
								<px:PXToolBarButton Text="Up" Tooltip="Move Node Up">
									<AutoCallBack Command="Up" Enabled="True" Target="ds" />
									<Images Normal="main@ArrowUp" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Text="Down" Tooltip="Move Node Down">
									<AutoCallBack Command="Down" Enabled="True" Target="ds" />
									<Images Normal="main@ArrowDown" />
								</px:PXToolBarButton>
							</CustomItems>
							<Actions>
								<Search Enabled="False" />
								<EditRecord Enabled="False" />
								<NoteShow Enabled="False" />
								<FilterShow Enabled="False" />
								<FilterSet Enabled="False" />
								<ExportExcel Enabled="False" />
							</Actions>
						</ActionBar>
					</px:PXGrid>
				</Template1>
				<Template2>
					<px:PXGrid ID="gridMembers" runat="server" DataSourceID="ds" ActionsPosition="Top" Height="200px" Width="100%"
						Caption="Category Members" SkinID="Details" CaptionVisible="true" >
						<AutoSize Enabled="True" />
						<Levels>
							<px:PXGridLevel DataMember="Members">
								<RowTemplate>
									<px:PXLabel ID="lblCategoryID" runat="server" Style="z-index: 100; position: absolute; left: 9px; top: 9px;">Category ID:</px:PXLabel>
									<px:PXSelector ID="edCategoryID" runat="server" DataField="CategoryID" DataMember="_INCategory_" HintField="description"
										HintLabelID="lblCategoryIDH" LabelID="lblCategoryID" Style="z-index: 101; position: absolute; left: 126px; top: 9px;"
										TabIndex="10" Width="108px">
									</px:PXSelector>
									<px:PXLabel ID="lblCategoryIDH" runat="server" Style="z-index: 102; position: absolute; left: 243px; top: 9px;"></px:PXLabel>
									<px:PXLabel ID="lblInventoryID" runat="server" Style="z-index: 103; position: absolute; left: 9px; top: 36px;">Inventory ID:</px:PXLabel>
									<px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" DataMember="_InventoryItem_AccessInfo.userName_"
										HintField="descr" HintLabelID="lblInventoryIDH" LabelID="lblInventoryID" Style="z-index: 104; position: absolute; left: 126px; top: 36px;"
										TabIndex="15" Width="81px">
										<Items>
											<px:PXMaskItem EditMask="AlphaNumeric" Length="10" Separator="-" TextCase="Upper" />
										</Items>
									</px:PXSegmentMask>
									<px:PXLabel ID="lblInventoryIDH" runat="server" Style="z-index: 105; position: absolute; left: 216px; top: 36px;"></px:PXLabel>
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="InventoryID" AutoCallBack="true">
									</px:PXGridColumn>
									<px:PXGridColumn DataField="InventoryItem__Descr" Width="200px">
									</px:PXGridColumn>
									<px:PXGridColumn DataField="InventoryItem__ItemClassID">
									</px:PXGridColumn>
									<px:PXGridColumn AllowNull="False" DataField="InventoryItem__ItemStatus" RenderEditorText="True">
									</px:PXGridColumn>
								</Columns>
								<Layout FormViewHeight=""></Layout>
							</px:PXGridLevel>
						</Levels>
						<ActionBar>
							<Actions>
								<Search Enabled="False" />
								<EditRecord Enabled="False" />
								<NoteShow Enabled="False" />
								<FilterShow Enabled="False" />
								<FilterSet Enabled="False" />
								<ExportExcel Enabled="False" />
							</Actions>
						</ActionBar>
						<Parameters>
							<px:PXControlParam ControlID="grid" Name="CategoryID" PropertyName="DataValues[&quot;CategoryID&quot;]"
								Type="Int32" />
						</Parameters>
					</px:PXGrid>
				</Template2>
			</px:PXSplitContainer>
		</Template2>
	</px:PXSplitContainer>
</asp:Content>
