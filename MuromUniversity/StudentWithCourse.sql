select
	s.FirstMidName,
	s.LastName,
	s.EnrollmentDate,
	c.Title,
	e.Grade
from dbo.Student s
	inner join dbo.Enrollment e on e.StudentID = s.StudentID
	inner join dbo.Course c on c.CourseID = e.CourseID