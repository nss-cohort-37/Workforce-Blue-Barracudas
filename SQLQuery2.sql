                               SELECT tp.[Name], tp.Id
                               FROM TrainingProgram tp
                               WHERE Id IN (SELECT TrainingProgramId FROM EmployeeTraining WHERE EmployeeTraining.EmployeeId = 1)
                               AND tp.StartDate >= GETDATE()

                                SELECT tp.[Name], tp.Id
                               FROM TrainingProgram tp
                               WHERE Id IN (SELECT TrainingProgramId FROM EmployeeTraining WHERE EmployeeTraining.EmployeeId = 1)
                               AND tp.StartDate >= GETDATE()
                                        