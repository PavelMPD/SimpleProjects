<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="DR201500.aspx.cs"
    Inherits="Page_DR201500" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.DR.DraftScheduleMaint" PrimaryView="Schedule">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Cancel" PopupVisible="true" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" PopupVisible="True" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand Name="ViewBatch" PostData="Self" DependOnGrid="grid" Visible="false" />
            <px:PXDSCallbackCommand Name="GenTran" DependOnGrid="componentGrid" PostData="Self" Visible="false" />
            <px:PXDSCallbackCommand Name="ViewSchedule" PostData="Self" DependOnGrid="gridSchedules" Visible="false" />
            <px:PXDSCallbackCommand Name="Release" PostData="Self" RepaintControls="All" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Deferral Schedule" DataMember="Schedule"
        NoteIndicator="True" FilesIndicator="True" ActivityIndicator="True" ActivityField="NoteActivity" TabIndex="-15436">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
            <px:PXSelector ID="edScheduleID" runat="server" DataField="ScheduleID" NullText="<NEW>" DataSourceID="ds" TextField="ProxyScheduleID" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edDocDate" runat="server" DataField="DocDate" />
            <px:PXSelector ID="edFinPeriodID" runat="server" DataField="FinPeriodID" DataSourceID="ds" />
            <px:PXCheckBox ID="chkIsCustom" Visible="False" runat="server" DataField="IsCustom" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
            <px:PXDropDown CommitChanges="True" ID="edDocumentType" runat="server" DataField="DocumentType" />
            <px:PXSelector CommitChanges="True" ID="edRefNbr" runat="server" DataField="RefNbr" AutoRefresh="True" DataSourceID="ds" />
            <px:PXSelector CommitChanges="True" runat="server" DataField="LineNbr" ID="edLineNbr" AutoRefresh="True"  DataSourceID="ds" AutoCallBack="true"/>
            <px:PXNumberEdit CommitChanges="True" runat="server" DataField="OrigLineAmt" ID="edOrigLineAmt" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXSelector CommitChanges="True" ID="edBAccountID" runat="server" DataField="BAccountID" DataSourceID="ds" AutoRefresh="true" />
            <px:PXSegmentMask ID="edBAccountLocID" runat="server" DataField="BAccountLocID" DataSourceID="ds" />
            <px:PXSegmentMask ID="PXSegmentMask1" runat="server" DataField="ProjectID" CommitChanges="True" DataSourceID="ds" />
            <px:PXSegmentMask ID="PXSegmentMask2" runat="server" DataField="TaskID" DataSourceID="ds" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" DataSourceID="ds" DataMember="DocumentProperties">
        <Items>
            <px:PXTabItem Text="Details">
                <Template>
                    <px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="300" SkinID="Horizontal" Height="200px" 
											Panel1MinSize="180" Panel2MinSize="180">
                        <AutoSize Enabled="true" MinHeight="360" />
                        <Template1>
                            <px:PXGrid ID="componentGrid" runat="server" DataSourceID="ds" Width="100%" SkinID="DetailsInTab" Caption="Components" Height="100px">
                                <AutoSize Enabled="true" />
                                <Levels>
                                    <px:PXGridLevel DataMember="Components" DataKeyNames="ScheduleID,ComponentID">
                                        <RowTemplate>
                                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                            <px:PXSelector ID="edComponentID" runat="server" DataField="ComponentID" />
                                            <px:PXDropDown ID="edStatus2" runat="server" DataField="Status" Enabled="False" />
                                            <px:PXSegmentMask ID="edAccountID2" runat="server" DataField="AccountID" />
                                            <px:PXNumberEdit ID="edTotalAmt" runat="server" DataField="TotalAmt" Enabled="False" />
                                            <px:PXSegmentMask ID="edSubID2" runat="server" DataField="SubID" />
                                            <px:PXNumberEdit ID="edDefAmt" runat="server" DataField="DefAmt" Enabled="False" />
                                            <px:PXSelector ID="edDefCode" runat="server" DataField="DefCode" Enabled="False" AutoRefresh="true"/>
                                            <px:PXSegmentMask ID="edDefAcctID" runat="server" DataField="DefAcctID" />
                                            <px:PXSegmentMask ID="edDefSubID" runat="server" DataField="DefSubID" />
                                        </RowTemplate>
                                        <Columns>
                                            <px:PXGridColumn DataField="ComponentID" Width="108px" />
                                            <px:PXGridColumn DataField="DefCode" Width="91px" AutoCallBack="True" />
                                            <px:PXGridColumn DataField="DefAcctID" Width="108px" />
                                            <px:PXGridColumn DataField="DefSubID" Width="91px" />
                                            <px:PXGridColumn DataField="AccountID" Width="61px" />
                                            <px:PXGridColumn DataField="SubID" Width="108px" />
                                            <px:PXGridColumn DataField="TotalAmt" TextAlign="Right" Width="81px" />
                                            <px:PXGridColumn DataField="DefAmt" TextAlign="Right" Width="108px" />
                                            <px:PXGridColumn DataField="DefTotal" TextAlign="Right" Width="108px" />
                                            <px:PXGridColumn DataField="Status" RenderEditorText="True" Width="54px" />
                                        </Columns>
                                    </px:PXGridLevel>
                                </Levels>
                                <AutoCallBack Target="grid" Command="Refresh" />
                                <ActionBar DefaultAction="cmdViewBatch">
                                    <CustomItems>
                                        <px:PXToolBarButton Text="Generate Transactions" Key="cmdGenTran">
                                            <AutoCallBack Target="ds" Command="GenTran" />
                                        </px:PXToolBarButton>
                                    </CustomItems>
                                </ActionBar>
                                <CallbackCommands>
                                    <Save CommitChangesIDs="componentGrid" RepaintControls="None" />
                                    <FetchRow RepaintControls="None" />
                                </CallbackCommands>
                                <Mode InitNewRow="True" />
                            </px:PXGrid>
                        </Template1>
                        <Template2>
                            <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Width="100%" SkinID="DetailsInTab" Caption="Transactions">
                                <Levels>
                                    <px:PXGridLevel DataMember="Transactions" DataKeyNames="ScheduleID,ComponentID,LineNbr">
                                        <Columns>
                                            <px:PXGridColumn DataField="LineNbr" TextAlign="Right" Width="63px" />
                                            <px:PXGridColumn DataField="Status" RenderEditorText="True" Width="54px" />
                                            <px:PXGridColumn DataField="RecDate" Width="90px" />
                                            <px:PXGridColumn DataField="TranDate" Width="90px" />
                                            <px:PXGridColumn DataField="Amount" TextAlign="Right" Width="81px" />
                                            <px:PXGridColumn DataField="AccountID" Width="81px" AutoCallBack="True" />
                                            <px:PXGridColumn DataField="SubID" Width="108px" />
                                            <px:PXGridColumn DataField="FinPeriodID" Width="63px" />
                                            <px:PXGridColumn DataField="BranchID" Width="81px" />
                                            <px:PXGridColumn DataField="BatchNbr" Width="81px" />
                                        </Columns>
                                        <RowTemplate>
                                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                            <px:PXNumberEdit ID="edScheduleID" runat="server" DataField="ScheduleID" />
                                            <px:PXNumberEdit ID="edLineNbr" runat="server" DataField="LineNbr" Enabled="False" />
                                            <px:PXTextEdit ID="edModule" runat="server" DataField="Module" />
                                            <px:PXDropDown ID="edStatus" runat="server" DataField="Status" Enabled="False" />
                                            <px:PXDateTimeEdit ID="edRecDate" runat="server" DataField="RecDate" />
                                            <px:PXDateTimeEdit ID="edTranDate" runat="server" DataField="TranDate" Enabled="False" />
                                            <px:PXNumberEdit ID="edAmount" runat="server" DataField="Amount" />
                                            <px:PXSegmentMask ID="edAccountID" runat="server" DataField="AccountID" AllowEdit="True" />
                                            <px:PXSegmentMask ID="edSubID" runat="server" DataField="SubID" />
                                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                            <px:PXMaskEdit ID="edFinPeriodID" runat="server" DataField="FinPeriodID" Enabled="False" InputMask="##-####" />
                                            <px:PXTextEdit ID="edBatchNbr" runat="server" DataField="BatchNbr" />
                                        </RowTemplate>
                                        <Layout FormViewHeight="" />
                                    </px:PXGridLevel>
                                </Levels>
                                <AutoSize Enabled="True" />
                                <Mode InitNewRow="True" />
                                <ActionBar DefaultAction="cmdViewBatch">
                                    <CustomItems>
                                        <px:PXToolBarButton Text="View GL Batch" Key="cmdViewBatch" CommandName="ViewBatch" CommandSourceID="ds">
                                        </px:PXToolBarButton>
                                    </CustomItems>
                                </ActionBar>
                                <CallbackCommands>
                                    <Save CommitChangesIDs="grid" RepaintControls="None" />
                                    <FetchRow RepaintControls="None" />
                                </CallbackCommands>
                                <Parameters>
                                    <px:PXSyncGridParam ControlID="componentGrid" />
                                </Parameters>
                            </px:PXGrid>
                        </Template2>
                    </px:PXSplitContainer>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem LoadOnDemand="true" Text="Associated Schedules" BindingContext="form" VisibleExp="DataControls[&quot;chkIsCustom&quot;].Value == False">
                <Template>
                    <px:PXGrid ID="gridSchedules" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="100%" SkinID="Details"
                        BorderStyle="None">
                        <Levels>
                            <px:PXGridLevel DataMember="Associated" DataKeyNames="ScheduleID">
                                <Columns>
                                    <px:PXGridColumn DataField="ScheduleID" TextAlign="Right" />
                                    <px:PXGridColumn Width="200px" DataField="TranDesc" />
                                    <px:PXGridColumn DataField="DocType" />
                                    <px:PXGridColumn DataField="RefNbr" />
                                    <px:PXGridColumn DataField="FinPeriodID" />
                                </Columns>
                                <Layout FormViewHeight="" />
                            </px:PXGridLevel>
                        </Levels>
                        <Parameters>
                            <px:PXControlParam ControlID="form" Name="ScheduleID" PropertyName="NewDataKey[&quot;ScheduleID&quot;]" Type="String" />
                        </Parameters>
                        <AutoSize Enabled="True" MinHeight="260" />
                        <Mode AllowAddNew="False" AllowDelete="False" />
                        <ActionBar DefaultAction="gridSchedules">
                            <CustomItems>
                                <px:PXToolBarButton Text="View Schedule" Key="gridSchedules">
                                    <AutoCallBack Command="ViewSchedule" Target="ds" />
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Enabled="True" Container="Window" />
    </px:PXTab>
</asp:Content>
