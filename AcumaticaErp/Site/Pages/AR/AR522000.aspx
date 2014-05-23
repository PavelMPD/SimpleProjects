<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AR522000.aspx.cs" Inherits="Page_AR522000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" 
        PrimaryView="Filter" TypeName="PX.Objects.AR.ARDunningLetterPrint">
		<CallbackCommands>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" 
		Width="100%" Caption="Selection" DataMember="Filter" NoteField="">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="S" ControlSize="M" />
            <px:PXDropDown CommitChanges="True" ID="edAction" runat="server" AllowNull="False" DataField="Action" SelectedIndex="-1"  />
            <px:PXDateTimeEdit CommitChanges="True" ID="edBeginDate" runat="server" DataField="BeginDate"  />
            <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="S" ControlSize="M" />

            <px:PXCheckBox CommitChanges="True" ID="chkShowAll" runat="server" DataField="ShowAll" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edEndDate" runat="server" DataField="EndDate"  /></Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" 
		Width="100%" Height="150px" SkinID="Details" Caption="Dunning Letters">
		<Levels>
			<px:PXGridLevel DataKeyNames="DunningLetterID" DataMember="Details">
			    <Columns>
                    <px:PXGridColumn AllowNull="False" DataField="Selected" Label="Selected" TextAlign="Center" Type="CheckBox" Width="20px" AutoCallBack="True" AllowCheckAll="True" />
                    <px:PXGridColumn AllowNull="False" DataField="BranchID" Label="BranchID" TextAlign="Left" />
                    <px:PXGridColumn DataField="CustomerId" Label="Customer" />
                    <px:PXGridColumn AllowNull="False" DataField="DunningLetterDate" Label="Dunning Letter Date" Width="90px" />
                    <px:PXGridColumn DataField="DunningLetterLevel" Label="Dunning Letter Level" TextAlign="Right" />
                    <px:PXGridColumn AllowNull="False" DataField="DocBal" Label="Overdue Balance" TextAlign="Right" Width="100px" />
                    <px:PXGridColumn DataField="LastLevel" Label="Final reminder" TextAlign="Center" Type="CheckBox" Width="60px" />
                    <px:PXGridColumn AllowNull="False" DataField="DontPrint" Label="Don't Print" TextAlign="Center" Type="CheckBox" Width="60px" />
                    <px:PXGridColumn AllowNull="False" DataField="Printed" Label="Printed" TextAlign="Center" Type="CheckBox" Width="60px" />
                    <px:PXGridColumn AllowNull="False" DataField="DontEmail" Label="Don't Email" TextAlign="Center" Type="CheckBox" Width="60px" />
                    <px:PXGridColumn AllowNull="False" DataField="Emailed" Label="Emailed" TextAlign="Center" Type="CheckBox" Width="60px" />
               </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXGrid>
</asp:Content>
