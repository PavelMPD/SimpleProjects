<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM200540.aspx.cs" Inherits="Page_SM260000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <script type="text/javascript">
        function commandResult(ds, context) {
            if (context.command == "Save") {
                var ds = px_all[context.id];
                var isSitemapAltered = (ds.callbackResultArg == "RefreshSitemap");
                if (isSitemapAltered) {
                    __refreshMainMenu();
                }
            }
        }
	</script>
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.SM.TranslationMaint"
		PrimaryView="LanguageFilter">
	    <ClientEvents CommandPerformed="commandResult" />
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" BlockPage="true" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%"
		DataMember="LanguageFilter" Caption="Target Locale" OnDataBound="form_DataBound">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True"
				LabelsWidth="SM" ControlSize="M" />
			<px:PXSelector CommitChanges="True" ID="edLanguage" runat="server" DataField="Language"
				TextField="TranslatedName" DataSourceID="ds">
			</px:PXSelector>
			<px:PXLayoutRule runat="server" Merge="True" />
			<px:PXCheckBox CommitChanges="True" ID="chkShowLocalized" runat="server" DataField="ShowLocalized">
			</px:PXCheckBox>
			<px:PXCheckBox CommitChanges="True" ID="chkShowObsolete" runat="server" DataField="ShowObsolete">
			</px:PXCheckBox>
		</Template>
		<CallbackCommands>
			<Refresh RepaintControls="None" RepaintControlsIDs="grid2" />
		</CallbackCommands>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="400" SkinID="Horizontal"
        Height="300px">
        <AutoSize Enabled="true" Container="Window" />
        <Template1>
            <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100; height: 188px;"
                Width="100%" AutoAdjustColumns="True" AdjustPageSize="Auto" AllowPaging="True"
                Caption="Default Values" AllowSearch="True" SkinID="Details" AutoSize="true" FastFilterFields="NeutralValue,Value" CaptionVisible="True">
                <Levels>
                    <px:PXGridLevel DataMember="DeltaResourcesDistinct">
                        <Columns>
                            <px:PXGridColumn DataField="NeutralValue" Width="108px">
                                <CellButtonStyle ForeColor="Black">
                                </CellButtonStyle>
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="Value" Width="108px">
                            </px:PXGridColumn>
                            <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="IsObsolete" TextAlign="Center"
                                Type="CheckBox" Width="60px">
                            </px:PXGridColumn>
                        </Columns>
                    </px:PXGridLevel>
                </Levels>
                <Mode AllowAddNew="False" AllowDelete="False" AllowUpload="true" />
                <AutoSize Enabled="True"></AutoSize>
                <ActionBar PagerVisible="False">
                    <Actions>
                        <Save Enabled="False" />
                        <AddNew Enabled="False" />
                        <Delete Enabled="False" />
                        <EditRecord Enabled="False" />
                        <NoteShow Enabled="False" />
                    </Actions>
                </ActionBar>
                <AutoCallBack Command="Refresh" Target="form">
                </AutoCallBack>
            </px:PXGrid>
        </Template1>
        <Template2>
            <px:PXGrid ID="grid2" runat="server" DataSourceID="ds" Height="500px" Style="z-index: 100"
                Width="100%" AutoAdjustColumns="True" AdjustPageSize="Auto" AllowPaging="True"
                Caption="Key-Specific Values" SkinID="Inquire" AutoSize="true" CaptionVisible="True">
                <CallbackCommands>
                    <Save RepaintControls="None" RepaintControlsIDs="grid2" />
                </CallbackCommands>
                <AutoSize Enabled="True"/>
                <Mode AllowAddNew="False" AllowDelete="False" />
                <Parameters>
                    <px:PXControlParam ControlID="grid" Name="NeutralValue" PropertyName="DataValues[&quot;NeutralValue&quot;]"
                        Type="String" />
                </Parameters>
                <Levels>
                    <px:PXGridLevel DataMember="ExceptionalResources">
                        <Columns>
                            <px:PXGridColumn DataField="ResKey" Width="90px">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="NeutralValue" Width="108px">
                            </px:PXGridColumn>
                            <px:PXGridColumn DataField="Value" Width="108px">
                            </px:PXGridColumn>
                        </Columns>
                    </px:PXGridLevel>
                </Levels>
                <ActionBar Position="Bottom" PagerVisible="False">
                    <Actions>
                        <Save Enabled="False" />
                        <AddNew Enabled="False" />
                        <Delete Enabled="False" />
                        <EditRecord Enabled="False" />
                        <NoteShow Enabled="False" />
                        <ExportExcel MenuVisible="False" ToolBarVisible="False" />
                    </Actions>
                </ActionBar>
            </px:PXGrid>
        </Template2>
    </px:PXSplitContainer>
    <px:PXSmartPanel ID="SkippedResources" runat="server" Caption="Skipped Resources"
        ContentLayout-OuterSpacing="None" CaptionVisible="True" Key="SkippedResourcesView"
        Width="100%" Height="400px" CommandSourceID="ds" AutoCallBack-Command="Refresh" AutoCallBack-Behavior-RepaintControlsIDs="grid3">
        <px:PXPanel ID="PXPanel1" runat="server">
            <px:PXLabel ID="PXLabel1" runat="server" Text="Following neutral values had not been collected because their length more than 448 symbols:"></px:PXLabel>
        </px:PXPanel>
        <px:PXGrid ID="grid3" runat="server" DataSourceID="ds" SkinID="Details"  Width="100%">
            <Levels>
                <px:PXGridLevel DataMember="SkippedResourcesView">
                    <Columns>
                        <px:PXGridColumn DataField="ResKey" Width="300px" />
                        <px:PXGridColumn DataField="NeutralValue" Width="680px" />
                    </Columns>
                </px:PXGridLevel>
            </Levels>            
            <AutoSize Enabled="True" MinHeight="250" />
            <ActionBar PagerVisible="False">
                <Actions>                    
                    <AddNew Enabled="False" />
                    <Delete Enabled="False" />
                </Actions>
            </ActionBar>
        </px:PXGrid>
        <px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton2" runat="server" DialogResult="OK" Text="OK" />
		</px:PXPanel>        
    </px:PXSmartPanel>
</asp:Content>
