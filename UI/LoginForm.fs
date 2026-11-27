namespace StudentGradesSystem

open System
open System.Drawing
open System.Windows.Forms
open StudentGradesSystem.Domain

type LoginForm() as this =
    inherit Form()

    let title =
        new Label(Text = "Student Grades System", Font = new Font("Segoe UI", 15.0f, FontStyle.Bold),
                  Bounds = Rectangle(20, 20, 340, 35), TextAlign = ContentAlignment.MiddleCenter)
    let userLabel = new Label(Text = "Username", Bounds = Rectangle(50, 75, 280, 22))
    let userBox = new TextBox(Bounds = Rectangle(50, 98, 280, 27))
    let passLabel = new Label(Text = "Password", Bounds = Rectangle(50, 135, 280, 22))
    let passBox = new TextBox(Bounds = Rectangle(50, 158, 280, 27), UseSystemPasswordChar = true)
    let showPass = new CheckBox(Text = "Show password", Bounds = Rectangle(50, 192, 180, 24))
    let errorLabel =
        new Label(ForeColor = Color.Firebrick, Bounds = Rectangle(50, 220, 280, 22),
                  TextAlign = ContentAlignment.MiddleCenter)
    let loginButton =
        new Button(Text = "Login", Bounds = Rectangle(50, 248, 280, 36), FlatStyle = FlatStyle.Flat,
                   BackColor = Color.SteelBlue, ForeColor = Color.White)

    let openScreen (user: User) =
        match user.Role with
        | Admin ->
            this.Hide()
            use admin = new AdminForm(user)
            admin.ShowDialog() |> ignore
            if admin.LoggedOut then
                userBox.Clear()
                this.Show()
                userBox.Focus() |> ignore
            else
                this.Close()   
        | Viewer ->
           
            MessageBox.Show("Viewer screen is not implemented yet.") |> ignore

    let tryLogin () =
        match Auth.login userBox.Text passBox.Text with
        | Ok user ->
            errorLabel.Text <- ""
            passBox.Clear()
            showPass.Checked <- false
            openScreen user
        | Error message ->
            errorLabel.Text <- message
            passBox.Clear()
            passBox.Focus() |> ignore

    do
      
        this.SuspendLayout()
        this.AutoScaleDimensions <- SizeF(96.0f, 96.0f)
        this.AutoScaleMode <- AutoScaleMode.Dpi
        this.Text <- "Login"
        this.Font <- new Font("Segoe UI", 10.0f)
        this.ClientSize <- Size(380, 310)
        this.FormBorderStyle <- FormBorderStyle.FixedDialog
        this.MaximizeBox <- false
        this.StartPosition <- FormStartPosition.CenterScreen
        this.Controls.AddRange(
            [| title :> Control; userLabel; userBox; passLabel; passBox; showPass; errorLabel; loginButton |])
        this.AcceptButton <- loginButton   

        showPass.CheckedChanged.Add(fun _ -> passBox.UseSystemPasswordChar <- not showPass.Checked)
        loginButton.Click.Add(fun _ -> tryLogin ())
        this.ResumeLayout(false)