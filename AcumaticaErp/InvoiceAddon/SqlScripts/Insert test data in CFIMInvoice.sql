TRUNCATE TABLE [AcumaticaDB].[dbo].[CFIMInvoice]

INSERT INTO [AcumaticaDB].[dbo].[CFIMInvoice]
           ([CompanyId]
           ,[Code]
           ,[Name]
           ,[Status])
SELECT 3, '100', N'������� ������', 0 UNION
SELECT 3, '101', N'������� ������', 1 

SELECT * FROM [AcumaticaDB].[dbo].[CFIMInvoice]