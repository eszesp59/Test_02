/* SQ:File header comment was not found
AUTHOR: Hernadi Lajos
Date: 2015-01-28
 */
CREATE PROCEDURE [UMS].[usp_getPass2UserIdListByApp]
    @app	CHAR(1)		-- : „F = FOR00”, „I = IUR”
AS
	;WITH Felh AS (
		SELECT UI.Pass2UserID
			 , MAX(UI.ConnectionStarted) AS ConnectionStarted
	    FROM 
	        UMS.UserInfo AS UI 
		WHERE
			@app = 'I' AND UI.MACAddress = 'IUR'
		OR  @app = 'F' AND LEN(UI.MACAddress) = 17
		GROUP BY UI.Pass2UserID
		)
    SELECT UI.SignalRConnectionID
		 , UI.Pass2UserID
		 , UI.EnvironmentName
		 , UI.MachineName
		 , CASE @app WHEN 'F' THEN UI.MACAddress
							  ELSE NULL END		AS MACAddress
    FROM 
        UMS.UserInfo UI INNER JOIN Felh F ON F.Pass2UserID = UI.Pass2UserID AND F.ConnectionStarted = UI.ConnectionStarted
	;
RETURN 0

GO
GRANT EXECUTE ON OBJECT::[UMS].[usp_getPass2UserIdListByApp] TO [appuser] AS [dbo];
