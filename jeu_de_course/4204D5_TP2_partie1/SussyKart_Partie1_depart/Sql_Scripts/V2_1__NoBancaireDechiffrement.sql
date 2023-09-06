-- •○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•
--					  NoBancaire
-- •○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•
	CREATE TABLE Utilisateurs.NoBancaire(
		NumBancaire nvarchar(30)
	);
	GO

	CREATE PROCEDURE Utilisateurs.USP_NoBancaire
		@Pseudo nvarchar(50)
	AS
	BEGIN
		
		DECLARE @MdpHache varbinary(32);
		
		SELECT @MdpHache = MotDePasse
		FROM Utilisateurs.Utilisateur
		WHERE Pseudo = @Pseudo;
		

		BEGIN

			OPEN SYMMETRIC KEY MaSuperCle
			DECRYPTION BY CERTIFICATE MonCertificat;

			SELECT CONVERT(nvarchar(30), DecryptByKey(NoBancaire))
			AS NumBancaire
			FROM Utilisateurs.Utilisateur WHERE Pseudo = @Pseudo;

			CLOSE SYMMETRIC KEY MaSuperCle;

		END
	
	END
	GO
