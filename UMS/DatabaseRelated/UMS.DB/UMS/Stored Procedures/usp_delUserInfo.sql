/* SQ:File header comment was not found
AUTHOR: Hernadi Lajos
Date: 2015-01-28
 */
CREATE PROCEDURE [UMS].[usp_delUserInfo]
    @connectionID VARCHAR(50)
AS
    DELETE FROM UMS.UserInfo WHERE SignalRConnectionID = @connectionID

RETURN 0

GO
GRANT EXECUTE ON OBJECT::[UMS].[usp_delUserInfo] TO [appuser] AS [dbo];