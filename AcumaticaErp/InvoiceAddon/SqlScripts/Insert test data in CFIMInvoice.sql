TRUNCATE TABLE [AcumaticaDB].[dbo].[CFIMInvoice]

INSERT INTO [AcumaticaDB].[dbo].[CFIMInvoice]
           ([Code]
           ,[Name]
           ,[Status])
SELECT '100', N'Покупка столов', 0 UNION
SELECT '101', N'Покупка слонов', 1 

SELECT * FROM [AcumaticaDB].[dbo].[CFIMInvoice]