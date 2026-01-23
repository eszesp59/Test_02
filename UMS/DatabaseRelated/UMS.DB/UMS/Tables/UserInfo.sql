/* SQ:File header comment was not found
AUTHOR: Hernadi Lajos
Date: 2015-01-28
*/
CREATE TABLE [UMS].[UserInfo]
(
    [SignalRConnectionID] VARCHAR(50) NOT NULL, 
    [ConnectionStarted] DATETIME NULL DEFAULT (getdate()), 

    [Pass2UserID] VARCHAR(100) NULL, 
    [EnvironmentName] VARCHAR(50) NULL, 
    [MachineName] VARCHAR(50) NULL, 
    [LastMessageTime] DATETIME NULL, 
    [LastMessageText] NVARCHAR(MAX) NULL, 
    [LastMessageSender] VARCHAR(100) NULL, 
    [MACAddress] VARCHAR(17) NULL, 
    CONSTRAINT [PK_UserInfo] PRIMARY KEY ([SignalRConnectionID])
)

GO

CREATE INDEX [IX_UserInfo_Pass2UserID] ON [UMS].[UserInfo] ([Pass2UserID])
