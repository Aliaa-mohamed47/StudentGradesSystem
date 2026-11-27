namespace StudentGradesSystem

open System
open System.Windows.Forms

module Program =
    [<EntryPoint; STAThread>]
    let main argv =
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(false)
        MessageBox.Show("Student Grades System - Ready!") |> ignore
        0