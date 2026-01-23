/* SQ:File header comment was not found
AUTHOR: Hernadi Lajos
Date: 2015-01-28
 */
CREATE PROCEDURE [UMS].[usp_addUserInfo]
    @connectionID VARCHAR(50), 
    @pass2UserID VARCHAR(100), 
    @environmentName VARCHAR(50),
	@macAddress VARCHAR(17)
AS

    INSERT INTO UMS.UserInfo 
        (SignalRConnectionID, Pass2UserID, EnvironmentName, MACAddress)
    VALUES
        (@connectionID, @pass2UserID, @environmentName, @macAddress) 


RETURN 0

GO
GRANT EXECUTE ON OBJECT::[UMS].[usp_addUserInfo] TO [appuser] AS [dbo];