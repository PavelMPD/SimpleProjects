<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="EP401000.aspx.cs" Inherits="Page_EP401000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.EP.EmployeeClaimsEnq"
        PrimaryView="Filter">
		<CallbackCommands>
			<px:PXDSCallbackCommand Visible="false" DependOnGrid="grid" Name="ClaimDetails" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection" TabIndex="100">
		<Template>
            <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="S" ControlSize="M" />

			<px:PXSelector CommitChanges="True" ID="edEmployeeID" runat="server" DataField="EmployeeID" DataSourceID="ds"  />
			<px:PXDateTimeEdit CommitChanges="True" ID="edStartDate" runat="server" DataField="StartDate"  />
			<px:PXDateTimeEdit CommitChanges="True" ID="edEndDate" runat="server" DataField="EndDate"  />

			<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="XS" ControlSize="XS" />

			<px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkOnHold" runat="server" Checked="True" DataField="OnHold" />
            <px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkPending" runat="server" Checked="True" DataField="Pending" />

            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True"  LabelsWidth="XS" ControlSize="XS" />

            <px:PXCheckBox CommitChanges="True" ID="chkApproved" runat="server" DataField="Approved" />
		    <px:PXCheckBox CommitChanges="True" ID="chkVoided" runat="server" DataField="Voided" />
			<px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkReleased" runat="server" DataField="Released" />

			</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" ActionsPosition="Top" Caption="Claims" SkinID="Inquire" RestrictFields="True">
		<Levels>
			<px:PXGridLevel DataMember="ExpenseClaimRecords" >
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="XM" />

					<px:PXLayoutRule runat="server" Merge="True" />

					<px:PXSelector Size="s" ID="edRefNbr" runat="server" DataField="RefNbr"  />
					<px:PXSelector Size="xxs" ID="edCuryID" runat="server" DataField="CuryID"  />
					<px:PXLayoutRule runat="server" Merge="False" />

					<px:PXLayoutRule runat="server" Merge="True" />

					<px:PXSelector Size="s" ID="edApproverID" runat="server" DataField="ApproverID" TextField="Username"  />
					<px:PXNumberEdit Size="xs" ID="edCuryOrigDocAmt" runat="server" DataField="CuryOrigDocAmt"  />
					<px:PXLayoutRule runat="server" Merge="False" />

					<px:PXDateTimeEdit ID="edDocDate" runat="server" DataField="DocDate"  />
					<px:PXDateTimeEdit ID="edApproveDate" runat="server" DataField="ApproveDate"  />
					<px:PXTextEdit ID="edDocDesc" runat="server" DataField="DocDesc"  />
					<px:PXDropDown ID="edStatus" runat="server" AllowNull="False" DataField="Status" Enabled="False"  /></RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="DocDate" Width="90px" />
					<px:PXGridColumn DataField="RefNbr" Width="108px" />
					<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="Status" Width="108px" RenderEditorText="True" />
					<px:PXGridColumn DataField="DocDesc" Width="250px" />
					<px:PXGridColumn DataField="ApproverID" Width="108px" />
					<px:PXGridColumn DataField="ApproveDate" Width="90px" />
					<px:PXGridColumn DataField="CuryID" DisplayFormat="&gt;LLLLL" Width="54px" />
					<px:PXGridColumn AllowNull="False" DataField="CuryOrigDocAmt" TextAlign="Right" Width="81px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<ActionBar>
			<CustomItems>
				<px:PXToolBarButton Text="Claim Details" Tooltip="Claim Details">
				    <AutoCallBack Command="ClaimDetails" Target="ds">
						<Behavior CommitChanges="True" />
					</AutoCallBack>
				</px:PXToolBarButton>
			</CustomItems>
		</ActionBar>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXGrid>
</asp:Content>
