<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="CR503220.aspx.cs" Inherits="Page_CR503220"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Width="100%" AutoCallBack="True" Visible="True"
		IsDefaultDatasourceWidth="100%" TypeName="PX.Objects.CR.UpdateCaseMassProcess" PrimaryView="Items"
		PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
			<px:PXDSCallbackCommand Visible="false" DependOnGrid="gridItems" Name="Items_ViewDetails" />
			<px:PXDSCallbackCommand Visible="false" DependOnGrid="gridItems" Name="Items_BAccount_ViewDetails" />
			<px:PXDSCallbackCommand Visible="false" DependOnGrid="gridItems" Name="Items_BAccountParent_ViewDetails" />
			<px:PXDSCallbackCommand Visible="false" DependOnGrid="gridItems" Name="Items_Contact_ViewDetails" />
			<px:PXDSCallbackCommand Visible="false" DependOnGrid="gridItems" Name="Items_Location_ViewDetails" />
			<px:PXDSCallbackCommand Visible="false" DependOnGrid="gridItems" Name="Items_Contract_ViewDetails" />
			<px:PXDSCallbackCommand Visible="false" DependOnGrid="gridItems" Name="Items_Contract_CustomerID_ViewDetails" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="gridItems" runat="server" DataSourceID="ds" Width="100%" Height="150px"
		SkinID="Inquire" AdjustPageSize="Auto" AllowPaging="True" Caption="Matching Records" AutoGenerateColumns="AppendDynamic" RestrictFields="True">
		<Levels>
			<px:PXGridLevel DataMember="Items">
				<Columns>
					<px:PXGridColumn AllowCheckAll="True" AllowShowHide="False" DataField="Selected"
						TextAlign="Center" Type="CheckBox" Width="40px" />
					<px:PXGridColumn DataField="CaseCD" DisplayFormat="&gt;aaaaaaaaaa" Width="90px" LinkCommand="Items_ViewDetails"/>
					<px:PXGridColumn DataField="Subject" Width="300px"/>
					<px:PXGridColumn AllowNull="False" DataField="Status" Width="90px"/>
					<px:PXGridColumn AllowNull="False" DataField="Resolution" Width="90px"/>
					<px:PXGridColumn AllowNull="False" DataField="Severity" Width="90px"/>
					<px:PXGridColumn AllowNull="False" DataField="Priority" Width="90px"/>
					<px:PXGridColumn DataField="ETA" Width="90px"/>
					<px:PXGridColumn DataField="TimeEstimated" Width="90px"/> 	
					<px:PXGridColumn DataField="RemaininingDate" Width="90px"/>					
                    <px:PXGridColumn DataField="Age" Width="120px"/>
                    <px:PXGridColumn DataField="CRActivityStatistics__LastIncomingActivityDate" Width="120px" DisplayFormat="g" TimeMode="True" />
                    <px:PXGridColumn DataField="CRActivityStatistics__LastOutgoingActivityDate" Width="120px" DisplayFormat="g" TimeMode="True" />
                    <px:PXGridColumn DataField="LastActivity" Width="120px" DisplayFormat="g" TimeMode="True" />
                    <px:PXGridColumn DataField="LastActivityAge" Width="120px"/>
                    <px:PXGridColumn DataField="LastModified" Width="120px" DisplayFormat="g" TimeMode="True" />
					<px:PXGridColumn DataField="CaseClassID" DisplayFormat="&gt;aaaaaaaaaa" Width="80px"/>	 
					<px:PXGridColumn DataField="BAccount__AcctCD" Width="150px" LinkCommand="Items_BAccount_ViewDetails"/>
					<px:PXGridColumn DataField="BAccount__AcctName" Width="200px" />  
					<px:PXGridColumn DataField="BAccountParent__AcctCD" RenderEditorText="True" Width="150px" LinkCommand="Items_BAccountParent_ViewDetails"/>
					<px:PXGridColumn DataField="BAccountParent__AcctName" Width="200px"/>
					<px:PXGridColumn DataField="Contact__DisplayName" Width="150px" LinkCommand="Items_Contact_ViewDetails"/>                    
					<px:PXGridColumn DataField="Location__LocationCD" Width="90px" LinkCommand="Items_Location_ViewDetails"/>	
					<px:PXGridColumn DataField="Contract__ContractCD" Width="90px" LinkCommand="Items_Contract_ViewDetails"/> 
					<px:PXGridColumn DataField="Contract__Description" Width="90px"/> 
                    <px:PXGridColumn DataField="Contract__CustomerID" Width="150px" LinkCommand="Items_Contract_CustomerID_ViewDetails"/>
                    <px:PXGridColumn DataField="BAccountContract__AcctName" Width="150px"/>                    
					<px:PXGridColumn DataField="InitResponse" Width="90px"/>  
					<px:PXGridColumn DataField="TimeResolution" Width="90px"/>  
					<px:PXGridColumn DataField="TimeSpent" Width="90px"/> 
					<px:PXGridColumn DataField="OvertimeSpent" Width="90px"/>  
					<px:PXGridColumn DataField="TimeBillable" Width="90px"/>  
					<px:PXGridColumn DataField="OvertimeBillable" Width="90px"/> 
					<px:PXGridColumn DataField="WorkgroupID" Width="110px" />
					<px:PXGridColumn DataField="OwnerID" Width="110px" DisplayMode="Text"/>
					<px:PXGridColumn DataField="CreatedByID_Creator_Username" Width="110px" /> 
					<px:PXGridColumn DataField="CreatedDateTime" Width="90px"  DisplayFormat="g" TimeMode="True"/> 
					<px:PXGridColumn DataField="LastModifiedByID_Modifier_Username" Width="110px" SyncVisible="False" SyncVisibility="False" Visible="False"/>	  
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
						<px:PXGrid ID="grdFields" runat="server" Height="250px" Width="100%" DataSourceID="ds" 
							AutoAdjustColumns="true" MatrixMode="True">
							<AutoSize Enabled="true" />
                             <CallbackCommands>
                                    <Save CommitChanges="true" CommitChangesIDs="grdFields" RepaintControls="None" RepaintControlsIDs="grdFields,grdAttrs" />
                                    <FetchRow RepaintControls="None" />
                                </CallbackCommands>
							<Levels>
								<px:PXGridLevel DataMember="Fields">
									<Columns>
										<px:PXGridColumn DataField="Selected" Width="30px" Type="CheckBox" AllowSort="false"
											AllowCheckAll="true" TextAlign="Center" />
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
								<AutoSize Enabled="true" />
                                <CallbackCommands>
                                    <Save CommitChanges="true" CommitChangesIDs="grdAttrs" RepaintControls="None" />
                                    <FetchRow RepaintControls="None" />
                                </CallbackCommands>
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
