<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="EP404000.aspx.cs" Inherits="Pages_EP404000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<script type="text/javascript">
		function refreshTasksAndEvents(sender, args)
		{
			var top = window.top;
			if (top != window && top.MainFrame != null) top.MainFrame.refreshEventsInfo();
		}
	</script>

	<px:PXDataSource ID="ds" runat="server" AutoCallBack="True" Visible="True" Width="100%"
		PrimaryView="Filter" TypeName="PX.Objects.EP.EPTaskEnq" PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
			<px:PXDSCallbackCommand DependOnGrid="gridTasks" Name="Tasks_ViewDetails" Visible="False" />
			<px:PXDSCallbackCommand DependOnGrid="gridTasks" Name="viewEntity" Visible="False" />
			<px:PXDSCallbackCommand DependOnGrid="gridTasks" Name="viewOwner" Visible="False" />
			<px:PXDSCallbackCommand Name="createNew" Visible="False"
				RepaintControls="All" />
			<px:PXDSCallbackCommand DependOnGrid="gridTasks" Name="cancelActivity" Visible="False"
				RepaintControls="All" />
			<px:PXDSCallbackCommand DependOnGrid="gridTasks" Name="complete" Visible="False"
				RepaintControls="All" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="Filter" Caption="Selection"
        Style="z-index: 100" Width="100%">
		<Template>
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" Merge="True" ControlSize="XM"/>
            <px:PXSelector AutoRefresh="True" CommitChanges="True" ID="edOwnerID" runat="server" DataField="OwnerID" />
            <px:PXCheckBox CommitChanges="True" ID="chkMyOwner" runat="server" Checked="True" DataField="MyOwner" />
            <px:PXLayoutRule ID="PXLayoutRule3" runat="server" Merge="False" />
            <px:PXLayoutRule ID="PXLayoutRule4" runat="server" Merge="True" ControlSize="XM"/>
            <px:PXSelector AutoRefresh="True" CommitChanges="True" ID="edWorkGroupID" runat="server" DataField="WorkGroupID" />
            <px:PXCheckBox CommitChanges="True" ID="chkMyWorkGroup" runat="server" DataField="MyWorkGroup" />
			<px:PXLayoutRule ID="PXLayoutRule2" runat="server" Merge="False" StartColumn="True" LabelsWidth="S"
				ControlSize="SM" SuppressLabel="true" />
			<px:PXCheckBox ID="chbEscalated" runat="server" DataField="IsEscalated" TabIndex="1">
				<AutoCallBack Command="Save" Enabled="true" Target="form" />
			</px:PXCheckBox>
			<px:PXCheckBox ID="chbFollowUp" runat="server" DataField="IsFollowUp" TabIndex="2">
				<AutoCallBack Command="Save" Enabled="true" Target="form" />
			</px:PXCheckBox>
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="server">
	<px:PXGrid ID="gridTasks" runat="server" DataSourceID="ds" Width="100%" ActionsPosition="Top" Caption="Tasks"
		OnRowDataBound="grid_RowDataBound" FilesField="NoteFiles" AllowPaging="true" AllowSearch="true" BlankFilterHeader="All Tasks"
		MatrixMode="true" SkinID="Inquire" AdjustPageSize="Auto" FastFilterFields="Subject" RestrictFields="True">
		<ClientEvents AfterRefresh="refreshTasksAndEvents" />
		<Levels>
			<px:PXGridLevel DataMember="Tasks">
				<Columns>
					<px:PXGridColumn DataField="IsCompleteIcon" Width="21px" AllowShowHide="False" Label="Complete Icon"
						AllowResize="False" ForceExport="True" />
					<px:PXGridColumn DataField="PriorityIcon" Width="21px" AllowShowHide="False" Label="Priority Icon"
						AllowResize="False" SortField="Priority" ForceExport="True" />
					<px:PXGridColumn DataField="ReminderIcon" Width="21px" AllowShowHide="False" Label="Reminder Icon"
						AllowResize="False" ForceExport="True" />
					<px:PXGridColumn DataField="TaskID" Width="50px" />
					<px:PXGridColumn DataField="Subject" Width="400px" LinkCommand="Tasks_ViewDetails" />
					<px:PXGridColumn AllowNull="False" DataField="Priority" Width="50px" />
					<px:PXGridColumn AllowNull="False" DataField="UIStatus" Width="80px" />
					<px:PXGridColumn DataField="PercentCompletion" Width="90px" />
					<px:PXGridColumn DataField="StartDate" DisplayFormat="d" Width="120px" />
					<px:PXGridColumn DataField="EndDate" DisplayFormat="d" Width="120px" />
					<px:PXGridColumn DataField="CategoryID" />
					<px:PXGridColumn DataField="GroupID" Width="150px" SyncVisible="False"
						SyncVisibility="False" Visible="False" />
					<px:PXGridColumn DataField="Owner" Width="150px" LinkCommand="viewOwner" SyncVisible="False"
						SyncVisibility="False" Visible="False" DisplayMode="Text" />
					<px:PXGridColumn DataField="CreatedByID_Creator_Username" Width="80px" SyncVisible="False"
						SyncVisibility="False" Visible="False" />
					<px:PXGridColumn DataField="Source" Width="100px" SyncVisible="False" SyncVisibility="False"
						Visible="False" />
					<px:PXGridColumn DataField="Source_Description" Width="150px" SyncVisible="False"
						SyncVisibility="False" Visible="True" LinkCommand="viewEntity" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<ActionBar DefaultAction="viewDetail" PagerActionsText="true" PagerVisible="False">
			<CustomItems>
				<px:PXToolBarButton Text="View Details" Key="viewDetail" Visible="False">
					<ActionBar GroupIndex="0" />
					<AutoCallBack Target="ds" Command="Tasks_ViewDetails" />
					<PopupCommand Target="gridTasks" Command="Refresh" />
					<Images Normal="main@Inquiry" />
				</px:PXToolBarButton>
				<px:PXToolBarButton Key="cmdcreateNew" DisplayStyle="Image">
					<AutoCallBack Target="ds" Command="createNew" />
					<PopupCommand Target="gridTasks" Command="Refresh" />
					<Images Normal="main@AddNew" />
				</px:PXToolBarButton>
				<px:PXToolBarButton Key="cmdcomplete">
					<AutoCallBack Target="ds" Command="complete" />
					<PopupCommand Target="gridTasks" Command="Refresh" />
				</px:PXToolBarButton>
				<px:PXToolBarButton Key="cmdcancelActivity">
					<AutoCallBack Target="ds" Command="cancelActivity" />
					<PopupCommand Target="gridTasks" Command="Refresh" />
				</px:PXToolBarButton>
			</CustomItems>
			<Actions>
				<AddNew Enabled="False" />
				<Delete Enabled="False" />
				<EditRecord Enabled="False" />
				<PageFirst Enabled="true" />
			</Actions>
		</ActionBar>
		<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXGrid>
</asp:Content>
