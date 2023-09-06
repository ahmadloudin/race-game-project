USE TP2_SussyKart
GO

-- •○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•
--				Création des tables
-- •○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•

CREATE TABLE Utilisateurs.Avatar(
	AvatarID int IDENTITY(1,1),
	Identifiant uniqueidentifier NOT NULL ROWGUIDCOL,
	UtilisateurID int NOT NULL
	CONSTRAINT PK_Avatar_AvatarID PRIMARY KEY (AvatarID)
);
GO

ALTER TABLE Utilisateurs.Avatar ADD CONSTRAINT UC_Avatar_Identifiant
UNIQUE (Identifiant);
GO

ALTER TABLE Utilisateurs.Avatar ADD CONSTRAINT UC_Avatar_UtilisateurID
UNIQUE (UtilisateurID);
GO

ALTER TABLE Utilisateurs.Avatar ADD CONSTRAINT DF_Avatar_Identifiant
DEFAULT newid() FOR Identifiant;
GO

ALTER TABLE Utilisateurs.Avatar ADD Fichier varbinary(max) FILESTREAM NULL;
GO

ALTER TABLE Utilisateurs.Avatar ADD CONSTRAINT FK_Avatar_UtilisateurID
FOREIGN KEY (UtilisateurID) REFERENCES Utilisateurs.Utilisateur(UtilisateurID);
GO

