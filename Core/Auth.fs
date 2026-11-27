namespace StudentGradesSystem
open System
open System.Text
open System.Security.Cryptography
open StudentGradesSystem.Domain

module Auth =

    /// SHA-256 hash (hex) so passwords are not stored in plain text.
    let hashPassword (password: string) : string =
        use sha = SHA256.Create()
        password
        |> Encoding.UTF8.GetBytes
        |> sha.ComputeHash
        |> Convert.ToHexString

    let private makeUser username password role : User =
        { Username = username
          Password = hashPassword password
          Role = role }

    /// Built-in accounts (immutable list).
    let users : User list =
        [ makeUser "admin" "admin123" Admin
          makeUser "viewer" "viewer123" Viewer ]

    /// Returns Some user when username + password match, otherwise None.
    let authenticate (username: string) (password: string) : User option =
        let name = if isNull username then "" else username.Trim()
        let hashed = hashPassword (if isNull password then "" else password)
        users
        |> List.tryFind (fun u ->
            u.Username.Equals(name, StringComparison.OrdinalIgnoreCase)
            && u.Password = hashed)

    /// Same as authenticate but returns a message the UI can show on failure.
    let login (username: string) (password: string) : Result<User, string> =
        if String.IsNullOrWhiteSpace username || String.IsNullOrEmpty password then
            Error "Please enter both username and password."
        else
            match authenticate username password with
            | Some user -> Ok user
            | None -> Error "Invalid username or password."

    // ---- Role checks ----

    let isAdmin (user: User) : bool =
        match user.Role with
        | Admin -> true
        | Viewer -> false

    /// Admin: add / edit / remove students and edit grades.
    let canEdit (user: User) : bool = isAdmin user

    /// Both roles can view grades and reports.
    let canView (_user: User) : bool = true

    let roleName (role: UserRole) : string =
        match role with
        | Admin -> "Admin"
        | Viewer -> "Viewer"