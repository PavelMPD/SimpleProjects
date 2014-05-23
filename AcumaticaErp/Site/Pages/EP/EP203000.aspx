<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="EP203000.aspx.cs"
    Inherits="Page_EP203000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Employee" TypeName="PX.Objects.EP.EmployeeMaint">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Cancel" PopupVisible="true" />
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" PopupVisible="true" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="ViewContact" Visible="False" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Employee" Caption="Employee Info"
        NoteIndicator="True" FilesIndicator="True" ActivityIndicator="True" ActivityField="NoteActivity" DefaultControlID="edAcctCD"
        TabIndex="100">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXSegmentMask ID="edAcctCD" runat="server" DataField="AcctCD" DataSourceID="ds" />
            <px:PXTextEdit ID="edAcctName" runat="server" DataField="AcctName" />
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="XS" ControlSize="S" />
            <px:PXDropDown ID="edStatus" runat="server" DataField="Status" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" Height="518px" DataSourceID="ds" DataMember="CurrentEmployee" BorderStyle="None"
        AccessKey="T">
        <Items>
            <px:PXTabItem Text="General Info">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True"  ControlSize="XM" LabelsWidth="SM" />
                    <px:PXFormView ID="ContactInfo" runat="server" Caption="Contact Info" DataMember="Contact" RenderStyle="Fieldset" DataSourceID="ds">
                        <Activity HighlightColor="" SelectedColor="" />
                        <Template>
                            <px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True"/>
                            <px:PXTextEdit ID="edDisplayName" runat="server" DataField="DisplayName" Enabled="False">
                                <LinkCommand Command="ViewContact" Target="ds"/>
                            </px:PXTextEdit>
                            <px:PXDropDown ID="edTitle" runat="server" DataField="Title" />
                            <px:PXTextEdit ID="edFirstName" runat="server" DataField="FirstName" />
                            <px:PXTextEdit ID="edMidName" runat="server" DataField="MidName" />
                            <px:PXTextEdit ID="edLastName" runat="server" DataField="LastName" />
                            <px:PXLayoutRule runat="server" Merge="True" />
                            <px:PXDropDown Size="XS" ID="edPhone1Type" runat="server" DataField="Phone1Type" />
                            <px:PXMaskEdit Width="164px" ID="edPhone1" runat="server" DataField="Phone1" SuppressLabel="True" />
                            <px:PXLayoutRule runat="server" />
                            <px:PXLayoutRule runat="server" Merge="True" />
                            <px:PXDropDown Size="XS" ID="edPhone2Type" runat="server" DataField="Phone2Type" SelectedIndex="1" />
                            <px:PXMaskEdit Width="164px" ID="edPhone2" runat="server" DataField="Phone2" SuppressLabel="True" />
                            <px:PXLayoutRule runat="server" />
                            <px:PXLayoutRule runat="server" Merge="True" />
                            <px:PXDropDown Size="XS" ID="edPhone3Type" runat="server" DataField="Phone3Type" SelectedIndex="5" />
                            <px:PXMaskEdit Width="164px" ID="edPhone3" runat="server" DataField="Phone3" SuppressLabel="True" />
                            <px:PXLayoutRule runat="server" />
                            <px:PXLayoutRule runat="server" Merge="True" />
                            <px:PXDropDown Size="XS" ID="edFaxType" runat="server" DataField="FaxType" SelectedIndex="4" />
                            <px:PXMaskEdit Width="164px" ID="edFax" runat="server" DataField="Fax" SuppressLabel="True" />
                            <px:PXLayoutRule runat="server" />
                            <px:PXMailEdit ID="edEMail" runat="server" DataField="EMail" CommitChanges="True"/>
                            <px:PXLinkEdit ID="edWebSite" runat="server" DataField="WebSite" CommitChanges="True"/>
                        </Template>
                    </px:PXFormView>
                     <px:PXFormView ID="AddressInfo" runat="server" Caption="Address info" DataMember="Address" DataSourceID="ds" RenderStyle="FieldSet">
                        <Template>
                            <px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
                            <px:PXTextEdit ID="edAddressLine1" runat="server" DataField="AddressLine1" />
                            <px:PXTextEdit ID="edAddressLine2" runat="server" DataField="AddressLine2" />
                            <px:PXTextEdit ID="edCity" runat="server" DataField="City" />
                            <px:PXSelector ID="edCountryID" runat="server" DataField="CountryID" AllowEdit="True" DataSourceID="ds"  CommitChanges="true" AutoRefresh="True" />
                            <px:PXSelector ID="edState" runat="server" DataField="State" AllowEdit="True" DataSourceID="ds" AutoRefresh="True" />
                            <px:PXMaskEdit ID="edPostalCode" runat="server" DataField="PostalCode" />
                        </Template>
                    </px:PXFormView>

                    <px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True"/>
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Employee Settings" />
                    <px:PXLayoutRule runat="server" Merge="True" />
                    <px:PXDateTimeEdit Size="s" ID="edHireDate" runat="server" DataField="HireDate" />
                    <px:PXCheckBox CommitChanges="True" ID="chkTerminated" runat="server" DataField="Terminated" />
                    <px:PXLayoutRule runat="server" />
                    <px:PXDateTimeEdit ID="edTerminationDate" runat="server" DataField="TerminationDate" />
                    <px:PXTextEdit ID="edAcctReferenceNbr" runat="server" DataField="AcctReferenceNbr" />
                    <px:PXSelector CommitChanges="True" ID="edVendorClassID" runat="server" DataField="VendorClassID" AllowEdit="True" />
                    <px:PXSegmentMask CommitChanges="True" ID="edParentBAccountID" runat="server" DataField="ParentBAccountID" AllowEdit="True" />
                    <px:PXSelector ID="edPositionID" runat="server" DataField="PositionID" AllowEdit="True" />
                    <px:PXSelector ID="edDepartmentID" runat="server" DataField="DepartmentID" AllowEdit="True" />
                    <px:PXSelector ID="edCalendarID" runat="server" DataField="CalendarID" AllowEdit="True" />
                    <px:PXDropDown ID="edHoursValidation" runat="server" AllowNull="False" DataField="HoursValidation"/>
                    <px:PXSegmentMask ID="edSupervisorID" runat="server" DataField="SupervisorID" AllowEdit="True" />
                    <px:PXSegmentMask ID="edSalesPersonID" runat="server" DataField="SalesPersonID" AutoRefresh="True" AllowEdit="True" />
                    <px:PXSelector ID="edUserID" runat="server" DataField="UserID"  AllowEdit="True" Enabled="False"/>
                    <px:PXLayoutRule runat="server" Merge="True" />
                    <px:PXSelector Size="S" ID="edCuryID" runat="server" DataField="CuryID" AllowEdit="True" />
                    <px:PXCheckBox ID="chkAllowOverrideCury" runat="server" DataField="AllowOverrideCury" />
                    <px:PXLayoutRule runat="server" />
                    <px:PXLayoutRule runat="server" Merge="True" />
                    <px:PXSelector Size="S" ID="edCuryRateTypeID" runat="server" DataField="CuryRateTypeID" AllowEdit="True" />
                    <px:PXCheckBox ID="chkAllowOverrideRate" runat="server" DataField="AllowOverrideRate" />
                    <px:PXLayoutRule runat="server" />
                    <px:PXSegmentMask ID="edLabourItemID" runat="server" DataField="LabourItemID" />
                    <px:PXCheckBox SuppressLabel="True" ID="edRouteEmails" runat="server" DataField="RouteEmails" />
                    <px:PXCheckBox SuppressLabel="True" ID="edTimeCardRequired" runat="server" DataField="TimeCardRequired" />
                    <px:PXFormView ID="PersonalInfo" runat="server" Caption="Personal info" DataMember="Contact" DataSourceID="ds" RenderStyle="FieldSet">
                        <Template>
                            <px:PXLayoutRule ID="PXLayoutRule2" runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True"/>
                            <px:PXDateTimeEdit ID="edDateOfBirth" runat="server" DataField="DateOfBirth" />
                        </Template>
                    </px:PXFormView>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="GL Accounts and Payment Settings">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" StartGroup="True" GroupCaption="GL Accounts" />
                    <px:PXFormView ID="frmPmtDefLocation" runat="server" CaptionVisible="False" DataSourceID="ds" DataMember="DefLocation" RenderStyle="Simple">
                        <Template>
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                            <px:PXSegmentMask CommitChanges="True" ID="edVAPAccountID" runat="server" DataField="VAPAccountID" DataSourceID="ds" />
                            <px:PXSegmentMask ID="edVAPSubID" runat="server" DataField="VAPSubID" DataSourceID="ds" />
                            <px:PXSelector ID="edVTaxZoneID" runat="server" DataField="VTaxZoneID" DataSourceID="ds" />
                        </Template>
                    </px:PXFormView>
                    <px:PXSegmentMask CommitChanges="True" ID="edPrepaymentAcctID" runat="server" DataField="PrepaymentAcctID" />
                    <px:PXSegmentMask ID="edPrepaymentSubID" runat="server" DataField="PrepaymentSubID" />
                    <px:PXSegmentMask CommitChanges="True" ID="edExpenseAcctID" runat="server" DataField="ExpenseAcctID" />
                    <px:PXSegmentMask ID="edExpenseSubID" runat="server" DataField="ExpenseSubID" />
                    <px:PXSegmentMask CommitChanges="True" ID="edSalesAcctID" runat="server" DataField="SalesAcctID" />
                    <px:PXSegmentMask ID="edSalesSubID" runat="server" DataField="SalesSubID" />
                    <px:PXSelector ID="edTermsID" runat="server" DataField="TermsID" AllowEdit="True" />
                    <px:PXLayoutRule runat="server" StartColumn="True" StartGroup="True" GroupCaption="Payment Settings" />
                    <px:PXFormView ID="PXFormView3" runat="server" CaptionVisible="False" DataSourceID="ds" DataMember="DefLocation" SkinID="Transparent">
                        <Template>
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                            <px:PXSelector CommitChanges="True" ID="edVPaymentMethodID" runat="server" DataField="VPaymentMethodID" AllowEdit="True"
                                DataSourceID="ds" />
                            <px:PXSegmentMask CommitChanges="True" ID="edVCashAccountID" runat="server" DataField="VCashAccountID" AllowEdit="True" DataSourceID="ds" />
                            <px:PXGrid ID="PXGrid1" runat="server" DataSourceID="ds" Caption="Payment Instructions" Width="400px" Height="160px" MatrixMode="True" SkinID="Attributes">
                                <Levels>
                                    <px:PXGridLevel DataMember="PaymentDetails" DataKeyNames="BAccountID,LocationID,PaymentMethodID,DetailID">
                                        <Columns>
                                            <px:PXGridColumn DataField="BAccountID" TextAlign="Right" />
                                            <px:PXGridColumn DataField="PaymentMethodID" />
                                            <px:PXGridColumn DataField="DetailID" TextAlign="Right" />
                                            <px:PXGridColumn DataField="DetailID_PaymentMethodDetail_descr" Width="120px" />
                                            <px:PXGridColumn DataField="DetailValue" Width="200px" />
                                        </Columns>
                                    </px:PXGridLevel>
                                </Levels>
                                <Layout HighlightMode="Cell" ColumnsMenu="False" HeaderVisible="False" />
                                <Mode AllowAddNew="False" AllowColMoving="False" AllowDelete="False" AllowSort="False" />
                                <AutoSize Enabled="True" />
                            </px:PXGrid>
                        </Template>
                    </px:PXFormView>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Mailings" LoadOnDemand="True">
                <Template>
                    <px:PXGrid runat="server" ID="gridNC" SkinID="DetailsInTab" DataSourceID="ds" Width="100%" AdjustPageSize="Auto">
                        <Mode AllowAddNew="False" />
                        <Levels>
                            <px:PXGridLevel DataMember="NWatchers" DataKeyNames="NotificationID">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXSelector ID="edNotificationID" runat="server" DataField="NotificationID" ValueField="Name" />
                                    <px:PXDropDown ID="edFormat" runat="server" DataField="Format" SelectedIndex="3" />
                                    <px:PXTextEdit ID="edEntityDescription" runat="server" DataField="EntityDescription" Enabled="False" />
                                    <px:PXSelector ID="edReportID" runat="server" DataField="ReportID" ValueField="ScreenID" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="NotificationSetup__Module" />
                                    <px:PXGridColumn DataField="NotificationSetup__SourceCD" />
                                    <px:PXGridColumn DataField="NotificationSetup__NotificationCD" Width="120px" />
                                    <px:PXGridColumn DataField="ClassID" Width="100px" />
                                    <px:PXGridColumn DataField="EntityDescription_Description" Width="200px" />
                                    <px:PXGridColumn DataField="ReportID" Width="100px" />
                                    <px:PXGridColumn DataField="TemplateID" Width="120px" />
                                    <px:PXGridColumn DataField="Format" RenderEditorText="True" Width="80px" />
                                    <px:PXGridColumn DataField="Hidden" TextAlign="Center" Type="CheckBox" Width="60px" />
                                    <px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox" Width="60px" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
			<px:PXTabItem Text="Labor Item Overrides">
				<Template>
					<px:PXGrid ID="LaborClassesGrid" runat="server" SkinID="Details" ActionsPosition="Top"
						DataSourceID="ds" Width="100%" BorderWidth="0px" MatrixMode="True">
						<Levels>
							<px:PXGridLevel DataMember="LaborMatrix">
								<Columns>
									<px:PXGridColumn DataField="EarningType" CommitChanges="True" Width="110px" />
                                    <px:PXGridColumn DataField="EPEarningType__Description" Width="200px" />
									<px:PXGridColumn DataField="LabourItemID" CommitChanges="True" Width="150px" />
                                    <px:PXGridColumn DataField="InventoryItem__BasePrice" Width="200px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
            <px:PXTabItem Text="Employee Cost">
                <Template>
                    <px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="300" SkinID="Horizontal" Height="500px">
                        <AutoSize Enabled="true" />
                        <Template1>
                            <px:PXGrid ID="gridEmployeeRates" runat="server" DataSourceID="ds" MatrixMode="True" SyncPosition="True" Height="400px" Width="100%"
                                SkinID="DetailsInTab">
                                <Levels>
                                    <px:PXGridLevel DataMember="EmployeeRates" DataKeyNames="RateID">
									    <RowTemplate>
									        <px:PXSelector ID="edPayGroupID" runat="server" DataField="PayGroupID" AllowAddNew="True" />
                                        </RowTemplate>
                                        <Columns>
                                            <px:PXGridColumn DataField="EffectiveDate" Width="100px" />
                                            <px:PXGridColumn DataField="RateType" AutoCallBack="True" Width="135px" />
                                            <px:PXGridColumn DataField="RegularHours" AutoCallBack="True" TextAlign="Right" Width="145px" />
                                            <px:PXGridColumn DataField="AnnualSalary" AutoCallBack="True" TextAlign="Right" Width="140px" />
                                            <px:PXGridColumn DataField="HourlyRate" AutoCallBack="True" TextAlign="Right" Width="80px" />
                                            <px:PXGridColumn DataField="CompensationCode" Width="130px" />
                                        </Columns>
                                    </px:PXGridLevel>
                                </Levels>
                                <AutoCallBack Target="gridEmployeeRatesByProject" Command="Refresh" />
                                <Mode InitNewRow="True" />
                                <AutoSize Enabled="True" />
                            </px:PXGrid>
                        </Template1>
                        <Template2>
                            <px:PXGrid ID="gridEmployeeRatesByProject" runat="server" DataSourceID="ds" Height="400px" Width="100%"
                                SkinID="DetailsInTab" Caption="Overrides">
                                <Levels>
                                    <px:PXGridLevel DataMember="EmployeeRatesByProject" DataKeyNames="RateID,Line">
                                        <Columns>
                                            <px:PXGridColumn DataField="ProjectID" AutoCallBack="True" Width="100px" />
                                            <px:PXGridColumn DataField="TaskID" AutoCallBack="True" Width="100px" />
                                            <px:PXGridColumn DataField="HourlyRate" AutoCallBack="True" TextAlign="Right" Width="80px" />
                                            <px:PXGridColumn DataField="CompensationCode" Width="130px" />
                                        </Columns>
                                    </px:PXGridLevel>
                                </Levels>
                                <AutoSize Enabled="True" />
                                <Mode InitNewRow="True" />
                            </px:PXGrid>
                        </Template2>
                    </px:PXSplitContainer>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Company Tree Member">
                <Template>
                    <px:PXGrid ID="companyTreeGrid" runat="server" DataSourceID="ds" Height="400px" Width="100%" SkinID="DetailsInTab">
                        <Levels>
                            <px:PXGridLevel DataMember="CompanyTree" DataKeyNames="WorkGroupID,UserID">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXMaskEdit ID="edWaitTime" runat="server" DataField="WaitTime" Text="0" />
                                    <px:PXCheckBox ID="chkIsOwner" runat="server" DataField="IsOwner" />
                                    <px:PXSelector ID="edWorkGroupID" runat="server" DataField="WorkGroupID" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="WorkGroupID" Label="Workgroup ID" Width="100px" />
                                    <px:PXGridColumn DataField="WaitTime" Label="Wait Time" Width="100px" />
                                    <px:PXGridColumn DataField="IsOwner" Label="Owner" TextAlign="Center" Type="CheckBox" Width="60px" />
                                    <px:PXGridColumn DataField="Active" Label="Active" TextAlign="Center" Type="CheckBox" Width="60px" />
                                </Columns>
                                <Layout FormViewHeight="" />
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Enabled="True" MinHeight="538" Container="Window" />
    </px:PXTab>
</asp:Content>
