USE TP2_SussyKart
GO

--Ce index rend plus vite la recherche pour l'action MeilleursChronosSolo(), car on utilise le champ NbJoueurs 
--pour trouver tout les participations avec un nombre de joueurs qui égale à 1.
CREATE NONCLUSTERED INDEX IX_ParticipationCourse_NbJoueurs ON Courses.ParticipationCourse(NbJoueurs);
GO

--En créant cet index, la base de données peut accéder 
--la colonne UtilisateurId lors des opérations de regroupement et de comptage distinct pour l'action 
--ParticipationsParCourse(), ce qui améliore les performances des requêtes.
CREATE NONCLUSTERED INDEX IX_ParticipationCourse_UtilisateurId ON Courses.ParticipationCourse(UtilisateurId);
GO

--En créant un index sur la colonne Chrono de la table ParticipationCourse, 
--la base de données peut optimiser l'opération de regroupement et améliorer 
--les performances de l'action MeilleursChronosSolo()
CREATE NONCLUSTERED INDEX IX_ParticipationCourse_Chrono ON Courses.ParticipationCourse(Chrono)
GO



