/* SQ:File header comment was not found
AUTHOR: Hernadi Lajos
Date: 2015-01-28
 */
CREATE PROCEDURE [UMS].[usp_setMessage]
    @connectionID VARCHAR(50), 
    @sender VARCHAR(100), 
    @message NVARCHAR(MAX)
AS
    

    UPDATE 
        UMS.UserInfo
    SET
        LastMessageSender = @sender,
        LastMessageText = @message,
        LastMessageTime = GETDATE()

    WHERE
        SignalRConnectionID = @connectionID

    IF @@ROWCOUNT=0 BEGIN
        RAISERROR('Nincs olyan UserInfo amihez az üzenetet köthetnénk -> (%s).', 16, 1, @connectionID);
    END


RETURN 0

GO
GRANT EXECUTE ON OBJECT::[UMS].[usp_setMessage] TO [appuser] AS [dbo];