USE TP2_SussyKart
GO

-- •○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•
--			  Création d'une vue
-- •○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•

CREATE VIEW Courses.VW_ToutesParticipations AS

SELECT 
    P.ParticipationCourseID, 
    P.Position, 
    P.Chrono, 
    P.NbJoueurs, 
    P.DateParticipation, 
    C.Nom, 
    U.Pseudo AS UtilisateurPseudo
FROM 
    Courses.ParticipationCourse P 
    INNER JOIN Courses.Course C ON P.CourseID = C.CourseID
    INNER JOIN Utilisateurs.Utilisateur U ON P.UtilisateurID = U.UtilisateurID