<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="AR508000.aspx.cs" Inherits="Page_AR508000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.AR.ARPrintInvoices"
		PrimaryView="Filter" PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Process" CommitChanges="true" StartNewGroup="true" />
			<px:PXDSCallbackCommand Name="ProcessAll" CommitChanges="true" />
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="viewDocument" Visible="False" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100"
		Width="100%" DataMember="Filter" Caption="Selection" DefaultControlID="edAction">
<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
		<Template>			
			<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="S"  />
			<px:PXDropDown Size = "m" CommitChanges="True" ID="edAction" runat="server" AllowNull="False" DataField="Action"  />			
			<px:PXLayoutRule runat="server" Merge="True" />
			<px:PXSelector Size = "m" CommitChanges="True" ID="edOwnerID" runat="server" DataField="OwnerID"  />			
            <px:PXCheckBox CommitChanges="True" ID="chkMyOwner" runat="server" Checked="True" DataField="MyOwner" />				
			<px:PXLayoutRule runat="server" Merge="False" />			
			<px:PXLayoutRule runat="server" Merge="True" />
			<px:PXSelector CommitChanges="True" Size="m" ID="edWorkGroupID" runat="server" DataField="WorkGroupID"  />			
			<px:PXCheckBox CommitChanges="True" ID="chkMyWorkGroup" runat="server" DataField="MyWorkGroup" />			
            <px:PXLayoutRule runat="server" Merge="False" />
</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="288px" Style="z-index: 100"
		Width="100%" Caption="Documents"
		AllowPaging="true" AdjustPageSize="Auto" SkinID="Inquire">
		<Levels>
			<px:PXGridLevel DataMember="ARDocumentList" >
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />

					<px:PXCheckBox ID="chkSelected" runat="server" DataField="Selected" />
					<px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr"
						  AutoRefresh="true" AllowEdit="True">
						<Parameters>
							<px:PXControlParam ControlID="grid" Name="ARInvoice.docType" PropertyName="DataValues[&quot;DocType&quot;]"
								Type="String" />
						</Parameters>
					</px:PXSelector>
					<px:PXSegmentMask ID="edCustomerID" runat="server" DataField="CustomerID"  />
					<px:PXSelector ID="edCuryID" runat="server" DataField="CuryID"  />
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />

					<px:PXTextEdit ID="edInvoiceNbr" runat="server" DataField="InvoiceNbr"  />
					<px:PXDateTimeEdit ID="edDueDate" runat="server" DataField="DueDate" />
					<px:PXDateTimeEdit ID="edDiscDate" runat="server" DataField="DiscDate" /></RowTemplate>
				<Columns>
					<px:PXGridColumn AllowNull="False" DataField="Selected" Width="20px" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" AllowSort="False" AllowMove="False" AllowUpdate="False" AutoCallBack="True" />
					<px:PXGridColumn DataField="DocType" Type="DropDownList" Width="81px" />
					<px:PXGridColumn DataField="RefNbr" Width="108px" />
					<px:PXGridColumn DataField="DocDate" Width="90px" />
					<px:PXGridColumn DataField="FinPeriodID" />
					<px:PXGridColumn DataField="CustomerID" DisplayFormat="&gt;AAAAAAAAAA" Width="81px" AllowUpdate="False" />
					<px:PXGridColumn DataField="CustomerID_BAccountR_acctName" Width="140px" />
					<px:PXGridColumn AllowNull="False" DataField="DueDate" Width="90px" AllowUpdate="False" />					
					<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="CuryOrigDocAmt" TextAlign="Right" Width="81px" />
					<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="CuryOrigDiscAmt" TextAlign="Right" Width="81px" />
					<px:PXGridColumn DataField="CuryID" DisplayFormat="&gt;LLLLL" Width="54px" AllowUpdate="False" />
					<px:PXGridColumn AllowNull="False" DataField="InvoiceNbr" Width="90px" AllowUpdate="False" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<ActionBar>
			<CustomItems>
				<px:PXToolBarButton Text="View Document" Tooltip="View Document" CommandName="viewDocument" CommandSourceID="ds" />
			</CustomItems>
		</ActionBar>
	</px:PXGrid>
</asp:Content>
