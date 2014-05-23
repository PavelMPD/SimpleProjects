<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" CodeFile="SM205070.aspx.cs" Inherits="Pages_SM_SM205070" ValidateRequest="False" %>

<asp:Content ID="Content1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource runat="server"
	ID="ds"
	Visible="True"
	TypeName="PX.SM.PerformanceMonitorMaint"
	PrimaryView="Filter"
	Width="100%">
		<CallbackCommands>
		    <px:PXDSCallbackCommand Name="actionFlushSamples" RepaintControls="Bound"/>
		    <%--<px:PXDSCallbackCommand Name="actionStopMonitor" RepaintControls="Bound"/>--%>
		    <px:PXDSCallbackCommand Name="actionClearSamples" RepaintControls="Bound"/>
		    <px:PXDSCallbackCommand Name="actionViewScreen" DependOnGrid="grid" Visible="False"/>
			
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>




<asp:Content ID="Content2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView runat="server" 
	ID="form"
	Width="100%"
	
	DataMember="Filter" CaptionVisible="False">
		<Template>
		    

			<px:PXLayoutRule ID="Column0" runat="server" StartColumn="True" GroupCaption="Profiler Status" SuppressLabel="True" />
                      <px:PXCheckBox runat="server" ID="ProfilerEnabled" DataField="ProfilerEnabled" CommitChanges="True"/>
                      <px:PXCheckBox runat="server" ID="SqlProfiler" DataField="SqlProfiler" CommitChanges="True"/>

			<px:PXLayoutRule ID="Column1" runat="server" StartColumn="True" GroupCaption="Filters"/>
		
		    <px:PXTextEdit runat="server" DataField="ScreenId" ID="edScreenId" CommitChanges="True"/>
		    <px:PXTextEdit runat="server" DataField="UserId" ID="edUserId" CommitChanges="True"/>
		    <px:PXTextEdit runat="server" DataField="TimeLimit" ID="edTimeLimit" CommitChanges="True"/>
		    <px:PXTextEdit runat="server" DataField="SqlCounterLimit" ID="SqlCounterLimit" CommitChanges="True"/>
   
			
			<px:PXLayoutRule ID="Column2" runat="server" StartColumn="True" GroupCaption="Memory Usage"/>
			<px:PXTextEdit runat="server" DataField="GCTotalMemory" ID="GCTotalMemory" />		
			<px:PXTextEdit runat="server" DataField="WorkingSet" ID="WorkingSet" />		
			<px:PXTextEdit runat="server" DataField="PrivateMemory" ID="PrivateMemory" />		
			<px:PXTextEdit runat="server" DataField="GCCollection" ID="GCCollection" />		
            <px:PXButton runat="server" NavigateUrl="http://technet.microsoft.com/en-us/sysinternals/dd535533.aspx" Target="_blank" Text="Inspect memory usage"></px:PXButton>
           

	
		</Template>
		
	</px:PXFormView>
</asp:Content>



<asp:Content ID="Content3" ContentPlaceHolderID="phG" Runat="Server">
	<px:PXGrid runat="server" ID="grid" SkinID="Details" Width="100%" Height="400px"
         SyncPosition="True"
        AutoGenerateColumns="Append" AutoAdjustColumns="True" AllowPaging="True" AdjustPageSize="Auto">
		<Levels>
			<px:PXGridLevel DataMember="Samples">
				<Columns>
					<px:PXGridColumn DataField="RequestStartTime" DisplayFormat="dd MMM HH:mm" />
                    <px:PXGridColumn DataField="UserId"  />
					<px:PXGridColumn DataField="ScreenId" Width="170" LinkCommand="actionViewScreen"/>
					<px:PXGridColumn DataField="CommandTarget"  />
					<px:PXGridColumn DataField="CommandName"  />
					<px:PXGridColumn DataField="ScriptTimeMs"  />
					<px:PXGridColumn DataField="RequestTimeMs"  />
                    <px:PXGridColumn DataField="SelectTimeMs"  />
                    <px:PXGridColumn DataField="SqlTimeMs"  />
					<px:PXGridColumn DataField="RequestCpuTimeMs"  />
					<px:PXGridColumn DataField="SqlCounter"  />
                    <px:PXGridColumn DataField="SelectCounter"  />

					<px:PXGridColumn DataField="MemBefore"  DisplayFormat="0,0" />
					<px:PXGridColumn DataField="MemDelta"  DisplayFormat="0,0" />
				</Columns>
				
			</px:PXGridLevel>
		</Levels>
<%--        <CallbackCommands>
            <Refresh RepaintControls="Bound" />
        </CallbackCommands>--%>
	    <AutoSize Enabled="True" Container="Window"/>
        <ActionBar PagerVisible="False" DefaultAction="buttonSql">
            <CustomItems>
                <px:PXToolBarButton Text="SQL" PopupPanel="PanelSqlProfiler" Key="buttonSql"/>
                <px:PXToolBarButton Text="Open URL"  Key="ViewScreen" >
                    <AutoCallBack Target="ds" Command="actionViewScreen"></AutoCallBack>
                </px:PXToolBarButton>
            </CustomItems>
        </ActionBar>
		

	</px:PXGrid>
    
    <px:PXSmartPanel runat="server" ID="PanelSqlProfiler" Width="100%" Height="650px"
        ShowMaximizeButton="True"
        CaptionVisible="True"
		Caption="Sql Profiler"
        AutoSize-Enabled="True"
         AutoRepaint="True" Key="SqlSamples">
       

        <px:PXGrid runat="server" ID="GridProfiler"
            Width="100%"
            SkinID="Details"
           PageSize="25"
           AllowPaging="True">
            <Mode AllowFormEdit="True"></Mode>
            <Levels>
                
			<px:PXGridLevel DataMember="Sql" >
				<Columns>
                    <px:PXGridColumn DataField="QueryOrderID" />
                    <px:PXGridColumn DataField="TableList" Width="300"  />
                    <px:PXGridColumn DataField="NRows"   />
                    <px:PXGridColumn DataField="RequestStartTime"   />
                    <px:PXGridColumn DataField="SqlTimeMs"   />
                    <px:PXGridColumn DataField="ShortParams" Width="250"  />


				</Columns>
				<RowTemplate>
				    <px:PXLayoutRule runat="server"/>

				    <px:PXTextEdit runat="server" ID="tables" DataField="TableList" SelectOnFocus="False" Width="600px" />
				    
				    <px:PXTextEdit runat="server" ID="SqlText" DataField="SQLWithParams" SelectOnFocus="False" TextMode="MultiLine" Width="600px" Height="490px"/>
				        
				    
				</RowTemplate>
			</px:PXGridLevel>
		</Levels>
	    <AutoSize Enabled="True" Container="Parent"/>
        <ActionBar PagerVisible="False"/>    

        </px:PXGrid>
    </px:PXSmartPanel>
</asp:Content>

