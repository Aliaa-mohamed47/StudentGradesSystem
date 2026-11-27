module StudentGradesSystem.StudentAnalytics

open StudentGradesSystem.Domain
let passThreshold = 50.0
let calculateAverage (grades: float list) : float =
    match grades with
    | [] -> 0.0
    | _ -> List.average grades

let determineStatus (average: float) : StudentStatus =
    if average >= passThreshold then Passed else Failed

let statusToString (status: StudentStatus) : string =
    match status with
    | Passed -> " (Passed)"
    | Failed -> " (Failed)"


let highestGrade (grades: float list) : float option =
    match grades with
    | [] -> None
    | _ -> Some(List.max grades)

let lowestGrade (grades: float list) : float option =
    match grades with
    | [] -> None
    | _ -> Some(List.min grades)

let buildReport (student: Student) : StudentReport =
    let avg = calculateAverage student.Grades

    { Student = student
      Average = avg
      Status = determineStatus avg }

let buildAllReports (students: Student list) : StudentReport list =
    students |> List.map buildReport

let formatReport (report: StudentReport) : string =
    let s = report.Student

    let optToString (v: float option) =
        match v with
        | Some x -> sprintf "%.2f" x
        | None -> "-"

    let gradesText =
        match s.Grades with
        | [] -> "There are no grades."
        | gs -> gs |> List.map (sprintf "%.1f") |> String.concat ", "

    [ "=============================="
      "      Student Report"
      "=============================="
      sprintf "ID      : %d" s.Id
      sprintf "Name    : %s" s.Name
      sprintf "Grades  : %s" gradesText
      sprintf "Count   : %d" (List.length s.Grades)
      sprintf "Average : %.2f" report.Average
      sprintf "Highest : %s" (optToString (highestGrade s.Grades))
      sprintf "Lowest  : %s" (optToString (lowestGrade s.Grades))
      sprintf "Status  : %s" (statusToString report.Status)
      "==============================" ]
    |> String.concat "\n"

let studentReportText (student: Student) : string =
    student |> buildReport |> formatReport
