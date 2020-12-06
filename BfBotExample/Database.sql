CREATE TABLE [dbo].[KillSwitch](
	[KillSwitchId] [int] IDENTITY(1,1) NOT NULL,
	[System] [varchar](150) NOT NULL,
	[TurnOff] [bit] NOT NULL,
 CONSTRAINT [PK_KillSwitch] PRIMARY KEY CLUSTERED 
(
	[KillSwitchId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE Proc [dbo].[KillSwitchGet] 
 @system varchar(150)
as
 Select count(*) as TurnOff from [dbo].[KillSwitch] where [TurnOff] = 1 and [System] in ('Global', @system)
GO

-----------------------------------------------------------------------------------------
INSERT INTO [KillSwitch] ([System] ,[TurnOff]) VALUES ( 'Global' , 0)
INSERT INTO [KillSwitch] ([System] ,[TurnOff]) VALUES ( 'BfBotExample' , 0)
GO
