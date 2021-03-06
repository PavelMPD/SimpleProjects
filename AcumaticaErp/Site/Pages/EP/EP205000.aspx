<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="EP205000.aspx.cs"
    Inherits="Page_EP205000" Title="Untitled Page" %>
<%@ Register TagPrefix="px" Namespace="PX.Web.UI" Assembly="PX.Web.UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=3b136cac2f602b8e" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" AutoCallBack="True" Visible="True" Width="100%" PrimaryView="AssigmentMap" TypeName="PX.Objects.EP.EPAssignmentMaint">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="Up" Visible="False" DependOnGrid="topGrid" />
            <px:PXDSCallbackCommand Name="Down" Visible="False" DependOnGrid="topGrid" />
        </CallbackCommands>
        <DataTrees>
            <px:PXTreeDataMember TreeKeys="AssignmentRouteID" TreeView="Nodes" />
            <px:PXTreeDataMember TreeView="_EPCompanyTree_Tree_" TreeKeys="WorkgroupID" />
            <px:PXTreeDataMember TreeKeys="Key" TreeView="CacheTree" />
            <px:PXTreeDataMember TreeKeys="Key" TreeView="EntityItems" />
        </DataTrees>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView runat="server" ID="mapForm" DataSourceID="ds" Width="100%" DataMember="AssigmentMap" 
        Caption="Assignment Rules Summary" NoteIndicator="True" FilesIndicator="True" ActivityIndicator="True" ActivityField="NoteActivity"
        DefaultControlID="edAssignmentMapID">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XL" />
            <px:PXSelector ID="edAssignmentMapID" runat="server" DataField="AssignmentMapID" TextField="Name" NullText="<NEW>" DataSourceID="ds" />
            <px:PXTextEdit CommitChanges="True" Size="XL" ID="edName" runat="server" DataField="Name" />
            <px:PXTreeSelector CommitChanges="True" SuppressLabel="False" ID="edEntityType" runat="server" DataField="EntityType" PopulateOnDemand="True"
                ShowRootNode="False" TreeDataSourceID="ds" TreeDataMember="CacheTree" InitialExpandLevel="0" MinDropWidth="297" MaxDropWidth="500"
                TextField="Name" Size="XL">
                <Images>
                    <ParentImages Normal="tree@Folder" Selected="tree@FolderS" />
                    <LeafImages Normal="main@Box" Selected="main@Box" />
                </Images>
                <DataBindings>
                    <px:PXTreeItemBinding DataMember="CacheTree" TextField="Name" ValueField="SubKey" ImageUrlField="Icon" />
                </DataBindings>
            </px:PXTreeSelector>
        </Template>
    </px:PXFormView>
    <px:PXSplitContainer runat="server" ID="sp0" SplitterPosition="300">
        <AutoSize Enabled="true" Container="Window" MinHeight="500" />
        <Template1>
            <px:PXTreeView ID="tree" runat="server" DataSourceID="ds" Height="500px" PopulateOnDemand="True" ShowRootNode="False"
                ExpandDepth="1" AutoRepaint="true" Caption="Tree" AllowCollapse="true">
				<ToolBarItems>
                    <px:PXToolBarButton Tooltip="Reload Tree" ImageKey="Refresh">
                        <AutoCallBack Target="tree" Command="Refresh" />
                    </px:PXToolBarButton>
				</ToolBarItems>
                <AutoCallBack Target="topGrid" Command="Refresh" ActiveBehavior="true">
                    <Behavior RepaintControlsIDs="bottomGrid" />
                </AutoCallBack>
                <AutoSize Enabled="True" MinHeight="300" />
                <DataBindings>
                    <px:PXTreeItemBinding DataMember="Nodes" TextField="Name" ValueField="AssignmentRouteID" ImageUrlField="Icon" />
                </DataBindings>
            </px:PXTreeView>
        </Template1>
        <Template2>
            <px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="300" SkinID="Horizontal" Height="700px">
                <AutoSize Enabled="true" />
                <Template1>
                    <px:PXGrid ID="topGrid" runat="server" ActionsPosition="Top" DataSourceID="ds" Width="100%" Height="350px" AllowFilter="False" 
                        Caption="Rules" SkinID="Details" KeepPosition="true"  CaptionVisible="True" TabIndex="-16236">
                        <AutoSize Enabled="True" />
                        <Levels>
                            <px:PXGridLevel DataMember="Items">
                                <Mode AllowFormEdit="true" />
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                                    <px:PXNumberEdit ID="edSequence" runat="server" DataField="Sequence" />
                                    <px:PXTextEdit ID="edName1" runat="server" DataField="Name" />
                                    <px:PXDropDown ID="edRouterType" runat="server" DataField="RouterType" />
                                    <px:PXSelector ID="edRouteID" runat="server" DataField="RouteID" TextField="Name" AutoRefresh="True" AllowEdit="True" />
                                    <px:PXTreeSelector ID="edWorkgroupID" runat="server" DataField="WorkgroupID" TreeDataMember="_EPCompanyTree_Tree_" TreeDataSourceID="ds"
                                        PopulateOnDemand="True" InitialExpandLevel="0" ShowRootNode="False">
                                        <DataBindings>
                                            <px:PXTreeItemBinding TextField="Description" ValueField="Description" />
                                        </DataBindings>
                                    </px:PXTreeSelector>
                                    <px:PXSelector ID="edOwnerID" runat="server" DataField="OwnerID" AutoRefresh="True" AllowEdit="True">
                                        <Parameters>
                                            <px:PXSyncGridParam ControlID="topGrid" />
                                            <px:PXControlParam ControlID="topGrid" Name="EPAssignmentRoute.WorkgroupID" PropertyName="DataValues[&quot;WorkgroupID&quot;]" />
                                        </Parameters>
                                    </px:PXSelector>
                                    <px:PXTreeSelector ID="edOwnerSource" runat="server" DataField="OwnerSource" TreeDataSourceID="ds" PopulateOnDemand="True"
                                        InitialExpandLevel="0" ShowRootNode="False" MinDropWidth="468" MaxDropWidth="600" AllowEditValue="True" AppendSelectedValue="True"
                                        Height="16px" AutoRefresh="True" TreeDataMember="EntityItems">
                                        <DataBindings>
                                            <px:PXTreeItemBinding TextField="Name" ValueField="Path" ImageUrlField="Icon" ToolTipField="Path" />
                                        </DataBindings>
                                        <ButtonImages Normal="main@AddNew" Hover="main@AddNew" Pushed="main@AddNew" />
                                    </px:PXTreeSelector>
                                    <px:PXCheckBox ID="chkUseWorkgroupByOwner" runat="server" DataField="UseWorkgroupByOwner" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="Sequence" TextAlign="Right" Width="52px" />
                                    <px:PXGridColumn DataField="RouterType" AutoCallBack="True" Width="72px" RenderEditorText="true" />
                                    <px:PXGridColumn DataField="Name" Width="200px" RenderEditorText="True" AutoCallBack="true" />
                                    <px:PXGridColumn DataField="RouteID" TextField="RouteID_EPAssignmentRoute_Name" TextAlign="Left" Width="108px" />
                                    <px:PXGridColumn DataField="WorkgroupID" TextAlign="Left" RenderEditorText="True" TextField="WorkgroupID_EPCompanyTree_Description"
                                        AutoCallBack="true" Width="100px" />
                                    <px:PXGridColumn DataField="OwnerID" AutoCallBack="true"  DisplayMode="Text"/>
                                    <px:PXGridColumn DataField="EPEmployee__acctName" Width="100px" />
                                    <px:PXGridColumn DataField="EPEmployee__positionID" Width="100px" />
                                    <px:PXGridColumn DataField="EPEmployee__departmentID" Width="100px" />
                                    <px:PXGridColumn DataField="OwnerSource" Width="170px" />
                                    <px:PXGridColumn DataField="UseWorkgroupByOwner" Type="CheckBox" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <Parameters>
                            <px:PXControlParam ControlID="tree" Name="parent" PropertyName="SelectedValue" />
                        </Parameters>
                        <AutoCallBack Target="bottomGrid" Command="Refresh" ActiveBehavior="True">
                            <Behavior RepaintControlsIDs="tree,formRuleType" />
                        </AutoCallBack>
                        <ActionBar>
                            <Actions>
                                <Search Enabled="False" />
                                <EditRecord Enabled="False" />
                                <NoteShow Enabled="False" />
                                <FilterShow Enabled="False" />
                                <FilterSet Enabled="False" />
                                <ExportExcel Enabled="False" />
                            </Actions>
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
                        </ActionBar>
                        <Mode HeaderClickAction="NotSet" />
                    </px:PXGrid>
                </Template1>
                <Template2>
                    <px:PXFormView ID="formRuleType" runat="server" Width="100%" DataMember="CurrentItem" DataSourceID="ds" Caption="Rules" TabIndex="-16036">
                        <Template>
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                            <px:PXDropDown CommitChanges="True" ID="edRuleType" runat="server" DataField="RuleType"/>
                        </Template>
                    </px:PXFormView>
                    <px:PXGrid ID="bottomGrid" runat="server" SkinID="Details" DataSourceID="ds" ActionsPosition="Top" Height="200px" Width="100%"
                        MatrixMode="true" Caption="Conditions" CaptionVisible="True" Style="z-index: 101;">
                        <AutoSize Enabled="True" />
                        <Levels>
                            <px:PXGridLevel DataMember="Rules">
                                <Columns>
                                    <px:PXGridColumn Type="DropDownList" DataField="Entity" AutoCallBack="True" Width="200px" />
                                    <px:PXGridColumn AutoCallBack="True" Type="DropDownList" DataField="FieldName" Width="200px" />
                                    <px:PXGridColumn AutoCallBack="True" DataField="Condition" Width="72px" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="FieldValue" Width="200px" />
                                </Columns>
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                                    <px:PXDropDown ID="edCondition" runat="server" DataField="Condition" />
                                </RowTemplate>
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
                        <CallbackCommands>
                            <Refresh PostData="Page" RepaintControlsIDs="formRuleType" />
                            <Save RepaintControls="None" RepaintControlsIDs="ds,mapForm,bottomGrid" />
                            <FetchRow RepaintControls="None" />
                        </CallbackCommands>
                        <Parameters>
                            <px:PXControlParam ControlID="topGrid" Name="routeID" PropertyName="DataValues[&quot;AssignmentRouteID&quot;]" Type="Int32" />
                        </Parameters>
                        <Mode InitNewRow="True" />
                    </px:PXGrid>
                </Template2>
            </px:PXSplitContainer>
        </Template2>
    </px:PXSplitContainer>
</asp:Content>
