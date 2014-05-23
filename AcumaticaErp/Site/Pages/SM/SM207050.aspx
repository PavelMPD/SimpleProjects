<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SM207050.aspx.cs" Inherits="Page_SM207050" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="EntityDescriptions"
		TypeName="PX.Api.Soap.Entity.EntityMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
		</CallbackCommands>
		<DataTrees>
			<px:PXTreeDataMember TreeView="CurrentEntityTree" TreeKeys="Key" />
			<px:PXTreeDataMember TreeView="CurrentEntityActionsTree" TreeKeys="Key" />
			<px:PXTreeDataMember TreeView="SiteMap" TreeKeys="NodeID" />
		</DataTrees>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100"
		Width="100%" DataMember="EntityDescriptions" TabIndex="100">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM"
				ControlSize="M" />
			<px:PXSelector runat="server" DataField="GateVersion" ID="edGateVersion" AutoRefresh="True" CommitChanges="True">
			</px:PXSelector>
			<px:PXSelector runat="server" DataField="InterfaceName" ID="edInterfaceName" AutoRefresh="True" CommitChanges="True">
			</px:PXSelector>
			<px:PXSelector runat="server" DataField="ObjectName" ID="edObjectName" AutoRefresh="True" CommitChanges="True">
			</px:PXSelector>
			<px:PXCheckBox runat="server" Text="Active" DataField="Active" ID="edActive" CommitChanges="True">
			</px:PXCheckBox>
			<px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="SM"
				ControlSize="M" />
			<px:PXSelector runat="server" DataField="BaseObjectId" ID="edBaseObjectName" CommitChanges="True">
			</px:PXSelector>
			<px:PXDropDown runat="server" DataField="ObjectType" ID="edObjectType" CommitChanges="True">
			</px:PXDropDown>

			<px:PXTreeSelector CommitChanges="True" ID="edScreenID" runat="server" DataField="ScreenID"
				MinDropWidth="413" PopulateOnDemand="True" ShowRootNode="False" TreeDataMember="SiteMap"
				TreeDataSourceID="ds">
				<DataBindings>
					<px:PXTreeItemBinding DataMember="SiteMap" ImageUrlField="Icon" TextField="Title"
						ValueField="ScreenID" />
				</DataBindings>
			</px:PXTreeSelector>

			<px:PXSelector runat="server" DataField="DefaultActionId" ID="edDefaultAction" CommitChanges="True">
			</px:PXSelector>
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" TabIndex="200" Height="100%" Width="100%">
		<AutoSize Enabled="True" MinHeight="250" MinWidth="100" Container="Window" />
		<Items>
			<px:PXTabItem Text="Fields">
				<Template>
					<px:PXSplitContainer ID="splitFields" runat="server">
						<AutoSize Enabled="true" Container="Parent" />
						<Template1>
							<px:PXTreeView ID="objectTree" runat="server" DataSourceID="ds" Height="180px" PopulateOnDemand="True" ExpandDepth="2" ShowRootNode="False"
								DataMember="CurrentEntityTree" AllowCollapse="False" SelectFirstNode="True">
								<AutoCallBack Target="fieldsGrid" Command="Refresh" Enabled="True" />
								<DataBindings>
									<px:PXTreeItemBinding DataMember="CurrentEntityTree" TextField="Title" ValueField="Key" ImageUrlField="Icon" />
								</DataBindings>
								<AutoSize Enabled="True" />
							</px:PXTreeView>
						</Template1>
						<Template2>
							<px:PXGrid ID="fieldsGrid" runat="server" DataSourceID="ds" Height="150px" CaptionVisible="False"
								Width="100%" AutoAdjustColumns="True" SkinID="Details" AdjustPageSize="Auto">
								<AutoSize Enabled="True" />
								<Levels>
									<px:PXGridLevel DataMember="Fields">
										<RowTemplate>
											<px:PXTextEdit ID="edFieldName" runat="server" DataField="FieldName" />
											<px:PXSelector ID="edFieldType" runat="server" DataField="FieldType" />
											<px:PXCheckBox ID="edPopulateByDefault" runat="server" DataField="PopulateByDefault" />
											<px:PXSelector ID="edMappedObject" runat="server" DataField="MappedObject" />
											<px:PXSelector ID="edMappedField" runat="server" DataField="MappedField" />
										</RowTemplate>
										<Columns>
											<px:PXGridColumn DataField="FieldName" Label="Field Name" />
											<px:PXGridColumn DataField="FieldType" Label="Field Type" />
											<px:PXGridColumn DataField="PopulateByDefault" Label="Populate By Default" TextAlign="Center" Type="CheckBox" />
											<px:PXGridColumn DataField="MappedObject" Label="Mapped Object" />
											<px:PXGridColumn DataField="MappedField" Label="Mapped Field" />
										</Columns>
									</px:PXGridLevel>
								</Levels>
								<Parameters>
									<px:PXControlParam ControlID="objectTree" Name="Key" PropertyName="SelectedValue" />
								</Parameters>
							</px:PXGrid>
						</Template2>
					</px:PXSplitContainer>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Actions">
				<Template>
					<px:PXSplitContainer ID="splitActions" runat="server">
						<AutoSize Enabled="true" Container="Parent" />
						<Template1>
							<px:PXTreeView ID="actionsTree" runat="server" DataSourceID="ds" Height="180px" PopulateOnDemand="True" ExpandDepth="2" ShowRootNode="False"
								DataMember="CurrentEntityActionsTree" AllowCollapse="False">
								<AutoCallBack Target="actionForm" Command="Refresh" Enabled="True" />
								<DataBindings>
									<px:PXTreeItemBinding DataMember="CurrentEntityActionsTree" TextField="Title" ValueField="Key" ImageUrlField="Icon" />
								</DataBindings>
								<AutoSize Enabled="True" />
							</px:PXTreeView>
						</Template1>
						<Template2>
							<px:PXFormView ID="actionForm" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" DataMember="CurrentAction" TabIndex="100">
								<Template>
									<px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartRow="True" LabelsWidth="SM" ControlSize="M" ColumnWidth="100%" />
									<px:PXTextEdit runat="server" DataField="ActionName" ID="edActionName" />
									<px:PXCheckBox runat="server" Text="Active" DataField="Active" ID="edActionActive" />
									<px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartRow="True" SuppressLabel="True" />
									<px:PXGrid ID="actionsGrid" runat="server" DataSourceID="ds" Height="150px" CaptionVisible="False"
										Width="100%" AutoAdjustColumns="True" SkinID="Details" AdjustPageSize="Auto">
										<AutoSize Enabled="True" />
										<Levels>
											<px:PXGridLevel DataMember="CurrentActionParameters">
												<RowTemplate>
													<px:PXTextEdit ID="edParameterName" runat="server" DataField="ParameterName" />
													<px:PXSelector ID="edParameterType" runat="server" DataField="ParameterType" />
													<px:PXSelector ID="edParameterMappedObject" runat="server" DataField="MappedObject" />
													<px:PXSelector ID="edParameterMappedField" runat="server" DataField="MappedField" />
												</RowTemplate>
												<Columns>
													<px:PXGridColumn DataField="ParameterName" Label="Field Name" />
													<px:PXGridColumn DataField="ParameterType" Label="Field Type" />
													<px:PXGridColumn DataField="MappedObject" Label="Mapped Object" />
													<px:PXGridColumn DataField="MappedField" Label="Mapped Field" />
												</Columns>
											</px:PXGridLevel>
										</Levels>
										<Parameters>
											<px:PXControlParam ControlID="actionsTree" Name="Key" PropertyName="SelectedValue" />
										</Parameters>
									</px:PXGrid>

									<px:PXGrid ID="actionEntityFieldsGrid" runat="server" DataSourceID="ds" Height="150px" CaptionVisible="False"
										Width="100%" AutoAdjustColumns="True" SkinID="Details" AdjustPageSize="Auto">
										<Mode AllowAddNew="False" AllowDelete="False" />
										<AutoSize Enabled="True" />
										<Levels>
											<px:PXGridLevel DataMember="CurrentActionEntityFields">
												<RowTemplate>
													<px:PXTextEdit ID="edActionFieldName" runat="server" DataField="FieldName" />
													<px:PXSelector ID="edActionFieldType" runat="server" DataField="FieldType" />
													<px:PXCheckBox ID="edActionPopulateByDefault" runat="server" DataField="PopulateByDefault" />
													<px:PXSelector ID="edActionMappedObject" runat="server" DataField="MappedObject" />
													<px:PXSelector ID="edActionMappedField" runat="server" DataField="MappedField" />
												</RowTemplate>
												<Columns>
													<px:PXGridColumn DataField="FieldName" Label="Field Name" />
													<px:PXGridColumn DataField="FieldType" Label="Field Type" />
													<px:PXGridColumn DataField="PopulateByDefault" Label="Populate By Default" TextAlign="Center" Type="CheckBox" />
													<px:PXGridColumn DataField="MappedObject" Label="Mapped Object" />
													<px:PXGridColumn DataField="MappedField" Label="Mapped Field" />
												</Columns>
											</px:PXGridLevel>
										</Levels>
										<Parameters>
											<px:PXControlParam ControlID="actionsTree" Name="Key" PropertyName="SelectedValue" />
										</Parameters>
									</px:PXGrid>
								</Template>
								<Parameters>
									<px:PXControlParam ControlID="actionsTree" Name="Key" PropertyName="SelectedValue" />
								</Parameters>
								<AutoSize Enabled="True" />
							</px:PXFormView>
						</Template2>
					</px:PXSplitContainer>
				</Template>
			</px:PXTabItem>
		</Items>
	</px:PXTab>
</asp:Content>
