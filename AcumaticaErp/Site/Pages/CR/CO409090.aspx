<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="CO409090.aspx.cs" Inherits="Pages_CR_CR409090"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" AutoCallBack="True" Visible="true" Width="100%"
		PrimaryView="Filter" TypeName="PX.Objects.CR.CRCommunicationDeleted" PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
			<px:PXDSCallbackCommand DependOnGrid="gridDeleted" Name="Deleted_ViewDetails" Visible="False" /> 
			<px:PXDSCallbackCommand DependOnGrid="gridDeleted" Name="reply" Visible="False" RepaintControls="All" />
			<px:PXDSCallbackCommand DependOnGrid="gridDeleted" Name="replyAll" Visible="False" RepaintControls="All" />
			<px:PXDSCallbackCommand DependOnGrid="gridDeleted" Name="forward" Visible="False" RepaintControls="All" />
			<px:PXDSCallbackCommand DependOnGrid="gridDeleted" Name="deleteRow" Visible="False"
				RepaintControls="All" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content> 

<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">	
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="Filter" 
	Caption="Selection" Style="z-index: 100" Width="100%" AllowCollapse="False">
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="XS" ControlSize="XM" SuppressLabel="True"/>
            <px:PXSelector runat="server" ID="edEmailAccountID" DataField="EmailAccountID" SuppressLabel="False"
					CommitChanges="True" DisplayMode="Text" TextMode="Search" AllowNull="True">
			<AutoCallBack Command="Save" Target="form" />	  
			</px:PXSelector>  
			<px:PXLayoutRule ID="PXLayoutRule2" runat="server" Merge="True" />
			<px:PXTextEdit Size="xl" runat="server" ID="edSearchText" DataField="SearchText">
				<AutoCallBack Command="Save" Target="form" />
			</px:PXTextEdit><px:PXButton Size="xs" runat="server" ID="btnSearch" Text="Search" Height="20">
                <AutoCallBack Command="Save" Target="form" />
            </px:PXButton>
            <px:PXLayoutRule ID="PXLayoutRule3" runat="server" Merge="False" />
        </Template>
    </px:PXFormView>
</asp:Content> 	

<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="gridDeleted" runat="server" DataSourceID="ds" ActionsPosition="Top" 
						AllowPaging="true" AdjustPageSize="Auto" AllowSearch="true" 
						SkinID="Inquire" Width="100%" MatrixMode="True" RestrictFields="True"> 
			<Levels>
				<px:PXGridLevel  DataMember="Deleted">
				<Columns>
                    <px:PXGridColumn DataField="Subject" Width="450px" LinkCommand="Deleted_ViewDetails" />
                    <px:PXGridColumn DataField="MailTo" Width="250px"/>  
					<px:PXGridColumn DataField="MailFrom" Width="250px"/>
					<px:PXGridColumn DataField="MPStatus" Width="70px" />	
					<px:PXGridColumn DataField="ProcessDate" DisplayFormat="g" Width="120px" />	 
					<px:PXGridColumn DataField="EMailAccount__Description" Width="200px"/> 
					<px:PXGridColumn DataField="Source" Width="100px" SyncVisible="False" SyncVisibility="False" Visible="False"/>
					<px:PXGridColumn DataField="Source_Description" Width="150px" SyncVisible="False" SyncVisibility="False" Visible="True" LinkCommand="viewEntity"/>
				</Columns> 
				</px:PXGridLevel>
			</Levels>

			<AutoSize Container="Parent" Enabled="True" /> 
				<ActionBar DefaultAction="cmd_ViewDetails" PagerVisible="False">
					<CustomItems>
						<px:PXToolBarButton Key="cmd_ViewDetails" Visible="False">
							<ActionBar GroupIndex="0" />	
							<AutoCallBack Command="Deleted_ViewDetails" Target="ds"/>
						</px:PXToolBarButton>
						<px:PXToolBarButton Key="cmddeleteRow" DisplayStyle="Image">
							<ActionBar GroupIndex="0" Order="5" />
							<AutoCallBack Target="ds" Command="deleteRow" />
							<PopupCommand Target="gridDeleted" Command="Refresh" />
							<Images Normal="main@Remove" />
						</px:PXToolBarButton>
						<px:PXToolBarButton Key="cmdreply" DisplayStyle="Text">
						<ActionBar GroupIndex="0" Order="6"/>	 
                		<AutoCallBack Target="ds" Command="reply" />
						<PopupCommand Target="gridDeleted" Command="Refresh" /> 
						</px:PXToolBarButton>
					
						<px:PXToolBarButton Key="cmdreplyAll" DisplayStyle="Text">
							<ActionBar GroupIndex="0" Order="7"/>	 
                		<AutoCallBack Target="ds" Command="replyAll" />
						<PopupCommand Target="gridDeleted" Command="Refresh" />  
						</px:PXToolBarButton>
					
						<px:PXToolBarButton Key="cmdforward" DisplayStyle="Text">
							<ActionBar GroupIndex="0" Order="8"/>	 
                		<AutoCallBack Target="ds" Command="forward" />
						<PopupCommand Target="gridDeleted" Command="Refresh" /> 
						</px:PXToolBarButton>
					</CustomItems>
                </ActionBar>
			<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" /> 
			<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		</px:PXGrid>
</asp:Content> 
