<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="CR209000.aspx.cs" Inherits="Page_CR209000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="OpportunityClass"
		TypeName="PX.Objects.CR.CROpportunityClassMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="ShowDetails" Visible="False" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%"
		Caption="Opportunity Class Summary" DataMember="OpportunityClass" FilesIndicator="True"
		NoteIndicator="True" ActivityIndicator="true" ActivityField="NoteActivity" DefaultControlID="edCROpportunityClassID">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="sm"
				ControlSize="XL" />
            <px:PXLayoutRule ID="PXLayoutRule3" runat="server" Merge="True"/>
			<px:PXSelector ID="edCROpportunityClassID" runat="server" DataField="CROpportunityClassID"
				Size="SM" FilterByAllFields="True" />
            <px:PXCheckBox ID="chkInternal" runat="server" DataField="IsInternal"/>
            <px:PXLayoutRule ID="PXLayoutRule4" runat="server"/>
			<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" /></Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Width="100%" Height="198px" DataSourceID="ds" DataMember="OpportunityClassProperties"
		LoadOnDemand="True">
		<Items>
			<px:PXTabItem Text="Details">
				<Template>
					<px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="sm"
						ControlSize="XM" />
					<px:PXSelector ID="edDefaultEMailAccount" runat="server" DataField="DefaultEMailAccountID"
						DisplayMode="Text" />
					<px:PXSegmentMask ID="edDiscountAcctID" runat="server" DataField="DiscountAcctID" />
					<px:PXSegmentMask ID="edDiscountSubID" runat="server" DataField="DiscountSubID" AllowEdit="true" /></Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Attributes">
				<Template>
					<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100;
						border: 0px;" Width="100%" ActionsPosition="Top" SkinID="Details"  MatrixMode="True">
						<Levels>
							<px:PXGridLevel DataMember="Mapping">
								<RowTemplate>
									<px:PXSelector CommitChanges="True" ID="edAttributeID" runat="server" DataField="AttributeID" AllowEdit="True" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="AttributeID" Width="81px" AutoCallBack="true" LinkCommand="ShowDetails" />
									<px:PXGridColumn AllowNull="False" DataField="Description" Width="351px" />
									<px:PXGridColumn DataField="SortOrder" TextAlign="Right" Width="81px" SortDirection="Ascending" />
									<px:PXGridColumn AllowNull="False" DataField="Required" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn AllowNull="True" DataField="CSAttribute__IsInternal" TextAlign="Center" Type="CheckBox" />
					                <px:PXGridColumn AllowNull="False" DataField="ControlType" Type="DropDownList" Width="81px" />
                                    <px:PXGridColumn AllowNull="True" DataField="DefaultValue" Width="100px" RenderEditorText="True" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXTab>
</asp:Content>
