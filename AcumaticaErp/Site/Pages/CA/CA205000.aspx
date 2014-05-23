<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CA205000.aspx.cs" Inherits="Page_CA205000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="ProcessingCenter" TypeName="PX.Objects.CA.CCProcessingCenterMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand StartNewGroup="True" Name="ImportSettings" CommitChanges="true" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" DataMember="ProcessingCenter" Caption="Credit Card Processing Center" NoteIndicator="True" FilesIndicator="True" ActivityIndicator="True"
		ActivityField="NoteActivity" TabIndex="30100">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
			<px:PXSelector ID="edProcessingCenterID" runat="server" DataField="ProcessingCenterID" AutoRefresh="True" DataSourceID="ds" />
			<px:PXTextEdit ID="edName" runat="server" AllowNull="False" DataField="Name" />
			<px:PXSegmentMask ID="edCashAccountID" runat="server" DataField="CashAccountID" AllowEdit="true" />
			<px:PXCheckBox ID="chkIsActive" CommitChanges="True" runat="server" DataField="IsActive" />
			<px:PXSelector ID="edProcessingTypeName" runat="server" DataField="ProcessingTypeName" CommitChanges="True" DataSourceID="ds" />
			<px:PXNumberEdit ID="edOpenTranTimeout" runat="server" DataField="OpenTranTimeout" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Width="100%" Height="150px">
		<Items>
			<px:PXTabItem Text="Settings">
				<Template>
					<px:PXGrid ID="grdDetails" runat="server" DataSourceID="ds" Height="100%" Width="100%" SkinID="DetailsInTab">
						<Levels>
							<px:PXGridLevel DataMember="Details">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
									<px:PXMaskEdit ID="edDetailID" runat="server" DataField="DetailID" />
									<px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
									<px:PXTextEdit ID="edValue" runat="server" DataField="Value" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="DetailID" Width="100px" />
									<px:PXGridColumn AllowNull="False" DataField="Descr" Width="350px" />
									<px:PXGridColumn DataField="Value" Width="300px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Payment Methods">
				<Template>
					<px:PXGrid ID="grdPaymentMethods" runat="server" DataSourceID="ds" Height="100%" Width="100%" SkinID="DetailsInTab">
						<Levels>
							<px:PXGridLevel DataMember="PaymentMethods">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
									<px:PXSelector ID="edPaymentMethodID" runat="server" DataField="PaymentMethodID" AllowEdit="True" />
									<px:PXCheckBox ID="chkIsActive" runat="server" DataField="IsActive" />
									<px:PXCheckBox ID="chkIsDefault" runat="server" DataField="IsDefault" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="PaymentMethodID" Width="100px" />
									<px:PXGridColumn DataField="IsActive" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn DataField="IsDefault" TextAlign="Center" Type="CheckBox" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXTab>
</asp:Content>
