<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="EP206000.aspx.cs" Inherits="Page_EP206000"
	Title="Activity" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Width="100%" Visible="true" PrimaryView="Message"
		TypeName="PX.Objects.EP.EmailRoutingMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Cancel" PopupVisible="true" />
			<px:PXDSCallbackCommand Name="Delete" PostData="Self" ClosePopup="true" PopupVisible="true" />
			<px:PXDSCallbackCommand Name="gotoParentActivity" StartNewGroup="true" Visible="true"
				PopupVisible="true" />
			<%--<px:PXDSCallbackCommand Name="SetViewed$EPEmailRouting" ClosePopup="false" SelectControlsIDs="message" />--%>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="server">
	<px:PXFormView ID="message" runat="server" DataSourceID="ds" Style="z-index: 100"
		Width="100%"  DataMember="Message" Caption="Email Summary"
		Overflow="Hidden"   NoteIndicator="true"
		FilesIndicator="true" DefaultControlID="edSubject">
		<FilesMenuUrls FilesDisplayUrl="~/Frames/GetFile.ashx" />
		<Template>
			<table width="100%" cellpadding="0" cellspacing="0" style="padding:9px; height:123px;">
				<tr style="height: 24px;">
					<td>
						<px:PXLabel ID="lblMailFrom" runat="server" Style="z-index: 100;">From :</px:PXLabel>
					</td>
					<td valign="top" style="padding-left:9px; padding-top:4px; padding-right:2px;">
						<div class="req-l req-lh" />
					</td>					
					<td rowspan="5" align="left" >
						<pxa:PXHtmlView ID="edEntityDescription" runat="server" DataField="EntityDescription"
							Style="z-index: 110; height: 123px; width: 327px; border: solid 1px Gray;
							background-color: White; margin-left:9px;" TabIndex="60" TextMode="MultiLine" SkinID="Label">
							<AutoSize Enabled="true" />
						</pxa:PXHtmlView>
					</td>
				</tr>
				<tr style="height: 24px;">
					<td>
						<px:PXLabel ID="lblMailTo" runat="server" Style="z-index: 102;">To :</px:PXLabel>
					</td>
					<td valign="top" style="padding-left:9px; padding-top:4px; padding-right:2px;">
						<div class="req-l req-lh" />
					</td>
					<td width="100%" >
						<pxa:PXEmailSelector ID="edMailTo" runat="server" DataField="MailTo" 
							TextField="displayName" ValueField="EMail" LabelID="lblMailTo" 
							Style="z-index: 103;" TabIndex="20" Width="100%" AutoGenerateColumns="false">
							<GridProperties FastFilterFields="DisplayName">
								<Columns>
									<px:PXGridColumn DataField="DisplayName" Width="240px" />
									<px:PXGridColumn DataField="EMail" Width="180px" />
									<px:PXGridColumn DataField="BAccount__acctCD" Width="200px" />
									<px:PXGridColumn DataField="Salutation" Width="200px" />
								</Columns>
								<PagerSettings Mode="NextPrevFirstLast" />
							</GridProperties>
						</pxa:PXEmailSelector>
					</td>
				</tr>
				<tr style="height: 24px;">
					<td>
						<px:PXLabel ID="lblMailCc" runat="server" Style="z-index: 104;">CC :</px:PXLabel>
					</td>
					<td>&nbsp;</td>
					<td width="100%" >
						<pxa:PXEmailSelector ID="edMailCc" runat="server" DataField="MailCc" 
							TextField="displayName" ValueField="EMail" LabelID="lblMailCc" 
							Style="z-index: 105;" TabIndex="30" Width="100%">
							<GridProperties FastFilterFields="DisplayName">
								<Columns>
									<px:PXGridColumn DataField="DisplayName" Width="240px" />
									<px:PXGridColumn DataField="EMail" Width="180px" />
									<px:PXGridColumn DataField="BAccount__acctCD" Width="200px" />
									<px:PXGridColumn DataField="Salutation" Width="200px" />
								</Columns>
								<PagerSettings Mode="NextPrevFirstLast" />
							</GridProperties>
						</pxa:PXEmailSelector>
					</td>
				</tr>
				<tr style="height: 24px;">
					<td>
						<px:PXLabel ID="lblMailBcc" runat="server" Style="z-index: 106;">BCC :</px:PXLabel>
					</td>
					<td>&nbsp;</td>
					<td >
						<pxa:PXEmailSelector ID="edMailBcc" runat="server" DataField="MailBcc" 
							TextField="displayName" ValueField="EMail" LabelID="lblMailBcc" 
							Style="z-index: 107;" TabIndex="40" Width="100%">
							<GridProperties FastFilterFields="DisplayName">
								<Columns>
									<px:PXGridColumn DataField="DisplayName" Width="240px" />
									<px:PXGridColumn DataField="EMail" Width="180px" />
									<px:PXGridColumn DataField="BAccount__acctCD" Width="200px" />
									<px:PXGridColumn DataField="Salutation" Width="200px" />
								</Columns>
								<PagerSettings Mode="NextPrevFirstLast" />
							</GridProperties>
						</pxa:PXEmailSelector>
					</td>
				</tr>
				<tr style="height: 24px;">
					<td>
						<px:PXLabel ID="lblSubject" runat="server" Style="z-index: 108; height: 13px;">Subject :</px:PXLabel>
					</td>
					<td valign="top" style="padding-left:9px; padding-top:4px; padding-right:2px;">
						<div class="req-l req-lh" />
					</td>
					<td width="100%" style="padding-right:6px;">
						<px:PXTextEdit ID="edSubject" runat="server" DataField="Subject" LabelID="lblSubject"
							 Style="z-index: 109;" TabIndex="50" Width="100%">
							<%--<AutoCallBack Enabled="True" Command="Save" Target="message" />--%>
						</px:PXTextEdit>
					</td>
				</tr>
			</table>
		</Template>
		
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" DataSourceID="ds" Height="270px" Style="z-index: 100"
		Width="100%"  DataMember="Activites">
		<Items>
			<px:PXTabItem Text="Message">
				<Template>
					<pxa:PXRichTextEdit ID="wikiEdit" runat="server" Style="z-index: 113; border-width: 0px;" TabIndex="51"
						DataField="Body" Width="100%" 
						FilesContainer="message" >
						<ContentStyle BorderStyle="None" />
						<AutoSize Enabled="True" Container="Parent" />
					</pxa:PXRichTextEdit>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Enabled="True" Container="Window" />
	</px:PXTab>
	<pxa:PXEntityViewer ID="viewer" runat="server" Target="ds" Command="SetViewed$EPEmailRouting" />
</asp:Content>
