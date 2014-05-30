USE [AcumaticaDB]
GO

/****** Object:  Table [dbo].[CFIMInvoice]    Script Date: 05/21/2014 17:34:03 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CFIMInvoice]') AND type in (N'U'))
DROP TABLE [dbo].[CFIMInvoice]
GO

USE [AcumaticaDB]
GO

/****** Object:  Table [dbo].[CFIMInvoice]    Script Date: 05/21/2014 17:34:03 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[CFIMInvoice](
	[CompanyID] [int] NOT NULL,
	[InvoceID] [int] NOT NULL IDENTITY(1,1),
	[Code] [nvarchar](50) NULL,
	[Name] [nvarchar](255) NULL,
	[Status] [int] NULL
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[CFIMInvoice] ADD  CONSTRAINT [CFIMInvoice_PK] PRIMARY KEY CLUSTERED 
(
	[CompanyID] ASC,
	[InvoceID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO