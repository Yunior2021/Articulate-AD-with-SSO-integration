Imports System.Data.SqlClient

Public Class SQL
    Shared Function GetConnectionString() As String
        ' To avoid storing the connection string in your code,  
        ' you can retrieve it from a configuration file.
        Return "Data Source=ServerName\SQLEXPRESS;Database=Articulate;User Id='';Password='';MultipleActiveResultSets=true"
    End Function

    Shared Sub CreateCommand(ByVal queryString As String, ByVal connectionString As String)
        Using connection As New SqlConnection(connectionString)
            Dim command As New SqlCommand(queryString, connection)
            command.Connection.Open()
            command.ExecuteNonQuery()
        End Using
    End Sub
End Class
