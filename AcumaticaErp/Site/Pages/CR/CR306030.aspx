<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="CR306030.aspx.cs" Inherits="Page_CR306030"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Events"
		TypeName="PX.Objects.EP.EPEventMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" Visible="False" />
			<px:PXDSCallbackCommand Name="NewTask" Visible="False" />
			<px:PXDSCallbackCommand Name="NewEvent" Visible="False" />
			<px:PXDSCallbackCommand Name="Cancel" PopupVisible="true" />
			<px:PXDSCallbackCommand Name="Delete" PopupVisible="true" ClosePopup="true" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="SaveClose" Visible="False" PopupVisible="True"
				ClosePopup="True" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" PopupVisible="True" />
			<px:PXDSCallbackCommand Name="complete" StartNewGroup="true" PopupVisible="true"
				ClosePopup="true" />
			<px:PXDSCallbackCommand Name="Action" PopupVisible="True" StartNewGroup="true" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="completeAndFollowUp" Visible="false" ClosePopup="true" />
			<px:PXDSCallbackCommand Name="cancelActivity" ClosePopup="true" />
			<px:PXDSCallbackCommand Name="acceptInvitation" StartNewGroup="true" PopupVisible="true" />
			<px:PXDSCallbackCommand Name="rejectInvitation" PopupVisible="true" />
			<px:PXDSCallbackCommand Name="gotoParentActivity" StartNewGroup="true" Visible="false" />
			<px:PXDSCallbackCommand Name="gotoEntity" PostData="Page" Visible="false" />
			<px:PXDSCallbackCommand Name="exportCard" Visible="false" />
			<px:PXDSCallbackCommand Name="SendCard" Visible="false" />
			<px:PXDSCallbackCommand Name="SendInvitations" Visible="False" RepaintControlsIDs="grdAttendees" />
			<px:PXDSCallbackCommand Name="SendPersonalInvitation" DependOnGrid="grdAttendees"
				Visible="false" RepaintControlsIDs="grdAttendees" />
			<px:PXDSCallbackCommand Name="NewActivity" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="NewMailActivity" Visible="False" CommitChanges="True"
				PopupCommand="Cancel" PopupCommandTarget="ds" />
			<px:PXDSCallbackCommand Name="ViewActivity" Visible="False" CommitChanges="True"
				DependOnGrid="gridReferencedActivities" />
			<px:PXDSCallbackCommand Name="Users$AddResource" Visible="False" />
			<px:PXDSCallbackCommand Name="Users$DeleteResource" Visible="False" />
			<px:PXDSCallbackCommand Name="Users$DetailsResource" Visible="False" />
			<px:PXDSCallbackCommand Name="Users$UpdateResource" Visible="False" />
			<px:PXDSCallbackCommand Name="Users$NavigateToResource" Visible="False" />
			<px:PXDSCallbackCommand Name="Users$PreviousRegion" Visible="False" />
			<px:PXDSCallbackCommand Name="Users$NextRegion" Visible="False" />
            <px:PXDSCallbackCommand Name="Events$Select_RefNote" Visible="False" />
            <px:PXDSCallbackCommand Name="Events$Navigate_ByRefNote" Visible="False" />
            
		</CallbackCommands>
	</px:PXDataSource>
	<pxa:PXEntityViewer ID="viewer" runat="server" Target="ds" Command="SetViewed$CREvent" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXTab ID="tab" runat="server" Height="450px" Style="z-index: 100" Width="100%"
		DataSourceID="ds" DataMember="Events" NoteIndicator="True" FilesIndicator="True"
		DefaultControlID="edSubject">
		<Items>
			<px:PXTabItem Text="Details">
				
				<Template>
					<px:PXPanel ID="PXPanel1" runat="server" RenderStyle="Simple" ContentLayout-OuterSpacing="Around">
						<px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartColumn="True" LabelsWidth="S"
							ControlSize="M" />
						<px:PXLayoutRule ID="PXLayoutRule5" runat="server" ColumnSpan="2" />
						<px:PXTextEdit ID="edSubject" runat="server" DataField="Subject" />
						<px:PXLayoutRule ID="PXLayoutRule6" runat="server" ColumnSpan="2" />
						<px:PXTextEdit ID="edLocation" runat="server" DataField="Location" />
						<px:PXLayoutRule ID="PXLayoutRule8" runat="server" Merge="True" />
						<px:PXLabel ID="lblStartDate" runat="server">Start Date</px:PXLabel>
						<px:PXDateTimeEdit CommitChanges="True" ID="edStartDate_Date" runat="server" DataField="StartDate_Date" SuppressLabel="True"/>
						<px:PXDateTimeEdit CommitChanges="True" ID="edStartDate_Time" runat="server" DataField="StartDate_Time"
							TimeMode="true" SuppressLabel="True" Width="84" />
						<px:PXCheckBox CommitChanges="True" ID="chkAllDay" runat="server" Checked="True"
							DataField="AllDay" />
						<px:PXLayoutRule ID="PXLayoutRule9" runat="server" />
						<px:PXLayoutRule ID="PXLayoutRule10" runat="server" Merge="True" />
						<px:PXDateTimeEdit CommitChanges="True" ID="edEndDate_Date" runat="server" DataField="EndDate_Date" />
						<px:PXDateTimeEdit CommitChanges="True" ID="edEndDate_Time" runat="server" DataField="EndDate_Time"
							TimeMode="true" SuppressLabel="True" Width="84" />
						<px:PXLayoutRule ID="PXLayoutRule11" runat="server" />
						<px:PXLayoutRule ID="PXLayoutRule7" runat="server" Merge="True" />
						<px:PXSelector ID="edCategoryID" runat="server" DataField="CategoryID" ValueField="Description" />
						<px:PXCheckBox ID="edIsPrivate" runat="server" DataField="IsPrivate" />
						<px:PXLayoutRule ID="PXLayoutRule2" runat="server" />
						<px:PXSelector ID="edShowAsID" runat="server" DataField="ShowAsID" />
						<px:PXCheckBox CommitChanges="True" ID="chkIsReminderOn" runat="server" DataField="IsReminderOn" />
						<px:PXLayoutRule ID="PXLayoutRule12" runat="server" Merge="True" />
						<px:PXDateTimeEdit CommitChanges="True" ID="edReminderDate_Date" runat="server" DataField="ReminderDate_Date" />
						<px:PXDateTimeEdit CommitChanges="True" ID="edReminderDate_Time" runat="server" DataField="ReminderDate_Time"
							TimeMode="true" SuppressLabel="True" Width="84" />
						<px:PXLayoutRule ID="PXLayoutRule13" runat="server" />
						<pxa:PXRefNoteSelector ID="edSource" runat="server" DataField="Source_Description" NoteIDDataField="RefNoteID"
							MaxValue="0" MinValue="0" ValueType="Int64">
							<EditButton CommandName="Events$Navigate_ByRefNote" CommandSourceID="ds" />
							<LookupButton CommandName="Events$Select_RefNote" CommandSourceID="ds" />
							<LookupPanel DataMember="Events$RefNoteView" DataSourceID="ds" TypeDataField="Type"
								IDDataField="NoteID" />
						</pxa:PXRefNoteSelector>
						<px:PXLayoutRule ID="PXLayoutRule14" runat="server" StartColumn="True" LabelsWidth="S"
							ControlSize="SM" />
						<px:PXDropDown ID="edPriority" runat="server" AllowNull="False" DataField="Priority" />
						<px:PXDropDown CommitChanges="True" ID="edStatus" runat="server" AllowNull="False"
							DataField="UIStatus" />
                        <px:PXTimeSpan TimeMode="True" ID="edTimeSpent" runat="server" DataField="TimeSpent" InputMask="hh:mm" Enabled="False" MaxHours="99"/>
                        <px:PXTimeSpan TimeMode="True" ID="edOvertimeSpent" runat="server" DataField="OvertimeSpent" InputMask="hh:mm" Enabled="False" MaxHours="99"/>
                        <px:PXTimeSpan TimeMode="True" ID="edTimeBillable1" runat="server" DataField="TimeBillable" InputMask="hh:mm" Enabled="False" MaxHours="99"/>
                        <px:PXTimeSpan TimeMode="True" ID="edOvertimeBillable" runat="server" DataField="OvertimeBillable" InputMask="hh:mm" Enabled="False" MaxHours="99"/>
					</px:PXPanel>
					<pxa:PXRichTextEdit ID="edBody" runat="server" DataField="Body" Height="135px" Style="border-width: 0px; border-top-width: 1px;" Width="100%">
						<AutoSize Enabled="true" MinHeight="135" />
					</pxa:PXRichTextEdit>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Related Activities">
				<Template>
					<px:PXGrid ID="gridReferencedActivities" runat="server" DataSourceID="ds" Height="323px"
						Style="z-index: 100; left: 0px; position: absolute; top: 0px" Width="100%" AllowSearch="True"
						AllowPaging="true" AdjustPageSize="Auto" BorderWidth="0" SkinID="Details" MatrixMode="true">
						<ActionBar DefaultAction="cmdViewActivity">
							<Actions>
								<AddNew Enabled="False" />
								<EditRecord Enabled="false" />
							</Actions>
							<CustomItems>
								<px:PXToolBarButton Text="Add Email" Key="cmdAddEmail">
									<AutoCallBack Command="NewMailActivity" Target="ds" />
									<PopupCommand Command="Refresh" Target="gridReferencedActivities" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Text="Add Activity" Key="cmdAddActivity">
									<AutoCallBack Command="NewActivity" Target="ds" />
									<PopupCommand Command="Refresh" Target="gridReferencedActivities" />
									<ActionBar />
								</px:PXToolBarButton>
								<px:PXToolBarButton Key="cmdViewActivity" Visible="false">
									<ActionBar MenuVisible="false" />
									<AutoCallBack Command="ViewActivity" Target="ds" />
									<PopupCommand Command="Refresh" Target="gridReferencedActivities" />
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
						<Levels>
							<px:PXGridLevel DataMember="ChildActivities">
								<RowTemplate>
									<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="M"
										ControlSize="XM" />									
                                    <px:PXTextEdit ID="edSubject" runat="server" DataField="Subject" />
									<px:PXDropDown ID="edStatus" runat="server" AllowNull="False" DataField="UIStatus" />
									<px:PXDateTimeEdit ID="edStartDate" runat="server" DataField="StartDate" DisplayFormat="g"
										EditFormat="g" />
									<px:PXTimeSpan TimeMode="true" ID="edTimeSpent" runat="server" DataField="TimeSpent" InputMask="hh:mm" MaxHours="99" />
									<px:PXDateTimeEdit TimeMode="true" ID="edTimeBillable" runat="server" DataField="TimeBillable"
										Enabled="False" />
									<px:PXDateTimeEdit TimeMode="true" ID="edOvertimeBillable" runat="server" DataField="OvertimeBillable"
										Enabled="False" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="ClassInfo" Width="81px" />
									<px:PXGridColumn DataField="IsBillable" TextAlign="Center" Type="CheckBox" Width="60px"
										AutoCallBack="true" />
									<px:PXGridColumn DataField="Subject" Width="300px" />
									<px:PXGridColumn AllowNull="False" DataField="UIStatus" Width="60px" RenderEditorText="true" />
									<px:PXGridColumn DataField="StartDate" DisplayFormat="g" Width="125px" AutoCallBack="true" />
									<px:PXGridColumn DataField="TimeSpent" Width="100px" AutoCallBack="true" RenderEditorText="True" />
									<px:PXGridColumn DataField="OvertimeSpent" Width="100px" />
									<px:PXGridColumn AllowUpdate="False" DataField="TimeBillable" Width="100px" />
									<px:PXGridColumn AllowUpdate="False" DataField="OvertimeBillable" Width="100px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
						<Mode AllowAddNew="False" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Attendees">
				<Template>
					<px:PXGrid ID="grdAttendees" runat="server" DataSourceID="ds" Width="100%" ActionsPosition="Top"
						AutoAdjustColumns="true" AllowPaging="true" BorderWidth="0" AllowSearch="True"
						AdjustPageSize="Auto" SkinID="Details" MatrixMode="true">
						<Levels>
							<px:PXGridLevel DataMember="AllAttendeesAndOwner">
								<RowTemplate>
									<px:PXSelector ID="edName" runat="server" DataField="Name" ValueField="PKID" TextField="DisplayName"
										TextMode="Editable" AutoRefresh="true" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="Name" Width="200px" AutoCallBack="true" TextField="DisplayName" RenderEditorText="true" />
									<px:PXGridColumn DataField="Email" Width="200px" />
									<px:PXGridColumn DataField="Comment" Width="200px" />
									<px:PXGridColumn DataField="Invitation" Width="80px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<ActionBar>
							<CustomItems>
								<px:PXToolBarButton Text="Invite" Tooltip="Send personal invitation" CommandName="SendPersonalInvitation"
									CommandSourceID="ds" />
								<px:PXToolBarButton Text="Invite All Attendees" Tooltip="Send invitation to all attendees" CommandName="SendInvitations"
									CommandSourceID="ds" />
							</CustomItems>
						</ActionBar>
						<AutoSize Enabled="true" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<%--<px:PXTabItem Text="Schedule">
				<Template>
					<pxa:PXSchedulePanel ID="pnlSchedule" runat="server" Height="200px" Width="100%"
						DataKeyNames="pkID" DataMember="AttendeeResources" DataSourceID="ds" RepaintControlsIDs="form"
						AddPanel="NewAttendeeCurrent" NotDeletableDataMember="CurrentOwner" ConfirmDelete="Are you sure you want to delete selected attendee?">
						<AutoSize Enabled="true" />
					</pxa:PXSchedulePanel>
				</Template>
				<AutoCallBack Command="NavigateToTarget" Target="pnlSchedule" />
			</px:PXTabItem>--%>
		</Items>
		<AutoSize Container="Window" Enabled="True" />
	</px:PXTab>
	<px:PXSmartPanel ID="addAttendeeDlg" runat="server" Height="72px" Width="300px" Caption="Add attendee to event"
		CaptionVisible="true" DesignView="Content" LoadOnDemand="true" Key="NewAttendeeCurrent">
		<px:PXFormView ID="frmAddAttendee" DataSourceID="ds" DataMember="NewAttendeeCurrent"
			runat="server" Style="z-index: 100" Width="99%" AutoReload="true" SkinID="Transparent">
			<Template>
				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
				<px:PXSelector ID="edNewAttendee" runat="server" DataField="PKID" TextField="fullname"
					AutoRefresh="true" />
			</Template>
			<CallbackCommands>
				<Save PostData="Page" />
			</CallbackCommands>
		</px:PXFormView>
		<px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
			<px:PXButton ID="btnSave1" runat="server" DialogResult="OK" Text="OK" />
			<px:PXButton ID="btnCancel1" runat="server" DialogResult="Cancel" Text="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXSmartPanel ID="sendCardDlg" runat="server" Height="86px" Width="281px" Caption="Send vCard by e-mail"
		CaptionVisible="true" DesignView="Content" LoadOnDemand="true" Key="SendCardSettings">
		<px:PXFormView ID="frmSendCard" DataSourceID="ds" DataMember="SendCardSettings" runat="server"
			Style="z-index: 100" Width="360px" AutoReload="true" SkinID="Transparent">
			<Template>
				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="XS" ControlSize="XM" />
				<px:PXTextEdit ID="edTargetEmail" runat="server" DataField="Email" />
			</Template>
			<CallbackCommands>
				<Save PostData="Container" />
			</CallbackCommands>
		</px:PXFormView>
		<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
			<px:PXButton ID="btnSend" runat="server" DialogResult="OK" Text="OK">
				<AutoCallBack Command="Save" Target="frmSendCard">
					<Behavior CommitChanges="true" PostData="Container" />
				</AutoCallBack>
			</px:PXButton>
			<px:PXButton ID="btnCancelSend" runat="server" DialogResult="Cancel" Text="Cancel" Style="margin-right: 24px" />
		</px:PXPanel>
	</px:PXSmartPanel>
</asp:Content>
