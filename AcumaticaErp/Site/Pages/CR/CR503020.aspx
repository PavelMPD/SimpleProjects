<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="CR503020.aspx.cs" Inherits="Page_CR503020"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" AutoCallBack="True" Visible="True" Width="100%"
        PrimaryView="Items" TypeName="PX.Objects.CR.UpdateLeadMassProcess" PageLoadBehavior="PopulateSavedValues">
        <CallbackCommands>
            <px:PXDSCallbackCommand DependOnGrid="grdItems" CommitChanges="True" Name="Items_ViewDetails" Visible="False" />
            <px:PXDSCallbackCommand DependOnGrid="grdItems" CommitChanges="True" Name="Items_BAccount_ViewDetails" Visible="False" />
			<px:PXDSCallbackCommand Visible="false" DependOnGrid="gridItems" Name="Items_BAccountParent_ViewDetails" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="grdItems" runat="server" DataSourceID="ds" Height="150px" Width="100%"
        ActionsPosition="Top" Caption="Matching Records" AllowPaging="True" AdjustPageSize="auto" SkinID="Inquire" FastFilterFields="DisplayName,FullName" AutoGenerateColumns="AppendDynamic" RestrictFields="True">
        <Levels>
            <px:PXGridLevel DataMember="Items">
                <Columns>
                    <px:PXGridColumn AllowCheckAll="True" AllowShowHide="False" DataField="Selected"
                        TextAlign="Center" Type="CheckBox" Width="40px" AutoCallBack="True" />
					<px:PXGridColumn DataField="DisplayName" Width="130px" LinkCommand="Items_ViewDetails" />
					<px:PXGridColumn DataField="Title" Width="50px" />
					<px:PXGridColumn DataField="FirstName" Width="100px" />
					<px:PXGridColumn DataField="LastName" Width="100px" />
					<px:PXGridColumn DataField="Salutation" Width="180px" />
                    <px:PXGridColumn DataField="DuplicateStatus" Width="200px" />
					<px:PXGridColumn DataField="BAccount__AcctCD" Width="150px" LinkCommand="Items_BAccount_ViewDetails" />
					<px:PXGridColumn DataField="FullName" Width="200px" />
					<px:PXGridColumn DataField="BAccountParent__AcctCD" Width="150px" LinkCommand="Items_BAccountParent_ViewDetails" />
					<px:PXGridColumn DataField="BAccountParent__AcctName" Width="200px" />
					<px:PXGridColumn DataField="IsActive" Width="60px" Type="CheckBox"/>
					<px:PXGridColumn DataField="Status" Width="90px" />
					<px:PXGridColumn DataField="Resolution" Width="90px" />
					<px:PXGridColumn DataField="ClassID" Width="90px" />
					<px:PXGridColumn DataField="Source" Width="90px" />
					<px:PXGridColumn DataField="State__name" Width="90px" />
					<px:PXGridColumn DataField="Address__CountryID" Width="90px" />
					<px:PXGridColumn DataField="Address__City" Width="90px" />
					<px:PXGridColumn DataField="Address__PostalCode" Width="90px" />
					<px:PXGridColumn DataField="Address__AddressLine1" Width="300px" />
					<px:PXGridColumn DataField="Address__AddressLine2" Width="300px" />
					<px:PXGridColumn DataField="EMail" Width="190px" />
					<px:PXGridColumn DataField="Phone1" DisplayFormat="+#(###) ###-####" Width="130px" />
					<px:PXGridColumn DataField="Phone2" DisplayFormat="+#(###) ###-####" Width="130px" />
					<px:PXGridColumn DataField="Phone3" DisplayFormat="+#(###) ###-####" Width="130px" />
					<px:PXGridColumn DataField="Fax" DisplayFormat="+#(###) ###-####" Width="130px" />
					<px:PXGridColumn DataField="WorkgroupID" Width="90px" />
					<px:PXGridColumn DataField="OwnerID" Width="90px" DisplayMode="Text" />
                    <px:PXGridColumn DataField="CRActivityStatistics__LastIncomingActivityDate" Width="120px" DisplayFormat="g" TimeMode="True" />
                    <px:PXGridColumn DataField="CRActivityStatistics__LastOutgoingActivityDate" Width="120px" DisplayFormat="g" TimeMode="True" />
					<px:PXGridColumn DataField="CreatedByID_Creator_Username" Width="100px" SyncVisible="False" SyncVisibility="False" Visible="False" />
					<px:PXGridColumn DataField="CreatedDateTime" Width="90px" />
					<px:PXGridColumn DataField="LastModifiedByID_Modifier_Username" Width="100px" Visible="False" SyncVisible="False" SyncVisibility="False"/>
					<px:PXGridColumn DataField="LastModifiedDateTime" Width="90px" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <ActionBar PagerVisible="False"/>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
    </px:PXGrid>
	<px:PXSmartPanel ID="spUpdateParamsDlg" runat="server" Width="500px" Height="350px" Caption="Values for Update" 
		CaptionVisible="True" Key="Fields" LoadOnDemand="True" ShowAfterLoad="true"
		AutoCallBack-Enabled="true" AutoCallBack-Target="PXWizard1" AutoCallBack-Command="Refresh"
		CallBackMode-CommitChanges="True" CallBackMode-PostData="Page"	>
		<px:PXWizard ID="PXWizard1" runat="server" Width="100%" Height="240px" DataMember="Fields" SkinID="Flat">
			<AutoSize Enabled="true" />
            <Pages>
                <px:PXWizardPage>
                    <Template>
						<px:PXPanel ID="formPanel" runat="server" RenderStyle="Simple" ContentLayout-OuterSpacing="Around">
                                <px:PXLayoutRule ID="PXLayoutRule11" runat="server" StartColumn="True" />
                                <px:PXLabel runat="server" ID="lblStep1" Width="370px" Style="font-weight: bold">Step 1 of 2 - Please select the fields you want to update</px:PXLabel>
						</px:PXPanel>
                                <px:PXGrid ID="grdFields" runat="server" Height="250px" Width="100%" DataSourceID="ds" AutoAdjustColumns="true" MatrixMode="True">
                                 <CallbackCommands>
                                    <Save CommitChanges="true" CommitChangesIDs="grdFields" RepaintControls="None" RepaintControlsIDs="grdFields,grdAttrs" />
                                    <FetchRow RepaintControls="None" />
                                </CallbackCommands>
							    <AutoSize Enabled="true" />
                                    <Levels>
                                        <px:PXGridLevel DataMember="Fields">
                                            <Columns>
                                                <px:PXGridColumn DataField="Selected" Width="30px" Type="CheckBox" AllowSort="false" AllowCheckAll="true" TextAlign="Center" />
										        <px:PXGridColumn DataField="DisplayName" Width="150px" />
                                                <px:PXGridColumn DataField="Value" Width="190px" RenderEditorText="true" AutoCallBack="True" />
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
                                </px:PXGrid>
                            </Template>
                </px:PXWizardPage>
                <px:PXWizardPage>
                    <Template>
						<px:PXPanel ID="formPanel" runat="server" RenderStyle="Simple" ContentLayout-OuterSpacing="Around">
                                <px:PXLayoutRule ID="PXLayoutRule11" runat="server" StartColumn="True" />
                                <px:PXLabel runat="server" ID="lblStep1" Width="370px" Style="font-weight: bold">Step 2 of 2 - Please select the attribute values</px:PXLabel>
						</px:PXPanel>
                                <px:PXGrid ID="grdAttrs" runat="server" Height="250px" Width="100%" DataSourceID="ds"
								AutoAdjustColumns="true" MatrixMode="True">
                                 <CallbackCommands>
                                    <Save CommitChanges="true" CommitChangesIDs="grdAttrs" RepaintControls="None" />
                                    <FetchRow RepaintControls="None" />
                                </CallbackCommands>
								<AutoSize Enabled="true" />
                                    <Levels>
                                        <px:PXGridLevel DataMember="Attributes">
                                            <Columns>
                                                <px:PXGridColumn DataField="Selected" Width="30px" Type="CheckBox" AllowSort="false"
												AllowCheckAll="true" TextAlign="Center" />
											<px:PXGridColumn DataField="DisplayName" Width="150px" />
                                                <px:PXGridColumn DataField="Value" Width="190px" RenderEditorText="true" AutoCallBack="True" />
                                                <px:PXGridColumn DataField="Required" Width="70px" Type="CheckBox" AllowSort="false"/>
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
                                </px:PXGrid>
                            </Template>
                </px:PXWizardPage>
            </Pages>
        </px:PXWizard>
    </px:PXSmartPanel>
</asp:Content>
