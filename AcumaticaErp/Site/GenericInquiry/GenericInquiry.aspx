<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="GenericInquiry.aspx.cs" Inherits="Page_GenericInquiry"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXGenericDataSource ID="ds" runat="server" Visible="True" Width="100%" FormID="form" GridID="grid">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand Name="Cancel" PostData="Self" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True"  />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand Name="exportIqyFile" Visible="false" />
			<px:PXDSCallbackCommand Name="exportExcelFile" Visible="false" />
			<px:PXDSCallbackCommand Name="getIqyLink" Visible="false" />
		</CallbackCommands>
	</px:PXGenericDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXGenericFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100"
		Width="100%" DataMember="Filter">
		<Parameters>
			<px:PXQueryStringParam Name="id" OnLoadOnly="True" QueryStringField="ID" Type="String" />
			<px:PXQueryStringParam Name="name" OnLoadOnly="True" QueryStringField="Name" Type="String" />
		</Parameters>
	</px:PXGenericFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" SkinID="Inquire">
		<ActionBar PagerVisible="False">
		</ActionBar>
		<Levels>
			<px:PXGridLevel DataMember="Results" DataKeyNames="Num">
				<Layout FormViewHeight=""></Layout>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXGrid>
</asp:Content>
