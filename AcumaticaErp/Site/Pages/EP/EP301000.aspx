<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="EP301000.aspx.cs"
    Inherits="Page_EP301000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.EP.ExpenseClaimEntry" PrimaryView="ExpenseClaim">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Cancel" PopupVisible="true" />
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" PopupVisible="True" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand Visible="False" Name="CurrencyView" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="ExpenseClaim" Caption="Document Summary"
        NoteIndicator="True" FilesIndicator="True" ActivityIndicator="True" ActivityField="NoteActivity" LinkIndicator="True"
        NotifyIndicator="True" EmailingGraph="PX.Objects.CR.CREmailActivityMaint,PX.Objects" DefaultControlID="edRefNbr" TabIndex="100">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
            <px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" AutoRefresh="True" DataSourceID="ds" DisplayMode="Value" />
            <px:PXDropDown Size="s" ID="edStatus" runat="server" DataField="Status" Enabled="False" />
            <px:PXCheckBox CommitChanges="True" ID="chkHold" runat="server" DataField="Hold" />
            <px:PXDateTimeEdit Size="s" ID="edDocDate" runat="server" DataField="DocDate" CommitChanges="True"/>
            <px:PXDateTimeEdit CommitChanges="True" Size="s" ID="edApproveDate" runat="server" DataField="ApproveDate" />
            <px:PXSelector CommitChanges="True" ID="edFinPeriodID" runat="server" DataField="FinPeriodID" DataSourceID="ds" />
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edDocDesc" runat="server" DataField="DocDesc" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXSegmentMask CommitChanges="True" ID="edEmployeeID" runat="server" DataField="EmployeeID" DataSourceID="ds" />
            <pxa:PXCurrencyRate DataField="CuryID" ID="edCuryID" runat="server" DataSourceID="ds" RateTypeView="_EPExpenseClaim_CurrencyInfo_"
                DataMember="_Currency_" />
            <px:PXSegmentMask ID="edLocationID" runat="server" DataField="LocationID" AutoRefresh="True" DataSourceID="ds" />
            <px:PXSelector ID="edDepartmentID" runat="server" DataField="DepartmentID" Enabled="False" DataSourceID="ds" />
            <px:PXCheckBox CommitChanges="True" ID="chkApproved" runat="server" DataField="Approved" />
            <px:PXCheckBox CommitChanges="True" ID="chkRejected" runat="server" DataField="Rejected" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXNumberEdit ID="edCuryDocBal" runat="server" DataField="CuryDocBal" />
            <px:PXNumberEdit ID="edCuryOrigDocAmt" runat="server" DataField="CuryOrigDocAmt" CommitChanges="True" />
            <px:PXNumberEdit ID="edCuryVatExemptTotal" runat="server" DataField="CuryVatExemptTotal" />
            <px:PXNumberEdit ID="edCuryVatTaxableTotal" runat="server" DataField="CuryVatTaxableTotal" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Height="400px" Style="z-index: 100;" Width="100%" DataMember="ExpenseClaimCurrent" DataSourceID="ds">
        <Items>
            <px:PXTabItem Text="Expense Claim Details">
                <Template>
                    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="100%" Style="z-index: 100; left: 0px; top: 0px;" Width="100%"
                        ActionsPosition="Top" BorderWidth="0px" SkinID="Details" TabIndex="1100">
                        <CallbackCommands>
                            <Save PostData="Container" />
                        </CallbackCommands>
                        <Levels>
                            <px:PXGridLevel DataMember="ExpenseClaimDetails" DataKeyNames="RefNbr,LineNbr">
                                <Columns>
                                    <px:PXGridColumn DataField="RefNbr" Visible="False" />
                                    <px:PXGridColumn DataField="LineNbr" TextAlign="Right" Visible="False" />
                                    <px:PXGridColumn DataField="ExpenseDate" TextCase="Upper" />
                                    <px:PXGridColumn DataField="ExpenseRefNbr" Width="60px" />
                                    <px:PXGridColumn DataField="InventoryID" Width="80px" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="Qty" TextAlign="Right" Width="50px" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="UOM" Width="40px" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="CuryUnitCost" TextAlign="Right" Width="60px" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="CuryExtCost" TextAlign="Right" Width="80px" />
                                    <px:PXGridColumn DataField="CuryEmployeePart" TextAlign="Right" Width="60px" />
                                    <px:PXGridColumn DataField="CuryTranAmt" TextAlign="Right" Width="60px" />
                                    <px:PXGridColumn DataField="TranDesc" />
                                    <px:PXGridColumn DataField="Billable" TextAlign="Center" Type="CheckBox" AutoCallBack="True" TextCase="Upper" Width="40px" />
                                    <px:PXGridColumn DataField="CustomerID" Width="80px" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="CustomerLocationID" Width="45px" />
                                    <px:PXGridColumn DataField="ContractID" Width="60px" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="TaskID" Label="Task" Width="54px" AutoCallBack="True"/>
                                    <px:PXGridColumn DataField="CustomerID_BAccountR_acctName" Width="80px" />
                                    <px:PXGridColumn DataField="ExpenseAccountID" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="ExpenseSubID" Width="100px" />
                                    <px:PXGridColumn DataField="SalesAccountID" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="SalesSubID" Width="100px" />
                                    <px:PXGridColumn DataField="TaxCategoryID" Width="50px" />
                                </Columns>
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
                                    <px:PXDateTimeEdit ID="edExpenseDate" runat="server" DataField="ExpenseDate" />
                                    <px:PXTextEdit ID="edExpenseRefNbr" runat="server" DataField="ExpenseRefNbr" />
                                    <px:PXSegmentMask CommitChanges="True" ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True" Size="XM" />
                                    <px:PXSelector ID="edUOM" runat="server" DataField="UOM" AutoRefresh="True" />
                                    <px:PXNumberEdit ID="edQty" runat="server" DataField="Qty" />
                                    <px:PXNumberEdit ID="edCuryUnitCost" runat="server" DataField="CuryUnitCost" />
                                    <px:PXNumberEdit ID="edCuryTotalAmount" runat="server" DataField="CuryExtCost" Enabled="False" />
                                    <px:PXNumberEdit ID="edCuryTranAmt" runat="server" DataField="CuryTranAmt" />
                                    <px:PXNumberEdit ID="edCuryEmployeePart" runat="server" DataField="CuryEmployeePart" />
                                    <px:PXLayoutRule runat="server" ColumnSpan="2" />
                                    <px:PXTextEdit ID="edTranDesc" runat="server" DataField="TranDesc" />
                                    <px:PXSegmentMask CommitChanges="True" ID="edCustomerID" runat="server" DataField="CustomerID" Size="XM" />
                                    <px:PXSegmentMask Size="XM" ID="edCustomerLocationID" runat="server" DataField="CustomerLocationID" AutoRefresh="True" />
                                    <px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="S" StartColumn="True" />
                                    <px:PXCheckBox ID="chkBillable" runat="server" DataField="Billable" />
                                    <px:PXSegmentMask ID="edExpenseAccountID" runat="server" DataField="ExpenseAccountID" />
                                    <px:PXSegmentMask ID="edExpenseSubID" runat="server" DataField="ExpenseSubID" AutoRefresh="True" />
                                    <px:PXSegmentMask ID="edSalesAccountID" runat="server" DataField="SalesAccountID" />
                                    <px:PXSegmentMask ID="edSalesSubID" runat="server" DataField="SalesSubID" AutoRefresh="True" />
                                    <px:PXSelector ID="edTaxCategoryID" runat="server" DataField="TaxCategoryID" AutoRefresh="True"/>
                                    <px:PXSelector CommitChanges="True" ID="edContractID" runat="server" DataField="ContractID" Height="18px" AllowEdit="True" AutoRefresh="True">
                                        <GridProperties FastFilterFields="Description" />
                                    </px:PXSelector>
                                    <px:PXSegmentMask ID="edTaskID" runat="server" AutoRefresh="True" DataField="TaskID" />
                                    <px:PXTextEdit ID="edCustomer_acctName" runat="server" DataField="CustomerID_Customer_acctName" Enabled="False" />
                                </RowTemplate>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="160" />
                        <Mode InitNewRow="True" AllowFormEdit="True" AllowUpload="True" />
                        <LevelStyles>
                            <RowForm Height="330px" Width="950px" />
                        </LevelStyles>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Tax Details">
                <Template>
                    <px:PXGrid ID="grid1" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" ActionsPosition="Top"
                        BorderWidth="0px" SkinID="Details">
                        <AutoSize Enabled="True" MinHeight="150" />
                        <ActionBar>
                            <Actions>
                                <Search Enabled="False" />
                                <Save Enabled="False" />
                                <EditRecord Enabled="False" />
                            </Actions>
                        </ActionBar>
                        <Levels>
                            <px:PXGridLevel DataMember="Taxes" DataKeyNames="RefNbr,LineNbr,TaxID">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXSelector SuppressLabel="True" ID="edTaxID" runat="server" DataField="TaxID" />
                                    <px:PXNumberEdit SuppressLabel="True" ID="edTaxRate" runat="server" DataField="TaxRate" Enabled="False" />
                                    <px:PXNumberEdit SuppressLabel="True" ID="edCuryTaxableAmt" runat="server" DataField="CuryTaxableAmt" />
                                    <px:PXNumberEdit SuppressLabel="True" ID="edCuryTaxAmt" runat="server" DataField="CuryTaxAmt" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="TaxID" Width="81px" />
                                    <px:PXGridColumn DataField="TaxRate" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="CuryTaxableAmt" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="CuryTaxAmt" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="Tax__TaxType" Label="Tax Type" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="Tax__PendingTax" Label="Pending VAT" TextAlign="Center" Type="CheckBox" Width="60px" />
                                    <px:PXGridColumn DataField="Tax__ReverseTax" Label="Reverse VAT" TextAlign="Center" Type="CheckBox" Width="60px" />
                                    <px:PXGridColumn DataField="Tax__ExemptTax" Label="Exempt From VAT" TextAlign="Center" Type="CheckBox" Width="60px" />
                                    <px:PXGridColumn DataField="Tax__StatisticalTax" Label="Statistical VAT" TextAlign="Center" Type="CheckBox" Width="60px" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Financial Details">
                <Template>
                    <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" GroupCaption="Link to AP" StartGroup="True" />
                    <px:PXSegmentMask CommitChanges="True" Height="19px" ID="edBranchID" runat="server" DataField="BranchID" />
                    <px:PXDropDown ID="edAPDocType" runat="server" DataField="APDocType" Enabled="False" />
                    <px:PXSelector ID="edAPRefNbr" runat="server" DataField="APRefNbr" AllowEdit="True" />
                    <px:PXDropDown ID="edAPStatus" runat="server" DataField="APStatus" Enabled="False" />
                    <px:PXLayoutRule ID="PXLayoutRule2" runat="server" ControlSize="XM" GroupCaption="Tax" LabelsWidth="SM" StartGroup="True" />
                    <px:PXSelector CommitChanges="True" ID="edTaxZoneID" runat="server" DataField="TaxZoneID" Text="ZONE1" />
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Approval Details">
                <Template>
                    <px:PXGrid ID="gridApproval" runat="server" DataSourceID="ds" Width="100%" SkinID="DetailsInTab" NoteIndicator="True">
                        <AutoSize Enabled="True" />
                        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
                        <Levels>
                            <px:PXGridLevel DataMember="Approval" DataKeyNames="ApprovalID,AssignmentMapID">
                                <Columns>
                                    <px:PXGridColumn DataField="ApproverEmployee__AcctCD" Width="160px" />
                                    <px:PXGridColumn DataField="ApproverEmployee__AcctName" Width="160px" />
                                    <px:PXGridColumn DataField="ApprovedByEmployee__AcctCD" Width="100px" />
                                    <px:PXGridColumn DataField="ApprovedByEmployee__AcctName" Width="160px" />
                                    <px:PXGridColumn DataField="ApproveDate" Width="90px" />
                                    <px:PXGridColumn DataField="Status" AllowNull="False" AllowUpdate="False" RenderEditorText="True" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
        <CallbackCommands>
            <Search CommitChanges="True" PostData="Page" />
            <Refresh CommitChanges="True" PostData="Page" />
        </CallbackCommands>
        <AutoSize Container="Window" Enabled="True" MinHeight="250" />
    </px:PXTab>
</asp:Content>
