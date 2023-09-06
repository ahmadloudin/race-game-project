-- •○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•
--				Création des tables
-- •○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•

USE TP2_SussyKart;
GO
-- Modifier la table
ALTER TABLE Utilisateurs.Utilisateur
ADD MotDePasse varbinary(32) NOT NULL DEFAULT 0x,
	NoBancaire varbinary(max) NOT NULL DEFAULT 0x,
	Sel varbinary(16) NOT NULL DEFAULT 0x;
GO

-- Créer une clé maîtresse avec un mot de passe
CREATE MASTER KEY ENCRYPTION BY PASSWORD = 'P4ssw0rd!';
GO

-- Créer un certificat auto-signé
CREATE CERTIFICATE MonCertificat WITH SUBJECT = 'ChiffrementNoBancaire';
GO

-- Créer une clé symétrique
CREATE SYMMETRIC KEY MaSuperCle WITH ALGORITHM = AES_256 ENCRYPTION BY CERTIFICATE MonCertificat;
GO

DECLARE @Sel varbinary(16) = CRYPT_GEN_RANDOM(16);

DECLARE @MotDePasseSel nvarchar(116) = CONCAT(N'Patate', @Sel);

OPEN SYMMETRIC KEY MaSuperCle
	DECRYPTION BY CERTIFICATE MonCertificat;

	UPDATE Utilisateurs.Utilisateur
	SET MotDePasse = HASHBYTES('SHA2_256', @MotDePasseSel),
	NoBancaire = CAST(EncryptByKey(KEY_GUID('MaSuperCle'), N'123456789') AS varbinary(max)),
	Sel = @Sel;

	CLOSE SYMMETRIC KEY MaSuperCle;
	GO

-- •○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•
--			  Création des procédures
-- •○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•

USE TP2_SussyKart;
GO

DROP PROCEDURE IF EXISTS Utilisateurs.USP_CreerUtilisateur;
GO

-- Procédure inscription
CREATE PROCEDURE Utilisateurs.USP_CreerUtilisateur
	@Pseudo nvarchar(30),
	@Courriel nvarchar(320),
	@NoBancaire nvarchar(30),
	@MotDePasse nvarchar(100)
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @Sel varbinary(16) = CRYPT_GEN_RANDOM(16);

	DECLARE @MotDePasseSel nvarchar(116) = CONCAT(@MotDePasse, @Sel);

	DECLARE @MdpHachage varbinary(32) = HASHBYTES('SHA2_256', @MotDePasseSel);

	-- Chiffrement du NoBancaire
	OPEN SYMMETRIC KEY MaSuperCle
	DECRYPTION BY CERTIFICATE MonCertificat;

	DECLARE @NoBancaireChiffre varbinary(max) = EncryptByKey(KEY_GUID('MaSuperCle'), @NoBancaire);

	CLOSE SYMMETRIC KEY MaSuperCle;

	INSERT INTO Utilisateurs.Utilisateur (Pseudo, DateInscription, Courriel, EstSuppr, MotDePasse, NoBancaire, Sel)
	VALUES (@Pseudo, GETDATE(), @Courriel, 0, @MdpHachage, @NoBancaireChiffre, @Sel);
END
GO

USE TP2_SussyKart;
GO
-- Procédure authentification
CREATE PROCEDURE Utilisateurs.USP_AuthUtilisateur
	@Pseudo nvarchar(50),
	@MotDePasse nvarchar(100)
AS
BEGIN
	DECLARE @Sel varbinary(16);
	DECLARE @MdpHache varbinary(32);
	SELECT @Sel = Sel, @MdpHache = MotDePasse
	FROM Utilisateurs.Utilisateur
	WHERE Pseudo = @Pseudo;
		
	IF HASHBYTES ('SHA2_256', CONCAT(@MotDePasse, @Sel)) = @MdpHache
	BEGIN
		SELECT * FROM Utilisateurs.Utilisateur WHERE Pseudo = @Pseudo;
	END
	ELSE
	BEGIN
		SELECT TOP 0 * FROM Utilisateurs.Utilisateur;
	END
	
END
GO

