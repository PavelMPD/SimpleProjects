<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="GL504500.aspx.cs" Inherits="Page_GL504500"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter"
		TypeName="PX.Objects.GL.AllocationProcess" PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
			<px:PXDSCallbackCommand StartNewGroup="True" CommitChanges="True" Name="Process" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="ProcessAll" />
			<px:PXDSCallbackCommand Name="viewAllocation" DependOnGrid="grid" Visible="False" />
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="viewBatch" Visible="False">
			</px:PXDSCallbackCommand>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" Width="100%" Caption="Allocation Post Period"
		DataMember="Filter" DefaultControlID="edDateEntered">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="S" />
			<px:PXDateTimeEdit CommitChanges="True" ID="edDateEntered" runat="server" DataField="DateEntered" />
			<px:PXSelector CommitChanges="True" ID="edFinPeriodID" runat="server" DataField="FinPeriodID" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="150px" Style="z-index: 100; left: 0px;
		top: 0px;" Width="100%" Caption="Active Allocations" AllowPaging="True" AllowSearch="true"
		SkinID="Inquire">
		<Levels>
			<px:PXGridLevel DataMember="Allocations">
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
					<px:PXCheckBox ID="chkSelected" runat="server" DataField="Selected" />
					<px:PXSelector ID="edGLAllocationID" runat="server" DataField="GLAllocationID" Enabled="False"
						AllowEdit="True" />
					<px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" Enabled="False" />
					<px:PXTextEdit ID="edBatchNbr" runat="server" DataField="BatchNbr" />
					<px:PXDropDown ID="edStatus" runat="server" DataField="Status" Enabled="False" />
					<px:PXDropDown ID="edAllocMethod" runat="server" DataField="AllocMethod" Enabled="False" />
					<px:PXSelector ID="edAllocLedgerID" runat="server" DataField="AllocLedgerID" Enabled="False" />
					<px:PXDateTimeEdit ID="edLastRevisionOn" runat="server" DataField="LastRevisionOn"
						Enabled="False" />
					<px:PXNumberEdit ID="edSortOrder" runat="server" DataField="SortOrder" Enabled="False" /></RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True"
						AllowSort="False" AllowMove="False" Width="30px" />
					<px:PXGridColumn DataField="GLAllocationID" Width="70px" />
					<px:PXGridColumn DataField="Descr" Width="200px" />
					<px:PXGridColumn DataField="AllocMethod" RenderEditorText="True" Width="150px" />
					<px:PXGridColumn DataField="AllocLedgerID" Width="100px" />
					<px:PXGridColumn DataField="SortOrder" TextAlign="Right" Width="70px" />
					<px:PXGridColumn DataField="BatchNbr" Width="70px" />
					<px:PXGridColumn DataField="BatchPeriod" Width="70px"/>
					<px:PXGridColumn DataField="ControlTotal" TextAlign="Right" Width="100px" />
					<px:PXGridColumn DataField="Status" RenderEditorText="True" Width="150px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<Mode AllowSort="False" />
		<ActionBar>
			<CustomItems>
				<px:PXToolBarButton Text="View Allocation" CommandName="viewAllocation" CommandSourceID="ds">
				</px:PXToolBarButton>
				<px:PXToolBarButton Text="View Batch" CommandName="viewBatch" CommandSourceID="ds">
				</px:PXToolBarButton>
			</CustomItems>
		</ActionBar>
	</px:PXGrid>
</asp:Content>
