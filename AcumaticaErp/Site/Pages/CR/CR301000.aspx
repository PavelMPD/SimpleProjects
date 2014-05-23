<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="CR301000.aspx.cs" Inherits="Page_CR301000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<script type="text/javascript">
		function refreshTasksAndEvents(ds, context)
		{
			if (context.command == "Cancel")
			{
				var top = window.top;
				if (top != window && top.MainFrame != null) top.MainFrame.refreshEventsInfo();
			}
		}
	</script>
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.CR.LeadMaint"
		PrimaryView="Lead">
		<ClientEvents CommandPerformed="refreshTasksAndEvents" />
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Cancel" PopupVisible="true" />
			<px:PXDSCallbackCommand Name="Delete" PopupVisible="true" />
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand Name="SaveClose" CommitChanges="True" Visible="false" PopupVisible="true"
				ClosePopup="true" />
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" PopupVisible="true" />
			<px:PXDSCallbackCommand Name="First" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Action" StartNewGroup="true" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="ConvertToContact" CommitChanges="True" Visible="False" />
            <px:PXDSCallbackCommand Name="ConvertToOpportunity" CommitChanges="True" Visible="False" />
            <px:PXDSCallbackCommand Name="ConvertToBAccount" CommitChanges="True" Visible="False" />
			<px:PXDSCallbackCommand Name="NewTask" Visible="False" CommitChanges="True" PopupCommand="Cancel" PopupCommandTarget="ds" />
			<px:PXDSCallbackCommand Name="NewEvent" Visible="False" CommitChanges="True" PopupCommand="Cancel" PopupCommandTarget="ds" />
			<px:PXDSCallbackCommand Name="NewActivity" Visible="False" CommitChanges="True" PopupCommand="Cancel" PopupCommandTarget="ds" />
			<px:PXDSCallbackCommand Name="NewMailActivity" Visible="False" CommitChanges="True"	PopupCommand="Cancel" PopupCommandTarget="ds" />
			<px:PXDSCallbackCommand Name="ViewActivity" Visible="False" CommitChanges="True"
				PopupCommand="Cancel" PopupCommandTarget="ds" DependOnGrid="gridActivities" />
			<px:PXDSCallbackCommand Name="OpenActivityOwner" Visible="False" CommitChanges="True" DependOnGrid="gridActivities" />
			<px:PXDSCallbackCommand Name="ViewOnMap" CommitChanges="true" Visible="false" />
			<px:PXDSCallbackCommand Name="ValidateAddress" Visible="False" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="Subscriptions_CRMarketingList_ViewDetails" Visible="False" CommitChanges="True" DependOnGrid="grdMarketingLists" />
			<px:PXDSCallbackCommand Name="Relations_EntityDetails" Visible="False" CommitChanges="True"	DependOnGrid="grdRelations" />
			<px:PXDSCallbackCommand Name="Relations_ContactDetails" Visible="False" CommitChanges="True" DependOnGrid="grdRelations" />
            <px:PXDSCallbackCommand Name="Merge"  Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="AttachToAccount"  Visible="False" CommitChanges="True" DependOnGrid="PXGridDuplicates"/>
            <px:PXDSCallbackCommand Name="CheckForDuplicates" CommitChanges="True" Visible="False" />
            <px:PXDSCallbackCommand Visible="false" DependOnGrid="PXGridDuplicates" Name="Duplicates_Contact_ViewDetails" />
			<px:PXDSCallbackCommand Visible="false" DependOnGrid="PXGridDuplicates" Name="Duplicates_BAccount_ViewDetails" />	
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%"
		DataMember="Lead" NoteIndicator="True" FilesIndicator="True" LinkIndicator="True"
		NotifyIndicator="True" Caption="Lead Summary" DefaultControlID="edContactID">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			<px:PXSelector ID="edContactID" runat="server" DataField="ContactID" NullText="<NEW>"
				DisplayMode="Text" TextMode="Search" FilterByAllFields="True" AutoRefresh="True" />
			<px:PXDropDown ID="edStatus" runat="server" DataField="Status" CommitChanges="True"	Size="SM" AllowNull="False"  />
			<px:PXDropDown ID="edResolution" runat="server" DataField="Resolution" CommitChanges="True" Size="SM" AutoRefresh="True" AllowNull="False" />
			<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
			<px:PXSelector ID="WorkgroupID" runat="server" DataField="WorkgroupID" CommitChanges="True"
				TextMode="Search" DisplayMode="Text" FilterByAllFields="True" />
			<px:PXSelector ID="OwnerID" runat="server" DataField="OwnerID" TextMode="Search"
				DisplayMode="Text" FilterByAllFields="True" AutoRefresh="True" />
            <px:PXDropDown ID="edDuplicateStatus" runat="server" DataField="DuplicateStatus" CommitChanges="True" Enabled="False"/>
            <px:PXCheckBox ID="edDuplicateFound" runat="server" DataField="DuplicateFound" Visible="False"/>
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Width="100%" DataSourceID="ds" DataMember="LeadCurrent">
		<Items>
			<px:PXTabItem Text="Details" RepaintOnDemand="False">
				<Template>
					<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
					<px:PXLayoutRule runat="server" GroupCaption="Summary" StartGroup="True" />
					<px:PXLayoutRule runat="server" Merge="True" />
					<px:PXLabel ID="LFirstName" runat="server" Size="SM" />
					<px:PXDropDown ID="Title" runat="server" DataField="Title" Size="XS" SuppressLabel="True" CommitChanges="True" />
					<px:PXTextEdit ID="FirstName" runat="server" DataField="FirstName" Width="164px"  CommitChanges="True"
						LabelID="LFirstName" />
					<px:PXLayoutRule runat="server" />
					<px:PXTextEdit ID="edLastName" runat="server" DataField="LastName" SuppressLabel="False"
						CommitChanges="True" />
					<px:PXTextEdit ID="edSalutation" runat="server" DataField="Salutation" SuppressLabel="False" />
					<px:PXSegmentMask ID="edBAccountID" runat="server" AllowEdit="True" CommitChanges="True"
						DataField="BAccountID" FilterByAllFields="True" TextMode="Search" DisplayMode="Hint" />
					<px:PXTextEdit ID="edCompanyName" runat="server" DataField="FullName" />
					<px:PXSegmentMask ID="edParentBAccountID" runat="server" AllowEdit="True" DataField="ParentBAccountID"
						FilterByAllFields="True" TextMode="Search" DisplayMode="Hint" />
					<px:PXLayoutRule ID="PXLayoutRule1" runat="server" GroupCaption="Contact" StartGroup="True" />
					<px:PXMailEdit ID="EMail" runat="server" CommandName="NewMailActivity" CommandSourceID="ds"
						DataField="EMail" CommitChanges="True"/>
					<px:PXLinkEdit ID="WebSite" runat="server" DataField="WebSite" CommitChanges="True"/>
                    <px:PXLayoutRule ID="PXLayoutRule2" runat="server" Merge="True" />
                    <px:PXLabel ID="LPhone1" runat="server" Size="SM" />
                    <px:PXDropDown ID="Phone1Type" runat="server" DataField="Phone1Type" Size="XS" SuppressLabel="True" CommitChanges="True" />
					<px:PXMaskEdit ID="Phone1" runat="server" DataField="Phone1" Width="164px" LabelID="LPhone1" />
                    <px:PXLayoutRule ID="PXLayoutRule3" runat="server" Merge="True" />
                    <px:PXLabel ID="LPhone2" runat="server" Size="SM" />
                    <px:PXDropDown ID="Phone2Type" runat="server" DataField="Phone2Type" Size="XS" SuppressLabel="True" CommitChanges="True"/>
					<px:PXMaskEdit ID="Phone2" runat="server" DataField="Phone2" Width="164px" LabelID="LPhone2" />
                    <px:PXLayoutRule ID="PXLayoutRule12" runat="server" Merge="True" />
                    <px:PXLabel ID="LPhone3" runat="server" Size="SM" />
                    <px:PXDropDown ID="Phone3Type" runat="server" DataField="Phone3Type" Size="XS" SuppressLabel="True" CommitChanges="True" />
					<px:PXTextEdit ID="Phone3" runat="server" DataField="Phone3" Width="164px" LabelID="LPhone3"/>
                    <px:PXLayoutRule ID="PXLayoutRule4" runat="server" Merge="True" />
                    <px:PXLabel ID="LFax" runat="server" Size="SM" />
                    <px:PXDropDown ID="FaxType" runat="server" DataField="FaxType" Size="XS" SuppressLabel="True" CommitChanges="True" />
					<px:PXMaskEdit ID="Fax" runat="server" DataField="Fax" Width="164px" LabelID="LFax"/>
					<px:PXLayoutRule runat="server" />
					<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
                    <px:PXLayoutRule ID="PXLayoutRule5" runat="server" GroupCaption="CRM" StartGroup="True" />
					<px:PXSelector ID="ClassID" runat="server" DataField="ClassID" CommitChanges="True"
						FilterByAllFields="True" AllowEdit="True" TextMode="Search" DisplayMode="Hint" />
					<px:PXDropDown ID="Source" runat="server" DataField="Source" />
                    <px:PXSelector ID="CampaignID" runat="server" DataField="CampaignID" CommitChanges="True"/>
					<px:PXDropDown ID="Method" runat="server" DataField="Method" />
					<px:PXLayoutRule ID="PXLayoutRule7" runat="server" Merge="True" />
					<px:PXCheckBox ID="edNoCall" runat="server" DataField="NoCall" Size="S" SuppressLabel="True" />
					<px:PXCheckBox ID="edNoFax" runat="server" DataField="NoFax" Size="S" SuppressLabel="True" />
					<px:PXLayoutRule ID="PXLayoutRule8" runat="server" />
					<px:PXLayoutRule ID="PXLayoutRule9" runat="server" Merge="True" />
					<px:PXCheckBox ID="edNoEMail" runat="server" DataField="NoEMail" Size="S" SuppressLabel="True" />
					<px:PXCheckBox ID="edNoMail" runat="server" DataField="NoMail" Size="S" SuppressLabel="True" />
					<px:PXLayoutRule ID="PXLayoutRule10" runat="server" />
					<px:PXLayoutRule ID="PXLayoutRule11" runat="server" Merge="True" />
					<px:PXCheckBox ID="edNoMassMail" runat="server" DataField="NoMassMail" Size="S" />
					<px:PXCheckBox ID="edNoMarketingMaterials" runat="server" DataField="NoMarketing"
						Size="S" />
					<px:PXLayoutRule runat="server" GroupCaption="Address " StartGroup="True" />
					<px:PXFormView ID="formA" runat="server" DataMember="AddressCurrent" DataSourceID="ds" RenderStyle="Simple">
						<Template>
							<px:PXLayoutRule ID="PXLayoutRule14" runat="server" Merge="True" SuppressLabel="False" />
							<px:PXFormView ID="panelfordatamember" runat="server" DataMember="LeadCurrent2" DataSourceID="ds"
								RenderStyle="Simple">
								<Template>
									<px:PXLayoutRule ID="PXLayoutRule14" runat="server" SuppressLabel="False" LabelsWidth="SM" />
									<px:PXCheckBox ID="chkIsSameAsMaint" runat="server" DataField="IsAddressSameAsMain"
										CommitChanges="true">
									</px:PXCheckBox>
								</Template>
								<ContentStyle BackColor="Transparent"/>
							</px:PXFormView>
							<px:PXCheckBox ID="chkIsValidated" runat="server" DataField="IsValidated" Enabled="False" />
							<px:PXLayoutRule ID="PXLayoutRule18" runat="server" />
							<px:PXTextEdit ID="edAddressLine1" runat="server" DataField="AddressLine1" />
							<px:PXTextEdit ID="edAddressLine2" runat="server" DataField="AddressLine2" />
							<px:PXTextEdit ID="edCity" runat="server" DataField="City" />
							<px:PXSelector ID="edCountryID" runat="server" AllowEdit="True" DataField="CountryID"
								FilterByAllFields="True" TextMode="Search" DisplayMode="Hint" CommitChanges="True" />
							<px:PXSelector ID="edState" runat="server" AutoRefresh="True" DataField="State" CommitChanges="true"
								FilterByAllFields="True" TextMode="Search" DisplayMode="Hint" />
							<px:PXLayoutRule runat="server" Merge="True" />
							<px:PXMaskEdit ID="edPostalCode" runat="server" DataField="PostalCode" Size="S" />
							<px:PXButton ID="btnViewOnMap" runat="server" CommandName="ViewOnMap" CommandSourceID="ds"
								Size="xs" Text="View On Map" Height="20" />
							<px:PXLayoutRule runat="server" />
						</Template>
						<ContentLayout ControlSize="XM" LabelsWidth="SM" OuterSpacing="None" />
						<ContentStyle BackColor="Transparent" BorderStyle="None">
						</ContentStyle>
					</px:PXFormView>
				</Template>
			</px:PXTabItem>
            <px:PXTabItem Text="Duplicates"  BindingContext="form" VisibleExp="DataControls[&quot;edDuplicateFound&quot;].Value == true" LoadOnDemand="True">
				<Template>
					<px:PXGrid ID="PXGridDuplicates" runat="server" DataSourceID="ds" SkinID="Inquire" Width="100%"
						Height="200px" MatrixMode="True">
					    <ActionBar>
							<CustomItems>
					            <px:PXToolBarButton Text="Merge" Key="cmdMerge">
                                        <AutoCallBack Command="Merge" Target="ds"></AutoCallBack>
                                        <PopupCommand Command="Cancel" Target="ds" />
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Text="Attach to Account" Key="cmdAttachToAccount">
                                        <AutoCallBack Command="AttachToAccount" Target="ds"></AutoCallBack>
                                        <PopupCommand Command="Cancel" Target="ds" />
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
						<Levels>
							<px:PXGridLevel DataMember="Duplicates">
								<Columns>
								    <px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" Width="80px" AllowCheckAll="True"/>
                                    <px:PXGridColumn DataField="Contact2__ContactType" TextAlign="Left" Width="80px" AllowShowHide="False" />
                                    <px:PXGridColumn DataField="Contact2__DuplicateStatus" TextAlign="Left" Width="140px" />
									<px:PXGridColumn DataField="Contact2__LastModifiedDateTime" TextAlign="Left" Width="160px" />
									<px:PXGridColumn DataField="Contact2__DisplayName" TextAlign="Left" Width="180px" AllowShowHide="False"  LinkCommand="Duplicates_Contact_ViewDetails" />									
									<px:PXGridColumn DataField="Contact2__BAccountID" TextAlign="Left" Width="180px" LinkCommand="Duplicates_BAccount_ViewDetails"  />
                                    <px:PXGridColumn DataField="BAccountR__Type" TextAlign="Left" Width="80px" />
                                    <px:PXGridColumn DataField="BAccountR__AcctName" TextAlign="Left" Width="180px" />
									<px:PXGridColumn DataField="Contact2__Email" TextAlign="Left" Width="180px" AllowShowHide="False" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="200" />
						<ActionBar>
							<Actions>
								<Search Enabled="False" />
							</Actions>
						</ActionBar>
                        <Mode AllowAddNew="False" AllowColMoving="False" AllowDelete="False" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Attributes">
				<Template>
					<px:PXGrid ID="PXGridAnswers" runat="server" DataSourceID="ds" SkinID="Inquire" Width="100%"
						Height="200px" MatrixMode="True">
						<Levels>
							<px:PXGridLevel DataMember="Answers">
								<Columns>
									<px:PXGridColumn DataField="AttributeID" TextAlign="Left" Width="250px" AllowShowHide="False"
										TextField="AttributeID_description" />
    								<px:PXGridColumn DataField="isRequired" TextAlign="Center" Type="CheckBox" Width="75px" />
									<px:PXGridColumn DataField="Value" Width="300px" AllowShowHide="False" AllowSort="False" />
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="200" />
						<ActionBar>
							<Actions>
								<Search Enabled="False" />
							</Actions>
						</ActionBar>
                        <Mode AllowAddNew="False" AllowColMoving="False" AllowDelete="False" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Activities" LoadOnDemand="True">
				<Template>
					<pxa:PXGridWithPreview ID="gridActivities" runat="server" DataSourceID="ds" Width="100%"
						AllowSearch="True" DataMember="Activities" AllowPaging="true" NoteField="NoteText"
						FilesField="NoteFiles" BorderWidth="0px" GridSkinID="Inquire" SplitterStyle="z-index: 100; border-top: solid 1px Gray;  border-bottom: solid 1px Gray"
						PreviewPanelStyle="z-index: 100; background-color: Window" PreviewPanelSkinID="Preview"
						BlankFilterHeader="All Activities" MatrixMode="true" PrimaryViewControlID="form">
						<ActionBar DefaultAction="cmdViewActivity" CustomItemsGroup="0">
							<CustomItems>
								<px:PXToolBarButton Text="Add Task" Key="cmdAddTask">
									<AutoCallBack Command="NewTask" Target="ds" />
									<PopupCommand Command="Cancel" Target="ds" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Text="Add Event" Key="cmdAddEvent">
									<AutoCallBack Command="NewEvent" Target="ds" />
									<PopupCommand Command="Cancel" Target="ds" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Text="Add Email" Key="cmdAddEmail">
									<AutoCallBack Command="NewMailActivity" Target="ds" />
									<PopupCommand Command="Cancel" Target="ds" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Text="Add Activity" Key="cmdAddActivity">
									<AutoCallBack Command="NewActivity" Target="ds" />
									<PopupCommand Command="Cancel" Target="ds" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Key="cmdViewActivity" Visible="false">
									<ActionBar MenuVisible="false" />
									<AutoCallBack Command="ViewActivity" Target="ds" />
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
						<Levels>
							<px:PXGridLevel DataMember="Activities">
								<Columns>
									<px:PXGridColumn DataField="IsCompleteIcon" Width="21px" AllowShowHide="False" AllowResize="False"
										ForceExport="True" />
									<px:PXGridColumn DataField="PriorityIcon" Width="21px" AllowShowHide="False" AllowResize="False"
										ForceExport="True" />
									<px:PXGridColumn DataField="ReminderIcon" Width="21px" AllowShowHide="False" AllowResize="False"
										ForceExport="True" />
									<px:PXGridColumn DataField="ClassIcon" Width="21px" AllowShowHide="False" AllowResize="False"
										ForceExport="True" />
									<px:PXGridColumn DataField="ClassInfo" Width="60px" />
									<px:PXGridColumn DataField="RefNoteID" Visible="false" AllowShowHide="False" />
									<px:PXGridColumn DataField="Subject" LinkCommand="ViewActivity" Width="297px" />
									<px:PXGridColumn DataField="UIStatus" />
                                    <px:PXGridColumn DataField="MPStatus" />
                                    <px:PXGridColumn DataField="Released" TextAlign="Center" Type="CheckBox"  />
									<px:PXGridColumn DataField="StartDate" DisplayFormat="g" Width="120px" />
                                    <px:PXGridColumn DataField="CreatedDateTime" DisplayFormat="g" Width="120px" Visible="False" />
                                    <px:PXGridColumn DataField="TimeSpent" Width="80px" />
									<px:PXGridColumn DataField="CreatedByID" Visible="false" AllowShowHide="False" />
									<px:PXGridColumn DataField="CreatedByID_Creator_Username" Visible="false"
										SyncVisible="False" SyncVisibility="False" Width="108px">
										<NavigateParams>
											<px:PXControlParam Name="PKID" ControlID="gridActivities" PropertyName="DataValues[&quot;CreatedByID&quot;]" />
										</NavigateParams>
									</px:PXGridColumn>
									<px:PXGridColumn DataField="GroupID" Width="90px" />
									<px:PXGridColumn DataField="Owner" LinkCommand="OpenActivityOwner" Width="150px" DisplayMode="Text" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<PreviewPanelTemplate>
							<pxa:PXHtmlView ID="edBody" runat="server" DataField="body" TextMode="MultiLine"
								MaxLength="50" Width="100%" Height="100%" SkinID="Label" />
						</PreviewPanelTemplate>
						<AutoSize Enabled="true" />
						<GridMode AllowAddNew="False" AllowDelete="False" AllowFormEdit="False" AllowUpdate="False" AllowUpload="False" />
					</pxa:PXGridWithPreview>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Relations" LoadOnDemand="True">
				<Template>
					<px:PXGrid ID="grdRelations" runat="server" Height="400px" Width="100%" AllowPaging="True"
						ActionsPosition="Top" AllowSearch="true" DataSourceID="ds" SkinID="Details">
						<Levels>
							<px:PXGridLevel DataMember="Relations">
								<Columns>
									<px:PXGridColumn DataField="Role" Width="120px" />
									<px:PXGridColumn DataField="EntityID" Width="160px" AutoCallBack="true" TextField="EntityCD"
										LinkCommand="Relations_EntityDetails" />
									<px:PXGridColumn DataField="Name" Width="200px" />
									<px:PXGridColumn DataField="ContactID" Width="160px" AutoCallBack="true" TextField="ContactName"
										DisplayMode="Text" LinkCommand="Relations_ContactDetails" />
									<px:PXGridColumn DataField="Email" Width="120px" />
									<px:PXGridColumn DataField="AddToCC" Width="70px" Type="CheckBox" TextAlign="Center" />
								</Columns>
								<RowTemplate>
									<px:PXSelector ID="edRelEntityID" runat="server" DataField="EntityID" FilterByAllFields="True" />
									<px:PXSelector ID="edRelContactID" runat="server" DataField="ContactID" FilterByAllFields="True" AutoRefresh="True" />
								</RowTemplate>
							</px:PXGridLevel>
						</Levels>
						<Mode InitNewRow="True" />
						<AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Marketing Lists" LoadOnDemand="True">
				<Template>
					<px:PXGrid ID="grdMarketingLists" runat="server" Height="400px" Width="100%" 
					AllowPaging="True" ActionsPosition="Top" AllowSearch="true" DataSourceID="ds" SkinID="Details">
						<Levels>
							<px:PXGridLevel DataMember="Subscriptions">
								<Columns>
									<px:PXGridColumn DataField="MarketingListID" Width="120px" AutoCallBack="true" TextField="CRMarketingList__MailListCode"
										LinkCommand="Subscriptions_CRMarketingList_ViewDetails" />
									<px:PXGridColumn DataField="CRMarketingList__Name" Width="200px" />
									<px:PXGridColumn DataField="Activated" Width="60px" Type="CheckBox" />
									<px:PXGridColumn DataField="Format" Width="80px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="450" MinWidth="300" />
	</px:PXTab>
    <px:PXSmartPanel ID="PanelCreateAccount" runat="server" Style="z-index: 108; position: absolute; left: 27px; top: 99px;" Caption="New Account"
        CaptionVisible="True" LoadOnDemand="true" ShowAfterLoad="true" Key="AccountInfo" AutoCallBack-Enabled="true" AutoCallBack-Target="formCreateAccount" AutoCallBack-Command="Refresh"
        CallBackMode-CommitChanges="True" CallBackMode-PostData="Page" AcceptButtonID="PXButtonOK" CancelButtonID="PXButtonCancel">
        <px:PXFormView ID="formCreateAccount" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Services Settings" CaptionVisible="False" SkinID="Transparent"
            DataMember="AccountInfo">
            <Template>
                <px:PXLayoutRule ID="PXLayoutRule6" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                <px:PXMaskEdit ID="edBAccountID" runat="server" DataField="BAccountID" CommitChanges="True"/>
                <px:PXTextEdit ID="edAccountName" runat="server" DataField="AccountName" CommitChanges="True"/>
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanel3" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButtonOK" runat="server" Text="Create" DialogResult="OK" Width="63px" Height="20px"></px:PXButton>
            <px:PXButton ID="PXButtonCancel" runat="server" DialogResult="Cancel" Text="Cancel" Width="63px" Height="20px" Style="margin-left: 5px" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <px:PXSmartPanel ID="PanelCreateOpportunity" runat="server" Style="z-index: 108; position: absolute; left: 27px; top: 99px;" Caption="New Opportunity"
        CaptionVisible="True" LoadOnDemand="true" ShowAfterLoad="true" Key="OpportunityInfo" AutoCallBack-Enabled="true" AutoCallBack-Target="formCreateOpportunity" AutoCallBack-Command="Refresh"
        CallBackMode-CommitChanges="True" CallBackMode-PostData="Page" AcceptButtonID="PXButtonOpportunityOK" CancelButtonID="PXButtonOpportunityCancel">
        <px:PXFormView ID="formCreateOpportunity" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Services Settings" CaptionVisible="False" SkinID="Transparent"
            DataMember="OpportunityInfo">
            <Template>
                <px:PXLayoutRule ID="PXLayoutRule6" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
                <px:PXMaskEdit CommitChanges="True" ID="edOpportunityID" runat="server" DataField="OpportunityID" />
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButtonOpportunityOK" runat="server" Text="Create" DialogResult="OK" Width="63px" Height="20px"></px:PXButton>
            <px:PXButton ID="PXButtonOpportunityCancel" runat="server" DialogResult="Cancel" Text="Cancel" Width="63px" Height="20px" Style="margin-left: 5px" />
        </px:PXPanel>
    </px:PXSmartPanel>
        <px:PXSmartPanel ID="spMergeParamsDlg" runat="server"
        Width="600" Caption="Please resolve the conflicts" CaptionVisible="True" Key="Duplicates" LoadOnDemand="True" ShowAfterLoad="true"
        AutoCallBack-Enabled="true" AutoCallBack-Target="formMerge" AutoCallBack-Command="Refresh"
        CallBackMode-CommitChanges="True" CallBackMode-PostData="Page"
        AcceptButtonID="btnOk" CancelButtonID="btnCancel">
        <px:PXFormView ID="formMerge" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" SkinID="Transparent"
            DataMember="mergeParams">
            <Template>
                <px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartColumn="True" />
                <px:PXSelector CommitChanges="True" ID="edContactID" runat="server" DataField="ContactID" FilterByAllFields="True" DisplayMode="Text" TextMode="Search" AutoRefresh="True"/>
                <px:PXGrid ID="grdFields" runat="server" Height="250px" DataSourceID="ds" MatrixMode="True">
                    <Levels>
                        <px:PXGridLevel DataMember="ValueConflicts">
                            <Columns>
                                <px:PXGridColumn DataField="DisplayName" Width="108px" />
                                <px:PXGridColumn DataField="Value" Width="190px" AutoCallBack="True" />
                            </Columns>
                            <Layout ColumnsMenu="False" />
                            <Mode AllowAddNew="false" AllowDelete="false" />
                        </px:PXGridLevel>
                    </Levels>
                    <ActionBar>
                        <Actions>
                            <ExportExcel Enabled="False" />
                            <AddNew Enabled="False" />
                            <FilterShow Enabled="False" />
                            <FilterSet Enabled="False" />
                            <Save Enabled="False" />
                            <Delete Enabled="False" />
                            <NoteShow Enabled="False" />
                            <Search Enabled="False" />
                            <AdjustColumns Enabled="False" />
                        </Actions>
                    </ActionBar>
                    <CallbackCommands>
                        <Save PostData="Page" />
                    </CallbackCommands>
                </px:PXGrid>
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
            <px:PXButton ID="btnOK" runat="server" Text="OK" DialogResult="OK" />
            <px:PXButton ID="btnCancel" runat="server" Text="Cancel" DialogResult="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>

</asp:Content>
