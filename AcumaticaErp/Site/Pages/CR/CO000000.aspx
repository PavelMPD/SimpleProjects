<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="CO000000.aspx.cs" Inherits="Pages_CR_CO000000"
	Title="Untitled Page" %>
<%@ Register TagPrefix="px" Namespace="PX.Web.Controls" Assembly="PX.Web.Controls" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" AutoCallBack="True" Visible="true" Width="100%"
		PrimaryView="Summary" TypeName="PX.Objects.CR.CRCommunicationOverview" PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
			<px:PXDSCallbackCommand DependOnGrid="gridEmails" Name="AllEmailsGrid_ViewDetails"/>	 
			<px:PXDSCallbackCommand DependOnGrid="gridTasks" Name="AllTasksGrid_ViewDetails"/>
			<px:PXDSCallbackCommand DependOnGrid="gridEvents" Name="AllEventsGrid_ViewDetails"/>	  
			<px:PXDSCallbackCommand DependOnGrid="gridApproval" Name="AllApprovalsGrid_ViewDetails"/>
				 
			<px:PXDSCallbackCommand Name="getMoreEmails" Visible="False"/>
			<px:PXDSCallbackCommand Name="getMoreEvents" Visible="False"/>
			<px:PXDSCallbackCommand Name="getMoreTasks" Visible="False"/>	  
			<px:PXDSCallbackCommand Name="getMoreApprovals" Visible="False"/>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content> 	

