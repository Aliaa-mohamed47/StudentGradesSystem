namespace StudentGradesSystem

open System
open System.IO
open System.Text.Json
open StudentGradesSystem.Domain

module StudentStorage =

    let deleteStudent (studentId: int) (students: Student list) : Result<Student list, string> =
        let exists = students |> List.exists (fun s -> s.Id = studentId)
        match exists with
        | false -> Error $"Student with ID {studentId} was not found."
        | true ->
            let updatedStudents = students |> List.filter (fun s -> s.Id <> studentId)
            Ok updatedStudents

    let saveStudents (filePath: string) (students: Student list) : Result<unit, string> =
        try
            let options = JsonSerializerOptions(WriteIndented = true)
            let json = JsonSerializer.Serialize(students, options)
            File.WriteAllText(filePath, json)
            Ok ()
        with
        | ex -> Error $"Failed to save students data: {ex.Message}"

    let loadStudents (filePath: string) : Result<Student list, string> =
        try
            if not (File.Exists(filePath)) then
                Ok []
            else
                let json = File.ReadAllText(filePath)
                if String.IsNullOrWhiteSpace(json) then
                    Ok []
                else
                    let students = JsonSerializer.Deserialize<Student list>(json)
                    Ok students
        with
        | ex -> Error $"Failed to load students data: {ex.Message}"