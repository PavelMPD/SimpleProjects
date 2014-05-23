<%@ Page Language="C#" MasterPageFile="~/MasterPages/TabView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="CR306010.aspx.cs" Inherits="Page_CR306010"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/TabView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Activites"
		TypeName="PX.Objects.EP.CRActivityMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" Visible="False" />
			<px:PXDSCallbackCommand Name="Cancel" PopupVisible="true" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="saveClose" Visible="False" PopupVisible="True"
				ClosePopup="True" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="save" PopupVisible="True" />
			<px:PXDSCallbackCommand Name="Delete" ClosePopup="true" PopupVisible="true" />
			<px:PXDSCallbackCommand Name="gotoEntity" Visible="false" />
			<px:PXDSCallbackCommand Name="gotoParentActivity" Visible="false" />
			<px:PXDSCallbackCommand Name="markAsCompleted" CommitChanges="True" PopupVisible="True"
				ClosePopup="True" StartNewGroup="true" />
			<px:PXDSCallbackCommand Name="markAsCompletedAndFollowUp" CommitChanges="True" Visible="False" ClosePopup="False" />
		</CallbackCommands>
		<DataTrees>
			<px:PXTreeDataMember TreeKeys="PageID" TreeView="Articles" />
			<px:PXTreeDataMember TreeView="_EPCompanyTree_Tree_" TreeKeys="WorkgroupID" />
		</DataTrees>
	</px:PXDataSource>
	<pxa:PXEntityViewer ID="viewer" runat="server" Target="ds" Command="SetViewed$CRCaseActivity" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXTab ID="tab" runat="server" DataSourceID="ds" DataMember="Activites" NoteIndicator="True"
		FilesIndicator="True" DefaultControlID="edSubject" Width="100%">
		<Items>
			<px:PXTabItem Text="Details">
				<Template>
					<px:PXPanel ID="PXPanel1" runat="server" RenderStyle="Simple" ContentLayout-OuterSpacing="Around">
						<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="S"
							ControlSize="M" />
						<px:PXLayoutRule ID="PXLayoutRule7" runat="server" ColumnSpan="2" />
						<px:PXTextEdit ID="edSubject" runat="server" DataField="Subject" />
						<px:PXLayoutRule ID="PXLayoutRule8" runat="server" />
						<px:PXLayoutRule ID="PXLayoutRule2" runat="server" Merge="True" />
						<px:PXSelector ID="ddType" runat="server" DataField="Type" CommitChanges="True" DisplayMode="Text" />
						<px:PXCheckBox ID="edIsPrivate" runat="server" DataField="IsPrivate" />
						<px:PXLayoutRule ID="PXLayoutRule6" runat="server" />
						<px:PXLayoutRule ID="PXLayoutRule3" runat="server" Merge="True" />
						<px:PXDateTimeEdit ID="edStartDate_Date" runat="server" DataField="StartDate_Date"
							CommitChanges="True" />
						<px:PXDateTimeEdit ID="edStartDate_Time" runat="server" DataField="StartDate_Time"
							TimeMode="true" SuppressLabel="true" Width="84" CommitChanges="True" />
						<px:PXLayoutRule ID="PXLayoutRule4" runat="server" />
						<px:PXLayoutRule ID="PXLayoutRule9" runat="server" LabelsWidth="S" ControlSize="M" />
						<px:PXSelector ID="edGroupID" runat="server" DataField="GroupID" CommitChanges="True" />
						<px:PXSelector ID="edOwner" runat="server" DataField="Owner" CommitChanges="True" AutoRefresh="true" />
						<pxa:PXRefNoteSelector ID="edRefEntity" runat="server" DataField="Source_Description" NoteIDDataField="RefNoteID"
							MaxValue="0" MinValue="0" ValueType="Int64">
							<EditButton CommandName="Activites$Navigate_ByRefNote" CommandSourceID="ds" />
							<LookupButton CommandName="Activites$Select_RefNote" CommandSourceID="ds" />
							<LookupPanel DataMember="Activites$RefNoteView" DataSourceID="ds" TypeDataField="Type"
								IDDataField="NoteID" />
						</pxa:PXRefNoteSelector>
						<px:PXSelector runat="server" ID="edParentTaskID" DataField="ParentTaskID" TextMode="Search" AllowEdit="True" CommitChanges="True"/>
						<px:PXSegmentMask ID="edProject" runat="server" DataField="ProjectID" HintField="description" CommitChanges="True" />
						<px:PXSegmentMask ID="edProjectTaskID" runat="server" DataField="ProjectTaskID" HintField="description"  AutoRefresh="true" CommitChanges="True"/>
						<px:PXLayoutRule ID="PXLayoutRule5" runat="server" StartColumn="True" LabelsWidth="S"
							ControlSize="SM" />
						<px:PXCheckBox ID="chkTrackTime" runat="server" DataField="TrackTime" CommitChanges="True" />
						<px:PXDropDown ID="edStatus" runat="server" AllowNull="False" DataField="UIStatus"
							CommitChanges="True" />
						<px:PXSelector ID="edApprover" runat="server" DataField="ApproverID" Enabled="False" />
						<px:PXSelector ID="edEType" runat="server" DataField="EarningTypeID" AutoRefresh="true" CommitChanges="True" />
						<px:PXTimeSpan ID="edTimeSpent" TimeMode="True" runat="server" DataField="TimeSpent" CommitChanges="True" InputMask="hh:mm" Size="SM" />
						<px:PXTimeSpan TimeMode="true" ID="edOvertimeSpent" runat="server" DataField="OvertimeSpent" Enabled="False" Size="SM" InputMask="hh:mm"/>
						<px:PXCheckBox ID="chkIsBillable" runat="server" DataField="IsBillable" Text="Billable" CommitChanges="True" />
                        <px:PXCheckBox ID="chkReleased" runat="server" DataField="Released" Text="Released"  />
						<px:PXTimeSpan TimeMode="true" ID="edTimeBillable" runat="server" DataField="TimeBillable" CommitChanges="True" Size="SM" InputMask="hh:mm"/>
						<px:PXTimeSpan TimeMode="true" ID="edOvertimeBillable" runat="server" DataField="OvertimeBillable" CommitChanges="True" Size="SM" InputMask="hh:mm"/>
						<px:PXTextEdit ID="edTaskID" runat="server" DataField="TaskID" Visible="False">
							<AutoCallBack Command="Cancel" Enabled="True" Target="tab" />
						</px:PXTextEdit>
					</px:PXPanel>
					<pxa:PXRichTextEdit ID="edBody" runat="server" DataField="Body" Style="border-width: 0px; border-top-width: 1px;
						width: 100%; height: 100%;">
						<AutoSize Enabled="True" MinHeight="216" />
					</pxa:PXRichTextEdit>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Enabled="True" Container="Window" />
	</px:PXTab>
</asp:Content>
