/* SQ:File header comment was not found
AUTHOR: Hernadi Lajos
Date: 2015-01-28
 */
CREATE PROCEDURE [UMS].[usp_getUserInfoByPass2UserId]
    @pass2UserID VARCHAR(100)
AS
    SELECT 
        U.SignalRConnectionID,

        U.Pass2UserID,
		U.MACAddress,

        U.LastMessageSender,

        U.ConnectionStarted,
        U.EnvironmentName,
        U.MachineName,
        
        U.LastMessageTime,
        U.LastMessageText
        
    FROM 
        UMS.UserInfo AS U 
    WHERE
        U.Pass2UserID = @pass2UserID

RETURN 0

GO
GRANT EXECUTE ON OBJECT::[UMS].[usp_getUserInfoByPass2UserId] TO [appuser] AS [dbo];