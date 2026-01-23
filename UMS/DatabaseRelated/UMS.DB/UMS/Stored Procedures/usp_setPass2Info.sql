/* SQ:File header comment was not found
AUTHOR: Hernadi Lajos
Date: 2015-01-28
 */
 CREATE PROCEDURE [UMS].[usp_setPass2Info]
    @connectionID VARCHAR(50), 
    @pass2UserID VARCHAR(100), 
    @environmentName VARCHAR(50),
    @machineName VARCHAR(50),
	@macAddress VARCHAR(17)
AS

    UPDATE 
        UMS.UserInfo
    SET
        Pass2UserID = @pass2UserID,
        EnvironmentName = @environmentName,
        MachineName = @machineName,
		MACAddress = @macAddress
    WHERE
        SignalRConnectionID = @connectionID

    IF @@ROWCOUNT=0 BEGIN
        RAISERROR('Nincs olyan UserInfo aminek a Pass2Info adatait szeretnénk módosítani -> (%s).', 16, 1, @connectionID);
    END

RETURN 0

GO
GRANT EXECUTE ON OBJECT::[UMS].[usp_setPass2Info] TO [appuser] AS [dbo];