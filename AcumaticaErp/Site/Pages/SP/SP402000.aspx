<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SP402000.aspx.cs" Inherits="Page_SP402000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="true" TypeName="PX.Objects.AR.ARDocumentEnq" PrimaryView="Filter" 
        PageLoadBehavior="PopulateSavedValues" style="float:left">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="PreviousPeriod" StartNewGroup="True" HideText="True" Visible="False"/>
            <px:PXDSCallbackCommand Name="NextPeriod" HideText="True" Visible="False"/>
            <px:PXDSCallbackCommand DependOnGrid="grid" Name="ViewDocument" Visible="false" />
            <px:PXDSCallbackCommand DependOnGrid="grid" Name="PayDocument" Visible="false" />
            <px:PXDSCallbackCommand Name="CreateInvoice" Visible="false" />
            <px:PXDSCallbackCommand Name="CreatePayment" Visible="false" />
            <px:PXDSCallbackCommand Name="ARBalanceByCustomerReport"/>
            <px:PXDSCallbackCommand Name="CustomerHistoryReport" Visible="False"/>
            <px:PXDSCallbackCommand Name="ARAgedPastDueReport"/>
            <px:PXDSCallbackCommand Name="ARAgedOutstandingReport" Visible="False"/>
            <px:PXDSCallbackCommand Name="ARRegisterReport" Visible="False"/>
            <px:PXDSCallbackCommand Name="PrintSelectedDocument" Visible="False"/>
            <px:PXDSCallbackCommand Name="PaySelectedDocument" Visible="False"/>
        </CallbackCommands>
    </px:PXDataSource>
    <px:PXToolBar ID="toolbar1" runat="server" SkinID="Navigation" BackColor="Transparent" CommandSourceID="ds">
        <Items>
        </Items>
        <Layout ItemsAlign="Left" />
    </px:PXToolBar>
    <div style="clear: left" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection" DefaultControlID="edCustomerID" TabIndex="1100">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="S" />	
            <px:PXTextEdit ID="edCustomerBalance" runat="server" DataField="SPCustomerBalance" Enabled="False" TextAlign="Right"/>
            <px:PXTextEdit ID="edCustomerDepositsBalance" runat="server" DataField="SPCustomerDepositsBalance" Enabled="False" TextAlign="Right"/>
            
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" SuppressLabel="true"/>
			<px:PXSegmentMask CommitChanges="True" ID="edCustomerID" runat="server" DataField="CustomerID" DataSourceID="ds" Visible="False"/>
			<%--<px:PXSelector CommitChanges="True" ID="edCuryID" runat="server" DataField="CuryID" DataSourceID="ds" />--%>
			<px:PXCheckBox CommitChanges="True" ID="chkShowOpenDocsOnly" runat="server" DataField="ShowAllDocs"/>
			
           <%-- <px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="S" />			
			<px:PXNumberEdit ID="edcuryCustomerBalance" runat="server" DataField="CuryCustomerBalance"/>
			<px:PXNumberEdit ID="edcuryCustomerDepositsBalance" runat="server" DataField="CuryCustomerDepositsBalance"/>--%>
		</Template>
		<Activity Width="" Height=""></Activity>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="153px" Style="z-index: 100" Width="100%" Caption="Documents" AllowSearch="True" AdjustPageSize="Auto" SkinID="Inquire" AllowPaging="True" 
		SyncPosition="True" RestrictFields="True">
		<Levels>
			<px:PXGridLevel DataMember="Documents">
				<RowTemplate>
					<px:PXLayoutRule ID="PXLayoutRule5" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
					</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="DisplayDocType" Type="DropDownList" Width="120px" />
					<px:PXGridColumn DataField="RefNbr" Width="120px" LinkCommand="PrintSelectedDocument"/>
					<px:PXGridColumn DataField="DocDate" Width="90px" />
					<px:PXGridColumn DataField="DueDate" Width="90px" />
					<px:PXGridColumn DataField="Status" Type="DropDownList" Width="72px" />
					<px:PXGridColumn DataField="OrigDocAmt" TextAlign="Right" Width="120px" />
					<px:PXGridColumn DataField="DocBal" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="DocDesc" Width="180px" />					
				</Columns>
                <Layout FormViewHeight=""></Layout>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<ActionBar DefaultAction="cmdViewDoc">
			<CustomItems>
				<px:PXToolBarButton Text="Print Selected Document" Key="cmdViewDoc">
				    <AutoCallBack Command="PrintSelectedDocument" Target="ds" />
				</px:PXToolBarButton>
                <px:PXToolBarButton>
				    <AutoCallBack Command="PaySelectedDocument" Target="ds" />
				</px:PXToolBarButton>
			</CustomItems>
		</ActionBar>
	</px:PXGrid>
</asp:Content>
