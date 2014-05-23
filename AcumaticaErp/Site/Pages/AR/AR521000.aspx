<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AR521000.aspx.cs" Inherits="Page_AR521000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" 
        PrimaryView="Filter" TypeName="PX.Objects.AR.ARDunningLetterProcess">
		<CallbackCommands>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
        <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" 
		Width="100%" DataMember="Filter" ActivityField="StartDate" FilesField="" NoteField="" Caption="Selection">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="M" />

            <px:PXSelector CommitChanges="True" runat="server" InputMask="&gt;aaaaaaaaaa" DataField="CustomerClassID" MaxLength="10" DataKeyNames="CustomerClassID" DataMember="_CustomerClass_" DataSourceID="ds" ID="edCustomerClass"  />
            <px:PXDateTimeEdit CommitChanges="True" runat="server" DataField="DocDate" ID="edDocDate"  /></Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100; height: 150px;" 
		Width="100%" SkinID="Details" Caption="Customers" AllowPaging="true" 
        AdjustPageSize="Auto">
		<Levels>
			<px:PXGridLevel 
                DataMember="DunningLetterList">
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />

					<px:PXCheckBox SuppressLabel="True" ID="chkSelected" runat="server" DataField="Selected" /></RowTemplate>
			    <Columns>
					<px:PXGridColumn AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" AllowSort="False" AllowMove="False" Width="20px" AutoCallBack="true" />
                    <px:PXGridColumn DataField="CustomerClassID" Label="Customer Class" MaxLength="10" />
                    <px:PXGridColumn AllowUpdate="False" DataField="BAccountID" DisplayFormat="&gt;AAAAAAAAAA" Label="Customer" />                    
                    <px:PXGridColumn DataField="BAccountID_BAccountR_acctName" Width="140px" />		
                    <px:PXGridColumn DataField="DueDate" Label="Due Date" Width="90px" />
                    <px:PXGridColumn AllowNull="False" DataField="BranchID" Label="BranchID" TextAlign="Left" />
                    <px:PXGridColumn AllowNull="False" DataField="NumberOfDocuments" Label="Number Of Documents" TextAlign="Right" />
                    <px:PXGridColumn AllowNull="False" DataField="NumberOfOverdueDocuments" Label="Number Of Due Documents" TextAlign="Right" />
                    <px:PXGridColumn AllowNull="False" DataField="OrigDocAmt" Label="Sum" TextAlign="Right" />
                    <px:PXGridColumn AllowNull="False" DataField="DocBal" Label="BalSum" TextAlign="Right" />
                    <px:PXGridColumn AllowNull="False" DataField="DunningLetterLevel" Label="DunningLetter Level" TextAlign="Right" />
                    <px:PXGridColumn DataField="LastDunningLetterDate" Label="Last Dunning Letter Date" Width="90px" />                    
                </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXGrid>
</asp:Content>
