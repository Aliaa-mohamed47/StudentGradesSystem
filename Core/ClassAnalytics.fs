namespace StudentGradesSystem
open StudentGradesSystem.Domain

module ClassAnalytics =

    /// كل الدرجات لكل الطلاب في قائمة واحدة.
    let private allGrades (students: Student list) : float list =
        students |> List.collect (fun s -> s.Grades)

    /// متوسط الدفعة = متوسط (متوسطات الطلاب). لو مفيش طلاب يرجع 0.
    let classAverage (students: Student list) : float =
        match students with
        | [] -> 0.0
        | _ ->
            students
            |> StudentAnalytics.buildAllReports
            |> List.averageBy (fun r -> r.Average)

    /// عدد الطلاب الناجحين.
    let passedCount (students: Student list) : int =
        students
        |> StudentAnalytics.buildAllReports
        |> List.filter (fun r -> r.Status = Passed)
        |> List.length

    /// عدد الطلاب الراسبين.
    let failedCount (students: Student list) : int =
        students
        |> StudentAnalytics.buildAllReports
        |> List.filter (fun r -> r.Status = Failed)
        |> List.length

    /// نسبة النجاح (%) — 0 لو الفصل فاضي.
    let passRate (students: Student list) : float =
        match List.length students with
        | 0 -> 0.0
        | total -> float (passedCount students) / float total * 100.0

    /// نسبة الرسوب (%) — 0 لو الفصل فاضي.
    let failRate (students: Student list) : float =
        match List.length students with
        | 0 -> 0.0
        | total -> float (failedCount students) / float total * 100.0

    /// أعلى درجة في الفصل كله (None لو مفيش درجات).
    let highestGrade (students: Student list) : float option =
        match allGrades students with
        | [] -> None
        | gs -> Some (List.max gs)

    /// أقل درجة في الفصل كله (None لو مفيش درجات).
    let lowestGrade (students: Student list) : float option =
        match allGrades students with
        | [] -> None
        | gs -> Some (List.min gs)

    /// الطالب صاحب أعلى متوسط (None لو الفصل فاضي).
    let topStudent (students: Student list) : StudentReport option =
        match StudentAnalytics.buildAllReports students with
        | [] -> None
        | rs -> Some (rs |> List.maxBy (fun r -> r.Average))

    /// الطالب صاحب أقل متوسط (None لو الفصل فاضي).
    let bottomStudent (students: Student list) : StudentReport option =
        match StudentAnalytics.buildAllReports students with
        | [] -> None
        | rs -> Some (rs |> List.minBy (fun r -> r.Average))

    /// بيجمع كل الإحصائيات في Record واحد (ClassStatistics من Domain).
    let computeStatistics (students: Student list) : ClassStatistics =
        { TotalStudents = List.length students
          ClassAverage = classAverage students
          PassRate = passRate students
          FailRate = failRate students
          HighestGrade = highestGrade students |> Option.defaultValue 0.0
          LowestGrade = lowestGrade students |> Option.defaultValue 0.0 }

    /// نص التقرير الجاهز للعرض في الـ ViewerForm.
    let formatStatistics (stats: ClassStatistics) : string =
        [ "=============================="
          "      Class Statistics"
          "=============================="
          sprintf "Total Students : %d" stats.TotalStudents
          sprintf "Class Average  : %.2f" stats.ClassAverage
          sprintf "Pass Rate      : %.1f%%" stats.PassRate
          sprintf "Fail Rate      : %.1f%%" stats.FailRate
          sprintf "Highest Grade  : %.2f" stats.HighestGrade
          sprintf "Lowest Grade   : %.2f" stats.LowestGrade
          "==============================" ]
        |> String.concat "\n"

    /// اختصار: من قائمة الطلاب لنص التقرير مباشرة.
    let classReportText (students: Student list) : string =
        students |> computeStatistics |> formatStatistics