<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="Summary" 
	Caption="Selection" Width="100%" Height="100%" AllowCollapse="False">
        <Template> 
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" ControlSize="XXS" LabelsWidth="M"  StartColumn="True" />
			<px:PXPanel ID="panelemails" runat="server" RenderStyle="Simple" style="background-color: White; border:1px solid #BBBBBB; padding: 10px">
			<px:PXLayoutRule ID="PXLayoutRule2" runat="server" ControlSize="XXS" LabelsWidth="s" SuppressLabel="True" StartColumn="True" />
			<px:PXLayoutRule ID="PXLayoutRule3" runat="server" Merge="True" SuppressLabel="true"/>
        		<px:PXLabel ID="PXLabel2" runat="server" Text="Emails" style="margin-top: 2px;  font-weight: bold"/>
				<px:PXTextEdit ID="PXTextEdit1" runat="server" DataField="Emails" Enabled="False" SkinID="Label" SuppressLabel="true" style="margin-left: 150px;  font-weight: bold" Size="35" Width="35"/>
			<px:PXLayoutRule ID="PXLayoutRule4" runat="server" />
			<px:PXGrid ID="gridEmails" runat="server" DataSourceID="ds" OnRowDataBound="grid_RowDataBound"
				ActionsPosition="Top" AdjustPageSize="Auto" AllowSearch="True" Height="120" 
						SkinID="Attribute" Width="250px" MatrixMode="True" NoteIndicator="False" 
				FilesIndicator="False" CssClass="GridMainTransparent" TabIndex="2100"> 
				<Layout RowSelectorsVisible="False" HeaderVisible="False"/>
			<Levels>
				<px:PXGridLevel  DataMember="AllEmailsGrid" DataKeyNames="TaskID">
				<Columns>
                    <px:PXGridColumn DataField="Subject" Width="250px" LinkCommand="AllEmailsGrid_ViewDetails" />
				</Columns>
				</px:PXGridLevel>
			</Levels> 
			<LevelStyles>
				<Row CssClass="GridTransparentRow" />
				<AlternateRow CssClass="GridTransparentRow" />
				<SelectedRow CssClass="GridTransparentRow" />
				<ActiveRow CssClass="GridTransparentRow" />	
				<ActiveCell CssClass="GridTransparentRow" />  
			</LevelStyles>
				<ActionBar DefaultAction="cmd_ViewAllEmailsDetails" ActionsVisible="False">
					<CustomItems>
						<px:PXToolBarButton Key="cmd_ViewAllEmailsDetails">
							<ActionBar GroupIndex="0" />	
							<AutoCallBack Command="AllEmailsGrid_ViewDetails" Target="ds"/>
						</px:PXToolBarButton> 
						<px:PXToolBarButton Key="cmd_GetMoreEmails">
							<Images Normal="main@AddNew" />  
							<ActionBar GroupIndex="2" />	
							<AutoCallBack Command="getMoreEmails" Target="ds"/>
						</px:PXToolBarButton>
					</CustomItems>
					<Actions>
						<FilterSet Enabled="False" />
						<FilterShow Enabled="False" />
						<AdjustColumns Enabled="False"/> 
						<ExportExcel Enabled="False"/> 
					</Actions>
                </ActionBar>
			<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" /> 
			<AutoSize Enabled="False" MinHeight="120"/> 
			</px:PXGrid>
			<px:PXButton ID="PXButton2" runat="server" CommandName="getMoreEmails" CommandSourceID="ds"
								Text="more.." Height="20" CssClass="ButtonLink" style="margin-left: 100px">
        	<Styles>
				<Hover CssClass="ButtonLinkHover" />
				<Pushed CssClass="ButtonLink" />
				<DropDown CssClass="ButtonLink" />
				<Disabled CssClass="ButtonLink" />
				<DropNormal CssClass="ButtonLink" />
				<DropHover CssClass="ButtonLink" />
			</Styles>
        	</px:PXButton>
			</px:PXPanel>

			<px:PXPanel ID="PXPanelApprovals" runat="server" RenderStyle="Simple" style="background-color: White; border:1px solid #BBBBBB; padding: 10px">
			<px:PXLayoutRule ID="PXLayoutRule6" runat="server" ControlSize="XXS" LabelsWidth="s" SuppressLabel="True" StartColumn="True" />
			<px:PXLayoutRule ID="PXLayoutRule17" runat="server" Merge="True" SuppressLabel="true"/>
        		<px:PXLabel ID="PXLabel3" runat="server" Text="Approvals" style="margin-top: 2px;  font-weight: bold"/>
				<px:PXTextEdit ID="PXTextEdit3" runat="server" DataField="Approvals" Enabled="False" SkinID="Label" SuppressLabel="true" style="margin-left: 150px;  font-weight: bold" Size="35" Width="35"/>
			<px:PXLayoutRule ID="PXLayoutRule18" runat="server" />
			<px:PXGrid ID="gridApproval" runat="server" DataSourceID="ds" OnRowDataBound="grid_RowDataBound"
				ActionsPosition="Top" AdjustPageSize="Auto" AllowSearch="True" Height="120" 
						SkinID="Attribute" Width="250px" MatrixMode="True" NoteIndicator="False" 
				FilesIndicator="False" CssClass="GridMainTransparent" TabIndex="2100"> 
				<Layout RowSelectorsVisible="False" HeaderVisible="False"/>
			<Levels>
				<px:PXGridLevel  DataMember="AllApprovalsGrid" DataKeyNames="TaskID">
				<Columns>
                    <px:PXGridColumn DataField="DocDesc" Width="250px" LinkCommand="AllApprovalsGrid_ViewDetails" />
				</Columns>
				</px:PXGridLevel>
			</Levels> 
			<LevelStyles>
				<Row CssClass="GridTransparentRow" />
				<AlternateRow CssClass="GridTransparentRow" />
				<SelectedRow CssClass="GridTransparentRow" />
				<ActiveRow CssClass="GridTransparentRow" />	
				<ActiveCell CssClass="GridTransparentRow" />  
			</LevelStyles>
				<ActionBar DefaultAction="cmd_ViewAllApprovalsDetails" ActionsVisible="False">
					<CustomItems>
						<px:PXToolBarButton Key="cmd_ViewAllApprovalsDetails">
							<ActionBar GroupIndex="0" />	
							<AutoCallBack Command="AllApprovalsGrid_ViewDetails" Target="ds"/>
						</px:PXToolBarButton> 
						<px:PXToolBarButton Key="cmd_GetMoreApprovals">
							<Images Normal="main@AddNew" />  
							<ActionBar GroupIndex="2" />	
							<AutoCallBack Command="getMoreApprovals" Target="ds"/>
						</px:PXToolBarButton>
					</CustomItems>
					<Actions>
						<FilterSet Enabled="False" />
						<FilterShow Enabled="False" />
						<AdjustColumns Enabled="False"/> 
						<ExportExcel Enabled="False"/> 
					</Actions>
                </ActionBar>
			<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" /> 
			<AutoSize Enabled="False" MinHeight="120"/> 
			</px:PXGrid>
			<px:PXButton ID="PXButton3" runat="server" CommandName="getMoreApprovals" CommandSourceID="ds"
								Text="more.." Height="20" CssClass="ButtonLink" style="margin-left: 100px">
        	<Styles>
				<Hover CssClass="ButtonLinkHover" />
				<Pushed CssClass="ButtonLink" />
				<DropDown CssClass="ButtonLink" />
				<Disabled CssClass="ButtonLink" />
				<DropNormal CssClass="ButtonLink" />
				<DropHover CssClass="ButtonLink" />
			</Styles>
        	</px:PXButton>
			</px:PXPanel> 	
			
			
			

			
			<px:PXLayoutRule ID="PXLayoutRule7" runat="server" ControlSize="XXS" LabelsWidth="SM" StartColumn="True"/>
			<px:PXPanel ID="PXPanel2" runat="server" RenderStyle="Simple" style="background-color: White; border:1px solid #BBBBBB; padding: 10px " >
			<px:PXLayoutRule ID="PXLayoutRule8" runat="server" ControlSize="XXS" LabelsWidth="s" SuppressLabel="True" StartColumn="True" />
			<px:PXLayoutRule ID="PXLayoutRule9" runat="server" Merge="True"/>
        		<px:PXLabel ID="label1" runat="server" Text="Tasks" style="margin-top: 2px;  font-weight: bold"/>
				<px:PXTextEdit ID="PXTextEdit2" runat="server" DataField="Tasks" Enabled="False" SkinID="Label" SuppressLabel="true" style="margin-left: 150px; font-weight: bold" Size="35" Width="35"/>
			<px:PXLayoutRule ID="PXLayoutRule10" runat="server" /> 
			<px:PXGrid ID="gridTasks" runat="server" DataSourceID="ds" OnRowDataBound="grid_RowDataBound"  
				ActionsPosition="Top" AdjustPageSize="Auto" AllowSearch="True"
						SkinID="Details" Width="250px" Height="120" MatrixMode="True" 
						NoteIndicator="False" FilesIndicator="False" CssClass="GridMainTransparent">
				<Layout RowSelectorsVisible="False" HeaderVisible="False"/>
			<Levels>
				<px:PXGridLevel  DataMember="AllTasksGrid" DataKeyNames="TaskID">
				<Columns>
                    <px:PXGridColumn DataField="Subject" Width="250px" LinkCommand="AllTasksGrid_ViewDetails"/>
				</Columns>
				</px:PXGridLevel>
			</Levels>  
			<LevelStyles>
				<Row CssClass="GridTransparentRow" />
				<AlternateRow CssClass="GridTransparentRow" />
				<SelectedRow CssClass="GridTransparentRow" />
				<ActiveRow CssClass="GridTransparentRow" />	  
				<ActiveCell CssClass="GridTransparentRow" />
			</LevelStyles>
			<ActionBar DefaultAction="cmd_ViewAllTasksDetails" ActionsVisible="False">
				<CustomItems>
					<px:PXToolBarButton Key="cmd_ViewAllTasksDetails">
						<ActionBar GroupIndex="2" />	
						<AutoCallBack Command="AllTasksGrid_ViewDetails" Target="ds"/>
					</px:PXToolBarButton> 
					<px:PXToolBarButton Key="cmd_GetMoreTasks">
						<Images Normal="main@AddNew" />	 
						<ActionBar GroupIndex="2" />	
						<AutoCallBack Command="getMoreTasks" Target="ds"/>
					</px:PXToolBarButton>
				</CustomItems>
				<Actions>
					<FilterSet Enabled="False" />
					<FilterShow Enabled="False" />
					<AdjustColumns Enabled="False"/> 
					<ExportExcel Enabled="False"/> 
				</Actions>
            </ActionBar>
			<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" /> 
			<AutoSize Enabled="False" MinHeight="120"/>
			</px:PXGrid>
        	<px:PXButton ID="btnmoretasks" runat="server" CommandName="getMoreTasks" CommandSourceID="ds"
								Text="more.." Height="20" CssClass="ButtonLink" style="margin-left: 100px">
        	<Styles>
				<Hover CssClass="ButtonLinkHover" />
				<Pushed CssClass="ButtonLink" />
				<DropDown CssClass="ButtonLink" />
				<Disabled CssClass="ButtonLink" />
				<DropNormal CssClass="ButtonLink" />
				<DropHover CssClass="ButtonLink" />
			</Styles>
        	</px:PXButton>
			</px:PXPanel>


			<px:PXLayoutRule ID="PXLayoutRule13" runat="server" ControlSize="XXS" LabelsWidth="SM" StartColumn="True" /> 
			<px:PXPanel ID="PXPanel1" runat="server" RenderStyle="Simple" style="background-color: White; border:1px solid #BBBBBB; padding: 10px ">
			<px:PXLayoutRule ID="PXLayoutRule14" runat="server" ControlSize="XXS" LabelsWidth="s" SuppressLabel="True" StartColumn="True" />
			<px:PXLayoutRule ID="PXLayoutRule15" runat="server" Merge="True"/>
        		<px:PXLabel ID="PXLabel1" runat="server" Text="Events" style="margin-top: 2px;  font-weight: bold"/>
				<px:PXTextEdit ID="PXTextEdit6" runat="server" DataField="Events" Enabled="False" SkinID="Label" SuppressLabel="true" style="margin-left: 150px; font-weight: bold" Size="35" Width="35"/>
			<px:PXLayoutRule ID="PXLayoutRule16" runat="server" />
			<px:PXGrid ID="gridEvents" runat="server" DataSourceID="ds" OnRowDataBound="grid_RowDataBound"
				ActionsPosition="Top" AdjustPageSize="Auto" AllowSearch="True"
						SkinID="Details" Width="250px" Height="120" MatrixMode="True" 
						NoteIndicator="False" FilesIndicator="False" CssClass="GridMainTransparent">
				<Layout RowSelectorsVisible="False" HeaderVisible="False"/>
			<Levels>
				<px:PXGridLevel DataMember="AllEventsGrid" DataKeyNames="TaskID">
				<Columns>
                    <px:PXGridColumn DataField="Subject"  Width="250px" LinkCommand="AllEventsGrid_ViewDetails"/>
				</Columns> 
				</px:PXGridLevel>
			</Levels>
			<LevelStyles>
				<Row CssClass="GridTransparentRow" />
				<AlternateRow CssClass="GridTransparentRow" />
				<SelectedRow CssClass="GridTransparentRow" />
				<ActiveRow CssClass="GridTransparentRow" />	
				<ActiveCell CssClass="GridTransparentRow" />  
			</LevelStyles>
			<ActionBar DefaultAction="cmd_ViewAllEventsDetails" ActionsVisible="False">
				<CustomItems>
					<px:PXToolBarButton Key="cmd_ViewAllEventsDetails">
						<ActionBar GroupIndex="0" />	
						<AutoCallBack Command="AllEventsGrid_ViewDetails" Target="ds"/> 
					</px:PXToolBarButton> 
					<px:PXToolBarButton Key="cmd_GetMoreEvents">
						<Images Normal="main@AddNew" />  
						<ActionBar GroupIndex="2" />	
						<AutoCallBack Command="getMoreEvents" Target="ds"/>
					</px:PXToolBarButton>
				</CustomItems>
				<Actions>
					<FilterSet Enabled="False" />
					<FilterShow Enabled="False" />
					<AdjustColumns Enabled="False"/> 
					<ExportExcel Enabled="False"/> 
				</Actions>
            </ActionBar>
			<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False"/> 
			<AutoSize Enabled="False" MinHeight="120" />
			</px:PXGrid> 
			<px:PXButton ID="PXButton1" runat="server" CommandName="getMoreEvents" CommandSourceID="ds"
								Text="more.." Height="20" CssClass="ButtonLink" style="margin-left: 100px">
        	<Styles>
				<Hover CssClass="ButtonLinkHover" />
				<Pushed CssClass="ButtonLink" />
				<DropDown CssClass="ButtonLink" />
				<Disabled CssClass="ButtonLink" />
				<DropNormal CssClass="ButtonLink" />
				<DropHover CssClass="ButtonLink" />
			</Styles>
        	</px:PXButton> 
			</px:PXPanel>  
			
		</Template>
		<AutoSize Container="Window" Enabled="True" MinHeight="400"/>
    </px:PXFormView>  
</asp:Content>