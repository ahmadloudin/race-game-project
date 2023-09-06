USE TP2_SussyKart
GO

-- •○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•
--				Création des tables
-- •○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•

CREATE TABLE Utilisateurs.Amitie (
	AmitieID int IDENTITY(1,1),
	UtilisateurID int,
	UtilisateurID_Ami int,
	CONSTRAINT PK_Amitie_AmitieID PRIMARY KEY (AmitieID)
);

ALTER TABLE Utilisateurs.Amitie ADD CONSTRAINT FK_Amitie_UtilisateurID
FOREIGN KEY (UtilisateurID) REFERENCES Utilisateurs.Utilisateur(UtilisateurID);
GO

ALTER TABLE Utilisateurs.Amitie ADD CONSTRAINT FK_Amitie_UtilisateurID_Ami
FOREIGN KEY (UtilisateurID_Ami) REFERENCES Utilisateurs.Utilisateur(UtilisateurID);
GO

ALTER TABLE Utilisateurs.Amitie ADD CONSTRAINT UC_Utilisateur_UtilisateurIDUtilisateurID_Ami
UNIQUE (UtilisateurID, UtilisateurID_Ami);
GO
ALTER TABLE Utilisateurs.Amitie ADD CONSTRAINT CHK_Amitie CHECK (UtilisateurID_Ami != UtilisateurID)
-- Suppression du déclencheur existant (si nécessaire)
DROP TRIGGER IF EXISTS Utilisateurs.TRd_Utilisateur_Utilisateur;
GO
-- Création du déclencheur pour la suppression d'utilisateurs
CREATE TRIGGER Utilisateurs.Utilisateur_dtrgSupprimerUtilisateur
ON Utilisateurs.Utilisateur
INSTEAD OF DELETE
AS
BEGIN
SET NOCOUNT ON
  -- Soft delete de l'utilisateur
  UPDATE Utilisateurs.Utilisateur SET EstSuppr = 1 WHERE UtilisateurID IN (SELECT UtilisateurID FROM deleted);

  -- Suppression des amitiés liées à l'utilisateur supprimé
  DELETE FROM Utilisateurs.Amitie WHERE UtilisateurID IN (SELECT UtilisateurID FROM deleted);

  -- Suppression de l'avatar de l'utilisateur supprimé s'il en avait un
  DELETE FROM Utilisateurs.Avatar WHERE UtilisateurID IN (SELECT UtilisateurID FROM deleted);

  -- Supprimer les utilisateurs supprimés de la liste d'amis des autres utilisateurs
  DELETE FROM Utilisateurs.Amitie WHERE UtilisateurID IN (SELECT UtilisateurID FROM deleted) OR UtilisateurID_Ami IN (SELECT UtilisateurID FROM deleted);

-- Empêcher l'ajout de l'utilisateur supprimé dans la liste d'amis

END
GO
 
-- Procédure authentification
ALTER PROCEDURE Utilisateurs.USP_AuthUtilisateur
	@Pseudo nvarchar(50),
	@MotDePasse nvarchar(100)
	
AS
BEGIN
	DECLARE @Sel varbinary(16);
	DECLARE @MdpHache varbinary(32);
	DECLARE @EstSuppr bit;
	SELECT @Sel = Sel, @MdpHache = MotDePasse, @EstSuppr = EstSuppr
	FROM Utilisateurs.Utilisateur
	WHERE Pseudo = @Pseudo;
		
	IF @EstSuppr = 0
	BEGIN
	IF HASHBYTES ('SHA2_256', CONCAT(@MotDePasse, @Sel)) = @MdpHache
	BEGIN
		SELECT * FROM Utilisateurs.Utilisateur WHERE Pseudo = @Pseudo;
	END
	ELSE
	BEGIN
		SELECT TOP 0 * FROM Utilisateurs.Utilisateur;
	END
	END
	ELSE
	BEGIN
		SELECT TOP 0 * FROM Utilisateurs.Utilisateur;
	END
END
GO