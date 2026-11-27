namespace StudentGradesSystem

module Domain =

    type UserRole =
        | Admin
        | Viewer

    type User = {
        Username: string
        Password: string
        Role: UserRole
    }

    type Student = {
        Id: int
        Name: string
        Grades: float list
    }

    type StudentStatus =
        | Passed
        | Failed

    type StudentReport = {
        Student: Student
        Average: float
        Status: StudentStatus
    }

    type ClassStatistics = {
        TotalStudents: int
        ClassAverage: float
        PassRate: float
        FailRate: float
        HighestGrade: float
        LowestGrade: float
    }