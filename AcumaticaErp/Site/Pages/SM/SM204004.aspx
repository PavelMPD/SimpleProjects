<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM204004.aspx.cs" Inherits="Pages_SM_SM204004"
	Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" Visible="True" PrimaryView="Filter"
		TypeName="PX.SM.SMNotificationExplore">
		<CallbackCommands>
			<px:PXDSCallbackCommand DependOnGrid="gridNotifications" Name="Notifications_ViewDetails" Visible="False" />	
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content> 
<asp:Content ID="Content1" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%"
		DataMember="Filter" Caption="Filter">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
			<px:PXDropDown ID="edStatus" runat="server" DataField="Status" Size="S">
				<AutoCallBack Command="Save" Target="form" />
			</px:PXDropDown>  

			<px:PXSelector ID="edCreatedByID" runat="server" DataField="CreatedByID" DataSourceID="ds" DisplayMode="Text">
				<AutoCallBack Command="Save" Target="form" />
			</px:PXSelector>
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="gridNotifications" runat="server" DataSourceID="ds" ActionsPosition="Top"
		AllowPaging="true" AdjustPageSize="Auto" AllowSearch="true" SkinID="Inquire"
		Width="100%" MatrixMode="True" FastFilterFields="Subject" FilesIndicator="False"
		NoteIndicator="False" >
		<Levels>
			<px:PXGridLevel DataMember="Notifications">
				<Columns>
					<px:PXGridColumn DataField="Name" Width="300px" LinkCommand="Notifications_ViewDetails"/>
					<px:PXGridColumn DataField="Status" Width="80px"/>  
					<px:PXGridColumn AllowUpdate="False" DataField="PublishedDateTime" Width="120px" />	
					<px:PXGridColumn AllowUpdate="False" DataField="CreatedDateTime" Width="120px" />  
					<px:PXGridColumn AllowUpdate="False" DataField="CreatedByID_Creator_Username" Width="120px" />
				</Columns>
			</px:PXGridLevel>
		</Levels> 
		<ActionBar DefaultAction="cmd_ViewDetails" PagerVisible="False">
			<CustomItems>
				<px:PXToolBarButton Key="cmd_ViewDetails" Visible="False">
					<ActionBar GroupIndex="0" />
					<AutoCallBack Command="Notifications_ViewDetails" Target="ds" />
				</px:PXToolBarButton>
			</CustomItems>
		</ActionBar>
		<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXGrid>
</asp:Content>
