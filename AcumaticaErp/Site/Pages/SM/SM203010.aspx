<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM203010.aspx.cs" Inherits="Page_SM201020"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <pxa:DynamicDataSource ID="ds" runat="server" AutoCallBack="True" Visible="True"
		Width="100%" PrimaryView="UserProfile" TypeName="PX.SM.MyProfileMaint,PX.SM.SMAccessPersonalMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="SaveUsers" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="changePassword" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="changeEmail" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="changeSecretAnswer" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="GetCalendarSyncUrl" Visible="false" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="resetTimeZone" Visible="false" />
		</CallbackCommands>
		<DataTrees>
			<px:PXTreeDataMember TreeView="SiteMap" TreeKeys="NodeID" />
		</DataTrees>
	</pxa:DynamicDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <script type="text/javascript">
		function btnResetTimeZone_Click(sender, args) {
			var findTimeZoneElem = function (container) {
				if (container.childNodes)
					for (var i = 0; i < container.childNodes.length; i++) {
						var child = container.childNodes[i];
						if (child && child.getAttribute && String.compare(child.getAttribute("timeZone"), "1", true)) return child;
						var innerTable = findTimeZoneElem(child);
						if (innerTable) return innerTable;
					}
			}
			var tzElem = findTimeZoneElem(document.body);
			//if (tzElem) alert(tzElem);
		}
	</script>
	<px:PXTab ID="tab" runat="server" DataSourceID="ds" Height="400px" Style="z-index: 100"
		Width="100%" DataMember="UserProfile" Caption="Edit your profile" OnDataBound="tab_DataBound"
		OnInit="tab_Init" RepaintOnDemand="False">
		<Items>
            <px:PXTabItem Text="General Info">
                <Template>
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="User Settings" ControlSize="XL" LabelsWidth="M"/>
                    
                    <px:PXControlParam Type="String" PropertyName='NewDataKey["Username"]' Name="Username" ControlID="tab" />
                    <px:PXTextEdit ID="edUsername" runat="server" DataField="Username" Enabled="false" />
                    <px:PXTextEdit ID="edFirstName" runat="server" DataField="FirstName" />
                    <px:PXTextEdit ID="edLastName" runat="server" DataField="LastName" />
                    <px:PXMaskEdit ID="edPhone" runat="server" DataField="Phone" />
                    <px:PXLayoutRule ID="PXLayoutRule13" runat="server" Merge="True"/>
                    <px:PXMailEdit ID="edEmail" runat="server" DataField="Email" Enabled="False"/>
                    <px:PXButton Style="left: 162px; position: absolute; top: 144px" ID="PXButton1"
                        runat="server" Width="110px" Height="20px" Text="Change Email" CommandName="changeEmail" CommandSourceID="ds">
                    </px:PXButton>
                    <px:PXLayoutRule ID="PXLayoutRule15" runat="server" Merge="True"/>
                    <px:PXTextEdit ID="edPassword" runat="server" DataField="Password" Enabled="False" TextMode="Password"/>
                    <px:PXButton Style="left: 162px; position: absolute; top: 144px" ID="btnChangePassword"
                        runat="server" Width="110px" Height="20px" Text="Change Password" CommandName="changePassword" CommandSourceID="ds">
                    </px:PXButton>
                    <px:PXLayoutRule ID="PXLayoutRule16" runat="server" Merge="True"/>
                    <px:PXTextEdit ID="edPasswordQuestion" runat="server" DataField="PasswordQuestion" />
                    <px:PXButton Style="left: 162px; position: absolute; top: 144px" ID="btnChangeAnswer"
                        runat="server" Width="110px" Height="20px" Text="Change Answer" CommandName="changeSecretAnswer" CommandSourceID="ds">
                    </px:PXButton>
                    <px:PXLayoutRule ID="PXLayoutRule14" runat="server" />
                    <px:PXTextEdit ID="edComment" runat="server" DataField="Comment" TextMode="MultiLine" />
                    
					<px:PXFormView ID="formUserPrefs" runat="server" DataMember="UserPrefs" RenderStyle="Simple">
						<Template>
						    <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartGroup="True" GroupCaption="Personal Settings" ControlSize="XL" LabelsWidth="M"/>
							<px:PXSelector ID="edPdfCertificateName" runat="server" DataField="PdfCertificateName" />
							<px:PXLayoutRule ID="PXLayoutRule2" runat="server" Merge="True" />
							<px:PXDropDown ID="edTimeZone" runat="server" DataField="TimeZone" />
							<px:PXButton Size="m" ID="btnResetTimeZone" runat="server" Height="20px" Text="Reset"
								ToolTip="Reset To Calendar Time Zone" CommandSourceID="ds" CommandName="resetTimeZone"
								Hidden="True">
								<ClientEvents Click="btnResetTimeZone_Click" />
							</px:PXButton>
							<px:PXLayoutRule ID="PXLayoutRule4" runat="server" Merge="True" />
							<px:PXDropDown Size="XM" ID="edEditorFont" runat="server" DataField="EditorFontName"
								AllowNull="False" />
							<px:PXDropDown SuppressLabel="True" Size="XS" ID="edEditorFontSize" runat="server"
								DataField="EditorFontSize" AllowNull="False" />
							<px:PXLayoutRule ID="PXLayoutRule5" runat="server" />
							<px:PXSelector ID="edBranchCD" runat="server" DataField="DefBranchID" ValueField="BranchID"
								TextField="BranchCD">
								<GridProperties FastFilterFields="BranchCD">
								</GridProperties>
							</px:PXSelector>
							<px:PXTreeSelector ID="edHomePage" runat="server" DataField="HomePage" PopulateOnDemand="True"
								TreeDataMember="SiteMap" TreeDataSourceID="ds" InitialExpandLevel="0" ShowRootNode="False">
								<DataBindings>
									<px:PXTreeItemBinding DataMember="SiteMap" TextField="Title" ValueField="NodeID"
										ImageUrlField="Icon" />
								</DataBindings>
							</px:PXTreeSelector>
							<px:PXLayoutRule ID="PXLayoutRule6" runat="server" StartGroup="True" GroupCaption="Export to Excel" ControlSize="XL" LabelsWidth="M"/>
							<px:PXCheckBox ID="chkHiddenSkip" runat="server" DataField="HiddenSkip" />
							<px:PXCheckBox ID="chkBorder" runat="server" DataField="Border" />
						</Template>
					</px:PXFormView>					

                    <px:PXSmartPanel ID="pnlChangePassword" runat="server" Caption="Change Password" CaptionVisible="True"
                        LoadOnDemand="True" Width="400px"
                        Key="Passwords" ShowAfterLoad="true"
                        AutoCallBack-Enabled="true" AutoCallBack-Target="formPasswords" AutoCallBack-Command="Refresh"
                        CallBackMode-CommitChanges="True" CallBackMode-PostData="Page" 
                        AcceptButtonID="btnOk" CancelButtonID="btnCancel">
                        <px:PXFormView ID="formPasswords" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" SkinID="Transparent"
                            DataMember="Passwords">
                            <Template>
                                <px:PXLayoutRule ID="PXLayoutRule12" runat="server" StartColumn="True" ControlSize="M" LabelsWidth="SM">
                                </px:PXLayoutRule>
                                <px:PXTextEdit ID="edOldPassword" runat="server" DataField="OldPassword" TextMode="Password" Required="True" />
                                <px:PXTextEdit ID="edNewPassword" runat="server" DataField="NewPassword" TextMode="Password" Required="True" />
                                <px:PXTextEdit ID="edConfirmPassword" runat="server" DataField="ConfirmPassword" TextMode="Password" Required="True" />
                            </Template>
                        </px:PXFormView>
						<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
						    <px:PXButton ID="btnOk" runat="server" DialogResult="OK" Text="OK"/>
                            <px:PXButton ID="btnCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
                        </px:PXPanel>
                    </px:PXSmartPanel>
                    <px:PXSmartPanel ID="pnlChangeEmail" runat="server" Caption="Change Email" CaptionVisible="True"
                        LoadOnDemand="True" Width="400px"
                        Key="NewEmail" ShowAfterLoad="true"
                        AutoCallBack-Enabled="true" AutoCallBack-Target="formNewEmail" AutoCallBack-Command="Refresh"
                        CallBackMode-CommitChanges="True" CallBackMode-PostData="Page" 
                        AcceptButtonID="btnOk2" CancelButtonID="btnCancel2">
                        <px:PXFormView ID="formNewEmail" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" SkinID="Transparent"
                            DataMember="NewEmail">
                            <Template>
                                <px:PXLayoutRule ID="PXLayoutRule12" runat="server" StartColumn="True" ControlSize="M" LabelsWidth="SM">
                                </px:PXLayoutRule>
                                <px:PXMailEdit ID="edEmail" runat="server" DataField="Email" Required="True" CommitChanges="True"/>
                                <px:PXTextEdit ID="edPassword" runat="server" DataField="Password" TextMode="Password" Required="True" />
                            </Template>
                        </px:PXFormView>
						<px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
						    <px:PXButton ID="btnOk2" runat="server" DialogResult="OK" Text="OK"/>
                            <px:PXButton ID="btnCancel2" runat="server" DialogResult="Cancel" Text="Cancel" />
                        </px:PXPanel>
                    </px:PXSmartPanel>
                    <px:PXSmartPanel ID="pnlChangeAnswer" runat="server" Caption="Change Password Recowery Answer" CaptionVisible="True"
                        LoadOnDemand="True" Width="400px"
                        Key="NewAnswer" ShowAfterLoad="true"
                        AutoCallBack-Enabled="true" AutoCallBack-Target="formNewAnswer" AutoCallBack-Command="Refresh"
                        CallBackMode-CommitChanges="True" CallBackMode-PostData="Page" 
                        AcceptButtonID="btnOk3" CancelButtonID="btnCancel3">
                        <px:PXFormView ID="formNewAnswer" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" SkinID="Transparent"
                            DataMember="NewAnswer">
                            <Template>
                                <px:PXLayoutRule ID="PXLayoutRule12" runat="server" StartColumn="True" ControlSize="M" LabelsWidth="SM">
                                </px:PXLayoutRule>
                                <px:PXTextEdit ID="edAnswer" runat="server" DataField="PasswordAnswer" Required="True"/>
                                <px:PXTextEdit ID="edPassword" runat="server" DataField="Password" TextMode="Password" Required="True" />
                            </Template>
                        </px:PXFormView>
						<px:PXPanel ID="PXPanel3" runat="server" SkinID="Buttons">
						    <px:PXButton ID="btnOk3" runat="server" DialogResult="OK" Text="OK"/>
                            <px:PXButton ID="btnCancel3" runat="server" DialogResult="Cancel" Text="Cancel" />
                        </px:PXPanel>
                    </px:PXSmartPanel>
				</Template>
            </px:PXTabItem>
			<px:PXTabItem Text="Email Settings">
				<Template>
                    <px:PXFormView ID="form" runat="server" Width="100%" DataMember="UserPrefs" DataSourceID="ds" CaptionVisible="False" SkinID="Transparent">
                        <Template>
                            <px:PXLayoutRule ID="PXLayoutRule8" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                            <px:PXSelector ID="edDefaultEMailAccountID" runat="server" DataField="DefaultEMailAccountID" DataSourceID="ds"
                                DataMember="_EMailAccount_" ValueField="emailAccountID" TextField="Address" MaxLength="30" />
                        </Template>
                    </px:PXFormView>
                    <px:PXFormView ID="form2" runat="server" Width="100%" DataMember="CalendarSettings" DataSourceID="ds" CaptionVisible="False" SkinID="Transparent">
                        <Template>
                            <px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                            <px:PXCheckBox ID="chbIsPublic" runat="server" DataField="IsPublic" />
                            <px:PXButton ID="cmdCheckMailSettings" runat="server" Text="Synchronization URL"
                                OnCallBack="cmdCheckMailSettings_CallBack">
                                <AutoCallBack Command="X"></AutoCallBack>
                            </px:PXButton>
                            <px:PXLabel ID="lblMailSignature" runat="server">User Email Signature:</px:PXLabel>
                        </Template>
                    </px:PXFormView>
                    <px:PXFormView ID="PXFormView1" runat="server" Width="100%" DataMember="UserPrefs" DataSourceID="ds" CaptionVisible="False" SkinID="Transparent">
                        <Template>
                            <pxa:PXRichTextEdit ID="wikiEdit" runat="server" DataField="MailSignature" AllowLinkEditor="true" AllowLoadTemplate="true" AllowImageEditor="true" FilesContainer="UserProfile" Width="100%" Style="z-index: 113; border-width: 0px;">
                                <LoadTemplate TypeName="PX.SM.SMNotificationMaint" DataMember="Notifications" ViewName="NotificationTemplate" ValueField="notificationID" TextField="Name" DataSourceID="ds" Size="M" />
                                <ImageEditor DataSourceID="ds" DataMember="AttachFiles" ValueField="FileID" TextField="Name">
                                    <Columns>
                                        <px:PXGridColumn DataField="FileID" Visible="false" AllowShowHide="False" />
                                        <px:PXGridColumn DataField="Name" Width="400px" AllowShowHide="False" />
                                    </Columns>
                                </ImageEditor>
                                <LinkEditor TypeName="PX.SM.WikiArticlesTree" DataMember="Articles" ValueField="pageID" TextField="Title" />
                                <AutoSize Enabled="True" />
                            </pxa:PXRichTextEdit>
                        </Template>
						<AutoSize Enabled="True" />
					</px:PXFormView>
				</Template> 
			</px:PXTabItem>
			<px:PXTabItem Text="Custom Locale Format">
				<Template>
					<px:PXFormView ID="formEditFormat" runat="server" DataSourceID="ds" Style="z-index: 100"
						Width="100%" DataMember="LocaleFormats" SkinID="Transparent" DataKeyNames="FormatID"
						Caption="Locale Preferences">
						<Template>
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M"
								ColumnSpan="2" ColumnWidth="XXL" />
							<px:PXSelector CommitChanges="True" ID="edTemplateLocale" runat="server" DataField="TemplateLocale"
								DataMember="_Locale_" DataSourceID="ds" />
							<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Date and Time Formats"
								ControlSize="M" LabelsWidth="SM" StartColumn="True" StartRow="True" />
							<px:PXSelector ID="edDateTimePattern" runat="server" DataField="DateTimePattern"
								AutoRefresh="True" DataSourceID="ds" />
							<px:PXSelector ID="edTimeShortPattern" runat="server" DataField="TimeShortPattern"
								AutoRefresh="True" DataSourceID="ds" />
							<px:PXSelector ID="edTimeLongPattern" runat="server" DataField="TimeLongPattern"
								AutoRefresh="True" DataSourceID="ds" />
							<px:PXSelector ID="edDateShortPattern" runat="server" DataField="DateShortPattern"
								AutoRefresh="True" DataSourceID="ds" />
							<px:PXSelector ID="edDateLongPattern" runat="server" DataField="DateLongPattern"
								AutoRefresh="True" DataSourceID="ds" />
							<px:PXTextEdit ID="edAMDesignator" runat="server" DataField="AMDesignator" />
							<px:PXTextEdit ID="edPMDesignator" runat="server" DataField="PMDesignator" />
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M"
								GroupCaption="Number Format" StartGroup="True" />
							<px:PXDropDown CommitChanges="True" ID="edNumberDecimalSeporator" runat="server"
								AllowEdit="True" DataField="NumberDecimalSeporator" />
							<px:PXDropDown CommitChanges="True" ID="edNumberGroupSeparator" runat="server" AllowEdit="True"
								DataField="NumberGroupSeparator" />
						</Template>
					</px:PXFormView>
				</Template>
			</px:PXTabItem>
		</Items>	
		<AutoSize Container="Window" Enabled="True" MinHeight="100" MinWidth="300" />
	</px:PXTab>
</asp:Content>
