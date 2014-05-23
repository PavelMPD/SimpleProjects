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
	[InvoceID] [int] IDENTITY(1,1) NOT NULL,
	[Code] [nvarchar](50) NULL,
	[Name] [nvarchar](255) NULL,
	[Status] [int] NULL
) ON [PRIMARY]

GO