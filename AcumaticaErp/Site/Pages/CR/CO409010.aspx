<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="CO409010.aspx.cs" Inherits="Pages_CR_CR409010"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" AutoCallBack="True" Visible="true" Width="100%"
		PrimaryView="Filter" TypeName="PX.Objects.CR.CRCommunicationDraft" PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
			<px:PXDSCallbackCommand DependOnGrid="gridDraft" Name="Draft_ViewDetails" Visible="False" /> 
			<px:PXDSCallbackCommand DependOnGrid="gridDraft" Name="viewEntity" Visible="False" /> 
			<px:PXDSCallbackCommand Name="createNew" Visible="False" RepaintControls="All" />
			<px:PXDSCallbackCommand DependOnGrid="gridDraft" Name="deleteRow" Visible="False" RepaintControls="All" />
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
			</px:PXTextEdit>
            <px:PXButton Size="xs" runat="server" ID="btnSearch" Text="Search" Height="20">
                <AutoCallBack Command="Save" Target="form" />
            </px:PXButton>
            <px:PXLayoutRule ID="PXLayoutRule3" runat="server" Merge="False" />
        </Template>
    </px:PXFormView>
</asp:Content> 	

<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="gridDraft" runat="server" DataSourceID="ds" ActionsPosition="Top" 
						AllowPaging="true" AdjustPageSize="Auto" AllowSearch="true" 
						SkinID="Inquire" Width="100%" RestrictFields="True"> 
			<Levels>
				<px:PXGridLevel  DataMember="Draft">
				<Columns>
                    <px:PXGridColumn DataField="Subject" Width="450px" LinkCommand="Draft_ViewDetails" />
                    <px:PXGridColumn DataField="MailTo" Width="250px"/>	
					<px:PXGridColumn DataField="ProcessDate" DisplayFormat="g" Width="120px" />	 
					<px:PXGridColumn DataField="EMailAccount__Description" Width="200px" />  
					<px:PXGridColumn DataField="Source" Width="100px" SyncVisible="False" SyncVisibility="False" Visible="False"/>
					<px:PXGridColumn DataField="Source_Description" Width="150px" LinkCommand="viewEntity" SyncVisible="False" SyncVisibility="False" Visible="True"/>
				</Columns> 
				</px:PXGridLevel>
			</Levels>

			<AutoSize Container="Parent" Enabled="True" /> 
				<ActionBar DefaultAction="cmd_ViewDetails" PagerVisible="False">
					<CustomItems>
						<px:PXToolBarButton Key="cmd_ViewDetails" Visible="False">
							<ActionBar GroupIndex="0" />	
							<AutoCallBack Command="Draft_ViewDetails" Target="ds"/>
						</px:PXToolBarButton>
					<px:PXToolBarButton Key="cmdcreateNew" DisplayStyle="Image">
						<ActionBar GroupIndex="0" Order="4"/>	 
                	<AutoCallBack Target="ds" Command="createNew" />
                    <PopupCommand Target="gridDraft" Command="Refresh" /> 
					<Images Normal="main@AddNew" />	
					</px:PXToolBarButton> 
					<px:PXToolBarButton Key="cmddeleteRow" DisplayStyle="Image">
						<ActionBar GroupIndex="0" Order="5"/>	 
                	<AutoCallBack Target="ds" Command="deleteRow" />
                    <PopupCommand Target="gridDraft" Command="Refresh" /> 
					<Images Normal="main@Remove" />	
					</px:PXToolBarButton> 

					</CustomItems>
                </ActionBar>
			<Mode AllowUpdate="False" AllowAddNew="False"/>
			<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		</px:PXGrid>
</asp:Content> 
