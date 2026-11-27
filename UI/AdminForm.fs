namespace StudentGradesSystem

open System
open System.Drawing
open System.Globalization
open System.Windows.Forms
open StudentGradesSystem.Domain


module AdminTempData =

    let mutable private students : Student list =
        [ { Id = 1; Name = "Ahmed Ali"; Grades = [ 85.0; 90.0; 78.0 ] }
          { Id = 2; Name = "Sara Mohamed"; Grades = [ 92.0; 88.0; 95.0 ] }
          { Id = 3; Name = "Omar Hassan"; Grades = [ 45.0; 52.0; 40.0 ] } ]

    let private exists id = students |> List.exists (fun s -> s.Id = id)

    let getAll () : Student list = students |> List.sortBy (fun s -> s.Id)

    let add (student: Student) : Result<unit, string> =
        if exists student.Id then
            Error "A student with this ID already exists."
        else
            students <- student :: students
            Ok()

    let update (student: Student) : Result<unit, string> =
        if not (exists student.Id) then
            Error "Student not found."
        else
            students <- students |> List.map (fun s -> if s.Id = student.Id then student else s)
            Ok()

    let remove (id: int) : Result<unit, string> =
        if not (exists id) then
            Error "Student not found."
        else
            students <- students |> List.filter (fun s -> s.Id <> id)
            Ok()

