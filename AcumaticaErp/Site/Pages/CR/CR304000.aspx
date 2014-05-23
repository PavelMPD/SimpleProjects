<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CR304000.aspx.cs" Inherits="Page_CR304000"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.CR.OpportunityMaint" PrimaryView="Opportunity">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand Name="Action" StartNewGroup="true" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="CreateInvoice" CommitChanges="True" Visible="False" />
            <px:PXDSCallbackCommand Name="CreateAccount" CommitChanges="True" Visible="false" />
            <px:PXDSCallbackCommand Name="ViewInvoice" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="CreateSalesOrder" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ViewSalesOrder" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewTask" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewEvent" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewActivity" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewMailActivity" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ViewActivity" DependOnGrid="gridActivities" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="OpenActivityOwner" Visible="False" CommitChanges="True" DependOnGrid="gridActivities" />
            <px:PXDSCallbackCommand Visible="False" Name="CurrencyView" />
            <px:PXDSCallbackCommand Name="Relations_EntityDetails" Visible="False" CommitChanges="True" DependOnGrid="grdRelations" />
            <px:PXDSCallbackCommand Name="Relations_ContactDetails" Visible="False" CommitChanges="True" DependOnGrid="grdRelations" />
        </CallbackCommands>
        <DataTrees>
            <px:PXTreeDataMember TreeView="_EPCompanyTree_Tree_" TreeKeys="WorkgroupID" />
        </DataTrees>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Opportunity Summary" DataMember="Opportunity" FilesIndicator="True"
        NoteIndicator="True" LinkIndicator="True" NotifyIndicator="True" DefaultControlID="edOpportunityID">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
            <px:PXSelector ID="edOpportunityID" runat="server" DataField="OpportunityID" FilterByAllFields="True" AutoRefresh="True" />
            <px:PXDropDown CommitChanges="True" ID="edStatus" runat="server" AllowNull="False" DataField="Status" />
            <px:PXDropDown CommitChanges="True" ID="edResolution" runat="server" DataField="Resolution" AllowNull="False" />
            <px:PXDropDown ID="edStageID" runat="server" AllowNull="False" DataField="StageID" CommitChanges="True" />
            <px:PXDropDown ID="edSource" runat="server" DataField="Source" />
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edOpportunityName" runat="server" AllowNull="False" DataField="OpportunityName" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXSelector CommitChanges="True" ID="edCROpportunityClassID" runat="server" DataField="CROpportunityClassID" AllowEdit="True" TextMode="Search" DisplayMode="Hint" FilterByAllFields="True" />
            <px:PXSegmentMask CommitChanges="True" ID="edBAccountID" runat="server" DataField="BAccountID" AllowEdit="True" FilterByAllFields="True" TextMode="Search" DisplayMode="Hint" />
            <px:PXSegmentMask ID="edLocationID" runat="server" DataField="LocationID" AllowEdit="True" AutoRefresh="True" FilterByAllFields="True" TextMode="Search" DisplayMode="Hint" CommitChanges="True" />
            <px:PXSelector CommitChanges="True" ID="edContactID" runat="server" DataField="ContactID" TextField="displayName" AllowEdit="True" AutoRefresh="True" TextMode="Search" DisplayMode="Text" FilterByAllFields="True" />
            <pxa:PXCurrencyRate ID="edCury" runat="server" DataMember="_Currency_" DataField="CuryID" RateTypeView="_CROpportunity_CurrencyInfo_" DataSourceID="ds"></pxa:PXCurrencyRate>
                 
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
            <px:PXCheckBox CommitChanges="True" ID="chkManualTotalEntry" runat="server" DataField="ManualTotalEntry" />
            <px:PXNumberEdit CommitChanges="True" ID="edCuryAmount" runat="server" DataField="CuryAmount"/>
            <px:PXNumberEdit CommitChanges="True" ID="edCuryDiscTot" runat="server" DataField="CuryDiscTot"/>
            <px:PXNumberEdit ID="edCuryProductsAmount" runat="server" DataField="CuryProductsAmount" Enabled="False"/>
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Width="100%" Height="280px" DataSourceID="ds" DataMember="OpportunityCurrent">
        <Items>
            <px:PXTabItem Text="Details">
                <Template>
                    <px:PXPanel ID="PXPanel1" runat="server" RenderStyle="Simple" Style="margin: 9px;">
                        <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                        <px:PXSegmentMask CommitChanges="True" ID="edBranchID" runat="server" DataField="BranchID" />
                        <px:PXSegmentMask CommitChanges="True" ID="edProjectID" runat="server" DataField="ProjectID" AutoRefresh="True" />
                        <px:PXSelector ID="WorkgroupID" runat="server" DataField="WorkgroupID" CommitChanges="True" TextMode="Search" DisplayMode="Text" FilterByAllFields="True" />
                        <px:PXSelector ID="OwnerID" runat="server" DataField="OwnerID" TextMode="Search" DisplayMode="Text" FilterByAllFields="True" />
                        <px:PXSegmentMask ID="edParentBAccountID" runat="server" DataField="ParentBAccountID" AllowEdit="True" FilterByAllFields="True" TextMode="Search" DisplayMode="Hint" />
                        <px:PXSelector ID="edCampaignSourceID" runat="server" DataField="CampaignSourceID" AllowEdit="True" TextMode="Search" DisplayMode="Hint" FilterByAllFields="True" />
                        <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                        <px:PXDateTimeEdit ID="edCloseDate" runat="server" DataField="CloseDate" Size="SM" />
                        
                        <px:PXFormView ID="panelfordatamember" runat="server" DataMember="ProbabilityCurrent" DataSourceID="ds" RenderStyle="Simple">
                            <Template>
                                <px:PXLayoutRule ID="PXLayoutRule14" runat="server" SuppressLabel="False" LabelsWidth="S" />
                                <px:PXNumberEdit ID="edProbability" runat="server" DataField="Probability" Enabled="False" Size="SM" />
                            </Template>
                            <ContentStyle BackColor="Transparent" />
                        </px:PXFormView>
                        <px:PXNumberEdit ID="edCuryWgtAmount" runat="server" DataField="CuryWgtAmount" Enabled="False" Size="SM" />
                        
                    </px:PXPanel>
                    <pxa:PXRichTextEdit ID="edDescription" runat="server" DataField="Description" Style="border-top: 1px solid #BBBBBB; border-left: 0px; border-right: 0px; margin: 0px;
                        padding: 0px; width: 100%; height: 100%;">
                        <AutoSize Enabled="True" MinHeight="216" />
                    </pxa:PXRichTextEdit>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Attributes">
                <Template>
                    <px:PXGrid ID="PXGridAnswers" runat="server" DataSourceID="ds" SkinID="Inquire" Width="100%" Height="200px" MatrixMode="True">
                        <Levels>
                            <px:PXGridLevel DataMember="Answers">
                                <Columns>
                                    <px:PXGridColumn DataField="isRequired" TextAlign="Center" Type="CheckBox" Width="75px" AllowResize="True" />
                                    <px:PXGridColumn DataField="AttributeID" TextAlign="Left" Width="250px" AllowShowHide="False" TextField="AttributeID_description" />
                                    <px:PXGridColumn DataField="Value" Width="300px" AllowShowHide="False" AllowSort="False" />
                                </Columns>
                                <Layout FormViewHeight="" />
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="200" />
                        <ActionBar>
                            <Actions>
                                <Search Enabled="False" />
                            </Actions>
                        </ActionBar>
                        <Mode AllowAddNew="False" AllowColMoving="False" AllowDelete="False" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Activities" LoadOnDemand="true">
                <Template>
                    <pxa:PXGridWithPreview ID="gridActivities" runat="server" DataSourceID="ds" Width="100%" AllowSearch="True" DataMember="Activities" AllowPaging="true" NoteField="NoteText"
                        FilesField="NoteFiles" BorderWidth="0px" GridSkinID="Inquire" SplitterStyle="z-index: 100; border-top: solid 1px Gray;  border-bottom: solid 1px Gray" PreviewPanelStyle="z-index: 100; background-color: Window"
                        PreviewPanelSkinID="Preview" BlankFilterHeader="All Activities" MatrixMode="true" PrimaryViewControlID="form">
                        <ActionBar DefaultAction="cmdViewActivity" PagerVisible="False">
                            <Actions>
                                <AddNew Enabled="False" />
                            </Actions>
                            <CustomItems>
                                <px:PXToolBarButton Text="Add Task" Key="cmdAddTask">
                                    <AutoCallBack Command="NewTask" Target="ds"></AutoCallBack>
                                    <PopupCommand Command="Cancel" Target="ds" />
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Text="Add Event" Key="cmdAddEvent">
                                    <AutoCallBack Command="NewEvent" Target="ds"></AutoCallBack>
                                    <PopupCommand Command="Cancel" Target="ds" />
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Text="Add Email" Key="cmdAddEmail">
                                    <AutoCallBack Command="NewMailActivity" Target="ds"></AutoCallBack>
                                    <PopupCommand Command="Cancel" Target="ds" />
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Text="Add Activity" Key="cmdAddActivity">
                                    <AutoCallBack Command="NewActivity" Target="ds"></AutoCallBack>
                                    <PopupCommand Command="Cancel" Target="ds" />
                                    <ActionBar />
                                </px:PXToolBarButton>
                                <px:PXToolBarButton Key="cmdViewActivity" Visible="false">
                                    <ActionBar MenuVisible="false" />
                                    <AutoCallBack Command="ViewActivity" Target="ds" />
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                        <Levels>
                            <px:PXGridLevel DataMember="Activities">
                                <RowTemplate>
                					<px:PXTimeSpan TimeMode="True" ID="edTimeSpent" runat="server" DataField="TimeSpent" InputMask="hh:mm" MaxHours="99" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="IsCompleteIcon" Width="21px" AllowShowHide="False" ForceExport="True" />
                                    <px:PXGridColumn DataField="PriorityIcon" Width="21px" AllowShowHide="False" AllowResize="False" ForceExport="True" />
                                    <px:PXGridColumn DataField="ReminderIcon" Width="21px" AllowShowHide="False" AllowResize="False" ForceExport="True" />
                                    <px:PXGridColumn DataField="ClassIcon" Width="21px" AllowShowHide="False" ForceExport="True" />
                                    <px:PXGridColumn DataField="ClassInfo" Width="60px" />
                                    <px:PXGridColumn DataField="RefNoteID" Visible="false" AllowShowHide="False" />
                                    <px:PXGridColumn DataField="Subject" LinkCommand="ViewActivity" Width="297px" />
                                    <px:PXGridColumn DataField="UIStatus" />
                                    <px:PXGridColumn DataField="MPStatus" />
                                    <px:PXGridColumn DataField="Released" TextAlign="Center" Type="CheckBox" Width="80px" />
                                    <px:PXGridColumn DataField="StartDate" DisplayFormat="g" Width="120px" />
                                    <px:PXGridColumn DataField="CreatedDateTime" DisplayFormat="g" Width="120px" Visible="False" />
                                    <px:PXGridColumn DataField="TimeSpent" Width="63px" RenderEditorText="True"/>
                                    <px:PXGridColumn AllowUpdate="False" DataField="CreatedByID_Creator_Username" Visible="false" SyncVisible="False" SyncVisibility="False" Width="108px" />
                                    <px:PXGridColumn DataField="GroupID" Width="90px" />
                                    <px:PXGridColumn DataField="Owner" LinkCommand="OpenActivityOwner" Width="150px" DisplayMode="Text"/>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <GridMode AllowAddNew="False" AllowUpdate="False" />
                        <PreviewPanelTemplate>
                            <pxa:PXHtmlView ID="edBody" runat="server" DataField="body" TextMode="MultiLine" MaxLength="50" Width="100%" Height="100%" SkinID="Label" />
                        </PreviewPanelTemplate>
                        <AutoSize Enabled="true" />
                        <GridMode AllowAddNew="False" AllowDelete="False" AllowFormEdit="False" AllowUpdate="False" AllowUpload="False" />
                    </pxa:PXGridWithPreview>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Relations" LoadOnDemand="True">
                <Template>
                    <px:PXGrid ID="grdRelations" runat="server" Height="400px" Width="100%" AllowPaging="True" ActionsPosition="Top" AllowSearch="true" DataSourceID="ds" SkinID="Details">
                        <Levels>
                            <px:PXGridLevel DataMember="Relations">
                                <Columns>
                                    <px:PXGridColumn DataField="Role" Width="120px" />
                                    <px:PXGridColumn DataField="EntityID" Width="160px" AutoCallBack="true" TextField="EntityCD" LinkCommand="Relations_EntityDetails" />
                                    <px:PXGridColumn DataField="Name" Width="200px" />
                                    <px:PXGridColumn DataField="ContactID" Width="160px" AutoCallBack="true" TextField="ContactName" DisplayMode="Text" LinkCommand="Relations_ContactDetails" />
                                    <px:PXGridColumn DataField="Email" Width="120px" />
                                    <px:PXGridColumn DataField="AddToCC" Width="70px" Type="CheckBox" TextAlign="Center" />
                                </Columns>
                                <RowTemplate>
                                    <px:PXSelector ID="edRelEntityID" runat="server" DataField="EntityID" FilterByAllFields="True" />
                                    <px:PXSelector ID="edRelContactID" runat="server" DataField="ContactID" FilterByAllFields="True" AutoRefresh="True" />
                                </RowTemplate>
                            </px:PXGridLevel>
                        </Levels>
                        <Mode InitNewRow="True" />
                        <AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Products" LoadOnDemand="true">
                <Template>
                    <px:PXGrid ID="ProductsGrid" SkinID="Details" runat="server" Width="100%" Height="500px" DataSourceID="ds" ActionsPosition="Top" BorderWidth="0px" SyncPosition="true">
                        <AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
                        <Levels>
                            <px:PXGridLevel DataMember="Products">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM" />
                                    <px:PXSegmentMask CommitChanges="True" ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True" />
                                    <px:PXSegmentMask CommitChanges="True" ID="edSubItemID" runat="server" DataField="SubItemID" AutoRefresh="True">
                                        <Parameters>
                                            <px:PXControlParam ControlID="ProductsGrid" Name="CROpportunityProducts.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                        </Parameters>
                                    </px:PXSegmentMask>                                                                    
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="InventoryID" DisplayFormat="CCCCCCCCCCCCCCCCCCCC" Width="135px" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="SubItemID" DisplayFormat="&gt;AA-A-&gt;AA" Width="63px" />
                                    <px:PXGridColumn AllowNull="False" DataField="IsFree" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="Quantity" TextAlign="Right" Width="108px" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="UOM" DisplayFormat="&gt;aaaaaa" Width="63px" AutoCallBack="True" />
                                    <px:PXGridColumn AllowNull="False" DataField="CuryUnitPrice" TextAlign="Right" Width="99px" AutoCallBack="True" />
                                    <px:PXGridColumn AllowNull="False" DataField="CuryExtPrice" TextAlign="Right" Width="81px" AutoCallBack="True" />
                                    <px:PXGridColumn AllowNull="False" DataField="DiscPct" TextAlign="Right" Width="108px" AutoCallBack="True" />
                                    <px:PXGridColumn AllowNull="False" DataField="CuryDiscAmt" TextAlign="Right" Width="81px" AutoCallBack="True" />
                                    <px:PXGridColumn AllowNull="False" DataField="ManualDisc" TextAlign="Center" Type="CheckBox" AutoCallBack="True" />
                                    <px:PXGridColumn AllowNull="False" DataField="CuryAmount" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="TaskID" DisplayFormat="&gt;#####" Width="108px" RenderEditorText="true" />
                                    <px:PXGridColumn DataField="TaxCategoryID" DisplayFormat="&gt;aaaaaaaaaa" Width="81px" />
                                    <px:PXGridColumn DataField="TransactionDescription" Width="180px" />
                                    <px:PXGridColumn DataField="SiteID" DisplayFormat="&gt;AAAAAAAAAA" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Tax Details" LoadOnDemand="true">
                <Template>
                    <px:PXGrid ID="grid1" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" SkinID="Details" ActionsPosition="Top" BorderWidth="0px">
                        <AutoSize Enabled="True" MinHeight="150" />
                        <Levels>
                            <px:PXGridLevel DataMember="Taxes">
                                <Columns>
                                    <px:PXGridColumn DataField="TaxID" Width="81px" AllowUpdate="False" />
                                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="TaxRate" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn AllowNull="False" DataField="CuryTaxableAmt" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn AllowNull="False" DataField="CuryTaxAmt" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn AllowNull="False" DataField="Tax__TaxType" Label="Tax Type" RenderEditorText="True" />
                                    <px:PXGridColumn AllowNull="False" DataField="Tax__PendingTax" Label="Pending VAT" TextAlign="Center" Type="CheckBox" Width="60px" />
                                    <px:PXGridColumn AllowNull="False" DataField="Tax__ReverseTax" Label="Reverse VAT" TextAlign="Center" Type="CheckBox" Width="60px" />
                                    <px:PXGridColumn AllowNull="False" DataField="Tax__ExemptTax" Label="Exempt From VAT" TextAlign="Center" Type="CheckBox" Width="60px" />
                                    <px:PXGridColumn AllowNull="False" DataField="Tax__StatisticalTax" Label="Statistical VAT" TextAlign="Center" Type="CheckBox" Width="60px" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Discount Details" LoadOnDemand="true">
                <Template>
                    <px:PXGrid ID="formDiscountDetail" runat="server" DataSourceID="ds" Width="100%" SkinID="Details" BorderStyle="None">
                        <AutoSize Enabled="True" MinHeight="150" />
                        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
                        <ActionBar>
                            <Actions>
                                <Upload Enabled="false" />
                                <AddNew Enabled="false" />
                                <Delete Enabled="False" />
                                <EditRecord Enabled="False" />
                            </Actions>
                        </ActionBar>
                        <Levels>
                            <px:PXGridLevel DataMember="DiscountDetails">
                                <Columns>
                                    <px:PXGridColumn DataField="DiscountID" Width="90px" />
                                    <px:PXGridColumn DataField="DiscountSequenceID" Width="90px" />
                                    <px:PXGridColumn DataField="Type" DisplayFormat="&gt;a" RenderEditorText="true" Width="90px" />
                                    <px:PXGridColumn DataField="CuryDiscountableAmt" TextAlign="Right" Width="99px" />
                                    <px:PXGridColumn DataField="DiscountableQty" TextAlign="Right" Width="99px" />
                                    <px:PXGridColumn DataField="CuryDiscountAmt" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="DiscountPct" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn DataField="FreeItemID" DisplayFormat="&gt;CCCCC-CCCCCCCCCCCCCCC" Width="144px" />
                                    <px:PXGridColumn DataField="FreeItemQty" TextAlign="Right" Width="81px" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="250" MinWidth="300" />
    </px:PXTab>
    <px:PXSmartPanel ID="PanelCreateAccount" runat="server" Style="z-index: 108; position: absolute; left: 27px; top: 99px;" Caption="New Account"
        CaptionVisible="True" LoadOnDemand="true" ShowAfterLoad="true" Key="AccountInfo" AutoCallBack-Enabled="true" AutoCallBack-Target="formCreateAccount" AutoCallBack-Command="Refresh"
        CallBackMode-CommitChanges="True" CallBackMode-PostData="Page" AcceptButtonID="PXButtonOK" CancelButtonID="PXButtonCancel">
        <px:PXFormView ID="formCreateAccount" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Services Settings" CaptionVisible="False" SkinID="Transparent"
            DataMember="AccountInfo">
            <Template>
                <px:PXLayoutRule ID="PXLayoutRule6" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                <px:PXMaskEdit ID="edBAccountID" runat="server" DataField="BAccountID" CommitChanges="True"/>
                <px:PXTextEdit ID="edAccountName" runat="server" DataField="AccountName" />
            </Template>
        </px:PXFormView>
        <div style="padding: 5px; text-align: right;">
            <px:PXButton ID="PXButtonOK" runat="server" Text="Create" DialogResult="OK" Width="63px" Height="20px"></px:PXButton>
            <px:PXButton ID="PXButtonCancel" runat="server" DialogResult="No" Text="Cancel" Width="63px" Height="20px" Style="margin-left: 5px" />
        </div>
    </px:PXSmartPanel>
    <px:PXSmartPanel ID="panelCopyTo" runat="server" Height="99px" Width="380px" Style="z-index: 108; left: 351px; position: absolute; top: 99px;" Caption="Create Sales Order"
        CaptionVisible="true" DesignView="Hidden" LoadOnDemand="true" Key="CreateOrderParams" AutoCallBack-Enabled="true" AutoCallBack-Target="formCopyTo" AutoCallBack-Command="Refresh"
        CallBackMode-CommitChanges="True" CallBackMode-PostData="Page">
        <px:PXFormView ID="formCopyTo" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" CaptionVisible="False" DataMember="CreateOrderParams">
            <ContentStyle BackColor="Transparent" BorderStyle="None" />
            <Template>
                <px:PXLayoutRule ID="PXLayoutRule7" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
                <px:PXSelector ID="edOrderType" runat="server" AllowNull="False" DataField="OrderType" DisplayMode="Text" Width="216px" />
                <px:PXCheckBox ID="chkRecalcDiscounts" runat="server" DataField="RecalcDiscounts" />
                <px:PXLayoutRule ID="PXLayoutRule2" runat="server" Merge="True" LabelsWidth="S" />
                <px:PXButton ID="btnSave" runat="server" DialogResult="OK" Text="OK" Height="20">
                    <AutoCallBack Target="formCopyTo" Command="Save" />
                </px:PXButton>
                <px:PXButton ID="btnCancel" runat="server" DialogResult="Cancel" Text="Cancel" Height="20" />
            </Template>
        </px:PXFormView>
    </px:PXSmartPanel>
</asp:Content>
