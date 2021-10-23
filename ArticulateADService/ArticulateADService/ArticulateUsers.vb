Imports System.DirectoryServices
Imports System.Text
Imports System.Data.SqlClient
Imports ArticulateADService.Articulate
Imports ArticulateADService.SQL
Imports System.DirectoryServices.AccountManagement
Public Class ArticulateUsers
    Public Shared Sub ListADGroupMembers()

        Const ADS_UF_ACCOUNTDISABLE As Integer = &H2

        'Dim ArticulateGroups As String

        'Dim Group_List As New ArrayList()

        Dim c As Articulate.Credentials = New Articulate.Credentials
        c.EmailAddress = ""
        c.Password = ""
        c.CustomerID = ""

        Dim GroupSearcher As New DirectorySearcher
        '!!!Change the OU path, domain and domain admin details!!!
        Dim GroupSearchRoot As New DirectoryEntry(“LDAP://OU=Contoso,DC=com“, “SQLuser”, “Pass”)

        With GroupSearcher
            .SearchRoot = GroupSearchRoot
            .Filter = “(&(ObjectClass=Group)(CN=Articulate))”  '<<< Change the Group name to search for here!!
        End With

        Dim Members As Object = GroupSearcher.FindOne.GetDirectoryEntry.Invoke(“Members”, Nothing) '<<< Get members
        For Each Member As Object In CType(Members, IEnumerable)  '<<< loop through members
            Dim CurrentMember As New DirectoryEntry(Member) '<<< Get user

            Try

                Dim Flags As Integer = CInt(CurrentMember.Properties("userAccountControl").Value)

                Dim cmd As SqlCommand
                Dim sql_result As SqlDataReader



                Using connection As SqlConnection = New SqlConnection(GetConnectionString)
                    connection.Open()

                    Dim rquery As String = "SELECT Username FROM Users WHERE Username='" & CurrentMember.Properties("SamAccountName").Value & "'"
                    cmd = New SqlCommand(rquery, connection)

                    sql_result = cmd.ExecuteReader()

                    If sql_result.HasRows Then
                        'Const ADS_UF_ACCOUNTDISABLE As Integer = &H2

                        'Dim Flags As Integer = CInt(CurrentMember.Properties("userAccountControl").Value)
                        If CBool(Flags And ADS_UF_ACCOUNTDISABLE) Then
                            'Disabled

                            Dim dquery As String = "DELETE FROM Users WHERE Username='" & CurrentMember.Properties("SamAccountName").Value & "'"
                            cmd = New SqlCommand(dquery, connection)
                            cmd.ExecuteNonQuery()
                            '
                            'Delete User from Articulate
                            Dim ao As Articulate.ArticulateOnline = New Articulate.ArticulateOnline
                            ao.Url = "https://articulate-online.com/services/api/1.0/articulateonline.asmx"

                            Dim request As ListUsersRequest = New ListUsersRequest()
                            request.Credentials = c
                            Dim response As ListUsersResponse = ao.ListUsers(request)

                            If response.Success Then

                                For Each userprofile In response.Profiles
                                    If userprofile.EmailAddress = CurrentMember.Properties.Item("mail").Value Then
                                        '
                                        Dim deluser As Articulate.DeleteUserRequest = New Articulate.DeleteUserRequest
                                        deluser.Credentials = c
                                        deluser.UserID = userprofile.UserID
                                        ao.DeleteUser(deluser)

                                    End If
                                Next

                            End If

                        Else

                        End If

                    ElseIf CBool(Flags And ADS_UF_ACCOUNTDISABLE) Then

                        ' Check to avoid creation of a disabled user not on db just AD.

                    Else

                        'Generate a password
                        '
                        Dim s As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789*@$"
                        Dim r As New Random
                        Dim sb As New StringBuilder
                        For i As Integer = 1 To 10
                            Dim idx As Integer = r.Next(0, 35)
                            sb.Append(s.Substring(idx, 1))
                        Next
                        '
                        'Insert new user in SQL - Create Articulate account

                        Dim iquery As String = "INSERT INTO Users (Name,Username,Email,Password,CustomerID) VALUES ('" & CurrentMember.Name.Remove(0, 3) & "','" & CurrentMember.Properties("SamAccountName").Value & "','" & CurrentMember.Properties.Item("mail").Value & "','" & sb.ToString & "','Customer ID #')"
                        cmd = New SqlCommand(iquery, connection)
                        cmd.ExecuteNonQuery()

                        '
                        'Create User in Articulate
                        Try



                            'Dim arrayquery As String = "SELECT AD_Group_Name FROM Groups"
                            'cmd = New SqlCommand(arrayquery, connection)
                            'Dim arrayresults As SqlDataReader
                            'arrayresults = cmd.ExecuteReader()

                            'If arrayresults.HasRows Then
                            'Do While (arrayresults.Read())
                            'Group_List.Add(arrayresults("AD_Group_Name"))
                            'Loop
                            'End If

                            'arrayresults.Close()



                            'For Each cur As String In Group_List
                            'If IsInGroup(cur) = True Then
                            'Dim getgroup As String = "SELECT Group_GUID FROM Groups WHERE AD_Group_Name='" & cur.ToString & "'"
                            'cmd = New SqlCommand(getgroup, connection)
                            'Dim queryr As String = cmd.ExecuteNonQuery()
                            'ArticulateGroups = queryr.ToString
                            'Else

                            'End If
                            'Next

                            Dim ao As Articulate.ArticulateOnline = New Articulate.ArticulateOnline
                            ao.Url = "https://articulate-online.com/services/api/1.0/articulateonline.asmx"
                            Dim request As Articulate.CreateUsersRequest = New Articulate.CreateUsersRequest
                            request.Credentials = c
                            Dim String1() As String = New String() {CurrentMember.Properties.Item("mail").Value}
                            request.Emails = String1
                            Dim String2() As String = New String() {"3954e456-47fd-4c9d-81e5-18b08b39fc48"} 'Default Group
                            request.MemberOfGroupIDs = String2
                            request.AutoGeneratePassword = False
                            request.Password = sb.ToString
                            request.PersonalComment = "Welcome to Training Portal"
                            request.SendLoginEmail = False
                            ao.PreAuthenticate = True
                            'ao.CreateUsers(request)
                            Dim response As CreateUsersResponse = ao.CreateUsers(request)
                            If (response.Success) Then
                                'Console.WriteLine(response.Success)

                                ' Get GUID for new user and add to SQL db.
                                Dim guid_request As ListUsersRequest = New ListUsersRequest()
                                guid_request.Credentials = c
                                Dim guid_response As ListUsersResponse = ao.ListUsers(guid_request)
                                If response.Success Then

                                    For Each userprofile In guid_response.Profiles
                                        If userprofile.EmailAddress = CurrentMember.Properties.Item("mail").Value Then
                                            Dim gid_query As String = "UPDATE Users SET Articulate_GUID='" & userprofile.UserID & "' WHERE Username='" & CurrentMember.Properties("SamAccountName").Value & "'"
                                            cmd = New SqlCommand(gid_query, connection)
                                            cmd.ExecuteNonQuery()
                                        End If
                                    Next
                                Else
                                    'Console.WriteLine(response.ErrorMessage)
                                End If
                            End If
                        Catch ex As Exception
                        End Try
                    End If
                    'Group_List.Clear()
                    sql_result.Close()
                    connection.Close()
                End Using
            Catch ex As Exception
                Dim logs As IO.StreamWriter = New IO.StreamWriter("C:\log.txt", append:=True)
                logs.WriteLine(ex.ToString)
                logs.Close()
            End Try

        Next
    End Sub
    'Public Shared Function IsInGroup(ByVal GroupName As String) As Boolean
    'Dim MyIdentity As System.Security.Principal.WindowsIdentity = System.Security.Principal.WindowsIdentity.GetCurrent()
    'Dim MyPrincipal As System.Security.Principal.WindowsPrincipal = New System.Security.Principal.WindowsPrincipal(MyIdentity)
    'Return MyPrincipal.IsInRole(GroupName)
    'End Function
    Public Shared Sub Cleaner()
        Dim cmd As SqlCommand
        Dim QRUser As String

        Using connection As SqlConnection = New SqlConnection(GetConnectionString)
            connection.Open()

            Dim dquery As String = "DELETE FROM Users WHERE Articulate_GUID='0'"
            cmd = New SqlCommand(dquery, connection)
            cmd.ExecuteNonQuery()

            connection.Close()
        End Using

        Try
            Dim c As Articulate.Credentials = New Articulate.Credentials
            c.EmailAddress = ""
            c.Password = ""
            c.CustomerID = ""

            Dim cmd1 As SqlCommand
            Dim sql_result As SqlDataReader

            Using connection As SqlConnection = New SqlConnection(GetConnectionString)
                connection.Open()

                Dim rquery As String = "SELECT Username,Email FROM Users"
                cmd1 = New SqlCommand(rquery, connection)

                sql_result = cmd1.ExecuteReader()

                If sql_result.HasRows Then
                    While sql_result.Read()

                        Dim inGroup As Boolean = Check_If_Member_Of_AD_Group(sql_result("Username"), "Articulate")
                        If inGroup = False Then

                            QRUser = sql_result("Username")

                            Dim dquery As String = "DELETE FROM Users WHERE Username='" & sql_result("Username") & "'"
                            cmd1 = New SqlCommand(dquery, connection)
                            cmd1.ExecuteNonQuery()
                            '
                            'Delete User from Articulate
                            Dim ao As Articulate.ArticulateOnline = New Articulate.ArticulateOnline
                            ao.Url = "https://articulate-online.com/services/api/1.0/articulateonline.asmx"

                            Dim request As ListUsersRequest = New ListUsersRequest()
                            request.Credentials = c
                            Dim response As ListUsersResponse = ao.ListUsers(request)

                            If response.Success Then

                                For Each userprofile In response.Profiles
                                    If userprofile.EmailAddress = sql_result("Email") Then
                                        '
                                        Dim deluser As Articulate.DeleteUserRequest = New Articulate.DeleteUserRequest
                                        deluser.Credentials = c
                                        deluser.UserID = userprofile.UserID
                                        ao.DeleteUser(deluser)

                                    End If
                                Next
                            End If


                        Else
                            'User is Enabled and/or Member of the Articulate Group.
                        End If
                    End While
                End If
                sql_result.Close()
                connection.Close()
            End Using
        Catch ex As Exception
            Dim logs As IO.StreamWriter = New IO.StreamWriter("C:\log.txt", append:=True)
            logs.WriteLine(ex.ToString)
            logs.WriteLine(QRUser.ToString)
            logs.Close()
        End Try

    End Sub
    Shared Function Check_If_Member_Of_AD_Group(ByVal username As String, ByVal inGroup As String)
        Dim domainct As PrincipalContext = New PrincipalContext(ContextType.Domain, "Contoso", "DC=Contoso,DC=com")
        Dim userprintc As UserPrincipal = UserPrincipal.FindByIdentity(domainct, IdentityType.SamAccountName, username)
        Dim isMember = userprintc.IsMemberOf(domainct, IdentityType.Name, inGroup)
        Return isMember
    End Function

End Class
