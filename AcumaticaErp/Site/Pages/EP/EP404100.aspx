<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="EP404100.aspx.cs" Inherits="Pages_EP404100"
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

	<px:PXDataSource ID="ds" runat="server" AutoCallBack="True" Visible="true" Width="100%"
		PrimaryView="Filter" TypeName="PX.Objects.EP.EPEventEnq" PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
			<px:PXDSCallbackCommand DependOnGrid="gridEvents" Name="Events_ViewDetails" Visible="False" />
			<px:PXDSCallbackCommand DependOnGrid="gridEvents" Name="viewEntity" Visible="False" />
			<px:PXDSCallbackCommand DependOnGrid="gridEvents" Name="viewOwner" Visible="False" />
			<%--<px:PXDSCallbackCommand DependOnGrid="gridEvents" Name="exportCard" Visible="False" />--%>
			<px:PXDSCallbackCommand Name="createNew" Visible="False" RepaintControls="All" />
			<px:PXDSCallbackCommand DependOnGrid="gridEvents" Name="complete" Visible="False" RepaintControls="All" />
			<px:PXDSCallbackCommand DependOnGrid="gridEvents" Name="cancelActivity" Visible="False" RepaintControls="All" />
			<%--<px:PXDSCallbackCommand Name="exportCalendar" RepaintControls="All" />--%>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="Filter" Caption="Selection"
        Style="z-index: 100" Width="100%">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" />
			<px:PXLayoutRule ID="PXLayoutRule2" runat="server" Merge="True" />
			<px:PXSelector ID="edOwnerID" runat="server" DataKeyNames="AcctCD" 
				Size="XM" DataField="OwnerID" CommitChanges="True" >
				<GridProperties>
					<Columns>
						<px:PXGridColumn DataField="AcctCD" MaxLength="64" Width="100px">
							<Header Text="User Name">
							</Header>
						</px:PXGridColumn>
						<px:PXGridColumn AllowUpdate="False" DataField="AcctName" MaxLength="10" Width="100px">
							<Header Text="Employee Name">
							</Header>
						</px:PXGridColumn>
						<px:PXGridColumn AllowUpdate="False" DataField="PositionID" MaxLength="10" Width="100px">
							<Header Text="Position ID">
							</Header>
						</px:PXGridColumn>
						<px:PXGridColumn AllowUpdate="False" DataField="DepartmentID" MaxLength="10" Width="100px">
							<Header Text="Department ID">
							</Header>
						</px:PXGridColumn>
						<px:PXGridColumn AllowUpdate="False" DataField="DefLocationID" MaxLength="2" Width="100px">
							<Header Text="Location">
							</Header>
						</px:PXGridColumn>
					</Columns>
					<Layout ColumnsMenu="False" />
				</GridProperties>
			</px:PXSelector> 
			<px:PXCheckBox ID="ckbMyOwner" runat="server" Width="40px" DataField="MyOwner" CommitChanges="True" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="server">
	<px:PXGrid ID="gridEvents" runat="server" DataSourceID="ds" Width="100%" ActionsPosition="Top"
		Caption="Events" OnRowDataBound="grid_RowDataBound" NoteField="NoteText" FilesField="NoteFiles"
		MatrixMode="true" BlankFilterHeader="All Events" AdjustPageSize="Auto" SkinID="Inquire" FastFilterFields="Subject" RestrictFields="True">
		<ClientEvents AfterRefresh="refreshTasksAndEvents" />
		<Levels>
			<px:PXGridLevel DataMember="Events">
				<Columns>
					<px:PXGridColumn DataField="IsCompleteIcon" Width="21px" AllowShowHide="False" Label="Complete Icon"
						AllowResize="False" ForceExport="True" />
					<px:PXGridColumn DataField="PriorityIcon" Width="21px" AllowShowHide="False" Label="Priority Icon"
						AllowResize="False" ForceExport="True" />
					<px:PXGridColumn DataField="ReminderIcon" Width="21px" AllowShowHide="False" Label="Reminder Icon"
						AllowResize="False" ForceExport="True" />
					<px:PXGridColumn DataField="Subject" Width="400px" LinkCommand="Events_ViewDetails" />
					<px:PXGridColumn AllowNull="False" DataField="UIStatus" Width="80px" />
					<px:PXGridColumn DataField="DayOfWeek" Width="110px" />
					<px:PXGridColumn DataField="StartDate_Date" DataType="DateTime" DisplayFormat="d" Width="130px" />
					<px:PXGridColumn DataField="StartDate_Time" DataType="DateTime" DisplayFormat="t" Width="80px" />
					<px:PXGridColumn DataField="EndDate_Time" DataType="DateTime" DisplayFormat="t" Width="80px" />
					<px:PXGridColumn DataField="Owner" Width="150px" LinkCommand="viewOwner" SyncVisible="False"
						SyncVisibility="False" Visible="False" DisplayMode="Text" />
					<px:PXGridColumn DataField="Source" Width="100px" SyncVisible="False" SyncVisibility="False"
						Visible="False" />
					<px:PXGridColumn DataField="Source_Description" Width="150px" LinkCommand="viewEntity"
						SyncVisible="False" SyncVisibility="False" Visible="True" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Parent" Enabled="True" />
		<ActionBar DefaultAction="viewDetail" PagerVisible="False">
			<CustomItems>
				<px:PXToolBarButton Key="cmdviewDetail" Visible="False">
					<ActionBar GroupIndex="0" />
					<AutoCallBack Enabled="True" Target="ds" Command="Events_ViewDetails" />
					<PopupCommand Enabled="true" Target="gridEvents" Command="Refresh" />
					<Images Normal="main@Inquiry" />
				</px:PXToolBarButton>
				<px:PXToolBarButton Key="cmdcreateNew" DisplayStyle="Image">
					<AutoCallBack Target="ds" Command="createNew" />
					<PopupCommand Target="gridEvents" Command="Refresh" />
					<Images Normal="main@AddNew" />
				</px:PXToolBarButton>
				<px:PXToolBarButton Key="cmdcomplete">
					<AutoCallBack Target="ds" Command="complete" />
					<PopupCommand Target="gridEvents" Command="Refresh" />
				</px:PXToolBarButton>
				<px:PXToolBarButton Key="cmdcancelActivity">
					<AutoCallBack Target="ds" Command="cancelActivity" />
					<PopupCommand Target="gridEvents" Command="Refresh" />
				</px:PXToolBarButton>
			</CustomItems>
			<Actions>
				<AddNew Enabled="False" />
				<Delete Enabled="False" />
				<EditRecord Enabled="False" />
				<PageFirst Enabled="true" />
			</Actions>
		</ActionBar>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
	</px:PXGrid>
</asp:Content>
