<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
	CodeFile="SM201020.aspx.cs" Inherits="Page_SM200000" Title="Access Rights Maintenance"
	ValidateRequest="false" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="EntityRoles" TypeName="PX.SM.Access"
		Visible="True">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Cancel" PostData="Page" />
			<px:PXDSCallbackCommand Name="Save" PostData="Page" CommitChanges="True" />
		</CallbackCommands>
		<DataTrees>
			<px:PXTreeDataMember TreeView="EntitiesWithLeafs" TreeKeys="NodeID,CacheName,MemberName" />
		</DataTrees>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="server">
	<px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="300">
		<AutoSize Enabled="true" Container="Window" />
		<Template1>
			<px:PXTreeView ID="tree" runat="server" DataSourceID="ds" PopulateOnDemand="True" RootNodeText="Entities"
				ShowRootNode="False" ExpandDepth="1" AllowCollapse="False" Caption="Sitemap Tree" CaptionVisible="False" >
				<AutoCallBack Target="grid" Command="Refresh" />
				<AutoSize Enabled="True" />
				<DataBindings>
					<px:PXTreeItemBinding DataMember="EntitiesWithLeafs" TextField="Text" ValueField="Path" ImageUrlField="Icon" />
				</DataBindings>
			</px:PXTreeView>
		</Template1>
		<Template2>
			<px:PXGrid ID="grid" runat="server" Height="200px" Width="100%" Style="z-index: 100; position: relative;"
				DataSourceID="ds" AllowSearch="True" ActionsPosition="None" SkinID="Details">
				<CallbackCommands>
					<Refresh CommitChanges="True" PostData="Page" RepaintControls="All" />
				</CallbackCommands>
				<Levels>
					<px:PXGridLevel DataMember="EntityRoles">
						<RowTemplate>
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
							<px:PXLayoutRule runat="server" Merge="True" />
							<px:PXLabel Size="xs" ID="lblRoleName" runat="server">Role :</px:PXLabel>
							<px:PXTextEdit SuppressLabel="True" Size="s" ID="edRoleName" runat="server" DataField="RoleName" Enabled="False" />
							<px:PXLayoutRule runat="server" Merge="False" />
							<px:PXLayoutRule runat="server" Merge="True" />
							<px:PXLabel Size="xs" ID="lblRoleDescr" runat="server">Description :</px:PXLabel>
							<px:PXTextEdit SuppressLabel="True" Size="s" ID="edRoleDescr" runat="server" DataField="RoleDescr" Enabled="False" />
							<px:PXLayoutRule runat="server" Merge="False" />
							<px:PXLayoutRule runat="server" Merge="True" />
							<px:PXLabel Size="xs" ID="lblRoleRight" runat="server">Access Rights :</px:PXLabel>
							<px:PXDropDown SuppressLabel="True" Size="s" ID="edRoleRight" runat="server" DataField="RoleRight" />
							<px:PXLayoutRule runat="server" Merge="False" />
						</RowTemplate>
						<Columns>
							<px:PXGridColumn AllowUpdate="False" DataField="ScreenID" Visible="False" AllowShowHide="False" />
							<px:PXGridColumn AllowUpdate="False" DataField="CacheName" Visible="False" AllowShowHide="False" />
							<px:PXGridColumn AllowUpdate="False" DataField="MemberName" Visible="False" AllowShowHide="False" />
							<px:PXGridColumn AllowUpdate="False" DataField="RoleName" Width="300px" />
							<px:PXGridColumn AllowUpdate="False" DataField="RoleDescr" Width="300px" />
							<px:PXGridColumn AllowNull="False" DataField="RoleRight" RenderEditorText="True">
								<ValueItems>
									<Items>
										<px:PXValueItem DisplayValue="Undefined" Value="-1" />
										<px:PXValueItem DisplayValue="Denied" Value="0" />
										<px:PXValueItem DisplayValue="Select" Value="1" />
										<px:PXValueItem DisplayValue="Update" Value="2" />
										<px:PXValueItem DisplayValue="Insert" Value="3" />
										<px:PXValueItem DisplayValue="Delete" Value="4" />
									</Items>
								</ValueItems>
							</px:PXGridColumn>
						</Columns>
						<Mode AllowAddNew="False" AllowDelete="False" />
					</px:PXGridLevel>
				</Levels>
				<Parameters>
					<px:PXControlParam ControlID="tree" Name="path" PropertyName="SelectedValue" Type="String" />
				</Parameters>
				<AutoSize Enabled="True" />
			</px:PXGrid>
		</Template2>
	</px:PXSplitContainer>
</asp:Content>
