<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="CR306020.aspx.cs" Inherits="Page_CR306020"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Tasks"
		TypeName="PX.Objects.CR.CRTaskMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" Visible="False" />
			<px:PXDSCallbackCommand Name="NewTask" Visible="False" />
			<px:PXDSCallbackCommand Name="NewEvent" Visible="False" />
			<px:PXDSCallbackCommand Name="Cancel" PopupVisible="true" />
			<px:PXDSCallbackCommand Name="Delete" PopupVisible="true" ClosePopup="true" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="SaveClose" Visible="False" PopupVisible="True"
				ClosePopup="True" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" PopupVisible="True" />
			<px:PXDSCallbackCommand Name="gotoParentActivity" Visible="false" />
			<px:PXDSCallbackCommand Name="gotoEntity" PostData="Page" Visible="false" />
			<px:PXDSCallbackCommand Name="complete" StartNewGroup="true" CommitChanges="true"
				PopupVisible="true" ClosePopup="true" />
			<px:PXDSCallbackCommand Name="completeAndFollowUp" CommitChanges="true" ClosePopup="true" PopupVisible="true"/>
			<px:PXDSCallbackCommand Name="cancelActivity" CommitChanges="true" PopupVisible="true" />
			<px:PXDSCallbackCommand Name="NewActivity" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="NewMailActivity" Visible="False" CommitChanges="True"
				PopupCommand="Cancel" PopupCommandTarget="ds" />
			<px:PXDSCallbackCommand Name="ViewActivity" Visible="False" CommitChanges="True"
				DependOnGrid="gridReferencedActivities" />
			<px:PXDSCallbackCommand Name="ViewTask" Visible="False" CommitChanges="True" DependOnGrid="gridReferencedTasks" />
		</CallbackCommands>
	</px:PXDataSource>
	<pxa:PXEntityViewer ID="viewer" runat="server" Target="ds" Command="SetViewed$CRTask" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXTab ID="tab" runat="server" DataSourceID="ds" Width="100%" DataMember="Tasks"
		NoteIndicator="True" FilesIndicator="True" DefaultControlID="edSubject">
		<Items>
			<px:PXTabItem Text="Details" >
				<AutoCallBack Enabled="True" Command="Save" Target="tab"><Behavior CommitChanges="True" /></AutoCallBack>
				<Template>
					<px:PXPanel ID="PXPanel1" runat="server" RenderStyle="Simple" ContentLayout-OuterSpacing="Around">
						<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="S"
							ControlSize="M" />
						<px:PXLayoutRule ID="PXLayoutRule2" runat="server" ColumnSpan="2" />
						<px:PXTextEdit ID="edSubject" runat="server" DataField="Subject" />
						<px:PXLayoutRule ID="PXLayoutRule3" runat="server" Merge="True" />
						<px:PXDateTimeEdit ID="edStartDate" runat="server" DataField="StartDate_Date" Size="M" CommitChanges="True"/>
						<px:PXCheckBox ID="edIsPrivate" runat="server" DataField="IsPrivate" />
						<px:PXLayoutRule ID="PXLayoutRule5" runat="server" />
						<px:PXDateTimeEdit ID="edDueDate" runat="server" DataField="EndDate_Date" Size="M" CommitChanges="True"/>
						<px:PXNumberEdit CommitChanges="True" ID="edPercentCompletion" runat="server" DataField="PercentCompletion"
							Size="XS" />
						<px:PXSelector CommitChanges="True" ID="edGroupID" runat="server" DataField="GroupID" />
						<px:PXSelector CommitChanges="True" ID="edOwner" runat="server" DataField="Owner"
							AutoRefresh="True">
							<Parameters>
								<px:PXControlParam ControlID="tab" Name="CRTask.groupID" PropertyName="NewDataKey[&quot;GroupID&quot;]" />
							</Parameters>
						</px:PXSelector>
						<px:PXCheckBox CommitChanges="True" ID="chkIsReminderOn" runat="server" DataField="IsReminderOn" />
						<px:PXLayoutRule ID="PXLayoutRule4" runat="server" Merge="True" />
						<px:PXDateTimeEdit CommitChanges="True" ID="edReminderDate_Date" runat="server" DataField="ReminderDate_Date" />
						<px:PXDateTimeEdit CommitChanges="True" ID="edReminderDate_Time" runat="server" DataField="ReminderDate_Time"
							DisplayFormat="g" EditFormat="g" TimeMode="True" SuppressLabel="True" Width="84" />
						<px:PXLayoutRule ID="PXLayoutRule6" runat="server" />
						<pxa:PXRefNoteSelector ID="edRefNoteID" runat="server" DataField="Source_Description" NoteIDDataField="RefNoteID"
							MaxValue="0" MinValue="0">
							<EditButton CommandName="Tasks$Navigate_ByRefNote" CommandSourceID="ds" />
							<LookupButton CommandName="Tasks$Select_RefNote" CommandSourceID="ds" />
							<LookupPanel DataMember="Tasks$RefNoteView" DataSourceID="ds" TypeDataField="Type"
								IDDataField="NoteID" />
						</pxa:PXRefNoteSelector>
						<px:PXSegmentMask ID="edProject" runat="server" DataField="ProjectID" HintField="description" CommitChanges="True" />
						<px:PXSegmentMask ID="edProjectTaskID" runat="server" DataField="ProjectTaskID" HintField="description"  AutoRefresh="true" CommitChanges="True"/>
						<px:PXLayoutRule ID="PXLayoutRule7" runat="server" StartColumn="True" LabelsWidth="S"
							ControlSize="SM" />
						<px:PXDropDown ID="edPriority" runat="server" AllowNull="False" DataField="Priority"
							SelectedIndex="1" />
						<px:PXDropDown CommitChanges="True" ID="edStatus" runat="server" AllowNull="False"
							DataField="UIStatus" />
						<px:PXSelector ID="edCategoryID" runat="server" DataField="CategoryID" Size="SM" />
						<px:PXDateTimeEdit ID="edCompletedDateTime" runat="server" DataField="CompletedDateTime"
							DisplayFormat="g" EditFormat="g" Enabled="False" Size="SM" />
                        <px:PXTimeSpan TimeMode="True" ID="edTimeSpent" runat="server" DataField="TimeSpent" InputMask="hh:mm"  MaxHours="99" Enabled="False"/>
                        <px:PXTimeSpan TimeMode="True" ID="edOvertimeSpent" runat="server" DataField="OvertimeSpent" InputMask="hh:mm" MaxHours="99" Enabled="False"/>
                        <px:PXTimeSpan TimeMode="True" ID="edTimeBillable1" runat="server" DataField="TimeBillable" InputMask="hh:mm" MaxHours="99" Enabled="False"/>
                        <px:PXTimeSpan TimeMode="True" ID="edOvertimeBillable" runat="server" DataField="OvertimeBillable" InputMask="hh:mm" MaxHours="99" Enabled="False"/>
						<px:PXTextEdit ID="PXTaskID" runat="server" DataField="TaskID" Visible="False">
							<AutoCallBack Command="Cancel" Target="tab" />
						</px:PXTextEdit>
					</px:PXPanel>
					<pxa:PXRichTextEdit ID="edBody" runat="server" DataField="Body" Style="width: 100%;
						height: 120px">
						<AutoSize Enabled="True" MinHeight="120" />
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
					                <px:PXTimeSpan TimeMode="True" ID="edTimeSpent" runat="server" DataField="TimeSpent" InputMask="hh:mm" MaxHours="99" />
					                <px:PXTimeSpan TimeMode="True" ID="edTimeBillable" runat="server" DataField="TimeBillable" InputMask="hh:mm" MaxHours="99" />
					                <px:PXTimeSpan TimeMode="True" ID="edOvertimeSpent" runat="server" DataField="OvertimeSpent" InputMask="hh:mm" MaxHours="99" />
					                <px:PXTimeSpan TimeMode="True" ID="edOvertimeBillable" runat="server" DataField="OvertimeBillable" InputMask="hh:mm" MaxHours="99" />
                                </RowTemplate>                                
                                <Columns>
                                    <px:PXGridColumn DataField="IsCompleteIcon" Width="21px" AllowShowHide="False" ForceExport="True" />
                                    <px:PXGridColumn DataField="PriorityIcon" Width="21px" AllowShowHide="False" AllowResize="False" ForceExport="True" />
                                    <px:PXGridColumn DataField="ReminderIcon" Width="21px" AllowShowHide="False" AllowResize="False" ForceExport="True" />
                                    <px:PXGridColumn DataField="ClassInfo" />
                                    <px:PXGridColumn DataField="RefNoteID" Visible="false" AllowShowHide="False" />
                                    <px:PXGridColumn DataField="Subject" LinkCommand="ViewActivity" Width="297px" />
                                    <px:PXGridColumn DataField="UIStatus" />
                                    <px:PXGridColumn DataField="Released" Width="80px" />
                                    <px:PXGridColumn DataField="StartDate" DisplayFormat="g" Width="120px" />
                                    <px:PXGridColumn DataField="CreatedDateTime" DisplayFormat="g" Width="120px" Visible="False" />
                                    <px:PXGridColumn DataField="CategoryID" />
                                    <px:PXGridColumn AllowNull="False" DataField="IsBillable" TextAlign="Center" Type="CheckBox" Width="60px" />
                                    <px:PXGridColumn DataField="TimeSpent" Width="100px" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="OvertimeSpent" Width="100px" RenderEditorText="True" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="TimeBillable" Width="100px" RenderEditorText="True" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="OvertimeBillable" Width="100px" RenderEditorText="True" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="CreatedByID_Creator_Username" Visible="false" SyncVisible="False" SyncVisibility="False" Width="108px" />
                                    <px:PXGridColumn DataField="GroupID" Width="90px" />
                                    <px:PXGridColumn DataField="Owner" LinkCommand="OpenActivityOwner" Width="150px" DisplayMode="Text"/>
                                </Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
						<Mode AllowAddNew="False" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Related Tasks">
				<Template>
					<px:PXGrid ID="gridReferencedTasks" runat="server" DataSourceID="ds" Height="323px"
						Style="z-index: 100; left: 0px; position: absolute; top: 0px" Width="100%" AllowSearch="True"
						AllowPaging="true" AdjustPageSize="Auto" BorderWidth="0" SkinID="Details" MatrixMode="true">
						<ActionBar DefaultAction="cmdViewTask">
							<Actions>
								<EditRecord Enabled="false" />
							</Actions>
							<CustomItems>
								<px:PXToolBarButton Key="cmdViewTask" Visible="false">
									<ActionBar MenuVisible="false" />
									<ActionBar GroupIndex="0" />
									<AutoCallBack Command="ViewTask" Target="ds" />
									<PopupCommand Command="Refresh" Target="gridReferencedTasks" />
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
						<Levels>
							<px:PXGridLevel DataMember="ReferencedTasks">
								<Columns>
									<px:PXGridColumn DataField="RefTaskID" Width="60px" AutoCallBack="true" />
									<px:PXGridColumn DataField="Subject" Width="300px" />
									<px:PXGridColumn DataField="Status" Width="60px" />
									<px:PXGridColumn DataField="StartDate" DisplayFormat="g" Width="120px" />
									<px:PXGridColumn DataField="EndDate" DisplayFormat="g" Width="120px" />
									<px:PXGridColumn DataField="CompletedDateTime" DisplayFormat="g" Width="120px" />
								</Columns>
								<RowTemplate>
									<px:PXSelector runat="server" ID="edRefTaskID" DataField="RefTaskID" FilterByAllFields="True" />
								</RowTemplate>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
						<Mode InitNewRow="true" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" />
	</px:PXTab>
</asp:Content>