type AdminForm(currentUser: User) as this =
    inherit Form()

    let mutable isLoading = false
    let mutable loggedOut = false

    let grid =
        new DataGridView(Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false,
                         AllowUserToDeleteRows = false, MultiSelect = false, RowHeadersVisible = false,
                         SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                         AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                         BackgroundColor = Color.White)

    let searchBox = new TextBox(Width = 250, PlaceholderText = "Search by name or ID...")
    let logoutButton = new Button(Text = "Logout", AutoSize = true)

    let idBox = new TextBox(Dock = DockStyle.Top)
    let nameBox = new TextBox(Dock = DockStyle.Top)
    let gradesBox = new TextBox(Dock = DockStyle.Top, PlaceholderText = "e.g. 80, 92.5, 74")

    let makeButton (text: string) (color: Color) =
        new Button(Text = text, Dock = DockStyle.Top, Height = 36, FlatStyle = FlatStyle.Flat,
                   BackColor = color, ForeColor = Color.White)

    let addButton = makeButton "Add" Color.SeaGreen
    let updateButton = makeButton "Update" Color.SteelBlue
    let deleteButton = makeButton "Delete" Color.Firebrick
    let clearButton = makeButton "Clear" Color.Gray

  
    let showError (message: string) =
        MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning) |> ignore

    let formatGrades (grades: float list) =
        grades
        |> List.map (fun g -> g.ToString(CultureInfo.InvariantCulture))
        |> String.concat ", "

    let parseGrade (text: string) =
        match Double.TryParse(text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture) with
        | true, g when g >= 0.0 && g <= 100.0 -> Some g
        | _ -> None

  
    let readInput () : Result<Student, string> =
        match Int32.TryParse(idBox.Text.Trim()) with
        | true, id when id > 0 ->
            if String.IsNullOrWhiteSpace nameBox.Text then
                Error "Name is required."
            else
                let parsed = gradesBox.Text.Split(',') |> Array.toList |> List.map parseGrade
                if parsed |> List.forall Option.isSome then
                    Ok { Id = id; Name = nameBox.Text.Trim(); Grades = parsed |> List.choose (fun g -> g) }
                else
                    Error "Grades must be numbers from 0 to 100, separated by commas."
        | _ -> Error "ID must be a positive whole number."

    let clearInputs () =
        grid.ClearSelection()
        idBox.Clear()
        nameBox.Clear()
        gradesBox.Clear()
        idBox.ReadOnly <- false
        updateButton.Enabled <- false
        deleteButton.Enabled <- false

    let refreshGrid () =
        isLoading <- true
        grid.Rows.Clear()
        let query = searchBox.Text.Trim()
        AdminTempData.getAll ()
        |> List.filter (fun s ->
            s.Name.Contains(query, StringComparison.OrdinalIgnoreCase) || (string s.Id).Contains(query))
        |> List.iter (fun s ->
         
            let average = if s.Grades.IsEmpty then "-" else (List.average s.Grades).ToString("F2")
            grid.Rows.Add([| box s.Id; box s.Name; box (formatGrades s.Grades); box average |]) |> ignore)
        isLoading <- false
        clearInputs ()

    let fillFromSelection () =
        if not isLoading && grid.SelectedRows.Count > 0 then
            let id = grid.SelectedRows.[0].Cells.["Id"].Value :?> int
            AdminTempData.getAll ()
            |> List.tryFind (fun s -> s.Id = id)
            |> Option.iter (fun s ->
                idBox.Text <- string s.Id
                nameBox.Text <- s.Name
                gradesBox.Text <- formatGrades s.Grades
                idBox.ReadOnly <- true  
                updateButton.Enabled <- true
                deleteButton.Enabled <- true)

    let apply (result: Result<unit, string>) =
        match result with
        | Ok() -> refreshGrid ()
        | Error message -> showError message

    let save (operation: Student -> Result<unit, string>) =
        match readInput () with
        | Ok student -> apply (operation student)
        | Error message -> showError message

    let deleteSelected () =
        match Int32.TryParse idBox.Text with
        | true, id ->
            let answer =
                MessageBox.Show($"Delete student '{nameBox.Text}'?", "Confirm",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                                MessageBoxDefaultButton.Button2)
            if answer = DialogResult.Yes then apply (AdminTempData.remove id)
        | _ -> showError "Select a student first."

   
    do
        this.SuspendLayout()
        this.AutoScaleDimensions <- SizeF(96.0f, 96.0f)
        this.AutoScaleMode <- AutoScaleMode.Dpi
        this.Text <- $"Admin Dashboard - {currentUser.Username}"
        this.Font <- new Font("Segoe UI", 10.0f)
        this.ClientSize <- Size(950, 560)
        this.MinimumSize <- Size(800, 520)
        this.StartPosition <- FormStartPosition.CenterScreen

        for name, header, weight in
            [ "Id", "ID", 12.0f; "Name", "Name", 35.0f; "Grades", "Grades", 38.0f; "Average", "Average", 15.0f ] do
            let index = grid.Columns.Add(name, header)
            grid.Columns.[index].FillWeight <- weight

        let topBar = new Panel(Dock = DockStyle.Top, Height = 50, Padding = Padding(10))
        searchBox.Dock <- DockStyle.Left
        logoutButton.Dock <- DockStyle.Right
        topBar.Controls.AddRange([| searchBox :> Control; logoutButton |])

        let side =
            new TableLayoutPanel(Dock = DockStyle.Right, Width = 280, ColumnCount = 1,
                                 Padding = Padding(12), AutoScroll = true)
        side.ColumnStyles.Add(ColumnStyle(SizeType.Percent, 100.0f)) |> ignore

        let makeLabel (text: string) = new Label(Text = text, AutoSize = true, Margin = Padding(3, 8, 3, 2))

        [ makeLabel "Student ID" :> Control; idBox
          makeLabel "Name"; nameBox
          makeLabel "Grades (comma separated)"; gradesBox
          addButton; updateButton; deleteButton; clearButton ]
        |> List.iter (fun control -> side.Controls.Add control)

       
        this.Controls.AddRange([| grid :> Control; side; topBar |])

     
        addButton.Click.Add(fun _ ->
            if idBox.ReadOnly then showError "Press Clear before adding a new student."
            else save AdminTempData.add)
        updateButton.Click.Add(fun _ -> save AdminTempData.update)
        deleteButton.Click.Add(fun _ -> deleteSelected ())
        clearButton.Click.Add(fun _ -> clearInputs ())
        searchBox.TextChanged.Add(fun _ -> refreshGrid ())
        grid.SelectionChanged.Add(fun _ -> fillFromSelection ())
        logoutButton.Click.Add(fun _ ->
            loggedOut <- true
            this.Close())
      
        let canEdit = Auth.canEdit currentUser
        addButton.Visible <- canEdit
        updateButton.Visible <- canEdit
        deleteButton.Visible <- canEdit

        this.Load.Add(fun _ -> refreshGrid ())
        this.Shown.Add(fun _ -> clearInputs ())
        this.ResumeLayout(false)

  
    member _.LoggedOut = loggedOut