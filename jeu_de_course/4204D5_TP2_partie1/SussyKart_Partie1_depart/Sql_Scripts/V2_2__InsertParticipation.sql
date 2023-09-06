USE TP2_SussyKart
GO

-- •○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•
--			  Création d'une procédure
-- •○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•○•

CREATE PROCEDURE Courses.USP_ParticipationCourses
	@Position int,
	@Chrono int,
	@NbJoueurs int,
	@NomCourse nvarchar(50),
	@UtilisateurId int
AS
BEGIN
	
	DECLARE @CourseID int

	SELECT @CourseID = CourseID FROM Courses.Course WHERE Nom = @NomCourse

    INSERT INTO Courses.ParticipationCourse (Position, Chrono, NbJoueurs, DateParticipation, CourseId, UtilisateurId)
    VALUES (@Position, @Chrono, @NbJoueurs, GETDATE(), @CourseId, @UtilisateurId)
END