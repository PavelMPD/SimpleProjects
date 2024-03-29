<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="AP102000.aspx.cs" Inherits="Page_AP102000" Title="Vendor Access Maintenance" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.AP.APAccess"
		PrimaryView="Group">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="Delete" Visible="false" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>

<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="formGroup" runat="server" DataSourceID="ds" Style="z-index: 100"
		Width="100%"  DataMember="Group" 
        Caption="Restriction Group" DefaultControlID="edGroupName" 
        TemplateContainer="" TabIndex="100">
		
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="SM" 
                ControlSize="M" />
			<px:PXSelector ID="edGroupName" runat="server" DataField="GroupName" 
                DataSourceID="ds">
			</px:PXSelector>
			<px:PXTextEdit ID="edDescription" runat="server" DataField="Description"  />
            <px:PXDropDown ID="edGroupType" runat="server" AllowNull="False" DataField="GroupType"  />
			<px:PXCheckBox ID="chkActive" runat="server" DataField="Active" /></Template>
	</px:PXFormView>
</asp:Content>

<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server" >
	<px:PXTab ID="tab" runat="server" Height="168%" Style="z-index: 100" Width="100%" SelectedIndex="1" TabIndex="200">
		<Items>
			<px:PXTabItem Text="Users">
				<Template>
					<px:PXGrid ID="gridUsers" BorderWidth="0px" runat="server" DataSourceID="ds" Height="150px"
						Style="z-index: 100" Width="100%" AllowPaging="True" AdjustPageSize="Auto"
						AllowSearch="True" SkinID="Details" TabIndex="300" FastFilterFields="Username,Comment">
						<Levels>
							<px:PXGridLevel DataMember="Users">
								<Mode AllowAddNew="True" AllowDelete="False" />
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="SM" ControlSize="M" />
									<px:PXSelector ID="edUsername" runat="server" DataField="Username" TextField="Username" />
									<px:PXTextEdit ID="edComment" runat="server" DataField="Comment"  />
									<px:PXCheckBox ID="chkIncluded" runat="server" DataField="Included" /></RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="Included" TextAlign="Center" Type="CheckBox" Width="50px" AllowCheckAll="True" RenderEditorText="True" />
									<px:PXGridColumn DataField="Username" Width="300px" AllowUpdate="False" />
									<px:PXGridColumn DataField="Comment" Width="300px" AllowUpdate="False" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
						<Mode AllowDelete="False" />
						<EditPageParams>
							<px:PXControlParam ControlID="gridUsers" Name="Username" PropertyName="DataValues[&quot;Username&quot;]"
								Type="String" />
						</EditPageParams>
						<ActionBar>
							<Actions>
								<Delete Enabled="False" />
							</Actions>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Vendors">
				<Template>
					<px:PXGrid SkinID="Details" ID="gridVendors" BorderWidth="0px" runat="server"
						DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" AllowPaging="True"
						AdjustPageSize="Auto" ActionsPosition="Top" AllowSearch="True" TabIndex="400" FastFilterFields="AcctCD,AcctName">
						<Levels>
							<px:PXGridLevel DataMember="Vendor" >
								<Mode AllowAddNew="True" AllowDelete="False" />
								<Columns>
									<px:PXGridColumn DataField="Included" TextAlign="Center" Type="CheckBox" Width="50px" AllowCheckAll="True" RenderEditorText="True" />
									<px:PXGridColumn DataField="AcctCD" Width="100px" RenderEditorText="true" />
									<px:PXGridColumn AllowUpdate="False" DataField="Status" Width="100px" RenderEditorText="true" />
									<px:PXGridColumn AllowUpdate="False" DataField="AcctName" Width="350px" />
								</Columns>
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="SM" ControlSize="M" />
									<px:PXSegmentMask ID="edAcctCD" runat="server" DataField="AcctCD" AllowEdit="True" FilterByAllFields="True" />
									<px:PXDropDown ID="edStatus" runat="server" AllowNull="False" DataField="Status" Enabled="False"  />
									<px:PXTextEdit ID="edAcctName" runat="server" DataField="AcctName" Enabled="False"  />
									<px:PXCheckBox ID="chkVendorIncluded" runat="server" DataField="Included" /></RowTemplate>
							    <Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
						<Mode AllowDelete="False" />
						<EditPageParams>
							<px:PXControlParam ControlID="gridVendors" Name="AcctCD" PropertyName="DataValues[&quot;AcctCD&quot;]"
								Type="String" />
						</EditPageParams>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="250" MinWidth="300" />
	</px:PXTab>
</asp:Content>
