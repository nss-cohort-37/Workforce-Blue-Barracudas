SELECT tp.[Name]
FROM TrainingProgram tp
LEFT JOIN EmployeeTraining et 
ON et.TrainingProgramId = tp.Id
LEFT JOIN Employee e 
ON et.EmployeeId = e.Id
WHERE et.EmployeeId != 1 
AND StartDate >=GETDATE()
OR et.EmployeeId IS NUll	
AND StartDate >=GETDATE()
GROUP BY tp.[Name]

