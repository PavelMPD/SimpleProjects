<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AR509900.aspx.cs" Inherits="Page_AR509900" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.AR.ARIntegrityCheck" PrimaryView="Filter">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Process" CommitChanges="True" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="ProcessAll" CommitChanges="True" />
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="ViewCustomer" Visible="False" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection" DefaultControlID="edCustomerClassID">
<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="S" ControlSize="M" />

			<px:PXSelector CommitChanges="True" ID="edCustomerClassID" runat="server" DataField="CustomerClassID"  />
        <px:PXSelector ID="edFinPeriodID" runat="server" DataField="FinPeriodID"  /></Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" 
		Style="z-index: 100" Width="100%" Caption="Customers" AllowPaging="true" 
		AdjustPageSize="Auto" SkinID="Inquire" AllowSearch="True" 
		FastFilterFields="AcctCD,AcctName">
		<Levels>
			<px:PXGridLevel DataMember="ARCustomerList" >
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" />

					<px:PXLayoutRule runat="server" Merge="True" />

					<px:PXCheckBox ID="chkSelected" runat="server" DataField="Selected" />
					<px:PXSegmentMask SuppressLabel="True" ID="edAcctCD" runat="server" DataField="AcctCD"  />
					<px:PXSelector SuppressLabel="True" ID="edCustomerClassID" runat="server" DataField="CustomerClassID"  />
					<px:PXLayoutRule runat="server" Merge="False" />

					<px:PXTextEdit ID="edAcctName" runat="server" DataField="AcctName"  /></RowTemplate>
				<Columns>
					<px:PXGridColumn AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" AllowSort="False" AllowMove="False" Width="20px" />
					<px:PXGridColumn DataField="AcctCD" Width="81px" />
					<px:PXGridColumn DataField="CustomerClassID" Width="81px" />
					<px:PXGridColumn DataField="AcctName" Width="297px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<ActionBar>
			<CustomItems>
                <px:PXToolBarButton Text="View Customer" Tooltip="View Customer" CommandName="ViewCustomer" CommandSourceID="ds" />
			</CustomItems>
		</ActionBar>
	</px:PXGrid>
</asp:Content>
